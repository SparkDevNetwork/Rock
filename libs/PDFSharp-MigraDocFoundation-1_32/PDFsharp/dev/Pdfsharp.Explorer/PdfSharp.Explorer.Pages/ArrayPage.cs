using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Explorer.Pages
{
  /// <summary>
  /// Viewer for PdfArray objects.
  /// </summary>
  public class ArrayPage : PageBase
  {
    private System.ComponentModel.Container components = null;

    ArrayPage()
    {
      InitializeComponent();
    }

    public ArrayPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
    }
    private System.Windows.Forms.ColumnHeader clmIndex;
    private System.Windows.Forms.ColumnHeader clmValue;
    private System.Windows.Forms.ColumnHeader clmIndirect;
    private System.Windows.Forms.ColumnHeader clmType;
    private System.Windows.Forms.ListView lvItems;
    private System.Windows.Forms.TextBox tbxObjectID;
    private System.Windows.Forms.Label lblObjectID;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.Label txtType;
    ExplorerPanel explorer;

    internal override void SetObject(PdfItem obj)
    {
      PdfArray array = (PdfArray)obj;

      if (PdfInternals.GetObjectID(array).IsEmpty)
        this.tbxObjectID.Text = "«direct»";
      else
        this.tbxObjectID.Text = PdfInternals.GetObjectID(array).ToString();
      this.txtType.Text = array.GetType().Name;

      this.lvItems.Items.Clear();
      PdfItem[] items = array.Elements.Items;
      int count = items.Length;
      for (int idx = 0; idx < count; idx++)
      {
        bool indirect = false;
        PdfItem tag = null;
        PdfItem value = items[idx];
        if (value == null)
        {
          throw new NotImplementedException("Value is null.");
        }
        else if (value is PdfReference)
        {
          indirect = true;
          value = ((PdfReference)value).Value;
          tag = value;
        }
        else
        {
          if (value is PdfObject)
            tag = value;
        }

        string address = "";
        if (indirect)
          address = /*((PdfObject)value).ObjectID.ToString() +*/ "---> ";
        ListViewItem item = new ListViewItem(
          new string[4]{idx.ToString(), 
          address + value.ToString(), 
          indirect ? PdfInternals.GetObjectID((PdfObject)value).ToString() : "", 
          value.GetType().Name});
        item.Tag = tag;
        this.lvItems.Items.Add(item);
      }
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
      this.lvItems = new System.Windows.Forms.ListView();
      this.clmIndex = new System.Windows.Forms.ColumnHeader();
      this.clmValue = new System.Windows.Forms.ColumnHeader();
      this.clmIndirect = new System.Windows.Forms.ColumnHeader();
      this.clmType = new System.Windows.Forms.ColumnHeader();
      this.tbxObjectID = new System.Windows.Forms.TextBox();
      this.lblObjectID = new System.Windows.Forms.Label();
      this.lblType = new System.Windows.Forms.Label();
      this.txtType = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lvItems
      // 
      this.lvItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lvItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                              this.clmIndex,
                                                                              this.clmValue,
                                                                              this.clmIndirect,
                                                                              this.clmType});
      this.lvItems.GridLines = true;
      this.lvItems.Location = new System.Drawing.Point(8, 28);
      this.lvItems.MultiSelect = false;
      this.lvItems.Name = "lvItems";
      this.lvItems.Size = new System.Drawing.Size(664, 464);
      this.lvItems.TabIndex = 0;
      this.lvItems.View = System.Windows.Forms.View.Details;
      this.lvItems.DoubleClick += new System.EventHandler(this.lvItems_DoubleClick);
      // 
      // clmIndex
      // 
      this.clmIndex.Text = "Index";
      this.clmIndex.Width = 80;
      // 
      // clmValue
      // 
      this.clmValue.Text = "Value   (double click items for navigation)";
      this.clmValue.Width = 400;
      // 
      // clmIndirect
      // 
      this.clmIndirect.Text = "Ref";
      this.clmIndirect.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.clmIndirect.Width = 50;
      // 
      // clmType
      // 
      this.clmType.Text = "PDFsharp Type";
      this.clmType.Width = 120;
      // 
      // tbxObjectID
      // 
      this.tbxObjectID.Location = new System.Drawing.Point(64, 4);
      this.tbxObjectID.Name = "tbxObjectID";
      this.tbxObjectID.ReadOnly = true;
      this.tbxObjectID.TabIndex = 4;
      this.tbxObjectID.Text = "";
      // 
      // lblObjectID
      // 
      this.lblObjectID.Location = new System.Drawing.Point(8, 8);
      this.lblObjectID.Name = "lblObjectID";
      this.lblObjectID.Size = new System.Drawing.Size(56, 16);
      this.lblObjectID.TabIndex = 3;
      this.lblObjectID.Text = "Object ID:";
      // 
      // lblType
      // 
      this.lblType.Location = new System.Drawing.Point(190, 8);
      this.lblType.Name = "lblType";
      this.lblType.Size = new System.Drawing.Size(38, 16);
      this.lblType.TabIndex = 5;
      this.lblType.Text = "Type:";
      // 
      // txtType
      // 
      this.txtType.Location = new System.Drawing.Point(228, 8);
      this.txtType.Name = "txtType";
      this.txtType.Size = new System.Drawing.Size(100, 16);
      this.txtType.TabIndex = 6;
      this.txtType.Text = "{Type}";
      // 
      // ArrayPage
      // 
      this.Controls.Add(this.txtType);
      this.Controls.Add(this.lblType);
      this.Controls.Add(this.tbxObjectID);
      this.Controls.Add(this.lblObjectID);
      this.Controls.Add(this.lvItems);
      this.Name = "ArrayPage";
      this.Tag = "Objects";
      this.ResumeLayout(false);

    }
    #endregion

    private void lvItems_DoubleClick(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = this.lvItems.SelectedItems;
      if (items.Count > 0)
      {
        ListViewItem item = items[0];
        object tag = item.Tag;
        if (tag != null)
        {
          Debug.Assert(tag is PdfObject);
          this.explorer.Process.Navigator.SetNext((PdfItem)tag);
          ((MainObjectsPageBase)Parent).ActivatePage((PdfObject)tag);
        }
      }
    }
  }
}
