using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Reflection;

namespace ComplexOrgSiteAgent
{

    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer
    {
        private readonly System.ServiceProcess.ServiceInstaller serviceInstaller;
        public ServiceInstaller()
        {
            InitializeComponent();

            var assembly = GetType().Assembly;

            serviceInstaller = new System.ServiceProcess.ServiceInstaller()
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title,
                DisplayName = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title,
                Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description
            };

            Installers.Add(serviceInstaller);
        }

        public override void Install(IDictionary stateSaver)
        {

            base.Install(stateSaver);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
        }
    }
}
