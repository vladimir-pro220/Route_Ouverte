namespace RouteOuverte.Views;

public partial class HeureDialog : ContentView
{
    int _h, _m;

    public event EventHandler<TimeSpan>? HeureChoisie;

    public HeureDialog() => InitializeComponent();

    public void Ouvrir(int minutes)
    {
        _h = minutes / 60;
        _m = minutes % 60;
        Maj();
        IsVisible = true;
    }

    void Maj()
    {
        HLabel.Text = _h.ToString("00");
        MLabel.Text = _m.ToString("00");
    }

    void OnHPlus(object? sender, EventArgs e) { _h = (_h + 1) % 24; Maj(); }
    void OnHMoins(object? sender, EventArgs e) { _h = (_h + 23) % 24; Maj(); }
    void OnMPlus(object? sender, EventArgs e) { _m = (_m + 5) % 60; Maj(); }
    void OnMMoins(object? sender, EventArgs e) { _m = (_m + 55) % 60; Maj(); }

    void OnAnnuler(object? sender, EventArgs e) => IsVisible = false;

    void OnOk(object? sender, EventArgs e)
    {
        IsVisible = false;
        HeureChoisie?.Invoke(this, new TimeSpan(_h, _m, 0));
    }
}