namespace RouteOuverte.Models;

/// <summary>Une des 4 bandes de l'écran Signaler.</summary>
public class BandeSignalement
{
    public BandeSignalement(StatutRoute statut, string titre, string sousTexte, string jauge, string icone)
    { Statut = statut; Titre = titre; SousTexte = sousTexte; Jauge = jauge; Icone = icone; }

    public StatutRoute Statut { get; }
    public string Titre { get; }
    public string SousTexte { get; }
    public string Jauge { get; }
    public string Icone { get; }
    public Color Couleur => Statut.Couleur();
    public Color CouleurTexte => Statut.CouleurTexte();
}