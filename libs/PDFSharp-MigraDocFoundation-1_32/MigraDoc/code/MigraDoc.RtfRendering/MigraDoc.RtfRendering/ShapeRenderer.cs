#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using System.Diagnostics;
using System.Globalization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Base class for Renderers that render shapes (images, textframes, charts) to RTF.
  /// </summary>
  internal abstract class ShapeRenderer : RendererBase
  {
    internal ShapeRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.shape = domObj as Shape;
    }

    /// <summary>
    /// Starts the area for a common shape description in RTF.
    /// </summary>
    protected virtual void StartShapeArea()
    {
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("shp");
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControlWithStar("shpinst");
      RenderShapeAttributes();
    }

    /// <summary>
    /// Renders attributes that belong to a shape.
    /// </summary>
    private void RenderShapeAttributes()
    {
      RenderTopPosition();
      RenderLeftPosition();

      if (DocumentRelations.HasParentOfType(this.shape, typeof(HeaderFooter)))
        this.rtfWriter.WriteControl("shpfhdr", "1");
      else
        this.rtfWriter.WriteControl("shpfhdr", "0");
      RenderWrapFormat();
      RenderRelativeHorizontal();
      RenderRelativeVertical();
      if (RenderInParagraph())
      {
        this.rtfWriter.WriteControl("shplockanchor");
        RenderNameValuePair("fPseudoInline", "1");
      }
      RenderLineFormat();
      RenderFillFormat();
    }

    /// <summary>
    /// Renders the fill format of the shape.
    /// </summary>
    protected void RenderFillFormat()
    {
      FillFormat ff = GetValueAsIntended("FillFormat") as FillFormat;
      if (ff != null && (ff.IsNull("Visible") || ff.Visible == true))
      {
        RenderNameValuePair("fFilled", "1");
        TranslateAsNameValuePair("FillFormat.Color", "fillColor", RtfUnit.Undefined, null);
      }
      else
        RenderNameValuePair("fFilled", "0");
    }

    protected Unit GetLineWidth()
    {
      LineFormat lf = GetValueAsIntended("LineFormat") as LineFormat;
      if (lf != null && (lf.IsNull("Visible") || lf.Visible == true))
      {
        if (lf.IsNull("Width"))
          return 1;
        else
          return lf.Width;
      }
      return 0;
    }
    /// <summary>
    /// Renders the line format of the shape.
    /// </summary>
    protected void RenderLineFormat()
    {
      LineFormat lf = GetValueAsIntended("LineFormat") as LineFormat;
      if (lf != null && (lf.IsNull("Visible") || lf.Visible == true))
      {
        RenderNameValuePair("fLine", "1");
        TranslateAsNameValuePair("LineFormat.Color", "lineColor", RtfUnit.Undefined, "0");
        TranslateAsNameValuePair("LineFormat.Width", "lineWidth", RtfUnit.EMU, ToEmu(1).ToString(CultureInfo.InvariantCulture));
        TranslateAsNameValuePair("LineFormat.DashStyle", "lineDashing", RtfUnit.Undefined, "0");
      }
      else
        RenderNameValuePair("fLine", "0");
    }


    /// <summary>
    /// Renders the shape's Left attribute by setting the \posv, \shptop and \shpbottom RTF controls.
    /// </summary>
    protected void RenderTopPosition()
    {
      RenderTopBottom();
      string topValue = "";
      ShapePosition topSP = this.shape.Top.ShapePosition;
      switch (topSP)
      {
        case ShapePosition.Top:
          //WrapFormat.DistanceTop is used slightly different in the rendering module than in word.
          //It must be taken into account for the top value.
          object wrapTop = GetValueAsIntended("WrapFormat.DistanceTop");
          if (wrapTop == null || ((Unit)wrapTop).Point == 0)
            topValue = "1";
          break;

        case ShapePosition.Center:
          topValue = "2";
          break;

        case ShapePosition.Bottom:
          //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
          //It must be taken into account for the bottom value.
          object wrapBottom = GetValueAsIntended("WrapFormat.DistanceBottom");
          if (wrapBottom == null || ((Unit)wrapBottom).Point == 0)
            topValue = "3";
          break;
      }
      if (topValue != "" && !RenderInParagraph())
        RenderNameValuePair("posv", topValue);
    }


    /// <summary>
    /// Renders the shape's Left attribute by setting the \posh, \shpleft and \shpright RTF controls.
    /// </summary>
    protected void RenderLeftPosition()
    {
      RenderLeftRight();
      ShapePosition leftSP = this.shape.Left.ShapePosition;
      string leftValue = "";
      switch (leftSP)
      {
        case ShapePosition.Left:

          //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
          //It must be taken into account for the left value.
          object wrapLeft = GetValueAsIntended("WrapFormat.DistanceLeft");
          if (wrapLeft == null || ((Unit)wrapLeft).Point == 0)
            leftValue = "1";
          break;
        case ShapePosition.Center:
          leftValue = "2";
          break;
        case ShapePosition.Right:

          //WrapFormat.DistanceBottom is used slightly different in the rendering module than in word.
          //It must be taken into account for the right value.
          object wrapRight = GetValueAsIntended("WrapFormat.DistanceRight");
          if (wrapRight == null || ((Unit)wrapRight).Point == 0)
            leftValue = "3";
          break;
      }
      if (leftValue != "" && !RenderInParagraph())
        RenderNameValuePair("posh", leftValue);
    }

    /// <summary>
    /// Gets the user defined shape height.
    /// </summary>
    protected virtual Unit GetShapeHeight()
    {
      return this.shape.Height;
    }


    /// <summary>
    /// Gets the user defined shape width.
    /// </summary>
    protected virtual Unit GetShapeWidth()
    {
      return this.shape.Width;
    }

    protected virtual void EndShapeArea()
    {
      this.rtfWriter.EndContent();
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// A shape that shall be placed between its predecessor and its successor must be embedded in a paragraph.
    /// </summary>
    protected virtual bool RenderInParagraph()
    {
      if (this.shape.IsNull("RelativeVertical") || this.shape.RelativeVertical == RelativeVertical.Line || this.shape.RelativeVertical == RelativeVertical.Paragraph)
      {

        DocumentObjectCollection docObjects = DocumentRelations.GetParent(this.shape) as DocumentObjectCollection;
        if (DocumentRelations.GetParent(docObjects) is Paragraph)//don't embed it twice!
          return false;

        return (shape.IsNull("WrapFormat.Style") || shape.WrapFormat.Style == WrapStyle.TopBottom);
      }

      return false;
    }

    /// <summary>
    /// Renders the dummy paragraph's attributes.
    /// </summary>
    protected virtual void RenderParagraphAttributes()
    {
      bool isInCell = DocumentRelations.HasParentOfType(this.shape, typeof(Cell));
      if (isInCell)
        this.rtfWriter.WriteControl("intbl");

      RenderParagraphAlignment();
      RenderParagraphIndents();
      RenderParagraphDistances();
    }


    /// <summary>
    /// Renders the dummy paragraph's space before and space after attributes.
    /// </summary>
    private void RenderParagraphDistances()
    {
      Unit spaceAfter = 0;
      Unit spaceBefore = 0;
      TopPosition top = (TopPosition)GetValueOrDefault("Top", new TopPosition());
      if (top.ShapePosition == ShapePosition.Undefined)
      {
        spaceBefore = top.Position + (Unit)GetValueOrDefault("WrapFormat.DistanceTop", (Unit)0);
      }
      spaceAfter = (Unit)GetValueOrDefault("WrapFormat.DistanceBottom", (Unit)0);
      RenderUnit("sa", spaceAfter);
      RenderUnit("sb", spaceBefore);
    }

    /// <summary>
    /// Renders the dummy paragraph's Alignment taking into account the shape's Left property.
    /// </summary>
    private void RenderParagraphAlignment()
    {
      if (!this.shape.IsNull("Left"))
      {
        LeftPosition leftPos = this.shape.Left.ShapePosition;
        if (leftPos.ShapePosition != ShapePosition.Undefined)
        {
          switch (leftPos.ShapePosition)
          {
            case ShapePosition.Right:
              this.rtfWriter.WriteControl("q", "r");
              break;

            case ShapePosition.Center:
              this.rtfWriter.WriteControl("q", "c");
              break;

            default:
              this.rtfWriter.WriteControl("q", "l");
              break;
          }
        }
      }
    }

    /// <summary>
    /// Renders the RelativeHorizontal attribute.
    /// </summary>
    void RenderRelativeHorizontal()
    {
      if (RenderInParagraph())
      {
        this.rtfWriter.WriteControl("shpbx", "para");
        this.rtfWriter.WriteControl("shpbx", "ignore");
        RenderNameValuePair("posrelh", "3");
      }
      else
      {
        //We need to write both shpbx and posrelh which are almost equivalent.
        Translate("RelativeHorizontal", "shpbx", RtfUnit.Undefined, "margin", false);
        this.rtfWriter.WriteControl("shpbx", "ignore");

        object relHorObj = GetValueAsIntended("RelativeHorizontal");
        RelativeHorizontal relHor = relHorObj == null ? RelativeHorizontal.Margin : (RelativeHorizontal)relHorObj;
        switch (relHor)
        {
          case RelativeHorizontal.Character:
            RenderNameValuePair("posrelh", "3");
            break;
          case RelativeHorizontal.Column:
            RenderNameValuePair("posrelh", "2");
            break;
          case RelativeHorizontal.Margin:
            RenderNameValuePair("posrelh", "0");
            break;
          case RelativeHorizontal.Page:
            RenderNameValuePair("posrelh", "1");
            break;

        }
      }
    }


    /// <summary>
    /// Renders the RelativeVerticalattribute.
    /// </summary>
    void RenderRelativeVertical()
    {
      if (RenderInParagraph())
      {
        this.rtfWriter.WriteControl("shpby", "para");
        this.rtfWriter.WriteControl("shpby", "ignore");
        RenderNameValuePair("posrelv", "3");
      }
      else
      {
        //We need to write both shpby and posrelv which are almost equivalent.
        Translate("RelativeVertical", "shpby", RtfUnit.Undefined, "para", false);
        this.rtfWriter.WriteControl("shpby", "ignore");
        object relVrtObj = GetValueAsIntended("RelativeVertical");
        RelativeVertical relVrt = relVrtObj == null ? RelativeVertical.Paragraph : (RelativeVertical)relVrtObj;

        switch (relVrt)
        {
          case RelativeVertical.Line:
            RenderNameValuePair("posrelv", "3");
            break;
          case RelativeVertical.Margin:
            RenderNameValuePair("posrelv", "0");
            break;

          case RelativeVertical.Page:
            RenderNameValuePair("posrelv", "1");
            break;

          case RelativeVertical.Paragraph:
            RenderNameValuePair("posrelv", "2");
            break;
        }
      }
    }

    /// <summary>
    /// Renders the WrapFormat.
    /// </summary>
    void RenderWrapFormat()
    {
      if (!RenderInParagraph())
      {
        Translate("WrapFormat.Style", "shpwr", RtfUnit.Undefined, "3", false);

        //REM: Distances don't work using them like this:
        /*
        TranslateAsNameValuePair("WrapFormat.DistanceTop", "dyWrapDistTop", RtfUnit.EMU, null);
        TranslateAsNameValuePair("WrapFormat.DistanceBottom", "dyWrapDistBottom", RtfUnit.EMU, null);
        TranslateAsNameValuePair("WrapFormat.DistanceLeft", "dxWrapDistLeft", RtfUnit.EMU, null);
        TranslateAsNameValuePair("WrapFormat.DistanceRight", "dxWrapDistRight", RtfUnit.EMU, null);
        */
      }
      else
      {
        //REM: Might not be necessary.
        this.rtfWriter.WriteControl("shpwrk", "0");
        this.rtfWriter.WriteControl("shpwr", "3");
      }
    }

    /// <summary>
    /// Renders the dummy paragraph's left indent.
    /// </summary>
    void RenderParagraphIndents()
    {
      object relHor = GetValueAsIntended("RelativeHorizontal");
      double leftInd = 0;
      double rightInd = 0;
      if (relHor != null && (RelativeHorizontal)relHor == RelativeHorizontal.Page)
      {
        Section parentSec = (Section)DocumentRelations.GetParentOfType(this.shape, typeof(Section));
        Unit leftPgMrg = (Unit)parentSec.PageSetup.GetValue("LeftMargin", GV.ReadOnly);
        leftInd = -leftPgMrg.Point;
        Unit rightPgMrg = (Unit)parentSec.PageSetup.GetValue("RightMargin", GV.ReadOnly);
        rightInd = -rightPgMrg;
      }

      LeftPosition leftPos = (LeftPosition)GetValueOrDefault("Left", new LeftPosition());
      switch (leftPos.ShapePosition)
      {
        case ShapePosition.Undefined:
          leftInd += leftPos.Position;
          leftInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0)).Point;
          break;

        case ShapePosition.Left:
          leftInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0)).Point;
          break;

        case ShapePosition.Right:
          rightInd += ((Unit)GetValueOrDefault("WrapFormat.DistanceRight", (Unit)0)).Point;
          break;
      }
      RenderUnit("li", leftInd);
      RenderUnit("lin", leftInd);
      RenderUnit("ri", rightInd);
      RenderUnit("rin", rightInd);
    }

    /// <summary>
    /// Renders (and calculates) the \shptop and \shpbottom controls in RTF.
    /// </summary>
    private void RenderTopBottom()
    {
      Unit height = GetShapeHeight();
      Unit top = 0;
      Unit bottom = height;

      if (!RenderInParagraph())
      {
        RelativeVertical relVert = (RelativeVertical)GetValueOrDefault("RelativeVertical", RelativeVertical.Paragraph);
        TopPosition topPos = (TopPosition)GetValueOrDefault("Top", new TopPosition());
        //REM: Will not work like this in table cells.
        //=>The shape would have to be put in a paragraph there.
        Section sec = (Section)DocumentRelations.GetParentOfType(this.shape, typeof(Section));

        PageSetup pgStp = sec.PageSetup;
        Unit topMrg = (Unit)pgStp.GetValue("TopMargin", GV.ReadOnly);
        Unit btmMrg = (Unit)pgStp.GetValue("BottomMargin", GV.ReadOnly);
        Unit pgHeight = pgStp.PageHeight;
        Unit pgWidth = pgStp.PageWidth;

        if (topPos.ShapePosition == ShapePosition.Undefined)
        {
          top = topPos.Position;
          bottom = top + height;
        }

        else
        {
          switch (relVert)
          {
            case RelativeVertical.Line:
              AlignVertically(topPos.ShapePosition, height, out top, out bottom);
              break;

            case RelativeVertical.Margin:
              AlignVertically(topPos.ShapePosition, pgHeight.Point - topMrg.Point - btmMrg.Point, out top, out bottom);
              break;

            case RelativeVertical.Page:
              AlignVertically(topPos.ShapePosition, pgHeight, out top, out bottom);
              break;
          }
        }
      }
      RenderUnit("shptop", top);
      RenderUnit("shpbottom", bottom);
    }

    /// <summary>
    /// Aligns the given top and bottom position so that ShapePosition.Top results in top position = 0.
    /// </summary>
    private void AlignVertically(ShapePosition shpPos, Unit distanceTopBottom, out Unit topValue, out Unit bottomValue)
    {
      double height = GetShapeHeight().Point;
      topValue = 0;
      bottomValue = height;
      Unit topWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceTop", (Unit)0);
      Unit bottomWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceBottom", (Unit)0);
      switch (shpPos)
      {
        case ShapePosition.Bottom:
          topValue = distanceTopBottom - height - bottomWrap;
          bottomValue = distanceTopBottom - bottomWrap;
          break;

        case ShapePosition.Center:
          {
            Unit centerPos = distanceTopBottom / 2.0;
            topValue = centerPos - height / 2.0;
            bottomValue = centerPos + height / 2.0;
          }
          break;

        case ShapePosition.Top:
          topValue = topWrap;
          bottomValue = topWrap + height;
          break;
      }
    }

    /// <summary>
    /// Renders (and calculates) the \shpleft and \shpright controls in RTF.
    /// </summary>
    private void RenderLeftRight()
    {
      Unit width = GetShapeWidth();
      Unit left = 0;
      Unit right = width;

      if (!RenderInParagraph())
      {
        RelativeHorizontal relHor = (RelativeHorizontal)GetValueOrDefault("RelativeHorizontal", RelativeHorizontal.Margin);
        LeftPosition leftPos = (LeftPosition)GetValueOrDefault("Left", new LeftPosition());
        //REM: Will not work like this in table cells.
        //=>The shape would have to be put in a paragraph there.

        Section sec = (Section)DocumentRelations.GetParentOfType(this.shape, typeof(Section));
        PageSetup pgStp = sec.PageSetup;
        Unit leftMrg = (Unit)pgStp.GetValue("LeftMargin", GV.ReadOnly);
        Unit rgtMrg = (Unit)pgStp.GetValue("RightMargin", GV.ReadOnly);
        Unit pgHeight = pgStp.PageHeight;
        Unit pgWidth = pgStp.PageWidth;

        if (leftPos.ShapePosition == ShapePosition.Undefined)
        {
          left = leftPos.Position;
          right = left + width;
        }

        else
        {
          switch (relHor)
          {
            case RelativeHorizontal.Column:
            case RelativeHorizontal.Character:
            case RelativeHorizontal.Margin:
              AlignHorizontally(leftPos.ShapePosition, pgWidth.Point - leftMrg.Point - rgtMrg.Point, out left, out right);
              break;

            case RelativeHorizontal.Page:
              AlignHorizontally(leftPos.ShapePosition, pgWidth, out left, out right);
              break;
          }
        }
      }
      RenderUnit("shpleft", left);
      RenderUnit("shpright", right);
    }

    /// <summary>
    /// Aligns the given left and right position so that ShapePosition.Left results in left position = 0.
    /// </summary>
    private void AlignHorizontally(ShapePosition shpPos, Unit distanceLeftRight, out Unit leftValue, out Unit rightValue)
    {
      double width = GetShapeWidth().Point;
      leftValue = 0;
      rightValue = width;
      Unit leftWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceLeft", (Unit)0);
      Unit rightWrap = (Unit)GetValueOrDefault("WrapFormat.DistanceRight", (Unit)0);
      switch (shpPos)
      {
        case ShapePosition.Right:
        //Positioning the shape Outside seems impossible=>Do the best that's possible.
        case ShapePosition.Outside:
          leftValue = distanceLeftRight.Point - width - rightWrap;
          rightValue = distanceLeftRight - rightWrap;
          break;

        case ShapePosition.Center:
          {
            double centerPos = distanceLeftRight.Point / 2;
            leftValue = centerPos - width / 2.0;
            rightValue = centerPos + width / 2.0;
          }
          break;

        case ShapePosition.Left:
        //Positioning the shape inside seems impossible=>Do the best that's possible.
        case ShapePosition.Inside:
          leftValue = leftWrap;
          rightValue = leftWrap + width;
          break;
      }
    }

    /// <summary>
    /// Renders a name value pair as shape property to RTF.
    /// </summary>
    protected void RenderNameValuePair(string name, string value)
    {
      StartNameValuePair(name);
      this.rtfWriter.WriteText(value);
      EndNameValuePair();
    }

    /// <summary>
    /// Renders name as the beginning of a shape's name value pair to RTF.
    /// Used in the order StartNameValuePair &lt;value rendering&gt; EndNameValuePair.
    /// </summary>
    protected void StartNameValuePair(string name)
    {
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("sp");
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("sn");
      this.rtfWriter.WriteText(name);
      this.rtfWriter.EndContent();
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("sv");
    }

    /// <summary>
    /// Renders the end of a shape's name value pair.
    /// Used in the order StartNameValuePair &lt;value rendering&gt; EndNameValuePair.
    /// </summary>
    protected void EndNameValuePair()
    {
      this.rtfWriter.EndContent();
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Translates a value as a shape's name value pair to RTF.
    /// </summary>
    protected void TranslateAsNameValuePair(string domValueName, string rtfName, RtfUnit unit, string defaultValue)
    {
      object val = GetValueAsIntended(domValueName);
      if (val == null && defaultValue == null)
        return;

      string valueStr = "";
      if (val == null)
        valueStr = defaultValue;
      else
      {
        if (val is Unit)
          valueStr = ToRtfUnit((Unit)val, unit).ToString(CultureInfo.InvariantCulture);
        else if (val is Color)
        {
          Color col = (Color)val;
          col = col.GetMixedTransparencyColor();
          valueStr = (col.R + (col.G * 256) + (col.B * 65536)).ToString(CultureInfo.InvariantCulture);
        }
        else if (val is Enum)
          valueStr = enumTranslationTable[val].ToString();
        else if (val is bool)
          valueStr = (bool)val ? "1" : "0";
        else
          Debug.Assert(false);
      }
      RenderNameValuePair(rtfName, valueStr);
    }

    /// <summary>
    /// Starts a dummy paragraph to put a shape in, which is wrapped TopBottom style.
    /// </summary>
    protected virtual void StartDummyParagraph()
    {
      this.rtfWriter.WriteControl("pard");
      RenderParagraphAttributes();
    }


    /// <summary>
    /// Ends a dummy paragraph to put a shape in, which is wrapped TopBottom style.
    /// </summary>
    protected virtual void EndDummyParagraph()
    {
      this.rtfWriter.WriteControl("par");
    }
    private Shape shape;
  }
}
