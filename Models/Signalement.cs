namespace RouteOuverte.Models;

public enum TypeEvenement { Signalement = 0, Confirmation = 1, Deconfirmation = 2 }

public class Signalement
{
    public string Troncon { get; set; } = "";
    public StatutRoute Statut { get; set; }
    public DateTime Date { get; set; }
    public string? Photo { get; set; }
    public TypeEvenement Type { get; set; } = TypeEvenement.Signalement;
}