using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Forms;
using XDrawing.TestLab.Tester;

namespace XDrawing.TestLab
{
  /// <summary>
  /// Form1.
  /// </summary>
  public class MainForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.MainMenu menuBar;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.ToolBar toolBar;
    private System.Windows.Forms.StatusBar statusBar;
    private System.Windows.Forms.ToolBarButton toolBarButton1;
    private System.Windows.Forms.ToolBarButton toolBarButton2;
    private System.Windows.Forms.ToolBarButton toolBarButton3;
    private System.Windows.Forms.ToolBarButton toolBarButton4;
    private System.Windows.Forms.RadioButton radioButton7;
    private System.Windows.Forms.RadioButton radioButton8;
    private System.Windows.Forms.RadioButton radioButton9;
    private System.Windows.Forms.RadioButton radioButton10;
    private System.Windows.Forms.RadioButton radioButton11;
    private System.Windows.Forms.RadioButton radioButton12;
    private System.Windows.Forms.RadioButton radioButton18;
    private System.Windows.Forms.RadioButton radioButton20;
    private System.Windows.Forms.RadioButton radioButton21;
    private System.Windows.Forms.RadioButton radioButton22;
    private System.Windows.Forms.RadioButton radioButton23;
    private System.Windows.Forms.RadioButton radioButton24;
    private System.Windows.Forms.RadioButton radioButton25;
    private System.Windows.Forms.RadioButton radioButton26;
    private System.Windows.Forms.RadioButton radioButton28;
    private System.Windows.Forms.RadioButton radioButton33;
    private System.Windows.Forms.RadioButton radioButton34;
    private System.Windows.Forms.RadioButton radioButton35;
    private System.Windows.Forms.RadioButton radioButton38;
    private System.Windows.Forms.RadioButton btnAcro8Bug;
    private System.Windows.Forms.RadioButton radioButton42;
    private System.Windows.Forms.RadioButton radioButton45;
    private System.Windows.Forms.RadioButton radioButton46;
    private System.Windows.Forms.RadioButton radioButton47;
    private System.Windows.Forms.RadioButton radioButton48;
    private System.Windows.Forms.RadioButton radioButton57;
    private System.Windows.Forms.RadioButton radioButton58;
    private System.Windows.Forms.RadioButton radioButton59;
    private System.Windows.Forms.RadioButton radioButton60;
    private System.Windows.Forms.TabPage tabLines;
    private System.Windows.Forms.TabPage tabShapes;
    private System.Windows.Forms.TabPage tabPaths;
    private System.Windows.Forms.TabPage tabText;
    private System.Windows.Forms.TabPage tabImages;
    private System.Windows.Forms.RadioButton btnShapesPolygon;
    private System.Windows.Forms.RadioButton btnShapesEllipse;
    private System.Windows.Forms.RadioButton btnShapesRectangle;
    private System.Windows.Forms.RadioButton btnShapesTransform;
    private System.Windows.Forms.RadioButton btnTextTransform1;
    private System.Windows.Forms.RadioButton btnTextAlign;
    private System.Windows.Forms.RadioButton btnTextTransform2;
    private System.Windows.Forms.RadioButton btnImagesJpeg;
    private System.Windows.Forms.RadioButton btnImagesPng;
    private System.Windows.Forms.TabPage tabMiscellaneous;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton13;
    private System.Windows.Forms.RadioButton radioButton14;
    private System.Windows.Forms.RadioButton radioButton15;
    private System.Windows.Forms.RadioButton radioButton19;
    private System.Windows.Forms.RadioButton radioButton37;
    private System.Windows.Forms.RadioButton radioButton43;
    private System.Windows.Forms.RadioButton radioButton44;
    private System.Windows.Forms.RadioButton radioButton50;
    private System.Windows.Forms.RadioButton radioButton51;
    private System.Windows.Forms.RadioButton btnSpiroGraph;
    private System.Windows.Forms.RadioButton btnLinesArc;
    private System.Windows.Forms.RadioButton btnLinesCircleArc;
    private System.Windows.Forms.RadioButton btnShapesPie;
    private System.Windows.Forms.RadioButton btnShapesRoundedRectangle;
    private System.Windows.Forms.RadioButton btnPathClipTest;
    private System.Windows.Forms.RadioButton btnText01;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton btnTextCodePage;
    private System.Windows.Forms.RadioButton radioButton5;
    private System.Windows.Forms.RadioButton radioButton16;
    private System.Windows.Forms.RadioButton radioButton36;
    private System.Windows.Forms.RadioButton radioButton61;
    private System.Windows.Forms.RadioButton radioButton62;
    private System.Windows.Forms.RadioButton radioButton63;
    private System.Windows.Forms.RadioButton radioButton64;
    private System.Windows.Forms.RadioButton btnLayout1;
    private System.Windows.Forms.RadioButton radioButton66;
    private System.Windows.Forms.RadioButton btnLinesCurve;
    private System.Windows.Forms.RadioButton btnShapesClosedCurve;
    private System.Windows.Forms.RadioButton btnLinesStraightLines;
    private System.Windows.Forms.RadioButton btnLinesPolyLines;
    private System.Windows.Forms.RadioButton btnPathTest01;
    private System.Windows.Forms.RadioButton btnPathFlatten;
    private System.Windows.Forms.RadioButton btnPathWiden;
    private System.Windows.Forms.RadioButton btnPathGlyph;
    private System.Windows.Forms.RadioButton btnPathClipGlyph;
    private System.Windows.Forms.RadioButton btnImagesGif;
    private System.Windows.Forms.RadioButton btnImagesTiff;
    private System.Windows.Forms.RadioButton btnImagesBmp;
    private System.Windows.Forms.RadioButton btnClock;
    private System.Windows.Forms.RadioButton btnLinesBézierCurve;
    private System.Windows.Forms.RadioButton btnLinesBézierCurves;
    private System.Windows.Forms.RadioButton btnImagesFormXObject;
    private System.Windows.Forms.RadioButton btnImagesBmpOS2;
    private System.Windows.Forms.TabControl tpcMain;
    private System.Windows.Forms.TabPage tabBrushes;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton4;
    private System.Windows.Forms.RadioButton radioButton17;
    private System.Windows.Forms.RadioButton radioButton27;
    private System.Windows.Forms.RadioButton radioButton29;
    private System.Windows.Forms.RadioButton radioButton30;
    private System.Windows.Forms.RadioButton radioButton31;
    private System.Windows.Forms.RadioButton radioButton32;
    private System.Windows.Forms.RadioButton radioButton39;
    private System.Windows.Forms.RadioButton radioButton49;
    private System.Windows.Forms.RadioButton radioButton52;
    private System.Windows.Forms.RadioButton btnBrushesLinearGradient;
    private System.Windows.Forms.RadioButton btnClipTest1;
    private System.Windows.Forms.RadioButton btnBeginContainer;
    private System.Windows.Forms.RadioButton btnShapesSave;
    private System.Windows.Forms.TabPage tabBarcodes;
    private RadioButton radioButton6;
    private RadioButton btnTypes;
    private RadioButton btnOrientation;
    private RadioButton radioButton55;
    private RadioButton radioButton68;
    private RadioButton radioButton69;
    private RadioButton radioButton70;
    private RadioButton radioButton71;
    private RadioButton radioButton72;
    private RadioButton radioButton73;
    private RadioButton radioButton74;
    private RadioButton radioButton75;
    private System.Windows.Forms.RadioButton btnImagesFormXObjectTemplate;
    private RadioButton btnShapesMigraDocObject;
    private System.Windows.Forms.RadioButton btnRealizeTest;
    private System.Windows.Forms.RadioButton btnUnicode;
    private IContainer components = null;

    public MainForm()
    {
      XGraphicsLab.mainForm = this;
      InitializeComponent();

      UITools.SetTabPageColor(this.tabLines);
      UITools.SetTabPageColor(this.tabShapes);
      UITools.SetTabPageColor(this.tabPaths);
      UITools.SetTabPageColor(this.tabBrushes);
      UITools.SetTabPageColor(this.tabText);
      UITools.SetTabPageColor(this.tabImages);
      UITools.SetTabPageColor(this.tabBarcodes);
      UITools.SetTabPageColor(this.tabMiscellaneous);

      //this.tester = new TestLines01();
      toolBarButton3.Pushed = true;
      ShowPreview(true);
    }

    TesterBase tester;

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

