using CRUDSolution.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDSolution;

public static class ConfigureServicesExtension
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<IPersonsService, PersonsService>();
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

        services.AddTransient<PersonsListActionFilter>();
    }
}