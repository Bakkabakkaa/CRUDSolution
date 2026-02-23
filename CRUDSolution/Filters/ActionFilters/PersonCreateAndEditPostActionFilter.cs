using CRUDSolution.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDSolution.Filters.ActionFilters;

public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
{
    private readonly ICountriesService _countriesService;

    public PersonCreateAndEditPostActionFilter(ICountriesService countriesService)
    {
        _countriesService = countriesService;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        //TO DO: before logic
        if (context.Controller is PersonsController personsController)
        {
            if (!personsController.ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                personsController.ViewBag.Countries = countries.Select(temp =>
                    new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

                personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                var personRequest = context.ActionArguments["personRequest"];
                context.Result = personsController.View(personRequest); // Short-circuits or skips the subsequent action filter & action method
            }
            else
            {
                await next(); // Invokes the subsequent filter or action method
            }
            
        }
        else
        {
            await next(); // Calls the subsequent filter or action method
        }
        
        //TO DO: after logic
    }
}