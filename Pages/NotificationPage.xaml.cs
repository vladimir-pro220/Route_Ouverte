using System.Globalization;
using RouteOuverte.Models;

namespace RouteOuverte.Pages;

public partial class NotificationPage : ContentPage
{
    public NotificationPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var min = Preferences.Get("notif_minutes", 6 * 60 + 30);
        HeureLabel.Text = $"{min / 60:00}:{min % 60:00}";

        var d = DateTime.Now.ToString("dddd d MMMM", new CultureInfo("fr-FR"));
        DateLabel.Text = char.ToUpper(d[0]) + d[1..];

        var l = RoadService.Trajets().Take(2)
            .Select(t => new NotifLigne { Index = t.Index, Titre = $"{t.Nom} : {Minuscule(t.Resume)}" })
            .ToList();
        BindableLayout.SetItemsSource(Notifs, l);
    }

    static string Minuscule(string s) => s.Length == 0 ? s : char.ToLower(s[0]) + s[1..];

    // 23 -> 17
    async void OnNotifTapped(object? sender, TappedEventArgs e)
    {
        var n = (NotifLigne)((Border)sender!).BindingContext;
        await Shell.Current.GoToAsync($"{nameof(DetailTrajetPage)}?i={n.Index}");
    }

    async void OnFermer(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}