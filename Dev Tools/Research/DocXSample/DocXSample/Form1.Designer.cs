namespace DocXSample
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnTableDocX = new System.Windows.Forms.Button();
            this.btnLabelsDocX = new System.Windows.Forms.Button();
            this.btnMakeDocumentDocX = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnMakeTableOpenXML = new System.Windows.Forms.Button();
            this.btnLabelsOpenXML = new System.Windows.Forms.Button();
            this.btnMakeDocumentOpenXML = new System.Windows.Forms.Button();
            this.btnMakeUsingNext = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnTableDocX);
            this.groupBox1.Controls.Add(this.btnLabelsDocX);
            this.groupBox1.Controls.Add(this.btnMakeDocumentDocX);
            this.groupBox1.Location = new System.Drawing.Point(12, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(230, 300);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DocX";
            // 
            // btnTableDocX
            // 
            this.btnTableDocX.Location = new System.Drawing.Point(17, 87);
            this.btnTableDocX.Name = "btnTableDocX";
            this.btnTableDocX.Size = new System.Drawing.Size(105, 23);
            this.btnTableDocX.TabIndex = 5;
            this.btnTableDocX.Text = "Make Table";
            this.btnTableDocX.UseVisualStyleBackColor = true;
            this.btnTableDocX.Click += new System.EventHandler(this.btnTable_Click);
            // 
            // btnLabelsDocX
            // 
            this.btnLabelsDocX.Location = new System.Drawing.Point(17, 58);
            this.btnLabelsDocX.Name = "btnLabelsDocX";
            this.btnLabelsDocX.Size = new System.Drawing.Size(105, 23);
            this.btnLabelsDocX.TabIndex = 4;
            this.btnLabelsDocX.Text = "Make Labels";
            this.btnLabelsDocX.UseVisualStyleBackColor = true;
            this.btnLabelsDocX.Click += new System.EventHandler(this.btnLabels_Click);
            // 
            // btnMakeDocumentDocX
            // 
            this.btnMakeDocumentDocX.Location = new System.Drawing.Point(17, 29);
            this.btnMakeDocumentDocX.Name = "btnMakeDocumentDocX";
            this.btnMakeDocumentDocX.Size = new System.Drawing.Size(105, 23);
            this.btnMakeDocumentDocX.TabIndex = 3;
            this.btnMakeDocumentDocX.Text = "Make Document";
            this.btnMakeDocumentDocX.UseVisualStyleBackColor = true;
            this.btnMakeDocumentDocX.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnMakeUsingNext);
            this.groupBox2.Controls.Add(this.btnMakeTableOpenXML);
            this.groupBox2.Controls.Add(this.btnLabelsOpenXML);
            this.groupBox2.Controls.Add(this.btnMakeDocumentOpenXML);
            this.groupBox2.Location = new System.Drawing.Point(248, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(214, 300);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "OpenXML";
            // 
            // btnMakeTableOpenXML
            // 
            this.btnMakeTableOpenXML.Location = new System.Drawing.Point(17, 118);
            this.btnMakeTableOpenXML.Name = "btnMakeTableOpenXML";
            this.btnMakeTableOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnMakeTableOpenXML.TabIndex = 5;
            this.btnMakeTableOpenXML.Text = "Make Table";
            this.btnMakeTableOpenXML.UseVisualStyleBackColor = true;
            this.btnMakeTableOpenXML.Click += new System.EventHandler(this.btnMakeTableOpenXML_Click);
            // 
            // btnLabelsOpenXML
            // 
            this.btnLabelsOpenXML.Location = new System.Drawing.Point(17, 89);
            this.btnLabelsOpenXML.Name = "btnLabelsOpenXML";
            this.btnLabelsOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnLabelsOpenXML.TabIndex = 4;
            this.btnLabelsOpenXML.Text = "Make Labels";
            this.btnLabelsOpenXML.UseVisualStyleBackColor = true;
            this.btnLabelsOpenXML.Click += new System.EventHandler(this.btnLabelsOpenXML_Click);
            // 
            // btnMakeDocumentOpenXML
            // 
            this.btnMakeDocumentOpenXML.Location = new System.Drawing.Point(17, 60);
            this.btnMakeDocumentOpenXML.Name = "btnMakeDocumentOpenXML";
            this.btnMakeDocumentOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnMakeDocumentOpenXML.TabIndex = 3;
            this.btnMakeDocumentOpenXML.Text = "Make Document";
            this.btnMakeDocumentOpenXML.UseVisualStyleBackColor = true;
            this.btnMakeDocumentOpenXML.Click += new System.EventHandler(this.btnMakeDocumentOpenXML_Click);
            // 
            // btnMakeUsingNext
            // 
            this.btnMakeUsingNext.Location = new System.Drawing.Point(17, 31);
            this.btnMakeUsingNext.Name = "btnMakeUsingNext";
            this.btnMakeUsingNext.Size = new System.Drawing.Size(146, 23);
            this.btnMakeUsingNext.TabIndex = 6;
            this.btnMakeUsingNext.Text = "Merge Using NextRec";
            this.btnMakeUsingNext.UseVisualStyleBackColor = true;
            this.btnMakeUsingNext.Click += new System.EventHandler(this.btnMergeUsingNextRecord_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 327);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnTableDocX;
        private System.Windows.Forms.Button btnLabelsDocX;
        private System.Windows.Forms.Button btnMakeDocumentDocX;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnMakeTableOpenXML;
        private System.Windows.Forms.Button btnLabelsOpenXML;
        private System.Windows.Forms.Button btnMakeDocumentOpenXML;
        private System.Windows.Forms.Button btnMakeUsingNext;
    }
}

