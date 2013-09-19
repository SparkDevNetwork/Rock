#region PDFsharp Ghostscript - A .NET wrapper of Ghostscript
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
using System.Drawing;
using System.Windows.Forms;
using PdfSharp.Ghostscript;

namespace UseGhostscript
{
  /// <summary>
  /// This sample has absolutely nothing to to with PDFsharp. It just shows how you can invoke Ghostscript
  /// from within a C# program.
  /// It shows how to create an PNG image from a PDF file. The code requires that you install gsdll32.dll
  /// on your computer. To get information about Ghostscript you can start from here http://www.ghostscript.com.
  /// Please pay attention to the copyright and licence of Ghostscript. It is different from the conditions
  /// of PDFsharp.
  /// </summary>
  class Program
  {
    [STAThread]
    static void Main()
    {
      string filename = @"..\..\..\..\PDFs\SomeLayout.pdf";

      // Create instance of Ghostscript wrapper class.
      GS gs = new GS();

      // Create image from PDF file (again: this has _nothing_ to do with PDFsharp)
      Image image = gs.PdfToPng(filename, 1, 96);

      // Show image in a PictureBox control.
      MainForm form = new MainForm();
      form.pictureBox.Image = image;

      Application.Run(form);
    }
  }
}