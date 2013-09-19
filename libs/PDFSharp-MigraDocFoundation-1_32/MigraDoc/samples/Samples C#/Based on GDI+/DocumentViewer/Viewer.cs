#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Forms;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.Rendering;
using MigraDoc.Rendering.Printing;
using MigraDoc.Rendering.Forms;
using MigraDoc.RtfRendering;

namespace DocumentViewer
{
  /// <summary>
  /// Demonstrates all techniques you need to preview and print a MigraDoc document, and convert it to a
  /// PDF, RTF, or image file.
  /// </summary>
  public class Viewer : System.Windows.Forms.Form
  {
    private System.Windows.Forms.MainMenu mainMenu;
    private System.Windows.Forms.MenuItem menuItem18;
    private System.Windows.Forms.MenuItem menuItem22;
    private System.Windows.Forms.MenuItem menuItem24;
    private System.Windows.Forms.MenuItem miFile;
    private System.Windows.Forms.MenuItem miZoom;
    private System.Windows.Forms.MenuItem miZoom800;
    private System.Windows.Forms.MenuItem miOpen;
    private System.Windows.Forms.MenuItem miPrint;
    private System.Windows.Forms.MenuItem miExit;
    private System.Windows.Forms.MenuItem miPrev;
    private System.Windows.Forms.MenuItem miNext;
    private System.Windows.Forms.MenuItem miPdf;
    private System.Windows.Forms.MenuItem miRtf;
    private System.Windows.Forms.MenuItem miZoom600;
    private System.Windows.Forms.MenuItem miZoom400;
    private System.Windows.Forms.MenuItem miZoom200;
    private System.Windows.Forms.MenuItem miZoom150;
    private System.Windows.Forms.MenuItem miZoom100;
    private System.Windows.Forms.MenuItem miZoom50;
    private System.Windows.Forms.MenuItem miZoom25;
    private System.Windows.Forms.MenuItem miZoom10;
    private System.Windows.Forms.MenuItem miBestFit;
    private System.Windows.Forms.MenuItem miFullPage;
    private System.Windows.Forms.MenuItem miBmp;
    private System.Windows.Forms.StatusBar statusBar;
    private System.Windows.Forms.MenuItem miFirst;
    private System.Windows.Forms.MenuItem miLast;
    private System.Windows.Forms.MenuItem menuItem1;
    private System.Windows.Forms.MenuItem menuItem2;
    private MigraDoc.Rendering.Forms.DocumentPreview pagePreview;
    private System.Windows.Forms.MenuItem miPreview;
    private System.Windows.Forms.MenuItem miPrinterSetup;
    private System.Windows.Forms.MenuItem miDemonstrate;
    private MenuItem miSample1;
    private MenuItem miSample2;
    private MenuItem miMetaFile;
    private MenuItem miOriginalSize;
    private IContainer components;

    public Viewer()
    {
      InitializeComponent();

      // Create a new MigraDoc document
      Document document = SampleDocuments.CreateSample1();

      // HACK
      string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
      this.pagePreview.Ddl = ddl;

      UpdateStatusBar();
    }
    PrinterSettings printerSettings = new PrinterSettings();

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

