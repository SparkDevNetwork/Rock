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
using System.Diagnostics;
using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Base class for BorderRenderer and BordersRenderer 
  /// Useful because the BordersRenderer needs to draw single borders too.
  /// </summary>
  internal abstract class BorderRendererBase : RendererBase
  {
    internal BorderRendererBase(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
    }

    /// <summary>
    /// Renders a single border that might have a parent of type Borders or Border .
    /// </summary>
    protected void RenderBorder(string borderControl)
    {
      object visible = GetValueAsIntended("Visible");
      object borderStyle = GetValueAsIntended("Style");
      object borderColor = GetValueAsIntended("Color");
      object borderWidth = GetValueAsIntended("Width");
      //REM: Necessary for invisible borders?
      this.rtfWriter.WriteControl(borderControl);

      if (visible == null && borderStyle == null && borderColor == null && borderWidth == null)
        return;

      //A border of width 0 would be displayed in Word:
      if (borderWidth != null && ((Unit)borderWidth) == 0)
        return;

      if ((visible != null && !(bool)visible) ||
        (borderStyle != null && ((BorderStyle)borderStyle) == BorderStyle.None))
        return;

      //Caution: Write width AFTER style to satisfy word.
      Translate("Style", "brdr", RtfUnit.Undefined, "s", false);
      Translate("Width", "brdrw", RtfUnit.Twips, "10", false);
      Translate("Color", "brdrcf", RtfUnit.Undefined, this.docRenderer.GetColorIndex(GetDefaultColor()).ToString(CultureInfo.InvariantCulture), false);
    }

    /// <summary>
    /// Gets the default color of the Border.
    /// Paragraph Borders use the font color by default.
    /// </summary>
    private Color GetDefaultColor()
    {
      if (this.parentFormat != null)
      {
        object clr = this.parentFormat.GetValue("Font.Color");
        if (clr != null)
          return (Color)clr;
      }
      return Colors.Black;
    }

    /// <summary>
    /// Gets the RTF border control for the given border type.
    /// </summary>
    protected string GetBorderControl(BorderType type)
    {
      string borderCtrl;
      bool isCellBorder = this.parentCell != null;
      if (isCellBorder)
        borderCtrl = "clbrdr";
      else
        borderCtrl = "brdr";
      switch (type)
      {
        case BorderType.Top:
          borderCtrl += "t";
          break;

        case BorderType.Bottom:
          borderCtrl += "b";
          break;

        case BorderType.Left:
          borderCtrl += "l";
          break;

        case BorderType.Right:
          borderCtrl += "r";
          break;

        case BorderType.DiagonalDown:
          Debug.Assert(isCellBorder);
          borderCtrl = "cldglu";
          break;

        case BorderType.DiagonalUp:
          Debug.Assert(isCellBorder);
          borderCtrl = "cldgll";
          break;
      }
      return borderCtrl;
    }

    /// <summary>
    /// Sets the paragraph format the Border is part of.
    /// This property is set by the ParagraphFormatRenderer
    /// </summary>
    internal ParagraphFormat ParentFormat
    {
      set
      {
        this.parentFormat = value;
        if (value != null)
          this.parentCell = null;
      }
    }


    /// <summary>
    /// Sets the cell the border is part of.
    /// This property is set by the CellFormatRenderer
    /// </summary>
    internal Cell ParentCell
    {
      set
      {
        this.parentCell = value;
        if (value != null)
          this.parentFormat = null;
      }
    }
    protected ParagraphFormat parentFormat = null;
    protected Cell parentCell = null;
  }
}
