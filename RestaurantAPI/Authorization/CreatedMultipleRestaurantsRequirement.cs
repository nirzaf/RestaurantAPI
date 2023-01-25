using Microsoft.AspNetCore.Authorization;

namespace RestaurantAPI.Authorization;

public class CreatedMultipleRestaurantsRequirement : IAuthorizationRequirement
{
    public CreatedMultipleRestaurantsRequirement(int minimumRestaurantsCreated)
    {
        MinimumRestaurantsCreated = minimumRestaurantsCreated;
    }

    public int MinimumRestaurantsCreated { get; }
}