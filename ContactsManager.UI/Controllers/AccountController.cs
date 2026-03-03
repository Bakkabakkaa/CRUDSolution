using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRUDSolution.Controllers;

[Route("[controller]/[action]")]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        IdentityResult result = await _userManager.CreateAsync(user, registerDto.Password);
        
        if (result.Succeeded)
        {
            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);
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

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO loginDto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Errors = ModelState.Values
                .SelectMany(temp => temp.Errors)
                .Select(temp => temp.ErrorMessage);
            
            return View(loginDto);
        }

        var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
            isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToAction(nameof(PersonsController.Index), "Persons");
        }
        
        ModelState.AddModelError("Login", "Invalid email or password");
        return View(loginDto);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        
        return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }
}