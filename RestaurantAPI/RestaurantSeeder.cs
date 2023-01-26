using System;
using System.Collections.Generic;
using System.Linq;

using Bogus;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using RestaurantAPI.Entities;
using RestaurantAPI.Models;

namespace RestaurantAPI;

public class RestaurantSeeder
{
    private readonly RestaurantDbContext _dbContext;
    private IPasswordHasher<User> _passwordHasher;

    public RestaurantSeeder(RestaurantDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public void Seed()
    {
        if (!_dbContext.Database.CanConnect()) return;
        if (_dbContext.Database.IsRelational())
        {
            var pendingMigrations = _dbContext.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                _dbContext.Database.Migrate();
            }
        }

        if (!_dbContext.Roles.Any())
        {
            var roles = GetRoles();
            _dbContext.Roles.AddRange(roles);
            _dbContext.SaveChanges();
        }

        if (_dbContext.Restaurants.Any()) return;
        var restaurants = GetRestaurants();
        _dbContext.Restaurants.AddRange(restaurants);
        _dbContext.SaveChanges();
    }

    private IEnumerable<Role> GetRoles()
    {
        var roles = new List<Role>
        {
            new(name: Roles.User),
            new(name: Roles.Manager),
            new(name: Roles.Admin),
        };

        return roles;
    }

    private IEnumerable<Restaurant> GetRestaurants()
    {
        //if name is more than 25 characters, limit it to 24 characters
        var restaurantList = new Faker<Restaurant>()
            .RuleFor(r => r.Name, f => f.Company.CompanyName())
            .RuleFor(r => r.Category, f => f.Commerce.Categories(1).First())
            .RuleFor(r => r.Description, f => f.Lorem.Paragraph())
            .RuleFor(r => r.ContactEmail, f => f.Internet.Email())
            .RuleFor(r => r.HasDelivery, f => f.Random.Bool())
            .RuleFor(r => r.ContactNumber, f => f.Phone.PhoneNumber())
            .RuleFor(r => r.Address, f => new Address
            {
                City = f.Address.City(),
                Street = f.Address.StreetAddress(),
                PostalCode = f.Address.ZipCode()
            })
            .RuleFor(r => r.Dishes, _ => new Faker<Dish>()
                .RuleFor(d => d.Name, f => f.Commerce.ProductName())
                .RuleFor(d => d.Description, f => f.Lorem.Paragraph())
                .RuleFor(d => d.Price, f => f.Random.Decimal(100, 1000))
                .Generate(10))
            .RuleFor(r => r.CreatedBy, _ => new Faker<User>()
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
                .RuleFor(u => u.Role, f => f.PickRandom(_dbContext.Roles.ToList()))
                .RuleFor(u => u.Nationality, f => f.Address.Country())
                .RuleFor(u => u.RoleId, f => f.PickRandom(_dbContext.Roles.ToList()).Id)
                .RuleFor(u => u.DateOfBirth, f => f.Date.Past(18, DateTime.Now.AddYears(-18)))
                .Generate())
            .Generate(100);
        //log all email address and respective password tp console  
        foreach (var restaurant in restaurantList)
        {
            Console.WriteLine($"{restaurant.CreatedBy.Email} - {restaurant.CreatedBy.PasswordHash}");
        }

        //hash the password
        foreach (var restaurant in restaurantList)
        {
            restaurant.CreatedBy.PasswordHash =
                _passwordHasher.HashPassword(restaurant.CreatedBy, restaurant.CreatedBy.PasswordHash);
        }

        return restaurantList;
    }
}