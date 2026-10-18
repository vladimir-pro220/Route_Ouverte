namespace RouteOuverte.Pages;

public partial class TrajetPage : ContentPage
{
    public TrajetPage() => InitializeComponent();

    void OnInverser(object? sender, EventArgs e)
    {
        var d = DepartEntry.Text;
        DepartEntry.Text = ArriveeEntry.Text;
        ArriveeEntry.Text = d;
    }

    void OnFavori(object? sender, EventArgs e)
    {
        var lieu = ((Button)sender!).Text;
        if (ArriveeEntry.Text == lieu) DepartEntry.Text = lieu; else ArriveeEntry.Text = lieu;
    }

    async void OnVoirEtat(object? sender, EventArgs e)
    {
        var d = DepartEntry.Text?.Trim() ?? "";
        var a = ArriveeEntry.Text?.Trim() ?? "";
        if (d == "" || a == "")
        {
            await DisplayAlert("Trajet incomplet", "Indiquez un départ et une arrivée.", "OK");
            return;
        }
        await Shell.Current.GoToAsync($"{nameof(EtatPage)}?depart={Uri.EscapeDataString(d)}&arrivee={Uri.EscapeDataString(a)}");
    }
}