namespace ComplexOrgSiteSetup
{
    partial class Setup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setup));
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabMain = new System.Windows.Forms.TabPage();
            this.txtInstructions = new System.Windows.Forms.TextBox();
            this.chkConfigConfirmed = new System.Windows.Forms.CheckBox();
            this.btnGoSvcConfig = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLoginAD = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.MaskedTextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblLocalDomainList = new System.Windows.Forms.Label();
            this.lstLocalDomainList = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSiteType = new System.Windows.Forms.TextBox();
            this.txtDomainName = new System.Windows.Forms.TextBox();
            this.lblDomainName = new System.Windows.Forms.Label();
            this.lstRemoteDomainList = new System.Windows.Forms.ListBox();
            this.lblRemoteDomainList = new System.Windows.Forms.Label();
            this.txtSiteUrl = new System.Windows.Forms.TextBox();
            this.lblSiteUrl = new System.Windows.Forms.Label();
            this.btnCheckConfig = new System.Windows.Forms.Button();
            this.lblApiKey = new System.Windows.Forms.Label();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.tabSvcConfig = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnUseADCredentials = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.lblServicePassword = new System.Windows.Forms.Label();
            this.lblServiceUsername = new System.Windows.Forms.Label();
            this.txtServicePassword = new System.Windows.Forms.TextBox();
            this.txtServiceUsername = new System.Windows.Forms.TextBox();
            this.btnRefreshSvcStatus = new System.Windows.Forms.Button();
            this.lblServiceLogs = new System.Windows.Forms.Label();
            this.txtServiceLog = new System.Windows.Forms.TextBox();
            this.lblServiceStatusRes = new System.Windows.Forms.Label();
            this.lblServiceStatus = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.lblStatusInfo = new System.Windows.Forms.Label();
            this.tabControl2.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabSvcConfig.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabMain);
            this.tabControl2.Controls.Add(this.tabSvcConfig);
            this.tabControl2.Location = new System.Drawing.Point(12, 12);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.Padding = new System.Drawing.Point(8, 8);
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(962, 689);
            this.tabControl2.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl2.TabIndex = 15;
            this.tabControl2.SelectedIndexChanged += new System.EventHandler(this.tabControl2_SelectedIndexChanged);
            this.tabControl2.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl2_Selecting);
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.txtInstructions);
            this.tabMain.Controls.Add(this.chkConfigConfirmed);
            this.tabMain.Controls.Add(this.btnGoSvcConfig);
            this.tabMain.Controls.Add(this.groupBox2);
            this.tabMain.Controls.Add(this.groupBox1);
            this.tabMain.Controls.Add(this.txtSiteUrl);
            this.tabMain.Controls.Add(this.lblSiteUrl);
            this.tabMain.Controls.Add(this.btnCheckConfig);
            this.tabMain.Controls.Add(this.lblApiKey);
            this.tabMain.Controls.Add(this.txtApiKey);
            this.tabMain.Location = new System.Drawing.Point(4, 43);
            this.tabMain.Name = "tabMain";
            this.tabMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabMain.Size = new System.Drawing.Size(954, 642);
            this.tabMain.TabIndex = 0;
            this.tabMain.Text = "Main";
            this.tabMain.UseVisualStyleBackColor = true;
            // 
            // txtInstructions
            // 
            this.txtInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInstructions.BackColor = System.Drawing.SystemColors.Info;
            this.txtInstructions.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtInstructions.Location = new System.Drawing.Point(23, 543);
            this.txtInstructions.Multiline = true;
            this.txtInstructions.Name = "txtInstructions";
            this.txtInstructions.ReadOnly = true;
            this.txtInstructions.Size = new System.Drawing.Size(600, 80);
            this.txtInstructions.TabIndex = 39;
            this.txtInstructions.Text = "Please enter your API Key and URL, click \"Get Config\", then \"Login to AD\". Once y" +
    "our site is confirmed you can continue with service configuration.";
            // 
            // chkConfigConfirmed
            // 
            this.chkConfigConfirmed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkConfigConfirmed.AutoSize = true;
            this.chkConfigConfirmed.Enabled = false;
            this.chkConfigConfirmed.Location = new System.Drawing.Point(669, 543);
            this.chkConfigConfirmed.Name = "chkConfigConfirmed";
            this.chkConfigConfirmed.Size = new System.Drawing.Size(249, 29);
            this.chkConfigConfirmed.TabIndex = 38;
            this.chkConfigConfirmed.Text = "Configuration Confirmed";
            this.chkConfigConfirmed.UseVisualStyleBackColor = true;
            // 
            // btnGoSvcConfig
            // 
            this.btnGoSvcConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoSvcConfig.Enabled = false;
            this.btnGoSvcConfig.Location = new System.Drawing.Point(669, 578);
            this.btnGoSvcConfig.Name = "btnGoSvcConfig";
            this.btnGoSvcConfig.Size = new System.Drawing.Size(248, 45);
            this.btnGoSvcConfig.TabIndex = 6;
            this.btnGoSvcConfig.Text = "Go to Service Config >>";
            this.btnGoSvcConfig.UseVisualStyleBackColor = true;
            this.btnGoSvcConfig.Click += new System.EventHandler(this.btnGoSvcConfig_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnLoginAD);
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.lblPassword);
            this.groupBox2.Controls.Add(this.lblUsername);
            this.groupBox2.Controls.Add(this.txtUsername);
            this.groupBox2.Controls.Add(this.lblLocalDomainList);
            this.groupBox2.Controls.Add(this.lstLocalDomainList);
            this.groupBox2.Location = new System.Drawing.Point(489, 170);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(446, 353);
            this.groupBox2.TabIndex = 36;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Local Domain Settings";
            // 
            // btnLoginAD
            // 
            this.btnLoginAD.Location = new System.Drawing.Point(300, 135);
            this.btnLoginAD.Name = "btnLoginAD";
            this.btnLoginAD.Size = new System.Drawing.Size(130, 44);
            this.btnLoginAD.TabIndex = 5;
            this.btnLoginAD.Text = "Login to AD";
            this.btnLoginAD.UseVisualStyleBackColor = true;
            this.btnLoginAD.Click += new System.EventHandler(this.btnLoginAD_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(22, 142);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(270, 29);
            this.txtPassword.TabIndex = 4;
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(17, 114);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(98, 25);
            this.lblPassword.TabIndex = 39;
            this.lblPassword.Text = "Password";
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(17, 40);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(236, 25);
            this.lblUsername.TabIndex = 38;
            this.lblUsername.Text = "Login (domain\\username)";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(22, 68);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(270, 29);
            this.txtUsername.TabIndex = 3;
            // 
            // lblLocalDomainList
            // 
            this.lblLocalDomainList.AutoSize = true;
            this.lblLocalDomainList.Location = new System.Drawing.Point(17, 187);
            this.lblLocalDomainList.Name = "lblLocalDomainList";
            this.lblLocalDomainList.Size = new System.Drawing.Size(194, 25);
            this.lblLocalDomainList.TabIndex = 37;
            this.lblLocalDomainList.Text = "Local UPN Suffix List";
            // 
            // lstLocalDomainList
            // 
            this.lstLocalDomainList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLocalDomainList.FormattingEnabled = true;
            this.lstLocalDomainList.ItemHeight = 24;
            this.lstLocalDomainList.Location = new System.Drawing.Point(22, 215);
            this.lstLocalDomainList.Name = "lstLocalDomainList";
            this.lstLocalDomainList.Size = new System.Drawing.Size(406, 100);
            this.lstLocalDomainList.TabIndex = 36;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtSiteType);
            this.groupBox1.Controls.Add(this.txtDomainName);
            this.groupBox1.Controls.Add(this.lblDomainName);
            this.groupBox1.Controls.Add(this.lstRemoteDomainList);
            this.groupBox1.Controls.Add(this.lblRemoteDomainList);
            this.groupBox1.Location = new System.Drawing.Point(23, 170);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(446, 353);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Site Config";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 25);
            this.label1.TabIndex = 40;
            this.label1.Text = "Site Type";
            // 
            // txtSiteType
            // 
            this.txtSiteType.BackColor = System.Drawing.SystemColors.Control;
            this.txtSiteType.Location = new System.Drawing.Point(19, 142);
            this.txtSiteType.Name = "txtSiteType";
            this.txtSiteType.ReadOnly = true;
            this.txtSiteType.Size = new System.Drawing.Size(291, 29);
            this.txtSiteType.TabIndex = 39;
            // 
            // txtDomainName
            // 
            this.txtDomainName.Location = new System.Drawing.Point(19, 68);
            this.txtDomainName.Name = "txtDomainName";
            this.txtDomainName.ReadOnly = true;
            this.txtDomainName.Size = new System.Drawing.Size(291, 29);
            this.txtDomainName.TabIndex = 38;
            // 
            // lblDomainName
            // 
            this.lblDomainName.AutoSize = true;
            this.lblDomainName.Location = new System.Drawing.Point(14, 40);
            this.lblDomainName.Name = "lblDomainName";
            this.lblDomainName.Size = new System.Drawing.Size(136, 25);
            this.lblDomainName.TabIndex = 37;
            this.lblDomainName.Text = "Domain Name";
            // 
            // lstRemoteDomainList
            // 
            this.lstRemoteDomainList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstRemoteDomainList.FormattingEnabled = true;
            this.lstRemoteDomainList.ItemHeight = 24;
            this.lstRemoteDomainList.Location = new System.Drawing.Point(19, 215);
            this.lstRemoteDomainList.Name = "lstRemoteDomainList";
            this.lstRemoteDomainList.Size = new System.Drawing.Size(409, 100);
            this.lstRemoteDomainList.TabIndex = 36;
            // 
            // lblRemoteDomainList
            // 
            this.lblRemoteDomainList.AutoSize = true;
            this.lblRemoteDomainList.Location = new System.Drawing.Point(14, 187);
            this.lblRemoteDomainList.Name = "lblRemoteDomainList";
            this.lblRemoteDomainList.Size = new System.Drawing.Size(186, 25);
            this.lblRemoteDomainList.TabIndex = 35;
            this.lblRemoteDomainList.Text = "Remote Domain List";
            // 
            // txtSiteUrl
            // 
            this.txtSiteUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSiteUrl.Location = new System.Drawing.Point(23, 102);
            this.txtSiteUrl.Name = "txtSiteUrl";
            this.txtSiteUrl.Size = new System.Drawing.Size(722, 29);
            this.txtSiteUrl.TabIndex = 1;
            // 
            // lblSiteUrl
            // 
            this.lblSiteUrl.AutoSize = true;
            this.lblSiteUrl.Location = new System.Drawing.Point(18, 74);
            this.lblSiteUrl.Name = "lblSiteUrl";
            this.lblSiteUrl.Size = new System.Drawing.Size(89, 25);
            this.lblSiteUrl.TabIndex = 18;
            this.lblSiteUrl.Text = "Site URL";
            // 
            // btnCheckConfig
            // 
            this.btnCheckConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckConfig.Location = new System.Drawing.Point(787, 64);
            this.btnCheckConfig.Name = "btnCheckConfig";
            this.btnCheckConfig.Size = new System.Drawing.Size(130, 44);
            this.btnCheckConfig.TabIndex = 2;
            this.btnCheckConfig.Text = "Get Config";
            this.btnCheckConfig.UseVisualStyleBackColor = true;
            this.btnCheckConfig.Click += new System.EventHandler(this.btnCheckConfig_Click);
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(18, 3);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(123, 25);
            this.lblApiKey.TabIndex = 12;
            this.lblApiKey.Text = "Site API Key";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtApiKey.Location = new System.Drawing.Point(23, 32);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(722, 29);
            this.txtApiKey.TabIndex = 0;
            // 
            // tabSvcConfig
            // 
            this.tabSvcConfig.Controls.Add(this.groupBox3);
            this.tabSvcConfig.Controls.Add(this.btnRefreshSvcStatus);
            this.tabSvcConfig.Controls.Add(this.lblServiceLogs);
            this.tabSvcConfig.Controls.Add(this.txtServiceLog);
            this.tabSvcConfig.Controls.Add(this.lblServiceStatusRes);
            this.tabSvcConfig.Controls.Add(this.lblServiceStatus);
            this.tabSvcConfig.Location = new System.Drawing.Point(4, 43);
            this.tabSvcConfig.Name = "tabSvcConfig";
            this.tabSvcConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabSvcConfig.Size = new System.Drawing.Size(954, 642);
            this.tabSvcConfig.TabIndex = 1;
            this.tabSvcConfig.Text = "Service Config";
            this.tabSvcConfig.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnUseADCredentials);
            this.groupBox3.Controls.Add(this.btnInstall);
            this.groupBox3.Controls.Add(this.lblServicePassword);
            this.groupBox3.Controls.Add(this.lblServiceUsername);
            this.groupBox3.Controls.Add(this.txtServicePassword);
            this.groupBox3.Controls.Add(this.txtServiceUsername);
            this.groupBox3.Location = new System.Drawing.Point(19, 20);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(400, 286);
            this.groupBox3.TabIndex = 52;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Manage Service";
            // 
            // btnUseADCredentials
            // 
            this.btnUseADCredentials.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUseADCredentials.Location = new System.Drawing.Point(238, 70);
            this.btnUseADCredentials.Name = "btnUseADCredentials";
            this.btnUseADCredentials.Size = new System.Drawing.Size(141, 108);
            this.btnUseADCredentials.TabIndex = 15;
            this.btnUseADCredentials.Text = "Use Login From Main Tab";
            this.btnUseADCredentials.UseVisualStyleBackColor = true;
            this.btnUseADCredentials.Click += new System.EventHandler(this.btnUseADCredentials_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstall.Location = new System.Drawing.Point(22, 214);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(125, 49);
            this.btnInstall.TabIndex = 14;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // lblServicePassword
            // 
            this.lblServicePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServicePassword.AutoSize = true;
            this.lblServicePassword.Location = new System.Drawing.Point(22, 123);
            this.lblServicePassword.Name = "lblServicePassword";
            this.lblServicePassword.Size = new System.Drawing.Size(169, 25);
            this.lblServicePassword.TabIndex = 13;
            this.lblServicePassword.Text = "Service Password";
            // 
            // lblServiceUsername
            // 
            this.lblServiceUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServiceUsername.AutoSize = true;
            this.lblServiceUsername.Location = new System.Drawing.Point(22, 39);
            this.lblServiceUsername.Name = "lblServiceUsername";
            this.lblServiceUsername.Size = new System.Drawing.Size(173, 25);
            this.lblServiceUsername.TabIndex = 12;
            this.lblServiceUsername.Text = "Service Username";
            // 
            // txtServicePassword
            // 
            this.txtServicePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServicePassword.Location = new System.Drawing.Point(22, 149);
            this.txtServicePassword.Name = "txtServicePassword";
            this.txtServicePassword.PasswordChar = '*';
            this.txtServicePassword.Size = new System.Drawing.Size(198, 29);
            this.txtServicePassword.TabIndex = 11;
            // 
            // txtServiceUsername
            // 
            this.txtServiceUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServiceUsername.Location = new System.Drawing.Point(22, 70);
            this.txtServiceUsername.Name = "txtServiceUsername";
            this.txtServiceUsername.Size = new System.Drawing.Size(198, 29);
            this.txtServiceUsername.TabIndex = 10;
            // 
            // btnRefreshSvcStatus
            // 
            this.btnRefreshSvcStatus.Location = new System.Drawing.Point(762, 47);
            this.btnRefreshSvcStatus.Name = "btnRefreshSvcStatus";
            this.btnRefreshSvcStatus.Size = new System.Drawing.Size(125, 49);
            this.btnRefreshSvcStatus.TabIndex = 51;
            this.btnRefreshSvcStatus.Text = "Refresh...";
            this.btnRefreshSvcStatus.UseVisualStyleBackColor = true;
            this.btnRefreshSvcStatus.Click += new System.EventHandler(this.btnRefreshSvcStatus_Click);
            // 
            // lblServiceLogs
            // 
            this.lblServiceLogs.AutoSize = true;
            this.lblServiceLogs.Location = new System.Drawing.Point(444, 112);
            this.lblServiceLogs.Name = "lblServiceLogs";
            this.lblServiceLogs.Size = new System.Drawing.Size(181, 25);
            this.lblServiceLogs.TabIndex = 8;
            this.lblServiceLogs.Text = "Service Install Logs";
            // 
            // txtServiceLog
            // 
            this.txtServiceLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServiceLog.Location = new System.Drawing.Point(449, 140);
            this.txtServiceLog.Multiline = true;
            this.txtServiceLog.Name = "txtServiceLog";
            this.txtServiceLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtServiceLog.Size = new System.Drawing.Size(476, 477);
            this.txtServiceLog.TabIndex = 7;
            // 
            // lblServiceStatusRes
            // 
            this.lblServiceStatusRes.AutoSize = true;
            this.lblServiceStatusRes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblServiceStatusRes.Location = new System.Drawing.Point(449, 51);
            this.lblServiceStatusRes.MinimumSize = new System.Drawing.Size(300, 2);
            this.lblServiceStatusRes.Name = "lblServiceStatusRes";
            this.lblServiceStatusRes.Padding = new System.Windows.Forms.Padding(4);
            this.lblServiceStatusRes.Size = new System.Drawing.Size(300, 35);
            this.lblServiceStatusRes.TabIndex = 50;
            // 
            // lblServiceStatus
            // 
            this.lblServiceStatus.AutoSize = true;
            this.lblServiceStatus.Location = new System.Drawing.Point(444, 17);
            this.lblServiceStatus.Name = "lblServiceStatus";
            this.lblServiceStatus.Size = new System.Drawing.Size(139, 25);
            this.lblServiceStatus.TabIndex = 4;
            this.lblServiceStatus.Text = "Service Status";
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(778, 728);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(173, 57);
            this.btnExit.TabIndex = 16;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblStatusInfo
            // 
            this.lblStatusInfo.AutoSize = true;
            this.lblStatusInfo.Location = new System.Drawing.Point(16, 728);
            this.lblStatusInfo.MinimumSize = new System.Drawing.Size(700, 60);
            this.lblStatusInfo.Name = "lblStatusInfo";
            this.lblStatusInfo.Size = new System.Drawing.Size(700, 60);
            this.lblStatusInfo.TabIndex = 17;
            // 
            // Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(986, 807);
            this.Controls.Add(this.lblStatusInfo);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.tabControl2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1010, 1200);
            this.MinimumSize = new System.Drawing.Size(1010, 871);
            this.Name = "Setup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AAD Complex Org Utility";
            this.tabControl2.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabMain.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabSvcConfig.ResumeLayout(false);
            this.tabSvcConfig.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabMain;
        private System.Windows.Forms.TextBox txtSiteUrl;
        private System.Windows.Forms.Label lblSiteUrl;
        private System.Windows.Forms.Button btnCheckConfig;
        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.TabPage tabSvcConfig;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblServiceLogs;
        private System.Windows.Forms.TextBox txtServiceLog;
        private System.Windows.Forms.Label lblServiceStatusRes;
        private System.Windows.Forms.Label lblServiceStatus;
        private System.Windows.Forms.Button btnRefreshSvcStatus;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnLoginAD;
        private System.Windows.Forms.MaskedTextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblLocalDomainList;
        private System.Windows.Forms.ListBox lstLocalDomainList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtDomainName;
        private System.Windows.Forms.Label lblDomainName;
        private System.Windows.Forms.ListBox lstRemoteDomainList;
        private System.Windows.Forms.Label lblRemoteDomainList;
        private System.Windows.Forms.CheckBox chkConfigConfirmed;
        private System.Windows.Forms.Button btnGoSvcConfig;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSiteType;
        private System.Windows.Forms.TextBox txtInstructions;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnUseADCredentials;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Label lblServicePassword;
        private System.Windows.Forms.Label lblServiceUsername;
        private System.Windows.Forms.TextBox txtServicePassword;
        private System.Windows.Forms.TextBox txtServiceUsername;
        private System.Windows.Forms.Label lblStatusInfo;
    }
}

