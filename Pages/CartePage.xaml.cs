using CommunityToolkit.Maui.ApplicationModel;
using RouteOuverte.Models;

namespace RouteOuverte.Pages;

public partial class CartePage : ContentPage
{
    List<Troncon> _t = new();
    Troncon? _sel;

    public CartePage()
    {
        InitializeComponent();
        Carte.SegmentTouche += (_, nom) => Choisir(nom);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Choix.DerniereConfirmation = null; // la barre « Merci » ne concerne que l'écran 02
        _t = RoadService.EtatDuJour();
        var pire = _t.OrderBy(x => x.Statut).FirstOrDefault(); // le tronçon le plus mauvais est sélectionné
        Choisir(_sel != null && _t.Any(x => x.Nom == _sel.Nom) ? _sel.Nom : pire?.Nom);
    }

    void Choisir(string? nom)
    {
        _sel = _t.FirstOrDefault(x => x.Nom == nom);
        Feuille.IsVisible = _sel != null;
        Carte.Afficher(_t, "Toutes", _sel?.Nom, true);
        if (_sel == null) return;

        NomLabel.Text = _sel.Nom;
        Badge.BackgroundColor = _sel.Couleur;
        BadgeLabel.Text = _sel.Libelle;
        BadgeLabel.TextColor = _sel.CouleurTexte;
        CompteursLabel.Text = "⏱ " + _sel.TexteCompteurs;
    }

    async void OnDetail(object? sender, EventArgs e)
    {
        if (_sel == null) return;
        await Shell.Current.GoToAsync($"{nameof(DetailPage)}?nom={Uri.EscapeDataString(_sel.Nom)}");
    }
}