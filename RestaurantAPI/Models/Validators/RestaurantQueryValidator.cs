using System.Linq;

using FluentValidation;

using RestaurantAPI.Entities;

namespace RestaurantAPI.Models.Validators;

public class RestaurantQueryValidator : AbstractValidator<RestaurantQuery>
{
    private readonly int[] allowedPageSizes = { 5, 10, 15 };

    private readonly string[] allowedSortByColumnNames =
        { nameof(Restaurant.Name), nameof(Restaurant.Category), nameof(Restaurant.Description), };

    public RestaurantQueryValidator()
    {
        RuleFor(r => r.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(r => r.PageSize).Custom((value, context) =>
        {
            if (!allowedPageSizes.Contains(value))
            {
                context.AddFailure("PageSize", $"PageSize must in [{string.Join(",", allowedPageSizes)}]");
            }
        });

        RuleFor(r => r.SortBy)
            .Must(value => string.IsNullOrEmpty(value) || allowedSortByColumnNames.Contains(value))
            .WithMessage($"Sort by is optional, or must be in [{string.Join(",", allowedSortByColumnNames)}]");
    }
}