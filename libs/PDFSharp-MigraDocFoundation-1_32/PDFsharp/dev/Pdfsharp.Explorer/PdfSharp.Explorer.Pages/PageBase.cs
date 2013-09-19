using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Pdf;

namespace PdfSharp.Explorer.Pages
{
  /// <summary>
  /// Base class for all pages.
  /// </summary>
  public class PageBase : System.Windows.Forms.UserControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public PageBase()
    {
      InitializeComponent();
    }

    internal virtual void UpdateDocument()
    {
    }

    internal virtual void SetObject(PdfItem value)
    {
    }

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
      // 
      // PageBase
      // 
      this.Name = "PageBase";
      this.Size = new System.Drawing.Size(680, 500);

    }
    #endregion
  }
}
