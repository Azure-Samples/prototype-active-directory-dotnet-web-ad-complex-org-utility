using Infrastructure;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using OrgRelay;
using Owin;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;

[assembly: OwinStartupAttribute(typeof(ADSyncApi.Startup))]
namespace ADSyncApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //SiteHubConnections.RelaySiteList = new ConcurrentDictionary<string, RelaySite>();

            var srConfig = new HubConfiguration
            {
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = false
            };

            var connStr = ConfigurationManager.AppSettings["RedisConnectionString"];
            GlobalHost.DependencyResolver.UseRedis(new RedisScaleoutConfiguration(connStr, "SiteHub"));
            app.MapSignalR("/sitelink", srConfig);
        }
    }
}
