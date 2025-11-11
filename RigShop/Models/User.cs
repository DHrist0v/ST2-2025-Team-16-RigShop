using System.ComponentModel.DataAnnotations;

namespace RigShop.Web.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Password { get; set; } = string.Empty;  // plaintext for now

    [MaxLength(200)]
    public string? Address { get; set; }

    public bool IsAdmin { get; set; } = false;
}