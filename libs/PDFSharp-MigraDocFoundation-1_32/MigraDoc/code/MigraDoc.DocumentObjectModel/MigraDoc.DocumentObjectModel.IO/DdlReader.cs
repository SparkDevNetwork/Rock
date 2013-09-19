#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
//   David Stephensen (mailto:David.Stephensen@pdfsharp.com)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
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
using System.IO;
using System.Text;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.DocumentObjectModel.IO
{
  /// <summary>
  /// Represents a reader that provides access to DDL data.
  /// </summary>
  public class DdlReader
  {
    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified Stream.
    /// </summary>
    public DdlReader(Stream stream)
      : this(stream, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified Stream and ErrorManager2.
    /// </summary>
    public DdlReader(Stream stream, DdlReaderErrors errors)
    {
      this.errorManager = errors;
      this.reader = new StreamReader(stream);
    }

    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified filename.
    /// </summary>
    public DdlReader(string filename)
      : this(filename, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified filename and ErrorManager2.
    /// </summary>
    public DdlReader(string filename, DdlReaderErrors errors)
    {
      this.fileName = filename;
      this.errorManager = errors;
      this.reader = new StreamReader(filename, Encoding.Default);
    }

    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified TextReader.
    /// </summary>
    public DdlReader(TextReader reader)
      : this(reader, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DdlReader class with the specified TextReader and ErrorManager2.
    /// </summary>
    public DdlReader(TextReader reader, DdlReaderErrors errors)
    {
      this.doClose = false;
      this.errorManager = errors;
      this.reader = reader;
    }

    /// <summary>
    /// Closes the underlying stream or text writer.
    /// </summary>
    public void Close()
    {
      if (this.doClose && this.reader != null)
      {
        this.reader.Close();
        this.reader = null;
      }
    }

    /// <summary>
    /// Reads and returns a Document from a file or a DDL string.
    /// </summary>
    public Document ReadDocument()
    {
      string ddl = this.reader.ReadToEnd();

      Document document = null;
      if (this.fileName != null && this.fileName != "")
      {
        DdlParser parser = new DdlParser(this.fileName, ddl, this.errorManager);
        document = parser.ParseDocument(null);
        document.ddlFile = this.fileName;
      }
      else
      {
        DdlParser parser = new DdlParser(ddl, this.errorManager);
        document = parser.ParseDocument(null);
      }

      return document;
    }

    /// <summary>
    /// Reads and returns a DocumentObject from a file or a DDL string.
    /// </summary>
    public DocumentObject ReadObject()
    {
      string ddl = this.reader.ReadToEnd();

      DdlParser parser = null;
      if (this.fileName != null && this.fileName != "")
        parser = new DdlParser(this.fileName, ddl, this.errorManager);
      else
        parser = new DdlParser(ddl, this.errorManager);
      return parser.ParseDocumentObject();
    }

    /// <summary>
    /// Reads and returns a Document from the specified file.
    /// </summary>
    public static Document DocumentFromFile(string documentFileName) //, ErrorManager2 _errorManager)
    {
      Document document = null;
      DdlReader reader = null;
      try
      {
        reader = new DdlReader(documentFileName); //, _errorManager);
        document = reader.ReadDocument();
      }
      finally
      {
        if (reader != null)
          reader.Close();
      }
      return document;
    }

    /// <summary>
    /// Reads and returns a Document from the specified DDL string.
    /// </summary>
    public static Document DocumentFromString(string ddl)
    {
      StringReader stringReader = null;
      Document document = null;
      DdlReader reader = null;
      try
      {
        stringReader = new StringReader(ddl);

        reader = new DdlReader(stringReader);
        document = reader.ReadDocument();
      }
      finally
      {
        if (stringReader != null)
          stringReader.Close();
        if (reader != null)
          reader.Close();
      }
      return document;
    }

    /// <summary>
    /// Reads and returns a domain object from the specified file.
    /// </summary>
    public static DocumentObject ObjectFromFile(string documentFileName, DdlReaderErrors errors)
    {
      DdlReader reader = null;
      DocumentObject domObj = null;
      try
      {
        reader = new DdlReader(documentFileName, errors);
        domObj = reader.ReadObject();
      }
      finally
      {
        if (reader != null)
          reader.Close();
      }
      return domObj;
    }

    /// <summary>
    /// Reads and returns a domain object from the specified file.
    /// </summary>
    public static DocumentObject ObjectFromFile(string documentFileName)
    {
      return ObjectFromFile(documentFileName, null);
    }

    /// <summary>
    /// Reads and returns a domain object from the specified DDL string.
    /// </summary>
    public static DocumentObject ObjectFromString(string ddl, DdlReaderErrors errors)
    {
      StringReader stringReader = null;
      DocumentObject domObj = null;
      DdlReader reader = null;
      try
      {
        stringReader = new StringReader(ddl);

        reader = new DdlReader(stringReader);
        domObj = reader.ReadObject();
      }
      finally
      {
        if (stringReader != null)
          stringReader.Close();
        if (reader != null)
          reader.Close();
      }
      return domObj;
    }

    /// <summary>
    /// Reads and returns a domain object from the specified DDL string.
    /// </summary>
    public static DocumentObject ObjectFromString(string ddl)
    {
      return ObjectFromString(ddl, null);
    }
    bool doClose = true;
    TextReader reader;
    DdlReaderErrors errorManager;
    string fileName;
  }
}