    void UpdateStatusBar()
    {
      string info = String.Format("Page {0} of {1} - Zoom: {2}%", this.pagePreview.Page, this.pagePreview.PageCount,
        this.pagePreview.ZoomPercent);
      this.statusBar.Text = info;
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
      this.miFile = new System.Windows.Forms.MenuItem();
      this.miSample1 = new System.Windows.Forms.MenuItem();
      this.miSample2 = new System.Windows.Forms.MenuItem();
      this.miOpen = new System.Windows.Forms.MenuItem();
      this.menuItem22 = new System.Windows.Forms.MenuItem();
      this.miPrinterSetup = new System.Windows.Forms.MenuItem();
      this.miPreview = new System.Windows.Forms.MenuItem();
      this.miPrint = new System.Windows.Forms.MenuItem();
      this.menuItem24 = new System.Windows.Forms.MenuItem();
      this.miExit = new System.Windows.Forms.MenuItem();
      this.miZoom = new System.Windows.Forms.MenuItem();
      this.miZoom800 = new System.Windows.Forms.MenuItem();
      this.miZoom600 = new System.Windows.Forms.MenuItem();
      this.miZoom400 = new System.Windows.Forms.MenuItem();
      this.miZoom200 = new System.Windows.Forms.MenuItem();
      this.miZoom150 = new System.Windows.Forms.MenuItem();
      this.miZoom100 = new System.Windows.Forms.MenuItem();
      this.miZoom50 = new System.Windows.Forms.MenuItem();
      this.miZoom25 = new System.Windows.Forms.MenuItem();
      this.miZoom10 = new System.Windows.Forms.MenuItem();
      this.menuItem18 = new System.Windows.Forms.MenuItem();
      this.miBestFit = new System.Windows.Forms.MenuItem();
      this.miFullPage = new System.Windows.Forms.MenuItem();
      this.miOriginalSize = new System.Windows.Forms.MenuItem();
      this.miDemonstrate = new System.Windows.Forms.MenuItem();
      this.miPdf = new System.Windows.Forms.MenuItem();
      this.miRtf = new System.Windows.Forms.MenuItem();
      this.miBmp = new System.Windows.Forms.MenuItem();
      this.miMetaFile = new System.Windows.Forms.MenuItem();
      this.miFirst = new System.Windows.Forms.MenuItem();
      this.miPrev = new System.Windows.Forms.MenuItem();
      this.miNext = new System.Windows.Forms.MenuItem();
      this.miLast = new System.Windows.Forms.MenuItem();
      this.menuItem1 = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.statusBar = new System.Windows.Forms.StatusBar();
      this.pagePreview = new MigraDoc.Rendering.Forms.DocumentPreview();
      this.SuspendLayout();
      // 
      // mainMenu
      // 
      this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miFile,
            this.miZoom,
            this.miDemonstrate,
            this.miFirst,
            this.miPrev,
            this.miNext,
            this.miLast});
      // 
      // miFile
      // 
      this.miFile.Index = 0;
      this.miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miSample1,
            this.miSample2,
            this.miOpen,
            this.menuItem22,
            this.miPrinterSetup,
            this.miPreview,
            this.miPrint,
            this.menuItem24,
            this.miExit});
      this.miFile.Text = "&File";
      // 
      // miSample1
      // 
      this.miSample1.Index = 0;
      this.miSample1.Text = "Create Sample 1";
      this.miSample1.Click += new System.EventHandler(this.miSample1_Click);
      // 
      // miSample2
      // 
      this.miSample2.Index = 1;
      this.miSample2.Text = "Create Sample 2";
      this.miSample2.Click += new System.EventHandler(this.miSample2_Click);
      // 
      // miOpen
      // 
      this.miOpen.Index = 2;
      this.miOpen.ShowShortcut = false;
      this.miOpen.Text = "&Open DDL File";
      this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
      // 
      // menuItem22
      // 
      this.menuItem22.Index = 3;
      this.menuItem22.Text = "-";
      // 
      // miPrinterSetup
      // 
      this.miPrinterSetup.Index = 4;
      this.miPrinterSetup.Text = "Printer Setup";
      this.miPrinterSetup.Click += new System.EventHandler(this.miPrinterSetup_Click);
      // 
      // miPreview
      // 
      this.miPreview.Index = 5;
      this.miPreview.Text = "Preview";
      this.miPreview.Click += new System.EventHandler(this.miPreview_Click);
      // 
      // miPrint
      // 
      this.miPrint.Index = 6;
      this.miPrint.Text = "&Print";
      this.miPrint.Click += new System.EventHandler(this.miPrint_Click);
      // 
      // menuItem24
      // 
      this.menuItem24.Index = 7;
      this.menuItem24.Text = "-";
      // 
      // miExit
      // 
      this.miExit.Index = 8;
      this.miExit.Text = "&Exit";
      this.miExit.Click += new System.EventHandler(this.miExit_Click);
      // 
      // miZoom
      // 
      this.miZoom.Index = 1;
      this.miZoom.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miZoom800,
            this.miZoom600,
            this.miZoom400,
            this.miZoom200,
            this.miZoom150,
            this.miZoom100,
            this.miZoom50,
            this.miZoom25,
            this.miZoom10,
            this.menuItem18,
            this.miBestFit,
            this.miFullPage,
            this.miOriginalSize});
      this.miZoom.Text = "&Zoom";
      // 
      // miZoom800
      // 
      this.miZoom800.Index = 0;
      this.miZoom800.Text = "800%";
      this.miZoom800.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom600
      // 
      this.miZoom600.Index = 1;
      this.miZoom600.Text = "600%";
      this.miZoom600.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom400
      // 
      this.miZoom400.Index = 2;
      this.miZoom400.Text = "400%";
      this.miZoom400.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom200
      // 
      this.miZoom200.Index = 3;
      this.miZoom200.Text = "200%";
      this.miZoom200.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom150
      // 
      this.miZoom150.Index = 4;
      this.miZoom150.Text = "150%";
      this.miZoom150.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom100
      // 
      this.miZoom100.Index = 5;
      this.miZoom100.Text = "100%";
      this.miZoom100.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom50
      // 
      this.miZoom50.Index = 6;
      this.miZoom50.Text = "50%";
      this.miZoom50.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom25
      // 
      this.miZoom25.Index = 7;
      this.miZoom25.Text = "25%";
      this.miZoom25.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // miZoom10
      // 
      this.miZoom10.Index = 8;
      this.miZoom10.Text = "10%";
      this.miZoom10.Click += new System.EventHandler(this.miZoom_Click);
      // 
      // menuItem18
      // 
      this.menuItem18.Index = 9;
      this.menuItem18.Text = "-";
      // 
      // miBestFit
      // 
      this.miBestFit.Index = 10;
      this.miBestFit.Text = "Best Fit";
      this.miBestFit.Click += new System.EventHandler(this.miBestFit_Click);
      // 
      // miFullPage
      // 
      this.miFullPage.Index = 11;
      this.miFullPage.Text = "Full Page";
      this.miFullPage.Click += new System.EventHandler(this.miFullPage_Click);
      // 
      // miOriginalSize
      // 
      this.miOriginalSize.Index = 12;
      this.miOriginalSize.Text = "OriginalSize";
      this.miOriginalSize.Click += new System.EventHandler(this.miOriginalSize_Click);
      // 
      // miDemonstrate
      // 
      this.miDemonstrate.Index = 2;
      this.miDemonstrate.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miPdf,
            this.miRtf,
            this.miBmp,
            this.miMetaFile});
      this.miDemonstrate.Text = "&Demonstrate";
      // 
      // miPdf
      // 
      this.miPdf.Index = 0;
      this.miPdf.Text = "Create PDF File";
      this.miPdf.Click += new System.EventHandler(this.miPdf_Click);
      // 
      // miRtf
      // 
      this.miRtf.Index = 1;
      this.miRtf.Text = "Create Word/RTF File";
      this.miRtf.Click += new System.EventHandler(this.miRtf_Click);
      // 
      // miBmp
      // 
      this.miBmp.Index = 2;
      this.miBmp.Text = "Create Image File";
      this.miBmp.Click += new System.EventHandler(this.miBmp_Click);
      // 
      // miMetaFile
      // 
      this.miMetaFile.Index = 3;
      this.miMetaFile.Text = "Create Meta File";
      this.miMetaFile.Click += new System.EventHandler(this.miMetaFile_Click);
      // 
      // miFirst
      // 
      this.miFirst.Index = 3;
      this.miFirst.Text = "<<";
      this.miFirst.Click += new System.EventHandler(this.miFirst_Click);
      // 
      // miPrev
      // 
      this.miPrev.Index = 4;
      this.miPrev.Text = "<";
      this.miPrev.Click += new System.EventHandler(this.miPrev_Click);
      // 
      // miNext
      // 
      this.miNext.Index = 5;
      this.miNext.Text = ">";
      this.miNext.Click += new System.EventHandler(this.miNext_Click);
      // 
      // miLast
      // 
      this.miLast.Index = 6;
      this.miLast.Text = ">>";
      this.miLast.Click += new System.EventHandler(this.miLast_Click);
      // 
      // menuItem1
      // 
      this.menuItem1.Index = -1;
      this.menuItem1.Text = "";
      // 
      // menuItem2
      // 
      this.menuItem2.Index = -1;
      this.menuItem2.Text = ",,";
      // 
      // statusBar
      // 
      this.statusBar.Location = new System.Drawing.Point(0, 491);
      this.statusBar.Name = "statusBar";
      this.statusBar.Size = new System.Drawing.Size(760, 22);
      this.statusBar.TabIndex = 2;
      // 
      // pagePreview
      // 
      this.pagePreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pagePreview.Ddl = null;
      this.pagePreview.DesktopColor = System.Drawing.SystemColors.ControlDark;
      this.pagePreview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pagePreview.Document = null;
      this.pagePreview.Location = new System.Drawing.Point(0, 0);
      this.pagePreview.Name = "pagePreview";
      this.pagePreview.Page = 0;
      this.pagePreview.PageColor = System.Drawing.Color.Snow;
      this.pagePreview.PageSize = new System.Drawing.Size(595, 842);
      this.pagePreview.PrivateFonts = null;
      this.pagePreview.Size = new System.Drawing.Size(760, 491);
      this.pagePreview.TabIndex = 3;
      this.pagePreview.Zoom = MigraDoc.Rendering.Forms.Zoom.FullPage;
      this.pagePreview.ZoomPercent = 41;
      this.pagePreview.PageChanged += new MigraDoc.Rendering.Forms.PagePreviewEventHandler(this.pagePreview_Changed);
      this.pagePreview.ZoomChanged += new MigraDoc.Rendering.Forms.PagePreviewEventHandler(this.pagePreview_Changed);
      // 
      // Viewer
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(760, 513);
      this.Controls.Add(this.pagePreview);
      this.Controls.Add(this.statusBar);
      this.Menu = this.mainMenu;
      this.Name = "Viewer";
      this.Text = "MigraDoc Document Viewer (for demonstration only)";
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// Opens and shows a MigraDoc DDL file.
    /// 
    /// A MigraDoc DDL file is a text based serialization of a MigraDoc document.
    /// </summary>
    private void miOpen_Click(object sender, System.EventArgs e)
    {
      OpenFileDialog dialog = null;
      try
      {
        dialog = new OpenFileDialog();
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        dialog.Filter = "MigraDoc DDL (*.mdddl)|*.mdddl|All Files (*.*)|*.*";
        dialog.FilterIndex = 1;
        dialog.InitialDirectory = Path.Combine(GetProgramDirectory(), "..\\..");
        //dialog.RestoreDirectory = true;
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          Document document = MigraDoc.DocumentObjectModel.IO.DdlReader.DocumentFromFile(dialog.FileName);
          string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
          this.pagePreview.Ddl = ddl;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, this.Text);
        this.pagePreview.Ddl = null; // TODO has no effect
      }
      finally
      {
        if (dialog != null)
          dialog.Dispose();
      }
      UpdateStatusBar();
    }

    /// <summary>
    /// Prints the current document on a printer.
    /// </summary>
    private void miPrint_Click(object sender, System.EventArgs e)
    {
      // Reuse the renderer from the preview
      DocumentRenderer renderer = this.pagePreview.Renderer;
      if (renderer != null)
      {
        int pageCount = renderer.FormattedDocument.PageCount;

        // Creates a PrintDocument that simplyfies printing of MigraDoc documents
        MigraDocPrintDocument printDocument = new MigraDocPrintDocument();

        // Attach the current printer settings
        printDocument.PrinterSettings = this.printerSettings;

        if (this.printerSettings.PrintRange == PrintRange.Selection)
          printDocument.SelectedPage = this.pagePreview.Page;

        // Attach the current document renderer
        printDocument.Renderer = renderer;

        // Print the document
        printDocument.Print();
      }
    }

    /// <summary>
    /// Sets the zoom factor.
    /// </summary>
    private void miZoom_Click(object sender, System.EventArgs e)
    {
      // Hack menu item to zoom value...
      string name = ((MenuItem)sender).Text;
      name = name.Substring(0, name.Length - 1);
      this.pagePreview.ZoomPercent = int.Parse(name);
      UpdateStatusBar();
    }

    private void miBestFit_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.Zoom = MigraDoc.Rendering.Forms.Zoom.BestFit;
      UpdateStatusBar();
    }

    private void miFullPage_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.Zoom = MigraDoc.Rendering.Forms.Zoom.FullPage;
      UpdateStatusBar();
    }

    private void miOriginalSize_Click(object sender, EventArgs e)
    {
      this.pagePreview.Zoom = MigraDoc.Rendering.Forms.Zoom.OriginalSize;
      UpdateStatusBar();
    }

    private void miFirst_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.FirstPage();
      UpdateStatusBar();
    }

    private void miPrev_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.PrevPage();
      UpdateStatusBar();
    }

    private void miNext_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.NextPage();
      UpdateStatusBar();
    }

    private void miLast_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.LastPage();
      UpdateStatusBar();
    }

    /// <summary>
    /// Creates a PDF file from the current document.
    /// </summary>
    private void miPdf_Click(object sender, System.EventArgs e)
    {
      PdfDocumentRenderer printer = new PdfDocumentRenderer();
      printer.DocumentRenderer = this.pagePreview.Renderer;
      printer.Document = this.pagePreview.Document;
      printer.RenderDocument();
      this.pagePreview.Document.BindToRenderer(null);
      printer.Save("test.pdf");

      Process.Start("test.pdf");
    }

    /// <summary>
    /// Creates a RTF file from the current document.
    /// </summary>
    private void miRtf_Click(object sender, System.EventArgs e)
    {
      RtfDocumentRenderer rtf = new RtfDocumentRenderer();
      rtf.Render(this.pagePreview.Document, "test.rtf", null);

      Process.Start("test.rtf");
    }

    /// <summary>
    /// Creates an image from the current page.
    /// </summary>
    private void miBmp_Click(object sender, System.EventArgs e)
    {
      int page = this.pagePreview.Page;

      // Reuse the renderer from the preview
      DocumentRenderer renderer = this.pagePreview.Renderer;
      PageInfo info = renderer.FormattedDocument.GetPageInfo(page);

      // Create an image
      int dpi = 150;
      int dx, dy;
      if (info.Orientation == PdfSharp.PageOrientation.Portrait)
      {
        dx = (int)(info.Width.Inch * dpi);
        dy = (int)(info.Height.Inch * dpi);
      }
      else
      {
        dx = (int)(info.Height.Inch * dpi);
        dy = (int)(info.Width.Inch * dpi);
      }

      Image image = new Bitmap(dx, dy, PixelFormat.Format32bppRgb);

      // Create a Graphics object for the image and scale it for drawing with 72 dpi
      Graphics graphics = Graphics.FromImage(image);
      graphics.Clear(System.Drawing.Color.White);
      float scale = dpi / 72f;
      graphics.ScaleTransform(scale, scale);

      // Create an XGraphics object and render the page
      XGraphics gfx = XGraphics.FromGraphics(graphics, new XSize(info.Width.Point, info.Height.Point));
      renderer.RenderPage(gfx, page);
      gfx.Dispose();
      image.Save("test.png", ImageFormat.Png);

      Process.Start("mspaint", "test.png");  // Use MSPaint, not Photoshop...
    }

    /// <summary>
    /// Creates a meta file from the current page.
    /// </summary>
    private void miMetaFile_Click(object sender, EventArgs e)
    {
      int page = this.pagePreview.Page;

      // Reuse the renderer from the preview
      DocumentRenderer renderer = this.pagePreview.Renderer;
      PageInfo info = renderer.FormattedDocument.GetPageInfo(page);

      // Create an image
      float dx, dy;
      if (info.Orientation == PdfSharp.PageOrientation.Portrait)
      {
        dx = (float)(info.Width.Inch * 72);
        dy = (float)(info.Height.Inch * 72);
      }
      else
      {
        dx = (float)(info.Height.Inch * 72);
        dy = (float)(info.Width.Inch * 72);
      }

      // Create a graphics object as reference
      Graphics graphicsDisplay = CreateGraphics();
      IntPtr hdc = graphicsDisplay.GetHdc();

      // There is a little difference between the display resolution (i.g. 96 DPI) and the real physical solution of the display.
      // This must be taken into account...
      DeviceInfos devInfo = DeviceInfos.GetInfos(hdc);

      // Create the metafile
      Metafile metafile = new Metafile("test.emf", hdc, 
        new RectangleF(0, 0, devInfo.ScaleX * dx, devInfo.ScaleY * dy), MetafileFrameUnit.Point);
      graphicsDisplay.ReleaseHdc(hdc);
      graphicsDisplay.Dispose();

      // Create a Graphics object for the metafile and scale it for drawing with 72 dpi
      Graphics graphics = Graphics.FromImage(metafile);
      graphics.Clear(System.Drawing.Color.White);
      //graphics.PageUnit = GraphicsUnit.Point; ???
      graphics.ScaleTransform(graphics.DpiX / 72, graphics.DpiY / 72);

      // Check if size is correct
      graphics.DrawLine(Pens.Red, 0, 0, dx, dy);

      // Create an XGraphics object and render the page
      XGraphics gfx = XGraphics.FromGraphics(graphics, new XSize(info.Width.Point, info.Height.Point));
      renderer.RenderPage(gfx, page);
      gfx.Dispose();

      metafile.Dispose();

      Process.Start("test.emf");
    }


    /// <summary>
    /// Demonstrates the preview using System.Windows.Froms.PrintPreviewDialog.
    /// In .NET 1.x this dialog is a lousy implementation. In .NET 2.0 it's a litte bit better 
    /// (at least portrait/landscape is handled correctly...).
    /// </summary>
    private void miPreview_Click(object sender, System.EventArgs e)
    {
      using (PrintPreviewDialog dialog = new PrintPreviewDialog())
      {
        dialog.Text = "Preview using System.Windows.Froms.PrintPreviewDialog";
#if NET_2_0
        dialog.ShowIcon = false;
#endif
        dialog.MinimizeBox = false;
        dialog.MaximizeBox = false;

        // Reuse the renderer from the preview
        DocumentRenderer renderer = this.pagePreview.Renderer;

        // Creates a PrintDocument that simplifies printing of MigraDoc documents
        MigraDocPrintDocument printDocument = new MigraDocPrintDocument();

        // Attach the current printer settings
        printDocument.PrinterSettings = this.printerSettings;

        // Attach the current document renderer
        printDocument.Renderer = renderer;

        // Attach the current print document
        dialog.Document = printDocument;

        // Show the preview
        dialog.ShowDialog();
      }
    }

    /// <summary>
    /// Opens the printer setup dialog.
    /// </summary>
    private void miPrinterSetup_Click(object sender, System.EventArgs e)
    {
      using (PrintDialog dialog = new PrintDialog())
      {
        dialog.PrinterSettings = this.printerSettings;
        dialog.AllowSelection = true;
        dialog.AllowSomePages = true;
        dialog.ShowDialog();
      }
    }

    /// <summary>
    /// Called by preview control to reflect changes to the text in the status bar.
    /// </summary>
    private void pagePreview_Changed(object sender, EventArgs e)
    {
      UpdateStatusBar();
    }

    private void miSample1_Click(object sender, EventArgs e)
    {
      Document document = SampleDocuments.CreateSample1();
      this.pagePreview.Ddl = DdlWriter.WriteToString(document);
    }

    private void miSample2_Click(object sender, EventArgs e)
    {
      Directory.SetCurrentDirectory(GetProgramDirectory());
      Document document = SampleDocuments.CreateSample2();
      this.pagePreview.Ddl = DdlWriter.WriteToString(document);
    }

    private void miExit_Click(object sender, System.EventArgs e)
    {
      Close();
    }

    private string GetProgramDirectory()
    {
      System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
      return Path.GetDirectoryName(assembly.Location);
    }

  }
}
