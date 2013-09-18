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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Annotations
{
  /// <summary>
  /// Represents a rubber stamp annotation.
  /// </summary>
  public sealed class PdfRubberStampAnnotation : PdfAnnotation
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfRubberStampAnnotation"/> class.
    /// </summary>
    public PdfRubberStampAnnotation()
    {
      Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfRubberStampAnnotation"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public PdfRubberStampAnnotation(PdfDocument document)
      : base(document)
    {
      Initialize();
    }

    void Initialize()
    {
      Elements.SetName(Keys.Subtype, "/Stamp");
      Color = XColors.Yellow;
    }

    /// <summary>
    /// Gets or sets an icon to be used in displaying the annotation.
    /// </summary>
    public PdfRubberStampAnnotationIcon Icon
    {
      get
      {
        string value = Elements.GetName(Keys.Name);
        if (value == "")
          return PdfRubberStampAnnotationIcon.NoIcon;
        value = value.Substring(1);
        if (!Enum.IsDefined(typeof(PdfRubberStampAnnotationIcon), value))
          return PdfRubberStampAnnotationIcon.NoIcon;
        return (PdfRubberStampAnnotationIcon)Enum.Parse(typeof(PdfRubberStampAnnotationIcon), value, false);
      }
      set
      {
        if (Enum.IsDefined(typeof(PdfRubberStampAnnotationIcon), value) &&
          PdfRubberStampAnnotationIcon.NoIcon != value)
        {
          Elements.SetName(Keys.Name, "/" + value.ToString());
        }
        else
          Elements.Remove(Keys.Name);
      }
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    internal new class Keys : PdfAnnotation.Keys
    {
      /// <summary>
      /// (Optional) The name of an icon to be used in displaying the annotation. Viewer
      /// applications should provide predefined icon appearances for at least the following
      /// standard names:
      ///   Approved
      ///   AsIs
      ///   Confidential
      ///   Departmental
      ///   Draft
      ///   Experimental
      ///   Expired
      ///   Final
      ///   ForComment
      ///   ForPublicRelease
      ///   NotApproved
      ///   NotForPublicRelease
      ///   Sold
      ///   TopSecret
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Optional)]
      public const string Name = "/Name";

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
