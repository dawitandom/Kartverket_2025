using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using FirstWebApplication.DataContext;
using Microsoft.Extensions.Logging;

namespace FirstWebApplication.Controllers
{
    /// <summary>
    /// Controller for håndtering av hindring-rapporter (obstacles).
    /// Inneholder separate metoder for Pilot (User) og Admin roller.
    /// Piloter kan legge inn og se egne rapporter.
    /// Admins kan se alle rapporter, sortere, godkjenne og avslå.
    /// </summary>
    [Authorize] // Krever at bruker er innlogget for alle metoder i denne controlleren
    public class ReportController : Controller
    {
        private readonly IReportRepository _reports;      // Repository for database-operasjoner på rapporter
        private readonly ApplicationContext _context;      // Database context for direkte spørringer
        private readonly ILogger<ReportController> _logger; // Logger for feilsøking og hendelser
        
        /// <summary>
        /// Constructor med Dependency Injection av repository, context og logger.
        /// </summary>
        public ReportController(IReportRepository reports, ApplicationContext context, ILogger<ReportController> logger)
        {
            _reports = reports;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Viser skjema for å opprette ny rapport (GET).
        /// Kun tilgjengelig for piloter (User rolle).
        /// Henter liste over hindring-typer fra databasen for dropdown.
        /// </summary>
        /// <returns>Scheme view med tom rapport og hindring-typer</returns>
        [HttpGet]
        [Authorize(Roles = "User")] // Kun piloter kan legge inn rapport
        public IActionResult Scheme()
        {
            // Hent alle hindring-typer sortert etter SortedOrder
            // Konverterer til SelectListItem for dropdown i view
            ViewBag.ObstacleTypes = _context.ObstacleTypes
                .OrderBy(o => o.SortedOrder)
                .Select(o => new SelectListItem {
                    Value = o.ObstacleId,
                    Text = o.ObstacleName
                }).ToList();
            
            return View(new Report());
        }

        /// <summary>
        /// Mottar og lagrer ny rapport fra skjema (POST).
        /// Kun tilgjengelig for piloter (User rolle).
        /// Validerer data, setter automatisk UserId fra innlogget bruker, og lagrer til database.
        /// </summary>
        /// <param name="model">Rapport-objektet fra skjema</param>
        /// <returns>Redirect til MyReports ved suksess, eller tilbake til skjema ved feil</returns>
        [HttpPost]
        [Authorize(Roles = "User")] // Kun piloter kan legge inn rapport
        public async Task<IActionResult> Scheme(Report model)
        {
            // Hent UserId fra innlogget bruker (fra Claims)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;

            // Fjern UserId fra ModelState validation siden vi setter den automatisk
            ModelState.Remove("UserId");

            // Logger for debugging
            _logger.LogInformation("=== SCHEME POST CALLED ===");
            _logger.LogInformation($"UserId: {model.UserId}");
            _logger.LogInformation($"ObstacleId: {model.ObstacleId}");
            _logger.LogInformation($"Description: {model.Description}");
            _logger.LogInformation($"Latitude: {model.Latitude}");
            _logger.LogInformation($"Longitude: {model.Longitude}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            // Sjekk om modellen er gyldig (validering basert på Data Annotations)
            if (!ModelState.IsValid)
            {
                // Logger alle valideringsfeil
                _logger.LogWarning("ModelState is invalid!");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        _logger.LogWarning($"Key: {error.Key}, Error: {err.ErrorMessage}");
                    }
                }
                
                // Hent hindring-typer på nytt for dropdown
                ViewBag.ObstacleTypes = _context.ObstacleTypes
                    .OrderBy(o => o.SortedOrder)
                    .Select(o => new SelectListItem {
                        Value = o.ObstacleId,
                        Text = o.ObstacleName
                    }).ToList();
                
                // Returner til skjema med feilmeldinger
                return View(model);
            }
            
            try
            {
                // Lagre rapporten til database via repository
                _logger.LogInformation("Adding report to database...");
                await _reports.AddAsync(model);
                _logger.LogInformation("Report added successfully!");
                
                // Sett suksessmelding som vises på neste side
                TempData["SuccessMessage"] = "Report created successfully!";
                
                // Redirect til brukerens rapport-liste
                return RedirectToAction(nameof(MyReports));
            }
            catch (Exception ex)
            {
                // Logger feil og vis feilmelding til bruker
                _logger.LogError(ex, "Error adding report");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                
                // Hent hindring-typer på nytt for dropdown
                ViewBag.ObstacleTypes = _context.ObstacleTypes
                    .OrderBy(o => o.SortedOrder)
                    .Select(o => new SelectListItem {
                        Value = o.ObstacleId,
                        Text = o.ObstacleName
                    }).ToList();
                
                return View(model);
            }
        }

        /// <summary>
        /// Viser liste over pilotens egne rapporter (GET).
        /// Kun tilgjengelig for piloter (User rolle).
        /// Filtrerer på UserId slik at pilot kun ser sine egne rapporter.
        /// </summary>
        /// <returns>MyReports view med brukerens rapporter</returns>
        [HttpGet]
        [Authorize(Roles = "User")] // Kun piloter
        public async Task<IActionResult> MyReports()
        {
            // Hent UserId fra innlogget bruker
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Hent alle rapporter fra database
            var items = await _reports.GetAllAsync();
            
            // Filtrer kun rapporter som tilhører denne brukeren
            var myReports = items.Where(r => r.UserId == userId).ToList();
            
            return View(myReports);
        }

