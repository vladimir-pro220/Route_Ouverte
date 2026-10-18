using System.Text.Json;
using RouteOuverte.Models;

namespace RouteOuverte.Services;

/// <summary>Signalements et trajets stockés localement : l'app fonctionne hors ligne.
/// État d'un tronçon = statut le plus fréquent des signalements des 6 dernières heures
/// (en cas d'égalité, le pire l'emporte).</summary>
public static class RoadService
{
    const string CleSignalements = "signalements", CleTrajets = "trajets";
    static readonly TimeSpan Validite = TimeSpan.FromHours(6);

    public static readonly string[] NomsTroncons = { "Boulevard de la Liberté", "Rue Njo-Njo", "Carrefour Deido" };

    static bool Recent(Signalement s) => DateTime.Now - s.Date < Validite;

    static List<Signalement> Charger()
    {
        var json = Preferences.Get(CleSignalements, "");
        if (string.IsNullOrEmpty(json)) return new List<Signalement>();
        return JsonSerializer.Deserialize<List<Signalement>>(json) ?? new List<Signalement>();
    }

    static void Sauver(List<Signalement> l) => Preferences.Set(CleSignalements, JsonSerializer.Serialize(l));

    public static void Init()
    {
        var l = Charger().Where(Recent).ToList();
        if (l.Count == 0)
        {
            var now = DateTime.Now;
            void Add(string t, StatutRoute s, int n)
            {
                for (int i = 0; i < n; i++)
                    l.Add(new Signalement { Troncon = t, Statut = s, Date = now.AddMinutes(-12 + i) });
            }
            Add("Boulevard de la Liberté", StatutRoute.Praticable, 5);
            Add("Rue Njo-Njo", StatutRoute.Difficile, 3);
            Add("Carrefour Deido", StatutRoute.Inondee, 7);
        }
        Sauver(l);

        if (!Preferences.ContainsKey(CleTrajets))
        {
            SauverTrajets(new List<Trajet>
            {
                new Trajet { Nom = "Domicile → Travail", Depart = "Domicile", Arrivee = "Travail" },
                new Trajet { Nom = "Domicile → École", Depart = "Domicile", Arrivee = "École" }
            });
        }
    }

    /// <summary>Pour la démonstration : mettre à true pour simuler l'absence de réseau.</summary>
    public static bool ForcerHorsLigne { get; set; } = false;
    public static bool HorsLigne => ForcerHorsLigne || Connectivity.Current.NetworkAccess != NetworkAccess.Internet;

    public static string Zone(string troncon) => troncon == "Carrefour Deido" ? "Deido" : "Akwa";

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

    public static List<Troncon> EtatDuJour()
    {
        var recents = Charger().Where(Recent).ToList();
        var res = new List<Troncon>();
        foreach (var nom in NomsTroncons)
        {
            var r = recents.Where(x => x.Troncon == nom).ToList();
            if (r.Count == 0) continue;
            var statut = r.GroupBy(x => x.Statut)
                          .OrderByDescending(g => g.Count()).ThenBy(g => (int)g.Key)
                          .First().Key;
            res.Add(new Troncon { Nom = nom, Statut = statut, NbSignalements = r.Count, DerniereMaj = r.Max(x => x.Date) });
        }
        return res;
    }

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
        if (m < 1) return "Mis à jour à l'instant";
        return m < 60 ? $"Mis à jour il y a {m} min" : $"Mis à jour il y a {m / 60} h";
    }

    public static List<Trajet> Trajets()
    {
        var json = Preferences.Get(CleTrajets, "");
        List<Trajet> l = string.IsNullOrEmpty(json)
            ? new List<Trajet>()
            : JsonSerializer.Deserialize<List<Trajet>>(json) ?? new List<Trajet>();
        var etat = EtatDuJour();
        var (statut, texte) = Resume(etat);
        foreach (var t in l)
        {
            t.Statut = statut;
            t.Resume = texte;
            t.MiseAJour = etat.Count == 0 ? "" : Ecoule(etat.Max(x => x.DerniereMaj));
        }
        return l;
    }

    public static void SauverTrajets(List<Trajet> l) => Preferences.Set(CleTrajets, JsonSerializer.Serialize(l));
}