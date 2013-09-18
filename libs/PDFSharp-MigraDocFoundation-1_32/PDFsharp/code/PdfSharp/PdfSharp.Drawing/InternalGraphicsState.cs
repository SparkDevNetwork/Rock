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
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
  // In GDI+ the functions Save/Restore, BeginContainer/EndContainer, Transform, SetClip and ResetClip
  // can be combined in any order. E.g. you can set a clip region, save the graphics state, empty the
  // clip region and draw without clipping. Then you can restore to the previous clip region. With PDF
  // this behavior is hard to implement. To solve this problem I first an automaton that keeps track
  // of all clipping paths and the current transformation when the clip path was set. The automation
  // manages a PDF graphics state stack to calculate the desired bahaviour. It also takes into consideration
  // not to multiply with inverse matrixes when the user sets a new transformation matrix.
  // After the design works on pager I decided not to implement it because it is much to large-scale.
  // Instead I lay down some rules how to use the XGraphics class.
  //
  // * Before you set a transformation matrix save the graphics state (Save) or begin a new container
  //   (BeginContainer).
  // 
  // * Instead of resetting the transformation matrix, call Restore or EndContainer. If you reset the
  //   transformation, in PDF must be multiplied with the inverse matrix. That leads to round off errors
  //   because in PDF file only 3 digits are used and Acrobat internally uses fixed point numbers (until
  //   versioin 6 or 7 I think).
  //
  // * When no clip path is defined, you can set or intersect a new path.
  //
  // * When a clip path is already defined, you can always intersect with a new one (wich leads in general
  //   to a smaller clip region).
  //
  // * When a clip path is already defined, you can only reset it to the empty region (ResetClip) when
  //   the graphics state stack is at the same position as it had when the clip path was defined. Otherwise
  //   an error occurs.
  //
  // Keeping these rules leads to easy to read code and best results in PDF output.

  /// <summary>
  /// Represents the internal state of an XGraphics object.
  /// </summary>
  internal class InternalGraphicsState
  {
    public InternalGraphicsState(XGraphics gfx)
    {
      this.gfx = gfx;
    }

    public InternalGraphicsState(XGraphics gfx, XGraphicsState state)
    {
      this.gfx = gfx;
      this.state = state;
      state.InternalState = this;
      //#if GDI
      //      //GdiGraphicsState = state.GdiState;
      //      this.gfx = gfx;
      //      this.state = state;
      //      state.InternalState = this;
      //#endif
      //#if WPF
      //      this.gfx = gfx;
      //      this.state = state;
      //      state.InternalState = this;
      //#endif
    }

    public InternalGraphicsState(XGraphics gfx, XGraphicsContainer container)
    {
      this.gfx = gfx;
      container.InternalState = this;
      //#if GDI
      //      //GdiGraphicsState = container.GdiState;
      //      this.gfx = gfx;
      //      container.InternalState = this;
      //#endif
      //#if WPF
      //      this.gfx = gfx;
      //      container.InternalState = this;
      //#endif
    }

    /// <summary>
    /// Gets or sets the current transformation matrix.
    /// </summary>
    public XMatrix Transform
    {
      get { return this.transform; }
      set { this.transform = value; }
    }
    XMatrix transform = new XMatrix();  //XMatrix.Identity;

    public void Pushed()
    {
#if GDI
#endif
#if WPF
#endif
    }

    public void Popped()
    {
      this.invalid = true;
#if GDI
#endif
#if WPF
      if (this.gfx.targetContext == XGraphicTargetContext.WPF)
      {
        for (int idx = 0; idx < this.transformPushLevel; idx++)
          this.gfx.dc.Pop();
        this.transformPushLevel = 0;
        for (int idx = 0; idx < this.geometryPushLevel; idx++)
          this.gfx.dc.Pop();
        this.geometryPushLevel = 0;
      }
#endif
    }

    internal bool invalid;

#if GDI_
    /// <summary>
    /// The GDI+ GraphicsState if contructed from XGraphicsState.
    /// </summary>
    public GraphicsState GdiGraphicsState;
#endif

#if WPF
    public void SetTransform(MatrixTransform transform)
    {
      this.gfx.dc.PushTransform(transform);
      this.transformPushLevel++;
    }
    int transformPushLevel;

    public void SetClip(Geometry geometry)
    {
      this.gfx.dc.PushClip(geometry);
      this.geometryPushLevel++;
    }
    int geometryPushLevel;

#endif

    internal XGraphics gfx;
    internal XGraphicsState state;
    // /// <summary>
    // /// The GDI+ GraphicsContainer if contructed from XGraphicsContainer.
    // /// </summary>
    // public GraphicsContainer GdiGraphicsContainer;
  }
}
