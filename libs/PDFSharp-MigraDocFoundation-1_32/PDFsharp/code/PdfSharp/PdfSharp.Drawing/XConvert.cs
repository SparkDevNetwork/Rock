#region PDFsharp - A .NET library for processing PDF
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
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Converts XGraphics enums to GDI+ enums.
  /// </summary>
  static class XConvert
  {
#if GDI
    /// <summary>
    /// Converts XLineJoin to LineJoin.
    /// </summary>
    public static LineJoin ToLineJoin(XLineJoin lineJoin)
    {
      return gdiLineJoin[(int)lineJoin];
    }
    static LineJoin[] gdiLineJoin = new LineJoin[3]{LineJoin.Miter, LineJoin.Round, LineJoin.Bevel};
#endif

#if GDI
    /// <summary>
    /// Converts XLineCap to LineCap.
    /// </summary>
    public static LineCap ToLineCap(XLineCap lineCap)
    {
      return gdiLineCap[(int)lineCap];
    }
    static LineCap[] gdiLineCap = new LineCap[3]{LineCap.Flat, LineCap.Round, LineCap.Square};
#endif

#if WPF
    /// <summary>
    /// Converts XLineJoin to PenLineJoin.
    /// </summary>
    public static PenLineJoin ToPenLineJoin(XLineJoin lineJoin)
    {
      return wpfLineJoin[(int)lineJoin];
    }
    static readonly PenLineJoin[] wpfLineJoin = new PenLineJoin[] { PenLineJoin.Miter, PenLineJoin.Round, PenLineJoin.Bevel };
#endif

#if WPF
    /// <summary>
    /// Converts XLineCap to PenLineCap.
    /// </summary>
    public static PenLineCap ToPenLineCap(XLineCap lineCap)
    {
      return wpfLineCap[(int)lineCap];
    }
    static readonly PenLineCap[] wpfLineCap = new PenLineCap[] { PenLineCap.Flat, PenLineCap.Round, PenLineCap.Square };
#endif
  }
}