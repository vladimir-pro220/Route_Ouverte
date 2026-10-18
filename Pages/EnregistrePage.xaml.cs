using RouteOuverte.Models;
using RouteOuverte.Services;

namespace RouteOuverte.Pages;

public partial class EnregistrePage : ContentPage
{
    public EnregistrePage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var s = Brouillon.Dernier;
        if (s == null) return;

        RouteLabel.Text = s.Troncon;
        EtatLabel.Text = s.Statut.Libelle();
        EtatLabel.TextColor = s.Statut.CouleurTexte();
        EtatBadge.BackgroundColor = s.Statut.Couleur();
        ZoneLabel.Text = RoadService.Zone(s.Troncon);
        PhotoLabel.Text = string.IsNullOrEmpty(s.Photo) ? "Aucune" : "📷 Jointe";
    }

    async void OnVoirEtat(object? sender, EventArgs e) => await Brouillon.VoirEtatDuJour();

    async void OnAutreRoute(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}