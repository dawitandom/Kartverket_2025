using System;
using System.Collections.Generic;
using System.Linq;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FirstWebApplication.Controllers
{
    public class ReportController : Controller 
    {
        private static readonly List<Report> _store = new(); //Felles, internt minnelager for report

        
        // Returnerer dropdown-valg (SelectListItem) for hindertyper; markerer valgt hvis 'selected' er satt
        private static IEnumerable<SelectListItem> GetObstacleOptions(ObstacleType? selected = null)
        {
            var options = Enum.GetValues(typeof(ObstacleType)) //Array med alle Enum verdiene (Crane, Mast etc)
                .Cast<ObstacleType>() //Gjør om Enum verdiene til ObstacleType
                .Where(t => t != ObstacleType.Unknown) //Fjerner alle ObstacleType.Unknown (0)
                .Select(t => new SelectListItem //Gjør om alle elementer i ObstacleType til SelectListItem objekter
                {
                    Text = t.ToString(), //Hva som vises i dropdown
                    Value = ((int)t).ToString(), //Hva som sendes tilbake ved submit
                    Selected = selected.HasValue && selected.Value == t //Markerer forhåndsvalgt alternativ
                })
                .ToList(); //Gjør om til en liste

            options.Insert(0, new SelectListItem //legger til ett nytt valg først i listen (indeks 0)
            {
                Text = "Select obstacle type…", //hva brukeren ser først i dropdownen
                Value = "", //Tom verdi for placeholderen, postes hvis ikke brukeren velger noe 
                Selected = !selected.HasValue || selected == ObstacleType.Unknown, //Sier at placeholderen skal være default
                Disabled = false //Sier at valget kan brukes å sendes inn 
            });

            return options;
        }

        [HttpGet]
        public IActionResult Scheme() // Viser Scheme siden med tom Report og dropdown valg i ViewBag
                                      
        {
            ViewBag.ObstacleOptions = GetObstacleOptions();
            return View(new Report());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Scheme(Report model) //Validerer skjema (Report), viser view på nytt ved feil, ellers setter ID/tid, lagrer i minne, viser "saved"-melding og redirecter til List 
        {
            ViewBag.ObstacleOptions = GetObstacleOptions(model.Type);

            if (model.Type == ObstacleType.Unknown)
                ModelState.AddModelError(nameof(model.Type), "Please select a valid obstacle type.");

            if (!ModelState.IsValid)
                return View(model);

            model.Id = (_store.Count == 0) ? 1 : _store.Max(r => r.Id) + 1;
            model.CreatedAtUtc = DateTime.UtcNow;

            _store.Add(model);

            TempData["Saved"] = "Report saved.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public IActionResult List() // Viser lagrede rapporter og viser evnt "saved"-melding fra TempData.
        {
            ViewBag.Message = TempData["Saved"] as string;
            var items = _store.OrderByDescending(r => r.CreatedAtUtc).ToList();
            return View(items); 
        }
    }
}
