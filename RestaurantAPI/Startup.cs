using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Text;

using AutoMapper;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Filters;
using RestaurantAPI.Middleware;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using RestaurantAPI.Services;

namespace RestaurantAPI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var authenticationSettings = new AuthenticationSettings();

        Configuration.GetSection("Authentication").Bind(authenticationSettings);

        services.AddSingleton(authenticationSettings);
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = "Bearer";
            option.DefaultScheme = "Bearer";
            option.DefaultChallengeScheme = "Bearer";
        }).AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = authenticationSettings.JwtIssuer,
                ValidAudience = authenticationSettings.JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
            };
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "German", "Polish"));
            options.AddPolicy("Atleast20", builder => builder.AddRequirements(new MinimumAgeRequirement(20)));
            options.AddPolicy("CreatedAtleast2Restaurants",
                builder => builder.AddRequirements(new CreatedMultipleRestaurantsRequirement(2)));
        });

        
        services.AddScoped<IAuthorizationHandler, CreatedMultipleRestaurantsRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();
        services.AddScoped<IAuthorizationHandler, ResourceOperationRequirementHandler>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddControllers(o=>o.Filters.Add(typeof(ExceptionFilters)));
        services.AddFluentValidationClientsideAdapters();
        services.AddResponseCaching();
        services.AddMemoryCache();
        services.AddFluentValidationAutoValidation();

        services.AddScoped<RestaurantSeeder>();
        services.AddAutoMapper(GetType().Assembly);
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IDishService, DishService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ErrorHandlingMiddleware>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
        services.AddScoped<IValidator<RestaurantQuery>, RestaurantQueryValidator>();
        services.AddScoped<RequestTimeMiddleware>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddHttpContextAccessor();
        services.AddSwaggerGen();
        services.AddCors(options =>
        {
            options.AddPolicy("FrontEndClient", builder =>
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins(Configuration["AllowedOrigins"]!)
            );
        });

        services.AddDbContext<RestaurantDbContext>
            (options => options.UseSqlServer(Configuration.GetConnectionString("RestaurantDbConnection")!));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RestaurantSeeder seeder)
    {
        app.UseResponseCaching();
        app.UseStaticFiles();
        app.UseCors("FrontEndClient");
        seeder.Seed();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseMiddleware<RequestTimeMiddleware>();
        app.UseAuthentication();
        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API"); });

        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}