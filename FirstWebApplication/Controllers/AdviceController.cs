using AspNetCoreGeneratedDocument;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication.Controllers
{
    public class AdviceController : Controller
    {
        
        private readonly IAdviceRepository _adviceRepository; //dette er et repository som brukes til å hente data fra databasen
        public AdviceController(IAdviceRepository iAdviceRepository)
            {
            _adviceRepository = iAdviceRepository; //setter repositoryet til det som blir sendt inn i konstruktøren
        }

        [HttpGet]
        public async Task<IActionResult> ShowAdvice()
        {
            return View(); 
        }

        [HttpPost]
        public async Task<ActionResult> ShowAdvice(ViewAdviceModel requestData)
        {
            AdviceDto adviceDto = new AdviceDto //lager et nytt adviceDto objekt som skal sendes til databasen
            {
                Title = requestData.ViewTitle,
                Description = requestData.ViewDescription
            };

            var sendToDataBase = await _adviceRepository.AddAdvice(adviceDto); //sender adviceDto til databasen via repositoryet
            return View(requestData); //returnerer det som ble lagt til i databasen
        }

    }
}
