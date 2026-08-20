using AQMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AQMS.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //Impressum und Datenschutzerklärung müssen ohne Anmeldung erreichbar sein.
        //Der HomeController trägt bewusst kein [Authorize] - anders als der
        //DashboardController - damit beide Seiten öffentlich abrufbar bleiben.
        public IActionResult Impressum()
        {
            return View();
        }

        public IActionResult Datenschutz()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
