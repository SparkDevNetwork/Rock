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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tbStandardized = new System.Windows.Forms.TextBox();
            this.tbGeocoded = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.tbStreet2 = new System.Windows.Forms.TextBox();
            this.lblGeocodedL = new System.Windows.Forms.Label();
            this.lblStandardizedL = new System.Windows.Forms.Label();
            this.tbZip = new System.Windows.Forms.TextBox();
            this.tbState = new System.Windows.Forms.TextBox();
            this.tbCity = new System.Windows.Forms.TextBox();
            this.tbStreet1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tbEncryptionResult = new System.Windows.Forms.TextBox();
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbEncryption = new System.Windows.Forms.TextBox();
            this.lblEncryptionSource = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tbAttrValue = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnAttrGo = new System.Windows.Forms.Button();
            this.cbAttrRequired = new System.Windows.Forms.CheckBox();
            this.tbAttrDescription = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbAttrCategory = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbAttrName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbAttrKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add( this.tabPage1 );
            this.tabControl1.Controls.Add( this.tabPage2 );
            this.tabControl1.Controls.Add( this.tabPage3 );
            this.tabControl1.Location = new System.Drawing.Point( 12, 12 );
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size( 329, 318 );
            this.tabControl1.TabIndex = 16;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add( this.tbStandardized );
            this.tabPage1.Controls.Add( this.tbGeocoded );
            this.tabPage1.Controls.Add( this.btnGo );
            this.tabPage1.Controls.Add( this.tbStreet2 );
            this.tabPage1.Controls.Add( this.lblGeocodedL );
            this.tabPage1.Controls.Add( this.lblStandardizedL );
            this.tabPage1.Controls.Add( this.tbZip );
            this.tabPage1.Controls.Add( this.tbState );
            this.tabPage1.Controls.Add( this.tbCity );
            this.tabPage1.Controls.Add( this.tbStreet1 );
            this.tabPage1.Controls.Add( this.label1 );
            this.tabPage1.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage1.Size = new System.Drawing.Size( 321, 292 );
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Address";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tbStandardized
            // 
            this.tbStandardized.Location = new System.Drawing.Point( 29, 162 );
            this.tbStandardized.Name = "tbStandardized";
            this.tbStandardized.Size = new System.Drawing.Size( 267, 20 );
            this.tbStandardized.TabIndex = 26;
            // 
            // tbGeocoded
            // 
            this.tbGeocoded.Location = new System.Drawing.Point( 29, 222 );
            this.tbGeocoded.Name = "tbGeocoded";
            this.tbGeocoded.Size = new System.Drawing.Size( 267, 20 );
            this.tbGeocoded.TabIndex = 25;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point( 31, 109 );
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size( 95, 23 );
            this.btnGo.TabIndex = 24;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler( this.btnGo_Click );
            // 
            // tbStreet2
            // 
            this.tbStreet2.Location = new System.Drawing.Point( 29, 57 );
            this.tbStreet2.Name = "tbStreet2";
            this.tbStreet2.Size = new System.Drawing.Size( 267, 20 );
            this.tbStreet2.TabIndex = 23;
            // 
            // lblGeocodedL
            // 
            this.lblGeocodedL.AutoSize = true;
            this.lblGeocodedL.Location = new System.Drawing.Point( 18, 206 );
            this.lblGeocodedL.Name = "lblGeocodedL";
            this.lblGeocodedL.Size = new System.Drawing.Size( 57, 13 );
            this.lblGeocodedL.TabIndex = 22;
            this.lblGeocodedL.Text = "Geocoded";
            // 
            // lblStandardizedL
            // 
            this.lblStandardizedL.AutoSize = true;
            this.lblStandardizedL.Location = new System.Drawing.Point( 18, 146 );
            this.lblStandardizedL.Name = "lblStandardizedL";
            this.lblStandardizedL.Size = new System.Drawing.Size( 69, 13 );
            this.lblStandardizedL.TabIndex = 21;
            this.lblStandardizedL.Text = "Standardized";
            // 
            // tbZip
            // 
            this.tbZip.Location = new System.Drawing.Point( 219, 83 );
            this.tbZip.Name = "tbZip";
            this.tbZip.Size = new System.Drawing.Size( 77, 20 );
            this.tbZip.TabIndex = 20;
            // 
            // tbState
            // 
            this.tbState.Location = new System.Drawing.Point( 178, 83 );
            this.tbState.Name = "tbState";
            this.tbState.Size = new System.Drawing.Size( 35, 20 );
            this.tbState.TabIndex = 19;
            this.tbState.Text = "AZ";
            // 
            // tbCity
            // 
            this.tbCity.Location = new System.Drawing.Point( 29, 83 );
            this.tbCity.Name = "tbCity";
            this.tbCity.Size = new System.Drawing.Size( 143, 20 );
            this.tbCity.TabIndex = 18;
            this.tbCity.Text = "Peoria";
            // 
            // tbStreet1
            // 
            this.tbStreet1.Location = new System.Drawing.Point( 29, 31 );
            this.tbStreet1.Name = "tbStreet1";
            this.tbStreet1.Size = new System.Drawing.Size( 267, 20 );
            this.tbStreet1.TabIndex = 17;
            this.tbStreet1.Text = "7007 West Happy Valley Road";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point( 18, 15 );
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size( 45, 13 );
            this.label1.TabIndex = 16;
            this.label1.Text = "Address";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add( this.tbEncryptionResult );
            this.tabPage2.Controls.Add( this.btnEncrypt );
            this.tabPage2.Controls.Add( this.btnDecrypt );
            this.tabPage2.Controls.Add( this.label3 );
            this.tabPage2.Controls.Add( this.tbEncryption );
            this.tabPage2.Controls.Add( this.lblEncryptionSource );
            this.tabPage2.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage2.Size = new System.Drawing.Size( 321, 292 );
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Encryption";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tbEncryptionResult
            // 
            this.tbEncryptionResult.Location = new System.Drawing.Point( 66, 48 );
            this.tbEncryptionResult.Multiline = true;
            this.tbEncryptionResult.Name = "tbEncryptionResult";
            this.tbEncryptionResult.Size = new System.Drawing.Size( 230, 175 );
            this.tbEncryptionResult.TabIndex = 7;
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.Location = new System.Drawing.Point( 159, 263 );
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size( 75, 23 );
            this.btnEncrypt.TabIndex = 6;
            this.btnEncrypt.Text = "Encrypt";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler( this.btnEncrypt_Click );
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Location = new System.Drawing.Point( 240, 263 );
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size( 75, 23 );
            this.btnDecrypt.TabIndex = 5;
            this.btnDecrypt.Text = "Decrypt";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler( this.btnDecrypt_Click );
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point( 19, 51 );
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size( 37, 13 );
            this.label3.TabIndex = 2;
            this.label3.Text = "Result";
            // 
            // tbEncryption
            // 
            this.tbEncryption.Location = new System.Drawing.Point( 66, 16 );
            this.tbEncryption.Name = "tbEncryption";
            this.tbEncryption.Size = new System.Drawing.Size( 234, 20 );
            this.tbEncryption.TabIndex = 1;
            // 
            // lblEncryptionSource
            // 
            this.lblEncryptionSource.AutoSize = true;
            this.lblEncryptionSource.Location = new System.Drawing.Point( 19, 19 );
            this.lblEncryptionSource.Name = "lblEncryptionSource";
            this.lblEncryptionSource.Size = new System.Drawing.Size( 41, 13 );
            this.lblEncryptionSource.TabIndex = 0;
            this.lblEncryptionSource.Text = "Source";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add( this.tbAttrValue );
            this.tabPage3.Controls.Add( this.label7 );
            this.tabPage3.Controls.Add( this.btnAttrGo );
            this.tabPage3.Controls.Add( this.cbAttrRequired );
            this.tabPage3.Controls.Add( this.tbAttrDescription );
            this.tabPage3.Controls.Add( this.label6 );
            this.tabPage3.Controls.Add( this.tbAttrCategory );
            this.tabPage3.Controls.Add( this.label5 );
            this.tabPage3.Controls.Add( this.tbAttrName );
            this.tabPage3.Controls.Add( this.label4 );
            this.tabPage3.Controls.Add( this.tbAttrKey );
            this.tabPage3.Controls.Add( this.label2 );
            this.tabPage3.Location = new System.Drawing.Point( 4, 22 );
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding( 3 );
            this.tabPage3.Size = new System.Drawing.Size( 321, 292 );
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Add Attribute";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tbAttrValue
            // 
            this.tbAttrValue.Location = new System.Drawing.Point( 87, 120 );
            this.tbAttrValue.Name = "tbAttrValue";
            this.tbAttrValue.Size = new System.Drawing.Size( 179, 20 );
            this.tbAttrValue.TabIndex = 27;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point( 21, 123 );
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size( 34, 13 );
            this.label7.TabIndex = 26;
            this.label7.Text = "Value";
            // 
            // btnAttrGo
            // 
            this.btnAttrGo.Location = new System.Drawing.Point( 87, 177 );
            this.btnAttrGo.Name = "btnAttrGo";
            this.btnAttrGo.Size = new System.Drawing.Size( 95, 23 );
            this.btnAttrGo.TabIndex = 25;
            this.btnAttrGo.Text = "Go";
            this.btnAttrGo.UseVisualStyleBackColor = true;
            this.btnAttrGo.Click += new System.EventHandler( this.btnAttrGo_Click );
            // 
            // cbAttrRequired
            // 
            this.cbAttrRequired.AutoSize = true;
            this.cbAttrRequired.Location = new System.Drawing.Point( 87, 154 );
            this.cbAttrRequired.Name = "cbAttrRequired";
            this.cbAttrRequired.Size = new System.Drawing.Size( 69, 17 );
            this.cbAttrRequired.TabIndex = 9;
            this.cbAttrRequired.Text = "Required";
            this.cbAttrRequired.UseVisualStyleBackColor = true;
            // 
            // tbAttrDescription
            // 
            this.tbAttrDescription.Location = new System.Drawing.Point( 87, 96 );
            this.tbAttrDescription.Name = "tbAttrDescription";
            this.tbAttrDescription.Size = new System.Drawing.Size( 179, 20 );
            this.tbAttrDescription.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point( 21, 99 );
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size( 60, 13 );
            this.label6.TabIndex = 6;
            this.label6.Text = "Description";
            // 
            // tbAttrCategory
            // 
            this.tbAttrCategory.Location = new System.Drawing.Point( 87, 70 );
            this.tbAttrCategory.Name = "tbAttrCategory";
            this.tbAttrCategory.Size = new System.Drawing.Size( 179, 20 );
            this.tbAttrCategory.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point( 21, 73 );
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size( 49, 13 );
            this.label5.TabIndex = 4;
            this.label5.Text = "Category";
            // 
            // tbAttrName
            // 
            this.tbAttrName.Location = new System.Drawing.Point( 87, 44 );
            this.tbAttrName.Name = "tbAttrName";
            this.tbAttrName.Size = new System.Drawing.Size( 179, 20 );
            this.tbAttrName.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point( 21, 47 );
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size( 35, 13 );
            this.label4.TabIndex = 2;
            this.label4.Text = "Name";
            // 
            // tbAttrKey
            // 
            this.tbAttrKey.Location = new System.Drawing.Point( 87, 18 );
            this.tbAttrKey.Name = "tbAttrKey";
            this.tbAttrKey.Size = new System.Drawing.Size( 179, 20 );
            this.tbAttrKey.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point( 21, 21 );
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size( 25, 13 );
            this.label2.TabIndex = 0;
            this.label2.Text = "Key";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 357, 356 );
            this.Controls.Add( this.tabControl1 );
            this.Name = "Form1";
            this.Text = "Test REST Address API";
            this.tabControl1.ResumeLayout( false );
            this.tabPage1.ResumeLayout( false );
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout( false );
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout( false );
            this.tabPage3.PerformLayout();
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox tbStandardized;
        private System.Windows.Forms.TextBox tbGeocoded;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.TextBox tbStreet2;
        private System.Windows.Forms.Label lblGeocodedL;
        private System.Windows.Forms.Label lblStandardizedL;
        private System.Windows.Forms.TextBox tbZip;
        private System.Windows.Forms.TextBox tbState;
        private System.Windows.Forms.TextBox tbCity;
        private System.Windows.Forms.TextBox tbStreet1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbEncryption;
        private System.Windows.Forms.Label lblEncryptionSource;
        private System.Windows.Forms.TextBox tbEncryptionResult;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnAttrGo;
        private System.Windows.Forms.CheckBox cbAttrRequired;
        private System.Windows.Forms.TextBox tbAttrDescription;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbAttrCategory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbAttrName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbAttrKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbAttrValue;
        private System.Windows.Forms.Label label7;
    }
}

