using System.Web.Mvc;

namespace InRule.RuleServices.ASPMVCTestUI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Home Page";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
