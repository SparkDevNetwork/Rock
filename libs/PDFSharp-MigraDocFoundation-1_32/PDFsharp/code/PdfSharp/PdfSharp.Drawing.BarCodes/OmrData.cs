#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using System.Collections;
using System.Text;

namespace PdfSharp.Drawing.BarCodes
{
#if _
  /// <summary>
  /// Represents the data coded within the OMR code.
  /// </summary>
  class OmrData
  {
    private OmrData()
    {
    }

    public static OmrData ForTesting
    {
      get
      {
        OmrData data = new OmrData();
        data.AddMarkDescription("LK");
        data.AddMarkDescription("DGR");
        data.AddMarkDescription("GM1");
        data.AddMarkDescription("GM2");
        data.AddMarkDescription("GM4");
        data.AddMarkDescription("GM8");
        data.AddMarkDescription("GM16");
        data.AddMarkDescription("GM32");
        data.AddMarkDescription("ZS1");
        data.AddMarkDescription("ZS2");
        data.AddMarkDescription("ZS3");
        data.AddMarkDescription("ZS4");
        data.AddMarkDescription("ZS5");
        data.InitMarks();
        return data;
      }
    }

    ///// <summary>
    ///// NYI: Get OMR description read from text file.
    ///// </summary>
    ///// <returns>An OmrData object.</returns>
    //public static OmrData FromDescriptionFile(string filename)
    //{
    //  throw new NotImplementedException();
    //}

    /// <summary>
    /// Adds a mark description by name.
    /// </summary>
    /// <param name="name">The name to for setting or unsetting the mark.</param>
    private void AddMarkDescription(string name)
    {
      if (this.marksInitialized)
        throw new InvalidOperationException(BcgSR.OmrAlreadyInitialized);

      this.nameToIndex[name] = this.addedDescriptions;
      ++this.addedDescriptions;
    }

    private void InitMarks()
    {
      if (this.addedDescriptions == 0)
        throw new InvalidOperationException();

      this.marks = new bool[this.addedDescriptions];
      this.marks.Initialize();
      this.marksInitialized = true;
    }

    private int FindIndex(string name)
    {
      if (!this.marksInitialized)
        InitMarks();

      if (!this.nameToIndex.Contains(name))
        throw new ArgumentException(BcgSR.InvalidMarkName(name));

      return (int)this.nameToIndex[name];
    }

    public void SetMark(string name)
    {
      int idx = FindIndex(name);
      this.marks[idx] = true;
    }

    public void UnsetMark(string name)
    {
      int idx = FindIndex(name);
      this.marks[idx] = false;
    }

    public bool[] Marks
    {
      get { return this.marks; }
    }
    System.Collections.Hashtable nameToIndex = new Hashtable();
    bool[] marks;
    int addedDescriptions = 0;
    bool marksInitialized = false;
  }
#endif
}
