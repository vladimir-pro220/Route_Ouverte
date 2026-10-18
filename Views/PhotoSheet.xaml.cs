namespace RouteOuverte.Views;

public partial class PhotoSheet : ContentView
{
    public event EventHandler<string>? PhotoChoisie;

    public PhotoSheet() => InitializeComponent();

    void OnAnnuler(object? sender, EventArgs e) => IsVisible = false;

    async void OnPrendre(object? sender, TappedEventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlertAsync("Appareil photo", "Aucun appareil photo disponible. Choisissez une photo dans la galerie.", "OK");
                return;
            }
            Terminer(await MediaPicker.Default.CapturePhotoAsync());
        }
        catch (Exception ex) { await Erreur(ex); }
    }

    async void OnGalerie(object? sender, TappedEventArgs e)
    {
        try { Terminer(await MediaPicker.Default.PickPhotoAsync()); }
        catch (Exception ex) { await Erreur(ex); }
    }

    // Copie la photo dans le dossier de l'application pour la conserver
    async void Terminer(FileResult? fichier)
    {
        if (fichier == null) return;
        var dest = Path.Combine(FileSystem.AppDataDirectory, $"photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fichier.FileName)}");
        using (var src = await fichier.OpenReadAsync())
        using (var fs = File.Create(dest))
            await src.CopyToAsync(fs);

        IsVisible = false;
        PhotoChoisie?.Invoke(this, dest);
    }

    static Task Erreur(Exception ex) =>
        Shell.Current.DisplayAlertAsync("Photo indisponible", ex.Message, "OK");
}