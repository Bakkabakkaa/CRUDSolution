using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRUDSolution.Controllers;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO registerDto)
    {
        // Check for validation errors
        if (ModelState.IsValid == false)
        {
            ViewBag.Errors = ModelState.Values
                .SelectMany(temp => temp.Errors)
                .Select(temp => temp.ErrorMessage);

            return View(registerDto);
        }

        ApplicationUser user = new ApplicationUser()
        {
            Email = registerDto.Email, PhoneNumber = registerDto.Phone,
            UserName = registerDto.Email, PersonName = registerDto.PersonName
        };

        IdentityResult result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }
        else
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("Register", error.Description);
            }
        }

        return View(registerDto);
        // TO DO: Store user registration details into Identity database
    }
}