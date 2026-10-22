using RouteOuverte.Models;

namespace RouteOuverte.Pages;

[QueryProperty(nameof(Depart), "depart")]
[QueryProperty(nameof(Arrivee), "arrivee")]
public partial class EtatPage : ContentPage
{
    public string Depart { get; set; } = "";
    public string Arrivee { get; set; } = "";

    string _zone = "Toutes";
    List<Troncon> _tous = new();
    string? _dernier;
    int _snackId;

    public EtatPage()
    {
        InitializeComponent();
        Carte.Touchee += async (_, _) => await Shell.Current.GoToAsync(nameof(CartePage));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = $"{Depart} → {Arrivee}";
        Rafraichir();

        // Retour de l'écran 09 après « Confirmer : toujours vrai »
        if (Choix.DerniereConfirmation != null)
        {
            _dernier = Choix.DerniereConfirmation;
            Choix.DerniereConfirmation = null;
            _ = MontrerSnack("Merci, confirmation enregistrée");
        }
    }

    void Rafraichir()
    {
        _tous = RoadService.EtatDuJour();

        MajLabel.Text = _tous.Count == 0 ? "Aucun signalement récent" : RoadService.Ecoule(_tous.Max(t => t.DerniereMaj));

        var n = _tous.Count(t => t.Statut <= StatutRoute.Inondee);
        Bandeau.IsVisible = n > 0;
        BandeauLabel.Text = n == 1 ? "1 tronçon inondé sur votre trajet" : $"{n} tronçons inondés sur votre trajet";

        ConstruirePuces();
        Appliquer();
    }

    void ConstruirePuces()
    {
        var res = Application.Current!.Resources;
        Puces.Children.Clear();
        foreach (var z in RoadService.Zones)
        {
            var b = new Button { Text = z, Style = (Style)res["Puce"] };
            if (z == _zone)
            {
                b.BackgroundColor = (Color)res["RoPrimary"];
                b.TextColor = Colors.White;
            }
            b.Clicked += (_, _) => { _zone = z; ConstruirePuces(); Appliquer(); };
            Puces.Children.Add(b);
        }
    }

    void Appliquer()
    {
        var filtre = _tous.Where(t => _zone == "Toutes" || t.Zone == _zone).ToList();

        BindableLayout.SetItemsSource(Liste, null);
        BindableLayout.SetItemsSource(Liste, filtre);

        EtatVide.IsVisible = filtre.Count == 0;
        BoutonSignaler.IsVisible = filtre.Count > 0;
        VideTexte.Text = _zone == "Toutes"
            ? "Aucun signalement sur votre trajet ces 6 dernières heures."
            : $"Les routes de {_zone} n'ont pas été signalées ces 6 dernières heures.";

        Carte.Afficher(_tous, _zone);
    }

    async void OnCarteTapped(object? sender, TappedEventArgs e)
    {
        var t = (Troncon)((Border)sender!).BindingContext;
        await Shell.Current.GoToAsync($"{nameof(DetailPage)}?nom={Uri.EscapeDataString(t.Nom)}");
    }

    async void OnPhotoTapped(object? sender, TappedEventArgs e)
    {
        var t = (Troncon)((Border)sender!).BindingContext;
        if (t.Photo == null) return;
        await Choix.VoirPhoto(t.Photo, $"{t.Nom} · {t.Libelle} · {RoadService.Depuis(t.DerniereMaj)}");
    }

    // Écran 11 : confirmation
    void OnConfirmer(object? sender, EventArgs e)
    {
        var t = (Troncon)((Button)sender!).BindingContext;
        if (t.Vote != null) return;
        RoadService.Voter(t.Nom, true);
        _dernier = t.Nom;
        Rafraichir();
        _ = MontrerSnack("Merci, confirmation enregistrée");
    }

    // Déconfirmation : on propose ensuite de signaler le nouvel état
    async void OnDeconfirmer(object? sender, EventArgs e)
    {
        var t = (Troncon)((Button)sender!).BindingContext;
        if (t.Vote != null) return;

        RoadService.Voter(t.Nom, false);
        _dernier = t.Nom;
        Rafraichir();

        var oui = await DisplayAlertAsync(
            "Vous n'êtes pas d'accord ?",
            $"Vous avez déconfirmé l'état « {t.Libelle} » de {t.Nom}. Voulez-vous signaler le nouvel état de cette route ?",
            "Signaler le nouvel état", "Plus tard");

        if (oui)
        {
            Brouillon.Route = t.Nom;
            await Shell.Current.GoToAsync("//Signaler");
        }
        else
        {
            _ = MontrerSnack("Votre désaccord est enregistré");
        }
    }

    void OnAnnulerConfirmation(object? sender, EventArgs e)
    {
        if (_dernier != null) RoadService.AnnulerVote(_dernier);
        _dernier = null;
        _snackId++;
        Snack.IsVisible = false;
        Rafraichir();
    }

    async Task MontrerSnack(string texte)
    {
        var id = ++_snackId;
        SnackTexte.Text = texte;
        Snack.IsVisible = true;
        await Task.Delay(5000);
        if (id == _snackId) Snack.IsVisible = false;
    }

    async void OnSignaler(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//Signaler");
}