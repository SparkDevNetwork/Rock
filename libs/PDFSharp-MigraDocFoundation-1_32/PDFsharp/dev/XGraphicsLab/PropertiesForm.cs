using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using XDrawing.TestLab.FormPages;

namespace XDrawing.TestLab
{
  /// <summary>
  /// PropertiesForm.
  /// </summary>
  public class PropertiesForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.TabControl tabMain;
    private System.Windows.Forms.TabPage tpgGeneral;
    private System.Windows.Forms.TabPage tpgPens;
    private System.Windows.Forms.TabPage tpgBrushes;
    private System.Windows.Forms.TabPage tpgFonts;
    private System.Windows.Forms.TabPage tpgPath;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.CheckBox btnAutoApply;
    private System.Windows.Forms.Button btnApply;
    private System.ComponentModel.Container components = null;

    public static PropertiesForm xxx;
    public PropertiesForm()
    {
      InitializeComponent();

      GraphicsProperties properties = XGraphicsLab.properties;

      Size pageSize = this.tpgGeneral.Size;
      TabControl tabControl;
      TabPage tabPage;

      // General
      GeneralPage generalPage = new GeneralPage();
      generalPage.UpdateDrawing +=  new UpdateDrawing(OnUpdateDrawing);
      generalPage.GeneralProperties = properties.General;
      this.tpgGeneral.Controls.Add(generalPage);
      UITools.SetTabPageColor(this.tpgGeneral);

      // Pens
      PenPage penPage;
      tabControl = new TabControl();
      tabControl.Size = pageSize;

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Pen1";
      penPage = new PenPage();
      penPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      penPage.PenProperty = properties.Pen1;
      tabPage.Controls.Add(penPage);
      tabControl.Controls.Add(tabPage);

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Pen2";
      penPage = new PenPage();
      penPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      penPage.PenProperty = properties.Pen2;
      tabPage.Controls.Add(penPage);
      tabControl.Controls.Add(tabPage);

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Pen3";
      penPage = new PenPage();
      penPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      penPage.PenProperty = properties.Pen3;
      tabPage.Controls.Add(penPage);
      tabControl.Controls.Add(tabPage);

      this.tpgPens.Controls.Add(tabControl);
      UITools.SetTabPageColor(this.tpgPens);

      // Brushes
      BrushPage brushPage;
      tabControl = new TabControl();
      tabControl.Size = pageSize;

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Brush1";
      brushPage = new BrushPage();
      brushPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      brushPage.BrushProperty = properties.Brush1;
      tabPage.Controls.Add(brushPage);
      tabControl.Controls.Add(tabPage);

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Brush2";
      brushPage = new BrushPage();
      brushPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      brushPage.BrushProperty = properties.Brush2;
      tabPage.Controls.Add(brushPage);
      tabControl.Controls.Add(tabPage);

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Brush3";
      brushPage = new BrushPage();
      brushPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      brushPage.BrushProperty = properties.Brush3;
      tabPage.Controls.Add(brushPage);
      tabControl.Controls.Add(tabPage);

      this.tpgBrushes.Controls.Add(tabControl);
      UITools.SetTabPageColor(this.tpgBrushes);

      // Fonts
      FontPage fonthPage;
      tabControl = new TabControl();
      tabControl.Size = pageSize;

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Font1";
      fonthPage = new FontPage();
      fonthPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      fonthPage.FontProperty = properties.Font1;
      tabPage.Controls.Add(fonthPage);
      tabControl.Controls.Add(tabPage);

      tabPage = new TabPage();
      UITools.SetTabPageColor(tabPage);
      tabPage.Text = "Font2";
      fonthPage = new FontPage();
      fonthPage.UpdateDrawing += new UpdateDrawing(OnUpdateDrawing);
      fonthPage.FontProperty = properties.Font2;
      tabPage.Controls.Add(fonthPage);
      tabControl.Controls.Add(tabPage);

      this.tpgFonts.Controls.Add(tabControl);
      UITools.SetTabPageColor(this.tpgFonts);

      // Path
      //GeneralPage generalPage = new GeneralPage();
      //generalPage.UpdateDrawing +=  new UpdateDrawing(OnUpdateDrawing);
      //generalPage.GeneralProperties = properties.General;
      //this.tpgGeneral.Controls.Add(generalPage);
      UITools.SetTabPageColor(this.tpgPath);

    }

    void UpdateDrawing()
    {
      PreviewForm preview = XGraphicsLab.mainForm.preview;
      if (preview != null)
        preview.UpdateDrawing();

#if NET_3_0
      if (XGraphicsLab.mainForm.wpfPreview != null)
        XGraphicsLab.mainForm.wpfPreview.OnRender();
#endif
    }

