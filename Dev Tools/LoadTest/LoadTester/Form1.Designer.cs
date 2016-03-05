namespace LoadTester
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.CustomLabel customLabel1 = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
            System.Windows.Forms.DataVisualization.Charting.CustomLabel customLabel2 = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStart = new System.Windows.Forms.Button();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.tbClientCount = new System.Windows.Forms.TextBox();
            this.lblClientCount = new System.Windows.Forms.Label();
            this.tbRequestCount = new System.Windows.Forms.TextBox();
            this.lblRequestCount = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cbDownloadHeaderSrcElements = new System.Windows.Forms.CheckBox();
            this.cbDownloadBodySrcElements = new System.Windows.Forms.CheckBox();
            this.tbStats = new System.Windows.Forms.TextBox();
            this.lblThreadCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbRequestsDelay = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 144);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(159, 27);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // tbUrl
            // 
            this.tbUrl.Location = new System.Drawing.Point(12, 29);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(280, 22);
            this.tbUrl.TabIndex = 1;
            this.tbUrl.Text = "http://ccvdev.ccvonline.com/easter16";
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(9, 10);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(28, 15);
            this.lblURL.TabIndex = 2;
            this.lblURL.Text = "URL";
            // 
            // tbClientCount
            // 
            this.tbClientCount.Location = new System.Drawing.Point(12, 72);
            this.tbClientCount.Name = "tbClientCount";
            this.tbClientCount.Size = new System.Drawing.Size(45, 22);
            this.tbClientCount.TabIndex = 3;
            this.tbClientCount.Text = "100";
            // 
            // lblClientCount
            // 
            this.lblClientCount.AutoSize = true;
            this.lblClientCount.Location = new System.Drawing.Point(9, 54);
            this.lblClientCount.Name = "lblClientCount";
            this.lblClientCount.Size = new System.Drawing.Size(72, 15);
            this.lblClientCount.TabIndex = 4;
            this.lblClientCount.Text = "Client Count";
            // 
            // tbRequestCount
            // 
            this.tbRequestCount.Location = new System.Drawing.Point(12, 116);
            this.tbRequestCount.Name = "tbRequestCount";
            this.tbRequestCount.Size = new System.Drawing.Size(45, 22);
            this.tbRequestCount.TabIndex = 5;
            this.tbRequestCount.Text = "10";
            // 
            // lblRequestCount
            // 
            this.lblRequestCount.AutoSize = true;
            this.lblRequestCount.Location = new System.Drawing.Point(9, 97);
            this.lblRequestCount.Name = "lblRequestCount";
            this.lblRequestCount.Size = new System.Drawing.Size(88, 15);
            this.lblRequestCount.TabIndex = 6;
            this.lblRequestCount.Text = "Requests/Client";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 178);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(960, 10);
            this.progressBar1.TabIndex = 9;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Area3DStyle.Inclination = 45;
            chartArea1.Area3DStyle.LightStyle = System.Windows.Forms.DataVisualization.Charting.LightStyle.Realistic;
            customLabel1.GridTicks = System.Windows.Forms.DataVisualization.Charting.GridTickTypes.TickMark;
            customLabel1.Text = "@0ms";
            customLabel1.ToPosition = 10D;
            customLabel2.FromPosition = 10D;
            customLabel2.Text = "b";
            customLabel2.ToPosition = 20D;
            chartArea1.AxisX.CustomLabels.Add(customLabel1);
            chartArea1.AxisX.CustomLabels.Add(customLabel2);
            chartArea1.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
            chartArea1.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 194);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Grayscale;
            this.chart1.Size = new System.Drawing.Size(960, 386);
            this.chart1.TabIndex = 11;
            this.chart1.Text = "chart1";
            // 
            // cbDownloadHeaderSrcElements
            // 
            this.cbDownloadHeaderSrcElements.AutoSize = true;
            this.cbDownloadHeaderSrcElements.Checked = true;
            this.cbDownloadHeaderSrcElements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDownloadHeaderSrcElements.Location = new System.Drawing.Point(514, 73);
            this.cbDownloadHeaderSrcElements.Name = "cbDownloadHeaderSrcElements";
            this.cbDownloadHeaderSrcElements.Size = new System.Drawing.Size(188, 19);
            this.cbDownloadHeaderSrcElements.TabIndex = 12;
            this.cbDownloadHeaderSrcElements.Text = "Download Header Src Elements";
            this.cbDownloadHeaderSrcElements.UseVisualStyleBackColor = true;
            // 
            // cbDownloadBodySrcElements
            // 
            this.cbDownloadBodySrcElements.AutoSize = true;
            this.cbDownloadBodySrcElements.Checked = true;
            this.cbDownloadBodySrcElements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbDownloadBodySrcElements.Location = new System.Drawing.Point(514, 97);
            this.cbDownloadBodySrcElements.Name = "cbDownloadBodySrcElements";
            this.cbDownloadBodySrcElements.Size = new System.Drawing.Size(178, 19);
            this.cbDownloadBodySrcElements.TabIndex = 13;
            this.cbDownloadBodySrcElements.Text = "Download Body Src Elements";
            this.cbDownloadBodySrcElements.UseVisualStyleBackColor = true;
            // 
            // tbStats
            // 
            this.tbStats.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbStats.Location = new System.Drawing.Point(727, 16);
            this.tbStats.Multiline = true;
            this.tbStats.Name = "tbStats";
            this.tbStats.Size = new System.Drawing.Size(245, 156);
            this.tbStats.TabIndex = 14;
            // 
            // lblThreadCount
            // 
            this.lblThreadCount.AutoSize = true;
            this.lblThreadCount.Font = new System.Drawing.Font("Open Sans", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreadCount.Location = new System.Drawing.Point(563, 145);
            this.lblThreadCount.Name = "lblThreadCount";
            this.lblThreadCount.Size = new System.Drawing.Size(158, 26);
            this.lblThreadCount.TabIndex = 15;
            this.lblThreadCount.Text = "lblThreadCount";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(125, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 15);
            this.label1.TabIndex = 17;
            this.label1.Text = "Requests Delay (ms)";
            // 
            // tbRequestsDelay
            // 
            this.tbRequestsDelay.Location = new System.Drawing.Point(128, 73);
            this.tbRequestsDelay.Name = "tbRequestsDelay";
            this.tbRequestsDelay.Size = new System.Drawing.Size(45, 22);
            this.tbRequestsDelay.TabIndex = 16;
            this.tbRequestsDelay.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 592);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbRequestsDelay);
            this.Controls.Add(this.lblThreadCount);
            this.Controls.Add(this.tbStats);
            this.Controls.Add(this.cbDownloadBodySrcElements);
            this.Controls.Add(this.cbDownloadHeaderSrcElements);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblRequestCount);
            this.Controls.Add(this.tbRequestCount);
            this.Controls.Add(this.lblClientCount);
            this.Controls.Add(this.tbClientCount);
            this.Controls.Add(this.lblURL);
            this.Controls.Add(this.tbUrl);
            this.Controls.Add(this.btnStart);
            this.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Load Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TextBox tbClientCount;
        private System.Windows.Forms.Label lblClientCount;
        private System.Windows.Forms.TextBox tbRequestCount;
        private System.Windows.Forms.Label lblRequestCount;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.CheckBox cbDownloadHeaderSrcElements;
        private System.Windows.Forms.CheckBox cbDownloadBodySrcElements;
        private System.Windows.Forms.TextBox tbStats;
        private System.Windows.Forms.Label lblThreadCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbRequestsDelay;
    }
}

