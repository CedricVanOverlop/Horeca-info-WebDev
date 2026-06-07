namespace Infrastructure.Models;

public class TransactionDb
{
    public string Id { get; set; } = string.Empty;
    public string CarteFideliteId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string? Description { get; set; }
}
