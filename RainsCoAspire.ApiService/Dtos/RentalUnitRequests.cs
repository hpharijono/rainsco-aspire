using System.ComponentModel.DataAnnotations;

namespace RainsCoAspire.ApiService.Dtos;

// Bound from multipart/form-data so images can be uploaded in the same request.
public class CreateRentalUnitRequest
{
    [Required]
    [StringLength(100)]
    public string Alias { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal SquareMeters { get; set; }

    // JPEG, PNG, WebP or GIF; max 3 MB each.
    public List<IFormFile>? Images { get; set; }
}

public class UpdateRentalUnitRequest : CreateRentalUnitRequest
{
    // Existing images to delete. Images in the Images list are added.
    public List<Guid>? RemoveImageIds { get; set; }
}
