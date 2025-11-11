using System.ComponentModel.DataAnnotations;

namespace RigShop.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = new();
}