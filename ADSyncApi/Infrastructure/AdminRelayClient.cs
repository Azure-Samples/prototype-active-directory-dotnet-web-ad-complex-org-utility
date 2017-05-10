using ADSync.Common.Models;
using ADSync.Data;
using Common;
using Infrastructure;
using OrgRelay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ADSync.Common.Enums;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ADSyncApi.Infrastructure
{
    public static class AdminRelayClient
    {
        public static async Task<RelayResponse> GetUserStatus(StagedUser user)
        {
            try
            {
                var client = new SigRClient(Settings.AdminSiteUrl, Settings.STSApiKey, "SiteHub");

                var msg = new RelayMessage
                {
                    ApiKey = Settings.AdminApiKey,
                    Data = user.Upn,
                    DestSiteId = user.SiteId,
                    Identifier = user.Upn,
                    Operation = SiteOperation.GetUserStatus
                };

                await client.StartAsync();
                RelayResponse res = await client.ProcessRelayMessage(msg);

                return res;
            }
            catch (Exception ex)
            {
                Utils.AddLogEntry("Error getting user status", EventLogEntryType.Error, 0, ex);
                return new RelayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public static async Task<RelayResponse> SetUserStatus(ADUser user, StagedUser stagedUser)
        {
            try
            {
                var client = new SigRClient(Settings.AdminSiteUrl, Settings.STSApiKey, "SiteHub");
                var data = JsonConvert.SerializeObject(user);

                var msg = new RelayMessage
                {
                    ApiKey = Settings.AdminApiKey,
                    Data = data,
                    DestSiteId = stagedUser.SiteId,
                    Identifier = stagedUser.Upn,
                    Operation = SiteOperation.SetUserStatus
                };

                await client.StartAsync();
                RelayResponse res = await client.ProcessRelayMessage(msg);

                return res;
            }
            catch (Exception ex)
            {
                Utils.AddLogEntry("Error setting user status", EventLogEntryType.Error, 0, ex);
                return new RelayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        public static async Task<RelayResponse> ResetPassword(PasswordReset credential, StagedUser stagedUser)
        {
            try
            {
                var client = new SigRClient(Settings.AdminSiteUrl, Settings.STSApiKey, "SiteHub");
                var data = JsonConvert.SerializeObject(credential);

                var msg = new RelayMessage
                {
                    ApiKey = Settings.AdminApiKey,
                    Data = data,
                    DestSiteId = stagedUser.SiteId,
                    Identifier = stagedUser.Id,
                    Operation = SiteOperation.ResetPW
                };

                await client.StartAsync();
                RelayResponse res = await client.ProcessRelayMessage(msg);

                return res;
            }
            catch (Exception ex)
            {
                Utils.AddLogEntry("Error setting user status", EventLogEntryType.Error, 0, ex);
                return new RelayResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}