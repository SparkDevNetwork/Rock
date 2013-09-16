#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Data;
using System.Windows.Forms;
using PdfSharp.Drawing;
using MigraDoc.Rendering;

namespace MigraDoc.Rendering.Forms
{
  /// <summary>
  /// Event handler for the PagePreview event.
  /// </summary>
  public delegate void PagePreviewEventHandler(object sender, EventArgs e);

  /// <summary>
  /// Represents a Windows control to display a MigraDoc document.
  /// </summary>
  public class DocumentPreview : System.Windows.Forms.UserControl
  {
    private PdfSharp.Forms.PagePreview preview;
    private System.ComponentModel.Container components = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPreview"/> class.
    /// </summary>
    public DocumentPreview()
    {
      InitializeComponent();
      this.preview.ZoomChanged += new EventHandler(preview_ZoomChanged);
      this.preview.SetRenderEvent(new PdfSharp.Forms.PagePreview.RenderEvent(RenderPage));
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

    Zoom GetNewZoomFactor(int currentZoom, bool larger)
    {
      int[] values = new int[]
      {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200, 
        250, 300, 350, 400, 450, 500, 600, 700, 800
      };

      if (currentZoom <= 10 && !larger)
        return Zoom.Percent10;
      else if (currentZoom >= 800 && larger)
        return Zoom.Percent800;

      if (larger)
      {
        for (int i = 0; i < values.Length; i++)
        {
          if (currentZoom < values[i])
            return (Zoom)values[i];
        }
      }
      else
      {
        for (int i = values.Length - 1; i >= 0; i--)
        {
          if (currentZoom > values[i])
            return (Zoom)values[i];
        }
      }
      return Zoom.Percent100;
    }

    /// <summary>
    /// Gets or sets the border style of the tree view control.
    /// </summary>
    /// <value></value>
    /// <returns>
    /// One of the <see cref="T:System.Windows.Forms.BorderStyle"/> values. The default is <see cref="F:System.Windows.Forms.BorderStyle.Fixed3D"/>.
    /// </returns>
    /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
    /// The assigned value is not one of the <see cref="T:System.Windows.Forms.BorderStyle"/> values.
    /// </exception>
    /// <PermissionSet>
    /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
    /// </PermissionSet>
    [DefaultValue((int)BorderStyle.Fixed3D), Description("Determines the style of the border."), Category("Preview Properties")]
    public new BorderStyle BorderStyle
    {
      get { return this.preview.BorderStyle; }
      set { this.preview.BorderStyle = value; }
    }

    /// <summary>
    /// Gets or sets the private fonts of the document. If used, must be set before Ddl is set!
    /// </summary>
    public XPrivateFontCollection PrivateFonts
    {
      get { return this.privateFonts; }
      set { this.privateFonts = value; }
    }
    internal XPrivateFontCollection privateFonts;

    /// <summary>
    /// Gets or sets a DDL string or file.
    /// </summary>
    public string Ddl
    {
      get { return ddl; }
      set
      {
        ddl = value;
        DdlUpdated();
      }
    }
    string ddl;

    /// <summary>
    /// Gets or sets the current page.
    /// </summary>
    public int Page
    {
      get { return page; }
      set
      {
        try
        {
          if (this.preview != null)
          {
            if (this.page != value)
            {
              this.page = value;
              PageInfo pageInfo = this.renderer.formattedDocument.GetPageInfo(this.page);
              if (pageInfo.Orientation == PdfSharp.PageOrientation.Portrait)
                this.preview.PageSize = new Size((int)pageInfo.Width, (int)pageInfo.Height);
              else
                this.preview.PageSize = new Size((int)pageInfo.Height, (int)pageInfo.Width);

              this.preview.Invalidate();
              OnPageChanged(new EventArgs());
            }
          }
          else
            this.page = -1;
        }
        catch { }
      }
    }
    int page;

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public int PageCount
    {
      get
      {
        if (this.renderer != null)
          return this.renderer.FormattedDocument.PageCount;
        return 0;
      }
    }

    /// <summary>
    /// Goes to the first page.
    /// </summary>
    public void FirstPage()
    {
      if (this.renderer != null)
      {
        Page = 1;
        this.preview.Invalidate();
        OnPageChanged(new EventArgs());
      }
    }

    /// <summary>
    /// Goes to the next page.
    /// </summary>
    public void NextPage()
    {
      if (this.renderer != null && this.page < PageCount)
      {
        Page++;
        this.preview.Invalidate();
        OnPageChanged(new EventArgs());
      }
    }

    /// <summary>
    /// Goes to the previous page.
    /// </summary>
    public void PrevPage()
    {
      if (this.renderer != null && this.page > 1)
      {
        Page--;
      }
    }

    /// <summary>
    /// Goes to the last page.
    /// </summary>
    public void LastPage()
    {
      if (this.renderer != null)
      {
        Page = PageCount;
        this.preview.Invalidate();
        OnPageChanged(new EventArgs());
      }
    }

    ///// <summary>
    ///// Gets or sets the working directory.
    ///// </summary>
    //public string WorkingDirectory
    //{
    //  get
    //  {
    //    return this.workingDirectory;
    //  }
    //  set
    //  {
    //    this.workingDirectory = value;
    //  }
    //}
    //string workingDirectory = "";

    /// <summary>
    /// Called when the Ddl property has changed.
    /// </summary>
    void DdlUpdated()
    {
      if (this.ddl != null)
      {
        this.document = MigraDoc.DocumentObjectModel.IO.DdlReader.DocumentFromString(this.ddl);
        this.renderer = new DocumentRenderer(document);
        this.renderer.PrivateFonts = this.privateFonts;
        this.renderer.PrepareDocument();
        Page = 1;
        this.preview.Invalidate();
      }
      //      if (this.job != null)
      //        this.job.Dispose();
      //
      //      if (this.ddl == null || this.ddl == "")
      //        return;
      //
      //      this.job = new PrintJob();
      //      this.job.Type = JobType.Standard;
      //      this.job.Ddl = this.ddl;
      //      this.job.WorkingDirectory = this.workingDirectory;
      //      this.job.InitDocument();
      //      this.preview = this.job.GetPreview(this.Handle);
      //      this.previewHandle = this.preview.Hwnd;
      //
      //      if (this.preview != null)
      //        this.preview.Page = 1;
    }

    /// <summary>
    /// Gets or sets the MigraDoc document that is previewed in this control.
    /// </summary>
    public MigraDoc.DocumentObjectModel.Document Document
    {
      get { return this.document; }
      set
      {
        if (value != null)
        {
          this.document = value;
          this.renderer = new DocumentRenderer(value);
          this.renderer.PrepareDocument();
          Page = 1;
          this.preview.Invalidate();
        }
        else
        {
          this.document = null;
          this.renderer = null;
          this.preview.Invalidate();
        }
      }
    }
    MigraDoc.DocumentObjectModel.Document document;

    /// <summary>
    /// Gets the underlying DocumentRenderer of the document currently in preview, or null, if no rederer exists.
    /// You can use this renderer for printing or creating PDF file. This evade the necessity to format the
    /// document a secound time when you want to print it or convert it into PDF.
    /// </summary>
    public DocumentRenderer Renderer
    {
      get { return this.renderer; }
    }

    void RenderPage(XGraphics gfx)
    {
      if (this.renderer == null)
        return;

      if (this.renderer != null)
      {
        try
        {
          this.renderer.RenderPage(gfx, this.page);
          return;
        }
        catch { };
      }
    }
    DocumentRenderer renderer;

    /// <summary>
    /// Gets or sets a predefined zoom factor.
    /// </summary>
    [DefaultValue((int)Zoom.FullPage), Description("Determines the zoom of the page."), Category("Preview Properties")]
    public Zoom Zoom
    {
      get { return (Zoom)this.preview.Zoom; }
      set
      {
        if (this.preview.Zoom != (PdfSharp.Forms.Zoom)value)
        {
          this.preview.Zoom = (PdfSharp.Forms.Zoom)value;
          OnZoomChanged(new EventArgs());
        }
      }
    }

    /// <summary>
    /// Gets or sets an arbitrary zoom factor. The range is from 10 to 800.
    /// </summary>
    [DefaultValue((int)Zoom.FullPage), Description("Determines the zoom of the page."), Category("Preview Properties")]
    public int ZoomPercent
    {
      get { return this.preview.ZoomPercent; }
      set
      {
        if (this.preview.ZoomPercent != value)
        {
          this.preview.ZoomPercent = value;
          OnZoomChanged(new EventArgs());
        }
      }
    }
    internal int zoomPercen = 100;

    /// <summary>
    /// Makes zoom factor smaller.
    /// </summary>
    public void MakeSmaller()
    {
      ZoomPercent = (int)GetNewZoomFactor(ZoomPercent, false);
    }

    /// <summary>
    /// Makes zoom factor larger.
    /// </summary>
    public void MakeLarger()
    {
      ZoomPercent = (int)GetNewZoomFactor(ZoomPercent, true);
    }

    /// <summary>
    /// Gets or sets the color of the page.
    /// </summary>
    /// <value>The color of the page.</value>
    [Description("The background color of the page."), Category("Preview Properties")]
    public Color PageColor
    {
      get { return this.preview.PageColor; }
      set { this.preview.PageColor = value; }
    }
    Color pageColor = Color.GhostWhite;

    /// <summary>
    /// Gets or sets the color of the desktop.
    /// </summary>
    /// <value>The color of the desktop.</value>
    [Description("The color of the desktop."), Category("Preview Properties")]
    public Color DesktopColor
    {
      get { return this.preview.DesktopColor; }
      set { this.preview.DesktopColor = value; }
    }
    internal Color desktopColor = SystemColors.ControlDark;

    /// <summary>
    /// Gets or sets a value indicating whether to show scrollbars.
    /// </summary>
    /// <value><c>true</c> if [show scrollbars]; otherwise, <c>false</c>.</value>
    [DefaultValue(true), Description("Determines whether the scrollbars are visible."), Category("Preview Properties")]
    public bool ShowScrollbars
    {
      get { return this.preview.ShowScrollbars; }
      set { this.preview.ShowScrollbars = value; }
    }
    internal bool showScrollbars = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the page.
    /// </summary>
    /// <value><c>true</c> if [show page]; otherwise, <c>false</c>.</value>
    [DefaultValue(true), Description("Determines whether the page visible."), Category("Preview Properties")]
    public bool ShowPage
    {
      get { return this.preview.ShowPage; }
      set { this.preview.ShowPage = value; }
    }
    internal bool showPage = true;

    /// <summary>
    /// Gets or sets the page size in point.
    /// </summary>
    [Description("Determines the size (in points) of the page."), Category("Preview Properties")]
    public Size PageSize
    {
      get { return new Size((int)this.preview.PageSize.Width, (int)this.preview.PageSize.Height); }
      set { this.preview.PageSize = value; }
    }

    /// <summary>
    /// Raises the ZoomChanged event when the zoom factor changed.
    /// </summary>
    protected virtual void OnZoomChanged(EventArgs e)
    {
      if (ZoomChanged != null)
        ZoomChanged(this, e);
    }

    /// <summary>
    /// Occurs when the zoom factor changed.
    /// </summary>
    [Description("Occurs when the zoom factor changed."), Category("Preview Properties")]
    public event PagePreviewEventHandler ZoomChanged;

    /// <summary>
    /// Raises the ZoomChanged event when the current page changed.
    /// </summary>
    protected virtual void OnPageChanged(EventArgs e)
    {
      if (PageChanged != null)
        PageChanged(this, e);
    }

    /// <summary>
    /// Occurs when the current page changed.
    /// </summary>
    [Description("Occurs when the current page changed."), Category("Preview Properties")]
    public event PagePreviewEventHandler PageChanged;


    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.preview = new PdfSharp.Forms.PagePreview();
      this.SuspendLayout();
      // 
      // preview
      // 
      this.preview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.preview.DesktopColor = System.Drawing.SystemColors.ControlDark;
      this.preview.Dock = System.Windows.Forms.DockStyle.Fill;
      this.preview.Location = new System.Drawing.Point(0, 0);
      this.preview.Name = "preview";
      this.preview.PageColor = System.Drawing.Color.GhostWhite;
      this.preview.PageSize = new System.Drawing.Size(595, 842);
      this.preview.Size = new System.Drawing.Size(200, 200);
      this.preview.TabIndex = 0;
      this.preview.Zoom = PdfSharp.Forms.Zoom.FullPage;
      this.preview.ZoomPercent = 15;
      // 
      // PagePreview
      // 
      this.Controls.Add(this.preview);
      this.Name = "PagePreview";
      this.Size = new System.Drawing.Size(200, 200);
      this.ResumeLayout(false);

    }
    #endregion

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data.</param>
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      int delta = e.Delta;
      if (delta > 0)
        PrevPage();
      else if (delta < 0)
        NextPage();
    }

    private void preview_ZoomChanged(object sender, EventArgs e)
    {
      OnZoomChanged(e);
    }
  }
}
