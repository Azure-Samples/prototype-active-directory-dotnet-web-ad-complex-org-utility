using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ADSyncApi.Startup))]
namespace ADSyncApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
