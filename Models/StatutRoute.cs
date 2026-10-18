namespace RouteOuverte.Models;

/// <summary>Du pire (1) au meilleur (4).</summary>
public enum StatutRoute { Bloquee = 1, Inondee = 2, Difficile = 3, Praticable = 4 }

public static class StatutRouteExt
{
    public static string Libelle(this StatutRoute s) => s switch
    {
        StatutRoute.Bloquee => "Bloquée",
        StatutRoute.Inondee => "Inondée",
        StatutRoute.Difficile => "Boueuse",
        _ => "Praticable"
    };

    public static Color Couleur(this StatutRoute s) => s switch
    {
        StatutRoute.Bloquee => Color.FromArgb("#8E1B1B"),
        StatutRoute.Inondee => Color.FromArgb("#E53935"),
        StatutRoute.Difficile => Color.FromArgb("#F57C00"),
        _ => Color.FromArgb("#2E7D32")
    };

    public static Color CouleurTexte(this StatutRoute s) =>
        s == StatutRoute.Difficile ? Color.FromArgb("#1A1A1A") : Colors.White;
}