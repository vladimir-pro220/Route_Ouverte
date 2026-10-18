using RouteOuverte.Pages;

namespace RouteOuverte.Services;

/// <summary>Lieux choisis (écrans 01 et 05) et données passées entre les écrans du parcours Trajet.</summary>
public static class Choix
{
    public static string Depart = "Akwa";
    public static string Arrivee = "Marché Deido";

    /// <summary>Tronçon confirmé depuis l'écran 09 : l'écran 02 affiche alors la barre « Merci ».</summary>
    public static string? DerniereConfirmation;

    public static string PhotoChemin = "";
    public static string PhotoLegende = "";

    public static async Task VoirPhoto(string chemin, string legende)
    {
        PhotoChemin = chemin;
        PhotoLegende = legende;
        await Shell.Current.GoToAsync(nameof(PhotoPage));
    }
}