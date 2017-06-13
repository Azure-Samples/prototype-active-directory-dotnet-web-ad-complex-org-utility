using ADSync.Common.Enums;
using ADSync.Common.Models;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Linq.Expressions;

namespace OrgRelay
{
    public static class ADTools
    {
        private static readonly string[] SProps = { "SAMAccountName","DisplayName","givenName","sn","title","department","division",
                                "mail","objectGuid","l","streetAddress","st","postalCode","userPrincipalName",
                                "telephoneNumber","mobile","homePhone","whenCreated","c","mS-DS-ConsistencyGuid" };

        private const string sFilterTemplate = "(&(objectClass=user)(objectCategory=person)(|(userPrincipalName={0})(mail={0})(SAMAccountName={0})))";

        public static string ADDomainName { get; set; }

        public static object[] GetADDomainList(string userName, string password)
        {
            var res = new List<string>
            {
                ADDomainName
            };

            var path = "DC=" + string.Join(",DC=", ADDomainName.Split('.'));
            DirectoryEntry oPartition = new DirectoryEntry(string.Format("LDAP://{0}/CN=Partitions,CN=Configuration,{1}", ADDomainName, path), userName, password);
            DirectorySearcher mySearcher = new DirectorySearcher(oPartition);
            mySearcher.PropertiesToLoad.Add("uPNSuffixes");
            foreach (SearchResult searchResults in mySearcher.FindAll())
            {
                foreach (string propertyName in searchResults.Properties.PropertyNames)
                {
                    if (propertyName == "upnsuffixes")
                    {
                        foreach (Object retEntry in searchResults.Properties[propertyName])
                        {
                            res.Add(retEntry.ToString());
                        }
                    }
                }
            }

            /*
            oPartition.Invoke("GetEx", new object[] { "uPNSuffixes" });

            var upns = oPartition.InvokeGet("uPNSuffixes");
            if (upns.GetType().Name == "Object[]")
            {
                res.AddRange((upns as object[]).Select(u => (string)u));
            }
            else
            {
                res.Add(upns.ToString());
            }
            */

            return res.ToArray<object>();
        }

        public static StagedUser SearchName(string user)
        {
            return GetResults(GetUser(user));
        }

        private static SearchResult GetUser(string user)
        {
            SearchResult res = null;

            using (var entry = new DirectoryEntry("LDAP://" + ADDomainName))
            {
                using (var search = new DirectorySearcher(entry))
                {
                    search.Filter = string.Format(sFilterTemplate, user);
                    search.SearchScope = SearchScope.Subtree;
                    search.PropertiesToLoad.AddRange(SProps);
                    res = search.FindOne();
                }
            }

            return res;
        }

        public static RelayResponse ResetPassword(RelayMessage message)
        {
            var credential = Utils.ConvertDynamic<PasswordReset>(message.Data);

            var res = new RelayResponse
            {
                Operation = SiteOperation.ResetPW,
                OriginConnectionId = message.OriginConnectionId,
                Identifier = message.Identifier,
                OriginSiteId = message.OriginSiteId,
                Data = credential.UserName
            };

            var user = GetUserPrincipal(credential.UserName);
            user.SetPassword(credential.NewPassword);
            user.Save();

            if (credential.Unlock)
            {
                user.UnlockAccount();
                user.Save();
            }

            if (credential.SetChangePasswordAtNextLogon)
            {
                user.ExpirePasswordNow();
            }

            return res;
        }

        public static RelayResponse EnableDisableUser(string username, bool enabled)
        {
            var res = new RelayResponse();
            try
            {
                UserPrincipal userPrincipal = GetUserPrincipal(username);
                userPrincipal.Enabled = enabled;
                userPrincipal.Save();
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = ex.Message;
                return res;
            }
        }

        public static RelayResponse UpdateUser(StagedUser user)
        {
            var res = new RelayResponse
            {
                Operation = SiteOperation.UpdateUser
            };

            try
            {
                var principal = GetUserPrincipal(user.Upn);
                principal.GivenName = user.GivenName;
                principal.Surname = user.Surname;
                principal.DisplayName = user.DisplayName;
                principal.Name = user.Name;
                principal.EmailAddress = user.Mail;
                principal.VoiceTelephoneNumber = user.TelephoneNumber;

                var de = (DirectoryEntry)principal.GetUnderlyingObject();
                de.Properties["title"].Value = user.Title;
                de.Properties["streetAddress"].Value = user.StreetAddress;
                de.Properties["st"].Value = user.State;
                de.Properties["postalCode"].Value = user.PostalCode;
                de.Properties["mobile"].Value = user.Mobile;
                de.Properties["homePhone"].Value = user.HomePhone;
                de.Properties["department"].Value = user.Department;
                de.Properties["c"].Value = user.Country;
                de.Properties["l"].Value = user.City;
                de.CommitChanges();
                principal.Save();
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = ex.Message;
                return res;
            }
        }

