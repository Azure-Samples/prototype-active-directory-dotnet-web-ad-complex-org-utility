using ADSync.Common.Enums;
using ADSync.Common.Models;
using Common;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;

namespace Infrastructure
{
    public class HubAuth : AuthorizeAttribute
    {
        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;
            var request = hubIncomingInvokerContext.Hub.Context.Request;
            var usr = request.GetHttpContext().User;

            return base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            IPrincipal p = null;

            try
            {
                var key = request.Headers.SingleOrDefault(h => h.Key == "apikey").Value;

                if (key == Settings.STSApiKey)
                {
                    if (!ApiAuth.AuthAndAddClaims(new RemoteSite
                    {
                        ApiKey = key,
                        Id = key,
                        SiteType = SiteTypes.LocalADOnly
                    }, ref p))
                    {
                        return false;
                    }
                    request.GetHttpContext().User = p;
                    return true;
                }

                RemoteSite site = null;
                var task = Task.Run(async () => {
                    site = await SiteUtils.AuthorizeApiAsync(key);
                });
                task.Wait();

                if (site == null)
                {
                    return false;
                }

                //we're authenticated
                if (!ApiAuth.AuthAndAddClaims(site, ref p))
                {
                    return false;
                }

                request.GetHttpContext().User = p;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }
    }
}
