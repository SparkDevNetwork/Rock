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
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Represents predefined text layouts.
  /// </summary>
  public static class XStringFormats
  {
    /// <summary>
    /// Gets a new XStringFormat object that aligns the text left on the base line.
    /// </summary>
    public static XStringFormat Default
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.LineAlignment = XLineAlignment.BaseLine;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that aligns the text top left of the layout rectangle.
    /// </summary>
    public static XStringFormat TopLeft
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Near;
        format.LineAlignment = XLineAlignment.Near;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text in the middle of the layout rectangle.
    /// </summary>
    public static XStringFormat Center
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Center;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text at the top of the layout rectangle.
    /// </summary>
    public static XStringFormat TopCenter
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Near;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text at the bottom of the layout rectangle.
    /// </summary>
    public static XStringFormat BottomCenter
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Far;
        return format;
      }
    }
  }
}