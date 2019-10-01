﻿namespace Rock.CodeGeneration
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.cblModels = new System.Windows.Forms.CheckedListBox();
            this.cbSelectAll = new System.Windows.Forms.CheckBox();
            this.fbdServiceOutput = new System.Windows.Forms.FolderBrowserDialog();
            this.cbRest = new System.Windows.Forms.CheckBox();
            this.cbService = new System.Windows.Forms.CheckBox();
            this.ofdAssembly = new System.Windows.Forms.OpenFileDialog();
            this.fbdRestOutput = new System.Windows.Forms.FolderBrowserDialog();
            this.cbClient = new System.Windows.Forms.CheckBox();
            this.lblAssemblyPath = new System.Windows.Forms.Label();
            this.lblAssembly = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tbServiceFolder = new System.Windows.Forms.TextBox();
            this.tbRestFolder = new System.Windows.Forms.TextBox();
            this.tbClientFolder = new System.Windows.Forms.TextBox();
            this.fdbRockClient = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblAssemblyDateTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.tbDatabaseFolder = new System.Windows.Forms.TextBox();
            this.cbDatabaseProcs = new System.Windows.Forms.CheckBox();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.cbGenerateObsoleteExport = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(13, 12);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenerate.Location = new System.Drawing.Point(14, 448);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // cblModels
            // 
            this.cblModels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cblModels.FormattingEnabled = true;
            this.cblModels.Location = new System.Drawing.Point(15, 70);
            this.cblModels.Name = "cblModels";
            this.cblModels.Size = new System.Drawing.Size(570, 260);
            this.cblModels.TabIndex = 3;
            // 
            // cbSelectAll
            // 
            this.cbSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSelectAll.AutoSize = true;
            this.cbSelectAll.Location = new System.Drawing.Point(516, 19);
            this.cbSelectAll.Name = "cbSelectAll";
            this.cbSelectAll.Size = new System.Drawing.Size(69, 17);
            this.cbSelectAll.TabIndex = 4;
            this.cbSelectAll.Text = "Select All";
            this.cbSelectAll.UseVisualStyleBackColor = true;
            this.cbSelectAll.CheckedChanged += new System.EventHandler(this.cbSelectAll_CheckedChanged);
            // 
            // fbdServiceOutput
            // 
            this.fbdServiceOutput.Description = "Select the project folder that the Service files should be added to.  The namespa" +
    "ce of the objects will be used to create a relative folder path if neccessary.";
            this.fbdServiceOutput.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.fbdServiceOutput.ShowNewFolderButton = false;
            // 
            // cbRest
            // 
            this.cbRest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbRest.AutoSize = true;
            this.cbRest.Checked = true;
            this.cbRest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRest.Location = new System.Drawing.Point(15, 367);
            this.cbRest.Name = "cbRest";
            this.cbRest.Size = new System.Drawing.Size(48, 17);
            this.cbRest.TabIndex = 6;
            this.cbRest.Text = "Rest";
            this.cbRest.UseVisualStyleBackColor = true;
            // 
            // cbService
            // 
            this.cbService.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbService.AutoSize = true;
            this.cbService.Checked = true;
            this.cbService.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbService.Location = new System.Drawing.Point(15, 340);
            this.cbService.Name = "cbService";
            this.cbService.Size = new System.Drawing.Size(61, 17);
            this.cbService.TabIndex = 7;
            this.cbService.Text = "Service";
            this.cbService.UseVisualStyleBackColor = true;
            // 
            // ofdAssembly
            // 
            this.ofdAssembly.FileName = "openFileDialog1";
            this.ofdAssembly.Title = "Assembly";
            // 
            // fbdRestOutput
            // 
            this.fbdRestOutput.Description = "Select the project folder that the Rest files should be added to.  The namespace " +
    "of the objects will be used to create a relative folder path if neccessary.";
            this.fbdRestOutput.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.fbdRestOutput.ShowNewFolderButton = false;
            // 
            // cbClient
            // 
            this.cbClient.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbClient.AutoSize = true;
            this.cbClient.Checked = true;
            this.cbClient.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbClient.Location = new System.Drawing.Point(15, 394);
            this.cbClient.Name = "cbClient";
            this.cbClient.Size = new System.Drawing.Size(53, 17);
            this.cbClient.TabIndex = 8;
            this.cbClient.Text = "Client";
            this.cbClient.UseVisualStyleBackColor = true;
            // 
            // lblAssemblyPath
            // 
            this.lblAssemblyPath.AutoSize = true;
            this.lblAssemblyPath.Location = new System.Drawing.Point(66, 38);
            this.lblAssemblyPath.Name = "lblAssemblyPath";
            this.lblAssemblyPath.Size = new System.Drawing.Size(31, 13);
            this.lblAssemblyPath.TabIndex = 9;
            this.lblAssemblyPath.Text = "none";
            // 
            // lblAssembly
            // 
            this.lblAssembly.AutoSize = true;
            this.lblAssembly.Location = new System.Drawing.Point(12, 38);
            this.lblAssembly.Name = "lblAssembly";
            this.lblAssembly.Size = new System.Drawing.Size(56, 13);
            this.lblAssembly.TabIndex = 10;
            this.lblAssembly.Text = "Assembly:";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(375, 452);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(212, 23);
            this.progressBar1.TabIndex = 11;
            this.progressBar1.Visible = false;
            // 
            // tbServiceFolder
            // 
            this.tbServiceFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbServiceFolder.Location = new System.Drawing.Point(122, 338);
            this.tbServiceFolder.Name = "tbServiceFolder";
            this.tbServiceFolder.Size = new System.Drawing.Size(465, 21);
            this.tbServiceFolder.TabIndex = 12;
            this.tbServiceFolder.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbServiceFolder_MouseDoubleClick);
            // 
            // tbRestFolder
            // 
            this.tbRestFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRestFolder.Location = new System.Drawing.Point(122, 365);
            this.tbRestFolder.Name = "tbRestFolder";
            this.tbRestFolder.Size = new System.Drawing.Size(465, 21);
            this.tbRestFolder.TabIndex = 13;
            this.tbRestFolder.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbRestFolder_MouseDoubleClick);
            // 
            // tbClientFolder
            // 
            this.tbClientFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbClientFolder.Location = new System.Drawing.Point(122, 392);
            this.tbClientFolder.Name = "tbClientFolder";
            this.tbClientFolder.Size = new System.Drawing.Size(465, 21);
            this.tbClientFolder.TabIndex = 14;
            this.tbClientFolder.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbClientFolder_MouseDoubleClick);
            // 
            // fdbRockClient
            // 
            this.fdbRockClient.Description = "Select folder for Rock.Client ";
            this.fdbRockClient.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.fdbRockClient.ShowNewFolderButton = false;
            // 
            // lblAssemblyDateTime
            // 
            this.lblAssemblyDateTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAssemblyDateTime.Location = new System.Drawing.Point(402, 38);
            this.lblAssemblyDateTime.Name = "lblAssemblyDateTime";
            this.lblAssemblyDateTime.Size = new System.Drawing.Size(183, 13);
            this.lblAssemblyDateTime.TabIndex = 15;
            this.lblAssemblyDateTime.Text = "unknown";
            this.lblAssemblyDateTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Database:";
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(66, 54);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(31, 13);
            this.lblDatabase.TabIndex = 17;
            this.lblDatabase.Text = "none";
            // 
            // tbDatabaseFolder
            // 
            this.tbDatabaseFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDatabaseFolder.Location = new System.Drawing.Point(122, 419);
            this.tbDatabaseFolder.Name = "tbDatabaseFolder";
            this.tbDatabaseFolder.Size = new System.Drawing.Size(465, 21);
            this.tbDatabaseFolder.TabIndex = 19;
            // 
            // cbDatabaseProcs
            // 
            this.cbDatabaseProcs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDatabaseProcs.AutoSize = true;
            this.cbDatabaseProcs.Checked = true;
            this.cbDatabaseProcs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDatabaseProcs.Location = new System.Drawing.Point(15, 421);
            this.cbDatabaseProcs.Name = "cbDatabaseProcs";
            this.cbDatabaseProcs.Size = new System.Drawing.Size(101, 17);
            this.cbDatabaseProcs.TabIndex = 18;
            this.cbDatabaseProcs.Text = "Database Procs";
            this.cbDatabaseProcs.UseVisualStyleBackColor = true;
            // 
            // tbResults
            // 
            this.tbResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbResults.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbResults.Location = new System.Drawing.Point(11, 504);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ReadOnly = true;
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(574, 199);
            this.tbResults.TabIndex = 20;
            // 
            // cbGenerateObsoleteExport
            // 
            this.cbGenerateObsoleteExport.AutoSize = true;
            this.cbGenerateObsoleteExport.Location = new System.Drawing.Point(11, 481);
            this.cbGenerateObsoleteExport.Name = "cbGenerateObsoleteExport";
            this.cbGenerateObsoleteExport.Size = new System.Drawing.Size(105, 17);
            this.cbGenerateObsoleteExport.TabIndex = 21;
            this.cbGenerateObsoleteExport.Text = "Report Obsolete";
            this.cbGenerateObsoleteExport.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 729);
            this.Controls.Add(this.cbGenerateObsoleteExport);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.tbDatabaseFolder);
            this.Controls.Add(this.cbDatabaseProcs);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblAssemblyDateTime);
            this.Controls.Add(this.tbClientFolder);
            this.Controls.Add(this.tbRestFolder);
            this.Controls.Add(this.tbServiceFolder);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblAssembly);
            this.Controls.Add(this.lblAssemblyPath);
            this.Controls.Add(this.cbClient);
            this.Controls.Add(this.cbService);
            this.Controls.Add(this.cbRest);
            this.Controls.Add(this.cbSelectAll);
            this.Controls.Add(this.cblModels);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnLoad);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "Form1";
            this.Text = "Rock Code Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.CheckedListBox cblModels;
        private System.Windows.Forms.CheckBox cbSelectAll;
        private System.Windows.Forms.FolderBrowserDialog fbdServiceOutput;
        private System.Windows.Forms.CheckBox cbRest;
        private System.Windows.Forms.CheckBox cbService;
		private System.Windows.Forms.OpenFileDialog ofdAssembly;
        private System.Windows.Forms.FolderBrowserDialog fbdRestOutput;
        private System.Windows.Forms.CheckBox cbClient;
        private System.Windows.Forms.Label lblAssemblyPath;
        private System.Windows.Forms.Label lblAssembly;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox tbServiceFolder;
        private System.Windows.Forms.TextBox tbRestFolder;
        private System.Windows.Forms.TextBox tbClientFolder;
        private System.Windows.Forms.FolderBrowserDialog fdbRockClient;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblAssemblyDateTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox tbDatabaseFolder;
        private System.Windows.Forms.CheckBox cbDatabaseProcs;
        private System.Windows.Forms.TextBox tbResults;
        private System.Windows.Forms.CheckBox cbGenerateObsoleteExport;
    }
}

