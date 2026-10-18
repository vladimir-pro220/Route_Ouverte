using RouteOuverte.Services;

namespace RouteOuverte.Views;

public class LigneRoute
{
    public string Nom { get; set; } = "";
    public string Zone { get; set; } = "";
    public bool Selectionne { get; set; }
    public string TexteZone => $"Zone : {Zone}";
    public string Radio => Selectionne ? "◉" : "○";
}

public partial class RouteSheet : ContentView
{
    static readonly string[] Zones = { "Toutes", "Akwa", "Deido", "Bonanjo", "Ndokoti" };
    string _zone = "Toutes";

    public event EventHandler<string>? RouteChoisie;

    public RouteSheet() => InitializeComponent();

    public void Ouvrir()
    {
        _zone = "Toutes";
        Rafraichir();
        IsVisible = true;
    }

    void Rafraichir()
    {
        var res = Application.Current!.Resources;

        Puces.Children.Clear();
        foreach (var z in Zones)
        {
            var b = new Button { Text = z, Style = (Style)res["Puce"] };
            if (z == _zone)
            {
                b.BackgroundColor = (Color)res["RoPrimary"];
                b.TextColor = Colors.White;
            }
            b.Clicked += (_, _) => { _zone = z; Rafraichir(); };
            Puces.Children.Add(b);
        }

        var lignes = RoadService.NomsTroncons
            .Where(n => _zone == "Toutes" || RoadService.Zone(n) == _zone)
            .Select(n => new LigneRoute { Nom = n, Zone = RoadService.Zone(n), Selectionne = n == Brouillon.Route })
            .ToList();

        BindableLayout.SetItemsSource(Liste, lignes);
        Vide.IsVisible = lignes.Count == 0;
    }

    void OnFermer(object? sender, EventArgs e) => IsVisible = false;

    void OnLigneTapped(object? sender, TappedEventArgs e)
    {
        var ligne = (LigneRoute)((Border)sender!).BindingContext;
        RouteChoisie?.Invoke(this, ligne.Nom);
        IsVisible = false;
    }
}