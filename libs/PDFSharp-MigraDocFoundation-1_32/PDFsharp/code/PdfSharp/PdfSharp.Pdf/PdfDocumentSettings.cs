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
using System.Collections;
using PdfSharp.Drawing;
using PdfSharp.Internal;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Holds PDF specific information of the document.
  /// </summary>
  public sealed class PdfDocumentSettings
  {
    internal PdfDocumentSettings(PdfDocument document)
    {
    }

    /// <summary>
    /// Sets the private font collection.
    /// </summary>
    public XPrivateFontCollection PrivateFontCollection
    {
      internal get { return this.privateFontCollection; }
      set
      {
        if (this.privateFontCollection != null)
          throw new InvalidOperationException("PrivateFontCollection can only be set once.");

        this.privateFontCollection = value;
      }
    }
    private XPrivateFontCollection privateFontCollection;


    /// <summary>
    /// Gets or sets the default trim margins.
    /// </summary>
    public TrimMargins TrimMargins
    {
      get
      {
        if (this.trimMargins == null)
          this.trimMargins = new TrimMargins();
        return this.trimMargins;
      }
      set
      {
        if (this.trimMargins == null)
          this.trimMargins = new TrimMargins();
        if (value != null)
        {
          this.trimMargins.Left = value.Left;
          this.trimMargins.Right = value.Right;
          this.trimMargins.Top = value.Top;
          this.trimMargins.Bottom = value.Bottom;
        }
        else
          this.trimMargins.All = 0;
      }
    }
    TrimMargins trimMargins = new TrimMargins();
  }
}