using System.Text.Json;
using RouteOuverte.Models;

namespace RouteOuverte.Services;

/// <summary>
/// Signalements, confirmations et trajets stockés localement (Preferences) : l'app fonctionne hors ligne.
/// Règle d'état d'un tronçon : statut le plus fréquent parmi les signalements des 6 dernières heures
/// (en cas d'égalité, le plus mauvais l'emporte).
/// </summary>
public static class RoadService
{
    const string CleSignalements = "signalements", CleTrajets = "trajets", CleConf = "confirmations", CleConfirmes = "confirmes";
    static readonly TimeSpan Validite = TimeSpan.FromHours(6);

    public static readonly string[] NomsTroncons = { "Boulevard de la Liberté", "Rue Njo-Njo", "Carrefour Deido" };
    public static readonly string[] Zones = { "Toutes", "Akwa", "Deido", "Bonanjo", "Ndokoti" };

    // Lieux de démonstration (écran 05)
    public static readonly (string Zone, string[] Lieux)[] Lieux =
    {
        ("Akwa", new[] { "Boulevard de la Liberté", "Rue Njo-Njo" }),
        ("Deido", new[] { "Carrefour Deido", "Marché Deido" }),
        ("Bonanjo", new[] { "Place du Gouvernement" }),
        ("Ndokoti", new[] { "Carrefour Ndokoti" })
    };

    /// <summary>Pour la démonstration : mettre à true pour simuler l'absence de réseau.</summary>
    public static bool ForcerHorsLigne { get; set; } = false;
    public static bool HorsLigne => ForcerHorsLigne || Connectivity.Current.NetworkAccess != NetworkAccess.Internet;

    public static string Zone(string troncon) => troncon == "Carrefour Deido" ? "Deido" : "Akwa";

    static bool Recent(Signalement s) => DateTime.Now - s.Date < Validite;

    static List<Signalement> Charger()
    {
        var json = Preferences.Get(CleSignalements, "");
        return string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<Signalement>>(json) ?? new();
    }
    static void Sauver(List<Signalement> l) => Preferences.Set(CleSignalements, JsonSerializer.Serialize(l));

    static Dictionary<string, int> Conf()
    {
        var j = Preferences.Get(CleConf, "");
        return string.IsNullOrEmpty(j) ? new() : JsonSerializer.Deserialize<Dictionary<string, int>>(j) ?? new();
    }
    static void SauverConf(Dictionary<string, int> d) => Preferences.Set(CleConf, JsonSerializer.Serialize(d));

    static HashSet<string> Confirmes()
    {
        var j = Preferences.Get(CleConfirmes, "");
        return string.IsNullOrEmpty(j) ? new() : (JsonSerializer.Deserialize<List<string>>(j) ?? new()).ToHashSet();
    }
    static void SauverConfirmes(HashSet<string> h) => Preferences.Set(CleConfirmes, JsonSerializer.Serialize(h.ToList()));

    /// <summary>Au démarrage : purge les signalements périmés et charge des données de démonstration si vide.</summary>
    public static void Init()
    {
        // Nouvelle version des données de démonstration : on repart de zéro une fois
        if (!Preferences.ContainsKey("demo_v2"))
        {
            Preferences.Remove(CleSignalements);
            Preferences.Remove(CleConfirmes);
            SauverConf(new() { ["Boulevard de la Liberté"] = 4, ["Rue Njo-Njo"] = 2, ["Carrefour Deido"] = 6 });
            Preferences.Set("demo_v2", true);
        }

        var l = Charger().Where(Recent).ToList();
        if (l.Count == 0)
        {
            var now = DateTime.Now;
            // n signalements répartis entre "ageMax" minutes et 12 minutes
            void Add(string t, StatutRoute s, int n, int ageMax)
            {
                for (int i = 0; i < n; i++)
                {
                    int minutes = n == 1 ? 12 : ageMax - (ageMax - 12) * i / (n - 1);
                    l.Add(new() { Troncon = t, Statut = s, Date = now.AddMinutes(-minutes) });
                }
            }
            Add("Boulevard de la Liberté", StatutRoute.Praticable, 5, 60);   // expire dans 5 h
            Add("Rue Njo-Njo", StatutRoute.Difficile, 3, 180);               // expire dans 3 h
            Add("Carrefour Deido", StatutRoute.Inondee, 7, 240);             // expire dans 2 h
        }
        Sauver(l);

        if (!Preferences.ContainsKey(CleTrajets))
            SauverTrajets(new()
            {
                new() { Nom = "Domicile → Travail", Depart = "Domicile", Arrivee = "Travail" },
                new() { Nom = "Domicile → École", Depart = "Domicile", Arrivee = "École" }
            });
    }

