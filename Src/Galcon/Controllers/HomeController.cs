using System.Web.Mvc;


namespace Galcon.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Hub");
        }


        public ActionResult Hub()
        {
            return View();
        }
    }
}