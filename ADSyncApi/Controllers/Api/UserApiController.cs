using ADSync.Common.Models;
using ADSync.Data.Models;
using ADSyncApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    [Authorize]
    public class UserApiController : ApiController
    {
        [HttpGet]
        public async Task<IEnumerable<StagedUser>> GetUsersBySite(string siteId)
        {
            var res = await StagedUserUtil.GetAllBySiteId(siteId);
            return res.OrderBy(u => u.Surname).OrderBy(u => u.GivenName).ToList();
        }

        [HttpGet]
        public async Task<dynamic> GetUserStatus(string userId)
        {
            var user = await StagedUserUtil.GetUser(userId);
            var response = await AdminRelayClient.GetUserStatus(user);
            return response;
        }

        [HttpPost]
        public async Task<dynamic> SetUserStatus(SetUserObject user)
        {
            var stagedUser = await StagedUserUtil.GetUser(user.userId);
            var response = await AdminRelayClient.SetUserStatus(user, stagedUser);
            return response;
        }
        [HttpPost]
        public async Task<dynamic> ResetPw(ResetPwObject reset)
        {
            var userId = reset.userId;
            var rst = new PasswordReset
            {
                NewPassword = reset.NewPassword,
                SetChangePasswordAtNextLogon = reset.SetChangePasswordAtNextLogon,
                Unlock = reset.Unlock,
                UserName = reset.UserName
            };
            var stagedUser = await StagedUserUtil.GetUser(reset.userId);
            var response = await AdminRelayClient.ResetPassword(rst, stagedUser);
            return response;
        }

        [HttpGet]
        public async Task<IEnumerable<StagedUser>> GetFilteredUsersBySite(string siteId, string filter)
        {
            var res = await StagedUserUtil.GetFilteredBySiteId(siteId, filter);
            return res.OrderBy(u => u.Surname).OrderBy(u => u.GivenName).ToList();
        }

        [HttpGet]
        public async Task<IEnumerable<StagedUser>> GetUsersByDomain(string domain)
        {
            var res = await StagedUserUtil.GetAllByDomain(domain);
            return res.OrderBy(u => u.Surname).OrderBy(u => u.GivenName).ToList();
        }
    }
    public class SetUserObject: ADUser
    {
        public string userId { get; set; }
    }
    public class ResetPwObject: PasswordReset
    {
        public string userId { get; set; }
    }
}
