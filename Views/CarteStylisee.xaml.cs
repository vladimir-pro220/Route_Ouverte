using RouteOuverte.Models;

namespace RouteOuverte.Views;

/// <summary>Carte stylisée (sans fond de carte) : 3 segments colorés selon l'état de chaque tronçon.</summary>
public partial class CarteStylisee : ContentView
{
    /// <summary>Touche sur la carte entière (écran 02).</summary>
    public event EventHandler? Touchee;
    /// <summary>Touche sur un segment (écran 08).</summary>
    public event EventHandler<string>? SegmentTouche;

    /// <summary>true : les segments sont tactiles (écran 08) ; false : toute la carte est tactile (écran 02).</summary>
    public bool Interactive { get; set; }

    public CarteStylisee() => InitializeComponent();

    void OnTap(object? sender, TappedEventArgs e)
    {
        if (!Interactive) Touchee?.Invoke(this, EventArgs.Empty);
    }

    public void Afficher(List<Troncon> troncons, string zone = "Toutes", string? selection = null, bool etiquettes = false)
    {
        Trace.Children.Clear();

        for (int i = 0; i < RoadService.NomsTroncons.Length; i++)
        {
            var nom = RoadService.NomsTroncons[i];
            var t = troncons.FirstOrDefault(x => x.Nom == nom);

            // Gris si le tronçon n'a pas de signalement ou n'est pas dans la zone filtrée
            var couleur = Color.FromArgb("#C3C6D2");
            if (t != null && (zone == "Toutes" || t.Zone == zone)) couleur = t.Couleur;

            var barre = new BoxView
            {
                Color = couleur,
                CornerRadius = new CornerRadius(6),
                HeightRequest = nom == selection ? 18 : 12,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(2, 0)
            };
            var zoneTactile = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Colors.Transparent,
                Padding = 0,
                HeightRequest = 48,
                VerticalOptions = LayoutOptions.Center,
                Content = barre
            };

            if (Interactive)
            {
                var tap = new TapGestureRecognizer();
                var n = nom;
                tap.Tapped += (_, _) => SegmentTouche?.Invoke(this, n);
                zoneTactile.GestureRecognizers.Add(tap);
            }

            Grid.SetColumn(zoneTactile, i);
            Grid.SetRow(zoneTactile, 0);
            Trace.Children.Add(zoneTactile);
        }

        if (etiquettes)
        {
            var depart = new Label { Text = "● Départ · " + Choix.Depart, FontSize = 13, FontAttributes = FontAttributes.Bold };
            Grid.SetRow(depart, 1); Grid.SetColumn(depart, 0); Grid.SetColumnSpan(depart, 2);
            Trace.Children.Add(depart);

            var arrivee = new Label { Text = "📍 Arrivée · " + Choix.Arrivee, FontSize = 13, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.End };
            Grid.SetRow(arrivee, 1); Grid.SetColumn(arrivee, 1); Grid.SetColumnSpan(arrivee, 2);
            Trace.Children.Add(arrivee);
        }
    }
}