using ADSync.Data.Models;
using Common;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADSyncApi.Controllers
{
    [Authorize]
    public class RemoteSiteController : Controller
    {
        // GET: RemoteSite
        public async Task<ActionResult> Index()
        {
            var res = await RemoteSite.GetAllSites();
            return View(res);
        }

        // GET: RemoteSite/Create
        public ActionResult Create()
        {
            return View("Edit");
        }

        // GET: RemoteSite/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            RemoteSite site;

            if (id == null)
            {
                site = new RemoteSite();
            } else
            {
                site = await RemoteSite.GetSite(id);
            }
            return View(site);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RemoteSiteModel site)
        {
            try
            {
                site.SiteDomains = site.SiteDomainsList.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();

                if (site.Id == null)
                {
                    //creating
                    var res = await RemoteSite.AddSite(site);
                }
                else
                {
                    if (site.ResetApiKey)
                    {
                        site.ApiKey = Utils.GenApiKey();
                    }
                    // TODO: Add update logic here
                    var res = await RemoteSite.UpdateSite(site);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(RemoteSiteModel site)
        {
            try
            {
                // TODO: Add delete logic here
                var res = await RemoteSite.DeleteSite(site.Id);
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                Logging.WriteToAppLog("Error deleting remote site", System.Diagnostics.EventLogEntryType.Error, ex);
                ViewBag.Error = "An error occured trying to delete the record, please check the error logs and try again.";
                return View("Edit", site);
            }
        }
    }
    public class RemoteSiteModel : RemoteSite
    {
        public bool ResetApiKey { get; set; }
        public string SiteDomainsList { get; set; }
    }
}
