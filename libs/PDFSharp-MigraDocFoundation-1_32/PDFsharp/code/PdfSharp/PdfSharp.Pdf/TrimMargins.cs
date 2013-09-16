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
using PdfSharp.Drawing;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents trim margins added to the page.
  /// </summary>
  [DebuggerDisplay("(Left={left.Millimeter}mm, Right={right.Millimeter}mm, Top={top.Millimeter}mm, Bottom={bottom.Millimeter}mm)")]
  public sealed class TrimMargins
  {
    ///// <summary>
    ///// Clones this instance.
    ///// </summary>
    //public TrimMargins Clone()
    //{
    //  TrimMargins trimMargins = new TrimMargins();
    //  trimMargins.left = this.left;
    //  trimMargins.top = this.top;
    //  trimMargins.right = this.right;
    //  trimMargins.bottom = this.bottom;
    //  return trimMargins;
    //}

    /// <summary>
    /// Sets all four crop margins simultaneously.
    /// </summary>
    public XUnit All
    {
      set
      {
        this.left = value;
        this.right = value;
        this.top = value;
        this.bottom = value;
      }
    }

    /// <summary>
    /// Gets or sets the left crop margin.
    /// </summary>
    public XUnit Left
    {
      get { return this.left; }
      set { this.left = value; }
    }
    XUnit left;

    /// <summary>
    /// Gets or sets the right crop margin.
    /// </summary>
    public XUnit Right
    {
      get { return this.right; }
      set { this.right = value; }
    }
    XUnit right;

    /// <summary>
    /// Gets or sets the top crop margin.
    /// </summary>
    public XUnit Top
    {
      get { return this.top; }
      set { this.top = value; }
    }
    XUnit top;

    /// <summary>
    /// Gets or sets the bottom crop margin.
    /// </summary>
    public XUnit Bottom
    {
      get { return this.bottom; }
      set { this.bottom = value; }
    }
    XUnit bottom;

    /// <summary>
    /// Gets a value indicating whether this instance has at least one margin with a value other than zero.
    /// </summary>
    public bool AreSet
    {
      get { return this.left.Value != 0 || this.right.Value != 0 || this.top.Value != 0 || this.bottom.Value != 0; }
    }
  }
}