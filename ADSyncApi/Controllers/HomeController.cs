using ADSync.Common.Models;
using ADSync.Data.Models;
using ADSyncApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ADSync.Common.Enums;
using Common;

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

        public async Task GetSetupZip(string siteId)
        {
            var path = Request.MapPath("/Files/");
            var ApiUrl = string.Format("{0}://{1}/", Request.Url.Scheme, Request.Url.Authority);
            var site = await RemoteSiteUtil.GetSite(siteId);
            var filename = string.Format("{0}_setup.zip", site.OnPremDomainName.Replace(".", "_"));

            var zip = ZipCopy.SetupZip(path, site, ApiUrl);

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
            Response.AddHeader("Content-Length", zip.Length.ToString());
            Response.ContentType = "application/octet-stream";
            Response.BinaryWrite(zip.ToArray());
        }

        public ActionResult Error()
        {
            ViewBag.Message = Request.QueryString["message"];
            return View();
        }
        [AllowAnonymous]
        public ActionResult Chat()
        {
            return View();
        }
    }
}