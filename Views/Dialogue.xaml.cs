namespace RouteOuverte.Views;

public partial class Dialogue : ContentView
{
    public event EventHandler? Confirme;

    public Dialogue() => InitializeComponent();

    /// <summary>annuler = null : un seul bouton (écran 19).</summary>
    public void Afficher(string titre, string message, string ok, string? annuler = null, Color? couleurOk = null)
    {
        TitreLabel.Text = titre;
        MessageLabel.Text = message;
        BtnOk.Text = ok;
        BtnOk.TextColor = couleurOk ?? (Color)Application.Current!.Resources["RoPrimary"];
        BtnAnnuler.IsVisible = annuler != null;
        if (annuler != null) BtnAnnuler.Text = annuler;
        IsVisible = true;
    }

    void OnAnnuler(object? sender, EventArgs e) => IsVisible = false;

    void OnOk(object? sender, EventArgs e)
    {
        IsVisible = false;
        Confirme?.Invoke(this, EventArgs.Empty);
    }
}