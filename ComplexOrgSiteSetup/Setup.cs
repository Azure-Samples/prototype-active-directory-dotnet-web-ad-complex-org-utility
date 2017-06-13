using OrgRelay;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ADSync.Common.Models;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using Common;
using ComplexOrgSiteAgent;
using System.Reflection;
using System.DirectoryServices;
using ADSync.Common.Enums;

namespace ComplexOrgSiteSetup
{
    public partial class Setup: Form
    {
        bool _hasConfig;
        RemoteSite _siteConfig;
        public static string LogSource = "ComplexOrg Setup Log";
        private string _appPath;
        private static string AgentAssemblyName;
        private static string AgentServiceName;

        public Setup()
        {
            try
            {
                var dd = ConfigurationManager.AppSettings["DebugDelay"];
                if (dd != "")
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(dd));
                }

                InitializeComponent();
                tabControl2.TabPages[1].Enabled = false;

                _appPath = Path.GetDirectoryName(Application.ExecutablePath);
                var agentPath = Path.Combine(_appPath, "ComplexOrgSiteAgent.exe");
                var assembly = Assembly.ReflectionOnlyLoadFrom(agentPath);
                var attrData = assembly.GetCustomAttributesData();
                var attr = attrData.Single(d => d.AttributeType == typeof(AssemblyTitleAttribute));
                AgentServiceName = attr.ConstructorArguments[0].Value.ToString();
                AgentAssemblyName = string.Format("{0}.SiteListenerService", assembly.GetName().Name);

                ServiceUtil.AssemblyName = AgentAssemblyName;
                ServiceUtil.ServiceName = AgentServiceName;

                LogSource = Utils.SetupLog(LogSource);

