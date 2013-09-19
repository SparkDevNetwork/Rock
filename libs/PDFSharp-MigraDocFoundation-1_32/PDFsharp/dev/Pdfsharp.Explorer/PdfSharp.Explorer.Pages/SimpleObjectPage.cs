using System;
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
  /// Viewer for simple PdfObject objects.
  /// </summary>
  public class SimpleObjectPage : PageBase
  {
    private System.ComponentModel.Container components = null;

    public SimpleObjectPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
    }

    private System.Windows.Forms.ListView lvKeys;
    private System.Windows.Forms.ColumnHeader clmValue;
    private System.Windows.Forms.TextBox tbxObjectID;
    private System.Windows.Forms.Label lblObjectID;
    private System.Windows.Forms.ColumnHeader clmType;
    private System.Windows.Forms.ColumnHeader clmPdfSharpType;
    private System.Windows.Forms.Label txtType;
    private System.Windows.Forms.Label lblType;
    ExplorerPanel explorer;

    internal override void SetObject(PdfItem value)
    {
      PdfObject obj = (PdfObject)value;

      this.tbxObjectID.Text = PdfInternals.GetObjectID(obj).ToString();
      this.txtType.Text = obj.GetType().Name;

      this.lvKeys.Items.Clear();

      ListViewItem item = new ListViewItem(
        new string[3]{ExplorerProcess.GetTypeName(obj), value.ToString(), value.GetType().Name});
      this.lvKeys.Items.Add(item);
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
      this.lvKeys = new System.Windows.Forms.ListView();
      this.clmType = new System.Windows.Forms.ColumnHeader();
      this.clmValue = new System.Windows.Forms.ColumnHeader();
      this.clmPdfSharpType = new System.Windows.Forms.ColumnHeader();
      this.tbxObjectID = new System.Windows.Forms.TextBox();
      this.lblObjectID = new System.Windows.Forms.Label();
      this.txtType = new System.Windows.Forms.Label();
      this.lblType = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // lvKeys
      // 
      this.lvKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lvKeys.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                             this.clmType,
                                                                             this.clmValue,
                                                                             this.clmPdfSharpType});
      this.lvKeys.GridLines = true;
      this.lvKeys.Location = new System.Drawing.Point(8, 28);
      this.lvKeys.Name = "lvKeys";
      this.lvKeys.Size = new System.Drawing.Size(664, 464);
      this.lvKeys.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.lvKeys.TabIndex = 0;
      this.lvKeys.View = System.Windows.Forms.View.Details;
      // 
      // clmType
      // 
      this.clmType.Text = "Type";
      this.clmType.Width = 80;
      // 
      // clmValue
      // 
      this.clmValue.Text = "Value";
      this.clmValue.Width = 450;
      // 
      // clmPdfSharpType
      // 
      this.clmPdfSharpType.Text = "PDFsharp Type";
      this.clmPdfSharpType.Width = 120;
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
      this.lblObjectID.Size = new System.Drawing.Size(56, 12);
      this.lblObjectID.TabIndex = 3;
      this.lblObjectID.Text = "Object ID:";
      // 
      // txtType
      // 
      this.txtType.Location = new System.Drawing.Point(228, 8);
      this.txtType.Name = "txtType";
      this.txtType.Size = new System.Drawing.Size(100, 16);
      this.txtType.TabIndex = 10;
      this.txtType.Text = "{Type}";
      // 
      // lblType
      // 
      this.lblType.Location = new System.Drawing.Point(190, 8);
      this.lblType.Name = "lblType";
      this.lblType.Size = new System.Drawing.Size(36, 16);
      this.lblType.TabIndex = 9;
      this.lblType.Text = "Type:";
      // 
      // SimpleObjectPage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.txtType);
      this.Controls.Add(this.lblType);
      this.Controls.Add(this.tbxObjectID);
      this.Controls.Add(this.lblObjectID);
      this.Controls.Add(this.lvKeys);
      this.Name = "SimpleObjectPage";
      this.Tag = "Objects";
      this.ResumeLayout(false);

    }
    #endregion

  }
}
