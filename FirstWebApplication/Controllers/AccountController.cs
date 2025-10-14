using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using FirstWebApplication.DataContext;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for autentisering og brukerinnlogging.
    /// Håndterer innlogging, utlogging og cookie-basert autentisering.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationContext _context;

        /// <summary>
        /// Constructor som injiserer database context via Dependency Injection.
        /// </summary>
        /// <param name="context">Database context for å hente brukerdata</param>
        public AccountController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Viser innloggingssiden (GET request).
        /// </summary>
        /// <returns>Login view</returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Håndterer innlogging (POST request).
        /// Validerer brukernavn og passord mot databasen.
        /// Ved suksess: oppretter cookie-basert autentisering med brukerens claims (brukernavn, rolle, ID).
        /// Ved feil: viser feilmelding på innloggingssiden.
        /// </summary>
        /// <param name="username">Brukernavn fra innloggingsskjema</param>
        /// <param name="password">Passord fra innloggingsskjema</param>
        /// <returns>Redirect til forsiden ved suksess, eller Login view med feilmelding</returns>
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Søk etter bruker i databasen basert på brukernavn og passord
            // Trim() fjerner mellomrom siden passord er lagret som char(60)
            var user = _context.Users
                .Where(u => u.Username == username && u.Password.Trim() == password.Trim())
                .Select(u => new { u.UserId, u.Username, u.FirstName, u.LastName, u.UserRoleId })
                .FirstOrDefault();

            // Hvis bruker ikke finnes eller passord er feil
            if (user == null)
            {
                ViewBag.Error = "Feil brukernavn eller passord";
                return View();
            }

            // Hent brukerens rolle (Admin eller User/Pilot)
            var roleName = _context.UserRoles
                .Where(r => r.UserRoleId == user.UserRoleId)
                .Select(r => r.Role)
                .FirstOrDefault();

            // Opprett claims (brukerinformasjon som lagres i cookie)
            // Claims brukes til å identifisere brukeren i hele applikasjonen
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),              // Brukernavn
                new Claim(ClaimTypes.NameIdentifier, user.UserId),      // Bruker ID
                new Claim("FullName", $"{user.FirstName} {user.LastName}"), // Fullt navn
                new Claim(ClaimTypes.Role, roleName ?? "User")          // Rolle (Admin eller User)
            };

            // Opprett identity med claims
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Konfigurer autentiserings-properties
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // Cookie overlever browser-lukking (husk innlogging)
            };

            // Logg inn brukeren ved å signere cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect til forsiden etter vellykket innlogging
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Håndterer utlogging (POST request).
        /// Sletter autentiserings-cookie og redirecter til innloggingssiden.
        /// </summary>
        /// <returns>Redirect til Login siden</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Slett autentiserings-cookie (logg ut bruker)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Redirect til innloggingssiden
            return RedirectToAction("Login");
        }
    }
}