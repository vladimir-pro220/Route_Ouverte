using RouteOuverte.Models;
using RouteOuverte.Services;

namespace RouteOuverte.Pages;

public partial class SignalerPage : ContentPage
{
    public SignalerPage()
    {
        InitializeComponent();

        BindableLayout.SetItemsSource(Bandes, new List<BandeSignalement>
        {
            new(StatutRoute.Bloquee,   "Route bloquée",    "Impossible de passer",            "■□□□", "⛔"),
            new(StatutRoute.Inondee,   "Route inondée",    "Eau haute, passage risqué",       "■■□□", "🌊"),
            new(StatutRoute.Difficile, "Route boueuse",    "Boue épaisse, on passe lentement", "■■■□", "⚠️"),
            new(StatutRoute.Praticable,"Route praticable", "Circulation normale",             "■■■■", "✅")
        });

        FeuilleRoute.RouteChoisie += (_, route) => { Brouillon.Route = route; MajRoute(); };
        FeuillePhoto.PhotoChoisie += (_, chemin) => { Brouillon.Photo = chemin; MajPhoto(); };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PastilleHorsLigne.IsVisible = RoadService.HorsLigne;
        MajRoute();
        MajPhoto();
    }

    void MajRoute()
    {
        RouteNom.Text = Brouillon.Route;
        RouteZone.Text = $"Zone : {RoadService.Zone(Brouillon.Route)}";
    }

    void MajPhoto()
    {
        var a = !string.IsNullOrEmpty(Brouillon.Photo) && File.Exists(Brouillon.Photo);
        PhotoVide.IsVisible = !a;
        PhotoAjoutee.IsVisible = a;
        Miniature.Source = a ? ImageSource.FromFile(Brouillon.Photo) : null;
    }

    void OnChoisirRoute(object? sender, TappedEventArgs e) => FeuilleRoute.Ouvrir();
    void OnPhoto(object? sender, EventArgs e) => FeuillePhoto.IsVisible = true;
    void OnSupprimerPhoto(object? sender, EventArgs e) { Brouillon.Photo = null; MajPhoto(); }

    async void OnBandeTapped(object? sender, TappedEventArgs e)
    {
        var bande = (BandeSignalement)((Border)sender!).BindingContext;
        var horsLigne = RoadService.HorsLigne;

        Brouillon.Dernier = RoadService.Ajouter(Brouillon.Route, bande.Statut, Brouillon.Photo);
        Brouillon.Photo = null; // le brouillon est consommé

        await Shell.Current.GoToAsync(horsLigne ? nameof(EnregistrePage) : nameof(EnvoyePage));
    }
}