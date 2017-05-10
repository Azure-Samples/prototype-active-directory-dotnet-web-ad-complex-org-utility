using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADSync.Common.Models;
using Common;
using ADSync.Common.Enums;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Infrastructure
{
    //[HubAuth]
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
        public async Task TriggerPoll()
        {
            RemoteSite site = await ValidateSite();
            if (site == null)
            {
                return;
            }
            var masterConnection = SiteHubConnections.RelaySiteList.SingleOrDefault(s => s.Value.IsHQ);
            if (masterConnection.Value != null)
            {
                Clients.Client(masterConnection.Value.ConnectionId).TriggerPoll(site.Id);
            }
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
            var site = SiteHubConnections.RelaySiteList.SingleOrDefault(s => s.Value.SiteId == message.DestSiteId);

            var res = Clients.Client(site.Value.ConnectionId).Send(message);
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
            var site = SiteHubConnections.RelaySiteList.SingleOrDefault(s => s.Value.SiteId == credential.RemoteSiteId);
            
            var res = Clients.Client(site.Value.ConnectionId).Validate(credential);
        }

        /// <summary>
        /// Ensure the site calling in has a valid Api key and is correctly associated and registered in the local relay connection cache
        /// </summary>
        /// <returns></returns>
        private async Task<RemoteSite> ValidateSite()
        {
            RemoteSite site = await RefreshRelayCacheAsync();
            if (site == null)
            {
                Clients.Caller.Unauthorized();
                return null;
            }

            return site;
        }

        /// <summary>
        /// Add incoming relay client to the static connected client array
        /// </summary>
        /// <returns></returns>
        private async Task<RemoteSite> RefreshRelayCacheAsync()
        {
            //store the connectionid, apikey, and siteid in local collection for further relay processing
            var rsEntry = SiteHubConnections.RelaySiteList.SingleOrDefault(c => c.Value.ConnectionId == Context.ConnectionId);

            if (rsEntry.Value == null)
            {
                var apiKey = Context.Headers["apikey"];
                var rs = new RelaySite
                {
                    ApiKey = apiKey,
                    ConnectionId = Context.ConnectionId,
                    IsSTS = (apiKey == Settings.STSApiKey),
                    CallerIp = SiteUtils.GetIPAddress(Context.Request.GetHttpContext().Request.ServerVariables)
                };

                //check the cache/DB first to validate the api key
                RemoteSite site = await SiteUtils.AuthorizeApiAsync(apiKey);

                if (site != null)
                {
                    rs.IsHQ = (site.SiteType == SiteTypes.MasterHQ);
                    rs.SiteId = site.Id;
                }
                
                SiteHubConnections.RelaySiteList.AddOrUpdate(Context.ConnectionId, rs);
                return site;
            }
            return null;
        }

        #region Overrides
        public override Task OnConnected()
        {
            var task = Task.Run(async () => {
                await RefreshRelayCacheAsync();
            });
            task.Wait();

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
            var rs = SiteHubConnections.RelaySiteList.SingleOrDefault(c => c.Value.ConnectionId == Context.ConnectionId);
            if (rs.Value != null)
            {
                var rSite = rs.Value;
                SiteHubConnections.RelaySiteList.TryRemove(rs.Key, out rSite);
            }

            return base.OnDisconnected(stopCalled);
        }
        #endregion
    }
    public static class SiteHubConnections
    {
        public static ConcurrentDictionary<string, RelaySite> RelaySiteList;
    }

    static class ExtensionMethods
    {
        // Either Add or overwrite
        public static void AddOrUpdate<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            dictionary.AddOrUpdate(key, value, (oldkey, oldvalue) => value);
        }
    }

    public class RelaySite
    {
        public string SiteId { get; set; }
        public string ApiKey { get; set; }
        public string ConnectionId { get; set; }
        public bool IsHQ { get; set; }
        public bool IsSTS { get; set; }
        public string CallerIp { get; set; }
    }
}
