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
  public class MainTreePage : PageBase
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MainTreePage(ExplorerPanel explorer)
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
      // 
      // MainTreePage
      // 
      this.Name = "MainTreePage";
      this.Tag = "Tree";

    }
    #endregion
  }
}
