#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
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
using System.Collections.Generic;
using System.IO;
using System.Windows;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace ExportImages
{
  /// <summary>
  /// This sample shows how to export JPEG images from a PDF file.
  /// </summary>
  class Program
  {
    static void Main()
    {
      const string filename = "../../../../../PDFs/SomeLayout.pdf";

      PdfDocument document = PdfReader.Open(filename);

      int imageCount = 0;
      // Iterate pages
      foreach (PdfPage page in document.Pages)
      {
        // Get resources dictionary
        PdfDictionary resources = page.Elements.GetDictionary("/Resources");
        if (resources != null)
        {
          // Get external objects dictionary
          PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
          if (xObjects != null)
          {
            ICollection<PdfItem> items = xObjects.Elements.Values;
            // Iterate references to external objects
            foreach (PdfItem item in items)
            {
              PdfReference reference = item as PdfReference;
              if (reference != null)
              {
                PdfDictionary xObject = reference.Value as PdfDictionary;
                // Is external object an image?
                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                {
                  ExportImage(xObject, ref imageCount);
                }
              }
            }
          }
        }
      }
#if GDI
      System.Windows.Forms. // OK, this is a HACK
#endif
      MessageBox.Show(imageCount + " images exported.", "Export Images");
    }

    /// <summary>
    /// Currently extracts only JPEG images.
    /// </summary>
    static void ExportImage(PdfDictionary image, ref int count)
    {
      string filter = image.Elements.GetName("/Filter");
      switch (filter)
      {
        case "/DCTDecode":
          ExportJpegImage(image, ref count);
          break;

        case "/FlateDecode":
          ExportAsPngImage(image, ref count);
          break;
      }
    }

    /// <summary>
    /// Exports a JPEG image.
    /// </summary>
    static void ExportJpegImage(PdfDictionary image, ref int count)
    {
      // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
      byte[] stream = image.Stream.Value;
      FileStream fs = new FileStream(String.Format("Image{0}.jpeg", count++), FileMode.Create, FileAccess.Write);
      BinaryWriter bw = new BinaryWriter(fs);
      bw.Write(stream);
      bw.Close();
    }

    /// <summary>
    /// Exports image in PNF format.
    /// </summary>
    static void ExportAsPngImage(PdfDictionary image, ref int count)
    {
      int width = image.Elements.GetInteger(PdfImage.Keys.Width);
      int height = image.Elements.GetInteger(PdfImage.Keys.Height);
      int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

      // TODO: You can put the code here that converts vom PDF internal image format to a Windows bitmap
      // and use GDI+ to save it in PNG format.
      // It is the work of a day or two for the most important formats. Take a look at the file
      // PdfSharp.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
      // We don't need that feature at the moment and therefore will not implement it.
      // If you write the code for exporting images I would be pleased to publish it in a future release
      // of PDFsharp.
    }
  }
}