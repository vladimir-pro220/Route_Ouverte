using RouteOuverte.Models;

namespace RouteOuverte.Pages;

public partial class MesTrajetsPage : ContentPage
{
    const int MaxTrajets = 3;
    bool _chargement;

    public MesTrajetsPage()
    {
        InitializeComponent();
        Heure.HeureChoisie += (_, ts) =>
        {
            Preferences.Set("notif_minutes", (int)ts.TotalMinutes);
            AfficherHeure();
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _chargement = true;
        NotifSwitch.IsToggled = Preferences.Get("notif_active", true);
        _chargement = false;
        MajNotif();
        Rafraichir();
    }

    void Rafraichir()
    {
        BindableLayout.SetItemsSource(Liste, null);
        BindableLayout.SetItemsSource(Liste, RoadService.Trajets());
    }

    // Écran 21 : interrupteur éteint -> heure grisée et texte différent
    void MajNotif()
    {
        var actif = NotifSwitch.IsToggled;
        NotifTexte.Text = actif
            ? "État de vos trajets avant de partir"
            : "Activez la notification pour être prévenu chaque matin.";
        HeureZone.Opacity = actif ? 1 : 0.4;
        AfficherHeure();
    }

    void AfficherHeure()
    {
        var min = Preferences.Get("notif_minutes", 6 * 60 + 30);
        HeureLabel.Text = $"{min / 60} h {min % 60:00}";
    }

    void OnNotifChange(object? sender, ToggledEventArgs e)
    {
        if (_chargement) return;
        Preferences.Set("notif_active", e.Value);
        MajNotif();
    }

    // Écran 22
    void OnHeure(object? sender, TappedEventArgs e)
    {
        if (!NotifSwitch.IsToggled) return;
        Heure.Ouvrir(Preferences.Get("notif_minutes", 6 * 60 + 30));
    }

    // Écran 17
    async void OnTrajetTapped(object? sender, TappedEventArgs e)
    {
        var t = (Trajet)((Border)sender!).BindingContext;
        await Shell.Current.GoToAsync($"{nameof(DetailTrajetPage)}?i={t.Index}");
    }

    // Écrans 18 ou 19
    async void OnAjouter(object? sender, TappedEventArgs e)
    {
        if (RoadService.Trajets().Count >= MaxTrajets)
        {
            Limite.Afficher("Limite atteinte",
                "Vous avez déjà 3 trajets. Supprimez-en un pour en ajouter un nouveau.", "Compris");
            return;
        }
        Choix.TrajetEdition = -1;
        Choix.TrajetNom = "";
        Choix.TrajetDepart = "";
        Choix.TrajetArrivee = "";
        await Shell.Current.GoToAsync(nameof(NouveauTrajetPage));
    }

    // Écran 23
    async void OnApercu(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(NotificationPage));
}