    public static Signalement Ajouter(string troncon, StatutRoute statut, string? photo = null)
    {
        var l = Charger().Where(Recent).ToList();
        var s = new Signalement { Troncon = troncon, Statut = statut, Date = DateTime.Now, Photo = photo };
        l.Add(s);
        Sauver(l);
        return s;
    }

    public static void Retirer(Signalement s)
    {
        var l = Charger();
        var i = l.FindIndex(x => x.Troncon == s.Troncon && x.Statut == s.Statut && x.Date == s.Date);
        if (i >= 0) { l.RemoveAt(i); Sauver(l); }
    }

    public static void Confirmer(string nom)
    {
        var c = Confirmes();
        if (c.Contains(nom)) return;
        var d = Conf();
        d[nom] = d.GetValueOrDefault(nom) + 1;
        SauverConf(d);
        c.Add(nom);
        SauverConfirmes(c);
    }

    public static void AnnulerConfirmation(string nom)
    {
        var c = Confirmes();
        if (!c.Remove(nom)) return;
        var d = Conf();
        d[nom] = Math.Max(0, d.GetValueOrDefault(nom) - 1);
        SauverConf(d);
        SauverConfirmes(c);
    }

    public static List<Signalement> Derniers(string troncon) =>
        Charger().Where(Recent).Where(x => x.Troncon == troncon).OrderByDescending(x => x.Date).ToList();

    public static List<Troncon> EtatDuJour()
    {
        var recents = Charger().Where(Recent).ToList();
        var conf = Conf();
        var confirmes = Confirmes();
        var res = new List<Troncon>();
        foreach (var nom in NomsTroncons)
        {
            var r = recents.Where(x => x.Troncon == nom).ToList();
            if (r.Count == 0) continue;
            var statut = r.GroupBy(x => x.Statut).OrderByDescending(g => g.Count()).ThenBy(g => (int)g.Key).First().Key;
            var heures = (int)Math.Ceiling((r.Min(x => x.Date) + Validite - DateTime.Now).TotalHours);
            res.Add(new Troncon
            {
                Nom = nom,
                Statut = statut,
                Zone = Zone(nom),
                NbSignalements = r.Count,
                Confirmations = conf.GetValueOrDefault(nom),
                Confirme = confirmes.Contains(nom),
                ExpireHeures = Math.Max(1, heures),
                DerniereMaj = r.Max(x => x.Date),
                Photo = r.Where(x => !string.IsNullOrEmpty(x.Photo) && File.Exists(x.Photo))
                         .OrderByDescending(x => x.Date).Select(x => x.Photo).FirstOrDefault()
            });
        }
        return res;
    }

    /// <summary>Phrase de synthèse : « 1 tronçon inondé », « Route dégagée », etc.</summary>
    public static (StatutRoute statut, string texte) Resume(List<Troncon> t)
    {
        if (t.Count == 0) return (StatutRoute.Praticable, "Aucun signalement");
        var pire = t.Min(x => x.Statut);
        var n = t.Count(x => x.Statut <= StatutRoute.Inondee);
        return pire switch
        {
            StatutRoute.Bloquee when n == 1 => (pire, "1 tronçon bloqué"),
            StatutRoute.Inondee when n == 1 => (pire, "1 tronçon inondé"),
            StatutRoute.Bloquee or StatutRoute.Inondee => (pire, $"{n} tronçons inondés ou bloqués"),
            StatutRoute.Difficile => (pire, "Boueuse mais passable"),
            _ => (pire, "Route dégagée")
        };
    }

    public static string Ecoule(DateTime d)
    {
        var m = (int)(DateTime.Now - d).TotalMinutes;
        return m < 1 ? "Mis à jour à l'instant" : m < 60 ? $"Mis à jour il y a {m} min" : $"Mis à jour il y a {m / 60} h";
    }

    public static string Depuis(DateTime d)
    {
        var m = (int)(DateTime.Now - d).TotalMinutes;
        return m < 1 ? "à l'instant" : m < 60 ? $"il y a {m} min" : $"il y a {m / 60} h";
    }

    // NB : en v1.0 tous les trajets utilisent les mêmes tronçons de démonstration.
    public static List<Trajet> Trajets()
    {
        var json = Preferences.Get(CleTrajets, "");
        var l = string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<Trajet>>(json) ?? new();
        var etat = EtatDuJour();
        var (statut, texte) = Resume(etat);
        foreach (var t in l)
        {
            t.Statut = statut; t.Resume = texte;
            t.MiseAJour = etat.Count == 0 ? "" : Ecoule(etat.Max(x => x.DerniereMaj));
        }
        return l;
    }

    public static void SauverTrajets(List<Trajet> l) => Preferences.Set(CleTrajets, JsonSerializer.Serialize(l));
}