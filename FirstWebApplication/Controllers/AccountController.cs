using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FirstWebApplication.Models;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for authentication and user login.
/// Uses ASP.NET Core Identity instead of cookie-based authentication.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Constructor that injects UserManager and SignInManager via Dependency Injection.
    /// </summary>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Displays the login page (GET request).
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Handles login (POST request).
    /// Uses Identity's SignInManager to authenticate the user.
    /// </summary>
    /// <param name="username">Username from login form</param>
    /// <param name="password">Password from login form</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and password are required";
            return View();
        }

        // Use Identity's SignInManager to log in
        // isPersistent = true means the cookie survives browser closing
        var result = await _signInManager.PasswordSignInAsync(
            username, 
            password, 
            isPersistent: true, 
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Successful login - redirect to home page
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ViewBag.Error = "Account is locked out. Please try again later.";
            return View();
        }

        // Invalid username or password
        ViewBag.Error = "Invalid username or password";
        return View();
    }

    /// <summary>
    /// Handles logout (POST request).
    /// Uses Identity's SignInManager to log out.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
