namespace ADSyncSiteSetup
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
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.lblApiKey = new System.Windows.Forms.Label();
            this.lstLocalDomainList = new System.Windows.Forms.ListBox();
            this.lblLocalDomainList = new System.Windows.Forms.Label();
            this.btnCheckConfig = new System.Windows.Forms.Button();
            this.lstRemoteDomainList = new System.Windows.Forms.ListBox();
            this.lblRemoteDomainList = new System.Windows.Forms.Label();
            this.lblSiteUrl = new System.Windows.Forms.Label();
            this.txtSiteUrl = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.MaskedTextBox();
            this.btnLoginAD = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(141, 23);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(677, 29);
            this.txtApiKey.TabIndex = 0;
            this.txtApiKey.TextChanged += new System.EventHandler(this.txtApiKey_TextChanged);
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(12, 23);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(123, 25);
            this.lblApiKey.TabIndex = 1;
            this.lblApiKey.Text = "Site API Key";
            // 
            // lstLocalDomainList
            // 
            this.lstLocalDomainList.FormattingEnabled = true;
            this.lstLocalDomainList.ItemHeight = 24;
            this.lstLocalDomainList.Location = new System.Drawing.Point(17, 439);
            this.lstLocalDomainList.Name = "lstLocalDomainList";
            this.lstLocalDomainList.Size = new System.Drawing.Size(415, 76);
            this.lstLocalDomainList.TabIndex = 2;
            // 
            // lblLocalDomainList
            // 
            this.lblLocalDomainList.AutoSize = true;
            this.lblLocalDomainList.Location = new System.Drawing.Point(12, 411);
            this.lblLocalDomainList.Name = "lblLocalDomainList";
            this.lblLocalDomainList.Size = new System.Drawing.Size(166, 25);
            this.lblLocalDomainList.TabIndex = 3;
            this.lblLocalDomainList.Text = "Local Domain List";
            // 
            // btnCheckConfig
            // 
            this.btnCheckConfig.Location = new System.Drawing.Point(834, 40);
            this.btnCheckConfig.Name = "btnCheckConfig";
            this.btnCheckConfig.Size = new System.Drawing.Size(173, 44);
            this.btnCheckConfig.TabIndex = 4;
            this.btnCheckConfig.Text = "Check Config";
            this.btnCheckConfig.UseVisualStyleBackColor = true;
            this.btnCheckConfig.Click += new System.EventHandler(this.btnCheckConfig_Click);
            // 
            // lstRemoteDomainList
            // 
            this.lstRemoteDomainList.FormattingEnabled = true;
            this.lstRemoteDomainList.ItemHeight = 24;
            this.lstRemoteDomainList.Location = new System.Drawing.Point(17, 136);
            this.lstRemoteDomainList.Name = "lstRemoteDomainList";
            this.lstRemoteDomainList.Size = new System.Drawing.Size(415, 76);
            this.lstRemoteDomainList.TabIndex = 5;
            // 
            // lblRemoteDomainList
            // 
            this.lblRemoteDomainList.AutoSize = true;
            this.lblRemoteDomainList.Location = new System.Drawing.Point(12, 108);
            this.lblRemoteDomainList.Name = "lblRemoteDomainList";
            this.lblRemoteDomainList.Size = new System.Drawing.Size(186, 25);
            this.lblRemoteDomainList.TabIndex = 6;
            this.lblRemoteDomainList.Text = "Remote Domain List";
            // 
            // lblSiteUrl
            // 
            this.lblSiteUrl.AutoSize = true;
            this.lblSiteUrl.Location = new System.Drawing.Point(12, 59);
            this.lblSiteUrl.Name = "lblSiteUrl";
            this.lblSiteUrl.Size = new System.Drawing.Size(89, 25);
            this.lblSiteUrl.TabIndex = 7;
            this.lblSiteUrl.Text = "Site URL";
            // 
            // txtSiteUrl
            // 
            this.txtSiteUrl.Location = new System.Drawing.Point(141, 59);
            this.txtSiteUrl.Name = "txtSiteUrl";
            this.txtSiteUrl.Size = new System.Drawing.Size(677, 29);
            this.txtSiteUrl.TabIndex = 8;
            this.txtSiteUrl.TextChanged += new System.EventHandler(this.txtSiteUrl_TextChanged);
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(141, 310);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(291, 29);
            this.txtUsername.TabIndex = 9;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(17, 310);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(102, 25);
            this.lblUsername.TabIndex = 11;
            this.lblUsername.Text = "Username";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(17, 346);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(98, 25);
            this.lblPassword.TabIndex = 12;
            this.lblPassword.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(141, 346);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(291, 29);
            this.txtPassword.TabIndex = 13;
            // 
            // btnLoginAD
            // 
            this.btnLoginAD.Location = new System.Drawing.Point(452, 327);
            this.btnLoginAD.Name = "btnLoginAD";
            this.btnLoginAD.Size = new System.Drawing.Size(173, 44);
            this.btnLoginAD.TabIndex = 14;
            this.btnLoginAD.Text = "Login to AD";
            this.btnLoginAD.UseVisualStyleBackColor = true;
            this.btnLoginAD.Click += new System.EventHandler(this.btnLoginAD_Click);
            // 
            // Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 544);
            this.Controls.Add(this.btnLoginAD);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtSiteUrl);
            this.Controls.Add(this.lblSiteUrl);
            this.Controls.Add(this.lblRemoteDomainList);
            this.Controls.Add(this.lstRemoteDomainList);
            this.Controls.Add(this.btnCheckConfig);
            this.Controls.Add(this.lblLocalDomainList);
            this.Controls.Add(this.lstLocalDomainList);
            this.Controls.Add(this.lblApiKey);
            this.Controls.Add(this.txtApiKey);
            this.Name = "Setup";
            this.Text = "AAD Complex Org Utility";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.ListBox lstLocalDomainList;
        private System.Windows.Forms.Label lblLocalDomainList;
        private System.Windows.Forms.Button btnCheckConfig;
        private System.Windows.Forms.ListBox lstRemoteDomainList;
        private System.Windows.Forms.Label lblRemoteDomainList;
        private System.Windows.Forms.Label lblSiteUrl;
        private System.Windows.Forms.TextBox txtSiteUrl;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.MaskedTextBox txtPassword;
        private System.Windows.Forms.Button btnLoginAD;
    }
}

