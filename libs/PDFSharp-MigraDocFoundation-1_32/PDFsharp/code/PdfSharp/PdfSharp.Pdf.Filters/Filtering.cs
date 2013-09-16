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
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.Filters
{
  /// <summary>
  /// Applies standard filters to streams.
  /// </summary>
  public static class Filtering
  {
    /// <summary>
    /// Gets the filter specified by the case sensitive name.
    /// </summary>
    public static Filter GetFilter(string filterName)
    {
      if (filterName.StartsWith("/"))
        filterName = filterName.Substring(1);

      // Some tools use abbreviations
      switch (filterName)
      {
        case "ASCIIHexDecode":
        case "AHx":
          if (Filtering.asciiHexDecode == null)
            Filtering.asciiHexDecode = new ASCIIHexDecode();
          return Filtering.asciiHexDecode;

        case "ASCII85Decode":
        case "A85":
          if (Filtering.ascii85Decode == null)
            Filtering.ascii85Decode = new ASCII85Decode();
          return Filtering.ascii85Decode;

        case "LZWDecode":
        case "LZW":
          if (Filtering.lzwDecode == null)
            Filtering.lzwDecode = new LzwDecode();
          return Filtering.lzwDecode;

        case "FlateDecode":
        case "Fl":
          if (Filtering.flateDecode == null)
            Filtering.flateDecode = new FlateDecode();
          return Filtering.flateDecode;

        //case "RunLengthDecode":
        //  if (Filtering.RunLengthDecode == null)
        //    Filtering.RunLengthDecode = new RunLengthDecode();
        //  return Filtering.RunLengthDecode;
        //
        //case "CCITTFaxDecode":
        //  if (Filtering.CCITTFaxDecode == null)
        //    Filtering.CCITTFaxDecode = new CCITTFaxDecode();
        //  return Filtering.CCITTFaxDecode;
        //
        //case "JBIG2Decode":
        //  if (Filtering.JBIG2Decode == null)
        //    Filtering.JBIG2Decode = new JBIG2Decode();
        //  return Filtering.JBIG2Decode;
        //
        //case "DCTDecode":
        //  if (Filtering.DCTDecode == null)
        //    Filtering.DCTDecode = new DCTDecode();
        //  return Filtering.DCTDecode;
        //
        //case "JPXDecode":
        //  if (Filtering.JPXDecode == null)
        //    Filtering.JPXDecode = new JPXDecode();
        //  return Filtering.JPXDecode;
        //
        //case "Crypt":
        //  if (Filtering.Crypt == null)
        //    Filtering.Crypt = new Crypt();
        //  return Filtering.Crypt;

        case "RunLengthDecode":
        case "CCITTFaxDecode":
        case "JBIG2Decode":
        case "DCTDecode":
        case "JPXDecode":
        case "Crypt":
          Debug.WriteLine("Filter not implemented: " + filterName);
          return null;
      }
      throw new NotImplementedException("Unknown filter: " + filterName);
    }

    /// <summary>
    /// Gets the filter singleton.
    /// </summary>
    public static ASCIIHexDecode ASCIIHexDecode
    {
      get
      {
        if (Filtering.asciiHexDecode == null)
          Filtering.asciiHexDecode = new ASCIIHexDecode();
        return Filtering.asciiHexDecode;
      }
    }
    static ASCIIHexDecode asciiHexDecode;

    /// <summary>
    /// Gets the filter singleton.
    /// </summary>
    public static ASCII85Decode ASCII85Decode
    {
      get
      {
        if (Filtering.ascii85Decode == null)
          Filtering.ascii85Decode = new ASCII85Decode();
        return Filtering.ascii85Decode;
      }
    }
    static ASCII85Decode ascii85Decode;

    /// <summary>
    /// Gets the filter singleton.
    /// </summary>
    public static LzwDecode LzwDecode
    {
      get
      {
        if (Filtering.lzwDecode == null)
          Filtering.lzwDecode = new LzwDecode();
        return Filtering.lzwDecode;
      }
    }
    static LzwDecode lzwDecode;

    /// <summary>
    /// Gets the filter singleton.
    /// </summary>
    public static FlateDecode FlateDecode
    {
      get
      {
        if (Filtering.flateDecode == null)
          Filtering.flateDecode = new FlateDecode();
        return Filtering.flateDecode;
      }
    }
    static FlateDecode flateDecode;

    //runLengthDecode
    //ccittFaxDecode
    //jbig2Decode
    //dctDecode
    //jpxDecode
    //crypt

    /// <summary>
    /// Encodes the data with the specified filter.
    /// </summary>
    public static byte[] Encode(byte[] data, string filterName)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.Encode(data);
      return null;
    }

    /// <summary>
    /// Encodes a raw string with the specified filter.
    /// </summary>
    public static byte[] Encode(string rawString, string filterName)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.Encode(rawString);
      return null;
    }

    /// <summary>
    /// Decodes the data with the specified filter.
    /// </summary>
    public static byte[] Decode(byte[] data, string filterName, FilterParms parms)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.Decode(data, parms);
      return null;
    }

    /// <summary>
    /// Decodes the data with the specified filter.
    /// </summary>
    public static byte[] Decode(byte[] data, string filterName)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.Decode(data, null);
      return null;
    }

    /// <summary>
    /// Decodes the data with the specified filter.
    /// </summary>
    public static byte[] Decode(byte[] data, PdfItem filterItem)
    {
      byte[] result = null;
      if (filterItem is PdfName)
      {
        Filter filter = GetFilter(filterItem.ToString());
        if (filter != null)
          result = filter.Decode(data);
      }
      else if (filterItem is PdfArray)
      {
        PdfArray array = (PdfArray)filterItem;
        foreach (PdfItem item in array)
          data = Filtering.Decode(data, item);
        result = data;
      }
      return result;
    }

    /// <summary>
    /// Decodes to a raw string with the specified filter.
    /// </summary>
    public static string DecodeToString(byte[] data, string filterName, FilterParms parms)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.DecodeToString(data, parms);
      return null;
    }

    /// <summary>
    /// Decodes to a raw string with the specified filter.
    /// </summary>
    public static string DecodeToString(byte[] data, string filterName)
    {
      Filter filter = GetFilter(filterName);
      if (filter != null)
        return filter.DecodeToString(data, null);
      return null;
    }
  }
}
