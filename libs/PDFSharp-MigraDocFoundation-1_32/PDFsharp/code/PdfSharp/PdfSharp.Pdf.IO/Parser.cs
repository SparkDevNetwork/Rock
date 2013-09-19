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
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.IO
{
  /*
     Direct and indireckt objects
     
     * If a simple object (boolean, integer, number, date, string, rectangle etc.) is referenced indirect,
       the parser reads this objects immediatly and consumes the indirection.
       
     * If a composite object (dictionary, array etc.) is referenced indirect, a PdfReference objects
       is returned.
       
     * If a composite object is a direct object, no PdfReference is created and the object is
       parsed immediatly.
       
     * A refernece to a non existing object is specified as legal, therefor null is returned.
  */

  /// <summary>
  /// Provides the functions to parse PDF documents.
  /// </summary>
  internal sealed class Parser
  {
    PdfDocument document;
    Lexer lexer;
    ShiftStack stack;

    public Parser(PdfDocument document, Stream pdf)
    {
      this.document = document;
      this.lexer = new Lexer(pdf);
      this.stack = new ShiftStack();
    }

    public Parser(PdfDocument document)
    {
      this.document = document;
      this.lexer = document.lexer;
      this.stack = new ShiftStack();
    }

    /// <summary>
    /// Sets PDF input stream position to the specified object.
    /// </summary>
    public int MoveToObject(PdfObjectID objectID)
    {
      int position = this.document.irefTable[objectID].Position;
      return this.lexer.Position = position;
    }

    public Symbol Symbol
    {
      get { return this.lexer.Symbol; }
    }

    /// <summary>
    /// Reads PDF object from input stream.
    /// </summary>
    /// <param name="pdfObject">Either the instance of a derived type or null. If it is null
    /// an appropriate object is created.</param>
    /// <param name="objectID">The address of the object.</param>
    /// <param name="includeReferences">If true, specifies that all indirect objects
    /// are included recursively.</param>
    public PdfObject ReadObject(PdfObject pdfObject, PdfObjectID objectID, bool includeReferences)
    {
      MoveToObject(objectID);
      int objectNumber = ReadInteger();
      int generationNumber = ReadInteger();
#if DEBUG
      // The following assertion sometime failed (see below)
      //Debug.Assert(objectID == new PdfObjectID(objectNumber, generationNumber));
      if (objectID != new PdfObjectID(objectNumber, generationNumber))
      {
        // A special kind of bug? Or is this an undocumented PDF feature?
        // PDF4NET 2.6 provides a sample called 'Unicode', which produces a file 'unicode.pdf'
        // The iref table of this file contains the following entries:
        //    iref
        //    0 148
        //    0000000000 65535 f 
        //    0000000015 00000 n 
        //    0000000346 00000 n 
        //    ....
        //    0000083236 00000 n 
        //    0000083045 00000 n 
        //    0000083045 00000 n 
        //    0000083045 00000 n 
        //    0000083045 00000 n 
        //    0000080334 00000 n 
        //    ....
        // Object 84, 85, 86, and 87 maps to the same dictionary, but all PDF readers I tested
        // ignores this mismatch! The following assertion failed about 50 times with this file.
#if true_
        string message = String.Format("xref entry {0} {1} maps to object {2} {3}.",
          objectID.ObjectNumber, objectID.GenerationNumber, objectNumber, generationNumber);
        Debug.Assert(false, message);
#endif
      }
#endif
      // Always use object ID from iref table (see above)
      objectNumber = objectID.ObjectNumber;
      generationNumber = objectID.GenerationNumber;
#if true_
      Debug.WriteLine(String.Format("obj: {0} {1}", objectNumber, generationNumber));
#endif
      ReadSymbol(Symbol.Obj);

      bool checkForStream = false;
      Symbol symbol = ScanNextToken();
      switch (symbol)
      {
        case Symbol.BeginArray:
          PdfArray array;
          if (pdfObject == null)
            array = new PdfArray(this.document);
          else
            array = (PdfArray)pdfObject;
          //PdfObject.RegisterObject(array, objectID, generation);
          pdfObject = ReadArray(array, includeReferences);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          break;

        case Symbol.BeginDictionary:
          PdfDictionary dict;
          if (pdfObject == null)
            dict = new PdfDictionary(this.document);
          else
            dict = (PdfDictionary)pdfObject;
          //PdfObject.RegisterObject(dict, objectID, generation);
          checkForStream = true;
          pdfObject = ReadDictionary(dict, includeReferences);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          break;

        // Acrobat 6 Professional proudly presents: The Null object!
        // Even with a one-digit object number an indirect reference «x 0 R» to this object is
        // one character larger than the direct use of «null». Probable this is the reason why
        // it is true that Acrobat Web Capture 6.0 creates this object, but obviously never 
        // creates a reference to it!
        case Symbol.Null:
          pdfObject = new PdfNullObject(this.document);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.Boolean:
          pdfObject = new PdfBooleanObject(this.document, string.Compare(this.lexer.Token, Boolean.TrueString, true) == 0); //!!!mod THHO 19.11.09
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.Integer:
          pdfObject = new PdfIntegerObject(this.document, this.lexer.TokenToInteger);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.UInteger:
          pdfObject = new PdfUIntegerObject(this.document, this.lexer.TokenToUInteger);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.Real:
          pdfObject = new PdfRealObject(this.document, this.lexer.TokenToReal);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.String:
          pdfObject = new PdfStringObject(this.document, this.lexer.Token);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.Name:
          pdfObject = new PdfNameObject(this.document, this.lexer.Token);
          pdfObject.SetObjectID(objectNumber, generationNumber);
          ReadSymbol(Symbol.EndObj);
          return pdfObject;

        case Symbol.Keyword:
          // Should not come here anymore
          throw new NotImplementedException("Keyword");

        default:
          // Should not come here anymore
          throw new NotImplementedException("unknown token \"" + symbol + "\"");
      }
      symbol = ScanNextToken();
      if (symbol == Symbol.BeginStream)
      {
        PdfDictionary dict = (PdfDictionary)pdfObject;
        Debug.Assert(checkForStream, "Unexpected stream...");
        int length = GetStreamLength(dict);
        byte[] bytes = this.lexer.ReadStream(length);
#if true_
        if (dict.Elements.GetString("/Filter") == "/FlateDecode")
        {
          if (dict.Elements["/Subtype"] == null)
          {
            try
            {
              byte[] decoded = Filtering.FlateDecode.Decode(bytes);
              if (decoded.Length == 0)
                goto End;
              string pageContent = Filtering.FlateDecode.DecodeToString(bytes);
              if (pageContent.Length > 100)
                pageContent = pageContent.Substring(pageContent.Length - 100);
              pageContent.GetType();
              bytes = decoded;
              dict.Elements.Remove("/Filter");
              dict.Elements.SetInteger("/Length", bytes.Length);
            }
            catch
            {
            }
          }
        End:;
        }
#endif
        PdfDictionary.PdfStream stream = new PdfDictionary.PdfStream(bytes, dict);
        dict.Stream = stream;
        ReadSymbol(Symbol.EndStream);
        symbol = ScanNextToken();
      }
      if (symbol != Symbol.EndObj)
        throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
      return pdfObject;
    }

    //public PdfObject ReadObject(PdfObject obj, bool includeReferences)

    // HACK solve problem more general
    int GetStreamLength(PdfDictionary dict)
    {
      if (dict.Elements["/F"] != null)
        throw new NotImplementedException("File streams are not yet implemented.");

      PdfItem value = dict.Elements["/Length"];
      if (value is PdfInteger)
        return Convert.ToInt32(value);
      else if (value is PdfReference)
      {
        ParserState state = SaveState();
        object length = ReadObject(null, ((PdfReference)value).ObjectID, false);
        RestoreState(state);
        int l = ((PdfIntegerObject)length).Value;
        dict.Elements["/Length"] = new PdfInteger(l);
        return l;
      }
      throw new InvalidOperationException("Cannot retrieve stream length.");
    }

    public PdfArray ReadArray(PdfArray array, bool includeReferences)
    {
      Debug.Assert(Symbol == Symbol.BeginArray);

      if (array == null)
        array = new PdfArray(this.document);

      int sp = this.stack.SP;
      ParseObject(Symbol.EndArray);
      int count = this.stack.SP - sp;
      PdfItem[] items = this.stack.ToArray(sp, count);
      this.stack.Reduce(count);
      for (int idx = 0; idx < count; idx++)
      {
        PdfItem val = items[idx];
        if (includeReferences && val is PdfReference)
          val = ReadReference((PdfReference)val, includeReferences);
        array.Elements.Add(val);
      }
      return array;
    }

#if DEBUG_
    static int ReadDictionaryCounter;
#endif
    internal PdfDictionary ReadDictionary(PdfDictionary dict, bool includeReferences)
    {
      Debug.Assert(Symbol == Symbol.BeginDictionary);

#if DEBUG_
      ReadDictionaryCounter++;
      Debug.WriteLine(ReadDictionaryCounter.ToString());
      if (ReadDictionaryCounter == 101)
        GetType();
#endif

      if (dict == null)
        dict = new PdfDictionary(this.document);
      DictionaryMeta meta = dict.Meta;

      int sp = this.stack.SP;
      ParseObject(Symbol.EndDictionary);
      int count = this.stack.SP - sp;
      Debug.Assert(count % 2 == 0);
      PdfItem[] items = this.stack.ToArray(sp, count);
      this.stack.Reduce(count);
      for (int idx = 0; idx < count; idx += 2)
      {
        PdfItem val = items[idx];
        if (!(val is PdfName))
          throw new PdfReaderException("name expected");
        string key = ((PdfName)val).ToString();
#if DEBUG_
        if (key == "/ID")
        {
          GetType();
          char x = ((PdfString)(((PdfArray)items[idx + 1]).Elements[0])).Value[0];
          x.GetType();
        }
#endif
        val = items[idx + 1];
        if (includeReferences && val is PdfReference)
          val = ReadReference((PdfReference)val, includeReferences);
        dict.Elements[key] = val;
      }
      return dict;
    }

#if DEBUG_
    static int ParseObjectCounter;
#endif
    /// <summary>
    /// Parses whatever comes until the specified stop symbol is reached.
    /// </summary>
    void ParseObject(Symbol stop)
    {
#if DEBUG_
      ParseObjectCounter++;
      Debug.WriteLine(ParseObjectCounter.ToString());
      if (ParseObjectCounter == 178)
        GetType();
#endif
      Symbol symbol;
      while ((symbol = ScanNextToken()) != Symbol.Eof)
      {
        if (symbol == stop)
          return;

        switch (symbol)
        {
          case Symbol.Comment:
            // ignore comments
            break;

          case Symbol.Null:
            this.stack.Shift(PdfNull.Value);
            break;

          case Symbol.Boolean:
            this.stack.Shift(new PdfBoolean(this.lexer.TokenToBoolean));
            break;

          case Symbol.Integer:
            this.stack.Shift(new PdfInteger(this.lexer.TokenToInteger));
            break;

          case Symbol.UInteger:
            this.stack.Shift(new PdfUInteger(this.lexer.TokenToUInteger));
            break;

          case Symbol.Real:
            this.stack.Shift(new PdfReal(this.lexer.TokenToReal));
            break;

          case Symbol.String:
            //this.stack.Shift(new PdfString(this.lexer.Token, PdfStringFlags.PDFDocEncoding));
            this.stack.Shift(new PdfString(this.lexer.Token, PdfStringFlags.RawEncoding));
            break;

          case Symbol.UnicodeString:
            this.stack.Shift(new PdfString(this.lexer.Token, PdfStringFlags.Unicode));
            break;

          case Symbol.HexString:
            this.stack.Shift(new PdfString(this.lexer.Token, PdfStringFlags.HexLiteral));
            break;

          case Symbol.UnicodeHexString:
            this.stack.Shift(new PdfString(this.lexer.Token, PdfStringFlags.Unicode | PdfStringFlags.HexLiteral));
            break;

          case Symbol.Name:
            this.stack.Shift(new PdfName(this.lexer.Token));
            break;

          case Symbol.R:
            {
              Debug.Assert(this.stack.GetItem(-1) is PdfInteger && this.stack.GetItem(-2) is PdfInteger);
              PdfObjectID objectID = new PdfObjectID(this.stack.GetInteger(-2), this.stack.GetInteger(-1));

              PdfReference iref = this.document.irefTable[objectID];
              if (iref == null)
              {
                // If a document has more than one PdfXRefTable it is possible that the first trailer has
                // indirect references to objects whos iref entry is not yet read in.
                if (this.document.irefTable.IsUnderConstruction)
                {
                  // XRefTable not complete when trailer is read. Create temporary irefs that are
                  // removed later in PdfTrailer.FixXRefs.
                  iref = new PdfReference(objectID, 0);
                  this.stack.Reduce(iref, 2);
                  break;
                }
                // PDF Reference section 3.2.9:
                // An indirect reference to an undefined object is not an error;
                // it is simply treated as a reference to the null object.
                this.stack.Reduce(PdfNull.Value, 2);
                // Let's see what null objects are good for...
                //Debug.Assert(false, "Null object detected!");
                //this.stack.Reduce(PdfNull.Value, 2);
              }
              else
                this.stack.Reduce(iref, 2);
              break;
            }

          case Symbol.BeginArray:
            PdfArray array = new PdfArray(this.document);
            ReadArray(array, false);
            this.stack.Shift(array);
            break;

          case Symbol.BeginDictionary:
            PdfDictionary dict = new PdfDictionary(this.document);
            ReadDictionary(dict, false);
            this.stack.Shift(dict);
            break;

          case Symbol.BeginStream:
            throw new NotImplementedException();

          default:
            string error = this.lexer.Token;
            Debug.Assert(false, "Unexpected: " + error);
            break;
        }
      }
      throw new PdfReaderException("Unexpected end of file.");
    }

    Symbol ScanNextToken()
    {
      return this.lexer.ScanNextToken();
    }

    //protected Symbol ScanNextToken(bool testReference)
    //{
    //  return this.lexer.ScanNextToken(testReference);
    //}

    Symbol ScanNextToken(out string token)
    {
      Symbol symbol = this.lexer.ScanNextToken();
      token = this.lexer.Token;
      return symbol;
    }

    //protected Symbol ScanNextToken(out string token, bool testReference)
    //{
    //  Symbol symbol = this.lexer.ScanNextToken(testReference);
    //  token = this.lexer.Token;
    //  return symbol;
    //}

    //    internal object ReadObject(int position)
    //    {
    //      this.lexer.Position = position;
    //      return ReadObject(false);
    //    }
    //
    //    internal virtual object ReadObject(bool directObject)
    //    {
    //      throw new InvalidOperationException("PdfParser.ReadObject() base class called");
    //    }

    /// <summary>
    /// Reads the object ID and the generation and sets it into the specified object.
    /// </summary>
    void ReadObjectID(PdfObject obj)
    {
      int objectNubmer = ReadInteger();
      int generationNumber = ReadInteger();
      ReadSymbol(Symbol.Obj);
      if (obj != null)
        obj.SetObjectID(objectNubmer, generationNumber);
    }

    PdfItem ReadReference(PdfReference iref, bool includeReferences)
    {
      throw new NotImplementedException("ReadReference");
    }

    /// <summary>
    /// Reads the next symbol that must be the specified one.
    /// </summary>
    Symbol ReadSymbol(Symbol symbol)
    {
      Symbol current = this.lexer.ScanNextToken();
      if (symbol != current)
        throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
      return current;
    }

    /// <summary>
    /// Reads the next token that must be the specified one.
    /// </summary>
    Symbol ReadToken(string token)
    {
      Symbol current = this.lexer.ScanNextToken();
      if (token != this.lexer.Token)
        throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
      return current;
    }

    /// <summary>
    /// Reads a name from the PDF data stream. The preceding slash is part of the result string.
    /// </summary>
    string ReadName()
    {
      string name;
      Symbol symbol = ScanNextToken(out name);
      if (symbol != Symbol.Name)
        throw new PdfReaderException(PSSR.UnexpectedToken(name));
      return name;
    }
    /*
        /// <summary>
        /// Reads a string immediately or (optionally) indirectly from the PDF data stream.
        /// </summary>
        protected string ReadString(bool canBeIndirect)
        {
          Symbol symbol = Symbol.None; //this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.String || symbol == Symbol.HexString)
            return this.lexer.Token;
          else if (symbol == Symbol.R)
          {
            int position = this.lexer.Position;
            MoveToObject(this.lexer.Token);
            ReadObjectID(null);
            string s = ReadString();
            ReadSymbol(Symbol.EndObj);
            this.lexer.Position = position;
            return s;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        protected string ReadString()
        {
          return ReadString(false);
        }

        /// <summary>
        /// Reads a string immediately or (optionally) indirectly from the PDF data stream.
        /// </summary>
        protected bool ReadBoolean(bool canBeIndirect)
        {
          Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.Boolean)
            return this.lexer.TokenToBoolean;
          else if (symbol == Symbol.R)
          {
            int position = this.lexer.Position;
            MoveToObject(this.lexer.Token);
            ReadObjectID(null);
            bool b = ReadBoolean();
            ReadSymbol(Symbol.EndObj);
            this.lexer.Position = position;
            return b;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        protected bool ReadBoolean()
        {
          return ReadBoolean(false);
        }
    */
    /// <summary>
    /// Reads an integer value directly from the PDF data stream.
    /// </summary>
    int ReadInteger(bool canBeIndirect)
    {
      Symbol symbol = this.lexer.ScanNextToken();
      if (symbol == Symbol.Integer)
        return this.lexer.TokenToInteger;
      else if (symbol == Symbol.R)
      {
        int position = this.lexer.Position;
        //        MoveToObject(this.lexer.Token);
        ReadObjectID(null);
        int n = ReadInteger();
        ReadSymbol(Symbol.EndObj);
        this.lexer.Position = position;
        return n;
      }
      throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
    }

    int ReadInteger()
    {
      return ReadInteger(false);
    }

    //    /// <summary>
    //    /// Reads a real value directly or (optionally) indirectly from the PDF data stream.
    //    /// </summary>
    //    double ReadReal(bool canBeIndirect)
    //    {
    //      Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
    //      if (symbol == Symbol.Real || symbol == Symbol.Integer)
    //        return this.lexer.TokenToReal;
    //      else if (symbol == Symbol.R)
    //      {
    //        int position = this.lexer.Position;
    ////        MoveToObject(this.lexer.Token);
    //        ReadObjectID(null);
    //        double f = ReadReal();
    //        ReadSymbol(Symbol.EndObj);
    //        this.lexer.Position = position;
    //        return f;
    //      }
    //      throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
    //    }
    //
    //    double ReadReal()
    //    {
    //      return ReadReal(false);
    //    }

    //    /// <summary>
    //    /// Reads an object from the PDF input stream. If the object has a specialized parser, it it used.
    //    /// </summary>
    //    public static PdfObject ReadObject(PdfObject pdfObject, PdfObjectID objectID)
    //    {
    //      if (pdfObject == null)
    //        throw new ArgumentNullException("pdfObject");
    //      if (pdfObject.Document == null)
    //        throw new ArgumentException(PSSR.OwningDocumentRequired, "pdfObject");
    //
    //      Type type = pdfObject.GetType();
    //      PdfParser parser = CreateParser(pdfObject.Document, type);
    //      return parser.ReadObject(pdfObject, objectID, false);
    //    }

    /// <summary>
    /// Reads an object from the PDF input stream using the default parser.
    /// </summary>
    public static PdfObject ReadObject(PdfDocument owner, PdfObjectID objectID)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");

      Parser parser = new Parser(owner);
      return parser.ReadObject(null, objectID, false);
    }

    /// <summary>
    /// Reads the iref table and the trailer dictionary.
    /// </summary>
    internal PdfTrailer ReadTrailer()
    {
      //Symbol symbol;
      //string token;
      //int xrefOffset = 0;
      int length = lexer.PdfLength;
#if true
      string trail = this.lexer.ReadRawString(length - 131, 130); //lexer.Pdf.Substring(length - 30);
      int idx = trail.IndexOf("startxref");
      this.lexer.Position = length - 131 + idx;
#else
      string trail = this.lexer.ReadRawString(length - 31, 30); //lexer.Pdf.Substring(length - 30);
      int idx = trail.IndexOf("startxref");
      this.lexer.Position = length - 31 + idx;
#endif
      ReadSymbol(Symbol.StartXRef);
      this.lexer.Position = ReadInteger();

      // Read all trailers
      PdfTrailer trailer;
      while (true)
      {
        trailer = ReadXRefTableAndTrailer(this.document.irefTable);
        // 1st trailer seems to be the best..
        if (this.document.trailer == null)
          this.document.trailer = trailer;
        int prev = trailer.Elements.GetInteger(PdfTrailer.Keys.Prev);
        if (prev == 0)
          break;
        //if (prev > this.lexer.PdfLength)
        //  break;
        this.lexer.Position = prev;
      }

      return this.document.trailer;
    }

    /// <summary>
    /// 
    /// </summary>
    PdfTrailer ReadXRefTableAndTrailer(PdfReferenceTable xrefTable)
    {
      Debug.Assert(xrefTable != null);

      Symbol symbol = ScanNextToken();
      // Is it an xref stream?
      if (symbol == Symbol.Integer)
        throw new PdfReaderException(PSSR.CannotHandleXRefStreams);
      // TODO: It is very high on the todo list, but still undone
      Debug.Assert(symbol == Symbol.XRef);
      while (true)
      {
        symbol = ScanNextToken();
        if (symbol == Symbol.Integer)
        {
          int start = this.lexer.TokenToInteger;
          int length = ReadInteger();
          for (int id = start; id < start + length; id++)
          {
            int position = ReadInteger();
            int generation = ReadInteger();
            ReadSymbol(Symbol.Keyword);
            string token = lexer.Token;
            // Skip start entry
            if (id == 0)
              continue;
            // Skip unused entries.
            if (token != "n")
              continue;
            // Even it is restricted, an object can exists in more than one subsection.
            // (PDF Reference Implementation Notes 15).
            PdfObjectID objectID = new PdfObjectID(id, generation);
            // Ignore the latter one
            if (xrefTable.Contains(objectID))
              continue;
            xrefTable.Add(new PdfReference(objectID, position));
          }
        }
        else if (symbol == Symbol.Trailer)
        {
          ReadSymbol(Symbol.BeginDictionary);
          PdfTrailer trailer = new PdfTrailer(this.document);
          this.ReadDictionary(trailer, false);
          return trailer;
        }
        else
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
      }
    }

    /// <summary>
    /// Parses a PDF date string.
    /// </summary>
    internal static DateTime ParseDateTime(string date, DateTime errorValue)
    {
      DateTime datetime = errorValue;
      try
      {
        if (date.StartsWith("D:"))
        {
          // Format is
          // D:YYYYMMDDHHmmSSOHH'mm'
          //   ^2      ^10   ^16 ^20
          int length = date.Length;
          int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, hh = 0, mm = 0;
          char o = 'Z';
          if (length >= 10)
          {
            year = Int32.Parse(date.Substring(2, 4));
            month = Int32.Parse(date.Substring(6, 2));
            day = Int32.Parse(date.Substring(8, 2));
            if (length >= 16)
            {
              hour = Int32.Parse(date.Substring(10, 2));
              minute = Int32.Parse(date.Substring(12, 2));
              second = Int32.Parse(date.Substring(14, 2));
              if (length >= 23)
              {
                if ((o = date[16]) != 'Z')
                {
                  hh = Int32.Parse(date.Substring(17, 2));
                  mm = Int32.Parse(date.Substring(20, 2));
                }
              }
            }
          }
          datetime = new DateTime(year, month, day, hour, minute, second);
          if (o != 'Z')
          {
            TimeSpan ts = new TimeSpan(hh, mm, 0);
            if (o == '+')
              datetime.Add(ts);
            else
              datetime.Subtract(ts);
          }
        }
        else
        {
          // Some libraries use plain English format.
          datetime = DateTime.Parse(date);
        }
      }
      catch { }
      return datetime;
    }

    //    /// <summary>
    //    /// Creates a parser for the specified PDF object type. A PDF object can define a specialized
    //    /// parser in the optional PdfObjectInfoAttribute. If no parser is specified, the default
    //    /// Parser object is returned.
    //    /// </summary>
    //    public static Parser CreateParser(PdfDocument document, Type pdfObjectType)
    //    {
    //      // TODO: ParserFactory
    //      object[] attribs = null; //pdfObjectType.GetCustomAttributes(typeof(PdfObjectInfoAttribute), false);
    //      if (attribs.Length == 1)
    //      {
    //        PdfObjectInfoAttribute attrib = null; //(PdfObjectInfoAttribute)attribs[0];
    //        Type parserType = attrib.Parser;
    //        if (parserType != null)
    //        {
    //          ConstructorInfo ctorInfo = parserType.GetConstructor(
    //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
    //            new Type[]{typeof(PdfDocument)}, null);
    //          Parser parser = (Parser)ctorInfo.Invoke(new object[]{document});
    //          Debug.Assert(parser != null, "Creation of parser failed.");
    //          return parser;
    //        }
    //      }
    //      return new Parser(document);
    //    }

    /*
        /// <summary>
        /// Reads a date value directly or (optionally) indirectly from the PDF data stream.
        /// </summary>
        protected DateTime ReadDate(bool canBeIndirect)
        {
          Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.String)
          {
            // D:YYYYMMDDHHmmSSOHH'mm'
            //   ^2      ^10   ^16 ^20
            string date = this.lexer.Token;
            int length = date.Length;
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, hh = 0, mm = 0;
            char o = 'Z';
            if (length >= 10)
            {
              year = Int32.Parse(date.Substring(2, 4));
              month = Int32.Parse(date.Substring(6, 2));
              day = Int32.Parse(date.Substring(8, 2));
              if (length >= 16)
              {
                hour = Int32.Parse(date.Substring(10, 2));
                minute = Int32.Parse(date.Substring(12, 2));
                second = Int32.Parse(date.Substring(14, 2));
                if (length >= 23)
                {
                  if ((o = date[16]) != 'Z')
                  {
                    hh = Int32.Parse(date.Substring(17, 2));
                    mm = Int32.Parse(date.Substring(20, 2));
                  }
                }
              }
            }
            DateTime datetime = new DateTime(year, month, day, hour, minute, second);
            if (o != 'Z')
            {
              TimeSpan ts = new TimeSpan(hh, mm, 0);
              if (o == '+')
                datetime.Add(ts);
              else
                datetime.Subtract(ts);
            }
            return datetime;
          }
          else if (symbol == Symbol.R)
          {
            int position = this.lexer.Position;
            MoveToObject(this.lexer.Token);
            ReadObjectID(null);
            DateTime d = ReadDate();
            ReadSymbol(Symbol.EndObj);
            this.lexer.Position = position;
            return d;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        protected DateTime ReadDate()
        {
          return ReadDate(false);
        }

        /// <summary>
        /// Reads a PdfRectangle value directly or (optionally) indirectly from the PDF data stream.
        /// </summary>
        protected PdfRectangle ReadRectangle(bool canBeIndirect)
        {
          Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.BeginArray)
          {
            PdfRectangle rect = new PdfRectangle();
            rect.X1 = ReadReal();
            rect.Y1 = ReadReal();
            rect.X2 = ReadReal();
            rect.Y2 = ReadReal();
            ReadSymbol(Symbol.EndArray);
            return rect;
          }
          else if (symbol == Symbol.R)
          {
            int position = this.lexer.Position;
            MoveToObject(this.lexer.Token);
            ReadObjectID(null);
            PdfRectangle rect = ReadRectangle();
            ReadSymbol(Symbol.EndObj);
            this.lexer.Position = position;
            return rect;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        /// <summary>
        /// Short cut for ReadRectangle(false).
        /// </summary>
        protected PdfRectangle ReadRectangle()
        {
          return ReadRectangle(false);
        }

        /// <summary>
        /// Reads a generic dictionary.
        /// </summary>
        protected PdfDictionary ReadDictionary(bool canBeIndirect)
        {
          // Just read over dictionary
          PdfDictionary dictionary = new PdfDictionary();
          Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.BeginDictionary)
          {
            int nestingLevel = 0;
            symbol = ScanNextToken();
            while (symbol != Symbol.Eof)
            {
              switch (symbol)
              {
                case Symbol.BeginDictionary:
                  nestingLevel++;
                  break;

                case Symbol.EndDictionary:
                  if (nestingLevel == 0)
                    return dictionary;
                  else
                    nestingLevel--;
                  break;
              }
              symbol = ScanNextToken();
            }
            Debug.Assert(false, "Must not come here");
            return dictionary;
          }
          else if (symbol == Symbol.R)
          {
            return dictionary;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        /// <summary>
        /// Short cut for ReadDictionary(false).
        /// </summary>
        protected PdfDictionary ReadDictionary()
        {
          return ReadDictionary(false);
        }

        /// <summary>
        /// Reads a generic array.
        /// </summary>
        protected PdfArray ReadArray(bool canBeIndirect)
        {
          // Just read over array
          PdfArray array = new PdfArray();
          Symbol symbol = this.lexer.ScanNextToken(canBeIndirect);
          if (symbol == Symbol.BeginArray)
          {
            int nestingLevel = 0;
            symbol = ScanNextToken();
            while (symbol != Symbol.Eof)
            {
              switch (symbol)
              {
                case Symbol.BeginArray:
                  nestingLevel++;
                  break;

                case Symbol.EndArray:
                  if (nestingLevel == 0)
                    return array;
                  else
                    nestingLevel--;
                  break;
              }
              symbol = ScanNextToken();
            }
            Debug.Assert(false, "Must not come here");
            return array;
          }
          else if (symbol == Symbol.R)
          {
            return array;
          }
          throw new PdfReaderException(PSSR.UnexpectedToken(this.lexer.Token));
        }

        protected PdfArray ReadArray()
        {
          return ReadArray(false);
        }

        protected object ReadGeneric(KeysMeta meta, string token)
        {
          KeyDescriptor descriptor =  meta[token];
          Debug.Assert(descriptor != null);
          object result = null;
          switch (descriptor.KeyType & KeyType.TypeMask)
          {
            case KeyType.Name:
              result = ReadName();
              break;

            case KeyType.String:
              result = ReadString(descriptor.CanBeIndirect);
              break;

            case KeyType.Boolean:
              result = ReadBoolean(descriptor.CanBeIndirect);
              break;

            case KeyType.Integer:
              result = ReadInteger(descriptor.CanBeIndirect);
              break;

            case KeyType.Real:
              result = ReadReal(descriptor.CanBeIndirect);
              break;

            case KeyType.Date:
              result = ReadDate(descriptor.CanBeIndirect);
              break;

            case KeyType.Rectangle:
              result = ReadRectangle(descriptor.CanBeIndirect);
              break;

            case KeyType.Array:
              result = ReadArray(descriptor.CanBeIndirect);
              break;

            case KeyType.Dictionary:
              result = ReadDictionary(descriptor.CanBeIndirect);
              break;

            case KeyType.Stream:
              break;

            case KeyType.NumberTree:
              throw new NotImplementedException("KeyType.NumberTree");

            case KeyType.NameOrArray:
              char ch = this.lexer.MoveToNonWhiteSpace();
              if (ch == '/')
                result = ReadName();
              else if (ch == '[')
                result = ReadArray();
              else
                throw new NotImplementedException("KeyType.NameOrArray");
              break;

            case KeyType.ArrayOrDictionary:
              throw new NotImplementedException("KeyType.ArrayOrDictionary");
          }
          //Debug.Assert(false, "ReadGeneric");
          return result;
        }

        //    /// <summary>
        //    /// Gets the current symbol from the lexer.
        //    /// </summary>
        //    protected Symbol Symbol
        //    {
        //      get {return lexer.Symbol;}
        //    }
        //
        //    /// <summary>
        //    /// Gets the current token from the lexer.
        //    /// </summary>
        //    protected string Token
        //    {
        //      get {return lexer.Token.ToString();}
        //    }

        public static object Read(PdfObject o, string key)
        {
          return null;
        }
    */

    ParserState SaveState()
    {
      ParserState state = new ParserState();
      state.Position = this.lexer.Position;
      state.Symbol = this.lexer.Symbol;
      return state;
    }

    void RestoreState(ParserState state)
    {
      this.lexer.Position = state.Position;
      this.lexer.Symbol = state.Symbol;
    }

    class ParserState
    {
      public int Position;
      public Symbol Symbol;
    }
  }
}
