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
  /// Viewer for PdfDictionary objects.
  /// </summary>
  public class DictionaryPage : PageBase
  {
    private System.ComponentModel.Container components = null;

    public DictionaryPage(ExplorerPanel explorer)
    {
      this.explorer = explorer;
      InitializeComponent();
    }

    private System.Windows.Forms.ListView lvKeys;
    private System.Windows.Forms.ColumnHeader clmKey;
    private System.Windows.Forms.ColumnHeader clmValue;
    private System.Windows.Forms.ColumnHeader clmType;
    private System.Windows.Forms.ColumnHeader clmIndirect;
    private System.Windows.Forms.RichTextBox tbxStream;
    private System.Windows.Forms.Panel pnlTop;
    private System.Windows.Forms.Splitter splitter;
    private System.Windows.Forms.Panel pnlBottom;
    private System.Windows.Forms.RadioButton btnHexdump;
    private System.Windows.Forms.RadioButton btnRawString;
    private System.Windows.Forms.CheckBox btnNoFilter;
    private System.Windows.Forms.Label lblObjectID;
    private System.Windows.Forms.TextBox tbxObjectID;
    private System.Windows.Forms.Label txtType;
    private System.Windows.Forms.Label lblType;
    ExplorerPanel explorer;

    internal override void SetObject(PdfItem obj)
    {
      PdfDictionary dict = (PdfDictionary)obj;

      if (PdfInternals.GetObjectID(dict).IsEmpty)
        this.tbxObjectID.Text = "«direct»";
      else
        this.tbxObjectID.Text = PdfInternals.GetObjectID(dict).ToString();
      this.txtType.Text = dict.GetType().Name;

      this.lvKeys.Items.Clear();
      this.tbxStream.Clear();
      PdfName[] keys = dict.Elements.KeyNames;
      foreach (PdfName key in keys)
      {
        bool indirect = false;
        PdfItem tag = null;
        PdfItem value = dict.Elements[key];
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
          new string[4]{key.Value, 
          address + value.ToString(), 
          indirect ? PdfInternals.GetObjectID((PdfObject)value).ToString() : "", 
          value.GetType().Name});
        item.Tag = tag;
        this.lvKeys.Items.Add(item);
      }

      FillStreamBox();
    }

    void FillStreamBox()
    {
      PdfItem item = this.explorer.MainForm.Process.Navigator.Current;
      PdfDictionary dict = item as PdfDictionary;

      if (dict.Stream == null)
      {
        this.pnlBottom.Enabled = false;
        this.tbxStream.Text = "";
        return;
      }

      this.pnlBottom.Enabled = true;

      if (dict != null && dict.Stream != null && dict.Stream.Value != null)
      {
        byte[] stream = this.btnNoFilter.Checked ? dict.Stream.UnfilteredValue : dict.Stream.Value;
        if (btnHexdump.Checked)
          this.tbxStream.Text = ExplorerHelper.HexDump(stream);
        else
        {
          int count = stream.Length;
          char[] chars = new char[count];
          for (int idx = 0; idx < count; idx++)
          {
            byte b = stream[idx];
            if (b == 0)
              b = 183;
            chars[idx] = (char)b;
          }
          this.tbxStream.Text = new string(chars);
        }
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
      this.lvKeys = new System.Windows.Forms.ListView();
      this.clmKey = new System.Windows.Forms.ColumnHeader();
      this.clmValue = new System.Windows.Forms.ColumnHeader();
      this.clmIndirect = new System.Windows.Forms.ColumnHeader();
      this.clmType = new System.Windows.Forms.ColumnHeader();
      this.btnHexdump = new System.Windows.Forms.RadioButton();
      this.btnRawString = new System.Windows.Forms.RadioButton();
      this.tbxStream = new System.Windows.Forms.RichTextBox();
      this.pnlTop = new System.Windows.Forms.Panel();
      this.txtType = new System.Windows.Forms.Label();
      this.lblType = new System.Windows.Forms.Label();
      this.tbxObjectID = new System.Windows.Forms.TextBox();
      this.lblObjectID = new System.Windows.Forms.Label();
      this.splitter = new System.Windows.Forms.Splitter();
      this.pnlBottom = new System.Windows.Forms.Panel();
      this.btnNoFilter = new System.Windows.Forms.CheckBox();
      this.pnlTop.SuspendLayout();
      this.pnlBottom.SuspendLayout();
      this.SuspendLayout();
      // 
      // lvKeys
      // 
      this.lvKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.lvKeys.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                             this.clmKey,
                                                                             this.clmValue,
                                                                             this.clmIndirect,
                                                                             this.clmType});
      this.lvKeys.GridLines = true;
      this.lvKeys.Location = new System.Drawing.Point(8, 28);
      this.lvKeys.Name = "lvKeys";
      this.lvKeys.Size = new System.Drawing.Size(664, 240);
      this.lvKeys.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.lvKeys.TabIndex = 0;
      this.lvKeys.View = System.Windows.Forms.View.Details;
      this.lvKeys.DoubleClick += new System.EventHandler(this.lvKeys_DoubleClick);
      // 
      // clmKey
      // 
      this.clmKey.Text = "Key";
      this.clmKey.Width = 80;
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
      // btnHexdump
      // 
      this.btnHexdump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnHexdump.Location = new System.Drawing.Point(588, 4);
      this.btnHexdump.Name = "btnHexdump";
      this.btnHexdump.Size = new System.Drawing.Size(76, 16);
      this.btnHexdump.TabIndex = 1;
      this.btnHexdump.Text = "HexDump";
      this.btnHexdump.CheckedChanged += new System.EventHandler(this.btnHexdump_CheckedChanged);
      // 
      // btnRawString
      // 
      this.btnRawString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRawString.Checked = true;
      this.btnRawString.Location = new System.Drawing.Point(488, 4);
      this.btnRawString.Name = "btnRawString";
      this.btnRawString.Size = new System.Drawing.Size(88, 16);
      this.btnRawString.TabIndex = 1;
      this.btnRawString.TabStop = true;
      this.btnRawString.Text = "Raw String";
      this.btnRawString.CheckedChanged += new System.EventHandler(this.btnRawString_CheckedChanged);
      // 
      // tbxStream
      // 
      this.tbxStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.tbxStream.DetectUrls = false;
      this.tbxStream.Font = new System.Drawing.Font("Courier New", 9.5F);
      this.tbxStream.Location = new System.Drawing.Point(8, 24);
      this.tbxStream.Name = "tbxStream";
      this.tbxStream.ReadOnly = true;
      this.tbxStream.Size = new System.Drawing.Size(664, 192);
      this.tbxStream.TabIndex = 2;
      this.tbxStream.Text = "";
      // 
      // pnlTop
      // 
      this.pnlTop.BackColor = System.Drawing.SystemColors.Control;
      this.pnlTop.Controls.Add(this.txtType);
      this.pnlTop.Controls.Add(this.lblType);
      this.pnlTop.Controls.Add(this.tbxObjectID);
      this.pnlTop.Controls.Add(this.lblObjectID);
      this.pnlTop.Controls.Add(this.lvKeys);
      this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.pnlTop.Location = new System.Drawing.Point(0, 0);
      this.pnlTop.Name = "pnlTop";
      this.pnlTop.Size = new System.Drawing.Size(680, 272);
      this.pnlTop.TabIndex = 3;
      // 
      // txtType
      // 
      this.txtType.Location = new System.Drawing.Point(228, 8);
      this.txtType.Name = "txtType";
      this.txtType.Size = new System.Drawing.Size(100, 16);
      this.txtType.TabIndex = 8;
      this.txtType.Text = "{Type}";
      // 
      // lblType
      // 
      this.lblType.Location = new System.Drawing.Point(190, 8);
      this.lblType.Name = "lblType";
      this.lblType.Size = new System.Drawing.Size(36, 16);
      this.lblType.TabIndex = 7;
      this.lblType.Text = "Type:";
      // 
      // tbxObjectID
      // 
      this.tbxObjectID.Location = new System.Drawing.Point(64, 4);
      this.tbxObjectID.Name = "tbxObjectID";
      this.tbxObjectID.ReadOnly = true;
      this.tbxObjectID.TabIndex = 2;
      this.tbxObjectID.Text = "";
      // 
      // lblObjectID
      // 
      this.lblObjectID.Location = new System.Drawing.Point(8, 8);
      this.lblObjectID.Name = "lblObjectID";
      this.lblObjectID.Size = new System.Drawing.Size(56, 12);
      this.lblObjectID.TabIndex = 1;
      this.lblObjectID.Text = "Object ID:";
      // 
      // splitter
      // 
      this.splitter.Dock = System.Windows.Forms.DockStyle.Top;
      this.splitter.Location = new System.Drawing.Point(0, 272);
      this.splitter.Name = "splitter";
      this.splitter.Size = new System.Drawing.Size(680, 3);
      this.splitter.TabIndex = 4;
      this.splitter.TabStop = false;
      // 
      // pnlBottom
      // 
      this.pnlBottom.Controls.Add(this.btnNoFilter);
      this.pnlBottom.Controls.Add(this.tbxStream);
      this.pnlBottom.Controls.Add(this.btnHexdump);
      this.pnlBottom.Controls.Add(this.btnRawString);
      this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlBottom.Location = new System.Drawing.Point(0, 275);
      this.pnlBottom.Name = "pnlBottom";
      this.pnlBottom.Size = new System.Drawing.Size(680, 225);
      this.pnlBottom.TabIndex = 5;
      // 
      // btnNoFilter
      // 
      this.btnNoFilter.Checked = true;
      this.btnNoFilter.CheckState = System.Windows.Forms.CheckState.Checked;
      this.btnNoFilter.Location = new System.Drawing.Point(8, 4);
      this.btnNoFilter.Name = "btnNoFilter";
      this.btnNoFilter.Size = new System.Drawing.Size(104, 16);
      this.btnNoFilter.TabIndex = 3;
      this.btnNoFilter.Text = "No Filter";
      this.btnNoFilter.CheckedChanged += new System.EventHandler(this.btnNoFilter_CheckedChanged);
      // 
      // DictionaryPage
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.pnlBottom);
      this.Controls.Add(this.splitter);
      this.Controls.Add(this.pnlTop);
      this.Name = "DictionaryPage";
      this.Tag = "Objects";
      this.pnlTop.ResumeLayout(false);
      this.pnlBottom.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void lvKeys_DoubleClick(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = this.lvKeys.SelectedItems;
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

    private void btnRawString_CheckedChanged(object sender, System.EventArgs e)
    {
      FillStreamBox();
    }

    private void btnHexdump_CheckedChanged(object sender, System.EventArgs e)
    {
      FillStreamBox();
    }

    private void btnNoFilter_CheckedChanged(object sender, System.EventArgs e)
    {
      FillStreamBox();
    }
  }
}
