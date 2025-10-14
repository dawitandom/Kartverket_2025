using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for hjemmesiden til Kartverket Obstacle Reporting System.
    /// Håndterer visning av forside med roller-basert innhold.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Viser forsiden til applikasjonen.
        /// Hvis bruker er innlogget: viser relevante knapper basert på rolle (Pilot eller Admin)
        /// Hvis bruker ikke er innlogget: viser innloggingskortet
        /// </summary>
        /// <returns>Index view med rolle-spesifikt innhold</returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}