        private static UserPrincipal GetUserPrincipal(string username)
        {
            PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, ADDomainName);
            return UserPrincipal.FindByIdentity(principalContext, username);
        }

        public static RelayResponse SetUserStatus(ADUser user)
        {
            var res = new RelayResponse
            {
                Operation = SiteOperation.SetUserStatus
            };
            try
            {
                UserPrincipal userPrincipal = GetUserPrincipal(user.OrgSamAccountName);
                userPrincipal.Enabled = user.Enabled;
                userPrincipal.AccountExpirationDate = user.AccountExpirationDate;
                userPrincipal.PasswordNeverExpires = user.PasswordNeverExpires;
                userPrincipal.SmartcardLogonRequired = user.SmartcardLogonRequired;
                userPrincipal.UserCannotChangePassword = user.UserCannotChangePassword;
                if (user.OrgSamAccountName != user.SamAccountName)
                {
                    userPrincipal.SamAccountName = user.SamAccountName;
                }
                userPrincipal.Save();
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = ex.Message;
                return res;
            }
        }

        public static RelayResponse GetUserStatus(string localGuid)
        {
            var res = new RelayResponse
            {
                Operation = SiteOperation.GetUserStatus
            };

            try
            {
                UserPrincipal p = GetUserPrincipal(localGuid);

                var usr = new ADUser
                {
                    AccountExpirationDate = p.AccountExpirationDate,
                    AccountLockoutTime = p.AccountLockoutTime,
                    BadLogonCount = p.BadLogonCount,
                    Enabled = p.Enabled,
                    LastBadPasswordAttempt = p.LastBadPasswordAttempt,
                    LastLogon = p.LastLogon,
                    LastPasswordSet = p.LastPasswordSet,
                    PasswordNeverExpires = p.PasswordNeverExpires,
                    SamAccountName = p.SamAccountName,
                    SmartcardLogonRequired = p.SmartcardLogonRequired,
                    UserCannotChangePassword = p.UserCannotChangePassword
                };
                res.Data = usr;
                return res;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.ErrorMessage = ex.Message;
                return res;
            }
        }

        private static StagedUser GetResults(SearchResult result)
        {
            var oRes = result.Properties;
            if (result == null) return null;

            var user = new StagedUser
            {
                Name = oRes["SAMAccountName"][0].ToString().ToLower(),
                DisplayName = (oRes["DisplayName"].Count > 0) ? oRes["DisplayName"][0].ToString() : "",
                GivenName = (oRes["givenName"].Count > 0) ? oRes["givenName"][0].ToString() : "",
                Surname = (oRes["sn"].Count > 0) ? oRes["sn"][0].ToString() : "",
                Title = (oRes["title"].Count > 0) ? oRes["title"][0].ToString() : "",
                Mail = (oRes["mail"].Count > 0) ? oRes["mail"][0].ToString().ToLower() : "",
                TelephoneNumber = (oRes["telephoneNumber"].Count > 0) ? oRes["telephoneNumber"][0].ToString() : "",
                Mobile = (oRes["mobile"].Count > 0) ? oRes["mobile"][0].ToString() : "",
                StreetAddress = (oRes["streetAddress"].Count > 0) ? oRes["streetAddress"][0].ToString() : "",
                State = (oRes["st"].Count > 0) ? oRes["st"][0].ToString() : "",
                PostalCode = (oRes["postalCode"].Count > 0) ? oRes["postalCode"][0].ToString() : "",
                City = (oRes["l"].Count > 0) ? oRes["l"][0].ToString() : "",
                Country = (oRes["c"].Count > 0) ? oRes["c"][0].ToString() : "US",
                Upn = (oRes["userPrincipalName"].Count > 0) ? oRes["userPrincipalName"][0].ToString() : "",
                Department = (oRes["department"].Count > 0) ? oRes["department"][0].ToString() : "",
                HomePhone = (oRes["homePhone"].Count > 0) ? oRes["homePhone"][0].ToString() : ""
            };

            var localGuid = (oRes["objectGuid"].Count > 0) ? (oRes["objectGuid"][0] as Byte[]) : null;
            var masterGuid = (oRes["mS-DS-ConsistencyGuid"].Count > 0) ? (oRes["mS-DS-ConsistencyGuid"][0] as Byte[]) : null;

            user.LocalGuid = (localGuid != null) ? Convert.ToBase64String(localGuid) : "";
            user.MasterGuid = (masterGuid != null) ? Convert.ToBase64String(masterGuid) : "";

            return user;
        }
    }
}
