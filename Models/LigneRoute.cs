namespace RouteOuverte.Models;

public class LigneRoute
{
    public string Nom { get; set; } = "";
    public string Zone { get; set; } = "";
    public bool Selectionne { get; set; }
    public string TexteZone => $"Zone : {Zone}";
    public string Radio => Selectionne ? "◉" : "○";
}