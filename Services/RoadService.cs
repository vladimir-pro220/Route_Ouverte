using System.Text.Json;
using RouteOuverte.Models;

namespace RouteOuverte.Services;

/// <summary>
/// Signalements, confirmations, déconfirmations et trajets stockés localement (Preferences).
/// Règle : l'état d'un tronçon est calculé en rejouant les événements des 6 dernières heures.
/// Il change quand 3 signalements successifs donnent le même nouvel état.
/// 3 déconfirmations successives marquent l'état comme « contesté ».
/// </summary>
public static class RoadService
{
    const string CleSignalements = "signalements", CleTrajets = "trajets", CleConf = "confirmations", CleVotes = "votes";
    const int Seuil = 3;
    static readonly TimeSpan Validite = TimeSpan.FromHours(6);

    public static readonly string[] NomsTroncons = { "Boulevard de la Liberté", "Rue Njo-Njo", "Carrefour Deido" };
    public static readonly string[] Zones = { "Toutes", "Akwa", "Deido", "Bonanjo", "Ndokoti" };

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

    // Confirmations de départ (données de démonstration)
    static Dictionary<string, int> Conf()
    {
        var j = Preferences.Get(CleConf, "");
        return string.IsNullOrEmpty(j) ? new() : JsonSerializer.Deserialize<Dictionary<string, int>>(j) ?? new();
    }
    static void SauverConf(Dictionary<string, int> d) => Preferences.Set(CleConf, JsonSerializer.Serialize(d));

    // Votes de cet usager : clé « tronçon|début de l'état » -> "C" ou "D"
    static Dictionary<string, string> Votes()
    {
        var j = Preferences.Get(CleVotes, "");
        return string.IsNullOrEmpty(j) ? new() : JsonSerializer.Deserialize<Dictionary<string, string>>(j) ?? new();
    }
    static void SauverVotes(Dictionary<string, string> d) => Preferences.Set(CleVotes, JsonSerializer.Serialize(d));

