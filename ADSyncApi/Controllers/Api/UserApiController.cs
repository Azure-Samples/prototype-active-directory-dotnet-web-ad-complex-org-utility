using ADSync.Common.Models;
using ADSync.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    public class UserApiController : ApiController
    {
        [HttpGet]
        public async Task<IEnumerable<StagedUser>> GetUsersBySite(string siteId)
        {
            var res = await StagedUserUtil.GetAllBySiteId(siteId);
            return res.OrderBy(u => u.Surname).OrderBy(u => u.GivenName).ToList();
        }
        [HttpGet]
        public async Task<IEnumerable<StagedUser>> GetUsersByDomain(string domain)
        {
            var res = await StagedUserUtil.GetAllByDomain(domain);
            return res.OrderBy(u => u.Surname).OrderBy(u => u.GivenName).ToList();
        }
    }
}
