namespace Rock.Custom.CCV.ClientTestApp
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbStreet1 = new System.Windows.Forms.TextBox();
            this.tbCity = new System.Windows.Forms.TextBox();
            this.tbState = new System.Windows.Forms.TextBox();
            this.tbZip = new System.Windows.Forms.TextBox();
            this.lblStandardizedL = new System.Windows.Forms.Label();
            this.lblStandardized = new System.Windows.Forms.Label();
            this.lblGeocoded = new System.Windows.Forms.Label();
            this.lblGeocodedL = new System.Windows.Forms.Label();
            this.tbStreet2 = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 12, 17 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 45, 13 );
            this.label1.TabIndex = 0;
            this.label1.Text = "Address";
            // 
            // tbStreet1
            // 
            this.tbStreet1.Location = new System.Drawing.Point( 23, 33 );
            this.tbStreet1.Name = "tbStreet1";
            this.tbStreet1.Size = new System.Drawing.Size( 267, 20 );
            this.tbStreet1.TabIndex = 1;
            this.tbStreet1.Text = "7007 West Happy Valley Road";
            // 
            // tbCity
            // 
            this.tbCity.Location = new System.Drawing.Point( 23, 85 );
            this.tbCity.Name = "tbCity";
            this.tbCity.Size = new System.Drawing.Size( 143, 20 );
            this.tbCity.TabIndex = 3;
            this.tbCity.Text = "Peoria";
            // 
            // tbState
            // 
            this.tbState.Location = new System.Drawing.Point( 172, 85 );
            this.tbState.Name = "tbState";
            this.tbState.Size = new System.Drawing.Size( 35, 20 );
            this.tbState.TabIndex = 5;
            this.tbState.Text = "AZ";
            // 
            // tbZip
            // 
            this.tbZip.Location = new System.Drawing.Point( 213, 85 );
            this.tbZip.Name = "tbZip";
            this.tbZip.Size = new System.Drawing.Size( 77, 20 );
            this.tbZip.TabIndex = 7;
            // 
            // lblStandardizedL
            // 
            this.lblStandardizedL.AutoSize = true;
            this.lblStandardizedL.Location = new System.Drawing.Point( 12, 148 );
            this.lblStandardizedL.Name = "lblStandardizedL";
            this.lblStandardizedL.Size = new System.Drawing.Size( 69, 13 );
            this.lblStandardizedL.TabIndex = 8;
            this.lblStandardizedL.Text = "Standardized";
            // 
            // lblStandardized
            // 
            this.lblStandardized.AutoSize = true;
            this.lblStandardized.Location = new System.Drawing.Point( 22, 170 );
            this.lblStandardized.Name = "lblStandardized";
            this.lblStandardized.Size = new System.Drawing.Size( 0, 13 );
            this.lblStandardized.TabIndex = 9;
            // 
            // lblGeocoded
            // 
            this.lblGeocoded.AutoSize = true;
            this.lblGeocoded.Location = new System.Drawing.Point( 22, 230 );
            this.lblGeocoded.Name = "lblGeocoded";
            this.lblGeocoded.Size = new System.Drawing.Size( 0, 13 );
            this.lblGeocoded.TabIndex = 11;
            // 
            // lblGeocodedL
            // 
            this.lblGeocodedL.AutoSize = true;
            this.lblGeocodedL.Location = new System.Drawing.Point( 12, 208 );
            this.lblGeocodedL.Name = "lblGeocodedL";
            this.lblGeocodedL.Size = new System.Drawing.Size( 57, 13 );
            this.lblGeocodedL.TabIndex = 10;
            this.lblGeocodedL.Text = "Geocoded";
            // 
            // tbStreet2
            // 
            this.tbStreet2.Location = new System.Drawing.Point( 23, 59 );
            this.tbStreet2.Name = "tbStreet2";
            this.tbStreet2.Size = new System.Drawing.Size( 267, 20 );
            this.tbStreet2.TabIndex = 12;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point( 25, 111 );
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size( 95, 23 );
            this.btnGo.TabIndex = 13;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler( this.btnGo_Click );
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 309, 337 );
            this.Controls.Add( this.btnGo );
            this.Controls.Add( this.tbStreet2 );
            this.Controls.Add( this.lblGeocoded );
            this.Controls.Add( this.lblGeocodedL );
            this.Controls.Add( this.lblStandardized );
            this.Controls.Add( this.lblStandardizedL );
            this.Controls.Add( this.tbZip );
            this.Controls.Add( this.tbState );
            this.Controls.Add( this.tbCity );
            this.Controls.Add( this.tbStreet1 );
            this.Controls.Add( this.label1 );
            this.Name = "Form1";
            this.Text = "Test REST Address API";
            this.ResumeLayout( false );
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbStreet1;
        private System.Windows.Forms.TextBox tbCity;
        private System.Windows.Forms.TextBox tbState;
        private System.Windows.Forms.TextBox tbZip;
        private System.Windows.Forms.Label lblStandardizedL;
        private System.Windows.Forms.Label lblStandardized;
        private System.Windows.Forms.Label lblGeocoded;
        private System.Windows.Forms.Label lblGeocodedL;
        private System.Windows.Forms.TextBox tbStreet2;
        private System.Windows.Forms.Button btnGo;
    }
}

