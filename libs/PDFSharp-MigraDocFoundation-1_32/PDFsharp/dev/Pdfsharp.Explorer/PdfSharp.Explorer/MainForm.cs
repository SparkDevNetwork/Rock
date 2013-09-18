using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// MainForm.
  /// </summary>
  public class MainForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.Panel clientArea;
    private System.Windows.Forms.StatusBarPanel statusBarPanel1;
    private System.Windows.Forms.StatusBarPanel statusBarPanel2;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem miOpen;
    private System.Windows.Forms.MenuItem miClose;
    private System.Windows.Forms.MenuItem miExit;
    private System.Windows.Forms.ImageList ilToolBar;
    private System.Windows.Forms.ToolBarButton tbbOpen;
    private System.Windows.Forms.ToolBarButton tbbSave;
    private System.Windows.Forms.ToolBarButton tbbBack;
    private System.Windows.Forms.ToolBarButton tbbForward;
    private System.Windows.Forms.ToolBarButton separator1;
    private System.Windows.Forms.ToolBar mainToolBar;
    private System.Windows.Forms.StatusBar mainStatusBar;
    private System.Windows.Forms.ToolBarButton separator2;
    private System.Windows.Forms.ToolBarButton tbbCopy;
    private System.ComponentModel.IContainer components;

    public MainForm(ExplorerProcess process)
    {
      this.process = process;
      InitializeComponent();
      this.explorer = new ExplorerPanel(this);
      explorer.Dock = DockStyle.Fill;
      this.clientArea.Controls.Add(explorer);

      this.title = this.Text;
    }
    string title;
    ExplorerPanel explorer;
    
    public ExplorerProcess Process
    {
      get {return this.process;}
    }
    ExplorerProcess process;

    void OpenDocument(string path)
    {
      try
      {
        Process.OpenDocument(path);
        this.Text = Path.GetFullPath(path) + " - " + this.title;
        this.explorer.OnNewDocument();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, this.title);
      }
    }

    void UpdateUI()
    {
      try
      {
        //this.tbbOpen.Enabled = false;
        //this.tbbSave.Enabled = false;
        //this.tbbBack.Enabled = this.process.Navigator.CanMoveBack;
        //this.tbbForward.Enabled = this.process.Navigator.CanMoveForward;
      }
      finally
      {
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

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      //OpenDocument("../../Output.pdf");
      if (this.process.Document != null)
      {
        this.Text = this.process.Filename + " - " + this.title;
        this.explorer.OnNewDocument();
      }
      UpdateUI();
    }

    void OpenFile()
    {
      using (OpenFileDialog dialog = new OpenFileDialog())
      {
        dialog.RestoreDirectory = true;
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        dialog.Multiselect = false;
        dialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          string fileName = dialog.FileName;
          OpenDocument(fileName);
          this.process.FormatDocument(fileName);
        }
      }
    }


    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
      this.mainMenu = new System.Windows.Forms.MainMenu();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.miOpen = new System.Windows.Forms.MenuItem();
      this.miClose = new System.Windows.Forms.MenuItem();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this.miExit = new System.Windows.Forms.MenuItem();
      this.mainToolBar = new System.Windows.Forms.ToolBar();
      this.tbbOpen = new System.Windows.Forms.ToolBarButton();
      this.tbbSave = new System.Windows.Forms.ToolBarButton();
      this.separator1 = new System.Windows.Forms.ToolBarButton();
      this.tbbBack = new System.Windows.Forms.ToolBarButton();
      this.tbbForward = new System.Windows.Forms.ToolBarButton();
      this.ilToolBar = new System.Windows.Forms.ImageList(this.components);
      this.mainStatusBar = new System.Windows.Forms.StatusBar();
      this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
      this.statusBarPanel2 = new System.Windows.Forms.StatusBarPanel();
      this.clientArea = new System.Windows.Forms.Panel();
      this.separator2 = new System.Windows.Forms.ToolBarButton();
      this.tbbCopy = new System.Windows.Forms.ToolBarButton();
      ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel2)).BeginInit();
      this.SuspendLayout();
      // 
      // mainMenu
      // 
      this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                             this.menuItem1});
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 0;
      this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.miOpen,
                                                                              this.miClose,
                                                                              this.menuItem3,
                                                                              this.miExit});
      this.menuItem1.Text = "&File";
      // 
      // miOpen
      // 
      this.miOpen.Index = 0;
      this.miOpen.Text = "&Open";
      this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
      // 
      // miClose
      // 
      this.miClose.Index = 1;
      this.miClose.Text = "&Close";
      this.miClose.Click += new System.EventHandler(this.miClose_Click);
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 2;
      this.menuItem3.Text = "-";
      // 
      // miExit
      // 
      this.miExit.Index = 3;
      this.miExit.Text = "E&xit";
      this.miExit.Click += new System.EventHandler(this.miExit_Click);
      // 
      // mainToolBar
      // 
      this.mainToolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
      this.mainToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                   this.tbbOpen,
                                                                                   this.separator1,
                                                                                   this.tbbCopy,
                                                                                   this.tbbSave,
                                                                                   this.separator2,
                                                                                   this.tbbBack,
                                                                                   this.tbbForward});
      this.mainToolBar.DropDownArrows = true;
      this.mainToolBar.ImageList = this.ilToolBar;
      this.mainToolBar.Location = new System.Drawing.Point(0, 0);
      this.mainToolBar.Name = "mainToolBar";
      this.mainToolBar.ShowToolTips = true;
      this.mainToolBar.Size = new System.Drawing.Size(992, 28);
      this.mainToolBar.TabIndex = 0;
      this.mainToolBar.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
      this.mainToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.mainToolBar_ButtonClick);
      // 
      // tbbOpen
      // 
      this.tbbOpen.ImageIndex = 0;
      this.tbbOpen.Tag = "Open";
      this.tbbOpen.Text = "Open";
      this.tbbOpen.ToolTipText = "Open PDF file";
      // 
      // tbbSave
      // 
      this.tbbSave.ImageIndex = 1;
      this.tbbSave.Tag = "Save";
      this.tbbSave.Text = "Save";
      this.tbbSave.ToolTipText = "Save stream to file";
      // 
      // separator1
      // 
      this.separator1.Enabled = false;
      this.separator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // tbbBack
      // 
      this.tbbBack.ImageIndex = 2;
      this.tbbBack.Tag = "Back";
      this.tbbBack.Text = "Back";
      this.tbbBack.ToolTipText = "Move to previous item";
      // 
      // tbbForward
      // 
      this.tbbForward.ImageIndex = 3;
      this.tbbForward.Tag = "Forward";
      this.tbbForward.Text = "Forward";
      this.tbbForward.ToolTipText = "Move to next item";
      // 
      // ilToolBar
      // 
      this.ilToolBar.ImageSize = new System.Drawing.Size(16, 16);
      this.ilToolBar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilToolBar.ImageStream")));
      this.ilToolBar.TransparentColor = System.Drawing.Color.Lime;
      // 
      // mainStatusBar
      // 
      this.mainStatusBar.Location = new System.Drawing.Point(0, 673);
      this.mainStatusBar.Name = "mainStatusBar";
      this.mainStatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
                                                                                     this.statusBarPanel1,
                                                                                     this.statusBarPanel2});
      this.mainStatusBar.ShowPanels = true;
      this.mainStatusBar.Size = new System.Drawing.Size(992, 22);
      this.mainStatusBar.TabIndex = 1;
      this.mainStatusBar.Text = "statusBar1";
      // 
      // clientArea
      // 
      this.clientArea.Dock = System.Windows.Forms.DockStyle.Fill;
      this.clientArea.Location = new System.Drawing.Point(0, 28);
      this.clientArea.Name = "clientArea";
      this.clientArea.Size = new System.Drawing.Size(992, 645);
      this.clientArea.TabIndex = 2;
      // 
      // separator2
      // 
      this.separator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // tbbCopy
      // 
      this.tbbCopy.ImageIndex = 4;
      this.tbbCopy.Tag = "Copy";
      this.tbbCopy.Text = "Copy";
      this.tbbCopy.ToolTipText = "Copy stream to clipboard";
      // 
      // MainForm
      // 
      this.AllowDrop = true;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(992, 695);
      this.Controls.Add(this.clientArea);
      this.Controls.Add(this.mainStatusBar);
      this.Controls.Add(this.mainToolBar);
      this.Menu = this.mainMenu;
      this.MinimumSize = new System.Drawing.Size(800, 600);
      this.Name = "MainForm";
      this.Text = "PDFsharp Document Explorer";
      ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.statusBarPanel2)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private void miOpen_Click(object sender, System.EventArgs e)
    {
      OpenFile();
    }

    private void miClose_Click(object sender, System.EventArgs e)
    {
    }

    private void miExit_Click(object sender, System.EventArgs e)
    {
      this.Close();
    }

    private void mainToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
    {
#if true_
      PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(@"G:\!StLa\PDFsharp Problems\06-05-22\Test1.pdf");
      PdfPage page = document.Pages[0];
      PdfAnnotations annotations = page.Annotations;
      object o = annotations[0];
      foreach (PdfAnnotation annotation in annotations)
      {
        annotation.GetType();
        string s = annotation.Contents;
        Debug.WriteLine(s);
      }
#endif
      switch ((string)e.Button.Tag)
      {
        case "Open":
          OpenFile();
          break;
      }
    
    }

  }
}
