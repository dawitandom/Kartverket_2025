using Microsoft.AspNetCore.Mvc; //MVC ting (views, controllers etc) 
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace FirstWebApplication.Controllers
{
    public class HomeController : Controller //Lager en klasse som heter homecontroller som arver fra bqisklassen controller
    {
        private readonly string _connectionString; //For å lagre connectionstring

        public HomeController(IConfiguration configuration) //Konstrukstør som tar inn ferdige verdier 
        {

        }
        
        [HttpGet] //Svar på get-forespørsel
        public IActionResult Index() //Action for /Home/Index, viser Index.cshtml
        {
            return View(); // Render view uten modell
        }

        public IActionResult Privacy() // Action for /Home/Privacy, viser Privacy.cshtml
        {
            return View(); //Render view uten modell
        }
        
    }
}
