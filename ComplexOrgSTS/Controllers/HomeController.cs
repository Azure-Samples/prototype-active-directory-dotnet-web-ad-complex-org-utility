using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ComplexOrgSTS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.StatusDescription = HttpStatusCode.NotFound.ToString();
            return null;
        }
    }
}