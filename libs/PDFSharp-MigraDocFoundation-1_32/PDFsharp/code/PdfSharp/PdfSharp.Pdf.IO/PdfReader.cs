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
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Security;
using PdfSharp.Pdf.Internal;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.IO
{
  /// <summary>
  /// Encapsulates the arguments of the PdfPasswordProvider delegate.
  /// </summary>
  public class PdfPasswordProviderArgs
  {
    /// <summary>
    /// Sets the password to open the document with.
    /// </summary>
    public string Password;

    /// <summary>
    /// When set to true the PdfReader.Open function returns null indicating that no PdfDocument was created.
    /// </summary>
    public bool Abort;
  }

  /// <summary>
  /// A delegated used by the PdfReader.Open function to retrieve a password if the document is protected.
  /// </summary>
  public delegate void PdfPasswordProvider(PdfPasswordProviderArgs args);

  /// <summary>
  /// Represents the functionality for reading PDF documents.
  /// </summary>
  public static class PdfReader
  {
    /// <summary>
    /// Determines whether the file specified by its path is a PDF file by inspecting the first eight
    /// bytes of the data. If the file header has the form «%PDF-x.y» the function returns the version
    /// number as integer (e.g. 14 for PDF 1.4). If the file header is invalid or inaccessible
    /// for any reason, 0 is returned. The function never throws an exception. 
    /// </summary>
    public static int TestPdfFile(string path)
    {
      FileStream stream = null;
      try
      {
        int pageNumber;
        string realPath = PdfSharp.Drawing.XPdfForm.ExtractPageNumber(path, out pageNumber);
        if (File.Exists(realPath)) // prevent unwanted exceptions during debugging
        {
          stream = new FileStream(realPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
          byte[] bytes = new byte[1024];
          stream.Read(bytes, 0, 1024);
          return GetPdfFileVersion(bytes);
        }
      }
      catch { }
      finally
      {
        try
        {
          if (stream != null)
            stream.Close();
        }
        catch { }
      }
      return 0;
    }

    /// <summary>
    /// Determines whether the specified stream is a PDF file by inspecting the first eight
    /// bytes of the data. If the data begins with «%PDF-x.y» the function returns the version
    /// number as integer (e.g. 14 for PDF 1.4). If the data is invalid or inaccessible
    /// for any reason, 0 is returned. The function never throws an exception. 
    /// </summary>
    public static int TestPdfFile(Stream stream)
    {
      long pos = -1;
      try
      {
        pos = stream.Position;
        byte[] bytes = new byte[1024];
        stream.Read(bytes, 0, 1024);
        return GetPdfFileVersion(bytes);
      }
      catch { }
      finally
      {
        try
        {
          if (pos != -1)
            stream.Position = pos;
        }
        catch { }
      }
      return 0;
    }

    /// <summary>
    /// Determines whether the specified data is a PDF file by inspecting the first eight
    /// bytes of the data. If the data begins with «%PDF-x.y» the function returns the version
    /// number as integer (e.g. 14 for PDF 1.4). If the data is invalid or inaccessible
    /// for any reason, 0 is returned. The function never throws an exception. 
    /// </summary>
    public static int TestPdfFile(byte[] data)
    {
      return GetPdfFileVersion(data);
    }

    /// <summary>
    /// Implements scanning the PDF file version.
    /// </summary>
    internal static int GetPdfFileVersion(byte[] bytes)
    {
#if !SILVERLIGHT
      try
      {
        // Acrobat accepts headers like «%!PS-Adobe-N.n PDF-M.m»...
        string header = Encoding.ASCII.GetString(bytes);
        if (header[0] == '%' || header.IndexOf("%PDF")>=0)
        {
          int ich = header.IndexOf("PDF-");
          if (ich > 0 && header[ich + 5] == (byte)'.')
          {
            char major = header[ich + 4];
            char minor = header[ich + 6];
            if (major >= '1' && major < '2' && minor >= '0' && minor <= '9')
              return (major - '0') * 10 + (minor - '0');
          }
        }
      }
      catch {}
      return 0;
#else
      return 50; // AGHACK
#endif
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path, PdfDocumentOpenMode openmode)
    {
      return Open(path, null, openmode, null);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path, PdfDocumentOpenMode openmode, PdfPasswordProvider provider)
    {
      return Open(path, null, openmode, provider);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path, string password, PdfDocumentOpenMode openmode)
    {
      return Open(path, password, openmode, null);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path, string password, PdfDocumentOpenMode openmode, PdfPasswordProvider provider)
    {
      PdfDocument document;
      Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
      try
      {
        document = PdfReader.Open(stream, password, openmode, provider);
        if (document != null)
        {
          document.fullPath = Path.GetFullPath(path);
        }
      }
      finally
      {
        if (stream != null)
          stream.Close();
      }
      return document;
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path)
    {
      return Open(path, null, PdfDocumentOpenMode.Modify, null);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(string path, string password)
    {
      return Open(path, password, PdfDocumentOpenMode.Modify, null);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openmode)
    {
      return Open(stream, null, openmode);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(Stream stream, PdfDocumentOpenMode openmode, PdfPasswordProvider passwordProvider)
    {
      return Open(stream, null, openmode);
    }
    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(Stream stream, string password, PdfDocumentOpenMode openmode)
    {
      return Open(stream, password, openmode, null);
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(Stream stream, string password, PdfDocumentOpenMode openmode, PdfPasswordProvider passwordProvider)
    {
      PdfDocument document = null;
      try
      {
        Lexer lexer = new Lexer(stream);
        document = new PdfDocument(lexer);
        document.state |= DocumentState.Imported;
        document.openMode = openmode;
        document.fileSize = stream.Length;

        // Get file version
        byte[] header = new byte[1024];
        stream.Position = 0;
        stream.Read(header, 0, 1024);
        document.version = GetPdfFileVersion(header);
        if (document.version == 0)
          throw new InvalidOperationException(PSSR.InvalidPdf);

        // Read all trailers
        document.irefTable.IsUnderConstruction = true;
        Parser parser = new Parser(document);
        document.trailer = parser.ReadTrailer();

        document.irefTable.IsUnderConstruction = false;

        // Is document encrypted?
        PdfReference xrefEncrypt = document.trailer.Elements[PdfTrailer.Keys.Encrypt] as PdfReference;
        if (xrefEncrypt != null)
        {
          //xrefEncrypt.Value = parser.ReadObject(null, xrefEncrypt.ObjectID, false);
          PdfObject encrypt = parser.ReadObject(null, xrefEncrypt.ObjectID, false);

          encrypt.Reference = xrefEncrypt;
          xrefEncrypt.Value = encrypt;
          PdfStandardSecurityHandler securityHandler = document.SecurityHandler;
        TryAgain:
          PasswordValidity validity = securityHandler.ValidatePassword(password);
          if (validity == PasswordValidity.Invalid)
          {
            if (passwordProvider != null)
            {
              PdfPasswordProviderArgs args = new PdfPasswordProviderArgs();
              passwordProvider(args);
              if (args.Abort)
                return null;
              password = args.Password;
              goto TryAgain;
            }
            else
            {
              if (password == null)
                throw new PdfReaderException(PSSR.PasswordRequired);
              else
                throw new PdfReaderException(PSSR.InvalidPassword);
            }
          }
          else if (validity == PasswordValidity.UserPassword && openmode == PdfDocumentOpenMode.Modify)
          {
            if (passwordProvider != null)
            {
              PdfPasswordProviderArgs args = new PdfPasswordProviderArgs();
              passwordProvider(args);
              if (args.Abort)
                return null;
              password = args.Password;
              goto TryAgain;
            }
            else
              throw new PdfReaderException(PSSR.OwnerPasswordRequired);
          }
        }
        else
        {
          if (password != null)
          {
            // Password specified but document is not encrypted.
            // ignore
          }
        }

        PdfReference[] irefs = document.irefTable.AllReferences;
        int count = irefs.Length;

        // Read all indirect objects
        for (int idx = 0; idx < count; idx++)
        {
          PdfReference iref = irefs[idx];
          if (iref.Value == null)
          {
            try
            {
              Debug.Assert(document.irefTable.Contains(iref.ObjectID));
              PdfObject pdfObject = parser.ReadObject(null, iref.ObjectID, false);
              Debug.Assert(pdfObject.Reference == iref);
              pdfObject.Reference = iref;
              Debug.Assert(pdfObject.Reference.Value != null, "something got wrong");
            }
            catch (Exception ex)
            {
              Debug.WriteLine(ex.Message);
            }
          }
          else
          {
            Debug.Assert(document.irefTable.Contains(iref.ObjectID));
            iref.GetType();
          }
          // Set maximum object number
          document.irefTable.maxObjectNumber = Math.Max(document.irefTable.maxObjectNumber, iref.ObjectNumber);
        }
        // Encrypt all objects
        if (xrefEncrypt != null)
        {
          document.SecurityHandler.EncryptDocument();
        }

        // Fix references of trailer values and then objects and irefs are consistent.
        document.trailer.Finish();

#if DEBUG_
        // Some tests...
        PdfReference[] reachables = document.xrefTable.TransitiveClosure(document.trailer);
        reachables.GetType();
        reachables = document.xrefTable.AllXRefs;
        document.xrefTable.CheckConsistence();
#endif

        if (openmode == PdfDocumentOpenMode.Modify)
        {
          // Create new or change existing document IDs
          if (document.Internals.SecondDocumentID == "")
            document.trailer.CreateNewDocumentIDs();
          else
          {
            byte[] agTemp = Guid.NewGuid().ToByteArray();
            document.Internals.SecondDocumentID = PdfEncoders.RawEncoding.GetString(agTemp, 0, agTemp.Length);
          }

          // Change modification date
          document.Info.ModificationDate = DateTime.Now;

          // Remove all unreachable objects
          int removed = document.irefTable.Compact();
          if (removed != 0)
            Debug.WriteLine("Number of deleted unreachable objects: " + removed);

          // Force flattening of page tree
          PdfPages pages = document.Pages;

          //bool b = document.irefTable.Contains(new PdfObjectID(1108));
          //b.GetType();

          document.irefTable.CheckConsistence();
          document.irefTable.Renumber();
          document.irefTable.CheckConsistence();
        }
      }
      finally
      {
        //if (filestream != null)
        //  filestream.Close();
      }
      return document;
    }

    /// <summary>
    /// Opens an existing PDF document.
    /// </summary>
    public static PdfDocument Open(Stream stream)
    {
      return PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
    }
  }
}