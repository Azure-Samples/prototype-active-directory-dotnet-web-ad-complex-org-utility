using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace ComplexOrgSiteAgent
{
    public static class ServiceUtil
    {
        public static string ServiceName { get; set; }
        public static string AssemblyName { get; set; }

        public static bool IsInstalled()
        {
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                try
                {
                    ServiceControllerStatus status = controller.Status;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool IsRunning()
        {
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                if (!IsInstalled()) return false;
                return (controller.Status == ServiceControllerStatus.Running);
            }
        }
        public static ServiceControllerStatus GetServiceStatus()
        {
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                return controller.Status;
            }
        }

        public static AssemblyInstaller GetInstaller()
        {
            AssemblyInstaller installer = new AssemblyInstaller(
                Type.GetType(AssemblyName).Assembly, null)
            {
                UseNewContext = true
            };

            return installer;
        }

        public static void InstallService(string userName, string password)
        {
            if (IsInstalled()) return;

            try
            {
                using (AssemblyInstaller installer = GetInstaller())
                {
                    IDictionary state = new Hashtable();
                    try
                    {
                        installer.CommandLine = new string[]
                        {
                            string.Format("/username={0}", userName),
                            string.Format("/password={0}", password),
                        };

                        installer.Install(state);
                        installer.Commit(state);
                        SetRecoveryOptions();
                    }
                    catch
                    {
                        try
                        {
                            installer.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public static void UninstallService()
        {
            if (!IsInstalled()) return;
            try
            {
                using (AssemblyInstaller installer = GetInstaller())
                {
                    IDictionary state = new Hashtable();
                    try
                    {
                        installer.Uninstall(state);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static void SetRecoveryOptions()
        {
            int exitCode;
            using (var process = new Process())
            {
                var startInfo = process.StartInfo;
                startInfo.FileName = "sc";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= restart/60000/restart/150000/restart/300000", ServiceName);

                process.Start();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
                throw new InvalidOperationException();
        }

        public static void StartService()
        {
            if (!IsInstalled()) return;

            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                try
                {
                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running,
                            TimeSpan.FromSeconds(10));
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void StopService()
        {
            if (!IsInstalled()) return;
            using (ServiceController controller =
                new ServiceController(ServiceName))
            {
                try
                {
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped,
                             TimeSpan.FromSeconds(10));
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
