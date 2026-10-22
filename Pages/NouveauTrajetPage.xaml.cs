using Microsoft.Extensions.Logging.Abstractions;
using RouteOuverte.Models;

namespace RouteOuverte.Pages;

public partial class NouveauTrajetPage : ContentPage
{
    const int MaxTrajets = 3;
    static readonly Color Gris = Color.FromArgb("#434750");

    public NouveauTrajetPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = Choix.TrajetEdition >= 0 ? "Modifier le trajet" : "Nouveau trajet";
        NomEntry.Text = Choix.TrajetNom;
        CompteurLabel.Text = $"{RoadService.Trajets().Count} sur {MaxTrajets} trajets enregistrés";
        ErreurLabel.IsVisible = false;
        Afficher();
    }

    void Afficher()
    {
        var d = Choix.TrajetDepart;
        var a = Choix.TrajetArrivee;
        DepartLabel.Text = string.IsNullOrEmpty(d) ? "Choisir un lieu" : d;
        DepartLabel.TextColor = string.IsNullOrEmpty(d) ? Gris : (Color)Application.Current!.Resources["RoTextPrimary"];
        ArriveeLabel.Text = string.IsNullOrEmpty(a) ? "Choisir un lieu" : a;
        ArriveeLabel.TextColor = string.IsNullOrEmpty(a) ? Gris : (Color)Application.Current!.Resources["RoTextPrimary"];
    }

    void Erreur(string message)
    {
        ErreurLabel.Text = "⚠ " + message;
        ErreurLabel.IsVisible = true;
    }

    // Écran 05, ouvert depuis ce formulaire
    async void OnDepart(object? sender, TappedEventArgs e)
    {
        Choix.TrajetNom = NomEntry.Text ?? "";
        await Shell.Current.GoToAsync($"{nameof(LieuPage)}?champ=trajetdepart");
    }

    async void OnArrivee(object? sender, TappedEventArgs e)
    {
        Choix.TrajetNom = NomEntry.Text ?? "";
        await Shell.Current.GoToAsync($"{nameof(LieuPage)}?champ=trajetarrivee");
    }

    async void OnEnregistrer(object? sender, EventArgs e)
    {
        var d = Choix.TrajetDepart;
        var a = Choix.TrajetArrivee;

        if (string.IsNullOrEmpty(d) || string.IsNullOrEmpty(a))
        { Erreur("Choisissez un départ et une arrivée."); return; }
        if (string.Equals(d, a, StringComparison.OrdinalIgnoreCase))
        { Erreur("Le départ et l'arrivée doivent être différents."); return; }

        var nom = string.IsNullOrWhiteSpace(NomEntry.Text) ? $"{d} → {a}" : NomEntry.Text.Trim();
        var l = RoadService.Trajets();
        var t = new Trajet { Nom = nom, Depart = d, Arrivee = a };

        var modification = Choix.TrajetEdition >= 0 && Choix.TrajetEdition < l.Count;
        if (modification) l[Choix.TrajetEdition] = t;
        else
        {
            if (l.Count >= MaxTrajets) { Erreur("Limite de 3 trajets atteinte."); return; }
            l.Add(t);
        }

        RoadService.SauverTrajets(l);
        Choix.TrajetEdition = -1;
        Choix.TrajetNom = Choix.TrajetDepart = Choix.TrajetArrivee = "";

        // Modification : on revient à l'écran 04 (en passant au-dessus du détail 17)
        await Shell.Current.GoToAsync(modification ? "../.." : "..");
    }
}