        /// <summary>
        /// Viser liste over ventende (pending) rapporter som trenger godkjenning (GET).
        /// Kun tilgjengelig for admins.
        /// Viser kun rapporter med status "Pending".
        /// Støtter sortering etter dato, bruker eller hindring-type.
        /// </summary>
        /// <param name="sortBy">Sorteringskriterium: "date" (default), "user" eller "obstacle"</param>
        /// <returns>PendingReports view med sorterte pending rapporter</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")] // Kun admins
        public async Task<IActionResult> PendingReports(string sortBy = "date")
        {
            // Hent alle rapporter fra database
            var items = await _reports.GetAllAsync();
            
            // Filtrer kun rapporter med status "Pending"
            var pending = items.Where(r => r.Status == "Pending").ToList();
            
            // Sorter basert på parameter
            pending = sortBy switch
            {
                "user" => pending.OrderBy(r => r.UserId).ToList(),
                "obstacle" => pending.OrderBy(r => r.ObstacleId).ToList(),
                _ => pending.OrderByDescending(r => r.DateTime).ToList() // Default: nyeste først
            };

            // Send sorteringskriterium til view for å markere aktiv sortering
            ViewBag.SortBy = sortBy;
            return View(pending);
        }

        /// <summary>
        /// Viser liste over godkjente og avslåtte rapporter (GET).
        /// Kun tilgjengelig for admins.
        /// Støtter filtrering på status (all, approved, rejected).
        /// Støtter sortering etter dato, bruker, hindring eller status.
        /// </summary>
        /// <param name="filterBy">Filtreringskriterium: "all" (default), "approved" eller "rejected"</param>
        /// <param name="sortBy">Sorteringskriterium: "date" (default), "user", "obstacle" eller "status"</param>
        /// <returns>ReviewedReports view med filtrerte og sorterte rapporter</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")] // Kun admins
        public async Task<IActionResult> ReviewedReports(string filterBy = "all", string sortBy = "date")
        {
            // Hent alle rapporter fra database
            var items = await _reports.GetAllAsync();
            
            // Filtrer kun rapporter som er godkjent eller avslått (ikke pending)
            var reviewed = items.Where(r => r.Status == "Approved" || r.Status == "Rejected").ToList();
            
            // Filtrer ytterligere basert på status
            reviewed = filterBy switch
            {
                "approved" => reviewed.Where(r => r.Status == "Approved").ToList(),
                "rejected" => reviewed.Where(r => r.Status == "Rejected").ToList(),
                _ => reviewed // Vis alle (både approved og rejected)
            };
            
            // Sorter basert på parameter
            reviewed = sortBy switch
            {
                "user" => reviewed.OrderBy(r => r.UserId).ToList(),
                "obstacle" => reviewed.OrderBy(r => r.ObstacleId).ToList(),
                "status" => reviewed.OrderBy(r => r.Status).ToList(),
                _ => reviewed.OrderByDescending(r => r.DateTime).ToList() // Default: nyeste først
            };

            // Send filter- og sorteringskriterier til view
            ViewBag.FilterBy = filterBy;
            ViewBag.SortBy = sortBy;
            return View(reviewed);
        }

        /// <summary>
        /// Godkjenner en rapport (POST).
        /// Kun tilgjengelig for admins.
        /// Setter status til "Approved" og lagrer endringen i databasen.
        /// </summary>
        /// <param name="reportId">ID på rapporten som skal godkjennes</param>
        /// <returns>Redirect til PendingReports med suksessmelding</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Kun admins
        public async Task<IActionResult> Approve(string reportId)
        {
            // Finn rapporten i databasen
            var report = (await _reports.GetAllAsync()).FirstOrDefault(r => r.ReportId == reportId);
            
            if (report != null)
            {
                // Endre status til Approved
                report.Status = "Approved";
                
                // Lagre endringen i databasen
                await _reports.UpdateAsync(report);
                
                // Sett suksessmelding
                TempData["SuccessMessage"] = "Report approved!";
            }
            
            // Redirect tilbake til pending rapporter
            return RedirectToAction(nameof(PendingReports));
        }

        /// <summary>
        /// Avslår en rapport (POST).
        /// Kun tilgjengelig for admins.
        /// Setter status til "Rejected" og lagrer endringen i databasen.
        /// </summary>
        /// <param name="reportId">ID på rapporten som skal avslås</param>
        /// <returns>Redirect til PendingReports med suksessmelding</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Kun admins
        public async Task<IActionResult> Reject(string reportId)
        {
            // Finn rapporten i databasen
            var report = (await _reports.GetAllAsync()).FirstOrDefault(r => r.ReportId == reportId);
            
            if (report != null)
            {
                // Endre status til Rejected
                report.Status = "Rejected";
                
                // Lagre endringen i databasen
                await _reports.UpdateAsync(report);
                
                // Sett suksessmelding
                TempData["SuccessMessage"] = "Report rejected!";
            }
            
            // Redirect tilbake til pending rapporter
            return RedirectToAction(nameof(PendingReports));
        }
    }
}