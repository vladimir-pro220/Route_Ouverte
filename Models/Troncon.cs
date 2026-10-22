namespace RouteOuverte.Models;

/// <summary>Un tronçon avec son état calculé à partir des signalements, confirmations et déconfirmations récents.</summary>
public class Troncon
{
    public string Nom { get; set; } = "";
    public StatutRoute Statut { get; set; }
    public string Zone { get; set; } = "";
    public int NbSignalements { get; set; }
    public int Confirmations { get; set; }
    public int Deconfirmations { get; set; }
    public bool Conteste { get; set; }
    public int ExpireHeures { get; set; }
    public DateTime DerniereMaj { get; set; }
    public string? Photo { get; set; }

    /// <summary>Vote de cet usager sur l'état actuel : "C" (confirmé), "D" (déconfirmé) ou null.</summary>
    public string? Vote { get; set; }
    public string CleVote { get; set; } = "";

    public bool Confirme => Vote == "C";
    public bool Deconfirme => Vote == "D";

    public string Libelle => Statut.Libelle();
    public Color Couleur => Statut.Couleur();
    public Color CouleurTexte => Statut.CouleurTexte();

    public string TexteZone => $"Zone : {Zone}";
    public string TexteSignalements => $"{NbSignalements} signalement{(NbSignalements > 1 ? "s" : "")}";
    public string TexteConfirmations => $"{Confirmations} confirmation{(Confirmations > 1 ? "s" : "")}";
    public string TexteCompteurs => $"{TexteSignalements} · {TexteConfirmations}"
        + (Deconfirmations > 0 ? $" · {Deconfirmations} déconfirmation{(Deconfirmations > 1 ? "s" : "")}" : "");
    public string TexteExpire => $"Expire dans {ExpireHeures} h";
    public string TexteConteste => "⚠ État contesté : à vérifier";

    public bool APhoto => !string.IsNullOrEmpty(Photo);
    public ImageSource? Miniature => APhoto ? ImageSource.FromFile(Photo!) : null;

    public string TexteConfirmer => Confirme ? "✓ Confirmé" : "👍 Confirmer";
    public Color FondConfirmer => Confirme ? Color.FromArgb("#D6E3FF") : Colors.White;
    public string TexteDeconfirmer => Deconfirme ? "✓ Déconfirmé" : "👎 Déconfirmer";
    public Color FondDeconfirmer => Deconfirme ? Color.FromArgb("#FFDAD6") : Colors.White;
}