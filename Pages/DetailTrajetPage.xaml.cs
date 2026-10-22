using RouteOuverte.Models;

namespace RouteOuverte.Pages;

[QueryProperty(nameof(Indice), "i")]
public partial class DetailTrajetPage : ContentPage
{
    public string Indice { get; set; } = "0";
    Trajet? _t;

    public DetailTrajetPage()
    {
        InitializeComponent();
        Suppression.Confirme += async (_, _) =>
        {
            if (_t == null) return;
            var l = RoadService.Trajets();
            if (_t.Index >= 0 && _t.Index < l.Count) l.RemoveAt(_t.Index);
            RoadService.SauverTrajets(l);
            await Shell.Current.GoToAsync("..");
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        int.TryParse(Indice, out var i);
        var l = RoadService.Trajets();
        if (i < 0 || i >= l.Count) return;

        _t = l[i];
        Title = _t.Nom;

        var troncons = RoadService.TronconsDuTrajet(_t);
        BindableLayout.SetItemsSource(Liste, null);
        BindableLayout.SetItemsSource(Liste, troncons);
        VideLabel.IsVisible = troncons.Count == 0;

        var n = troncons.Count(x => x.Statut <= StatutRoute.Inondee);
        Bandeau.IsVisible = n > 0;
        BandeauLabel.Text = n == 1 ? "1 tronçon inondé sur votre trajet" : $"{n} tronçons inondés sur votre trajet";
    }

    async void OnEtat(object? sender, EventArgs e)
    {
        if (_t == null) return;
        await Shell.Current.GoToAsync(
            $"{nameof(EtatPage)}?depart={Uri.EscapeDataString(_t.Depart)}&arrivee={Uri.EscapeDataString(_t.Arrivee)}");
    }

    // 17 -> 18 en mode modification
    async void OnModifier(object? sender, EventArgs e)
    {
        if (_t == null) return;
        Choix.TrajetEdition = _t.Index;
        Choix.TrajetNom = _t.Nom;
        Choix.TrajetDepart = _t.Depart;
        Choix.TrajetArrivee = _t.Arrivee;
        await Shell.Current.GoToAsync(nameof(NouveauTrajetPage));
    }

    // 17 -> 20
    void OnSupprimer(object? sender, EventArgs e)
    {
        if (_t == null) return;
        Suppression.Afficher("Supprimer ce trajet ?",
            $"{_t.Nom} sera retiré de vos trajets habituels.", "Supprimer", "Annuler", Color.FromArgb("#BA1A1A"));
    }
}