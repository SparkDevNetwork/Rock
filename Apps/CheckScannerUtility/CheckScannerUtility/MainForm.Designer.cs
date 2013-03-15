namespace Rock.CheckScannerUtility
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.grpScannerStatus = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboImageOption = new System.Windows.Forms.ComboBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.shapeStatus = new Microsoft.VisualBasic.PowerPacks.OvalShape();
            this.RangerScanner = new AxRANGERLib.AxRanger();
            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnScanAction = new System.Windows.Forms.Button();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.pbxFront = new System.Windows.Forms.PictureBox();
            this.pbxBack = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnUpload = new System.Windows.Forms.Button();
            this.txtRockURL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.grpScannerStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RangerScanner)).BeginInit();
            this.grpActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxFront)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxBack)).BeginInit();
            this.SuspendLayout();
            // 
            // grpScannerStatus
            // 
            this.grpScannerStatus.Controls.Add(this.label3);
            this.grpScannerStatus.Controls.Add(this.cboImageOption);
            this.grpScannerStatus.Controls.Add(this.txtStatus);
            this.grpScannerStatus.Controls.Add(this.lblStatus);
            this.grpScannerStatus.Controls.Add(this.shapeContainer2);
            this.grpScannerStatus.Location = new System.Drawing.Point(111, 12);
            this.grpScannerStatus.Name = "grpScannerStatus";
            this.grpScannerStatus.Size = new System.Drawing.Size(332, 91);
            this.grpScannerStatus.TabIndex = 1;
            this.grpScannerStatus.TabStop = false;
            this.grpScannerStatus.Text = "Scanner Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Image Option";
            // 
            // cboImageOption
            // 
            this.cboImageOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageOption.FormattingEnabled = true;
            this.cboImageOption.Location = new System.Drawing.Point(94, 64);
            this.cboImageOption.Name = "cboImageOption";
            this.cboImageOption.Size = new System.Drawing.Size(148, 21);
            this.cboImageOption.TabIndex = 4;
            this.cboImageOption.SelectedIndexChanged += new System.EventHandler(this.cboImageOption_SelectedIndexChanged);
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(94, 23);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(148, 21);
            this.txtStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(16, 26);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(38, 13);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Status";
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(3, 17);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.shapeStatus});
            this.shapeContainer2.Size = new System.Drawing.Size(326, 71);
            this.shapeContainer2.TabIndex = 2;
            this.shapeContainer2.TabStop = false;
            // 
            // shapeStatus
            // 
            this.shapeStatus.BackColor = System.Drawing.Color.Red;
            this.shapeStatus.BackStyle = Microsoft.VisualBasic.PowerPacks.BackStyle.Opaque;
            this.shapeStatus.Location = new System.Drawing.Point(56, 9);
            this.shapeStatus.Name = "shapeStatus";
            this.shapeStatus.Size = new System.Drawing.Size(14, 14);
            // 
            // RangerScanner
            // 
            this.RangerScanner.Enabled = true;
            this.RangerScanner.Location = new System.Drawing.Point(656, 371);
            this.RangerScanner.Name = "RangerScanner";
            this.RangerScanner.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("RangerScanner.OcxState")));
            this.RangerScanner.Size = new System.Drawing.Size(96, 93);
            this.RangerScanner.TabIndex = 0;
            this.RangerScanner.TransportNewState += new AxRANGERLib._DRangerEvents_TransportNewStateEventHandler(this.RangerScanner_TransportNewState);
            this.RangerScanner.TransportChangeOptionsState += new AxRANGERLib._DRangerEvents_TransportChangeOptionsStateEventHandler(this.RangerScanner_TransportChangeOptionsState);
            this.RangerScanner.TransportFeedingStopped += new AxRANGERLib._DRangerEvents_TransportFeedingStoppedEventHandler(this.RangerScanner_TransportFeedingStopped);
            this.RangerScanner.TransportNewItem += new System.EventHandler(this.RangerScanner_TransportNewItem);
            this.RangerScanner.TransportSetItemOutput += new AxRANGERLib._DRangerEvents_TransportSetItemOutputEventHandler(this.RangerScanner_TransportSetItemOutput);
            this.RangerScanner.TransportItemInPocket += new AxRANGERLib._DRangerEvents_TransportItemInPocketEventHandler(this.RangerScanner_TransportItemInPocket);
            // 
            // grpActions
            // 
            this.grpActions.Controls.Add(this.btnScanAction);
            this.grpActions.Location = new System.Drawing.Point(12, 12);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new System.Drawing.Size(93, 91);
            this.grpActions.TabIndex = 2;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "Actions";
            // 
            // btnScanAction
            // 
            this.btnScanAction.Location = new System.Drawing.Point(6, 21);
            this.btnScanAction.Name = "btnScanAction";
            this.btnScanAction.Size = new System.Drawing.Size(75, 23);
            this.btnScanAction.TabIndex = 1;
            this.btnScanAction.Text = "Start";
            this.btnScanAction.UseVisualStyleBackColor = true;
            this.btnScanAction.Click += new System.EventHandler(this.btnScanAction_Click);
            // 
            // lstLog
            // 
            this.lstLog.FormattingEnabled = true;
            this.lstLog.Location = new System.Drawing.Point(12, 109);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(431, 355);
            this.lstLog.TabIndex = 3;
            // 
            // pbxFront
            // 
            this.pbxFront.ErrorImage = null;
            this.pbxFront.InitialImage = null;
            this.pbxFront.Location = new System.Drawing.Point(452, 33);
            this.pbxFront.Name = "pbxFront";
            this.pbxFront.Size = new System.Drawing.Size(300, 100);
            this.pbxFront.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxFront.TabIndex = 4;
            this.pbxFront.TabStop = false;
            // 
            // pbxBack
            // 
            this.pbxBack.ErrorImage = null;
            this.pbxBack.InitialImage = null;
            this.pbxBack.Location = new System.Drawing.Point(452, 162);
            this.pbxBack.Name = "pbxBack";
            this.pbxBack.Size = new System.Drawing.Size(300, 100);
            this.pbxBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxBack.TabIndex = 5;
            this.pbxBack.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(449, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Front";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(449, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Back";
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(452, 330);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(109, 31);
            this.btnUpload.TabIndex = 8;
            this.btnUpload.Text = "Upload to Rock";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // txtRockURL
            // 
            this.txtRockURL.Location = new System.Drawing.Point(452, 303);
            this.txtRockURL.Name = "txtRockURL";
            this.txtRockURL.Size = new System.Drawing.Size(265, 21);
            this.txtRockURL.TabIndex = 9;
            this.txtRockURL.Text = "http://localhost:6229";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(449, 287);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Rock URL";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 481);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtRockURL);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbxBack);
            this.Controls.Add(this.pbxFront);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.grpActions);
            this.Controls.Add(this.grpScannerStatus);
            this.Controls.Add(this.RangerScanner);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(780, 520);
            this.MinimumSize = new System.Drawing.Size(780, 520);
            this.Name = "MainForm";
            this.Text = "Rock Check Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.grpScannerStatus.ResumeLayout(false);
            this.grpScannerStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RangerScanner)).EndInit();
            this.grpActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxFront)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxBack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxRANGERLib.AxRanger RangerScanner;
        private System.Windows.Forms.GroupBox grpScannerStatus;
        private System.Windows.Forms.Label lblStatus;
        private Microsoft.VisualBasic.PowerPacks.OvalShape shapeStatus;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.Button btnScanAction;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.PictureBox pbxFront;
        private System.Windows.Forms.PictureBox pbxBack;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboImageOption;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.TextBox txtRockURL;
        private System.Windows.Forms.Label label4;
    }
}