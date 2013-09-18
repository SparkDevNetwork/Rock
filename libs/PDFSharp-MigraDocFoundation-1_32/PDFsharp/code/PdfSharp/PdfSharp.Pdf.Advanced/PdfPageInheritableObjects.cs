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
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Represents a PDF page object.
  /// </summary>
  internal class PdfPageInheritableObjects : PdfDictionary
  {
    public PdfPageInheritableObjects()
    {
    }

    // TODO Inheritable Resources not yet supported

    /// <summary>
    /// 
    /// </summary>
    public PdfRectangle MediaBox
    {
      get {return this.mediaBox;}
      set {this.mediaBox = value;}
    }
    PdfRectangle mediaBox;

    public PdfRectangle CropBox
    {
      get {return this.cropBox;}
      set {this.cropBox = value;}
    }
    PdfRectangle cropBox;

    public int Rotate
    {
      get {return this.rotate;}
      set 
      {
        if (value % 90 != 0)
          throw new ArgumentException("Rotate", "The value must be a multiple of 90.");
        this.rotate = value;
      }
    }
    int rotate;
  }
}
