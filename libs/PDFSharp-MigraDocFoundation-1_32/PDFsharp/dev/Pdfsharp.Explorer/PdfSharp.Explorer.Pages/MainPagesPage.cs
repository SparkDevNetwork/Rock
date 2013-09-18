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
  public class MainPagesPage : MainObjectsPageBase
  {
    private System.ComponentModel.Container components = null;

    public MainPagesPage(ExplorerPanel explorer) 
      : base(explorer)
    {
      InitializeComponent();
      explorer.lvPages.SelectedIndexChanged += new System.EventHandler(this.lvPages_SelectedIndexChanged);
    }

    //void ActivatePage(PdfPage page)
    //{
    //  //if (this.currentPage != null)
    //  //{
    //  //  this.Controls.Remove(this.currentPage);
    //  //  this.currentPage = null;
    //  //}
    //
    //  if (this.currentPage == null)
    //  {
    //    this.currentPage = new DictionaryPage(this.explorer);
    //    this.Controls.Add(this.currentPage);
    //    this.currentPage.Dock = DockStyle.Fill;
    //  }
    //  this.currentPage.SetObject(page);
    //  currentPage.UpdateDocument();
    //}
    //PageBase currentPage;

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
      // MainPagesPage
      // 
      this.Name = "MainPagesPage";
      this.Tag = "Pages";

    }
    #endregion

    private void lvPages_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = this.explorer.lvPages.SelectedItems;
      if (items.Count > 0)
      {
        ListViewItem item = items[0];
        ActivatePage((PdfObject)item.Tag);
      }
    }
  }
}
