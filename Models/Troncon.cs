namespace RouteOuverte.Models;

public class Troncon
{
    public string Nom { get; set; } = "";
    public StatutRoute Statut { get; set; }
    public int NbSignalements { get; set; }
    public DateTime DerniereMaj { get; set; }

    public string Libelle => Statut.Libelle();
    public Color Couleur => Statut.Couleur();
    public Color CouleurTexte => Statut.CouleurTexte();
    public string TexteSignalements => NbSignalements <= 1 ? $"{NbSignalements} signalement" : $"{NbSignalements} signalements";
}