using OrgRelay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace ComplexOrgSiteAgent
{
    partial class SiteListenerService : ServiceBase
    {
        readonly SigRClient _relay;

        public SiteListenerService(SigRClient relay)
        {
            InitializeComponent();

            _relay = relay;
            ServiceName = GetType().Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            CanShutdown = true;
            CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            var task = _relay.StartAsync();
            task.Wait();
        }

        protected override void OnStop()
        {
            _relay.Stop();
        }
    }
}
