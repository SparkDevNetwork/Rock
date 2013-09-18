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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Pdf;
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;

namespace PdfSharp.Fonts.OpenType
{
  /// <summary>
  /// PDFsharp internal stuff.
  /// For more information see Andrew Schulman "Undocumented PDFsharp"  :-))
  /// </summary>
  public static class ExternalHelper
  {
    /// <summary>
    /// This is an external helper function.
    /// </summary>
    public static byte[] F74167FFE4044F53B28A4AF049E9EF25(XFont font, XPdfFontOptions options, bool subset)
    {
      byte[] data = null;
      if (subset)
      {
        OpenTypeDescriptor descriptor = new OpenTypeDescriptor(font, options);
        FontData image = descriptor.fontData;
        CMapInfo cmapInfo = new CMapInfo(descriptor);
        cmapInfo.AddAnsiChars();
        image = image.CreateFontSubSet(cmapInfo.GlyphIndices, false);
        data = image.Data;
      }
      else
      {
        FontData fontData = new FontData(font, options);
        data = fontData.Data;
      }
      return data;
    }
  }
}