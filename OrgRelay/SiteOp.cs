using ADSync.Common.Enums;
using ADSync.Common.Models;
using System;
using System.DirectoryServices.AccountManagement;
using Newtonsoft.Json;
using System.Diagnostics;
using Common;
using System.IO;
using System.Reflection;

namespace OrgRelay
{
    public static class SiteOp
    {
        private const int ERROR_LOGON_FAILURE = 0x31;

        public static RelayResponseInternal ProcessMessage(RelayMessage message)
        {
            RelayResponse r;
            RelayResponseInternal res = new RelayResponseInternal
            {
                OriginSiteId = message.OriginSiteId,
                OriginConnectionId = message.OriginConnectionId,
                RespondingSiteId = message.DestSiteId,
                Identifier = message.Identifier,
                Operation = message.Operation
            };

            try
            {
                switch (message.Operation)
                {
                    case SiteOperation.ResetPW:
                        r = ADTools.ResetPassword(message);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        LogPWReset(res);
                        return res;

                    case SiteOperation.TriggerPoll:
                        r = ActivatePoll(message);
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        res.Operation = SiteOperation.TriggerPoll;
                        return res;

                    case SiteOperation.DisableUser:
                        r = ADTools.EnableDisableUser(message.Data, enabled: false);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        return res;

                    case SiteOperation.EnableUser:
                        r = ADTools.EnableDisableUser(message.Data, enabled: true);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        return res;

                    case SiteOperation.GetUserStatus:
                        r = ADTools.GetUserStatus(message.Data);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        return res;

                    case SiteOperation.SetUserStatus:
                        var user = Utils.ConvertDynamic<ADUser>(message.Data);
                        r = ADTools.SetUserStatus(user);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        return res;

                    case SiteOperation.GetScriptVersion:
                        r = GetScriptVersion(message);
                        res.Data = r.Data;
                        res.Success = r.Success;
                        res.ErrorMessage = r.ErrorMessage;
                        return res;

                    case SiteOperation.Ping:
                    case SiteOperation.AddLogEntry:
                    case SiteOperation.FireScript:
                        res.Data = message.Data;
                        return res;

                }
                return null;

            }
            catch (PrincipalException ex)
            {
                res.Success = false;
                res.ErrorMessage = string.Format("An error occured processing your request. {0}", ex.Message);
                res.Exception = ex;
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = string.Format("An error occured processing your request. The site and sync logs will have more details.");
                res.Exception = ex;
                return res;
            }
        }

        /// <summary>
        /// Site will validate the user's local AD credentials for the cloud STS
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public static ValidationResponse GetValidationResponse(STSCredential credential)
        {
            var data = new ValidationResponse();
            data.UserName = credential.UserName;
            data.STSConnectionId = credential.STSConnectionId;

            // create a "principal context" - e.g. your domain (could be machine, too)
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, credential.Domain))
            {
                // validate the credentials
                data.IsValid = pc.ValidateCredentials(credential.UserName, credential.Password, ContextOptions.Sealing);
            }
            if (data.IsValid)
            {
                data.UserProperties = ADTools.SearchName(credential.UserName);
            }
            return data;
        }

        /// <summary>
        /// Site will reset the ADUser password from the admin portal
        /// </summary>
        /// <param name="credential"></param>
        private static void LogPWReset(RelayResponse response)
        {
            SyncLogEntry log;
            
            if (response.Success)
            {
                log = new SyncLogEntry(
                    EventLogEntryType.SuccessAudit,
                    string.Format("Password was reset for {0}", response.Data),
                    "SiteAgent.SiteOp.ResetPW",
                    response.Identifier);
            }
            else
            {
                log = new SyncLogEntry(
                    EventLogEntryType.FailureAudit,
                    string.Format("Password reset failed for {0} (error: {1})", response.Data, response.ErrorMessage),
                    "SiteAgent.SiteOp.ResetPW",
                    response.Identifier);
            }
            OrgApiCalls.AddSyncLog(log);
        }


        /// <summary>
        /// Master site will fire the PS call to check the queue
        /// </summary>
        public static RelayResponse ActivatePoll(RelayMessage message)
        {

            throw new NotImplementedException();
        }
        public static RelayResponse GetScriptVersion(RelayMessage message)
        {
            var res = new RelayResponse();

            try
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.GetFullPath(string.Format("{0}\\Scripts\\ScriptVersion.txt", dir));
                res.Data = File.ReadAllText(path);
                res.Success = true;
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = ex.Message;
                return res;
            }
        }
    }

    public class RelayResponseInternal: RelayResponse
    {
        public Exception Exception { get; set; }
    }
}
