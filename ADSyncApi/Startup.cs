using Infrastructure;
using Microsoft.Owin;
using OrgRelay;
using Owin;
using System.Collections.Concurrent;
using System.Collections.Generic;

[assembly: OwinStartupAttribute(typeof(ADSyncApi.Startup))]
namespace ADSyncApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            SiteHubConnections.RelaySiteList = new ConcurrentDictionary<string, RelaySite>();

            var srConfig = new Microsoft.AspNet.SignalR.HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = false
            };

            app.MapSignalR("/sitelink", srConfig);
        }
    }
}
