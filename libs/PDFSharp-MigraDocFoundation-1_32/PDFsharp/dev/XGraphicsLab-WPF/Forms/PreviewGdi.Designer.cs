namespace XDrawing.Forms
{
  partial class PreviewGdi
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
      this.Preview = new PdfSharp.Forms.PagePreview();
      this.SuspendLayout();
      // 
      // Preview
      // 
      this.Preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.Preview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.Preview.DesktopColor = System.Drawing.SystemColors.ControlDark;
      this.Preview.Location = new System.Drawing.Point(12, 12);
      this.Preview.Name = "Preview";
      this.Preview.PageColor = System.Drawing.Color.GhostWhite;
      this.Preview.PageSize = new System.Drawing.Size(595, 842);
      this.Preview.Size = new System.Drawing.Size(599, 420);
      this.Preview.TabIndex = 0;
      this.Preview.Zoom = PdfSharp.Forms.Zoom.FullPage;
      this.Preview.ZoomPercent = 35;
      // 
      // PreviewGdi
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(623, 444);
      this.Controls.Add(this.Preview);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PreviewGdi";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "GDI+ Preview";
      this.ResumeLayout(false);

    }

    #endregion

    public PdfSharp.Forms.PagePreview Preview;
  }
}