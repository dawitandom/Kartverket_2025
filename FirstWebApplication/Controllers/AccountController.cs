using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel; // for RegisterViewModel
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers;

/// <summary>
/// Controller for autentisering (innlogging, utlogging) og selvbetjent registrering.
/// Bruker ASP.NET Core Identity for brukerhåndtering og innlogging.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Oppretter en ny instans av AccountController med de angitte tjenestene.
    /// </summary>
    /// <param name="userManager">UserManager for å administrere brukere</param>
    /// <param name="signInManager">SignInManager for å håndtere innlogging og utlogging</param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ---------- LOGIN ----------

    /// <summary>
    /// Viser innloggingssiden. Alle brukere, også ikke-innloggede, kan nå denne siden.
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // ikke-innloggede må kunne nå login
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Håndterer innloggingsforsøk. Validerer brukernavn og passord, og logger inn brukeren hvis legitimasjonen er korrekt.
    /// Hvis innloggingen lykkes, sendes brukeren til hjemmesiden. Hvis ikke, vises en feilmelding.
    /// </summary>
    /// <param name="username">Brukernavnet som skal brukes for innlogging</param>
    /// <param name="password">Passordet som skal brukes for innlogging</param>
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

    /// <summary>
    /// Viser registreringssiden hvor nye brukere kan opprette en konto. Alle kan nå denne siden uten å være innlogget.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    /// <summary>
    /// Oppretter en ny bruker med rollen DefaultUser og logger dem inn automatisk.
    /// Sjekker at brukernavn og e-post ikke allerede er i bruk før opprettelsen.
    /// Hvis registreringen lykkes, sendes brukeren til hjemmesiden eller til returnUrl hvis den er angitt.
    /// </summary>
    /// <param name="model">Modellen som inneholder brukerens registreringsinformasjon (brukernavn, e-post, passord, navn)</param>
    /// <param name="returnUrl">Valgfri URL som brukeren skal sendes til etter vellykket registrering</param>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Forhåndssjekk brukernavn/e-post for å gi feltspesifikke feilmeldinger
        var existingByName = await _userManager.FindByNameAsync(model.UserName);
        if (existingByName != null)
        {
            ModelState.AddModelError(nameof(model.UserName), "Username is already taken.");
            return View(model);
        }

        var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingByEmail != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var e in createResult.Errors)
            {
                // Kartlegg Identity-duplikatfeil til feltfeil når mulig
                if (!string.IsNullOrEmpty(e.Code) && e.Code.Contains("DuplicateUserName", System.StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(model.UserName), "Username is already taken.");
                else if (!string.IsNullOrEmpty(e.Code) && e.Code.Contains("DuplicateEmail", System.StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                else
                    ModelState.AddModelError(string.Empty, e.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "DefaultUser");
        await _signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ---------- LOGOUT ----------

    /// <summary>
    /// Logger ut den innloggede brukeren og sender dem tilbake til innloggingssiden.
    /// Kun innloggede brukere kan kalle denne metoden.
    /// </summary>
    [HttpPost]
    [Authorize] // kun innloggede kan logge ut
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
