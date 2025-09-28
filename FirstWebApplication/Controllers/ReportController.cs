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
        private static readonly List<Report> _store = new();

        private static IEnumerable<SelectListItem> GetObstacleOptions(ObstacleType? selected = null)
        {
            var options = Enum.GetValues(typeof(ObstacleType))
                .Cast<ObstacleType>()
                .Where(t => t != ObstacleType.Unknown)
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString(),
                    Selected = selected.HasValue && selected.Value == t
                })
                .ToList();

            options.Insert(0, new SelectListItem
            {
                Text = "Select obstacle type…",
                Value = "",
                Selected = !selected.HasValue || selected == ObstacleType.Unknown,
                Disabled = false
            });

            return options;
        }

        [HttpGet]
        public IActionResult Scheme()
        {
            ViewBag.ObstacleOptions = GetObstacleOptions();
            return View(new Report());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Scheme(Report model)
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
        public IActionResult List()
        {
            ViewBag.Message = TempData["Saved"] as string;
            var items = _store.OrderByDescending(r => r.CreatedAtUtc).ToList();
            return View(items); 
        }
    }
}