    /// <summary>Au démarrage : purge les signalements périmés et charge des données de démonstration si vide.</summary>
    public static void Init()
    {
        if (!Preferences.ContainsKey("demo_v3"))
        {
            Preferences.Remove(CleSignalements);
            Preferences.Remove(CleVotes);
            SauverConf(new() { ["Boulevard de la Liberté"] = 4, ["Rue Njo-Njo"] = 2, ["Carrefour Deido"] = 6 });
            Preferences.Set("demo_v3", true);
        }

        var l = Charger().Where(Recent).ToList();
        if (l.Count == 0)
        {
            var now = DateTime.Now;
            void Add(string t, StatutRoute s, int n, int ageMax)
            {
                for (int i = 0; i < n; i++)
                {
                    int minutes = n == 1 ? 12 : ageMax - (ageMax - 12) * i / (n - 1);
                    l.Add(new() { Troncon = t, Statut = s, Date = now.AddMinutes(-minutes) });
                }
            }
            Add("Boulevard de la Liberté", StatutRoute.Praticable, 5, 60);
            Add("Rue Njo-Njo", StatutRoute.Difficile, 3, 180);
            Add("Carrefour Deido", StatutRoute.Inondee, 7, 240);
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
        var i = l.FindIndex(x => x.Troncon == s.Troncon && x.Statut == s.Statut && x.Date == s.Date && x.Type == s.Type);
        if (i >= 0) { l.RemoveAt(i); Sauver(l); }
    }

    /// <summary>Confirme (true) ou déconfirme (false) l'état actuel d'un tronçon. Un seul vote par état.</summary>
    public static void Voter(string nom, bool confirme)
    {
        var etat = EtatDuJour().FirstOrDefault(x => x.Nom == nom);
        if (etat == null || etat.Vote != null) return;

        var l = Charger().Where(Recent).ToList();
        l.Add(new Signalement
        {
            Troncon = nom,
            Statut = etat.Statut,
            Date = DateTime.Now,
            Type = confirme ? TypeEvenement.Confirmation : TypeEvenement.Deconfirmation
        });
        Sauver(l);

        var v = Votes();
        v[etat.CleVote] = confirme ? "C" : "D";
        SauverVotes(v);
    }

    public static void AnnulerVote(string nom)
    {
        var etat = EtatDuJour().FirstOrDefault(x => x.Nom == nom);
        if (etat?.Vote == null) return;

        var type = etat.Vote == "C" ? TypeEvenement.Confirmation : TypeEvenement.Deconfirmation;
        var l = Charger();
        var i = l.FindLastIndex(x => x.Troncon == nom && x.Type == type);
        if (i >= 0) { l.RemoveAt(i); Sauver(l); }

        var v = Votes();
        v.Remove(etat.CleVote);
        SauverVotes(v);
    }

    /// <summary>Démonstration uniquement : simule n votes d'autres usagers sur l'état actuel.</summary>
    public static void SimulerVotes(string nom, bool confirme, int n)
    {
        var etat = EtatDuJour().FirstOrDefault(x => x.Nom == nom);
        if (etat == null) return;
        var l = Charger().Where(Recent).ToList();
        for (int i = 0; i < n; i++)
            l.Add(new Signalement
            {
                Troncon = nom,
                Statut = etat.Statut,
                Date = DateTime.Now,
                Type = confirme ? TypeEvenement.Confirmation : TypeEvenement.Deconfirmation
            });
        Sauver(l);
    }

    /// <summary>Derniers signalements d'états (hors confirmations et déconfirmations).</summary>
    public static List<Signalement> Derniers(string troncon) =>
        Charger().Where(Recent)
            .Where(x => x.Troncon == troncon && x.Type == TypeEvenement.Signalement)
            .OrderByDescending(x => x.Date).ToList();

    /// <summary>
    /// Rejoue les événements d'un tronçon (du plus ancien au plus récent).
    /// debut : début des signalements de l'état actuel ; depuis : événement qui a fixé cet état.
    /// </summary>
    static (StatutRoute statut, int debut, int depuis, bool conteste)? Analyser(List<Signalement> ev)
    {
        int first = ev.FindIndex(x => x.Type == TypeEvenement.Signalement);
        if (first < 0) return null;

        var etat = ev[first].Statut;
        int debut = first, depuis = first;
        StatutRoute? candidat = null;
        int nCandidat = 0, candDebut = 0, nContre = 0;
        bool conteste = false;

        for (int i = first + 1; i < ev.Count; i++)
        {
            var e = ev[i];
            switch (e.Type)
            {
                case TypeEvenement.Confirmation:
                    candidat = null; nCandidat = 0; nContre = 0; conteste = false;
                    break;

                case TypeEvenement.Deconfirmation:
                    nContre++;
                    if (nContre >= Seuil) conteste = true;
                    break;

                default: // Signalement
                    if (e.Statut == etat)
                    {
                        candidat = null; nCandidat = 0; nContre = 0; conteste = false;
                    }
                    else
                    {
                        if (candidat == e.Statut) nCandidat++;
                        else { candidat = e.Statut; nCandidat = 1; candDebut = i; }

                        if (nCandidat >= Seuil)
                        {
                            etat = e.Statut; debut = candDebut; depuis = i;
                            candidat = null; nCandidat = 0; nContre = 0; conteste = false;
                        }
                    }
                    break;
            }
        }
        return (etat, debut, depuis, conteste);
    }

    public static List<Troncon> EtatDuJour()
    {
        var recents = Charger().Where(Recent).OrderBy(x => x.Date).ToList();
        var conf = Conf();
        var votes = Votes();
        var res = new List<Troncon>();

        foreach (var nom in NomsTroncons)
        {
            var ev = recents.Where(x => x.Troncon == nom).ToList();
            var a = Analyser(ev);
            if (a == null) continue;
            var (statut, debut, depuis, conteste) = a.Value;

            int first = ev.FindIndex(x => x.Type == TypeEvenement.Signalement);
            bool change = depuis != first;

            var apres = ev.Skip(depuis + 1).ToList();
            var sigsEtat = ev.Skip(debut).Where(x => x.Type == TypeEvenement.Signalement && x.Statut == statut).ToList();

            var cle = $"{nom}|{ev[depuis].Date.Ticks}";
            var heures = (int)Math.Ceiling((ev[debut].Date + Validite - DateTime.Now).TotalHours);

            res.Add(new Troncon
            {
                Nom = nom,
                Statut = statut,
                Zone = Zone(nom),
                NbSignalements = sigsEtat.Count,
                Confirmations = (change ? 0 : conf.GetValueOrDefault(nom)) + apres.Count(x => x.Type == TypeEvenement.Confirmation),
                Deconfirmations = apres.Count(x => x.Type == TypeEvenement.Deconfirmation),
                Conteste = conteste,
                Vote = votes.GetValueOrDefault(cle),
                CleVote = cle,
                ExpireHeures = Math.Max(1, heures),
                DerniereMaj = ev.Max(x => x.Date),
                Photo = sigsEtat.Where(x => !string.IsNullOrEmpty(x.Photo) && File.Exists(x.Photo))
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

    /// <summary>Démo : le trajet vers l'école n'emprunte que le Boulevard de la Liberté.</summary>
    public static List<Troncon> TronconsDuTrajet(Trajet t)
    {
        var tous = EtatDuJour();
        return t.Arrivee == "École" ? tous.Where(x => x.Nom == "Boulevard de la Liberté").ToList() : tous;
    }

    public static List<Trajet> Trajets()
    {
        var json = Preferences.Get(CleTrajets, "");
        var l = string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<List<Trajet>>(json) ?? new();
        for (int i = 0; i < l.Count; i++)
        {
            var t = l[i];
            var etat = TronconsDuTrajet(t);
            var (statut, texte) = Resume(etat);
            t.Index = i;
            t.Statut = statut;
            t.Resume = texte;
            t.MiseAJour = etat.Count == 0 ? "" : Ecoule(etat.Max(x => x.DerniereMaj));
        }
        return l;
    }

    public static void SauverTrajets(List<Trajet> l) => Preferences.Set(CleTrajets, JsonSerializer.Serialize(l));
}