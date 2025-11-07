using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel; // <-- for RegisterViewModel
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for authentication (login, logout) + self-service registration (sign up).
/// Uses ASP.NET Core Identity.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ---------- LOGIN ----------

    /// <summary>Shows the login page.</summary>
    [HttpGet]
    [AllowAnonymous] // ikke-innloggede må kunne nå login
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>Handles login submit.</summary>
    [HttpPost]
    [AllowAnonymous] // ikke-innloggede må kunne poste login
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and password are required";
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(
            username,
            password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        if (result.IsLockedOut)
        {
            ViewBag.Error = "Account is locked out. Please try again later.";
            return View();
        }

        ViewBag.Error = "Invalid username or password";
        return View();
    }

    // ---------- REGISTER (SIGN UP) ----------

    /// <summary>Shows the self-service registration page.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    /// <summary>Creates a new user as DefaultUser, then signs them in.</summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Opprett ny bruker
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            EmailConfirmed = true,     // forenkling i prosjektet
            FirstName = model.FirstName,
            LastName  = model.LastName
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var e in createResult.Errors)
                ModelState.AddModelError("", e.Description);
            return View(model);
        }

        // Gi standardrollen for selv-registrerte
        await _userManager.AddToRoleAsync(user, "DefaultUser");

        // Logg inn og send rett til å opprette rapport
        await _signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Scheme", "Report");
    }

    // ---------- LOGOUT ----------

    /// <summary>Logs the user out.</summary>
    [HttpPost]
    [Authorize] // kun innloggede kan logge ut
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
