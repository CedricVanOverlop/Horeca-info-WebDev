namespace Infrastructure.Models;

public class CarteFideliteDb
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int Points { get; set; }
}
