namespace RouteOuverte.Models;

/// <summary>Un tronçon avec son statut calculé à partir des signalements récents.</summary>
public class Troncon
{
    public string Nom { get; set; } = "";
    public StatutRoute Statut { get; set; }
    public string Zone { get; set; } = "";
    public int NbSignalements { get; set; }
    public int Confirmations { get; set; }
    public bool Confirme { get; set; }
    public int ExpireHeures { get; set; }
    public DateTime DerniereMaj { get; set; }
    public string? Photo { get; set; }

    public string Libelle => Statut.Libelle();
    public Color Couleur => Statut.Couleur();
    public Color CouleurTexte => Statut.CouleurTexte();

    public string TexteZone => $"Zone : {Zone}";
    public string TexteSignalements => $"{NbSignalements} signalement{(NbSignalements > 1 ? "s" : "")}";
    public string TexteConfirmations => $"{Confirmations} confirmation{(Confirmations > 1 ? "s" : "")}";
    public string TexteCompteurs => $"{TexteSignalements} · {TexteConfirmations}";
    public string TexteExpire => $"Expire dans {ExpireHeures} h";

    public bool APhoto => !string.IsNullOrEmpty(Photo);
    public ImageSource? Miniature => APhoto ? ImageSource.FromFile(Photo!) : null;

    public string TexteConfirmer => Confirme ? "✓ Confirmé" : "👍 Confirmer";
    public Color FondConfirmer => Confirme ? Color.FromArgb("#D6E3FF") : Colors.White;
}