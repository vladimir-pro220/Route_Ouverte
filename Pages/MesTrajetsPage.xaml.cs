using RouteOuverte.Models;
using RouteOuverte.Services;

namespace RouteOuverte.Pages;

public partial class MesTrajetsPage : ContentPage
{
    const int MaxTrajets = 3;
    bool _chargement;

    public MesTrajetsPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _chargement = true;
        NotifSwitch.IsToggled = Preferences.Get("notif_active", true);
        HeurePicker.Time = TimeSpan.FromMinutes(Preferences.Get("notif_minutes", 6 * 60 + 30));
        _chargement = false;
        Rafraichir();
    }

    void Rafraichir()
    {
        var l = RoadService.Trajets();
        BindableLayout.SetItemsSource(Liste, l);
        AjouterCarte.IsVisible = l.Count < MaxTrajets;
    }

    async void OnAjouter(object? sender, TappedEventArgs e)
    {
        var l = RoadService.Trajets();
        if (l.Count >= MaxTrajets) return;
        var depart = await DisplayPromptAsync("Nouveau trajet", "Lieu de départ ?", "Suivant", "Annuler", "Domicile");
        if (string.IsNullOrWhiteSpace(depart)) return;
        var arrivee = await DisplayPromptAsync("Nouveau trajet", "Lieu d'arrivée ?", "Enregistrer", "Annuler", "Travail");
        if (string.IsNullOrWhiteSpace(arrivee)) return;
        l.Add(new Trajet { Nom = $"{depart.Trim()} → {arrivee.Trim()}", Depart = depart.Trim(), Arrivee = arrivee.Trim() });
        RoadService.SauverTrajets(l);
        Rafraichir();
    }

    void OnNotifChange(object? sender, ToggledEventArgs e)
    {
        if (!_chargement) Preferences.Set("notif_active", e.Value);
    }

    void OnHeureChange(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_chargement || e.PropertyName != nameof(TimePicker.Time)) return;
        object? t = HeurePicker.Time; // fonctionne que Time soit nullsable ou non selon la version de MAUI
        if (t is TimeSpan ts) Preferences.Set("notif_minutes", (int)ts.TotalMinutes);
    }
}