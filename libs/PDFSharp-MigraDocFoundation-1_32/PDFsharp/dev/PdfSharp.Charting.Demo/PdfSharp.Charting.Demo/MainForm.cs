#region PDFsharp Charting - A .NET charting library based on PDFsharp
//
// Authors:
//   Niklas Schneider (mailto:Niklas.Schneider@pdfsharp.com)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Forms;

namespace PdfSharp.Charting.Demo
{
  /// <summary>
  /// MainForm.
  /// </summary>
  public class MainForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Panel pnlMain;
    private System.Windows.Forms.Panel pnlLeft;
    private System.Windows.Forms.Panel pnlRight;
    private System.Windows.Forms.Splitter splitter;
    private System.Windows.Forms.TreeView tvNavigation;
    private PdfSharp.Forms.PagePreview pagePreview;
    private System.Windows.Forms.Button btnPdf;
    private System.ComponentModel.Container components = null;
    private System.Windows.Forms.Panel pnlPreview;
    private System.Windows.Forms.Button btnOriginalSize;
    private System.Windows.Forms.Button btnFullPage;
    private System.Windows.Forms.Button btnBestFit;
    private System.Windows.Forms.Button btnMinus;
    private System.Windows.Forms.Button btnPlus;

    private ChartFrame chartFrame;

