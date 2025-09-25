using FirstWebApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public IActionResult Scheme()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Scheme(ReportDTO reportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return RedirectToAction("FormValid", reportDto);
        }
        
        
          

            
        public IActionResult FormValid(ReportDTO schemeValues)

        {
            return View(schemeValues);
        }
    }
}
