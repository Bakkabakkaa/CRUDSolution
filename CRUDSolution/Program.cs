using CRUDSolution.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using Services;
using RepositoryContracts;
using Repositories;
using Serilog;

ExcelPackage.License.SetNonCommercialPersonal("CRUDSolution");
var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider serviceProvider,
    LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration) // Read configuration from built in IConfiguration
        .ReadFrom.Services(serviceProvider); // Read out current app's services and make them available to serilog
});

builder.Services.AddTransient<ResponseHeaderActionFilter>();
// It adds controllers and views as services
builder.Services.AddControllersWithViews(options =>
{
    // options.Filters.Add<ResponseHeaderActionFilter>();
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
    options.Filters.Add(new ResponseHeaderActionFilter(logger)
    {
        Key = "My-Key-From-Global",
        Value = "My-Value-From-Global",
        Order = 2
    });
});

// Add services into IoC container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
}); 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddTransient<PersonsListActionFilter>();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpLogging();

if (builder.Environment.IsEnvironment("Test") == false)
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program // Make the auto-generated Program accessible programmatically 
{
    
}