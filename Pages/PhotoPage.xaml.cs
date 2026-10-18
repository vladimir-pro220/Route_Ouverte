namespace RouteOuverte.Pages;

public partial class PhotoPage : ContentPage
{
    public PhotoPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Photo.Source = ImageSource.FromFile(Choix.PhotoChemin);
        Legende.Text = Choix.PhotoLegende;
    }

    async void OnFermer(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}