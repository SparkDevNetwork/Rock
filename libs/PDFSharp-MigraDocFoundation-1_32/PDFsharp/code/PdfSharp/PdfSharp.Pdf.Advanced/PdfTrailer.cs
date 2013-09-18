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
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Represents a PDF trailer dictionary. Even trailers are dictionaries their never have a cross
  /// reference entry in PdfReferenceTable.
  /// </summary>
  internal sealed class PdfTrailer : PdfDictionary
  {
    /// <summary>
    /// Initializes a new instance of PdfTrailer.
    /// </summary>
    public PdfTrailer(PdfDocument document) : base(document)
    {
      this.document = document;
    }

    public int Size
    {
      get {return Elements.GetInteger(Keys.Size);}
      set {Elements.SetInteger(Keys.Size, value);}
    }

    // TODO: needed when linearized...
    //public int Prev
    //{
    //  get {return Elements.GetInteger(Keys.Prev);}
    //}

    public PdfDocumentInformation Info
    {
      get {return (PdfDocumentInformation)Elements.GetValue(Keys.Info, VCF.CreateIndirect);}
    }

    /// <summary>
    /// (Required; must be an indirect reference)
    /// The catalog dictionary for the PDF document contained in the file.
    /// </summary>
    public PdfCatalog Root
    {
      get {return (PdfCatalog)Elements.GetValue(PdfTrailer.Keys.Root, VCF.CreateIndirect);}
    }

    /// <summary>
    /// Gets the first or second document identifier.
    /// </summary>
    public string GetDocumentID(int index)
    {
      if (index < 0 || index > 1)
        throw new ArgumentOutOfRangeException("index", index, "Index must be 0 or 1.");

      PdfArray array = Elements[Keys.ID] as PdfArray;
      if (array == null || array.Elements.Count < 2)
        return "";
      PdfItem item = array.Elements[index];
      if (item is PdfString)
        return ((PdfString)item).Value;
      return "";
    }

    /// <summary>
    /// Sets the first or second document identifier.
    /// </summary>
    public void SetDocumentID(int index, string value)
    {
      if (index < 0 || index > 1)
        throw new ArgumentOutOfRangeException("index", index, "Index must be 0 or 1.");

      PdfArray array = Elements[Keys.ID] as PdfArray;
      if (array == null || array.Elements.Count < 2)
        array = CreateNewDocumentIDs();
      array.Elements[index] = new PdfString(value, PdfStringFlags.HexLiteral);
    }

    /// <summary>
    /// Creates and sets two identical new document IDs.
    /// </summary>
    internal PdfArray CreateNewDocumentIDs()
    {
      PdfArray array = new PdfArray(this.document);
      byte[] docID = Guid.NewGuid().ToByteArray();
      string id = PdfEncoders.RawEncoding.GetString(docID, 0, docID.Length);
      array.Elements.Add(new PdfString(id, PdfStringFlags.HexLiteral));
      array.Elements.Add(new PdfString(id, PdfStringFlags.HexLiteral));
      Elements[Keys.ID] = array;
      return array;
    }

    /// <summary>
    /// Gets the standard security handler.
    /// </summary>
    public PdfStandardSecurityHandler SecurityHandler
    {
      get 
      {
        if (this.securityHandler == null)
          this.securityHandler = (PdfStandardSecurityHandler)Elements.GetValue(Keys.Encrypt, VCF.CreateIndirect);
        return this.securityHandler;
      }
    }
    internal PdfStandardSecurityHandler securityHandler;

    //internal override void WriteDictionaryElement(PdfSharp.Pdf.IO.PdfWriter writer, PdfName key)
    //{
    //  //if (key == Keys.ID)
    //  //{
    //  //  PdfArray array = Elements[key] as PdfArray;
    //  //  PdfItem item = array.Elements[0];
    //  //  //base.WriteDictionaryElement(writer, key);
    //  //  return;
    //  //}
    //  base.WriteDictionaryElement (writer, key);
    //}

    internal override void WriteObject(PdfWriter writer)
    {
      // Delete /XRefStm entry, if any
      this.elements.Remove(Keys.XRefStm);

      // Don't encypt myself
      PdfStandardSecurityHandler securityHandler = writer.SecurityHandler;
      writer.SecurityHandler = null;
      base.WriteObject(writer);
      writer.SecurityHandler = securityHandler;
    }

    /// <summary>
    /// Replace temporary irefs by their correct counterparts from the iref table.
    /// </summary>
    internal void Finish()
    {
      // \Root
      PdfReference iref = document.trailer.Elements[PdfTrailer.Keys.Root] as PdfReference;
      if (iref != null && iref.Value == null)
      {
        iref = document.irefTable[iref.ObjectID];
        Debug.Assert(iref.Value != null);
        this.document.trailer.Elements[PdfTrailer.Keys.Root] = iref;
      }

      // \Info
      iref = this.document.trailer.Elements[PdfTrailer.Keys.Info] as PdfReference;
      if (iref != null && iref.Value == null)
      {
        iref = document.irefTable[iref.ObjectID];
        Debug.Assert(iref.Value != null);
        this.document.trailer.Elements[PdfTrailer.Keys.Info] = iref;
      }

      // \Encrypt
      iref = this.document.trailer.Elements[PdfTrailer.Keys.Encrypt] as PdfReference;
      if (iref != null)
      {
        iref = document.irefTable[iref.ObjectID];
        Debug.Assert(iref.Value != null);
        this.document.trailer.Elements[PdfTrailer.Keys.Encrypt] = iref;

        // The encryption dictionary (security handler) was read in before the XRefTable construction 
        // was completed. The next lines fix that state (it take several hours to find that bugs...).
        iref.Value = this.document.trailer.securityHandler;
        this.document.trailer.securityHandler.Reference = iref;
        iref.Value.Reference = iref;
      }

      Elements.Remove(Keys.Prev);

      this.document.irefTable.IsUnderConstruction = false;
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    internal sealed class Keys : KeysBase
    {
      /// <summary>
      /// (Required; must not be an indirect reference) The total number of entries in the file’s 
      /// cross-reference table, as defined by the combination of the original section and all
      /// update sections. Equivalently, this value is 1 greater than the highest object number
      /// used in the file.
      /// Note: Any object in a cross-reference section whose number is greater than this value is
      /// ignored and considered missing.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Required)]
      public const string Size = "/Size";

      /// <summary>
      /// (Present only if the file has more than one cross-reference section; must not be an indirect
      /// reference) The byte offset from the beginning of the file to the beginning of the previous 
      /// cross-reference section.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Optional)]
      public const string Prev = "/Prev";

      /// <summary>
      /// (Required; must be an indirect reference) The catalog dictionary for the PDF document
      /// contained in the file.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required, typeof(PdfCatalog))]
      public const string Root = "/Root";

      /// <summary>
      /// (Required if document is encrypted; PDF 1.1) The document’s encryption dictionary.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfStandardSecurityHandler))]
      public const string Encrypt = "/Encrypt";

      /// <summary>
      /// (Optional; must be an indirect reference) The document’s information dictionary.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfDocumentInformation))]
      public const string Info = "/Info";

      /// <summary>
      /// (Optional, but strongly recommended; PDF 1.1) An array of two strings constituting
      /// a file identifier for the file. Although this entry is optional, 
      /// its absence might prevent the file from functioning in some workflows
      /// that depend on files being uniquely identified.
      /// </summary>
      [KeyInfo(KeyType.Array | KeyType.Optional)]
      public const string ID = "/ID";

      /// <summary>
      /// (Optional) The byte offset from the beginning of the file of a cross-reference stream.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Optional)]
      public const string XRefStm = "/XRefStm";

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
      get {return Keys.Meta;}
    }
  }
}
