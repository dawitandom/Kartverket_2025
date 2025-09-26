using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ReportController : Controller
    {
        // GET: show the form
        [HttpGet]
        public IActionResult Scheme() => View(new ReportDto());

        // POST: receive the form and show a confirmation page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Scheme(ReportDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // No persistence — just show the confirmation page with submitted data
            return View("FormValid", model);
        }

        // Optional: direct route to the confirmation view
        [HttpGet]
        public IActionResult FormValid(ReportDto model) => View(model);
    }
}