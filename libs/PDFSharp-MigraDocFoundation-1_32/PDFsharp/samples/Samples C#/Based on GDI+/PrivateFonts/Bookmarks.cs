using System;
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PrivateFonts
{
  /// <summary>
  /// This sample is the obligatory Hello World program.
  /// </summary>
  class Test
  {
    public static void Xxx()
    {
      PdfDocument document = PdfReader.Open(@"G:\!StLa\PDFsharp Bugs\09-04-23 Bookmark\Seite1_KK_Recht.pdf");

      var objects = document.Internals.GetAllObjects();
      PdfDictionary dict = objects[1] as PdfDictionary;
      string s = ((PdfString)dict.Elements["/Title"]).ToStringFromPdfDocEncoded();

    }
  }
}