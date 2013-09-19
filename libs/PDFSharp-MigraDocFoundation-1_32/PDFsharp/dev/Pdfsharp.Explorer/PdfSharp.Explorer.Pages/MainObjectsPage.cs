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
  public class MainObjectsPage : MainObjectsPageBase
  {
    private System.ComponentModel.Container components = null;

    public MainObjectsPage(ExplorerPanel explorer)
      : base(explorer)
    {
      InitializeComponent();
      //explorer.lvObjects.SelectedIndexChanged += new System.EventHandler(this.lvObjects_SelectedIndexChanged);
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
      // MainObjectsPage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Name = "MainObjectsPage";
      this.Tag = "Objects";

    }
    #endregion

//    private void lvObjects_SelectedIndexChanged(object sender, System.EventArgs e)
//    {
//      ListView.SelectedListViewItemCollection items = this.explorer.lvObjects.SelectedItems;
//      if (items.Count > 0)
//      {
//        ListViewItem item = items[0];
//        ActivatePage((PdfObject)item.Tag);
//      }
//    }
  }
}
