using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser") return RedirectToAction("Login", "Account");
            return View();
        }
    }
}