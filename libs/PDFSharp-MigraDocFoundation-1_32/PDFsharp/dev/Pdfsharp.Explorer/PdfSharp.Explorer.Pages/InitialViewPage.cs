using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PdfSharp.Explorer.Pages
{
  /// <summary>
  /// 
  /// </summary>
  public class InitialViewPage : PageBase
  {
    private System.Windows.Forms.Label lblTitel;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public InitialViewPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
    }
    ExplorerPanel explorer;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
          components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.lblTitel = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lblTitel
      // 
      this.lblTitel.Location = new System.Drawing.Point(12, 20);
      this.lblTitel.Name = "lblTitel";
      this.lblTitel.Size = new System.Drawing.Size(100, 16);
      this.lblTitel.TabIndex = 0;
      this.lblTitel.Text = "TODO...";
      // 
      // InitialViewPage
      // 
      this.Controls.Add(this.lblTitel);
      this.Name = "InitialViewPage";
      this.Size = new System.Drawing.Size(680, 500);
      this.ResumeLayout(false);

    }
    #endregion
  }
}
