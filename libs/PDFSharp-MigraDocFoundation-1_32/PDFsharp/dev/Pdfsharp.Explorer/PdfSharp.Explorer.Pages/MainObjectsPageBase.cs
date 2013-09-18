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
  /// Base class for MainObjectsPage and MainPagesPage
  /// </summary>
  public class MainObjectsPageBase : PageBase
  {
    private System.ComponentModel.Container components = null;

    public MainObjectsPageBase()
    {
      InitializeComponent();
    }

    public MainObjectsPageBase(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
      //explorer.lvPages.SelectedIndexChanged += new System.EventHandler(this.lvPages_SelectedIndexChanged);
    }
    protected ExplorerPanel explorer;

//    internal virtual void UpdateDocument()
//    {
//    }
//
//    internal virtual void SetObject(PdfItem value)
//    {
//    }

    public void ActivatePage(PdfObject obj)
    {
      if (this.currentPage != null)
      {
        this.Controls.Remove(this.currentPage);
        this.currentPage = null;
      }

      if (obj is PdfDictionary)
      {
        if (this.dictPage == null)
          this.dictPage = new DictionaryPage(this.explorer);
        this.currentPage = this.dictPage;
        this.currentPage.SetObject(obj);
      }
      else if (obj is PdfArray)
      {
        if (this.arrayPage == null)
          this.arrayPage = new ArrayPage(this.explorer);
        this.currentPage = this.arrayPage;
        this.currentPage.SetObject(obj);
      }
      else
      {
        if (this.simplePage == null)
          this.simplePage = new SimpleObjectPage(this.explorer);
        this.currentPage = this.simplePage;
        this.currentPage.SetObject(obj);
      }
      this.Controls.Add(this.currentPage);
      this.currentPage.Dock = DockStyle.Fill;
      currentPage.UpdateDocument();
    }
    PageBase currentPage, dictPage, arrayPage, simplePage;

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
