//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
            this.label3 = new System.Windows.Forms.Label();
            this.cboImageOption = new System.Windows.Forms.ComboBox();
            this.shapeStatus = new Microsoft.VisualBasic.PowerPacks.OvalShape();
            this.btnConnect = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnUpload = new System.Windows.Forms.Button();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.RangerScanner = new AxRANGERLib.AxRanger();
            this.dataRepeater1 = new Microsoft.VisualBasic.PowerPacks.DataRepeater();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblBatchDate = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.button1 = new System.Windows.Forms.Button();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RangerScanner)).BeginInit();
            this.dataRepeater1.ItemTemplate.SuspendLayout();
            this.dataRepeater1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(440, 88);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 22);
            this.label3.TabIndex = 5;
            this.label3.Text = "Image Option";
            // 
            // cboImageOption
            // 
            this.cboImageOption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboImageOption.FormattingEnabled = true;
            this.cboImageOption.Location = new System.Drawing.Point(568, 80);
            this.cboImageOption.Margin = new System.Windows.Forms.Padding(5);
            this.cboImageOption.Name = "cboImageOption";
            this.cboImageOption.Size = new System.Drawing.Size(220, 30);
            this.cboImageOption.TabIndex = 4;
            this.cboImageOption.SelectedIndexChanged += new System.EventHandler(this.cboImageOption_SelectedIndexChanged);
            // 
            // shapeStatus
            // 
            this.shapeStatus.BackColor = System.Drawing.Color.Red;
            this.shapeStatus.BackStyle = Microsoft.VisualBasic.PowerPacks.BackStyle.Opaque;
            this.shapeStatus.Location = new System.Drawing.Point(1160, 16);
            this.shapeStatus.Name = "shapeStatus";
            this.shapeStatus.Size = new System.Drawing.Size(14, 14);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(8, 8);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(113, 32);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Start";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnScanAction_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(568, 504);
            this.btnUpload.Margin = new System.Windows.Forms.Padding(5);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(144, 32);
            this.btnUpload.TabIndex = 8;
            this.btnUpload.Text = "Upload to Rock";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnConnect);
            this.pnlTop.Controls.Add(this.shapeContainer1);
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1184, 48);
            this.pnlTop.TabIndex = 11;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.shapeStatus});
            this.shapeContainer1.Size = new System.Drawing.Size(1184, 48);
            this.shapeContainer1.TabIndex = 2;
            this.shapeContainer1.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(568, 472);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(144, 23);
            this.progressBar1.TabIndex = 12;
            // 
            // RangerScanner
            // 
            this.RangerScanner.Enabled = true;
            this.RangerScanner.Location = new System.Drawing.Point(1052, 144);
            this.RangerScanner.Margin = new System.Windows.Forms.Padding(5);
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
            // dataRepeater1
            // 
            // 
            // dataRepeater1.ItemTemplate
            // 
            this.dataRepeater1.ItemTemplate.Controls.Add(this.lblStatus);
            this.dataRepeater1.ItemTemplate.Controls.Add(this.lblName);
            this.dataRepeater1.ItemTemplate.Controls.Add(this.lblBatchDate);
            this.dataRepeater1.ItemTemplate.Size = new System.Drawing.Size(312, 69);
            this.dataRepeater1.Location = new System.Drawing.Point(0, 104);
            this.dataRepeater1.Name = "dataRepeater1";
            this.dataRepeater1.Size = new System.Drawing.Size(320, 520);
            this.dataRepeater1.TabIndex = 13;
            this.dataRepeater1.Text = "dataRepeater1";
            this.dataRepeater1.DrawItem += new Microsoft.VisualBasic.PowerPacks.DataRepeaterItemEventHandler(this.dataRepeater1_DrawItem);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(160, 8);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(117, 22);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status: Closed";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(8, 32);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(133, 22);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "Name: Batch001";
            // 
            // lblBatchDate
            // 
            this.lblBatchDate.AutoSize = true;
            this.lblBatchDate.Location = new System.Drawing.Point(8, 8);
            this.lblBatchDate.Name = "lblBatchDate";
            this.lblBatchDate.Size = new System.Drawing.Size(138, 22);
            this.lblBatchDate.TabIndex = 0;
            this.lblBatchDate.Text = "Date: 01/01/1901";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(80, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 14;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1186, 667);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataRepeater1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cboImageOption);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.RangerScanner);
            this.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(780, 627);
            this.Name = "MainForm";
            this.Text = "Rock Check Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnlTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RangerScanner)).EndInit();
            this.dataRepeater1.ItemTemplate.ResumeLayout(false);
            this.dataRepeater1.ItemTemplate.PerformLayout();
            this.dataRepeater1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private AxRANGERLib.AxRanger RangerScanner;
        private Microsoft.VisualBasic.PowerPacks.OvalShape shapeStatus;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboImageOption;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.Panel pnlTop;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private Microsoft.VisualBasic.PowerPacks.DataRepeater dataRepeater1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblBatchDate;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button button1;
    }
}