namespace RouteOuverte.Models;

public class BandeSignalement
{
    public BandeSignalement(StatutRoute statut, string titre, string sousTexte, string jauge)
    { Statut = statut; Titre = titre; SousTexte = sousTexte; Jauge = jauge; }

    public StatutRoute Statut { get; }
    public string Titre { get; }
    public string SousTexte { get; }
    public string Jauge { get; }
    public Color Couleur => Statut.Couleur();
    public Color CouleurTexte => Statut.CouleurTexte();
}