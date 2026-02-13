using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CRUDSolution.Controllers;

[Route("[controller]")]
public class CountriesController : Controller
{
    [Route("UploadFromExcel")]
    public IActionResult UploadFromExcel()
    {
        return View();
    }
}