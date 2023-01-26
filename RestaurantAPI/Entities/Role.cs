namespace RestaurantAPI.Entities;

public class Role
{
    public Role()
    {
    }

    public Role(string name)
    {
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}