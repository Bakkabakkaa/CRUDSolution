using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDSolution.Controllers;

public class PersonController : Controller
{
    private readonly IPersonsService _personsService;

    public PersonController(IPersonsService personsService)
    {
        _personsService = personsService;
    }
    [Route("person/index")]
    [Route("/")]
    public IActionResult Index()
    {
        List<PersonResponse> persons = _personsService.GetAllPersons();
        
        return View(persons);
    }
}