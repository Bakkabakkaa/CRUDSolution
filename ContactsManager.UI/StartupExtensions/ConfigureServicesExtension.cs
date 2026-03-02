using ContactsManager.Core.Domain.IdentityEntities;
using CRUDSolution.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDSolution;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ResponseHeaderActionFilter>();
// It adds controllers and views as services
        services.AddControllersWithViews(options =>
        {
            // options.Filters.Add<ResponseHeaderActionFilter>();
            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
            options.Filters.Add(new ResponseHeaderActionFilter(logger)
            {
                Key = "My-Key-From-Global",
                Value = "My-Value-From-Global",
                Order = 2
            });
        });

// Add services into IoC container
        services.AddScoped<ICountriesService, CountriesService>();
        services.AddScoped<IPersonsGetterService, PersonsGetterService>();
        services.AddScoped<IPersonsAdderService, PersonsAdderService>();
        services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
        services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
        services.AddScoped<IPersonsSorterService, PersonsSorterService>();
        services.AddScoped<ICountriesRepository, CountriesRepository>();
        services.AddScoped<IPersonsRepository, PersonsRepository>();
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
        }); 
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        // Enable Identity in this project
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

        services.AddTransient<PersonsListActionFilter>();

        return services;
    }
}