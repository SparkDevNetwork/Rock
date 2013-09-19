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

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Specifies the type of a key's value in a dictionary.
  /// </summary>
  [Flags]
  internal enum KeyType
  {
    Name = 0x00000001,
    String = 0x00000002,
    Boolean = 0x00000003,
    Integer = 0x00000004,
    Real = 0x00000005,
    Date = 0x00000006,
    Rectangle = 0x00000007,
    Array = 0x00000008,
    Dictionary = 0x00000009,
    Stream = 0x0000000A,
    NumberTree = 0x0000000B,
    Function = 0x0000000C,
    TextString = 0x0000000D,

    NameOrArray = 0x00000010,
    NameOrDictionary = 0x00000020,
    ArrayOrDictionary = 0x00000030,
    StreamOrArray = 0x00000040,
    StreamOrName = 0x00000050,
    ArrayOrNameOrString = 0x00000060,
    FunctionOrName = 0x000000070,
    Various = 0x000000080,

    TypeMask = 0x000000FF,

    Optional = 0x00000100,
    Required = 0x00000200,
    Inheritable = 0x00000400,
    MustBeIndirect = 0x00001000,
    MustNotBeIndirect = 0x00002000,
  }

  /// <summary>
  /// Summary description for KeyInfo.
  /// </summary>
  internal class KeyInfoAttribute : Attribute
  {
    public KeyInfoAttribute()
    {
    }

    public KeyInfoAttribute(KeyType keyType)
    {
      //this.version = version;
      this.KeyType = keyType;
    }

    public KeyInfoAttribute(string version, KeyType keyType)
    {
      this.version = version;
      this.KeyType = keyType;
    }

    public KeyInfoAttribute(KeyType keyType, Type objectType)
    {
      //this.version = version;
      this.KeyType = keyType;
      this.objectType = objectType;
    }

    public KeyInfoAttribute(string version, KeyType keyType, Type objectType)
    {
      //this.version = version;
      this.KeyType = keyType;
      this.objectType = objectType;
    }

    public string Version
    {
      get { return this.version; }
      set { this.version = value; }
    }
    string version = "1.0";

    public KeyType KeyType
    {
      get { return this.entryType; }
      set { this.entryType = value; }
    }
    KeyType entryType;

    public Type ObjectType
    {
      get { return this.objectType; }
      set { this.objectType = value; }
    }
    Type objectType;

    public string FixedValue
    {
      get { return this.fixedValue; }
      set { this.fixedValue = value; }
    }
    string fixedValue;
  }
}