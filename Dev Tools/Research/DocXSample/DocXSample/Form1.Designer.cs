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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnMergeHalfPageUsingNextRecord = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnMakeUsingNext = new System.Windows.Forms.Button();
            this.btnMakeTableOpenXML = new System.Windows.Forms.Button();
            this.btnLabelsOpenXML = new System.Windows.Forms.Button();
            this.btnMakeDocumentOpenXML = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnMergeHalfPageUsingNextRecord);
            this.groupBox2.Controls.Add(this.button1);
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
            // btnMergeHalfPageUsingNextRecord
            // 
            this.btnMergeHalfPageUsingNextRecord.Location = new System.Drawing.Point(17, 89);
            this.btnMergeHalfPageUsingNextRecord.Name = "btnMergeHalfPageUsingNextRecord";
            this.btnMergeHalfPageUsingNextRecord.Size = new System.Drawing.Size(191, 23);
            this.btnMergeHalfPageUsingNextRecord.TabIndex = 8;
            this.btnMergeHalfPageUsingNextRecord.Text = "Merge Half Page Using NextRec";
            this.btnMergeHalfPageUsingNextRecord.UseVisualStyleBackColor = true;
            this.btnMergeHalfPageUsingNextRecord.Click += new System.EventHandler(this.btnMergeHalfPageUsingNextRecord_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(17, 60);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(191, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Merge Labels Using NextRec";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnMergeLabelsUsingNextRecord_Click);
            // 
            // btnMakeUsingNext
            // 
            this.btnMakeUsingNext.Location = new System.Drawing.Point(17, 31);
            this.btnMakeUsingNext.Name = "btnMakeUsingNext";
            this.btnMakeUsingNext.Size = new System.Drawing.Size(191, 23);
            this.btnMakeUsingNext.TabIndex = 6;
            this.btnMakeUsingNext.Text = "Merge Letter Using NextRec";
            this.btnMakeUsingNext.UseVisualStyleBackColor = true;
            this.btnMakeUsingNext.Click += new System.EventHandler(this.btnMergeLetterUsingNextRecord_Click);
            // 
            // btnMakeTableOpenXML
            // 
            this.btnMakeTableOpenXML.Location = new System.Drawing.Point(26, 248);
            this.btnMakeTableOpenXML.Name = "btnMakeTableOpenXML";
            this.btnMakeTableOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnMakeTableOpenXML.TabIndex = 5;
            this.btnMakeTableOpenXML.Text = "Make Table";
            this.btnMakeTableOpenXML.UseVisualStyleBackColor = true;
            this.btnMakeTableOpenXML.Click += new System.EventHandler(this.btnMakeTableOpenXML_Click);
            // 
            // btnLabelsOpenXML
            // 
            this.btnLabelsOpenXML.Location = new System.Drawing.Point(26, 219);
            this.btnLabelsOpenXML.Name = "btnLabelsOpenXML";
            this.btnLabelsOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnLabelsOpenXML.TabIndex = 4;
            this.btnLabelsOpenXML.Text = "Make Labels";
            this.btnLabelsOpenXML.UseVisualStyleBackColor = true;
            this.btnLabelsOpenXML.Click += new System.EventHandler(this.btnLabelsOpenXML_Click);
            // 
            // btnMakeDocumentOpenXML
            // 
            this.btnMakeDocumentOpenXML.Location = new System.Drawing.Point(26, 190);
            this.btnMakeDocumentOpenXML.Name = "btnMakeDocumentOpenXML";
            this.btnMakeDocumentOpenXML.Size = new System.Drawing.Size(105, 23);
            this.btnMakeDocumentOpenXML.TabIndex = 3;
            this.btnMakeDocumentOpenXML.Text = "Make Document";
            this.btnMakeDocumentOpenXML.UseVisualStyleBackColor = true;
            this.btnMakeDocumentOpenXML.Click += new System.EventHandler(this.btnMakeDocumentOpenXML_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 327);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnMakeTableOpenXML;
        private System.Windows.Forms.Button btnLabelsOpenXML;
        private System.Windows.Forms.Button btnMakeDocumentOpenXML;
        private System.Windows.Forms.Button btnMakeUsingNext;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnMergeHalfPageUsingNextRecord;
    }
}

