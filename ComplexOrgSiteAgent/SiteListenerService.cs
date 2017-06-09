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
        readonly ScriptTimer _timer;

        public SiteListenerService(SigRClient relay, ScriptTimer timer)
        {
            InitializeComponent();

            _relay = relay;
            _timer = timer;
            ServiceName = GetType().Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            CanShutdown = true;
            CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            _timer.Start();
            var task = _relay.StartAsync();
            task.Wait();
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _relay.Stop();
        }
    }
}
