using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using ADSync.Common.Models;
using ADSync.Common.Enums;
using System.Diagnostics;
using System.Security.Claims;

namespace Infrastructure
{
    [HubAuth]
    public class SiteHub : Hub
    {
        /// <summary>
        /// Generic request received from a site, checkes SiteOperation flag for instructions
        /// </summary>
        /// <returns></returns>
        public void ProcessMessage(RelayMessage message)
        {
            switch (message.Operation)
            {
                case SiteOperation.Ping:
                    message.Data = "Ping Received";
                    Clients.Caller.Send(message);
                    break;
            }
        }
        /// <summary>
        /// When a site notices a local change, it will queue the change then fire this call to get HQ to check the queue immediately
        /// </summary>
        /// <returns></returns>
        public void TriggerPoll(RelayMessage message)
        {
            message.OriginConnectionId = Context.ConnectionId;
            Clients.Group(message.DestSiteId).Send(message);
        }
        /// <summary>
        /// When a relay response is returned from a site, this method forwards the response back to the STS (which is also a relay client)
        /// </summary>
        /// <param name="response"></param>
        public void ForwardRelayResponse(RelayResponse response)
        {
            Clients.Client(response.OriginConnectionId).ProcessRelayResponse(response);
        }

        public void ProcessRelayMessage(RelayMessage message)
        {
            message.OriginConnectionId = Context.ConnectionId;
            Clients.Group(message.DestSiteId).Send(message);
        }

        /// <summary>
        /// When a validation response is returned from a site, this method forwards the response back to the STS (which is also a relay client)
        /// </summary>
        /// <param name="response"></param>
        public void ForwardValidationResponse(ValidationResponse response)
        {
            Clients.Client(response.STSConnectionId).ProcessValidationResponse(response);
        }

        /// <summary>
        /// When a validation request is received from the STS, this method forwards the request to the appropriate site
        /// </summary>
        /// <param name="credential"></param>
        public void ProcessSTSValidationRequest(STSCredential credential)
        {
            credential.STSConnectionId = Context.ConnectionId;
            Clients.Group(credential.RemoteSiteId).Validate(credential);
        }
        
        #region Overrides
        public override Task OnConnected()
        {
            var environment = Context.Request.Environment;
            var principal = environment["server.User"] as ClaimsPrincipal;
            
            var identity = principal.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claim = identity.FindFirst("siteId");
                if (claim != null)
                {
                    Groups.Add(Context.ConnectionId, claim.Value);
                }
            }

            Debug.WriteLine("Client {0} connected", new { Context.ConnectionId });
            return base.OnConnected();
        }
        public override Task OnReconnected()
        {
            Debug.WriteLine("Client {0} RECONNECTED", Context.ConnectionId);

            return base.OnReconnected();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            Debug.WriteLine("Client {0} DISconnected", Context.ConnectionId);
            
            return base.OnDisconnected(stopCalled);
        }
        #endregion
    }
}
