namespace RouteOuverte.Models;

public class Signalement
{
    public string Troncon { get; set; } = "";
    public StatutRoute Statut { get; set; }
    public DateTime Date { get; set; }
}