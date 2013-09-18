#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a MigraDoc document to RTF format.
  /// </summary>
  public class RtfDocumentRenderer : RendererBase
  {
    /// <summary>
    /// Initializes a new instance of the DocumentRenderer class.
    /// </summary>
    public RtfDocumentRenderer()
    {
    }

    /// <summary>
    /// This function is declared only for technical reasons!
    /// </summary>
    internal override void Render()
    {
      throw new InvalidOperationException();
    }

    /// <summary>
    /// Renders a MigraDoc document to the specified file.
    /// </summary>
    public void Render(Document doc, string file, string workingDirectory)
    {
      StreamWriter strmWrtr = null;
      try
      {
        this.document = doc;
        this.docObject = doc;
        this.workingDirectory = workingDirectory;
        string path = file;
        if (workingDirectory != null)
          path = Path.Combine(workingDirectory, file);

        strmWrtr = new StreamWriter(path, false, System.Text.Encoding.Default);
        this.rtfWriter = new RtfWriter(strmWrtr);
        WriteDocument();
      }
      finally
      {
        if (strmWrtr != null)
        {
          strmWrtr.Flush();
          strmWrtr.Close();
        }
      }
    }

    /// <summary>
    /// Renders a MigraDoc document to the specified stream.
    /// </summary>
    public void Render(Document document, Stream stream, string workingDirectory)
    {
      if (document == null)
        throw new ArgumentNullException("document");
      if (document.UseCmykColor)
        throw new InvalidOperationException("Cannot create RTF document with CMYK colors.");

      StreamWriter strmWrtr = null;
      try
      {
        strmWrtr = new StreamWriter(stream, System.Text.Encoding.Default);
        this.document = document;
        this.docObject = document;
        this.workingDirectory = workingDirectory;
        this.rtfWriter = new RtfWriter(strmWrtr);
        WriteDocument();
      }
      finally
      {
        if (strmWrtr != null)
        {
          strmWrtr.Flush();
          strmWrtr.Close();
        }
      }
    }

    /// <summary>
    /// Renders a MigraDoc to Rtf and returns the result as string.
    /// </summary>
    public string RenderToString(Document document, string workingDirectory)
    {
      if (document == null)
        throw new ArgumentNullException("document");
       if (document.UseCmykColor)
        throw new InvalidOperationException("Cannot create RTF document with CMYK colors.");

      this.document = document;
      this.docObject = document;
      this.workingDirectory = workingDirectory;
      StringWriter writer = null;
      try
      {
        writer = new StringWriter();
        this.rtfWriter = new RtfWriter(writer);
        WriteDocument();
        writer.Flush();
        return writer.GetStringBuilder().ToString();
      }
      finally
      {
        if (writer != null)
          writer.Close();
      }
    }

    /// <summary>
    /// Renders a MigraDoc document with help of the internal RtfWriter.
    /// </summary>
    private void WriteDocument()
    {
      RtfFlattenVisitor flattener = new RtfFlattenVisitor();
      flattener.Visit(this.document);
      Prepare();
      this.rtfWriter.StartContent();
      RenderHeader();
      RenderDocumentArea();
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Prepares this renderer by collecting Information for font and color table.
    /// </summary>
    private void Prepare()
    {
      this.fontList.Clear();
        //Fonts 
      this.fontList.Add("Symbol");
      this.fontList.Add("Wingdings");
      this.fontList.Add("Courier New");

      this.colorList.Clear();
      this.colorList.Add(Colors.Black);//!!necessary for borders!!
      this.listList.Clear();
      ListInfoRenderer.Clear();
      ListInfoOverrideRenderer.Clear();
      CollectTables(this.document);
    }

    /// <summary>
    /// Renders the RTF Header.
    /// </summary>
    private void RenderHeader()
    {
      this.rtfWriter.WriteControl("rtf", 1);
      this.rtfWriter.WriteControl("ansi");
      this.rtfWriter.WriteControl("ansicpg", 1252);
      this.rtfWriter.WriteControl("deff", 0);//default font

      //Document properties can occur before and between the header tables.

      RenderFontTable();
      RenderColorTable();
      RenderStyles();
      //Lists are not yet implemented.
      RenderListTable();
    }

    /// <summary>
    /// Fills the font, color and (later!) list hashtables so they can be rendered and used by other renderers.
    /// </summary>
    private void CollectTables(DocumentObject dom)
    {
      ValueDescriptorCollection vds = Meta.GetMeta(dom).ValueDescriptors;
      int count = vds.Count;
      for (int idx = 0; idx < count; idx++)
      {
        ValueDescriptor vd = vds[idx];
        if (!vd.IsRefOnly && !vd.IsNull(dom))
        {
          if (vd.ValueType == typeof(Color))
          {
            Color clr = (Color)vd.GetValue(dom, GV.ReadWrite);
            clr = clr.GetMixedTransparencyColor();
            if (!this.colorList.Contains(clr))
              this.colorList.Add(clr);
          }
          else if (vd.ValueType == typeof(Font))
          {
            Font fnt = vd.GetValue(dom, GV.ReadWrite) as Font; //ReadOnly
            if (!fnt.IsNull("Name") && !this.fontList.Contains(fnt.Name))
              this.fontList.Add(fnt.Name);
          }
          else if (vd.ValueType == typeof(ListInfo))
          {
            ListInfo lst = vd.GetValue(dom, GV.ReadWrite) as ListInfo; //ReadOnly
            if (!this.listList.Contains(lst))
              this.listList.Add(lst);
          }
          if (typeof(DocumentObject).IsAssignableFrom(vd.ValueType))
          {
            CollectTables(vd.GetValue(dom, GV.ReadWrite) as DocumentObject); //ReadOnly
            if (typeof(DocumentObjectCollection).IsAssignableFrom(vd.ValueType))
            {
              DocumentObjectCollection coll = vd.GetValue(dom, GV.ReadWrite) as DocumentObjectCollection; //ReadOnly
              if (coll != null)
              {
                foreach (DocumentObject obj in coll)
                {
                  //In SeriesCollection kann null vorkommen.
                  if (obj != null)
                    CollectTables(obj);
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Renders the font hashtable within the RTF header.
    /// </summary>
    private void RenderFontTable()
    {
      if (this.fontList.Count == 0)
        return;

      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("fonttbl");
      for (int idx = 0; idx < this.fontList.Count; ++idx)
      {
        this.rtfWriter.StartContent();
        string name = (string)fontList[idx];
        this.rtfWriter.WriteControl("f", idx);
        System.Drawing.Font font = new System.Drawing.Font(name, 12); //any size
        this.rtfWriter.WriteControl("fcharset", (int)font.GdiCharSet);
        this.rtfWriter.WriteText(name);
        this.rtfWriter.WriteSeparator();
        this.rtfWriter.EndContent();
      }
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Renders the color hashtable within the RTF header.
    /// </summary>
    private void RenderColorTable()
    {
      if (this.colorList.Count == 0)
        return;

      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("colortbl");
      //this would indicate index 0 as auto color:
      //this.rtfWriter.WriteSeparator();
      //left away cause there is no auto color in MigraDoc.
      foreach (object obj in this.colorList)
      {
        Color color = (Color)obj;
        this.rtfWriter.WriteControl("red", (int)color.R);
        this.rtfWriter.WriteControl("green", (int)color.G);
        this.rtfWriter.WriteControl("blue", (int)color.B);
        this.rtfWriter.WriteSeparator();
      }
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Gets the font table index for the specified font name.
    /// </summary>
    internal int GetFontIndex(string fontName)
    {
      if (this.fontList.Contains(fontName))
        return (int)this.fontList.IndexOf(fontName);

      //development purpose exception
      throw new ArgumentException("Font does not exist in this document's font table.", "fontName");
    }

    /// <summary>
    /// Gets the color table index for the specified color.
    /// </summary>
    internal int GetColorIndex(Color color)
    {
      Color clr = color.GetMixedTransparencyColor();
      int idx = (int)this.colorList.IndexOf(clr);
      //development purpose exception
      if (idx < 0)
        throw new ArgumentException("Color does not exist in this document's color table.", "color");
      return idx;
    }

    /// <summary>
    /// Gets the style index for the specified color.
    /// </summary>
    internal int GetStyleIndex(string styleName)
    {
      return this.document.Styles.GetIndex(styleName);
    }

    /// <summary>
    /// Renders styles as part of the RTF header.
    /// </summary>
    private void RenderStyles()
    {
      rtfWriter.StartContent();
      rtfWriter.WriteControl("stylesheet");
      foreach (Style style in this.document.Styles)
      {
        RendererFactory.CreateRenderer(style, this).Render();
      }
      rtfWriter.EndContent();
    }

    /// <summary>
    /// Renders the list hashtable within the RTF header.
    /// </summary>
    private void RenderListTable()
    {
      if (this.listList.Count == 0)
        return;

      rtfWriter.StartContent();
      rtfWriter.WriteControlWithStar("listtable");
      foreach (object obj in this.listList)
      {
        ListInfo lst = (ListInfo)obj;
        ListInfoRenderer lir = new ListInfoRenderer(lst, this);
        lir.Render();
      }
      rtfWriter.EndContent();
      
      rtfWriter.StartContent();
      rtfWriter.WriteControlWithStar("listoverridetable");
      foreach (object obj in this.listList)
      {
        ListInfo lst = (ListInfo)obj;
        ListInfoOverrideRenderer lir = 
            new ListInfoOverrideRenderer(lst, this);
        lir.Render();
      }
      rtfWriter.EndContent();
    }

    /// <summary>
    /// Renders the RTF document area, which is all except the header.
    /// </summary>
    private void RenderDocumentArea()
    {
      RenderInfo();
      RenderDocumentFormat();
      RenderGlobalPorperties();
      foreach (Section sect in this.document.Sections)
      {
        RendererFactory.CreateRenderer(sect, this).Render();
      }
    }

    /// <summary>
    /// Renders global document properties, such as mirror margins and unicode treatment.
    /// Note that a section specific margin mirroring does not work in Word.
    /// </summary>
    private void RenderGlobalPorperties()
    {
      this.rtfWriter.WriteControl("viewkind", 4);
      this.rtfWriter.WriteControl("uc", 1);

      //Em4-Space doesn't work without this:
      this.rtfWriter.WriteControl("lnbrkrule");

      //Footnotes only, no endnotes:
      this.rtfWriter.WriteControl("fet", 0);

      //Enables title pages as (FirstpageHeader):
      this.rtfWriter.WriteControl("facingp");

      //Space between paragraphs as maximum between space after and space before:
      this.rtfWriter.WriteControl("htmautsp");

      //Word cannot realize the mirror margins property for single sections,
      //although rtf control words exist for this purpose. 
      //Thus, the mirror margins property is set globally if it's true for the first section.
      Section sec = this.document.Sections.First as Section;
      if (sec != null)
      {
        if (!sec.PageSetup.IsNull("MirrorMargins") && sec.PageSetup.MirrorMargins)
          this.rtfWriter.WriteControl("margmirror");
      }
    }

    /// <summary>
    /// Renders the document format such as standard tab stops and footnote settings.
    /// </summary>
    private void RenderDocumentFormat()
    {
      Translate("DefaultTabStop", "deftab");
      Translate("FootnoteNumberingRule", "ftn");
      Translate("FootnoteLocation", "ftn", RtfUnit.Undefined, "bj", false);
      Translate("FootnoteNumberStyle", "ftnn");
      Translate("FootnoteStartingNumber", "ftnstart");
    }

    /// <summary>
    /// Renders footnote properties for a section. (not part of the rtf specification, but necessary for Word)
    /// </summary>
    internal void RenderSectionProperties()
    {
      Translate("FootnoteNumberingRule", "sftn");
      Translate("FootnoteLocation", "sftn", RtfUnit.Undefined, "bj", false);
      Translate("FootnoteNumberStyle", "sftnn");
      Translate("FootnoteStartingNumber", "sftnstart");
    }

    /// <summary>
    /// Renders the document information of title, author etc..
    /// </summary>
    private void RenderInfo()
    {
      if (document.IsNull("Info"))
        return;

      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("info");
      DocumentInfo info = this.document.Info;
      if (!info.IsNull("Title"))
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("title");
        this.rtfWriter.WriteText(info.Title);
        this.rtfWriter.EndContent();
      }
      if (!info.IsNull("Subject"))
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("subject");
        this.rtfWriter.WriteText(info.Subject);
        this.rtfWriter.EndContent();
      }
      if (!info.IsNull("Author"))
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("author");
        this.rtfWriter.WriteText(info.Author);
        this.rtfWriter.EndContent();
      }
      if (!info.IsNull("Keywords"))
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("keywords");
        this.rtfWriter.WriteText(info.Keywords);
        this.rtfWriter.EndContent();
      }
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Gets the MigraDoc document that is currently rendered.
    /// </summary>
    internal Document Document
    {
      get { return this.document; }
    }
    private Document document;

    /// <summary>
    /// Gets the RtfWriter the document is rendered with.
    /// </summary>
    internal RtfWriter RtfWriter
    {
      get { return this.rtfWriter; }
    }

    internal string WorkingDirectory
    {
      get { return this.workingDirectory; }
    }
    string workingDirectory;

    private ArrayList colorList = new ArrayList();
    private ArrayList fontList = new ArrayList();
    private ArrayList listList = new ArrayList();
  }
}
