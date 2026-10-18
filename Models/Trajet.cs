using System.Text.Json.Serialization;

namespace RouteOuverte.Models;

public class Trajet
{
    public string Nom { get; set; } = "";
    public string Depart { get; set; } = "";
    public string Arrivee { get; set; } = "";

    [JsonIgnore] public StatutRoute Statut { get; set; } = StatutRoute.Praticable;
    [JsonIgnore] public string Resume { get; set; } = "";
    [JsonIgnore] public string MiseAJour { get; set; } = "";
    [JsonIgnore] public Color Couleur => Statut.Couleur();
}