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
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Provides access to the internal data structures of XGraphicsPath.
  /// This class prevents the public interface from pollution with internal functions.
  /// </summary>
  public sealed class XGraphicsPathInternals
  {
    internal XGraphicsPathInternals(XGraphicsPath path)
    {
      this.path = path;
    }
    XGraphicsPath path;

#if GDI
    /// <summary>
    /// Gets the underlying GDI+ path object.
    /// </summary>
    public GraphicsPath GdiPath
    {
      get { return this.path.gdipPath; }
    }
#endif

#if WPF
    /// <summary>
    /// Gets the underlying WPF path geometry object.
    /// </summary>
    public PathGeometry WpfPath
    {
      get { return this.path.pathGeometry; }
    }
#endif
  }
}