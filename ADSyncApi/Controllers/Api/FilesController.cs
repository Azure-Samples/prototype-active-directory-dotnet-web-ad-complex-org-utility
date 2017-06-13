using ADSyncApi.Infrastructure;
using Common;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;

namespace ADSyncApi.Controllers.Api
{
    [ApiAuth]
    public class FilesController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> GetSiteServiceZip()
        {
            var siteId = User.Identity.GetClaim(CustomClaimTypes.SiteId);
            var pathToFiles = System.Web.Hosting.HostingEnvironment.MapPath("/Files/");
            var apiUrl = string.Format("{0}://{1}/", Request.RequestUri.Scheme, Request.RequestUri.Authority);
            var zip = await ZipCopy.GetSetupZip(siteId, pathToFiles, apiUrl);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(zip.ToArray());
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            var siteName = User.Identity.GetClaim(CustomClaimTypes.OnPremDomainName);
            var filename = string.Format("{0}_setup.zip", siteName.Replace(".", "_"));

            result.Content.Headers.ContentDisposition.FileName = filename;
            return result;
        }

        [HttpGet]
        public string GetSiteServiceVersion()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath("/Files/ScriptVersion.txt");
            var res = File.ReadAllText(path);
            return res;
        }
    }
}