    public MainForm()
    {
      InitializeComponent();
      this.tvNavigation.SelectedNode = this.tvNavigation.Nodes[0];
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

    int GetNewZoom(int currentZoom, bool larger)
    {
      int[] values = new int[]
      {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200, 
        250, 300, 350, 400, 450, 500, 600, 700, 800
      };

      if (currentZoom <= (int)Zoom.Mininum && !larger)
        return (int)Zoom.Mininum;
      else if (currentZoom >= (int)Zoom.Maximum && larger)
        return (int)Zoom.Maximum;

      if (larger)
      {
        for (int i = 0; i < values.Length; i++)
        {
          if (currentZoom < values[i])
            return values[i];
        }
      }
      else
      {
        for (int i = values.Length - 1; i >= 0 ; i--)
        {
          if (currentZoom > values[i])
            return values[i];
        }
      }
      return (int)Zoom.Percent100;
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
      this.pnlMain = new System.Windows.Forms.Panel();
      this.pnlRight = new System.Windows.Forms.Panel();
      this.pnlPreview = new System.Windows.Forms.Panel();
      this.pagePreview = new PdfSharp.Forms.PagePreview();
      this.splitter = new System.Windows.Forms.Splitter();
      this.pnlLeft = new System.Windows.Forms.Panel();
      this.tvNavigation = new System.Windows.Forms.TreeView();
      this.btnPdf = new System.Windows.Forms.Button();
      this.btnOriginalSize = new System.Windows.Forms.Button();
      this.btnFullPage = new System.Windows.Forms.Button();
      this.btnBestFit = new System.Windows.Forms.Button();
      this.btnMinus = new System.Windows.Forms.Button();
      this.btnPlus = new System.Windows.Forms.Button();
      this.pnlMain.SuspendLayout();
      this.pnlRight.SuspendLayout();
      this.pnlPreview.SuspendLayout();
      this.pnlLeft.SuspendLayout();
      this.SuspendLayout();
      // 
      // pnlMain
      // 
      this.pnlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlMain.BackColor = System.Drawing.SystemColors.Control;
      this.pnlMain.Controls.Add(this.pnlRight);
      this.pnlMain.Controls.Add(this.splitter);
      this.pnlMain.Controls.Add(this.pnlLeft);
      this.pnlMain.Location = new System.Drawing.Point(0, 80);
      this.pnlMain.Name = "pnlMain";
      this.pnlMain.Size = new System.Drawing.Size(792, 486);
      this.pnlMain.TabIndex = 0;
      // 
      // pnlRight
      // 
      this.pnlRight.BackColor = System.Drawing.SystemColors.Control;
      this.pnlRight.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pnlRight.Controls.Add(this.pnlPreview);
      this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlRight.Location = new System.Drawing.Point(205, 0);
      this.pnlRight.Name = "pnlRight";
      this.pnlRight.Size = new System.Drawing.Size(587, 486);
      this.pnlRight.TabIndex = 2;
      // 
      // pnlPreview
      // 
      this.pnlPreview.Controls.Add(this.pagePreview);
      this.pnlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pnlPreview.Location = new System.Drawing.Point(0, 0);
      this.pnlPreview.Name = "pnlPreview";
      this.pnlPreview.Size = new System.Drawing.Size(583, 482);
      this.pnlPreview.TabIndex = 0;
      // 
      // pagePreview
      // 
      this.pagePreview.BackColor = System.Drawing.SystemColors.Control;
      this.pagePreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.pagePreview.DesktopColor = System.Drawing.SystemColors.ControlDark;
      this.pagePreview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pagePreview.Location = new System.Drawing.Point(0, 0);
      this.pagePreview.Name = "pagePreview";
      this.pagePreview.PageColor = System.Drawing.Color.GhostWhite;
      this.pagePreview.PageSize = new System.Drawing.Size(595, 842);
      this.pagePreview.Size = new System.Drawing.Size(583, 482);
      this.pagePreview.TabIndex = 4;
      this.pagePreview.Zoom = PdfSharp.Forms.Zoom.BestFit;
      this.pagePreview.ZoomPercent = 70;
      // 
      // splitter
      // 
      this.splitter.BackColor = System.Drawing.SystemColors.Control;
      this.splitter.Location = new System.Drawing.Point(200, 0);
      this.splitter.Name = "splitter";
      this.splitter.Size = new System.Drawing.Size(5, 486);
      this.splitter.TabIndex = 1;
      this.splitter.TabStop = false;
      // 
      // pnlLeft
      // 
      this.pnlLeft.BackColor = System.Drawing.SystemColors.Control;
      this.pnlLeft.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.pnlLeft.Controls.Add(this.tvNavigation);
      this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
      this.pnlLeft.Location = new System.Drawing.Point(0, 0);
      this.pnlLeft.Name = "pnlLeft";
      this.pnlLeft.Size = new System.Drawing.Size(200, 486);
      this.pnlLeft.TabIndex = 0;
      // 
      // tvNavigation
      // 
      this.tvNavigation.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tvNavigation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvNavigation.ImageIndex = -1;
      this.tvNavigation.Location = new System.Drawing.Point(0, 0);
      this.tvNavigation.Name = "tvNavigation";
      this.tvNavigation.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
                                                                             new System.Windows.Forms.TreeNode("Line Chart"),
                                                                             new System.Windows.Forms.TreeNode("Column Chart"),
                                                                             new System.Windows.Forms.TreeNode("Stacked Column Chart"),
                                                                             new System.Windows.Forms.TreeNode("Bar Chart"),
                                                                             new System.Windows.Forms.TreeNode("Stacked Bar Chart"),
                                                                             new System.Windows.Forms.TreeNode("Area Chart"),
                                                                             new System.Windows.Forms.TreeNode("Pie Chart"),
                                                                             new System.Windows.Forms.TreeNode("Pie Exploded Chart"),
                                                                             new System.Windows.Forms.TreeNode("Combination Chart")});
      this.tvNavigation.SelectedImageIndex = -1;
      this.tvNavigation.Size = new System.Drawing.Size(196, 482);
      this.tvNavigation.TabIndex = 0;
      this.tvNavigation.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvNavigation_AfterSelect);
      // 
      // btnPdf
      // 
      this.btnPdf.BackColor = System.Drawing.Color.Transparent;
      this.btnPdf.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnPdf.Image = ((System.Drawing.Image)(resources.GetObject("btnPdf.Image")));
      this.btnPdf.Location = new System.Drawing.Point(464, 16);
      this.btnPdf.Name = "btnPdf";
      this.btnPdf.Size = new System.Drawing.Size(44, 44);
      this.btnPdf.TabIndex = 1;
      this.btnPdf.Click += new System.EventHandler(this.btnPdf_Click);
      // 
      // btnOriginalSize
      // 
      this.btnOriginalSize.BackColor = System.Drawing.Color.Transparent;
      this.btnOriginalSize.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnOriginalSize.Image = ((System.Drawing.Image)(resources.GetObject("btnOriginalSize.Image")));
      this.btnOriginalSize.Location = new System.Drawing.Point(204, 16);
      this.btnOriginalSize.Name = "btnOriginalSize";
      this.btnOriginalSize.Size = new System.Drawing.Size(44, 44);
      this.btnOriginalSize.TabIndex = 2;
      this.btnOriginalSize.Click += new System.EventHandler(this.btnOriginalSize_Click);
      // 
      // btnFullPage
      // 
      this.btnFullPage.BackColor = System.Drawing.Color.Transparent;
      this.btnFullPage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnFullPage.Image = ((System.Drawing.Image)(resources.GetObject("btnFullPage.Image")));
      this.btnFullPage.Location = new System.Drawing.Point(256, 16);
      this.btnFullPage.Name = "btnFullPage";
      this.btnFullPage.Size = new System.Drawing.Size(44, 44);
      this.btnFullPage.TabIndex = 2;
      this.btnFullPage.Click += new System.EventHandler(this.btnFullPage_Click);
      // 
      // btnBestFit
      // 
      this.btnBestFit.BackColor = System.Drawing.Color.Transparent;
      this.btnBestFit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnBestFit.Image = ((System.Drawing.Image)(resources.GetObject("btnBestFit.Image")));
      this.btnBestFit.Location = new System.Drawing.Point(308, 16);
      this.btnBestFit.Name = "btnBestFit";
      this.btnBestFit.Size = new System.Drawing.Size(44, 44);
      this.btnBestFit.TabIndex = 2;
      this.btnBestFit.Click += new System.EventHandler(this.btnBestFit_Click);
      // 
      // btnMinus
      // 
      this.btnMinus.BackColor = System.Drawing.Color.Transparent;
      this.btnMinus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnMinus.Image = ((System.Drawing.Image)(resources.GetObject("btnMinus.Image")));
      this.btnMinus.Location = new System.Drawing.Point(360, 16);
      this.btnMinus.Name = "btnMinus";
      this.btnMinus.Size = new System.Drawing.Size(44, 44);
      this.btnMinus.TabIndex = 2;
      this.btnMinus.Click += new System.EventHandler(this.btnMinus_Click);
      // 
      // btnPlus
      // 
      this.btnPlus.BackColor = System.Drawing.Color.Transparent;
      this.btnPlus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnPlus.Image = ((System.Drawing.Image)(resources.GetObject("btnPlus.Image")));
      this.btnPlus.Location = new System.Drawing.Point(412, 16);
      this.btnPlus.Name = "btnPlus";
      this.btnPlus.Size = new System.Drawing.Size(44, 44);
      this.btnPlus.TabIndex = 2;
      this.btnPlus.Click += new System.EventHandler(this.btnPlus_Click);
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.ClientSize = new System.Drawing.Size(792, 566);
      this.Controls.Add(this.btnOriginalSize);
      this.Controls.Add(this.btnPdf);
      this.Controls.Add(this.pnlMain);
      this.Controls.Add(this.btnFullPage);
      this.Controls.Add(this.btnBestFit);
      this.Controls.Add(this.btnMinus);
      this.Controls.Add(this.btnPlus);
      this.DockPadding.Bottom = -2;
      this.Name = "MainForm";
      this.Text = "PDFsharp Charting Demo";
      this.pnlMain.ResumeLayout(false);
      this.pnlRight.ResumeLayout(false);
      this.pnlPreview.ResumeLayout(false);
      this.pnlLeft.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    private void tvNavigation_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
    {
      this.chartFrame = new ChartFrame();
      this.chartFrame.Location = new XPoint(30, 30);
      this.chartFrame.Size = new XSize(500, 600);

      Chart chart = null;
      switch (e.Node.Text.ToLower())
      {
        case "line chart":
          chart = ChartSamples.LineChart();
          break;

        case "column chart":
          chart = ChartSamples.ColumnChart();
          break;

        case "stacked column chart":
          chart = ChartSamples.ColumnStackedChart();
          break;

        case "bar chart":
          chart = ChartSamples.BarChart();
          break;

        case "stacked bar chart":
          chart = ChartSamples.BarStackedChart();
          break;

        case "area chart":
          chart = ChartSamples.AreaChart();
          break;

        case "pie chart":
          chart = ChartSamples.PieChart();
          break;

        case "pie exploded chart":
          chart = ChartSamples.PieExplodedChart();
          break;

        case "combination chart":
          chart = ChartSamples.CombinationChart();
          break;
      }
      this.chartFrame.Add(chart);

      this.pagePreview.SetRenderEvent(new PagePreview.RenderEvent(chartFrame.Draw));
    }

    private void btnOriginalSize_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.Zoom = Zoom.Percent100;
    }

    private void btnFullPage_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.Zoom = Zoom.FullPage;
    }

    private void btnBestFit_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.Zoom = Zoom.BestFit;
    }

    private void btnMinus_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.ZoomPercent = GetNewZoom((int)this.pagePreview.ZoomPercent, false);
    }

    private void btnPlus_Click(object sender, System.EventArgs e)
    {
      this.pagePreview.ZoomPercent = GetNewZoom((int)this.pagePreview.ZoomPercent, true);
    }

    private void btnPdf_Click(object sender, System.EventArgs e)
    {
      string filename = Guid.NewGuid().ToString().ToUpper() + ".pdf";
      PdfDocument document = new PdfDocument(filename);

      PdfPage page = document.AddPage();
      page.Size = PageSize.A4;
      XGraphics gfx = XGraphics.FromPdfPage(page);
      this.chartFrame.Draw(gfx);
      document.Close();
      Process.Start(filename);
    }

  }
}
