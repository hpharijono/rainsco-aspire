using System.ComponentModel.DataAnnotations;

namespace RainsCoAspire.Web.Models;

public class RentalUnitFormModel
{
    [Required]
    [StringLength(100)]
    public string Alias { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "The size is required.")]
    [Range(0.01, 1_000_000, ErrorMessage = "The size must be greater than 0.")]
    public decimal? SquareMeters { get; set; }

    public static RentalUnitFormModel FromRentalUnit(RentalUnit unit) => new()
    {
        Alias = unit.Alias,
        Description = unit.Description,
        SquareMeters = unit.SquareMeters,
    };
}

public sealed record RentalUnitFormSubmission(
    RentalUnitFormModel Model,
    IReadOnlyList<ImageUpload> NewImages,
    IReadOnlyCollection<Guid> RemovedImageIds);
