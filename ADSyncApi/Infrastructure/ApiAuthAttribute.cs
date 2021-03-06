﻿using ADSync.Common.Enums;
using ADSync.Common.Models;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Security.Principal;

namespace Infrastructure
{
    public class ApiAuth: AuthorizeAttribute
    {
        private bool _isAdmin;
        public ApiAuth(string isAdmin="")
        {
            _isAdmin = (isAdmin == "Admin");
        }
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var key = actionContext.Request.Headers.SingleOrDefault(h => h.Key=="apikey").Value.FirstOrDefault();

                RemoteSite site = null;
                var task = Task.Run(async () => {
                    site = await SiteUtils.AuthorizeApiAsync(key);
                });
                task.Wait();

                if (site == null)
                {
                    Unauthorized(ref actionContext);
                    base.OnAuthorization(actionContext);
                    return;
                }
                if (_isAdmin && site.SiteType != SiteTypes.MasterHQ)
                {
                    Unauthorized(ref actionContext);
                    base.OnAuthorization(actionContext);
                    return;
                }

                //we're authenticated
                var p = actionContext.RequestContext.Principal;
                if (!AuthAndAddClaims(site, ref p))
                {
                    Unauthorized(ref actionContext);
                    base.OnAuthorization(actionContext);
                    return;
                }
                actionContext.RequestContext.Principal = p;
                base.OnAuthorization(actionContext);
                return;
            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }

        protected void Unauthorized(ref HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Forbidden);
            response.Headers.Add("AuthenticationStatus", "NotAuthorized");
            response.ReasonPhrase = "ApiKey is invalid.";
            actionContext.Response = response;
        }

        public static bool AuthAndAddClaims(RemoteSite site, ref IPrincipal principal)
        {
            try
            {
                string domainList = string.Join(",", site.SiteDomains.ToArray());
                List<Claim> claims = new List<Claim>
                {
                    new Claim(CustomClaimTypes.SiteId, site.Id),
                    new Claim(CustomClaimTypes.SiteDomain, domainList),
                    new Claim(CustomClaimTypes.OnPremDomainName, site.OnPremDomainName)
                };
                
                // create an identity with the valid claims.
                ClaimsIdentity identity = new ClaimsIdentity(claims, CustomAuthTypes.Api);

                // set the context principal.
                principal = new ClaimsPrincipal(new[] { identity });
                return true;

            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Auth Error", System.Diagnostics.EventLogEntryType.Error, ex);
                return false;
            }
        }

    }
}
