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
        public async Task<ActionResult> Index()
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

        public void GetSetupZip(string siteId)
        {
            RemoteSite site = null;

            var task = Task.Run(async () => {
                site = await RemoteSiteUtil.GetSite(siteId);
            });
            task.Wait();
            var path = Request.MapPath("/Files/");

            //Convert the memorystream to an array of bytes.

            var ApiUrl = string.Format("{0}://{1}/", Request.Url.Scheme, Request.Url.Authority);
            var zip = ZipCopy.SetupZip(path, site, ApiUrl);

            var filename = string.Format("{0}_setup.zip", site.OnPremDomainName.Replace(".", "_"));

            // Clear all content output from the buffer stream
            Response.Clear();

            // Add a HTTP header to the output stream that specifies the default filename
            // for the browser's download dialog
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);

            // Add a HTTP header to the output stream that contains the 
            // content length(File Size). This lets the browser know how much data is being transfered
            Response.AddHeader("Content-Length", zip.Length.ToString());

            // Set the HTTP MIME type of the output stream
            Response.ContentType = "application/octet-stream";
            
            // Write the data out to the client.
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