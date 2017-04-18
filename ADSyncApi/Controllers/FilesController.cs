using ADSync.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADSyncApi.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        // GET: Files
        public async Task<ActionResult> Index()
        {
            var sites = await RemoteSite.GetAllSites();
            return View(sites);
        }
    }
}