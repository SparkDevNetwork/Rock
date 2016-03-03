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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.btnStart = new System.Windows.Forms.Button();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.tbClientCount = new System.Windows.Forms.TextBox();
            this.lblClientCount = new System.Windows.Forms.Label();
            this.tbRequestCount = new System.Windows.Forms.TextBox();
            this.lblRequestCount = new System.Windows.Forms.Label();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.tbStats = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblThreadCount = new System.Windows.Forms.Label();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 150);
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
            this.tbUrl.TextChanged += new System.EventHandler(this.tbUrl_TextChanged);
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
            this.tbClientCount.Size = new System.Drawing.Size(100, 22);
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
            this.tbRequestCount.Size = new System.Drawing.Size(202, 22);
            this.tbRequestCount.TabIndex = 5;
            this.tbRequestCount.Text = "10";
            // 
            // lblRequestCount
            // 
            this.lblRequestCount.AutoSize = true;
            this.lblRequestCount.Location = new System.Drawing.Point(9, 97);
            this.lblRequestCount.Name = "lblRequestCount";
            this.lblRequestCount.Size = new System.Drawing.Size(141, 15);
            this.lblRequestCount.TabIndex = 6;
            this.lblRequestCount.Text = "Request Count (per client)";
            // 
            // tbResults
            // 
            this.tbResults.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbResults.Location = new System.Drawing.Point(589, 15);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(238, 132);
            this.tbResults.TabIndex = 7;
            // 
            // tbStats
            // 
            this.tbStats.Location = new System.Drawing.Point(346, 13);
            this.tbStats.Multiline = true;
            this.tbStats.Name = "tbStats";
            this.tbStats.Size = new System.Drawing.Size(237, 134);
            this.tbStats.TabIndex = 8;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(346, 150);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(481, 27);
            this.progressBar1.TabIndex = 9;
            // 
            // lblThreadCount
            // 
            this.lblThreadCount.AutoSize = true;
            this.lblThreadCount.Location = new System.Drawing.Point(302, 162);
            this.lblThreadCount.Name = "lblThreadCount";
            this.lblThreadCount.Size = new System.Drawing.Size(38, 15);
            this.lblThreadCount.TabIndex = 10;
            this.lblThreadCount.Text = "label1";
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 194);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(960, 386);
            this.chart1.TabIndex = 11;
            this.chart1.Text = "chart1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 592);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.lblThreadCount);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.tbStats);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.lblRequestCount);
            this.Controls.Add(this.tbRequestCount);
            this.Controls.Add(this.lblClientCount);
            this.Controls.Add(this.tbClientCount);
            this.Controls.Add(this.lblURL);
            this.Controls.Add(this.tbUrl);
            this.Controls.Add(this.btnStart);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "Form1";
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
        private System.Windows.Forms.TextBox tbResults;
        private System.Windows.Forms.TextBox tbStats;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblThreadCount;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
    }
}

