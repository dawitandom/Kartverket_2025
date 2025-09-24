using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login() => Content("Login-skjerm kommer senere 🙂");

    [HttpPost]
    public IActionResult Logout()
    {
        return RedirectToAction("Index", "Home");
    }
}