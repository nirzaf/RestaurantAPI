using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models;

public class CreateDishDto
{
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }

    public int RestaurantId { get; set; }
}