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
  /// 
  /// </summary>
  public class MainInformationPage : PageBase
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public MainInformationPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
      explorer.lbxInfo.SelectedValueChanged += new System.EventHandler(this.lbxInfo_SelectedValueChanged);
    }
    ExplorerPanel explorer;

    void ActivatePage(string name)
    {
      if (this.currentPage != null)
      {
        this.Controls.Remove(this.currentPage);
        this.currentPage = null;
      }

      switch (name)
      {
        case "description":
          this.currentPage = new DescriptionPage(this.explorer);
          break;

        case "view":
          this.currentPage = new InitialViewPage(this.explorer);
          break;

        default:
          throw new NotImplementedException("tag: " + name);
      }
      this.Controls.Add(this.currentPage);
      currentPage.UpdateDocument();
    }
    PageBase currentPage;

    internal override void UpdateDocument()
    {
      base.UpdateDocument ();
      ActivatePage("description");
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

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      ActivatePage("description");
    }


    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      // 
      // MainInformationPage
      // 
      this.Name = "MainInformationPage";
      this.Size = new System.Drawing.Size(680, 500);
      this.Tag = "Info";

    }
    #endregion

    private void lbxInfo_SelectedValueChanged(object sender, System.EventArgs e)
    {
      int idx = this.explorer.lbxInfo.SelectedIndex;
      if (idx == 0)
        ActivatePage("description");
      else
        ActivatePage("view");
    }

  }
}
