#region PDFsharp Viewing - A .NET wrapper of the Adobe ActiveX control
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using PdfSharp.Viewing;

namespace Tester
{
  /// <summary>
  /// Sample of a PDF viewer.
  /// </summary>
  public class PdfViewer : System.Windows.Forms.Form
  {
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;
    private System.Windows.Forms.Button button4;
    private System.Windows.Forms.Button button5;
    private System.Windows.Forms.Button button6;
    private System.Windows.Forms.Button button7;
    private PdfSharp.Viewing.PdfAcroViewer acroViewer;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public PdfViewer()
    {
      InitializeComponent();
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
        this.acroViewer.Dispose();
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PdfViewer));
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.button4 = new System.Windows.Forms.Button();
      this.button5 = new System.Windows.Forms.Button();
      this.button6 = new System.Windows.Forms.Button();
      this.button7 = new System.Windows.Forms.Button();
      this.acroViewer = new PdfSharp.Viewing.PdfAcroViewer();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(4, 4);
      this.button1.Name = "button1";
      this.button1.TabIndex = 1;
      this.button1.Text = "Load PDF";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.Location = new System.Drawing.Point(84, 4);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(72, 23);
      this.button2.TabIndex = 2;
      this.button2.Text = "Outline";
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // button3
      // 
      this.button3.Location = new System.Drawing.Point(164, 4);
      this.button3.Name = "button3";
      this.button3.TabIndex = 3;
      this.button3.Text = "<<";
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // button4
      // 
      this.button4.Location = new System.Drawing.Point(244, 4);
      this.button4.Name = "button4";
      this.button4.TabIndex = 4;
      this.button4.Text = "<";
      this.button4.Click += new System.EventHandler(this.button4_Click);
      // 
      // button5
      // 
      this.button5.Location = new System.Drawing.Point(324, 4);
      this.button5.Name = "button5";
      this.button5.TabIndex = 5;
      this.button5.Text = ">";
      this.button5.Click += new System.EventHandler(this.button5_Click);
      // 
      // button6
      // 
      this.button6.Location = new System.Drawing.Point(404, 4);
      this.button6.Name = "button6";
      this.button6.TabIndex = 6;
      this.button6.Text = ">>";
      this.button6.Click += new System.EventHandler(this.button6_Click);
      // 
      // button7
      // 
      this.button7.Location = new System.Drawing.Point(484, 4);
      this.button7.Name = "button7";
      this.button7.TabIndex = 7;
      this.button7.Text = "FullPage";
      this.button7.Click += new System.EventHandler(this.button7_Click);
      // 
      // acroViewer
      // 
      this.acroViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.acroViewer.Location = new System.Drawing.Point(0, 34);
      this.acroViewer.Name = "acroViewer";
      this.acroViewer.Size = new System.Drawing.Size(692, 432);
      this.acroViewer.TabIndex = 8;
      // 
      // PdfViewer
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(692, 466);
      this.Controls.Add(this.acroViewer);
      this.Controls.Add(this.button7);
      this.Controls.Add(this.button6);
      this.Controls.Add(this.button5);
      this.Controls.Add(this.button4);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "PdfViewer";
      this.Text = "Sample PDF Viewer based on Adobe Reader ActiveX Control";
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() 
    {
      Application.Run(new PdfViewer());
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      using (OpenFileDialog dlg = new OpenFileDialog())
      {
        dlg.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*" ;
        dlg.ShowDialog();
        this.acroViewer.LoadFile(dlg.FileName);
      }
      //this.acroViewer.ShowToolbar = false;
    }

    private void button2_Click(object sender, System.EventArgs e)
    {
      this.acroViewer.SetPageMode(PdfSharp.Pdf.PdfPageMode.UseThumbs);
    }

    private void button3_Click(object sender, System.EventArgs e)
    {
      this.acroViewer.GotoFirstPage();
    }

    private void button4_Click(object sender, System.EventArgs e)
    {
      this.acroViewer.GotoPreviousPage();
    }

    private void button5_Click(object sender, System.EventArgs e)
    {
      this.acroViewer.GotoNextPage();
    }

    private void button6_Click(object sender, System.EventArgs e)
    {
      this.acroViewer.GotoLastPage();
    }

    private void button7_Click(object sender, System.EventArgs e)
    {
      // TODO seems not to work
      this.acroViewer.SetPageMode(PdfSharp.Pdf.PdfPageMode.UseAttachments);
    }
  }
}
