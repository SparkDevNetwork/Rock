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
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms
{
  /// <summary>
  /// Represents the check box field.
  /// </summary>
  public sealed class PdfCheckBoxField : PdfButtonField
  {
    /// <summary>
    /// Initializes a new instance of PdfCheckBoxField.
    /// </summary>
    internal PdfCheckBoxField(PdfDocument document) : base(document)
    {
      this.document = document;
    }

    internal PdfCheckBoxField(PdfDictionary dict) : base(dict)
    {
    }

    /// <summary>
    /// Indicates whether the field is checked.
    /// </summary>
    public bool Checked
    { 
      get
      {
        string value = Elements.GetString(Keys.V);
        return value.Length != 0 && value != "/Off";
      }
      set 
      {
        string name = value ? GetNonOffValue() : "/Off";
        Elements.SetName(Keys.V, name);
        Elements.SetName(PdfAnnotation.Keys.AS, name);
      }
    }

    /// <summary>
    /// Predefined keys of this dictionary. 
    /// The description comes from PDF 1.4 Reference.
    /// </summary>
    public new class Keys : PdfButtonField.Keys
    {
      /// <summary>
      /// (Optional; inheritable; PDF 1.4) A text string to be used in place of the V entry for the
      /// value of the field.
      /// </summary>
      [KeyInfo(KeyType.TextString | KeyType.Optional)]
      public const string Opt = "/Opt";
  
      /// <summary>
      /// Gets the KeysMeta for these keys.
      /// </summary>
      internal static DictionaryMeta Meta
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
      get {return Keys.Meta;}
    }
  }
}