    void ShowPreview(bool show)
    {
      if (this.preview == null)
        return;

      if (show)
      {
        this.preview.Show();
        if (this.tester != null)
          this.preview.SetRenderEvent(new PagePreview.RenderEvent(this.tester.RenderPage));
      }
      else
        this.preview.Hide();
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.menuBar = new System.Windows.Forms.MainMenu(this.components);
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.toolBar = new System.Windows.Forms.ToolBar();
      this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
      this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
      this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
      this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
      this.statusBar = new System.Windows.Forms.StatusBar();
      this.tpcMain = new System.Windows.Forms.TabControl();
      this.tabLines = new System.Windows.Forms.TabPage();
      this.btnLinesBézierCurve = new System.Windows.Forms.RadioButton();
      this.btnLinesPolyLines = new System.Windows.Forms.RadioButton();
      this.btnLinesStraightLines = new System.Windows.Forms.RadioButton();
      this.btnLinesArc = new System.Windows.Forms.RadioButton();
      this.btnLinesCircleArc = new System.Windows.Forms.RadioButton();
      this.btnLinesCurve = new System.Windows.Forms.RadioButton();
      this.radioButton7 = new System.Windows.Forms.RadioButton();
      this.radioButton8 = new System.Windows.Forms.RadioButton();
      this.radioButton9 = new System.Windows.Forms.RadioButton();
      this.radioButton10 = new System.Windows.Forms.RadioButton();
      this.radioButton11 = new System.Windows.Forms.RadioButton();
      this.radioButton12 = new System.Windows.Forms.RadioButton();
      this.btnLinesBézierCurves = new System.Windows.Forms.RadioButton();
      this.tabBrushes = new System.Windows.Forms.TabPage();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton4 = new System.Windows.Forms.RadioButton();
      this.btnBrushesLinearGradient = new System.Windows.Forms.RadioButton();
      this.radioButton17 = new System.Windows.Forms.RadioButton();
      this.radioButton27 = new System.Windows.Forms.RadioButton();
      this.radioButton29 = new System.Windows.Forms.RadioButton();
      this.radioButton30 = new System.Windows.Forms.RadioButton();
      this.radioButton31 = new System.Windows.Forms.RadioButton();
      this.radioButton32 = new System.Windows.Forms.RadioButton();
      this.radioButton39 = new System.Windows.Forms.RadioButton();
      this.radioButton49 = new System.Windows.Forms.RadioButton();
      this.radioButton52 = new System.Windows.Forms.RadioButton();
      this.tabImages = new System.Windows.Forms.TabPage();
      this.btnImagesGif = new System.Windows.Forms.RadioButton();
      this.btnImagesPng = new System.Windows.Forms.RadioButton();
      this.btnImagesJpeg = new System.Windows.Forms.RadioButton();
      this.btnImagesTiff = new System.Windows.Forms.RadioButton();
      this.btnImagesBmp = new System.Windows.Forms.RadioButton();
      this.btnImagesBmpOS2 = new System.Windows.Forms.RadioButton();
      this.btnImagesFormXObject = new System.Windows.Forms.RadioButton();
      this.btnImagesFormXObjectTemplate = new System.Windows.Forms.RadioButton();
      this.radioButton57 = new System.Windows.Forms.RadioButton();
      this.radioButton58 = new System.Windows.Forms.RadioButton();
      this.radioButton59 = new System.Windows.Forms.RadioButton();
      this.radioButton60 = new System.Windows.Forms.RadioButton();
      this.tabPaths = new System.Windows.Forms.TabPage();
      this.radioButton25 = new System.Windows.Forms.RadioButton();
      this.radioButton26 = new System.Windows.Forms.RadioButton();
      this.btnPathTest01 = new System.Windows.Forms.RadioButton();
      this.radioButton28 = new System.Windows.Forms.RadioButton();
      this.btnPathFlatten = new System.Windows.Forms.RadioButton();
      this.btnPathWiden = new System.Windows.Forms.RadioButton();
      this.btnPathGlyph = new System.Windows.Forms.RadioButton();
      this.btnPathClipGlyph = new System.Windows.Forms.RadioButton();
      this.radioButton33 = new System.Windows.Forms.RadioButton();
      this.radioButton34 = new System.Windows.Forms.RadioButton();
      this.radioButton35 = new System.Windows.Forms.RadioButton();
      this.btnPathClipTest = new System.Windows.Forms.RadioButton();
      this.tabShapes = new System.Windows.Forms.TabPage();
      this.btnShapesMigraDocObject = new System.Windows.Forms.RadioButton();
      this.btnClipTest1 = new System.Windows.Forms.RadioButton();
      this.btnShapesPolygon = new System.Windows.Forms.RadioButton();
      this.btnShapesEllipse = new System.Windows.Forms.RadioButton();
      this.btnShapesRectangle = new System.Windows.Forms.RadioButton();
      this.btnShapesPie = new System.Windows.Forms.RadioButton();
      this.btnShapesClosedCurve = new System.Windows.Forms.RadioButton();
      this.radioButton18 = new System.Windows.Forms.RadioButton();
      this.btnShapesTransform = new System.Windows.Forms.RadioButton();
      this.radioButton20 = new System.Windows.Forms.RadioButton();
      this.radioButton21 = new System.Windows.Forms.RadioButton();
      this.radioButton22 = new System.Windows.Forms.RadioButton();
      this.radioButton23 = new System.Windows.Forms.RadioButton();
      this.radioButton24 = new System.Windows.Forms.RadioButton();
      this.btnShapesRoundedRectangle = new System.Windows.Forms.RadioButton();
      this.btnBeginContainer = new System.Windows.Forms.RadioButton();
      this.btnShapesSave = new System.Windows.Forms.RadioButton();
      this.tabText = new System.Windows.Forms.TabPage();
      this.btnTextAlign = new System.Windows.Forms.RadioButton();
      this.radioButton38 = new System.Windows.Forms.RadioButton();
      this.btnText01 = new System.Windows.Forms.RadioButton();
      this.btnRealizeTest = new System.Windows.Forms.RadioButton();
      this.btnAcro8Bug = new System.Windows.Forms.RadioButton();
      this.radioButton42 = new System.Windows.Forms.RadioButton();
      this.btnTextTransform1 = new System.Windows.Forms.RadioButton();
      this.btnTextTransform2 = new System.Windows.Forms.RadioButton();
      this.radioButton45 = new System.Windows.Forms.RadioButton();
      this.radioButton46 = new System.Windows.Forms.RadioButton();
      this.radioButton47 = new System.Windows.Forms.RadioButton();
      this.radioButton48 = new System.Windows.Forms.RadioButton();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.btnTextCodePage = new System.Windows.Forms.RadioButton();
      this.radioButton5 = new System.Windows.Forms.RadioButton();
      this.radioButton16 = new System.Windows.Forms.RadioButton();
      this.radioButton36 = new System.Windows.Forms.RadioButton();
      this.radioButton61 = new System.Windows.Forms.RadioButton();
      this.radioButton62 = new System.Windows.Forms.RadioButton();
      this.radioButton63 = new System.Windows.Forms.RadioButton();
      this.radioButton64 = new System.Windows.Forms.RadioButton();
      this.btnLayout1 = new System.Windows.Forms.RadioButton();
      this.radioButton66 = new System.Windows.Forms.RadioButton();
      this.btnUnicode = new System.Windows.Forms.RadioButton();
      this.tabBarcodes = new System.Windows.Forms.TabPage();
      this.radioButton6 = new System.Windows.Forms.RadioButton();
      this.btnTypes = new System.Windows.Forms.RadioButton();
      this.btnOrientation = new System.Windows.Forms.RadioButton();
      this.radioButton55 = new System.Windows.Forms.RadioButton();
      this.radioButton68 = new System.Windows.Forms.RadioButton();
      this.radioButton69 = new System.Windows.Forms.RadioButton();
      this.radioButton70 = new System.Windows.Forms.RadioButton();
      this.radioButton71 = new System.Windows.Forms.RadioButton();
      this.radioButton72 = new System.Windows.Forms.RadioButton();
      this.radioButton73 = new System.Windows.Forms.RadioButton();
      this.radioButton74 = new System.Windows.Forms.RadioButton();
      this.radioButton75 = new System.Windows.Forms.RadioButton();
      this.tabMiscellaneous = new System.Windows.Forms.TabPage();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.btnClock = new System.Windows.Forms.RadioButton();
      this.btnSpiroGraph = new System.Windows.Forms.RadioButton();
      this.radioButton13 = new System.Windows.Forms.RadioButton();
      this.radioButton14 = new System.Windows.Forms.RadioButton();
      this.radioButton15 = new System.Windows.Forms.RadioButton();
      this.radioButton19 = new System.Windows.Forms.RadioButton();
      this.radioButton37 = new System.Windows.Forms.RadioButton();
      this.radioButton43 = new System.Windows.Forms.RadioButton();
      this.radioButton44 = new System.Windows.Forms.RadioButton();
      this.radioButton50 = new System.Windows.Forms.RadioButton();
      this.radioButton51 = new System.Windows.Forms.RadioButton();
      this.tpcMain.SuspendLayout();
      this.tabLines.SuspendLayout();
      this.tabBrushes.SuspendLayout();
      this.tabImages.SuspendLayout();
      this.tabPaths.SuspendLayout();
      this.tabShapes.SuspendLayout();
      this.tabText.SuspendLayout();
      this.tabBarcodes.SuspendLayout();
      this.tabMiscellaneous.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuBar
      // 
      this.menuBar.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
      // 
      // menuItem1
      // 
      this.menuItem1.Index = 0;
      this.menuItem1.Text = "File";
      // 
      // toolBar
      // 
      this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.toolBarButton4,
            this.toolBarButton1,
            this.toolBarButton2,
            this.toolBarButton3});
      this.toolBar.DropDownArrows = true;
      this.toolBar.Location = new System.Drawing.Point(0, 0);
      this.toolBar.Name = "toolBar";
      this.toolBar.ShowToolTips = true;
      this.toolBar.Size = new System.Drawing.Size(572, 28);
      this.toolBar.TabIndex = 0;
      this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
      // 
      // toolBarButton4
      // 
      this.toolBarButton4.Name = "toolBarButton4";
      this.toolBarButton4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // toolBarButton1
      // 
      this.toolBarButton1.Name = "toolBarButton1";
      // 
      // toolBarButton2
      // 
      this.toolBarButton2.Name = "toolBarButton2";
      // 
      // toolBarButton3
      // 
      this.toolBarButton3.Name = "toolBarButton3";
      this.toolBarButton3.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
      this.toolBarButton3.Tag = "3";
      // 
      // statusBar
      // 
      this.statusBar.Location = new System.Drawing.Point(0, 384);
      this.statusBar.Name = "statusBar";
      this.statusBar.Size = new System.Drawing.Size(572, 22);
      this.statusBar.TabIndex = 1;
      // 
      // tpcMain
      // 
      this.tpcMain.Controls.Add(this.tabLines);
      this.tpcMain.Controls.Add(this.tabBrushes);
      this.tpcMain.Controls.Add(this.tabImages);
      this.tpcMain.Controls.Add(this.tabPaths);
      this.tpcMain.Controls.Add(this.tabShapes);
      this.tpcMain.Controls.Add(this.tabText);
      this.tpcMain.Controls.Add(this.tabBarcodes);
      this.tpcMain.Controls.Add(this.tabMiscellaneous);
      this.tpcMain.Location = new System.Drawing.Point(16, 44);
      this.tpcMain.Name = "tpcMain";
      this.tpcMain.SelectedIndex = 0;
      this.tpcMain.Size = new System.Drawing.Size(544, 328);
      this.tpcMain.TabIndex = 3;
      this.tpcMain.SelectedIndexChanged += new System.EventHandler(this.tpgMain_SelectedIndexChanged);
      // 
      // tabLines
      // 
      this.tabLines.Controls.Add(this.btnLinesBézierCurve);
      this.tabLines.Controls.Add(this.btnLinesPolyLines);
      this.tabLines.Controls.Add(this.btnLinesStraightLines);
      this.tabLines.Controls.Add(this.btnLinesArc);
      this.tabLines.Controls.Add(this.btnLinesCircleArc);
      this.tabLines.Controls.Add(this.btnLinesCurve);
      this.tabLines.Controls.Add(this.radioButton7);
      this.tabLines.Controls.Add(this.radioButton8);
      this.tabLines.Controls.Add(this.radioButton9);
      this.tabLines.Controls.Add(this.radioButton10);
      this.tabLines.Controls.Add(this.radioButton11);
      this.tabLines.Controls.Add(this.radioButton12);
      this.tabLines.Controls.Add(this.btnLinesBézierCurves);
      this.tabLines.Location = new System.Drawing.Point(4, 22);
      this.tabLines.Name = "tabLines";
      this.tabLines.Size = new System.Drawing.Size(536, 302);
      this.tabLines.TabIndex = 0;
      this.tabLines.Text = "Lines";
      // 
      // btnLinesBézierCurve
      // 
      this.btnLinesBézierCurve.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesBézierCurve.Location = new System.Drawing.Point(8, 52);
      this.btnLinesBézierCurve.Name = "btnLinesBézierCurve";
      this.btnLinesBézierCurve.Size = new System.Drawing.Size(104, 16);
      this.btnLinesBézierCurve.TabIndex = 11;
      this.btnLinesBézierCurve.Tag = "LinesBézierCurve";
      this.btnLinesBézierCurve.Text = "Bézier Curve";
      this.btnLinesBézierCurve.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesPolyLines
      // 
      this.btnLinesPolyLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesPolyLines.Location = new System.Drawing.Point(8, 32);
      this.btnLinesPolyLines.Name = "btnLinesPolyLines";
      this.btnLinesPolyLines.Size = new System.Drawing.Size(104, 16);
      this.btnLinesPolyLines.TabIndex = 4;
      this.btnLinesPolyLines.Tag = "LinesPolyLines";
      this.btnLinesPolyLines.Text = "Poly Lines";
      this.btnLinesPolyLines.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesStraightLines
      // 
      this.btnLinesStraightLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesStraightLines.Location = new System.Drawing.Point(8, 12);
      this.btnLinesStraightLines.Name = "btnLinesStraightLines";
      this.btnLinesStraightLines.Size = new System.Drawing.Size(104, 16);
      this.btnLinesStraightLines.TabIndex = 3;
      this.btnLinesStraightLines.Tag = "LinesStraightLines";
      this.btnLinesStraightLines.Text = "Straight Lines";
      this.btnLinesStraightLines.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesArc
      // 
      this.btnLinesArc.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesArc.Location = new System.Drawing.Point(8, 92);
      this.btnLinesArc.Name = "btnLinesArc";
      this.btnLinesArc.Size = new System.Drawing.Size(104, 16);
      this.btnLinesArc.TabIndex = 10;
      this.btnLinesArc.Tag = "LinesArc";
      this.btnLinesArc.Text = "Arc";
      this.btnLinesArc.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesCircleArc
      // 
      this.btnLinesCircleArc.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesCircleArc.Location = new System.Drawing.Point(8, 116);
      this.btnLinesCircleArc.Name = "btnLinesCircleArc";
      this.btnLinesCircleArc.Size = new System.Drawing.Size(104, 16);
      this.btnLinesCircleArc.TabIndex = 12;
      this.btnLinesCircleArc.Tag = "LinesCircleArc";
      this.btnLinesCircleArc.Text = "Circle Arc";
      this.btnLinesCircleArc.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesCurve
      // 
      this.btnLinesCurve.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesCurve.Location = new System.Drawing.Point(8, 136);
      this.btnLinesCurve.Name = "btnLinesCurve";
      this.btnLinesCurve.Size = new System.Drawing.Size(104, 16);
      this.btnLinesCurve.TabIndex = 14;
      this.btnLinesCurve.Tag = "LinesCurve";
      this.btnLinesCurve.Text = "Curve";
      this.btnLinesCurve.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton7
      // 
      this.radioButton7.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton7.Location = new System.Drawing.Point(8, 156);
      this.radioButton7.Name = "radioButton7";
      this.radioButton7.Size = new System.Drawing.Size(104, 16);
      this.radioButton7.TabIndex = 13;
      this.radioButton7.Text = "Test07";
      this.radioButton7.Visible = false;
      this.radioButton7.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton8
      // 
      this.radioButton8.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton8.Location = new System.Drawing.Point(8, 176);
      this.radioButton8.Name = "radioButton8";
      this.radioButton8.Size = new System.Drawing.Size(104, 16);
      this.radioButton8.TabIndex = 6;
      this.radioButton8.Text = "Test08";
      this.radioButton8.Visible = false;
      this.radioButton8.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton9
      // 
      this.radioButton9.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton9.Location = new System.Drawing.Point(8, 200);
      this.radioButton9.Name = "radioButton9";
      this.radioButton9.Size = new System.Drawing.Size(104, 16);
      this.radioButton9.TabIndex = 5;
      this.radioButton9.Text = "Test09";
      this.radioButton9.Visible = false;
      this.radioButton9.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton10
      // 
      this.radioButton10.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton10.Location = new System.Drawing.Point(8, 220);
      this.radioButton10.Name = "radioButton10";
      this.radioButton10.Size = new System.Drawing.Size(104, 16);
      this.radioButton10.TabIndex = 7;
      this.radioButton10.Text = "Test10";
      this.radioButton10.Visible = false;
      this.radioButton10.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton11
      // 
      this.radioButton11.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton11.Location = new System.Drawing.Point(8, 240);
      this.radioButton11.Name = "radioButton11";
      this.radioButton11.Size = new System.Drawing.Size(104, 16);
      this.radioButton11.TabIndex = 9;
      this.radioButton11.Text = "Test11";
      this.radioButton11.Visible = false;
      this.radioButton11.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton12
      // 
      this.radioButton12.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton12.Location = new System.Drawing.Point(8, 260);
      this.radioButton12.Name = "radioButton12";
      this.radioButton12.Size = new System.Drawing.Size(104, 16);
      this.radioButton12.TabIndex = 8;
      this.radioButton12.Text = "Test12";
      this.radioButton12.Visible = false;
      this.radioButton12.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLinesBézierCurves
      // 
      this.btnLinesBézierCurves.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLinesBézierCurves.Location = new System.Drawing.Point(8, 72);
      this.btnLinesBézierCurves.Name = "btnLinesBézierCurves";
      this.btnLinesBézierCurves.Size = new System.Drawing.Size(104, 16);
      this.btnLinesBézierCurves.TabIndex = 11;
      this.btnLinesBézierCurves.Tag = "LinesBézierCurves";
      this.btnLinesBézierCurves.Text = "Bézier Curves";
      this.btnLinesBézierCurves.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabBrushes
      // 
      this.tabBrushes.Controls.Add(this.radioButton2);
      this.tabBrushes.Controls.Add(this.radioButton4);
      this.tabBrushes.Controls.Add(this.btnBrushesLinearGradient);
      this.tabBrushes.Controls.Add(this.radioButton17);
      this.tabBrushes.Controls.Add(this.radioButton27);
      this.tabBrushes.Controls.Add(this.radioButton29);
      this.tabBrushes.Controls.Add(this.radioButton30);
      this.tabBrushes.Controls.Add(this.radioButton31);
      this.tabBrushes.Controls.Add(this.radioButton32);
      this.tabBrushes.Controls.Add(this.radioButton39);
      this.tabBrushes.Controls.Add(this.radioButton49);
      this.tabBrushes.Controls.Add(this.radioButton52);
      this.tabBrushes.Location = new System.Drawing.Point(4, 22);
      this.tabBrushes.Name = "tabBrushes";
      this.tabBrushes.Size = new System.Drawing.Size(536, 302);
      this.tabBrushes.TabIndex = 6;
      this.tabBrushes.Text = "Brushes";
      // 
      // radioButton2
      // 
      this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton2.Location = new System.Drawing.Point(12, 56);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(104, 16);
      this.radioButton2.TabIndex = 23;
      this.radioButton2.Text = "Test03";
      this.radioButton2.Visible = false;
      this.radioButton2.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton4
      // 
      this.radioButton4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton4.Location = new System.Drawing.Point(12, 36);
      this.radioButton4.Name = "radioButton4";
      this.radioButton4.Size = new System.Drawing.Size(104, 16);
      this.radioButton4.TabIndex = 16;
      this.radioButton4.Tag = "Test02";
      this.radioButton4.Text = "Test02";
      this.radioButton4.Visible = false;
      this.radioButton4.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnBrushesLinearGradient
      // 
      this.btnBrushesLinearGradient.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnBrushesLinearGradient.Location = new System.Drawing.Point(12, 12);
      this.btnBrushesLinearGradient.Name = "btnBrushesLinearGradient";
      this.btnBrushesLinearGradient.Size = new System.Drawing.Size(104, 16);
      this.btnBrushesLinearGradient.TabIndex = 15;
      this.btnBrushesLinearGradient.Tag = "BrushesLinearGradient";
      this.btnBrushesLinearGradient.Text = "LinearGradient";
      this.btnBrushesLinearGradient.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton17
      // 
      this.radioButton17.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton17.Location = new System.Drawing.Point(12, 76);
      this.radioButton17.Name = "radioButton17";
      this.radioButton17.Size = new System.Drawing.Size(104, 16);
      this.radioButton17.TabIndex = 22;
      this.radioButton17.Text = "Test04";
      this.radioButton17.Visible = false;
      this.radioButton17.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton27
      // 
      this.radioButton27.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton27.Location = new System.Drawing.Point(12, 96);
      this.radioButton27.Name = "radioButton27";
      this.radioButton27.Size = new System.Drawing.Size(104, 16);
      this.radioButton27.TabIndex = 24;
      this.radioButton27.Tag = "PathFlatten";
      this.radioButton27.Text = "Flatten";
      this.radioButton27.Visible = false;
      this.radioButton27.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton29
      // 
      this.radioButton29.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton29.Location = new System.Drawing.Point(12, 120);
      this.radioButton29.Name = "radioButton29";
      this.radioButton29.Size = new System.Drawing.Size(104, 16);
      this.radioButton29.TabIndex = 26;
      this.radioButton29.Tag = "PathWiden";
      this.radioButton29.Text = "Widen";
      this.radioButton29.Visible = false;
      this.radioButton29.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton30
      // 
      this.radioButton30.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton30.Location = new System.Drawing.Point(12, 140);
      this.radioButton30.Name = "radioButton30";
      this.radioButton30.Size = new System.Drawing.Size(104, 16);
      this.radioButton30.TabIndex = 25;
      this.radioButton30.Tag = "PathGlyph";
      this.radioButton30.Text = "Glyph";
      this.radioButton30.Visible = false;
      this.radioButton30.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton31
      // 
      this.radioButton31.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton31.Location = new System.Drawing.Point(12, 160);
      this.radioButton31.Name = "radioButton31";
      this.radioButton31.Size = new System.Drawing.Size(104, 16);
      this.radioButton31.TabIndex = 18;
      this.radioButton31.Tag = "PathClipGlyph";
      this.radioButton31.Text = "Clip Glyphs";
      this.radioButton31.Visible = false;
      this.radioButton31.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton32
      // 
      this.radioButton32.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton32.Location = new System.Drawing.Point(12, 180);
      this.radioButton32.Name = "radioButton32";
      this.radioButton32.Size = new System.Drawing.Size(104, 16);
      this.radioButton32.TabIndex = 17;
      this.radioButton32.Text = "Test09";
      this.radioButton32.Visible = false;
      this.radioButton32.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton39
      // 
      this.radioButton39.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton39.Location = new System.Drawing.Point(12, 204);
      this.radioButton39.Name = "radioButton39";
      this.radioButton39.Size = new System.Drawing.Size(104, 16);
      this.radioButton39.TabIndex = 19;
      this.radioButton39.Text = "Test10";
      this.radioButton39.Visible = false;
      this.radioButton39.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton49
      // 
      this.radioButton49.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton49.Location = new System.Drawing.Point(12, 224);
      this.radioButton49.Name = "radioButton49";
      this.radioButton49.Size = new System.Drawing.Size(104, 16);
      this.radioButton49.TabIndex = 21;
      this.radioButton49.Text = "Test11";
      this.radioButton49.Visible = false;
      this.radioButton49.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton52
      // 
      this.radioButton52.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton52.Location = new System.Drawing.Point(12, 244);
      this.radioButton52.Name = "radioButton52";
      this.radioButton52.Size = new System.Drawing.Size(104, 16);
      this.radioButton52.TabIndex = 20;
      this.radioButton52.Tag = "PathClipTest";
      this.radioButton52.Text = "Clip Test";
      this.radioButton52.Visible = false;
      this.radioButton52.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabImages
      // 
      this.tabImages.Controls.Add(this.btnImagesGif);
      this.tabImages.Controls.Add(this.btnImagesPng);
      this.tabImages.Controls.Add(this.btnImagesJpeg);
      this.tabImages.Controls.Add(this.btnImagesTiff);
      this.tabImages.Controls.Add(this.btnImagesBmp);
      this.tabImages.Controls.Add(this.btnImagesBmpOS2);
      this.tabImages.Controls.Add(this.btnImagesFormXObject);
      this.tabImages.Controls.Add(this.btnImagesFormXObjectTemplate);
      this.tabImages.Controls.Add(this.radioButton57);
      this.tabImages.Controls.Add(this.radioButton58);
      this.tabImages.Controls.Add(this.radioButton59);
      this.tabImages.Controls.Add(this.radioButton60);
      this.tabImages.Location = new System.Drawing.Point(4, 22);
      this.tabImages.Name = "tabImages";
      this.tabImages.Size = new System.Drawing.Size(536, 302);
      this.tabImages.TabIndex = 4;
      this.tabImages.Text = "Images";
      this.tabImages.Click += new System.EventHandler(this.tpgImages_Click);
      // 
      // btnImagesGif
      // 
      this.btnImagesGif.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesGif.Location = new System.Drawing.Point(8, 56);
      this.btnImagesGif.Name = "btnImagesGif";
      this.btnImagesGif.Size = new System.Drawing.Size(104, 16);
      this.btnImagesGif.TabIndex = 11;
      this.btnImagesGif.Tag = "ImagesGif";
      this.btnImagesGif.Text = "GIF";
      this.btnImagesGif.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnImagesPng
      // 
      this.btnImagesPng.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesPng.Location = new System.Drawing.Point(8, 36);
      this.btnImagesPng.Name = "btnImagesPng";
      this.btnImagesPng.Size = new System.Drawing.Size(104, 16);
      this.btnImagesPng.TabIndex = 4;
      this.btnImagesPng.Tag = "ImagesPng";
      this.btnImagesPng.Text = "PNG";
      this.btnImagesPng.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnImagesJpeg
      // 
      this.btnImagesJpeg.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesJpeg.Location = new System.Drawing.Point(8, 16);
      this.btnImagesJpeg.Name = "btnImagesJpeg";
      this.btnImagesJpeg.Size = new System.Drawing.Size(104, 16);
      this.btnImagesJpeg.TabIndex = 3;
      this.btnImagesJpeg.Tag = "ImagesJpeg";
      this.btnImagesJpeg.Text = "JPEG";
      this.btnImagesJpeg.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnImagesTiff
      // 
      this.btnImagesTiff.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesTiff.Location = new System.Drawing.Point(8, 80);
      this.btnImagesTiff.Name = "btnImagesTiff";
      this.btnImagesTiff.Size = new System.Drawing.Size(104, 16);
      this.btnImagesTiff.TabIndex = 10;
      this.btnImagesTiff.Tag = "ImagesTiff";
      this.btnImagesTiff.Text = "TIFF";
      this.btnImagesTiff.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnImagesBmp
      // 
      this.btnImagesBmp.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesBmp.Location = new System.Drawing.Point(8, 100);
      this.btnImagesBmp.Name = "btnImagesBmp";
      this.btnImagesBmp.Size = new System.Drawing.Size(104, 16);
      this.btnImagesBmp.TabIndex = 12;
      this.btnImagesBmp.Tag = "ImagesBmp";
      this.btnImagesBmp.Text = "BMP (Windows)";
      this.btnImagesBmp.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnImagesBmpOS2
      // 
      this.btnImagesBmpOS2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesBmpOS2.Location = new System.Drawing.Point(8, 120);
      this.btnImagesBmpOS2.Name = "btnImagesBmpOS2";
      this.btnImagesBmpOS2.Size = new System.Drawing.Size(104, 16);
      this.btnImagesBmpOS2.TabIndex = 14;
      this.btnImagesBmpOS2.Tag = "ImagesBmpOS2";
      this.btnImagesBmpOS2.Text = "BMP (OS/2)";
      this.btnImagesBmpOS2.Click += new System.EventHandler(this.btnTest_Click);
      this.btnImagesBmpOS2.CheckedChanged += new System.EventHandler(this.radioButton54_CheckedChanged);
      // 
      // btnImagesFormXObject
      // 
      this.btnImagesFormXObject.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesFormXObject.Location = new System.Drawing.Point(8, 140);
      this.btnImagesFormXObject.Name = "btnImagesFormXObject";
      this.btnImagesFormXObject.Size = new System.Drawing.Size(192, 16);
      this.btnImagesFormXObject.TabIndex = 13;
      this.btnImagesFormXObject.Tag = "ImagesFormXObject";
      this.btnImagesFormXObject.Text = "FormXObject (only visible in PDF)";
      this.btnImagesFormXObject.Click += new System.EventHandler(this.btnTest_Click);
      this.btnImagesFormXObject.CheckedChanged += new System.EventHandler(this.radioButton55_CheckedChanged);
      // 
      // btnImagesFormXObjectTemplate
      // 
      this.btnImagesFormXObjectTemplate.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnImagesFormXObjectTemplate.Location = new System.Drawing.Point(8, 164);
      this.btnImagesFormXObjectTemplate.Name = "btnImagesFormXObjectTemplate";
      this.btnImagesFormXObjectTemplate.Size = new System.Drawing.Size(192, 16);
      this.btnImagesFormXObjectTemplate.TabIndex = 6;
      this.btnImagesFormXObjectTemplate.Tag = "ImagesFormXObjectTemplate";
      this.btnImagesFormXObjectTemplate.Text = "FormXObjectTemplate";
      this.btnImagesFormXObjectTemplate.Click += new System.EventHandler(this.btnTest_Click);
      this.btnImagesFormXObjectTemplate.CheckedChanged += new System.EventHandler(this.radioButton56_CheckedChanged);
      // 
      // radioButton57
      // 
      this.radioButton57.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton57.Location = new System.Drawing.Point(8, 184);
      this.radioButton57.Name = "radioButton57";
      this.radioButton57.Size = new System.Drawing.Size(104, 16);
      this.radioButton57.TabIndex = 5;
      this.radioButton57.Text = "Test09";
      this.radioButton57.Visible = false;
      this.radioButton57.Click += new System.EventHandler(this.btnTest_Click);
      this.radioButton57.CheckedChanged += new System.EventHandler(this.radioButton57_CheckedChanged);
      // 
      // radioButton58
      // 
      this.radioButton58.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton58.Location = new System.Drawing.Point(8, 204);
      this.radioButton58.Name = "radioButton58";
      this.radioButton58.Size = new System.Drawing.Size(104, 16);
      this.radioButton58.TabIndex = 7;
      this.radioButton58.Text = "Test10";
      this.radioButton58.Visible = false;
      this.radioButton58.Click += new System.EventHandler(this.btnTest_Click);
      this.radioButton58.CheckedChanged += new System.EventHandler(this.radioButton58_CheckedChanged);
      // 
      // radioButton59
      // 
      this.radioButton59.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton59.Location = new System.Drawing.Point(8, 224);
      this.radioButton59.Name = "radioButton59";
      this.radioButton59.Size = new System.Drawing.Size(104, 16);
      this.radioButton59.TabIndex = 9;
      this.radioButton59.Text = "Test11";
      this.radioButton59.Visible = false;
      this.radioButton59.Click += new System.EventHandler(this.btnTest_Click);
      this.radioButton59.CheckedChanged += new System.EventHandler(this.radioButton59_CheckedChanged);
      // 
      // radioButton60
      // 
      this.radioButton60.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton60.Location = new System.Drawing.Point(8, 248);
      this.radioButton60.Name = "radioButton60";
      this.radioButton60.Size = new System.Drawing.Size(104, 16);
      this.radioButton60.TabIndex = 8;
      this.radioButton60.Text = "Test12";
      this.radioButton60.Visible = false;
      this.radioButton60.Click += new System.EventHandler(this.btnTest_Click);
      this.radioButton60.CheckedChanged += new System.EventHandler(this.radioButton60_CheckedChanged);
      // 
      // tabPaths
      // 
      this.tabPaths.Controls.Add(this.radioButton25);
      this.tabPaths.Controls.Add(this.radioButton26);
      this.tabPaths.Controls.Add(this.btnPathTest01);
      this.tabPaths.Controls.Add(this.radioButton28);
      this.tabPaths.Controls.Add(this.btnPathFlatten);
      this.tabPaths.Controls.Add(this.btnPathWiden);
      this.tabPaths.Controls.Add(this.btnPathGlyph);
      this.tabPaths.Controls.Add(this.btnPathClipGlyph);
      this.tabPaths.Controls.Add(this.radioButton33);
      this.tabPaths.Controls.Add(this.radioButton34);
      this.tabPaths.Controls.Add(this.radioButton35);
      this.tabPaths.Controls.Add(this.btnPathClipTest);
      this.tabPaths.Location = new System.Drawing.Point(4, 22);
      this.tabPaths.Name = "tabPaths";
      this.tabPaths.Size = new System.Drawing.Size(536, 302);
      this.tabPaths.TabIndex = 2;
      this.tabPaths.Text = "Paths";
      // 
      // radioButton25
      // 
      this.radioButton25.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton25.Location = new System.Drawing.Point(12, 56);
      this.radioButton25.Name = "radioButton25";
      this.radioButton25.Size = new System.Drawing.Size(104, 16);
      this.radioButton25.TabIndex = 11;
      this.radioButton25.Text = "Test03";
      this.radioButton25.Visible = false;
      this.radioButton25.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton26
      // 
      this.radioButton26.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton26.Location = new System.Drawing.Point(12, 36);
      this.radioButton26.Name = "radioButton26";
      this.radioButton26.Size = new System.Drawing.Size(104, 16);
      this.radioButton26.TabIndex = 4;
      this.radioButton26.Tag = "Test02";
      this.radioButton26.Text = "Test02";
      this.radioButton26.Visible = false;
      this.radioButton26.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathTest01
      // 
      this.btnPathTest01.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathTest01.Location = new System.Drawing.Point(12, 12);
      this.btnPathTest01.Name = "btnPathTest01";
      this.btnPathTest01.Size = new System.Drawing.Size(104, 16);
      this.btnPathTest01.TabIndex = 3;
      this.btnPathTest01.Tag = "PathTest01";
      this.btnPathTest01.Text = "Test01";
      this.btnPathTest01.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton28
      // 
      this.radioButton28.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton28.Location = new System.Drawing.Point(12, 76);
      this.radioButton28.Name = "radioButton28";
      this.radioButton28.Size = new System.Drawing.Size(104, 16);
      this.radioButton28.TabIndex = 10;
      this.radioButton28.Text = "Test04";
      this.radioButton28.Visible = false;
      this.radioButton28.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathFlatten
      // 
      this.btnPathFlatten.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathFlatten.Location = new System.Drawing.Point(12, 96);
      this.btnPathFlatten.Name = "btnPathFlatten";
      this.btnPathFlatten.Size = new System.Drawing.Size(104, 16);
      this.btnPathFlatten.TabIndex = 12;
      this.btnPathFlatten.Tag = "PathFlatten";
      this.btnPathFlatten.Text = "Flatten";
      this.btnPathFlatten.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathWiden
      // 
      this.btnPathWiden.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathWiden.Location = new System.Drawing.Point(12, 120);
      this.btnPathWiden.Name = "btnPathWiden";
      this.btnPathWiden.Size = new System.Drawing.Size(104, 16);
      this.btnPathWiden.TabIndex = 14;
      this.btnPathWiden.Tag = "PathWiden";
      this.btnPathWiden.Text = "Widen";
      this.btnPathWiden.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathGlyph
      // 
      this.btnPathGlyph.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathGlyph.Location = new System.Drawing.Point(12, 140);
      this.btnPathGlyph.Name = "btnPathGlyph";
      this.btnPathGlyph.Size = new System.Drawing.Size(104, 16);
      this.btnPathGlyph.TabIndex = 13;
      this.btnPathGlyph.Tag = "PathGlyph";
      this.btnPathGlyph.Text = "Glyph";
      this.btnPathGlyph.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathClipGlyph
      // 
      this.btnPathClipGlyph.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathClipGlyph.Location = new System.Drawing.Point(12, 160);
      this.btnPathClipGlyph.Name = "btnPathClipGlyph";
      this.btnPathClipGlyph.Size = new System.Drawing.Size(104, 16);
      this.btnPathClipGlyph.TabIndex = 6;
      this.btnPathClipGlyph.Tag = "PathClipGlyph";
      this.btnPathClipGlyph.Text = "Clip Glyphs";
      this.btnPathClipGlyph.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton33
      // 
      this.radioButton33.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton33.Location = new System.Drawing.Point(12, 180);
      this.radioButton33.Name = "radioButton33";
      this.radioButton33.Size = new System.Drawing.Size(104, 16);
      this.radioButton33.TabIndex = 5;
      this.radioButton33.Text = "Test09";
      this.radioButton33.Visible = false;
      this.radioButton33.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton34
      // 
      this.radioButton34.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton34.Location = new System.Drawing.Point(12, 204);
      this.radioButton34.Name = "radioButton34";
      this.radioButton34.Size = new System.Drawing.Size(104, 16);
      this.radioButton34.TabIndex = 7;
      this.radioButton34.Text = "Test10";
      this.radioButton34.Visible = false;
      this.radioButton34.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton35
      // 
      this.radioButton35.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton35.Location = new System.Drawing.Point(12, 224);
      this.radioButton35.Name = "radioButton35";
      this.radioButton35.Size = new System.Drawing.Size(104, 16);
      this.radioButton35.TabIndex = 9;
      this.radioButton35.Text = "Test11";
      this.radioButton35.Visible = false;
      this.radioButton35.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnPathClipTest
      // 
      this.btnPathClipTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnPathClipTest.Location = new System.Drawing.Point(12, 244);
      this.btnPathClipTest.Name = "btnPathClipTest";
      this.btnPathClipTest.Size = new System.Drawing.Size(104, 16);
      this.btnPathClipTest.TabIndex = 8;
      this.btnPathClipTest.Tag = "PathClipTest";
      this.btnPathClipTest.Text = "Clip Test";
      this.btnPathClipTest.Visible = false;
      this.btnPathClipTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabShapes
      // 
      this.tabShapes.Controls.Add(this.btnShapesMigraDocObject);
      this.tabShapes.Controls.Add(this.btnClipTest1);
      this.tabShapes.Controls.Add(this.btnShapesPolygon);
      this.tabShapes.Controls.Add(this.btnShapesEllipse);
      this.tabShapes.Controls.Add(this.btnShapesRectangle);
      this.tabShapes.Controls.Add(this.btnShapesPie);
      this.tabShapes.Controls.Add(this.btnShapesClosedCurve);
      this.tabShapes.Controls.Add(this.radioButton18);
      this.tabShapes.Controls.Add(this.btnShapesTransform);
      this.tabShapes.Controls.Add(this.radioButton20);
      this.tabShapes.Controls.Add(this.radioButton21);
      this.tabShapes.Controls.Add(this.radioButton22);
      this.tabShapes.Controls.Add(this.radioButton23);
      this.tabShapes.Controls.Add(this.radioButton24);
      this.tabShapes.Controls.Add(this.btnShapesRoundedRectangle);
      this.tabShapes.Controls.Add(this.btnBeginContainer);
      this.tabShapes.Controls.Add(this.btnShapesSave);
      this.tabShapes.Location = new System.Drawing.Point(4, 22);
      this.tabShapes.Name = "tabShapes";
      this.tabShapes.Size = new System.Drawing.Size(536, 302);
      this.tabShapes.TabIndex = 1;
      this.tabShapes.Text = "Shapes";
      // 
      // btnShapesMigraDocObject
      // 
      this.btnShapesMigraDocObject.Location = new System.Drawing.Point(152, 32);
      this.btnShapesMigraDocObject.Name = "btnShapesMigraDocObject";
      this.btnShapesMigraDocObject.Size = new System.Drawing.Size(105, 17);
      this.btnShapesMigraDocObject.TabIndex = 16;
      this.btnShapesMigraDocObject.TabStop = true;
      this.btnShapesMigraDocObject.Tag = "ShapesMigraDocObject";
      this.btnShapesMigraDocObject.Text = "MigraDoc-Object";
      this.btnShapesMigraDocObject.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnClipTest1
      // 
      this.btnClipTest1.Location = new System.Drawing.Point(152, 12);
      this.btnClipTest1.Name = "btnClipTest1";
      this.btnClipTest1.Size = new System.Drawing.Size(104, 16);
      this.btnClipTest1.TabIndex = 15;
      this.btnClipTest1.Tag = "ShapesClipTest1";
      this.btnClipTest1.Text = "ClipTest 1";
      this.btnClipTest1.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesPolygon
      // 
      this.btnShapesPolygon.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesPolygon.Location = new System.Drawing.Point(12, 72);
      this.btnShapesPolygon.Name = "btnShapesPolygon";
      this.btnShapesPolygon.Size = new System.Drawing.Size(104, 16);
      this.btnShapesPolygon.TabIndex = 11;
      this.btnShapesPolygon.Tag = "ShapesPolygon";
      this.btnShapesPolygon.Text = "Polygon";
      this.btnShapesPolygon.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesEllipse
      // 
      this.btnShapesEllipse.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesEllipse.Location = new System.Drawing.Point(12, 52);
      this.btnShapesEllipse.Name = "btnShapesEllipse";
      this.btnShapesEllipse.Size = new System.Drawing.Size(104, 16);
      this.btnShapesEllipse.TabIndex = 4;
      this.btnShapesEllipse.Tag = "ShapesEllipse";
      this.btnShapesEllipse.Text = "Ellipse";
      this.btnShapesEllipse.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesRectangle
      // 
      this.btnShapesRectangle.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesRectangle.Location = new System.Drawing.Point(12, 12);
      this.btnShapesRectangle.Name = "btnShapesRectangle";
      this.btnShapesRectangle.Size = new System.Drawing.Size(104, 16);
      this.btnShapesRectangle.TabIndex = 3;
      this.btnShapesRectangle.Tag = "ShapesRectangle";
      this.btnShapesRectangle.Text = "Rectangle";
      this.btnShapesRectangle.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesPie
      // 
      this.btnShapesPie.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesPie.Location = new System.Drawing.Point(12, 96);
      this.btnShapesPie.Name = "btnShapesPie";
      this.btnShapesPie.Size = new System.Drawing.Size(104, 16);
      this.btnShapesPie.TabIndex = 10;
      this.btnShapesPie.Tag = "ShapesPie";
      this.btnShapesPie.Text = "Pie";
      this.btnShapesPie.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesClosedCurve
      // 
      this.btnShapesClosedCurve.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesClosedCurve.Location = new System.Drawing.Point(12, 116);
      this.btnShapesClosedCurve.Name = "btnShapesClosedCurve";
      this.btnShapesClosedCurve.Size = new System.Drawing.Size(104, 16);
      this.btnShapesClosedCurve.TabIndex = 12;
      this.btnShapesClosedCurve.Tag = "ShapesClosedCurve";
      this.btnShapesClosedCurve.Text = "ClosedCurve";
      this.btnShapesClosedCurve.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton18
      // 
      this.radioButton18.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton18.Location = new System.Drawing.Point(12, 152);
      this.radioButton18.Name = "radioButton18";
      this.radioButton18.Size = new System.Drawing.Size(104, 16);
      this.radioButton18.TabIndex = 14;
      this.radioButton18.Text = "...";
      this.radioButton18.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesTransform
      // 
      this.btnShapesTransform.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesTransform.Location = new System.Drawing.Point(12, 172);
      this.btnShapesTransform.Name = "btnShapesTransform";
      this.btnShapesTransform.Size = new System.Drawing.Size(104, 16);
      this.btnShapesTransform.TabIndex = 13;
      this.btnShapesTransform.Tag = "ShapesTransform";
      this.btnShapesTransform.Text = "Transform";
      this.btnShapesTransform.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton20
      // 
      this.radioButton20.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton20.Location = new System.Drawing.Point(12, 196);
      this.radioButton20.Name = "radioButton20";
      this.radioButton20.Size = new System.Drawing.Size(104, 16);
      this.radioButton20.TabIndex = 6;
      this.radioButton20.Text = "Test08";
      this.radioButton20.Visible = false;
      this.radioButton20.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton21
      // 
      this.radioButton21.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton21.Location = new System.Drawing.Point(12, 216);
      this.radioButton21.Name = "radioButton21";
      this.radioButton21.Size = new System.Drawing.Size(104, 16);
      this.radioButton21.TabIndex = 5;
      this.radioButton21.Text = "Test09";
      this.radioButton21.Visible = false;
      this.radioButton21.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton22
      // 
      this.radioButton22.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton22.Location = new System.Drawing.Point(12, 236);
      this.radioButton22.Name = "radioButton22";
      this.radioButton22.Size = new System.Drawing.Size(104, 16);
      this.radioButton22.TabIndex = 7;
      this.radioButton22.Text = "Test10";
      this.radioButton22.Visible = false;
      this.radioButton22.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton23
      // 
      this.radioButton23.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton23.Location = new System.Drawing.Point(12, 256);
      this.radioButton23.Name = "radioButton23";
      this.radioButton23.Size = new System.Drawing.Size(104, 16);
      this.radioButton23.TabIndex = 9;
      this.radioButton23.Text = "Test11";
      this.radioButton23.Visible = false;
      this.radioButton23.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton24
      // 
      this.radioButton24.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton24.Location = new System.Drawing.Point(12, 280);
      this.radioButton24.Name = "radioButton24";
      this.radioButton24.Size = new System.Drawing.Size(104, 16);
      this.radioButton24.TabIndex = 8;
      this.radioButton24.Text = "Test12";
      this.radioButton24.Visible = false;
      this.radioButton24.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesRoundedRectangle
      // 
      this.btnShapesRoundedRectangle.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnShapesRoundedRectangle.Location = new System.Drawing.Point(12, 32);
      this.btnShapesRoundedRectangle.Name = "btnShapesRoundedRectangle";
      this.btnShapesRoundedRectangle.Size = new System.Drawing.Size(124, 16);
      this.btnShapesRoundedRectangle.TabIndex = 3;
      this.btnShapesRoundedRectangle.Tag = "ShapesRoundedRectangle";
      this.btnShapesRoundedRectangle.Text = "RoundedRectangle";
      this.btnShapesRoundedRectangle.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnBeginContainer
      // 
      this.btnBeginContainer.Location = new System.Drawing.Point(152, 112);
      this.btnBeginContainer.Name = "btnBeginContainer";
      this.btnBeginContainer.Size = new System.Drawing.Size(104, 16);
      this.btnBeginContainer.TabIndex = 15;
      this.btnBeginContainer.Tag = "ShapesBeginContainer";
      this.btnBeginContainer.Text = "BeginContainer";
      this.btnBeginContainer.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnShapesSave
      // 
      this.btnShapesSave.Location = new System.Drawing.Point(152, 88);
      this.btnShapesSave.Name = "btnShapesSave";
      this.btnShapesSave.Size = new System.Drawing.Size(104, 16);
      this.btnShapesSave.TabIndex = 15;
      this.btnShapesSave.Tag = "ShapesSave";
      this.btnShapesSave.Text = "Save";
      this.btnShapesSave.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabText
      // 
      this.tabText.Controls.Add(this.btnTextAlign);
      this.tabText.Controls.Add(this.radioButton38);
      this.tabText.Controls.Add(this.btnText01);
      this.tabText.Controls.Add(this.btnRealizeTest);
      this.tabText.Controls.Add(this.btnAcro8Bug);
      this.tabText.Controls.Add(this.radioButton42);
      this.tabText.Controls.Add(this.btnTextTransform1);
      this.tabText.Controls.Add(this.btnTextTransform2);
      this.tabText.Controls.Add(this.radioButton45);
      this.tabText.Controls.Add(this.radioButton46);
      this.tabText.Controls.Add(this.radioButton47);
      this.tabText.Controls.Add(this.radioButton48);
      this.tabText.Controls.Add(this.radioButton3);
      this.tabText.Controls.Add(this.btnTextCodePage);
      this.tabText.Controls.Add(this.radioButton5);
      this.tabText.Controls.Add(this.radioButton16);
      this.tabText.Controls.Add(this.radioButton36);
      this.tabText.Controls.Add(this.radioButton61);
      this.tabText.Controls.Add(this.radioButton62);
      this.tabText.Controls.Add(this.radioButton63);
      this.tabText.Controls.Add(this.radioButton64);
      this.tabText.Controls.Add(this.btnLayout1);
      this.tabText.Controls.Add(this.radioButton66);
      this.tabText.Controls.Add(this.btnUnicode);
      this.tabText.Location = new System.Drawing.Point(4, 22);
      this.tabText.Name = "tabText";
      this.tabText.Size = new System.Drawing.Size(536, 302);
      this.tabText.TabIndex = 3;
      this.tabText.Text = "Text";
      // 
      // btnTextAlign
      // 
      this.btnTextAlign.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTextAlign.Location = new System.Drawing.Point(12, 56);
      this.btnTextAlign.Name = "btnTextAlign";
      this.btnTextAlign.Size = new System.Drawing.Size(104, 16);
      this.btnTextAlign.TabIndex = 11;
      this.btnTextAlign.Tag = "TextAlign";
      this.btnTextAlign.Text = "Alignment";
      this.btnTextAlign.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton38
      // 
      this.radioButton38.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton38.Location = new System.Drawing.Point(12, 36);
      this.radioButton38.Name = "radioButton38";
      this.radioButton38.Size = new System.Drawing.Size(104, 16);
      this.radioButton38.TabIndex = 4;
      this.radioButton38.Tag = "TestText02";
      this.radioButton38.Text = "Text02";
      this.radioButton38.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnText01
      // 
      this.btnText01.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnText01.Location = new System.Drawing.Point(12, 16);
      this.btnText01.Name = "btnText01";
      this.btnText01.Size = new System.Drawing.Size(104, 16);
      this.btnText01.TabIndex = 3;
      this.btnText01.Tag = "TestText01";
      this.btnText01.Text = "Text01";
      this.btnText01.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnRealizeTest
      // 
      this.btnRealizeTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnRealizeTest.Location = new System.Drawing.Point(12, 76);
      this.btnRealizeTest.Name = "btnRealizeTest";
      this.btnRealizeTest.Size = new System.Drawing.Size(104, 16);
      this.btnRealizeTest.TabIndex = 10;
      this.btnRealizeTest.Tag = "RealizeTest";
      this.btnRealizeTest.Text = "Realize Test";
      this.btnRealizeTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnAcro8Bug
      // 
      this.btnAcro8Bug.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnAcro8Bug.Location = new System.Drawing.Point(12, 96);
      this.btnAcro8Bug.Name = "btnAcro8Bug";
      this.btnAcro8Bug.Size = new System.Drawing.Size(121, 16);
      this.btnAcro8Bug.TabIndex = 12;
      this.btnAcro8Bug.Tag = "Acro8Bug";
      this.btnAcro8Bug.Text = "Adobe Reader 8 Bug";
      this.btnAcro8Bug.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton42
      // 
      this.radioButton42.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton42.Location = new System.Drawing.Point(12, 116);
      this.radioButton42.Name = "radioButton42";
      this.radioButton42.Size = new System.Drawing.Size(104, 16);
      this.radioButton42.TabIndex = 14;
      this.radioButton42.Text = "Test06";
      this.radioButton42.Visible = false;
      this.radioButton42.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnTextTransform1
      // 
      this.btnTextTransform1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTextTransform1.Location = new System.Drawing.Point(12, 140);
      this.btnTextTransform1.Name = "btnTextTransform1";
      this.btnTextTransform1.Size = new System.Drawing.Size(104, 16);
      this.btnTextTransform1.TabIndex = 13;
      this.btnTextTransform1.Tag = "TextTransform1";
      this.btnTextTransform1.Text = "Transform 1";
      this.btnTextTransform1.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnTextTransform2
      // 
      this.btnTextTransform2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTextTransform2.Location = new System.Drawing.Point(12, 160);
      this.btnTextTransform2.Name = "btnTextTransform2";
      this.btnTextTransform2.Size = new System.Drawing.Size(104, 16);
      this.btnTextTransform2.TabIndex = 6;
      this.btnTextTransform2.Tag = "TextTransform2";
      this.btnTextTransform2.Text = "Transform 2";
      this.btnTextTransform2.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton45
      // 
      this.radioButton45.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton45.Location = new System.Drawing.Point(12, 180);
      this.radioButton45.Name = "radioButton45";
      this.radioButton45.Size = new System.Drawing.Size(104, 16);
      this.radioButton45.TabIndex = 5;
      this.radioButton45.Text = "Test09";
      this.radioButton45.Visible = false;
      this.radioButton45.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton46
      // 
      this.radioButton46.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton46.Location = new System.Drawing.Point(12, 204);
      this.radioButton46.Name = "radioButton46";
      this.radioButton46.Size = new System.Drawing.Size(104, 16);
      this.radioButton46.TabIndex = 7;
      this.radioButton46.Text = "Test10";
      this.radioButton46.Visible = false;
      this.radioButton46.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton47
      // 
      this.radioButton47.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton47.Location = new System.Drawing.Point(12, 224);
      this.radioButton47.Name = "radioButton47";
      this.radioButton47.Size = new System.Drawing.Size(104, 16);
      this.radioButton47.TabIndex = 9;
      this.radioButton47.Text = "Test11";
      this.radioButton47.Visible = false;
      this.radioButton47.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton48
      // 
      this.radioButton48.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton48.Location = new System.Drawing.Point(12, 244);
      this.radioButton48.Name = "radioButton48";
      this.radioButton48.Size = new System.Drawing.Size(104, 16);
      this.radioButton48.TabIndex = 8;
      this.radioButton48.Text = "Test12";
      this.radioButton48.Visible = false;
      this.radioButton48.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton3
      // 
      this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton3.Location = new System.Drawing.Point(160, 156);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(104, 16);
      this.radioButton3.TabIndex = 6;
      this.radioButton3.Tag = "TextTransform2";
      this.radioButton3.Text = "Transform 2";
      this.radioButton3.Visible = false;
      this.radioButton3.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnTextCodePage
      // 
      this.btnTextCodePage.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTextCodePage.Location = new System.Drawing.Point(160, 12);
      this.btnTextCodePage.Name = "btnTextCodePage";
      this.btnTextCodePage.Size = new System.Drawing.Size(104, 16);
      this.btnTextCodePage.TabIndex = 3;
      this.btnTextCodePage.Tag = "TextCodePage";
      this.btnTextCodePage.Text = "CodePage";
      this.btnTextCodePage.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton5
      // 
      this.radioButton5.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton5.Location = new System.Drawing.Point(160, 176);
      this.radioButton5.Name = "radioButton5";
      this.radioButton5.Size = new System.Drawing.Size(104, 16);
      this.radioButton5.TabIndex = 5;
      this.radioButton5.Text = "Test09";
      this.radioButton5.Visible = false;
      this.radioButton5.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton16
      // 
      this.radioButton16.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton16.Location = new System.Drawing.Point(160, 200);
      this.radioButton16.Name = "radioButton16";
      this.radioButton16.Size = new System.Drawing.Size(104, 16);
      this.radioButton16.TabIndex = 7;
      this.radioButton16.Text = "Test10";
      this.radioButton16.Visible = false;
      this.radioButton16.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton36
      // 
      this.radioButton36.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton36.Location = new System.Drawing.Point(160, 72);
      this.radioButton36.Name = "radioButton36";
      this.radioButton36.Size = new System.Drawing.Size(104, 16);
      this.radioButton36.TabIndex = 10;
      this.radioButton36.Tag = "Embedding";
      this.radioButton36.Text = "Embedding";
      this.radioButton36.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton61
      // 
      this.radioButton61.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton61.Location = new System.Drawing.Point(160, 220);
      this.radioButton61.Name = "radioButton61";
      this.radioButton61.Size = new System.Drawing.Size(104, 16);
      this.radioButton61.TabIndex = 9;
      this.radioButton61.Text = "Test11";
      this.radioButton61.Visible = false;
      this.radioButton61.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton62
      // 
      this.radioButton62.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton62.Location = new System.Drawing.Point(160, 92);
      this.radioButton62.Name = "radioButton62";
      this.radioButton62.Size = new System.Drawing.Size(104, 16);
      this.radioButton62.TabIndex = 12;
      this.radioButton62.Text = "Test05";
      this.radioButton62.Visible = false;
      this.radioButton62.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton63
      // 
      this.radioButton63.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton63.Location = new System.Drawing.Point(160, 240);
      this.radioButton63.Name = "radioButton63";
      this.radioButton63.Size = new System.Drawing.Size(104, 16);
      this.radioButton63.TabIndex = 8;
      this.radioButton63.Text = "Test12";
      this.radioButton63.Visible = false;
      this.radioButton63.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton64
      // 
      this.radioButton64.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton64.Location = new System.Drawing.Point(160, 112);
      this.radioButton64.Name = "radioButton64";
      this.radioButton64.Size = new System.Drawing.Size(104, 16);
      this.radioButton64.TabIndex = 14;
      this.radioButton64.Text = "Test06";
      this.radioButton64.Visible = false;
      this.radioButton64.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnLayout1
      // 
      this.btnLayout1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnLayout1.Location = new System.Drawing.Point(160, 52);
      this.btnLayout1.Name = "btnLayout1";
      this.btnLayout1.Size = new System.Drawing.Size(104, 16);
      this.btnLayout1.TabIndex = 11;
      this.btnLayout1.Tag = "Layout1";
      this.btnLayout1.Text = "Layout 1";
      this.btnLayout1.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton66
      // 
      this.radioButton66.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton66.Location = new System.Drawing.Point(160, 136);
      this.radioButton66.Name = "radioButton66";
      this.radioButton66.Size = new System.Drawing.Size(104, 16);
      this.radioButton66.TabIndex = 13;
      this.radioButton66.Tag = "TextTransform1";
      this.radioButton66.Text = "Transform 1";
      this.radioButton66.Visible = false;
      this.radioButton66.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnUnicode
      // 
      this.btnUnicode.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnUnicode.Location = new System.Drawing.Point(160, 32);
      this.btnUnicode.Name = "btnUnicode";
      this.btnUnicode.Size = new System.Drawing.Size(104, 16);
      this.btnUnicode.TabIndex = 4;
      this.btnUnicode.Tag = "Unicode";
      this.btnUnicode.Text = "Unicode";
      this.btnUnicode.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabBarcodes
      // 
      this.tabBarcodes.BackColor = System.Drawing.Color.Transparent;
      this.tabBarcodes.Controls.Add(this.radioButton6);
      this.tabBarcodes.Controls.Add(this.btnTypes);
      this.tabBarcodes.Controls.Add(this.btnOrientation);
      this.tabBarcodes.Controls.Add(this.radioButton55);
      this.tabBarcodes.Controls.Add(this.radioButton68);
      this.tabBarcodes.Controls.Add(this.radioButton69);
      this.tabBarcodes.Controls.Add(this.radioButton70);
      this.tabBarcodes.Controls.Add(this.radioButton71);
      this.tabBarcodes.Controls.Add(this.radioButton72);
      this.tabBarcodes.Controls.Add(this.radioButton73);
      this.tabBarcodes.Controls.Add(this.radioButton74);
      this.tabBarcodes.Controls.Add(this.radioButton75);
      this.tabBarcodes.Location = new System.Drawing.Point(4, 22);
      this.tabBarcodes.Name = "tabBarcodes";
      this.tabBarcodes.Size = new System.Drawing.Size(536, 302);
      this.tabBarcodes.TabIndex = 7;
      this.tabBarcodes.Text = "Bar Code";
      // 
      // radioButton6
      // 
      this.radioButton6.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton6.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton6.Location = new System.Drawing.Point(12, 58);
      this.radioButton6.Name = "radioButton6";
      this.radioButton6.Size = new System.Drawing.Size(104, 16);
      this.radioButton6.TabIndex = 35;
      this.radioButton6.Tag = "BarCodesDataMatrix";
      this.radioButton6.Text = "Data Matrix";
      this.radioButton6.UseVisualStyleBackColor = false;
      this.radioButton6.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnTypes
      // 
      this.btnTypes.BackColor = System.Drawing.SystemColors.Control;
      this.btnTypes.CausesValidation = false;
      this.btnTypes.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnTypes.Location = new System.Drawing.Point(12, 36);
      this.btnTypes.Name = "btnTypes";
      this.btnTypes.Size = new System.Drawing.Size(104, 16);
      this.btnTypes.TabIndex = 28;
      this.btnTypes.Tag = "BarCodesTypes";
      this.btnTypes.Text = "Types";
      this.btnTypes.UseVisualStyleBackColor = false;
      this.btnTypes.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnOrientation
      // 
      this.btnOrientation.BackColor = System.Drawing.SystemColors.Control;
      this.btnOrientation.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnOrientation.Location = new System.Drawing.Point(12, 14);
      this.btnOrientation.Name = "btnOrientation";
      this.btnOrientation.Size = new System.Drawing.Size(104, 16);
      this.btnOrientation.TabIndex = 27;
      this.btnOrientation.Tag = "BarCodesOrientation";
      this.btnOrientation.Text = "Orientation";
      this.btnOrientation.UseVisualStyleBackColor = false;
      this.btnOrientation.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton55
      // 
      this.radioButton55.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton55.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton55.Location = new System.Drawing.Point(12, 78);
      this.radioButton55.Name = "radioButton55";
      this.radioButton55.Size = new System.Drawing.Size(104, 16);
      this.radioButton55.TabIndex = 34;
      this.radioButton55.Text = "Test04";
      this.radioButton55.UseVisualStyleBackColor = false;
      this.radioButton55.Visible = false;
      this.radioButton55.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton68
      // 
      this.radioButton68.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton68.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton68.Location = new System.Drawing.Point(12, 98);
      this.radioButton68.Name = "radioButton68";
      this.radioButton68.Size = new System.Drawing.Size(104, 16);
      this.radioButton68.TabIndex = 36;
      this.radioButton68.Text = "Test05";
      this.radioButton68.UseVisualStyleBackColor = false;
      this.radioButton68.Visible = false;
      this.radioButton68.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton69
      // 
      this.radioButton69.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton69.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton69.Location = new System.Drawing.Point(12, 122);
      this.radioButton69.Name = "radioButton69";
      this.radioButton69.Size = new System.Drawing.Size(104, 16);
      this.radioButton69.TabIndex = 38;
      this.radioButton69.Text = "Test06";
      this.radioButton69.UseVisualStyleBackColor = false;
      this.radioButton69.Visible = false;
      this.radioButton69.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton70
      // 
      this.radioButton70.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton70.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton70.Location = new System.Drawing.Point(12, 142);
      this.radioButton70.Name = "radioButton70";
      this.radioButton70.Size = new System.Drawing.Size(104, 16);
      this.radioButton70.TabIndex = 37;
      this.radioButton70.Text = "Test07";
      this.radioButton70.UseVisualStyleBackColor = false;
      this.radioButton70.Visible = false;
      this.radioButton70.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton71
      // 
      this.radioButton71.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton71.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton71.Location = new System.Drawing.Point(12, 162);
      this.radioButton71.Name = "radioButton71";
      this.radioButton71.Size = new System.Drawing.Size(104, 16);
      this.radioButton71.TabIndex = 30;
      this.radioButton71.Text = "Test08";
      this.radioButton71.UseVisualStyleBackColor = false;
      this.radioButton71.Visible = false;
      this.radioButton71.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton72
      // 
      this.radioButton72.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton72.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton72.Location = new System.Drawing.Point(12, 182);
      this.radioButton72.Name = "radioButton72";
      this.radioButton72.Size = new System.Drawing.Size(104, 16);
      this.radioButton72.TabIndex = 29;
      this.radioButton72.Text = "Test09";
      this.radioButton72.UseVisualStyleBackColor = false;
      this.radioButton72.Visible = false;
      this.radioButton72.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton73
      // 
      this.radioButton73.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton73.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton73.Location = new System.Drawing.Point(12, 206);
      this.radioButton73.Name = "radioButton73";
      this.radioButton73.Size = new System.Drawing.Size(104, 16);
      this.radioButton73.TabIndex = 31;
      this.radioButton73.Text = "Test10";
      this.radioButton73.UseVisualStyleBackColor = false;
      this.radioButton73.Visible = false;
      this.radioButton73.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton74
      // 
      this.radioButton74.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton74.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton74.Location = new System.Drawing.Point(12, 226);
      this.radioButton74.Name = "radioButton74";
      this.radioButton74.Size = new System.Drawing.Size(104, 16);
      this.radioButton74.TabIndex = 33;
      this.radioButton74.Text = "Test11";
      this.radioButton74.UseVisualStyleBackColor = false;
      this.radioButton74.Visible = false;
      this.radioButton74.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton75
      // 
      this.radioButton75.BackColor = System.Drawing.SystemColors.Control;
      this.radioButton75.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton75.Location = new System.Drawing.Point(12, 246);
      this.radioButton75.Name = "radioButton75";
      this.radioButton75.Size = new System.Drawing.Size(104, 16);
      this.radioButton75.TabIndex = 32;
      this.radioButton75.Text = "Test12";
      this.radioButton75.UseVisualStyleBackColor = false;
      this.radioButton75.Visible = false;
      this.radioButton75.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tabMiscellaneous
      // 
      this.tabMiscellaneous.Controls.Add(this.radioButton1);
      this.tabMiscellaneous.Controls.Add(this.btnClock);
      this.tabMiscellaneous.Controls.Add(this.btnSpiroGraph);
      this.tabMiscellaneous.Controls.Add(this.radioButton13);
      this.tabMiscellaneous.Controls.Add(this.radioButton14);
      this.tabMiscellaneous.Controls.Add(this.radioButton15);
      this.tabMiscellaneous.Controls.Add(this.radioButton19);
      this.tabMiscellaneous.Controls.Add(this.radioButton37);
      this.tabMiscellaneous.Controls.Add(this.radioButton43);
      this.tabMiscellaneous.Controls.Add(this.radioButton44);
      this.tabMiscellaneous.Controls.Add(this.radioButton50);
      this.tabMiscellaneous.Controls.Add(this.radioButton51);
      this.tabMiscellaneous.Location = new System.Drawing.Point(4, 22);
      this.tabMiscellaneous.Name = "tabMiscellaneous";
      this.tabMiscellaneous.Size = new System.Drawing.Size(536, 302);
      this.tabMiscellaneous.TabIndex = 5;
      this.tabMiscellaneous.Text = "Miscellaneous";
      // 
      // radioButton1
      // 
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton1.Location = new System.Drawing.Point(12, 56);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(104, 16);
      this.radioButton1.TabIndex = 23;
      this.radioButton1.Text = "Test03";
      this.radioButton1.Visible = false;
      this.radioButton1.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // btnClock
      // 
      this.btnClock.CausesValidation = false;
      this.btnClock.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnClock.Location = new System.Drawing.Point(12, 36);
      this.btnClock.Name = "btnClock";
      this.btnClock.Size = new System.Drawing.Size(104, 16);
      this.btnClock.TabIndex = 16;
      this.btnClock.Tag = "MiscClock";
      this.btnClock.Text = "Clock";
      this.btnClock.Click += new System.EventHandler(this.btnTest_Click);
      this.btnClock.CheckedChanged += new System.EventHandler(this.btnClock_CheckedChanged);
      // 
      // btnSpiroGraph
      // 
      this.btnSpiroGraph.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.btnSpiroGraph.Location = new System.Drawing.Point(12, 12);
      this.btnSpiroGraph.Name = "btnSpiroGraph";
      this.btnSpiroGraph.Size = new System.Drawing.Size(104, 16);
      this.btnSpiroGraph.TabIndex = 15;
      this.btnSpiroGraph.Tag = "MiscSpiroGraph";
      this.btnSpiroGraph.Text = "SpiroGraph";
      this.btnSpiroGraph.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton13
      // 
      this.radioButton13.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton13.Location = new System.Drawing.Point(12, 76);
      this.radioButton13.Name = "radioButton13";
      this.radioButton13.Size = new System.Drawing.Size(104, 16);
      this.radioButton13.TabIndex = 22;
      this.radioButton13.Text = "Test04";
      this.radioButton13.Visible = false;
      this.radioButton13.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton14
      // 
      this.radioButton14.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton14.Location = new System.Drawing.Point(12, 96);
      this.radioButton14.Name = "radioButton14";
      this.radioButton14.Size = new System.Drawing.Size(104, 16);
      this.radioButton14.TabIndex = 24;
      this.radioButton14.Text = "Test05";
      this.radioButton14.Visible = false;
      this.radioButton14.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton15
      // 
      this.radioButton15.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton15.Location = new System.Drawing.Point(12, 120);
      this.radioButton15.Name = "radioButton15";
      this.radioButton15.Size = new System.Drawing.Size(104, 16);
      this.radioButton15.TabIndex = 26;
      this.radioButton15.Text = "Test06";
      this.radioButton15.Visible = false;
      this.radioButton15.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton19
      // 
      this.radioButton19.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton19.Location = new System.Drawing.Point(12, 140);
      this.radioButton19.Name = "radioButton19";
      this.radioButton19.Size = new System.Drawing.Size(104, 16);
      this.radioButton19.TabIndex = 25;
      this.radioButton19.Text = "Test07";
      this.radioButton19.Visible = false;
      this.radioButton19.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton37
      // 
      this.radioButton37.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton37.Location = new System.Drawing.Point(12, 160);
      this.radioButton37.Name = "radioButton37";
      this.radioButton37.Size = new System.Drawing.Size(104, 16);
      this.radioButton37.TabIndex = 18;
      this.radioButton37.Text = "Test08";
      this.radioButton37.Visible = false;
      this.radioButton37.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton43
      // 
      this.radioButton43.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton43.Location = new System.Drawing.Point(12, 180);
      this.radioButton43.Name = "radioButton43";
      this.radioButton43.Size = new System.Drawing.Size(104, 16);
      this.radioButton43.TabIndex = 17;
      this.radioButton43.Text = "Test09";
      this.radioButton43.Visible = false;
      this.radioButton43.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton44
      // 
      this.radioButton44.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton44.Location = new System.Drawing.Point(12, 204);
      this.radioButton44.Name = "radioButton44";
      this.radioButton44.Size = new System.Drawing.Size(104, 16);
      this.radioButton44.TabIndex = 19;
      this.radioButton44.Text = "Test10";
      this.radioButton44.Visible = false;
      this.radioButton44.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton50
      // 
      this.radioButton50.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton50.Location = new System.Drawing.Point(12, 224);
      this.radioButton50.Name = "radioButton50";
      this.radioButton50.Size = new System.Drawing.Size(104, 16);
      this.radioButton50.TabIndex = 21;
      this.radioButton50.Text = "Test11";
      this.radioButton50.Visible = false;
      this.radioButton50.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // radioButton51
      // 
      this.radioButton51.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton51.Location = new System.Drawing.Point(12, 244);
      this.radioButton51.Name = "radioButton51";
      this.radioButton51.Size = new System.Drawing.Size(104, 16);
      this.radioButton51.TabIndex = 20;
      this.radioButton51.Text = "Test12";
      this.radioButton51.Visible = false;
      this.radioButton51.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(572, 406);
      this.Controls.Add(this.tpcMain);
      this.Controls.Add(this.statusBar);
      this.Controls.Add(this.toolBar);
      this.Location = new System.Drawing.Point(5, 400);
      this.Menu = this.menuBar;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "XGraphics Lab";
      this.tpcMain.ResumeLayout(false);
      this.tabLines.ResumeLayout(false);
      this.tabBrushes.ResumeLayout(false);
      this.tabImages.ResumeLayout(false);
      this.tabPaths.ResumeLayout(false);
      this.tabShapes.ResumeLayout(false);
      this.tabText.ResumeLayout(false);
      this.tabBarcodes.ResumeLayout(false);
      this.tabMiscellaneous.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      if (this.preview == null)
      {
        this.preview = new PreviewForm();
        this.preview.Owner = this;
        this.preview.Show();
      }

      PropertiesForm propertiesForm = new PropertiesForm();
      propertiesForm.Owner = this;
      propertiesForm.Show();
    }

    private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
    {
      object tag = e.Button.Tag;
      if (tag != null)
      {
        switch (tag.ToString())
        {
          case "3":
            ShowPreview(!this.preview.Visible);
            break;
        }
      }
    }

    internal PreviewForm preview;

    private void btnTest_Click(object sender, System.EventArgs e)
    {
      object tag = ((Control)sender).Tag;
      if (tag != null)
      {
        string name = Path.GetFileNameWithoutExtension(typeof(TesterBase).FullName) + "." + tag.ToString();
        Type type = Type.GetType(name);
        if (type != null)
        {
          this.tester = (TesterBase)type.GetConstructor(Type.EmptyTypes).Invoke(null);
          this.preview.SetRenderEvent(new PagePreview.RenderEvent(this.tester.RenderPage));
          this.statusBar.Text = this.tester.Description;
        }
      }
    }

    private void btnPdf_Click(object sender, System.EventArgs e)
    {
      string filename = Guid.NewGuid().ToString().ToUpper() + ".pdf";
      //PdfSharp.Drawing.XGraphic gfx = new PdfSharp.Drawing.XGraphic();
      PdfDocument document = new PdfDocument(filename);
      PdfPage page = document.AddPage();
      double width = page.Width;
      double height = page.Height;
      page.Height = width;
      page.Width = height;
      XGraphics gfx = XGraphics.FromPdfPage(page);
      this.tester.RenderPage(gfx);
      //document.WriteToFile(filename);
      document.Close();
      Process.Start(filename);
    }

    private void radioButton57_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton58_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton59_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton60_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void tpgImages_Click(object sender, System.EventArgs e)
    {

    }

    private void tpgMain_SelectedIndexChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton54_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton55_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void radioButton56_CheckedChanged(object sender, System.EventArgs e)
    {

    }

    private void btnClock_CheckedChanged(object sender, System.EventArgs e)
    {

    }
  }
}
