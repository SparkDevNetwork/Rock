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
using System.Reflection;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Security;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents a PDF document.
  /// </summary>
  [DebuggerDisplay("(Name={Name})")] // A name makes debugging easier
  public sealed class PdfDocument : PdfObject, IDisposable
  {
    internal DocumentState state;
    internal PdfDocumentOpenMode openMode;

#if DEBUG_
    static PdfDocument()
    {
      PSSR.TestResourceMessages();
      //string test = PSSR.ResMngr.GetString("SampleMessage1");
      //test.GetType();
    }
#endif

    /// <summary>
    /// Creates a new PDF document in memory.
    /// To open an existing PDF file, use the PdfReader class.
    /// </summary>
    public PdfDocument()
    {
      //PdfDocument.Gob.AttatchDocument(this.Handle);

      this.creation = DateTime.Now;
      this.state = DocumentState.Created;
      this.version = 14;
      Initialize();
      Info.CreationDate = this.creation;
    }

    /// <summary>
    /// Creates a new PDF document with the specified file name. The file is immediately created and keeps
    /// looked until the document is saved.
    /// To open an existing PDF file and import it, use the PdfReader class.
    /// </summary>
    public PdfDocument(string filename)
    {
      //PdfDocument.Gob.AttatchDocument(this.Handle);

      this.creation = DateTime.Now;
      this.state = DocumentState.Created;
      this.version = 14;
      Initialize();
      Info.CreationDate = this.creation;

      this.outStream = new FileStream(filename, FileMode.Create);
    }

    /// <summary>
    /// Creates a new PDF document using the specified stream.
    /// To open an existing PDF file, use the PdfReader class.
    /// </summary>
    public PdfDocument(Stream outputStream)
    {
      //PdfDocument.Gob.AttatchDocument(this.Handle);

      this.creation = DateTime.Now;
      this.state = DocumentState.Created;
      Initialize();
      Info.CreationDate = this.creation;

      this.outStream = outputStream;
    }

    internal PdfDocument(Lexer lexer)
    {
      //PdfDocument.Gob.AttatchDocument(this.Handle);

      this.creation = DateTime.Now;
      this.state = DocumentState.Imported;

      //this.info = new PdfInfo(this);
      //this.pages = new PdfPages(this);
      //this.fontTable = new PdfFontTable();
      //this.catalog = new PdfCatalog(this);
      ////this.font = new PdfFont();
      //this.objects = new PdfObjectTable(this);
      //this.trailer = new PdfTrailer(this);
      this.irefTable = new PdfReferenceTable(this);
      this.lexer = lexer;
    }

    void Initialize()
    {
      //this.info       = new PdfInfo(this);
      this.fontTable = new PdfFontTable(this);
      this.imageTable = new PdfImageTable(this);
      this.trailer = new PdfTrailer(this);
      this.irefTable = new PdfReferenceTable(this);
      this.trailer.CreateNewDocumentIDs();
    }

    //~PdfDocument()
    //{
    //  Dispose(false);
    //}

    /// <summary>
    /// Disposes all references to this document stored in other documents. This function should be called
    /// for documents you finished importing pages from. Calling Dispose is technically not necessary but
    /// useful for earlier reclaiming memory of documents you do not need anymore.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      //GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing)
    {
      if (this.state != DocumentState.Disposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
        }
        //PdfDocument.Gob.DetatchDocument(Handle);
      }
      this.state = DocumentState.Disposed;
    }

    /// <summary>
    /// Gets or sets a user defined object that contains arbitrary information associated with this document.
    /// The tag is not used by PDFsharp.
    /// </summary>
    public object Tag
    {
      get { return this.tag; }
      set { this.tag = value; }
    }
    object tag;

    /// <summary>
    /// Gets or sets a value used to distinguish PdfDocument objects.
    /// The name is not used by PDFsharp.
    /// </summary>
    string Name
    {
      get { return this.name; }
      set { this.name = value; }
    }
    string name = NewName();

    /// <summary>
    /// Get a new default name for a new document.
    /// </summary>
    static string NewName()
    {
#if DEBUG_
      if (PdfDocument.nameCount == 57)
        PdfDocument.nameCount.GetType();
#endif
      return "Document " + nameCount++;
    }
    static int nameCount;

    internal bool CanModify
    {
      //get {return this.state == DocumentState.Created || this.state == DocumentState.Modifyable;}
      // THHO4STLA: TODO: correct implementation
      get { return openMode == PdfDocumentOpenMode.Modify; } // TODO: correct implementation
    }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public void Close()
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);

      if (this.outStream != null)
      {
        // Get security handler if document gets encrypted
        PdfStandardSecurityHandler securityHandler = null;
        if (SecuritySettings.DocumentSecurityLevel != PdfDocumentSecurityLevel.None)
          securityHandler = SecuritySettings.SecurityHandler;

        PdfWriter writer = new PdfWriter(this.outStream, securityHandler);
        try
        {
          DoSave(writer);
        }
        finally
        {
          writer.Close();
        }
      }
    }

    /// <summary>
    /// Saves the document to the specified path. If a file already exists, it will be overwritten.
    /// </summary>
    public void Save(string path)
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);

      Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
      Save(stream);
    }

    /// <summary>
    /// Saves the document to the specified stream.
    /// </summary>
    public void Save(Stream stream, bool closeStream)
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);

      // TODO: more diagnostic checks
      string message = "";
      if (!CanSave(ref message))
        throw new PdfSharpException(message);

      // Get security handler if document gets encrypted
      PdfStandardSecurityHandler securityHandler = null;
      if (SecuritySettings.DocumentSecurityLevel != PdfDocumentSecurityLevel.None)
        securityHandler = SecuritySettings.SecurityHandler;

      PdfWriter writer = new PdfWriter(stream, securityHandler);
      try
      {
        DoSave(writer);
      }
      finally
      {
        if (stream != null)
        {
          if (closeStream)
            stream.Close();
          else
            stream.Position = 0; // Reset the stream position if the stream is left open.
        }
        if (writer != null)
          writer.Close(closeStream);
      }
    }

    /// <summary>
    /// Saves the document to the specified stream and closes the stream.
    /// </summary>
    public void Save(Stream stream)
    {
      Save(stream, true);
    }

    /// <summary>
    /// Implements saving a PDF file.
    /// </summary>
    void DoSave(PdfWriter writer)
    {
      if (this.pages == null || this.pages.Count == 0)
        throw new InvalidOperationException("Cannot save a PDF document with no pages.");

      try
      {
        bool encrypt = this.securitySettings.DocumentSecurityLevel != PdfDocumentSecurityLevel.None;
        if (encrypt)
        {
          PdfStandardSecurityHandler securityHandler = this.securitySettings.SecurityHandler;
          if (securityHandler.Reference == null)
            this.irefTable.Add(securityHandler);
          else
            Debug.Assert(this.irefTable.Contains(securityHandler.ObjectID));
          this.trailer.Elements[PdfTrailer.Keys.Encrypt] = this.securitySettings.SecurityHandler.Reference;
        }
        else
          this.trailer.Elements.Remove(PdfTrailer.Keys.Encrypt);

        PrepareForSave();

        if (encrypt)
          this.securitySettings.SecurityHandler.PrepareEncryption();

        writer.WriteFileHeader(this);
        PdfReference[] irefs = this.irefTable.AllReferences;
        int count = irefs.Length;
        for (int idx = 0; idx < count; idx++)
        {
          PdfReference iref = irefs[idx];
#if DEBUG_
          if (iref.ObjectNumber == 378)
            GetType();
#endif
          iref.Position = writer.Position;
          iref.Value.WriteObject(writer);
        }
        int startxref = writer.Position;
        this.irefTable.WriteObject(writer);
        writer.WriteRaw("trailer\n");
        this.trailer.Elements.SetInteger("/Size", count + 1);
        this.trailer.WriteObject(writer);
        writer.WriteEof(this, startxref);

        //if (encrypt)
        //{
        //  this.state &= ~DocumentState.SavingEncrypted;
        //  //this.securitySettings.SecurityHandler.EncryptDocument();
        //}
      }
      finally
      {
        if (writer != null)
        {
          writer.Stream.Flush();
          // DO NOT CLOSE WRITER HERE
          //writer.Close();
        }
      }
    }

    /// <summary>
    /// Dispatches PrepareForSave to the objects that need it.
    /// </summary>
    internal override void PrepareForSave()
    {
      PdfDocumentInformation info = Info;

      // Set Creator if value is undefined
      if (info.Elements[PdfDocumentInformation.Keys.Creator] == null)
        info.Creator = VersionInfo.Producer;

      // Keep original producer if file was imported
      string producer = info.Producer;
      if (producer.Length == 0)
        producer = VersionInfo.Producer;
      else
      {
        // Prevent endless concatenation if file is edited with PDFsharp more than once
        if (!producer.StartsWith(VersionInfo.Title))
          producer = VersionInfo.Producer + " (Original: " + producer + ")";
      }
      info.Elements.SetString(PdfDocumentInformation.Keys.Producer, producer);

      // Prepare used fonts
      if (this.fontTable != null)
        this.fontTable.PrepareForSave();

      // Let catalog do the rest
      Catalog.PrepareForSave();

#if true
      // Remove all unreachable objects (e.g. from deleted pages)
      int removed = this.irefTable.Compact();
      if (removed != 0)
        Debug.WriteLine("PrepareForSave: Number of deleted unreachable objects: " + removed);
      this.irefTable.Renumber();
#endif
    }

    /// <summary>
    /// Determines whether the document can be saved.
    /// </summary>
    public bool CanSave(ref string message)
    {
      if (!SecuritySettings.CanSave(ref message))
        return false;

      return true;
    }

    internal bool HasVersion(string version)
    {
      return String.Compare(Catalog.Version, version) >= 0;
    }

    /// <summary>
    /// Gets the document options used for saving the document.
    /// </summary>
    public PdfDocumentOptions Options
    {
      get
      {
        if (this.options == null)
          this.options = new PdfDocumentOptions(this);
        return this.options;
      }
    }
    PdfDocumentOptions options;

    /// <summary>
    /// Gets PDF specific document settings.
    /// </summary>
    public PdfDocumentSettings Settings
    {
      get
      {
        if (this.settings == null)
          this.settings = new PdfDocumentSettings(this);
        return this.settings;
      }
    }
    PdfDocumentSettings settings;

    /// <summary>
    /// NYI Indicates whether large objects are written immediately to the output stream to relieve
    /// memory consumption.
    /// </summary>
    internal bool EarlyWrite
    {
      get { return false; }
    }

    /// <summary>
    /// Gets or sets the PDF version number. Return value 14 e.g. means PDF 1.4 / Acrobat 5 etc.
    /// </summary>
    public int Version
    {
      get { return this.version; }
      set
      {
        if (!CanModify)
          throw new InvalidOperationException(PSSR.CannotModify);
        if (value < 12 || value > 17) // TODO not really implemented
          throw new ArgumentException(PSSR.InvalidVersionNumber, "value");
        this.version = value;
      }
    }
    internal int version;

    /// <summary>
    /// Gets the number of pages in the document.
    /// </summary>
    public int PageCount
    {
      get
      {
        if (CanModify)
          return Pages.Count;
        // PdfOpenMode is InformationOnly
        PdfDictionary pageTreeRoot = (PdfDictionary)Catalog.Elements.GetObject(PdfCatalog.Keys.Pages);
        return pageTreeRoot.Elements.GetInteger(PdfPages.Keys.Count);
      }
    }

    /// <summary>
    /// Gets the file size of the document.
    /// </summary>
    public long FileSize
    {
      get { return this.fileSize; }
    }
    internal long fileSize;

    /// <summary>
    /// Gets the full qualified file name if the document was read form a file, or an empty string otherwise.
    /// </summary>
    public string FullPath
    {
      get { return this.fullPath; }
    }
    internal string fullPath = String.Empty;

    /// <summary>
    /// Gets a Guid that uniquely identifies this instance of PdfDocument.
    /// </summary>
    public Guid Guid
    {
      get { return this.guid; }
    }
    Guid guid = Guid.NewGuid();

    internal DocumentHandle Handle
    {
      get
      {
        if (this.handle == null)
          this.handle = new DocumentHandle(this);
        return this.handle;
      }
    }
    DocumentHandle handle;

    /// <summary>
    /// Returns a value indicating whether the document was newly created or opened from an existing document.
    /// Returns true if the document was opened with the PdfReader.Open function, false otherwise.
    /// </summary>
    public bool IsImported
    {
      get { return (this.state & DocumentState.Imported) != 0; }
    }

    /// <summary>
    /// Returns a value indicating whether the document is read only or can be modified.
    /// </summary>
    public bool IsReadOnly
    {
      get { return (this.openMode != PdfDocumentOpenMode.Modify); }
    }

    internal Exception DocumentNotImported()
    {
      return new InvalidOperationException("Document not imported.");
    }

    /// <summary>
    /// Gets information about the document.
    /// </summary>
    public PdfDocumentInformation Info
    {
      get
      {
        if (this.info == null)
          this.info = this.trailer.Info;
        return this.info;
      }
    }
    PdfDocumentInformation info;  // never changes if once created

    /// <summary>
    /// This function is intended to be undocumented.
    /// </summary>
    public PdfCustomValues CustomValues
    {
      get
      {
        if (this.customValues == null)
          this.customValues = PdfCustomValues.Get(Catalog.Elements);
        return this.customValues;
      }
      set
      {
        if (value != null)
          throw new ArgumentException("Only null is allowed to clear all custom values.");
        PdfCustomValues.Remove(Catalog.Elements);
        this.customValues = null;
      }
    }
    PdfCustomValues customValues;

    /// <summary>
    /// Get the pages dictionary.
    /// </summary>
    public PdfPages Pages
    {
      get
      {
        if (this.pages == null)
          this.pages = Catalog.Pages;
        return this.pages;
      }
    }
    PdfPages pages;  // never changes if once created

    /// <summary>
    /// Gets or sets a value specifying the page layout to be used when the document is opened.
    /// </summary>
    public PdfPageLayout PageLayout
    {
      get { return Catalog.PageLayout; }
      set
      {
        if (!CanModify)
          throw new InvalidOperationException(PSSR.CannotModify);
        Catalog.PageLayout = value;
      }
    }

    /// <summary>
    /// Gets or sets a value specifying how the document should be displayed when opened.
    /// </summary>
    public PdfPageMode PageMode
    {
      get { return Catalog.PageMode; }
      set
      {
        if (!CanModify)
          throw new InvalidOperationException(PSSR.CannotModify);
        Catalog.PageMode = value;
      }
    }

    /// <summary>
    /// Gets the viewer preferences of this document.
    /// </summary>
    public PdfViewerPreferences ViewerPreferences
    {
      get { return Catalog.ViewerPreferences; }
    }

    /// <summary>
    /// Gets the root of the outline (or bookmark) tree.
    /// </summary>
    public PdfOutline.PdfOutlineCollection Outlines
    {
      get { return Catalog.Outlines; }
    }

    /// <summary>
    /// Get the AcroForm dictionary.
    /// </summary>
    public PdfAcroForm AcroForm
    {
      get { return Catalog.AcroForm; }
    }

    /// <summary>
    /// Gets or sets the default language of the document.
    /// </summary>
    public string Language
    {
      get { return Catalog.Elements.GetString(PdfCatalog.Keys.Lang); }
      set { Catalog.Elements.SetString(PdfCatalog.Keys.Lang, value); }
    }

    /// <summary>
    /// Gets the security settings of this document.
    /// </summary>
    public PdfSecuritySettings SecuritySettings
    {
      get
      {
        if (this.securitySettings == null)
          this.securitySettings = new PdfSecuritySettings(this);
        return this.securitySettings;
      }
    }
    internal PdfSecuritySettings securitySettings;

    /// <summary>
    /// Gets the document font table that holds all fonts used in the current document.
    /// </summary>
    internal PdfFontTable FontTable
    {
      get
      {
        if (this.fontTable == null)
          this.fontTable = new PdfFontTable(this);
        return this.fontTable;
      }
    }
    PdfFontTable fontTable;

    /// <summary>
    /// Gets the document image table that holds all images used in the current document.
    /// </summary>
    internal PdfImageTable ImageTable
    {
      get
      {
        if (this.imageTable == null)
          this.imageTable = new PdfImageTable(this);
        return this.imageTable;
      }
    }
    PdfImageTable imageTable;

    /// <summary>
    /// Gets the document form table that holds all form external objects used in the current document.
    /// </summary>
    internal PdfFormXObjectTable FormTable
    {
      get
      {
        if (this.formTable == null)
          this.formTable = new PdfFormXObjectTable(this);
        return this.formTable;
      }
    }
    PdfFormXObjectTable formTable;

    /// <summary>
    /// Gets the document ExtGState table that holds all form state objects used in the current document.
    /// </summary>
    internal PdfExtGStateTable ExtGStateTable
    {
      get
      {
        if (this.extGStateTable == null)
          this.extGStateTable = new PdfExtGStateTable(this);
        return this.extGStateTable;
      }
    }
    PdfExtGStateTable extGStateTable;

    /// <summary>
    /// Gets the PdfCatalog of the current document.
    /// </summary>
    internal PdfCatalog Catalog
    {
      get
      {
        if (this.catalog == null)
          this.catalog = this.trailer.Root;
        return catalog;
      }
    }
    PdfCatalog catalog;  // never changes if once created

    /// <summary>
    /// Gets the PdfInternals object of this document, that grants access to some internal structures
    /// which are not part of the public interface of PdfDocument.
    /// </summary>
    public new PdfInternals Internals
    {
      get
      {
        if (this.internals == null)
          this.internals = new PdfInternals(this);
        return this.internals;
      }
    }
    PdfInternals internals;

    /// <summary>
    /// Creates a new page and adds it to this document.
    /// </summary>
    public PdfPage AddPage()
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);
      return Catalog.Pages.Add();
    }

    /// <summary>
    /// Adds the specified page to this document. If the page is from an external document,
    /// it is imported to this document. In this case the returned page is not the same
    /// object as the specified one.
    /// </summary>
    public PdfPage AddPage(PdfPage page)
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);
      return Catalog.Pages.Add(page);
    }

    /// <summary>
    /// Creates a new page and inserts it in this document at the specified position.
    /// </summary>
    public PdfPage InsertPage(int index)
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);
      return Catalog.Pages.Insert(index);
    }

    /// <summary>
    /// Inserts the specified page in this document. If the page is from an external document,
    /// it is imported to this document. In this case the returned page is not the same
    /// object as the specified one.
    /// </summary>
    public PdfPage InsertPage(int index, PdfPage page)
    {
      if (!CanModify)
        throw new InvalidOperationException(PSSR.CannotModify);
      return Catalog.Pages.Insert(index, page);
    }

    /// <summary>
    /// Gets the security handler.
    /// </summary>
    public PdfStandardSecurityHandler SecurityHandler
    {
      get { return this.trailer.SecurityHandler; }
    }

    internal PdfTrailer trailer;
    internal PdfReferenceTable irefTable;
    internal Stream outStream;

    // Imported Document
    internal Lexer lexer;

    internal DateTime creation;

    /// <summary>
    /// Occurs when the specified document is not used anymore for importing content.
    /// </summary>
    internal void OnExternalDocumentFinalized(PdfDocument.DocumentHandle handle)
    {
      if (tls != null)
      {
        //PdfDocument[] documents = tls.Documents;
        tls.DetachDocument(handle);
      }

      if (this.formTable != null)
        formTable.DetachDocument(handle);
    }

    //internal static GlobalObjectTable Gob = new GlobalObjectTable();

    /// <summary>
    /// Gets the ThreadLocalStorage object. It is used for caching objects that should created
    /// only once.
    /// </summary>
    internal static ThreadLocalStorage Tls
    {
      get
      {
        if (tls == null)
          tls = new ThreadLocalStorage();
        return tls;
      }
    }
    [ThreadStatic]
    static ThreadLocalStorage tls;

    [DebuggerDisplay("(ID={ID}, alive={IsAlive})")]
    internal class DocumentHandle
    {
      public DocumentHandle(PdfDocument document)
      {
        this.weakRef = new WeakReference(document);
        this.ID = document.guid.ToString("B").ToUpper();
      }

      public bool IsAlive
      {
        get { return this.weakRef.IsAlive; }
      }

      public PdfDocument Target
      {
        get { return this.weakRef.Target as PdfDocument; }
      }
      WeakReference weakRef;

      public string ID;

      public override bool Equals(object obj)
      {
        DocumentHandle handle = obj as DocumentHandle;
        if (!Object.ReferenceEquals(handle, null))
          return this.ID == handle.ID;
        return false;
      }

      public override int GetHashCode()
      {
        return this.ID.GetHashCode();
      }

      public static bool operator ==(DocumentHandle left, DocumentHandle right)
      {
        if (Object.ReferenceEquals(left, null))
          return Object.ReferenceEquals(right, null);
        return left.Equals(right);
      }

      public static bool operator !=(DocumentHandle left, DocumentHandle right)
      {
        return !(left == right);
      }
    }
  }
}