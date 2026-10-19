using System.ComponentModel.DataAnnotations;

namespace RainsCoAspire.Web.Models;

public class LoginFormModel
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
