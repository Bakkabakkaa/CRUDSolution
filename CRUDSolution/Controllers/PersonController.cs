using Microsoft.AspNetCore.Mvc;

namespace CRUDSolution.Controllers;

public class PersonController : Controller
{
    [Route("person/index")]
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }
}