                txtApiKey.Text = ConfigurationManager.AppSettings["ApiKey"];
                txtSiteUrl.Text = ConfigurationManager.AppSettings["SiteUrl"];
                txtUsername.Text = ConfigurationManager.AppSettings["Username"];
                CheckConfig();
            }
            catch (Exception ex)
            {
                var msg = string.Format("An unknown error occured during initialization ({0}).", ex.Message);
                Utils.AddLogEntry("Error initializing", System.Diagnostics.EventLogEntryType.Error, 0, ex);
                MessageBox.Show(msg, "Error initializing application", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
        }

        private void btnCheckConfig_Click(object sender, EventArgs e)
        {
            CheckConfig();
        }

        private bool CheckDomainsToUpns()
        {
            if (lstRemoteDomainList.Items.Count == 0)
            {
                WriteStatus(string.Format("Not logged into site configuration yet"));
                txtApiKey.Focus();
                return false;
            }
            if (lstLocalDomainList.Items.Count == 0)
            {
                WriteStatus(string.Format("Not logged into local AD yet"));
                txtUsername.Focus();
                return false;
            }

            //test: are all of the remote domains represented in the local domain list?
            foreach (var item in lstRemoteDomainList.Items)
            {
                if (!lstLocalDomainList.Items.Contains(item))
                {
                    WriteStatus(string.Format("{0} not found in local UPN list", item));
                    return false;
                }
            }
            //we're here, site is confirmed
            btnGoSvcConfig.Enabled = true;
            tabControl2.TabPages[1].Enabled = true;
            chkConfigConfirmed.Enabled = true;
            chkConfigConfirmed.Checked = true;
            WriteStatus("Site is confirmed, continue to Service Config");
            btnGoSvcConfig.Focus();
            return true;
        }

        private void btnLoginAD_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Please enter the credentials to connect to Active Directory", "Missing Credentials");
                return;
            }
            SaveConfig();
            try
            {
                lstLocalDomainList.Items.Clear();
                var domList = ADTools.GetADDomainList(txtUsername.Text, txtPassword.Text);
                
                lstLocalDomainList.Items.AddRange(domList);
                CheckDomainsToUpns();
            }
            catch (ActiveDirectoryOperationException ex)
            {
                var msg = string.Format("There was a problem enumerating the domain list in your forest. Please ensure that the machine running this utility is a domain member ({0}).", ex.Message);
                MessageBox.Show(msg, "Active Directory Access Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch(DirectoryServicesCOMException ex)
            {
                var msg = string.Format("The server was unable to complete the login.\n\nDetail: {0}", ex.Message);
                MessageBox.Show(msg, "Active Directory Access Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (Exception ex)
            {
                var msg = string.Format("An unknown error occured getting the domain list ({0}).", ex.Message);
                Utils.AddLogEntry("Error getting AD domain list", System.Diagnostics.EventLogEntryType.Error, 0, ex);
                MessageBox.Show(msg, "Active Directory Access Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void CheckConfig()
        {
            _hasConfig = (txtApiKey.Text.Length > 0 && txtSiteUrl.Text.Length > 0);
            if (_hasConfig)
            {
                OrgApiCalls.ApiKey = txtApiKey.Text;
                OrgApiCalls.SiteUrl = txtSiteUrl.Text;
                _siteConfig = OrgApiCalls.GetSiteConfig();
                txtDomainName.Text = _siteConfig.OnPremDomainName;
                txtSiteID.Text = _siteConfig.Id;
                ADTools.ADDomainName = _siteConfig.OnPremDomainName;

                txtSiteType.Text = _siteConfig.SiteType.ToString();
                SaveConfig();
                lstRemoteDomainList.Items.Clear();
                lstRemoteDomainList.Items.AddRange(_siteConfig.SiteDomains.ToArray());
                CheckDomainsToUpns();
            } 
            else
            {
                MessageBox.Show("Please configure your API Key and Site URL before confirming your config.", "Need Config", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
        }

        private void SaveConfig()
        {
            //saving config file for agent
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            string agentConfigFile = Path.Combine(appPath, "ComplexOrgSiteAgent.exe.config");
            string setupConfigFile = Path.Combine(appPath, "ComplexOrgSiteSetup.exe.config");
            string scriptConfigFile = Path.Combine(appPath, "Scripts\\SyncVars.json");

            var updates = new Dictionary<string, string>
            {
                { "ApiKey", txtApiKey.Text },
                { "SiteUrl", txtSiteUrl.Text },
                { "Username", txtUsername.Text }
            };

            var scriptUpdates = new Dictionary<string, string>
            {
                { "ApiKey", txtApiKey.Text },
                { "ApiSite", txtSiteUrl.Text }
            };

            if (txtSiteType.Text == SiteTypes.MasterHQ.ToString())
            {
                //to enable GraphAPI calls for B2B invitations
                scriptUpdates.Add("AADClientID", ConfigurationManager.AppSettings["AADClientID"]);
                scriptUpdates.Add("AADClientKey", ConfigurationManager.AppSettings["AADClientKey"]);
                scriptUpdates.Add("AADTenantID", ConfigurationManager.AppSettings["AADTenantID"]);

                updates.Add("AADClientID", ConfigurationManager.AppSettings["AADClientID"]);
                updates.Add("AADClientKey", ConfigurationManager.AppSettings["AADClientKey"]);
                updates.Add("AADTenantID", ConfigurationManager.AppSettings["AADTenantID"]);
            }

            var success = AppUtil.ModifyAppConfig(setupConfigFile, updates);
            success = success && AppUtil.ModifyAppConfig(agentConfigFile, updates);
            success = success && AppUtil.ModifyJsonConfig(scriptConfigFile, scriptUpdates);
            if (!success)
            {
                WriteStatus("One or more configuration files weren't updated, please check the error logs");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            SaveConfig();
            Application.Exit();
        }

        private void btnUseADCredentials_Click(object sender, EventArgs e)
        {
            txtServicePassword.Text = txtPassword.Text;
            txtServiceUsername.Text = txtUsername.Text;
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl2.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    RefreshServiceData();
                    break;
            }
        }

        private void RefreshServiceData()
        {
            if (ServiceUtil.IsInstalled())
            {
                btnInstall.Text = "Uninstall";
                txtServiceUsername.Enabled = false;
                txtServicePassword.Enabled = false;

                var status = ServiceUtil.GetServiceStatus();
                lblServiceStatusRes.Text = status.ToString();
            }
            else
            {
                btnInstall.Text = "Install";
                txtServiceUsername.Enabled = true;
                txtServicePassword.Enabled = true;
                lblServiceStatusRes.Text = "Service Not Installed";
            }

            var log = Path.Combine(_appPath, "ComplexOrgSiteAgent.InstallLog");
            if (File.Exists(log))
            {

                txtServiceLog.Text = File.ReadAllText(log);
            }
            else
            {
                txtServiceLog.Text = "Logs not found";
            }
        }

        private void WriteStatus(string message)
        {
            lblStatusInfo.Text = message;
            var t = new Timer()
            {
                Interval = 5000
            };
            t.Tick += (o, e) => {
                lblStatusInfo.Text = "";
                t.Stop();
                t.Dispose();
            };
            t.Start();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                switch (btnInstall.Text)
                {
                    case "Install":
                        if (txtServiceUsername.Text.Length == 0 || txtServicePassword.Text.Length == 0)
                        {
                            MessageBox.Show("Please enter the service account credentials to use for the service installation. Account must be a member of the domain admins group.");
                            return;
                        }
                        ServiceUtil.InstallService(txtServiceUsername.Text, txtServicePassword.Text);
                        Task.Delay(3000).Wait();

                        ServiceUtil.StartService();
                        break;

                    case "Uninstall":
                        var res = MessageBox.Show("Are you sure you want to remove the Site Listener Agent?", "Remove Agent", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2);
                        if (res == DialogResult.No)
                        {
                            return;
                        }
                        lblServiceStatusRes.Text = "Uninstalling...";
                        Application.DoEvents();
                        ServiceUtil.StopService();
                        ServiceUtil.UninstallService();
                        break;
                }
                RefreshServiceData();
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occured managing the service - {0}.", ex.Message);
                Utils.AddLogEntry("Error managing the service from the setup utility", System.Diagnostics.EventLogEntryType.Error, 0, ex);
                MessageBox.Show(msg, "Service Management Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefreshSvcStatus_Click(object sender, EventArgs e)
        {
            RefreshServiceData();
        }

        private void btnGoSvcConfig_Click(object sender, EventArgs e)
        {
            tabControl2.SelectTab(1);
        }

        private void tabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex==1 && !e.TabPage.Enabled)
            {
                tabControl2.SelectedIndex = 0;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLoginAD_Click(sender, e);
            }
        }

    }
}
