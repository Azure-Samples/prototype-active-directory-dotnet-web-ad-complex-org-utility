using ADSync.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADSyncSiteSetup
{
    public partial class Setup : Form
    {
        ApiCalls _api;
        bool _hasConfig;
        RemoteSite _siteConfig;

        public Setup()
        {
            InitializeComponent();
            txtApiKey.Text = Properties.Settings.Default.ApiKey;
            txtSiteUrl.Text = Properties.Settings.Default.SiteUrl;
            CheckConfig();
            if (_hasConfig)
            {
                _api = new ApiCalls(txtApiKey.Text, txtSiteUrl.Text);
                LoadRemoteSiteList();
            }

        }

        private void btnCheckConfig_Click(object sender, EventArgs e)
        {
            if (!_hasConfig)
            {
                MessageBox.Show("Please configure your API Key and Site URL before confirming your config.", "Need Config", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            LoadRemoteSiteList();
        }

        private void txtApiKey_TextChanged(object sender, EventArgs e)
        {
            CheckConfig();
        }

        private void txtSiteUrl_TextChanged(object sender, EventArgs e)
        {
            CheckConfig();
        }

        private void btnLoginAD_Click(object sender, EventArgs e)
        {
            var domList = GetADDomainList(txtUsername.Text, txtPassword.Text);
            lstLocalDomainList.Items.AddRange(domList.ToArray());
        }
    }
}
