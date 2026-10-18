using RouteOuverte.Models;
using RouteOuverte.Services;

namespace RouteOuverte.Pages;

[QueryProperty(nameof(Depart), "depart")]
[QueryProperty(nameof(Arrivee), "arrivee")]
public partial class EtatPage : ContentPage
{
    public string Depart { get; set; } = "";
    public string Arrivee { get; set; } = "";

    public EtatPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = $"{Depart} → {Arrivee}";
        var troncons = RoadService.EtatDuJour();
        Liste.BindingContext = troncons;

        MajLabel.Text = troncons.Count == 0 ? "Aucun signalement récent" : RoadService.Ecoule(troncons.Max(t => t.DerniereMaj));
        var n = troncons.Count(t => t.Statut <= StatutRoute.Inondee);
        Bandeau.IsVisible = n > 0;
        BandeauLabel.Text = n == 1 ? "1 tronçon inondé sur votre trajet" : $"{n} tronçons inondés sur votre trajet";
    }

    async void OnSignaler(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//Signaler");
}