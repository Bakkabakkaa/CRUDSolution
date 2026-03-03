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
using CRUDSolution.Middleware;

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


if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();

app.UseHttpLogging();

if (builder.Environment.IsEnvironment("Test") == false)
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseStaticFiles();
app.UseRouting(); // Identifying action method based route
app.UseAuthentication(); // Reading Identity cookie
app.UseAuthorization(); // Validates access permissions of the user
app.MapControllers(); // Execute the filter pipeline (action + filters)

app.Run();

public partial class Program // Make the auto-generated Program accessible programmatically 
{
    
}