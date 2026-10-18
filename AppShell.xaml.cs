using RouteOuverte.Pages;

namespace RouteOuverte;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(EtatPage), typeof(EtatPage));
        Routing.RegisterRoute(nameof(EnvoyePage), typeof(EnvoyePage));
        Routing.RegisterRoute(nameof(EnregistrePage), typeof(EnregistrePage));
        Routing.RegisterRoute(nameof(LieuPage), typeof(LieuPage));
        Routing.RegisterRoute(nameof(CartePage), typeof(CartePage));
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        Routing.RegisterRoute(nameof(PhotoPage), typeof(PhotoPage));
    }
}