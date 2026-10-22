using Microsoft.Maui.Controls.Shapes;
using RouteOuverte.Models;

namespace RouteOuverte.Pages;

[QueryProperty(nameof(Nom), "nom")]
public partial class DetailPage : ContentPage
{
    public string Nom { get; set; } = "";
    Troncon? _t;

    public DetailPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = Nom;
        Remplir();
    }

    void Remplir()
    {
        _t = RoadService.EtatDuJour().FirstOrDefault(x => x.Nom == Nom);
        if (_t == null) return;

        Badge.BackgroundColor = _t.Couleur;
        BadgeLabel.Text = _t.Libelle;
        BadgeLabel.TextColor = _t.CouleurTexte;
        ZoneLabel.Text = "📍 " + _t.TexteZone;
        ExpireLabel.Text = "⏱ " + _t.TexteExpire;
        NbSignalements.Text = _t.NbSignalements.ToString();
        NbConfirmations.Text = _t.Confirmations.ToString();

        DeconfLabel.IsVisible = _t.Deconfirmations > 0;
        DeconfLabel.Text = $"👎 {_t.Deconfirmations} déconfirmation{(_t.Deconfirmations > 1 ? "s" : "")}";
        ConstesteLabel.IsVisible = _t.Conteste;

        BtnConfirmer.Text = _t.Confirme ? "✓ Confirmé" : "👍 Confirmer : toujours vrai";
        BtnDeconfirmer.Text = _t.Deconfirme ? "✓ Déconfirmé" : "👎 Déconfirmer : l'état a changé";

        var sigs = RoadService.Derniers(Nom);

        Photos.Children.Clear();
        foreach (var s in sigs.Where(x => !string.IsNullOrEmpty(x.Photo) && File.Exists(x.Photo)).Take(4))
        {
            var b = new Border
            {
                StrokeThickness = 0,
                Padding = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
                Content = new Image { Source = ImageSource.FromFile(s.Photo!), Aspect = Aspect.AspectFill, WidthRequest = 96, HeightRequest = 96 }
            };
            var legende = $"{Nom} · {s.Statut.Libelle()} · {RoadService.Depuis(s.Date)}";
            var chemin = s.Photo!;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await Choix.VoirPhoto(chemin, legende);
            b.GestureRecognizers.Add(tap);
            Photos.Children.Add(b);
        }
        BlocPhotos.IsVisible = Photos.Children.Count > 0;

        BindableLayout.SetItemsSource(Derniers, sigs.Take(3).Select(s => new LigneSignalement
        {
            Texte = $"{RoadService.Depuis(s.Date)} · {s.Statut.Libelle()}",
            Couleur = s.Statut.Couleur()
        }).ToList());
    }

    // 09 -> 11 : on confirme puis on revient à l'état du jour, qui affiche la barre « Merci »
    async void OnConfirmer(object? sender, EventArgs e)
    {
        if (_t == null || _t.Vote != null) return;
        RoadService.Voter(_t.Nom, true);
        Choix.DerniereConfirmation = _t.Nom;
        await Shell.Current.GoToAsync("..");
    }

    async void OnDeconfirmer(object? sender, EventArgs e)
    {
        if (_t == null || _t.Vote != null) return;
        var libelle = _t.Libelle;
        RoadService.Voter(_t.Nom, false);
        Remplir();

        var oui = await DisplayAlertAsync(
            "Vous n'êtes pas d'accord ?",
            $"Vous avez déconfirmé l'état « {libelle} » de {Nom}. Voulez-vous signaler le nouvel état de cette route ?",
            "Signaler le nouvel état", "Plus tard");

        if (oui)
        {
            Brouillon.Route = Nom;
            await Shell.Current.GoToAsync("//Signaler");
        }
    }

    async void OnChangement(object? sender, EventArgs e)
    {
        Brouillon.Route = Nom;
        await Shell.Current.GoToAsync("//Signaler");
    }
}