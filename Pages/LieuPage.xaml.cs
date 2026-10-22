using Microsoft.Maui.Controls.Shapes;
namespace RouteOuverte.Pages;

[QueryProperty(nameof(Champ), "champ")]
public partial class LieuPage : ContentPage
{
    public string Champ { get; set; } = "depart";

    readonly HashSet<string> _ouvertes = new() { "Akwa", "Deido" };
    static readonly Color Gris = Color.FromArgb("#434750");

    public LieuPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = Champ.EndsWith("arrivee") ? "Choisir l'arrivée" : "Choisir le départ";
        Construire();
    }

    void OnRecherche(object? sender, TextChangedEventArgs e) => Construire();

    void Construire()
    {
        Contenu.Children.Clear();
        var q = (Recherche.Text ?? "").Trim();

        // Recherche : liste à plat
        if (q.Length > 0)
        {
            var trouves = RoadService.Lieux.SelectMany(z => z.Lieux)
                .Concat(RoadService.Lieux.Select(z => z.Zone))
                .Where(l => l.Contains(q, StringComparison.OrdinalIgnoreCase))
                .Distinct().ToList();

            if (trouves.Count == 0)
                Contenu.Children.Add(new Label { Text = "Aucun lieu trouvé.", TextColor = Gris, Margin = new Thickness(8, 12) });
            foreach (var l in trouves) Contenu.Children.Add(Ligne(l));
            return;
        }

        Contenu.Children.Add(Section("RÉCENTS"));
        foreach (var l in new[] { Choix.Depart, Choix.Arrivee }.Distinct())
            Contenu.Children.Add(Ligne(l));

        Contenu.Children.Add(Section("PAR ZONE"));
        foreach (var (zone, lieux) in RoadService.Lieux)
        {
            var ouverte = _ouvertes.Contains(zone);

            var g = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection(
                    new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto))
            };
            g.Add(new Label { Text = "🏙  " + zone, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center }, 0, 0);
            g.Add(new Label { Text = ouverte ? "⌃" : "⌄", FontSize = 22, TextColor = Gris, VerticalOptions = LayoutOptions.Center }, 1, 0);

            var entete = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#F6F3F2"),
                Padding = new Thickness(12, 12),
                MinimumHeightRequest = 48,
                Margin = new Thickness(0, 4, 0, 0),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
                Content = g
            };
            var z = zone;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => { if (!_ouvertes.Remove(z)) _ouvertes.Add(z); Construire(); };
            entete.GestureRecognizers.Add(tap);
            Contenu.Children.Add(entete);

            if (ouverte)
                foreach (var l in lieux) Contenu.Children.Add(Ligne(l, true));
        }
    }

    static View Section(string texte) => new Label
    {
        Text = texte,
        FontSize = 13,
        FontAttributes = FontAttributes.Bold,
        TextColor = Gris,
        Margin = new Thickness(0, 12, 0, 4)
    };

    View Ligne(string lieu, bool indente = false)
    {
        var b = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(indente ? 24 : 8, 12),
            MinimumHeightRequest = 48,
            Content = new Label { Text = "📍  " + lieu, FontSize = 16, VerticalOptions = LayoutOptions.Center }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Choisir(lieu);
        b.GestureRecognizers.Add(tap);
        return b;
    }

    async Task Choisir(string lieu)
    {
        switch (Champ)
        {
            case "arrivee": Choix.Arrivee = lieu; break;
            case "trajetdepart": Choix.TrajetDepart = lieu; break;
            case "trajetarrivee": Choix.TrajetArrivee = lieu; break;
            default: Choix.Depart = lieu; break;
        }
        await Shell.Current.GoToAsync("..");
    }
}