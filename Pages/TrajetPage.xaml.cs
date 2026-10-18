namespace RouteOuverte.Pages;

public partial class TrajetPage : ContentPage
{
    public TrajetPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Afficher();
    }

    void Afficher()
    {
        DepartLabel.Text = Choix.Depart;
        ArriveeLabel.Text = Choix.Arrivee;
        Erreur(false);
    }

    // Écran 06 : champ Arrivée bordé de rouge + message
    void Erreur(bool oui)
    {
        ErreurLabel.IsVisible = oui;
        ArriveeCarte.Stroke = oui ? new SolidColorBrush(Color.FromArgb("#BA1A1A")) : new SolidColorBrush(Colors.Transparent);
        ArriveeCarte.StrokeThickness = oui ? 2 : 0;
    }

    async void OnChoisirDepart(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync($"{nameof(LieuPage)}?champ=depart");

    async void OnChoisirArrivee(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync($"{nameof(LieuPage)}?champ=arrivee");

    void OnInverser(object? sender, EventArgs e)
    {
        (Choix.Depart, Choix.Arrivee) = (Choix.Arrivee, Choix.Depart);
        Afficher();
    }

    // Un favori remplit l'arrivée (ou le départ si l'arrivée est déjà ce favori)
    void OnFavori(object? sender, EventArgs e)
    {
        var lieu = ((Button)sender!).Text;
        if (lieu == "Marché") lieu = "Marché Deido";
        if (Choix.Arrivee == lieu) Choix.Depart = lieu; else Choix.Arrivee = lieu;
        Afficher();
    }

    async void OnVoirEtat(object? sender, EventArgs e)
    {
        if (string.Equals(Choix.Depart.Trim(), Choix.Arrivee.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            Erreur(true);
            return;
        }
        await Shell.Current.GoToAsync(
            $"{nameof(EtatPage)}?depart={Uri.EscapeDataString(Choix.Depart)}&arrivee={Uri.EscapeDataString(Choix.Arrivee)}");
    }
}