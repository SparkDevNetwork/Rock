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
using System.Globalization;
using System.Diagnostics;
using System.Collections;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Annotations
{
  /// <summary>
  /// Represents a link annotation.
  /// </summary>
  public sealed class PdfLinkAnnotation : PdfAnnotation
  {
    // Just a hack to make MigraDoc work with this code.
    enum LinkType
    {
      Document, Web, File
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
    /// </summary>
    public PdfLinkAnnotation()
    {
      Elements.SetName(Keys.Subtype, "/Link");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfLinkAnnotation"/> class.
    /// </summary>
    public PdfLinkAnnotation(PdfDocument document)
      : base(document)
    {
      Elements.SetName(Keys.Subtype, "/Link");
    }

    /// <summary>
    /// Creates a link within the current document.
    /// </summary>
    /// <param name="rect">The link area in default page coordinates.</param>
    /// <param name="destinationPage">The one-based destination page number.</param>
    public static PdfLinkAnnotation CreateDocumentLink(PdfRectangle rect, int destinationPage)
    {
      PdfLinkAnnotation link = new PdfLinkAnnotation();
      link.linkType = PdfLinkAnnotation.LinkType.Document;
      link.Rectangle = rect;
      link.destPage = destinationPage;
      return link;
    }
    int destPage;
    LinkType linkType;
    string url;

    /// <summary>
    /// Creates a link to the web.
    /// </summary>
    public static PdfLinkAnnotation CreateWebLink(PdfRectangle rect, string url)
    {
      PdfLinkAnnotation link = new PdfLinkAnnotation();
      link.linkType = PdfLinkAnnotation.LinkType.Web;
      link.Rectangle = rect;
      link.url = url;
      return link;
    }

    /// <summary>
    /// Creates a link to a file.
    /// </summary>
    public static PdfLinkAnnotation CreateFileLink(PdfRectangle rect, string fileName)
    {
      PdfLinkAnnotation link = new PdfLinkAnnotation();
      link.linkType = PdfLinkAnnotation.LinkType.File;
      // TODO: Adjust bleed box here (if possible)
      link.Rectangle = rect;
      link.url = fileName;
      return link;
    }

    internal override void WriteObject(PdfWriter writer)
    {
      PdfPage dest = null;
      //pdf.AppendFormat(CultureInfo.InvariantCulture,
      //  "{0} 0 obj\n<<\n/Type/Annot\n/Subtype/Link\n" +
      //  "/Rect[{1} {2} {3} {4}]\n/BS<</Type/Border>>\n/Border[0 0 0]\n/C[0 0 0]\n",
      //  this.ObjectID.ObjectNumber, rect.X1, rect.Y1, rect.X2, rect.Y2);

      if (Elements[Keys.BS] == null)
        Elements[Keys.BS] = new PdfLiteral("<</Type/Border>>");

      if (Elements[Keys.Border] == null)
        Elements[Keys.Border] = new PdfLiteral("[0 0 0]");

      switch (this.linkType)
      {
        case LinkType.Document:
          // destIndex > Owner.PageCount can happen rendering pages using PDFsharp directly
          int destIndex = this.destPage;
          if (destIndex > Owner.PageCount)
            destIndex = Owner.PageCount;
          destIndex--;
          dest = this.Owner.Pages[destIndex];
          //pdf.AppendFormat("/Dest[{0} 0 R/XYZ null null 0]\n", dest.ObjectID);
          Elements[Keys.Dest] = new PdfLiteral("[{0} 0 R/XYZ null null 0]", dest.ObjectNumber);
          break;

        case LinkType.Web:
          //pdf.AppendFormat("/A<</S/URI/URI{0}>>\n", PdfEncoders.EncodeAsLiteral(this.url));
          Elements[Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>", //PdfEncoders.EncodeAsLiteral(this.url));
            PdfEncoders.ToStringLiteral(this.url, PdfStringEncoding.WinAnsiEncoding, writer.SecurityHandler));
          break;

        case LinkType.File:
          //pdf.AppendFormat("/A<</Type/Action/S/Launch/F<</Type/Filespec/F{0}>> >>\n", 
          //  PdfEncoders.EncodeAsLiteral(this.url));
          Elements[Keys.A] = new PdfLiteral("<</Type/Action/S/Launch/F<</Type/Filespec/F{0}>> >>",
            //PdfEncoders.EncodeAsLiteral(this.url));
            PdfEncoders.ToStringLiteral(this.url, PdfStringEncoding.WinAnsiEncoding, writer.SecurityHandler));
          break;
      }
      base.WriteObject(writer);
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    internal new class Keys : PdfAnnotation.Keys
    {
      //  /// <summary>
      //  /// (Required) The type of annotation that this dictionary describes;
      //  /// must be Link for a link annotation.
      //  /// </summary>
      // inherited from base class

      /// <summary>
      /// (Optional; not permitted if an A entry is present) A destination to be displayed
      /// when the annotation is activated.
      /// </summary>
      [KeyInfo(KeyType.ArrayOrNameOrString | KeyType.Optional)]
      public const string Dest = "/Dest";

      /// <summary>
      /// (Optional; PDF 1.2) The annotation’s highlighting mode, the visual effect to be
      /// used when the mouse button is pressed or held down inside its active area:
      /// N (None) No highlighting.
      /// I (Invert) Invert the contents of the annotation rectangle.
      /// O (Outline) Invert the annotation’s border.
      /// P (Push) Display the annotation as if it were being pushed below the surface of the page.
      /// Default value: I.
      /// Note: In PDF 1.1, highlighting is always done by inverting colors inside the annotation rectangle.
      /// </summary>
      [KeyInfo("1.2", KeyType.Name | KeyType.Optional)]
      public const string H = "/H";

      /// <summary>
      /// (Optional; PDF 1.3) A URI action formerly associated with this annotation. When Web 
      /// Capture changes and annotation from a URI to a go-to action, it uses this entry to save 
      /// the data from the original URI action so that it can be changed back in case the target page for 
      /// the go-to action is subsequently deleted.
      /// </summary>
      [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional)]
      public const string PA = "/PA";

      // QuadPoints

      /// <summary>
      /// Gets the KeysMeta for these keys.
      /// </summary>
      public static DictionaryMeta Meta
      {
        get
        {
          if (Keys.meta == null)
            Keys.meta = CreateMeta(typeof(Keys));
          return Keys.meta;
        }
      }
      static DictionaryMeta meta;
    }

    /// <summary>
    /// Gets the KeysMeta of this dictionary type.
    /// </summary>
    internal override DictionaryMeta Meta
    {
      get { return Keys.Meta; }
    }
  }
}
