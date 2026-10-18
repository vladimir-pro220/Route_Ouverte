using RouteOuverte.Pages;

namespace RouteOuverte;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EtatPage), typeof(EtatPage)); // écran 02, sous-écran de l'onglet Trajet
    }
}