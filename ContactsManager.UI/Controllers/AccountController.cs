using ContactsManager.Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace CRUDSolution.Controllers;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterDTO registerDto)
    {
        return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }
    
}