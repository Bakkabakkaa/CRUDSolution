using Microsoft.AspNetCore.Mvc;

namespace CRUDSolution.Controllers;

public class HomeController : Controller
{
    [Route("Error")]
    public IActionResult Index()
    {
        return View();
    }
}