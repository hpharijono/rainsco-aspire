namespace RainsCoAspire.ApiService.Models;

public class RentalUnit
{
    public int Id { get; set; }

    public string Alias { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal SquareMeters { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<RentalUnitImage> Images { get; set; } = [];
}
