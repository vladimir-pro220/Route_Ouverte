using RouteOuverte.Models;
using RouteOuverte.Pages;

namespace RouteOuverte.Services;

/// <summary>État du signalement en cours, partagé entre les écrans 03, 12-14, 15 et 16.</summary>
public static class Brouillon
{
    public static string Route = "Carrefour Deido";
    public static string? Photo;
    public static Signalement? Dernier;

    public static async Task VoirEtatDuJour()
    {
        await Shell.Current.GoToAsync("//Trajet");
        await Shell.Current.GoToAsync($"{nameof(EtatPage)}?depart={Uri.EscapeDataString(Choix.Depart)}&arrivee={Uri.EscapeDataString(Choix.Arrivee)}");
    }
}