using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Explorer.Pages;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// 
  /// </summary>
  public class ExplorerPanel : System.Windows.Forms.UserControl
  {
    private System.Windows.Forms.ColumnHeader id;
    private System.Windows.Forms.ColumnHeader type;
    internal System.Windows.Forms.ListView lvObjects;
    private System.Windows.Forms.TabPage tpObjects;
    private System.Windows.Forms.TabPage tpInfo;
    internal System.Windows.Forms.ListBox lbxInfo;
    private System.Windows.Forms.TabControl tbcNavigation;
    private System.Windows.Forms.TabPage tpPages;
    internal System.Windows.Forms.ListView lvPages;
    private System.Windows.Forms.ColumnHeader clmPage;
    private System.Windows.Forms.Panel pnlNavigation;
    private System.Windows.Forms.Splitter splitter;
    private System.Windows.Forms.Panel pnlWorkArea;
    private System.Windows.Forms.TabPage tpPdf;
    private System.Windows.Forms.TabPage tpData;
    private System.Windows.Forms.ColumnHeader clmSize;
    private System.Windows.Forms.TabControl tbcMain;
    private System.ComponentModel.Container components = null;

    public ExplorerPanel(MainForm mainForm)
    {
      this.mainForm = mainForm;
      this.process = mainForm.Process;
      InitializeComponent();

      PageBase page = new PdfTextPage(this);
      this.tbcMain.TabPages[1].Controls.Add(page);
      page.Dock = DockStyle.Fill;
    }

    public MainForm MainForm
    {
      get {return this.mainForm;}
    }
    MainForm mainForm;

    public void OnNewDocument()
    {
      this.process = this.mainForm.Process;

      PdfObject[] objects = this.process.Document.Internals.GetAllObjects();
      this.lvObjects.Items.Clear();
      for (int idx = 0; idx < objects.Length; idx++)
      {
        PdfObject obj = objects[idx];
        ListViewItem item = new ListViewItem(new string[2]{PdfInternals.GetObjectID(obj).ToString(), ExplorerProcess.GetTypeName(obj)});
        item.Tag = obj;
        this.lvObjects.Items.Add(item);
      }

      PdfPages pages = this.process.Document.Pages;
      this.lvPages.Items.Clear();
      for (int idx = 0; idx < pages.Count; idx++)
      {
        PdfPage page = pages[idx];
        ListViewItem item = new ListViewItem(new string[2]{(idx + 1).ToString(), 
          ExplorerHelper.PageSize(page, this.mainForm.Process.IsMetric)});
          //String.Format("{0:0} x {1:0} mm", XUnit.FromPoint(page.Width).Millimeter,XUnit.FromPoint(page.Height).Millimeter)});
        item.Tag = page;
        this.lvPages.Items.Add(item);
      }

      this.process.Navigator.SetNext(this.process.Document.Info);
      ActivatePage("Info");
    }

    public ExplorerProcess Process
    {
      get { return this.process; }
    }
    ExplorerProcess process;

    public void NavigateTo(PdfItem item)
    {
    }

    void ActivatePage(string name)
    {
      if (this.currentPage != null)
      {
        if (this.currentPage.Tag.Equals(name))
        {
          this.currentPage.UpdateDocument();
          return;
        }
        this.tpData.Controls.Remove(this.currentPage);
        this.currentPage = null;
      }

      switch (name)
      {
        case "Info":
          if (this.infoPage == null)
            this.infoPage = new MainInformationPage(this);
          this.currentPage = this.infoPage;
          break;

        case "Pages":
          if (this.pagesPage == null)
            this.pagesPage = new MainPagesPage(this);
          this.currentPage = this.pagesPage;
          this.currentPage.Dock = DockStyle.Fill;
          break;

        case "Objects":
          if (this.objectsPage == null)
            this.objectsPage = new MainObjectsPage(this);
          this.currentPage = this.objectsPage;
          this.currentPage.Dock = DockStyle.Fill;
          break;

        //case "Tree":
        //  if (this.treePage == null)
        //    this.treePage = new MainTreePage(this);
        //  this.currentPage = this.treePage;
        //  this.currentPage.Dock = DockStyle.Fill;
        //  break;

        default:
          throw new NotImplementedException("tag: " + name);
      }
      this.tpData.Controls.Add(this.currentPage);
    }
    PageBase currentPage;
    PageBase infoPage;
    PageBase pagesPage;
    PageBase objectsPage;
    //PageBase treePage;

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
      ActivatePage("Info");
    }

    void ActivateTab(PdfObject value)
    {
      switch (this.tbcMain.SelectedIndex)
      {
          // Data
        case 0:
          if (this.tbcMain.TabPages[0].Controls[0] is MainObjectsPageBase)
            ((MainObjectsPageBase)this.tbcMain.TabPages[0].Controls[0]).ActivatePage(value);
          break;

          // PDF
        case 1:
          ((PdfTextPage)this.tbcMain.TabPages[1].Controls[0]).SetObject(value);
          break;
      }
    }



    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tbcNavigation = new System.Windows.Forms.TabControl();
      this.tpInfo = new System.Windows.Forms.TabPage();
      this.lbxInfo = new System.Windows.Forms.ListBox();
      this.tpObjects = new System.Windows.Forms.TabPage();
      this.lvObjects = new System.Windows.Forms.ListView();
      this.id = new System.Windows.Forms.ColumnHeader();
      this.type = new System.Windows.Forms.ColumnHeader();
      this.tpPages = new System.Windows.Forms.TabPage();
      this.lvPages = new System.Windows.Forms.ListView();
      this.clmPage = new System.Windows.Forms.ColumnHeader();
      this.clmSize = new System.Windows.Forms.ColumnHeader();
      this.pnlNavigation = new System.Windows.Forms.Panel();
      this.splitter = new System.Windows.Forms.Splitter();
      this.pnlWorkArea = new System.Windows.Forms.Panel();
      this.tbcMain = new System.Windows.Forms.TabControl();
      this.tpData = new System.Windows.Forms.TabPage();
      this.tpPdf = new System.Windows.Forms.TabPage();
      this.tbcNavigation.SuspendLayout();
      this.tpInfo.SuspendLayout();
      this.tpObjects.SuspendLayout();
      this.tpPages.SuspendLayout();
      this.pnlNavigation.SuspendLayout();
      this.pnlWorkArea.SuspendLayout();
      this.tbcMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // tbcNavigation
      // 
      this.tbcNavigation.Alignment = System.Windows.Forms.TabAlignment.Left;
      this.tbcNavigation.Controls.Add(this.tpInfo);
      this.tbcNavigation.Controls.Add(this.tpObjects);
      this.tbcNavigation.Controls.Add(this.tpPages);
      this.tbcNavigation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbcNavigation.Font = new System.Drawing.Font("Verdana", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tbcNavigation.Location = new System.Drawing.Point(3, 0);
      this.tbcNavigation.Multiline = true;
      this.tbcNavigation.Name = "tbcNavigation";
      this.tbcNavigation.SelectedIndex = 0;
      this.tbcNavigation.Size = new System.Drawing.Size(195, 630);
      this.tbcNavigation.TabIndex = 0;
      this.tbcNavigation.SelectedIndexChanged += new System.EventHandler(this.tbcNavigation_SelectedIndexChanged);
      // 
      // tpInfo
      // 
      this.tpInfo.Controls.Add(this.lbxInfo);
      this.tpInfo.Location = new System.Drawing.Point(24, 4);
      this.tpInfo.Name = "tpInfo";
      this.tpInfo.Size = new System.Drawing.Size(167, 622);
      this.tpInfo.TabIndex = 2;
      this.tpInfo.Tag = "Info";
      this.tpInfo.Text = "Information";
      // 
      // lbxInfo
      // 
      this.lbxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lbxInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.lbxInfo.Items.AddRange(new object[] {
            "Description",
            "Security",
            "Initial View",
            "(Fonts)",
            "(Annotations)",
            "(Images)",
            "ect..."});
      this.lbxInfo.Location = new System.Drawing.Point(0, 0);
      this.lbxInfo.Name = "lbxInfo";
      this.lbxInfo.Size = new System.Drawing.Size(167, 615);
      this.lbxInfo.TabIndex = 0;
      // 
      // tpObjects
      // 
      this.tpObjects.Controls.Add(this.lvObjects);
      this.tpObjects.Location = new System.Drawing.Point(24, 4);
      this.tpObjects.Name = "tpObjects";
      this.tpObjects.Size = new System.Drawing.Size(167, 622);
      this.tpObjects.TabIndex = 0;
      this.tpObjects.Tag = "Objects";
      this.tpObjects.Text = "Objects";
      // 
      // lvObjects
      // 
      this.lvObjects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.id,
            this.type});
      this.lvObjects.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvObjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.lvObjects.FullRowSelect = true;
      this.lvObjects.GridLines = true;
      this.lvObjects.HideSelection = false;
      this.lvObjects.Location = new System.Drawing.Point(0, 0);
      this.lvObjects.MultiSelect = false;
      this.lvObjects.Name = "lvObjects";
      this.lvObjects.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.lvObjects.Size = new System.Drawing.Size(167, 622);
      this.lvObjects.TabIndex = 0;
      this.lvObjects.UseCompatibleStateImageBehavior = false;
      this.lvObjects.View = System.Windows.Forms.View.Details;
      this.lvObjects.SelectedIndexChanged += new System.EventHandler(this.lvObjects_SelectedIndexChanged);
      // 
      // id
      // 
      this.id.Text = "ID";
      this.id.Width = 57;
      // 
      // type
      // 
      this.type.Text = "Type";
      this.type.Width = 72;
      // 
      // tpPages
      // 
      this.tpPages.Controls.Add(this.lvPages);
      this.tpPages.Location = new System.Drawing.Point(24, 4);
      this.tpPages.Name = "tpPages";
      this.tpPages.Size = new System.Drawing.Size(167, 622);
      this.tpPages.TabIndex = 3;
      this.tpPages.Tag = "Pages";
      this.tpPages.Text = "Pages";
      // 
      // lvPages
      // 
      this.lvPages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmPage,
            this.clmSize});
      this.lvPages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvPages.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.lvPages.FullRowSelect = true;
      this.lvPages.HideSelection = false;
      this.lvPages.Location = new System.Drawing.Point(0, 0);
      this.lvPages.MultiSelect = false;
      this.lvPages.Name = "lvPages";
      this.lvPages.Size = new System.Drawing.Size(167, 622);
      this.lvPages.TabIndex = 0;
      this.lvPages.UseCompatibleStateImageBehavior = false;
      this.lvPages.View = System.Windows.Forms.View.Details;
      this.lvPages.SelectedIndexChanged += new System.EventHandler(this.lvPages_SelectedIndexChanged);
      // 
      // clmPage
      // 
      this.clmPage.Text = "Page";
      // 
      // clmSize
      // 
      this.clmSize.Text = "Size";
      this.clmSize.Width = 100;
      // 
      // pnlNavigation
      // 
      this.pnlNavigation.Controls.Add(this.tbcNavigation);
      this.pnlNavigation.Dock = System.Windows.Forms.DockStyle.Left;
      this.pnlNavigation.Location = new System.Drawing.Point(0, 0);
      this.pnlNavigation.Name = "pnlNavigation";
      this.pnlNavigation.Padding = new System.Windows.Forms.Padding(3, 0, 2, 0);
      this.pnlNavigation.Size = new System.Drawing.Size(200, 630);
      this.pnlNavigation.TabIndex = 1;
      // 
      // splitter
      // 
      this.splitter.BackColor = System.Drawing.SystemColors.Control;
      this.splitter.Location = new System.Drawing.Point(200, 0);
      this.splitter.Name = "splitter";
      this.splitter.Size = new System.Drawing.Size(3, 630);
      this.splitter.TabIndex = 2;
      this.splitter.TabStop = false;
      // 
      // pnlWorkArea
      // 
      this.pnlWorkArea.BackColor = System.Drawing.SystemColors.Control;
      this.pnlWorkArea.Controls.Add(this.tbcMain);
      this.pnlWorkArea.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlWorkArea.Location = new System.Drawing.Point(203, 0);
      this.pnlWorkArea.Name = "pnlWorkArea";
      this.pnlWorkArea.Padding = new System.Windows.Forms.Padding(2, 0, 4, 0);
      this.pnlWorkArea.Size = new System.Drawing.Size(787, 630);
      this.pnlWorkArea.TabIndex = 3;
      // 
      // tbcMain
      // 
      this.tbcMain.Controls.Add(this.tpData);
      this.tbcMain.Controls.Add(this.tpPdf);
      this.tbcMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tbcMain.Location = new System.Drawing.Point(2, 0);
      this.tbcMain.Name = "tbcMain";
      this.tbcMain.SelectedIndex = 0;
      this.tbcMain.Size = new System.Drawing.Size(781, 630);
      this.tbcMain.TabIndex = 0;
      this.tbcMain.SelectedIndexChanged += new System.EventHandler(this.tbcMain_SelectedIndexChanged);
      // 
      // tpData
      // 
      this.tpData.Location = new System.Drawing.Point(4, 22);
      this.tpData.Name = "tpData";
      this.tpData.Size = new System.Drawing.Size(773, 604);
      this.tpData.TabIndex = 0;
      this.tpData.Text = "Data";
      // 
      // tpPdf
      // 
      this.tpPdf.Location = new System.Drawing.Point(4, 22);
      this.tpPdf.Name = "tpPdf";
      this.tpPdf.Size = new System.Drawing.Size(773, 604);
      this.tpPdf.TabIndex = 1;
      this.tpPdf.Text = "PDF";
      // 
      // ExplorerPanel
      // 
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.pnlWorkArea);
      this.Controls.Add(this.splitter);
      this.Controls.Add(this.pnlNavigation);
      this.Name = "ExplorerPanel";
      this.Size = new System.Drawing.Size(990, 630);
      this.tbcNavigation.ResumeLayout(false);
      this.tpInfo.ResumeLayout(false);
      this.tpObjects.ResumeLayout(false);
      this.tpPages.ResumeLayout(false);
      this.pnlNavigation.ResumeLayout(false);
      this.pnlWorkArea.ResumeLayout(false);
      this.tbcMain.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void tbcNavigation_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ActivatePage(this.tbcNavigation.SelectedTab.Tag.ToString());
    }

    private void lvObjects_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = this.lvObjects.SelectedItems;
      if (items.Count > 0)
      {
        ListViewItem item = items[0];
        this.process.Navigator.SetNext((PdfObject)item.Tag);
        ActivateTab((PdfObject)item.Tag);
      }
    }

    private void lvPages_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = this.lvPages.SelectedItems;
      if (items.Count > 0)
      {
        ListViewItem item = items[0];
        this.process.Navigator.SetNext((PdfObject)item.Tag);
        ActivateTab((PdfObject)item.Tag);
      }
    }

    private void tbcMain_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      ActivateTab((PdfObject)Process.Navigator.Current);
    }
  }
}
