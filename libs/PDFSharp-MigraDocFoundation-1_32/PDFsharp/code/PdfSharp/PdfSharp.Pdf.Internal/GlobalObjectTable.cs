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
  internal class GlobalObjectTable
  {
    public GlobalObjectTable()
    {
    }

    public void AttatchDocument(PdfDocument.DocumentHandle handle)
    {
      lock (this.documentHandles)
      {
        this.documentHandles.Add(handle);
      }

      //WeakReference weakRef = new WeakReference(document);
      //lock (this.documents)
      //{
      //  this.documents.Add(weakRef);
      //}
    }

    public void DetatchDocument(PdfDocument.DocumentHandle handle)
    {
      lock (this.documentHandles)
      {
        // Notify other documents about detach
        int count = this.documentHandles.Count;
        for (int idx = 0; idx < count; idx++)
        {
          if (((PdfDocument.DocumentHandle)this.documentHandles[idx]).IsAlive)
          {
            PdfDocument target = ((PdfDocument.DocumentHandle)this.documentHandles[idx]).Target;
            if (target != null)
              target.OnExternalDocumentFinalized(handle);
          }
        }

        // Clean up table
        for (int idx = 0; idx < this.documentHandles.Count; idx++)
        {
          PdfDocument target = ((PdfDocument.DocumentHandle)this.documentHandles[idx]).Target;
          if (target == null)
          {
            this.documentHandles.RemoveAt(idx);
            idx--;
          }
        }
      }

      //lock (this.documents)
      //{
      //  int index = IndexOf(document);
      //  if (index != -1)
      //  {
      //    this.documents.RemoveAt(index);
      //    int count = this.documents.Count;
      //    for (int idx = 0; idx < count; idx++)
      //    {
      //      PdfDocument target = ((WeakReference)this.documents[idx]).Target as PdfDocument;
      //      if (target != null)
      //        target.OnExternalDocumentFinalized(document);
      //    }

      //    for (int idx = 0; idx < this.documents.Count; idx++)
      //    {
      //      PdfDocument target = ((WeakReference)this.documents[idx]).Target as PdfDocument;
      //      if (target == null)
      //      {
      //        this.documents.RemoveAt(idx);
      //        idx--;
      //      }
      //    }
      //  }
      //}
    }

    //int IndexOf(PdfDocument.Handle handle)
    //{
    //  int count = this.documents.Count;
    //  for (int idx = 0; idx < count; idx++)
    //  {
    //    if ((PdfDocument.Handle)this.documents[idx] == handle)
    //      return idx;
    //    //if (Object.ReferenceEquals(((WeakReference)this.documents[idx]).Target, document))
    //    //  return idx;
    //  }
    //  return -1;
    //}

    /// <summary>
    /// Array of handles to all documents.
    /// </summary>
    List<object> documentHandles = new List<object>();
  }
}
