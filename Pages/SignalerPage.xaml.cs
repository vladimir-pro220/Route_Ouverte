using RouteOuverte.Models;

namespace RouteOuverte.Pages;

public partial class SignalerPage : ContentPage
{
    public SignalerPage()
    {
        InitializeComponent();
        RoutePicker.ItemsSource = RoadService.NomsTroncons;
        RoutePicker.SelectedIndex = RoadService.NomsTroncons.Length - 1; // Carrefour Deido, comme sur la maquette
        BindableLayout.SetItemsSource(Bandes, new List<BandeSignalement>
        {
            new BandeSignalement(StatutRoute.Bloquee,    "Route bloquée",    "Impossible de passer",            "■□□□"),
            new BandeSignalement(StatutRoute.Inondee,    "Route inondée",    "Eau haute, passage risqué",       "■■□□"),
            new BandeSignalement(StatutRoute.Difficile,  "Route boueuse",    "Boue épaisse, on passe lentement", "■■■□"),
            new BandeSignalement(StatutRoute.Praticable, "Route praticable", "Circulation normale",             "■■■■")
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        HorsLigne.IsVisible = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
    }

    async void OnBandeTapped(object? sender, TappedEventArgs e)
    {
        if (RoutePicker.SelectedItem is not string route) return;
        var bande = (BandeSignalement)((Border)sender!).BindingContext;
        RoadService.Ajouter(route, bande.Statut);

        Confirmation.Text = $"{bande.Titre} enregistrée pour {route}" + (HorsLigne.IsVisible ? " (envoi différé)." : ".");
        Confirmation.IsVisible = true;
        await Task.Delay(3200);
        Confirmation.IsVisible = false;
    }
}