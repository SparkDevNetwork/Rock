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
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Internal
{
  /// <summary>
  /// Provides a thread-local cache for large objects.
  /// </summary>
  internal class ThreadLocalStorage
  {
    public ThreadLocalStorage()
    {
      this.importedDocuments = new Dictionary<string, PdfDocument.DocumentHandle>(StringComparer.InvariantCultureIgnoreCase);
    }

    public void AddDocument(string path, PdfDocument document)
    {
      this.importedDocuments.Add(path, document.Handle);
    }

    public void RemoveDocument(string path)
    {
      this.importedDocuments.Remove(path);
    }

    public PdfDocument GetDocument(string path)
    {
      Debug.Assert(path.StartsWith("*") || Path.IsPathRooted(path), "Path must be full qualified.");

      PdfDocument document = null;
      PdfDocument.DocumentHandle handle;
      if (this.importedDocuments.TryGetValue(path, out handle))
      {
        document = handle.Target;
        if (document == null)
          RemoveDocument(path);
      }
      if (document == null)
      {
        document = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        this.importedDocuments.Add(path, document.Handle);
      }
      return document;
    }

    public PdfDocument[] Documents
    {
      get
      {
        List<PdfDocument> list = new List<PdfDocument>();
        foreach (PdfDocument.DocumentHandle handle in this.importedDocuments.Values)
        {
          if (handle.IsAlive)
            list.Add(handle.Target);
        }
        return list.ToArray();
      }
    }

    public void DetachDocument(PdfDocument.DocumentHandle handle)
    {
      if (handle.IsAlive)
      {
        foreach (String path in this.importedDocuments.Keys)
        {
          if (this.importedDocuments[path] == handle)
          {
            this.importedDocuments.Remove(path);
            break;
          }
        }
      }

      // Clean table
      bool itemRemoved = true;
      while (itemRemoved)
      {
        itemRemoved = false;
        foreach (String path in this.importedDocuments.Keys)
        {
          if (!this.importedDocuments[path].IsAlive)
          {
            this.importedDocuments.Remove(path);
            itemRemoved = true;
            break;
          }
        }
      }
    }

    /// <summary>
    /// Maps path to document handle.
    /// </summary>
    readonly Dictionary<string, PdfDocument.DocumentHandle> importedDocuments;
  }
}