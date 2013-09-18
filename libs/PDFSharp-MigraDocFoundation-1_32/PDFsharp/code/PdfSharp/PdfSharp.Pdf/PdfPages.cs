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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents the pages of the document.
  /// </summary>
  [DebuggerDisplay("(PageCount={Count})")]
  public sealed class PdfPages : PdfDictionary, IEnumerable
  {
    internal PdfPages(PdfDocument document)
      : base(document)
    {
      Elements.SetName(Keys.Type, "/Pages");
      Elements[Keys.Count] = new PdfInteger(0);
    }

    internal PdfPages(PdfDictionary dictionary)
      : base(dictionary)
    {
    }

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public int Count
    {
      get { return PagesArray.Elements.Count; }
    }

    /// <summary>
    /// Gets the page with the specified index.
    /// </summary>
    public PdfPage this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.PageIndexOutOfRange);

        PdfDictionary dict = (PdfDictionary)((PdfReference)PagesArray.Elements[index]).Value;
        if (!(dict is PdfPage))
          dict = new PdfPage(dict);
        return (PdfPage)dict;
      }
      //set
      //{
      //  if (index < 0 || index >= this.pages.Count)
      //    throw new ArgumentOutOfRangeException("index", index, PSSR.PageIndexOutOfRange);
      //  this.pages[index] = value;
      //}
    }

    /// <summary>
    /// Creates a new PdfPage, adds it to this document, and returns it.
    /// </summary>
    public PdfPage Add()
    {
      PdfPage page = new PdfPage();
      Insert(Count, page);
      return page;
    }

    /// <summary>
    /// Adds the specified PdfPage to this document and maybe returns a new PdfPage object.
    /// The value returned is a new object if the added page comes from a foreign document.
    /// </summary>
    public PdfPage Add(PdfPage page)
    {
      return Insert(Count, page);
    }

    /// <summary>
    /// Creates a new PdfPage, inserts it at the specified position into this document, and returns it.
    /// </summary>
    public PdfPage Insert(int index)
    {
      PdfPage page = new PdfPage();
      Insert(index, page);
      return page;
    }

    /// <summary>
    /// Inserts the specified PdfPage at the specified position to this document and maybe returns a new PdfPage object.
    /// The value returned is a new object if the inserted page comes from a foreign document.
    /// </summary>
    public PdfPage Insert(int index, PdfPage page)
    {
      if (page == null)
        throw new ArgumentNullException("page");

      // Is the page already owned by this document
      if (page.Owner == Owner)
      {
        // Case: Page is removed and than inserted a another position.
        int count = Count;
        for (int idx = 0; idx < count; idx++)
        {
          if (ReferenceEquals(this[idx], page))
            throw new InvalidOperationException(PSSR.MultiplePageInsert);
        }
        // TODO: check this case
        Owner.irefTable.Add(page);
        PagesArray.Elements.Insert(index, page.Reference);
        Elements.SetInteger(PdfPages.Keys.Count, PagesArray.Elements.Count);
        return page;
      }

      // All page insertions come here
      if (page.Owner == null)
      {
        // Case: New page was created and inserted now.
        page.Document = Owner;

        Owner.irefTable.Add(page);
        PagesArray.Elements.Insert(index, page.Reference);
        Elements.SetInteger(PdfPages.Keys.Count, PagesArray.Elements.Count);
      }
      else
      {
        // Case: Page is from an external document -> import it.
        page = ImportExternalPage(page);
        Owner.irefTable.Add(page);
        PagesArray.Elements.Insert(index, page.Reference);
        Elements.SetInteger(PdfPages.Keys.Count, PagesArray.Elements.Count);
        PdfAnnotations.FixImportedAnnotation(page);
      }
      if (Owner.Settings.TrimMargins.AreSet)
        page.TrimMargins = Owner.Settings.TrimMargins;
      return page;
    }

    /// <summary>
    /// Removes the specified page from the document.
    /// </summary>
    public void Remove(PdfPage page)
    {
      PagesArray.Elements.Remove(page.Reference);
      Elements.SetInteger(PdfPages.Keys.Count, PagesArray.Elements.Count);
    }

    /// <summary>
    /// Removes the specified page from the document.
    /// </summary>
    public void RemoveAt(int index)
    {
      PagesArray.Elements.RemoveAt(index);
      Elements.SetInteger(PdfPages.Keys.Count, PagesArray.Elements.Count);
    }

    /// <summary>
    /// Moves a page within the page sequence.
    /// </summary>
    /// <param name="oldIndex">The page index before this operation.</param>
    /// <param name="newIndex">The page index after this operation.</param>
    public void MovePage(int oldIndex, int newIndex)
    {
      if (oldIndex < 0 || oldIndex >= Count)
        throw new ArgumentOutOfRangeException("oldIndex");
      if (newIndex < 0 || newIndex >= Count)
        throw new ArgumentOutOfRangeException("newIndex");
      if (oldIndex == newIndex)
        return;

      PdfReference page = (PdfReference)pagesArray.Elements[oldIndex];
      pagesArray.Elements.RemoveAt(oldIndex);
      pagesArray.Elements.Insert(newIndex, page);
    }

    /// <summary>
    /// Imports an external page. The elements of the imported page are cloned and added to this document.
    /// Important: In contrast to PdfFormXObject adding an external page always make a deep copy
    /// of their transitive closure. Any reuse of already imported objects is not intended because
    /// any modification of an imported page must not change another page.
    /// </summary>
    PdfPage ImportExternalPage(PdfPage importPage)
    {
      if (importPage.Owner.openMode != PdfDocumentOpenMode.Import)
        throw new InvalidOperationException("A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.");

      PdfPage page = new PdfPage(this.document);

      CloneElement(page, importPage, PdfPage.Keys.Resources, false);
      CloneElement(page, importPage, PdfPage.Keys.Contents, false);
      CloneElement(page, importPage, PdfPage.Keys.MediaBox, true);
      CloneElement(page, importPage, PdfPage.Keys.CropBox, true);
      CloneElement(page, importPage, PdfPage.Keys.Rotate, true);
      CloneElement(page, importPage, PdfPage.Keys.BleedBox, true);
      CloneElement(page, importPage, PdfPage.Keys.TrimBox, true);
      CloneElement(page, importPage, PdfPage.Keys.ArtBox, true);
#if true
      // Do not deep copy annotations.
      CloneElement(page, importPage, PdfPage.Keys.Annots, false);
#else
      // Deep copy annotations.
      CloneElement(page, importPage, PdfPage.Keys.Annots, true);
#endif
      // TODO more elements?
      return page;
    }

    /// <summary>
    /// Helper function for ImportExternalPage.
    /// </summary>
    void CloneElement(PdfPage page, PdfPage importPage, string key, bool deepcopy)
    {
      Debug.Assert(page.Owner == this.document);
      Debug.Assert(importPage.Owner != null);
      Debug.Assert(importPage.Owner != this.document);

      PdfItem item = importPage.Elements[key];
      if (item != null)
      {
        PdfImportedObjectTable importedObjectTable = null;
        if (!deepcopy)
          importedObjectTable = Owner.FormTable.GetImportedObjectTable(importPage);

        // The item can be indirect. If so, replace it by its value.
        if (item is PdfReference)
          item = ((PdfReference)item).Value;
        if (item is PdfObject)
        {
          PdfObject root = (PdfObject)item;
          if (deepcopy)
          {
            Debug.Assert(root.Owner != null, "See 'else' case for details");
            root = PdfObject.DeepCopyClosure(this.document, root);
          }
          else
          {
            // The owner can be null if the item is not a reference
            if (root.Owner == null)
              root.Document = importPage.Owner;
            root = PdfObject.ImportClosure(importedObjectTable, page.Owner, root);
          }

          if (root.Reference == null)
            page.Elements[key] = root;
          else
            page.Elements[key] = root.Reference;
        }
        else
        {
          // Simple items are just cloned.
          page.Elements[key] = item.Clone();
        }
      }
    }

    /// <summary>
    /// Gets a PdfArray containing all pages of this document. The array must not be modified.
    /// </summary>
    public PdfArray PagesArray
    {
      get
      {
        if (this.pagesArray == null)
          this.pagesArray = (PdfArray)Elements.GetValue(Keys.Kids, VCF.Create);
        return this.pagesArray;
      }
    }
    PdfArray pagesArray;

    /// <summary>
    /// Replaces the page tree by a flat array of indirect references to the pages objects.
    /// </summary>
    internal void FlattenPageTree()
    {
      // Acrobat creates a balanced tree if the number of pages is rougly more than ten. This is
      // not difficult but obviously also not necessary. I created a document with 50000 pages with
      // PDF4NET and Acrobat opened it in less than 2 seconds.

      //PdfReference xrefRoot = this.Document.Catalog.Elements[PdfCatalog.Keys.Pages] as PdfReference;
      //PdfDictionary[] pages = GetKids(xrefRoot, null);

      // Promote inheritable values down the page tree
      PdfPage.InheritedValues values = new PdfPage.InheritedValues();
      PdfPage.InheritValues(this, ref values);
      PdfDictionary[] pages = GetKids(Reference, values, null);

      // Replace /Pages in catalog by this object
      // xrefRoot.Value = this;

      PdfArray array = new PdfArray(Owner);
      foreach (PdfDictionary page in pages)
      {
        // Fix the parent
        page.Elements[PdfPage.Keys.Parent] = Reference;
        array.Elements.Add(page.Reference);
      }

      Elements.SetName(Keys.Type, "/Pages");
#if true
      // direct array
      Elements.SetValue(Keys.Kids, array);
#else
      // incdirect array
      this.Document.xrefTable.Add(array);
      Elements.SetValue(Keys.Kids, array.XRef);
#endif
      Elements.SetInteger(Keys.Count, array.Elements.Count);
    }

    /// <summary>
    /// Recursively converts the page tree into a flat array.
    /// </summary>
    PdfDictionary[] GetKids(PdfReference iref, PdfPage.InheritedValues values, PdfDictionary parent)
    {
      // TODO: inherit inheritable keys...
      PdfDictionary kid = (PdfDictionary)iref.Value;

      if (kid.Elements.GetName(Keys.Type) == "/Page")
      {
        PdfPage.InheritValues(kid, values);
        return new PdfDictionary[] { kid };
      }
      else
      {
        Debug.Assert(kid.Elements.GetName(Keys.Type) == "/Pages");
        PdfPage.InheritValues(kid, ref values);
        List<PdfDictionary> list = new List<PdfDictionary>();
        PdfArray kids = kid.Elements["/Kids"] as PdfArray;
        //newTHHO 15.10.2007 begin
        if (kids == null)
        {
          PdfReference xref3 = kid.Elements["/Kids"] as PdfReference;
          kids = xref3.Value as PdfArray;
        }
        //newTHHO 15.10.2007 end
        foreach (PdfReference xref2 in kids)
          list.AddRange(GetKids(xref2, values, kid));
        int count = list.Count;
        Debug.Assert(count == kid.Elements.GetInteger("/Count"));
        //return (PdfDictionary[])list.ToArray(typeof(PdfDictionary));
        return list.ToArray();
      }
    }

    /// <summary>
    /// Prepares the document for saving.
    /// </summary>
    internal override void PrepareForSave()
    {
      // TODO: Close all open content streams

      // TODO: Create the page tree.
      // Arrays have a limit of 8192 entries, but I successfully tested documents
      // with 50000 pages and no page tree.
      // ==> wait for bug report.
      int count = this.pagesArray.Elements.Count;
      for (int idx = 0; idx < count; idx++)
      {

        PdfPage page = this[idx];
        page.PrepareForSave();
      }
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    public new IEnumerator GetEnumerator()
    {
      return new PdfPagesEnumerator(this);
    }

    private class PdfPagesEnumerator : IEnumerator
    {
      private PdfPage currentElement;
      private int index;
      private PdfPages list;

      internal PdfPagesEnumerator(PdfPages list)
      {
        this.list = list;
        this.index = -1;
      }

      public bool MoveNext()
      {
        if (this.index < this.list.Count - 1)
        {
          this.index++;
          this.currentElement = this.list[this.index];
          return true;
        }
        this.index = this.list.Count;
        return false;
      }

      public void Reset()
      {
        this.currentElement = null;
        this.index = -1;
      }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      public PdfPage Current
      {
        get
        {
          if (this.index == -1 || this.index >= this.list.Count)
            throw new InvalidOperationException(PSSR.ListEnumCurrentOutOfRange);
          return this.currentElement;
        }
      }
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    internal sealed class Keys : PdfPage.InheritablePageKeys
    {
      /// <summary>
      /// (Required) The type of PDF object that this dictionary describes; 
      /// must be Pages for a page tree node.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Pages")]
      public const string Type = "/Type";

      /// <summary>
      /// (Required except in root node; must be an indirect reference)
      /// The page tree node that is the immediate parent of this one.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string Parent = "/Parent";

      /// <summary>
      /// (Required) An array of indirect references to the immediate children of this node.
      /// The children may be page objects or other page tree nodes.
      /// </summary>
      [KeyInfo(KeyType.Array | KeyType.Required)]
      public const string Kids = "/Kids";

      /// <summary>
      /// (Required) The number of leaf nodes (page objects) that are descendants of this node 
      /// within the page tree.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Required)]
      public const string Count = "/Count";

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
