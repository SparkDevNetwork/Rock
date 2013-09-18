namespace Acrobat8Test
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
      this.axAcroPDF1 = new AxAcroPDFLib.AxAcroPDF();
      ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).BeginInit();
      this.SuspendLayout();
      // 
      // axAcroPDF1
      // 
      this.axAcroPDF1.Enabled = true;
      this.axAcroPDF1.Location = new System.Drawing.Point(12, 12);
      this.axAcroPDF1.Name = "axAcroPDF1";
      this.axAcroPDF1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axAcroPDF1.OcxState")));
      this.axAcroPDF1.Size = new System.Drawing.Size(467, 279);
      this.axAcroPDF1.TabIndex = 0;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(491, 303);
      this.Controls.Add(this.axAcroPDF1);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.axAcroPDF1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private AxAcroPDFLib.AxAcroPDF axAcroPDF1;
  }
}

