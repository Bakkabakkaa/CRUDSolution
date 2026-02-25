using CRUDSolution;
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

builder.Services.ConfigureServices(builder.Configuration);

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