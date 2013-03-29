//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.CheckScannerUtility
{
    partial class ScanChecksForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pbxBack = new System.Windows.Forms.PictureBox();
            this.pbxFront = new System.Windows.Forms.PictureBox();
            this.lblAccountNumber = new System.Windows.Forms.Label();
            this.lblRoutingNumber = new System.Windows.Forms.Label();
            this.lblCheckNumber = new System.Windows.Forms.Label();
            this.rectangleShape2 = new Microsoft.VisualBasic.PowerPacks.RectangleShape();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.btnScanAction = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxBack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxFront)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(328, 72);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 22);
            this.label2.TabIndex = 11;
            this.label2.Text = "Back";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 72);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 22);
            this.label1.TabIndex = 10;
            this.label1.Text = "Front";
            // 
            // pbxBack
            // 
            this.pbxBack.ErrorImage = null;
            this.pbxBack.InitialImage = null;
            this.pbxBack.Location = new System.Drawing.Point(332, 96);
            this.pbxBack.Margin = new System.Windows.Forms.Padding(5);
            this.pbxBack.Name = "pbxBack";
            this.pbxBack.Size = new System.Drawing.Size(300, 100);
            this.pbxBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxBack.TabIndex = 9;
            this.pbxBack.TabStop = false;
            // 
            // pbxFront
            // 
            this.pbxFront.ErrorImage = null;
            this.pbxFront.InitialImage = null;
            this.pbxFront.Location = new System.Drawing.Point(20, 96);
            this.pbxFront.Margin = new System.Windows.Forms.Padding(5);
            this.pbxFront.Name = "pbxFront";
            this.pbxFront.Size = new System.Drawing.Size(300, 100);
            this.pbxFront.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbxFront.TabIndex = 8;
            this.pbxFront.TabStop = false;
            // 
            // lblAccountNumber
            // 
            this.lblAccountNumber.AutoSize = true;
            this.lblAccountNumber.Location = new System.Drawing.Point(16, 16);
            this.lblAccountNumber.Name = "lblAccountNumber";
            this.lblAccountNumber.Size = new System.Drawing.Size(188, 22);
            this.lblAccountNumber.TabIndex = 12;
            this.lblAccountNumber.Text = "Account: 123456789012";
            // 
            // lblRoutingNumber
            // 
            this.lblRoutingNumber.AutoSize = true;
            this.lblRoutingNumber.Location = new System.Drawing.Point(328, 16);
            this.lblRoutingNumber.Name = "lblRoutingNumber";
            this.lblRoutingNumber.Size = new System.Drawing.Size(225, 22);
            this.lblRoutingNumber.TabIndex = 13;
            this.lblRoutingNumber.Text = "Routing Number: 012345678";
            // 
            // lblCheckNumber
            // 
            this.lblCheckNumber.AutoSize = true;
            this.lblCheckNumber.Location = new System.Drawing.Point(16, 40);
            this.lblCheckNumber.Name = "lblCheckNumber";
            this.lblCheckNumber.Size = new System.Drawing.Size(166, 22);
            this.lblCheckNumber.TabIndex = 14;
            this.lblCheckNumber.Text = "Check Number: 0123";
            // 
            // rectangleShape2
            // 
            this.rectangleShape2.BorderColor = System.Drawing.SystemColors.AppWorkspace;
            this.rectangleShape2.Location = new System.Drawing.Point(7, 209);
            this.rectangleShape2.Name = "rectangleShape2";
            this.rectangleShape2.Size = new System.Drawing.Size(633, 0);
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.rectangleShape2});
            this.shapeContainer1.Size = new System.Drawing.Size(651, 266);
            this.shapeContainer1.TabIndex = 15;
            this.shapeContainer1.TabStop = false;
            // 
            // btnScanAction
            // 
            this.btnScanAction.Location = new System.Drawing.Point(8, 224);
            this.btnScanAction.Margin = new System.Windows.Forms.Padding(5);
            this.btnScanAction.Name = "btnScanAction";
            this.btnScanAction.Size = new System.Drawing.Size(113, 32);
            this.btnScanAction.TabIndex = 16;
            this.btnScanAction.Text = "Start/Stop";
            this.btnScanAction.UseVisualStyleBackColor = true;
            // 
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(528, 224);
            this.btnDone.Margin = new System.Windows.Forms.Padding(5);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(113, 32);
            this.btnDone.TabIndex = 17;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            // 
            // ScanChecksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 266);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnScanAction);
            this.Controls.Add(this.lblCheckNumber);
            this.Controls.Add(this.lblRoutingNumber);
            this.Controls.Add(this.lblAccountNumber);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pbxBack);
            this.Controls.Add(this.pbxFront);
            this.Controls.Add(this.shapeContainer1);
            this.Font = new System.Drawing.Font("Open Sans", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ScanChecksForm";
            this.Text = "Scan Checks";
            ((System.ComponentModel.ISupportInitialize)(this.pbxBack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbxFront)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbxBack;
        private System.Windows.Forms.PictureBox pbxFront;
        private System.Windows.Forms.Label lblAccountNumber;
        private System.Windows.Forms.Label lblRoutingNumber;
        private System.Windows.Forms.Label lblCheckNumber;
        private Microsoft.VisualBasic.PowerPacks.RectangleShape rectangleShape2;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private System.Windows.Forms.Button btnScanAction;
        private System.Windows.Forms.Button btnDone;
    }
}