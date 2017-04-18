using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ADSyncApi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Files()
        {
            return View();
        }

        public ActionResult Error()
        {
            ViewBag.Message = Request.QueryString["message"];
            return View();
        }
    }
}