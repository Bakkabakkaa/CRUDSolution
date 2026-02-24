using CRUDSolution.Filters;
using CRUDSolution.Filters.ActionFilters;
using CRUDSolution.Filters.AuthorizationFilter;
using CRUDSolution.Filters.ExceptionFilters;
using CRUDSolution.Filters.ResourceFilters;
using CRUDSolution.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDSolution.Controllers;

[Route("persons")]
[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[]
{
    "My-Key-From-Controller", "My-Value-From-Controller", 3
}, Order = 3)]
[TypeFilter(typeof(HandleExceptionFilter))]
[TypeFilter(typeof(PersonsAlwaysRunResultFilter))]
public class PersonsController : Controller
{
    private readonly IPersonsService _personsService;
    private readonly ICountriesService _countriesService;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
    {
        _personsService = personsService;
        _countriesService = countriesService;
        _logger = logger;
    }
    
    [HttpGet]
    [Route("index")]
    [Route("/")]
    [ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[]
    {
        "My-Key-From-Action", "My-Value-From-Controller", 1
    }, Order = 1)]
    [TypeFilter(typeof(PersonsListResultFilter))]
    [SkipFilter]
    public async Task<IActionResult> Index(string searchBy, string? searchString,
        string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
    {
        _logger.LogInformation("Index action method of PersonsController");
        _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder} ");
        
        List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);
        
        // Sort
        List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, sortOrder);
        
        return View(sortedPersons);
    }

    // Executes when the user clicks on "Crete Person" hyperlink (while opening the create view)
    [HttpGet]
    [Route("create")]
    [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[]
    {
        "my-key", "my-value", 4
    })]
    public async Task<IActionResult> Create()
    {
        List<CountryResponse> countries = await _countriesService.GetAllCountries();
        ViewBag.Countries = countries.Select(temp => new SelectListItem()
        {
            Text = temp.CountryName, Value = temp.CountryID.ToString()
        });
        
        return View();
    }

    [HttpPost]
    [Route("create")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false })]
    public async Task<IActionResult> Create(PersonAddRequest personRequest)
    {
        // Call the service method
        PersonResponse personResponse = await _personsService.AddPerson(personRequest);
        
        // Navigation to Index() action method (it makes another get request to "persons/index")
        return RedirectToAction("Index", "Persons");
    }

    [HttpGet]
    [Route("[action]/{personID}")]
    [TypeFilter(typeof(TokenResultFilter))]
    public async Task<IActionResult> Edit(Guid personID)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

        if (personResponse == null)
        {
            return RedirectToAction("Index");
        }

        PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
        
        List<CountryResponse> countries = await _countriesService.GetAllCountries();
        ViewBag.Countries = countries.Select(temp => new SelectListItem()
        {
            Text = temp.CountryName, Value = temp.CountryID.ToString()
        });
        
        return View(personUpdateRequest);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(TokenAuthorizationFilter))]
    public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personRequest.PersonID);

        if (personResponse == null)
        {
            return RedirectToAction("Index");
        }
        
        PersonResponse updatePerson = await _personsService.UpdatePerson(personRequest);
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route(("[action]/{personID}"))]
    public async Task<IActionResult> Delete(Guid personID)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

        if (personResponse == null)
            return RedirectToAction("Index");

        return View(personResponse);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
    {
        PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

        if (personResponse == null)
        {
            return RedirectToAction("Index");
        }

        await _personsService.DeletePerson(personUpdateRequest.PersonID);
        
        return RedirectToAction("Index");
    }

    [Route("PersonsPDF")]
    public async Task<IActionResult> PersonsPDF()
    {
        // Get list of persons
        List<PersonResponse>  persons = await _personsService.GetAllPersons();
        
        // Return view as pdf
        return new ViewAsPdf("PersonsPDF", persons, ViewData)
        {
            PageMargins = new Rotativa.AspNetCore.Options.Margins()
            {
                Top = 20, Right = 20, Bottom = 20, Left = 20
            },
            PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
        };
    }

    [Route("PersonsCSV")]
    public async Task<IActionResult> PersonsCSV()
    {
        MemoryStream memoryStream = await _personsService.GetPersonsCSV();

        return File(memoryStream, "application/octet-stream", "persons.csv");
    }

    [Route("PersonsExcel")]
    public async Task<IActionResult> PersonsExcel()
    {
        MemoryStream memoryStream = await _personsService.GetPersonsExcel();

        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
    }
}