    void OnUpdateDrawing()
    {
      if (XGraphicsLab.properties.AutoApply)
        UpdateDrawing();
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tabMain = new System.Windows.Forms.TabControl();
      this.tpgGeneral = new System.Windows.Forms.TabPage();
      this.tpgPens = new System.Windows.Forms.TabPage();
      this.tpgBrushes = new System.Windows.Forms.TabPage();
      this.tpgFonts = new System.Windows.Forms.TabPage();
      this.tpgPath = new System.Windows.Forms.TabPage();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnAutoApply = new System.Windows.Forms.CheckBox();
      this.btnApply = new System.Windows.Forms.Button();
      this.tabMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabMain
      // 
      this.tabMain.Controls.Add(this.tpgGeneral);
      this.tabMain.Controls.Add(this.tpgPens);
      this.tabMain.Controls.Add(this.tpgBrushes);
      this.tabMain.Controls.Add(this.tpgFonts);
      this.tabMain.Controls.Add(this.tpgPath);
      this.tabMain.Location = new System.Drawing.Point(8, 8);
      this.tabMain.Name = "tabMain";
      this.tabMain.SelectedIndex = 0;
      this.tabMain.Size = new System.Drawing.Size(528, 296);
      this.tabMain.TabIndex = 0;
      // 
      // tpgGeneral
      // 
      this.tpgGeneral.BackColor = System.Drawing.SystemColors.Control;
      this.tpgGeneral.Location = new System.Drawing.Point(4, 22);
      this.tpgGeneral.Name = "tpgGeneral";
      this.tpgGeneral.Size = new System.Drawing.Size(520, 270);
      this.tpgGeneral.TabIndex = 0;
      this.tpgGeneral.Text = "General";
      // 
      // tpgPens
      // 
      this.tpgPens.Location = new System.Drawing.Point(4, 22);
      this.tpgPens.Name = "tpgPens";
      this.tpgPens.Size = new System.Drawing.Size(520, 270);
      this.tpgPens.TabIndex = 1;
      this.tpgPens.Text = "Pens";
      // 
      // tpgBrushes
      // 
      this.tpgBrushes.Location = new System.Drawing.Point(4, 22);
      this.tpgBrushes.Name = "tpgBrushes";
      this.tpgBrushes.Size = new System.Drawing.Size(520, 270);
      this.tpgBrushes.TabIndex = 2;
      this.tpgBrushes.Text = "Brushes";
      // 
      // tpgFonts
      // 
      this.tpgFonts.Location = new System.Drawing.Point(4, 22);
      this.tpgFonts.Name = "tpgFonts";
      this.tpgFonts.Size = new System.Drawing.Size(520, 270);
      this.tpgFonts.TabIndex = 3;
      this.tpgFonts.Text = "Fonts";
      // 
      // tpgPath
      // 
      this.tpgPath.Location = new System.Drawing.Point(4, 22);
      this.tpgPath.Name = "tpgPath";
      this.tpgPath.Size = new System.Drawing.Size(520, 270);
      this.tpgPath.TabIndex = 4;
      this.tpgPath.Text = "Path";
      // 
      // btnClose
      // 
      this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnClose.Location = new System.Drawing.Point(480, 328);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(75, 23);
      this.btnClose.TabIndex = 1;
      this.btnClose.Text = "Close";
      this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
      // 
      // btnAutoApply
      // 
      this.btnAutoApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnAutoApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnAutoApply.Location = new System.Drawing.Point(16, 328);
      this.btnAutoApply.Name = "btnAutoApply";
      this.btnAutoApply.Size = new System.Drawing.Size(104, 20);
      this.btnAutoApply.TabIndex = 2;
      this.btnAutoApply.Text = "AutoApply";
      this.btnAutoApply.CheckedChanged += new System.EventHandler(this.btnAutoApply_CheckedChanged);
      // 
      // btnApply
      // 
      this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnApply.Location = new System.Drawing.Point(396, 328);
      this.btnApply.Name = "btnApply";
      this.btnApply.Size = new System.Drawing.Size(75, 23);
      this.btnApply.TabIndex = 3;
      this.btnApply.Text = "Apply";
      this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
      // 
      // PropertiesForm
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.ClientSize = new System.Drawing.Size(564, 358);
      this.Controls.Add(this.btnApply);
      this.Controls.Add(this.btnAutoApply);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.tabMain);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Location = new System.Drawing.Point(5, 5);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PropertiesForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "XGraphics Properties";
      this.tabMain.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad (e);
      this.btnAutoApply.Checked = XGraphicsLab.properties.AutoApply;
      this.btnApply.Enabled = !XGraphicsLab.properties.AutoApply;

      //UITools.SetTabPageColor(this.tpgGeneral);

    }

    private void btnClose_Click(object sender, System.EventArgs e)
    {
    
    }

    private void btnAutoApply_CheckedChanged(object sender, System.EventArgs e)
    {
      XGraphicsLab.properties.AutoApply = this.btnAutoApply.Checked;
      if (XGraphicsLab.properties.AutoApply)
      {
        this.btnApply.Enabled = false;
        UpdateDrawing();
      }
      else
        this.btnApply.Enabled = true;
    }

    private void btnApply_Click(object sender, System.EventArgs e)
    {
      UpdateDrawing();
    }
  }
}
