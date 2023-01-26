namespace RestaurantAPI.Entities;

public sealed class Dish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; }
}