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
            this.btnStart = new System.Windows.Forms.Button();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.tbClientCount = new System.Windows.Forms.TextBox();
            this.lblClientCount = new System.Windows.Forms.Label();
            this.tbRequestCount = new System.Windows.Forms.TextBox();
            this.lblRequestCount = new System.Windows.Forms.Label();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 187);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(159, 55);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // tbUrl
            // 
            this.tbUrl.Location = new System.Drawing.Point(12, 25);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(280, 20);
            this.tbUrl.TabIndex = 1;
            this.tbUrl.Text = "http://localhost:6229/KeepAlive.aspx";
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(9, 9);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(29, 13);
            this.lblURL.TabIndex = 2;
            this.lblURL.Text = "URL";
            // 
            // tbClientCount
            // 
            this.tbClientCount.Location = new System.Drawing.Point(12, 78);
            this.tbClientCount.Name = "tbClientCount";
            this.tbClientCount.Size = new System.Drawing.Size(100, 20);
            this.tbClientCount.TabIndex = 3;
            this.tbClientCount.Text = "1000";
            this.tbClientCount.TextChanged += new System.EventHandler(this.tbClientCount_TextChanged);
            // 
            // lblClientCount
            // 
            this.lblClientCount.AutoSize = true;
            this.lblClientCount.Location = new System.Drawing.Point(9, 62);
            this.lblClientCount.Name = "lblClientCount";
            this.lblClientCount.Size = new System.Drawing.Size(64, 13);
            this.lblClientCount.TabIndex = 4;
            this.lblClientCount.Text = "Client Count";
            // 
            // tbRequestCount
            // 
            this.tbRequestCount.Location = new System.Drawing.Point(12, 134);
            this.tbRequestCount.Name = "tbRequestCount";
            this.tbRequestCount.Size = new System.Drawing.Size(202, 20);
            this.tbRequestCount.TabIndex = 5;
            this.tbRequestCount.Text = "100";
            // 
            // lblRequestCount
            // 
            this.lblRequestCount.AutoSize = true;
            this.lblRequestCount.Location = new System.Drawing.Point(9, 118);
            this.lblRequestCount.Name = "lblRequestCount";
            this.lblRequestCount.Size = new System.Drawing.Size(130, 13);
            this.lblRequestCount.TabIndex = 6;
            this.lblRequestCount.Text = "Request Count (per client)";
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(345, 18);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(238, 325);
            this.tbResults.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 355);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.lblRequestCount);
            this.Controls.Add(this.tbRequestCount);
            this.Controls.Add(this.lblClientCount);
            this.Controls.Add(this.tbClientCount);
            this.Controls.Add(this.lblURL);
            this.Controls.Add(this.tbUrl);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
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
    }
}

