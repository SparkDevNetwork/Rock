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
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Summary description for ChartRenderer.
  /// </summary>
  internal class ChartRenderer : ShapeRenderer
  {
    internal ChartRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.chart = (Chart)domObj;
      this.isInline = DocumentRelations.HasParentOfType(this.chart, typeof(Paragraph)) ||
        RenderInParagraph();
    }

    /// <summary>
    /// Renders an image to RTF.
    /// </summary>
    internal override void Render()
    {
      string fileName = System.IO.Path.GetTempFileName();
      if (!StoreTempImage(fileName))
        return;

      bool renderInParagraph = RenderInParagraph();
      DocumentElements elms = DocumentRelations.GetParent(this.chart) as DocumentElements;
      if (elms != null && !renderInParagraph && !(DocumentRelations.GetParent(elms) is Section || DocumentRelations.GetParent(elms) is HeaderFooter))
      {
        Trace.WriteLine(Messages.ChartFreelyPlacedInWrongContext, "warning");
        return;
      }
      if (renderInParagraph)
        StartDummyParagraph();

      if (!this.isInline)
        StartShapeArea();

      RenderImage(fileName);

      if (!this.isInline)
        EndShapeArea();

      if (renderInParagraph)
        EndDummyParagraph();

      if (File.Exists(fileName))
        File.Delete(fileName);
    }

    /// <summary>
    /// Renders image specific attributes and the image byte series to RTF.
    /// </summary>
    void RenderImage(string fileName)
    {
      StartImageDescription();
      RenderImageAttributes();
      RenderByteSeries(fileName);
      EndImageDescription();
    }

    void StartImageDescription()
    {
      if (isInline)
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControlWithStar("shppict");
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("pict");
      }
      else
      {
        RenderNameValuePair("shapeType", "75");//75 entspr. Bildrahmen.
        StartNameValuePair("pib");
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl("pict");
      }
    }

    void EndImageDescription()
    {
      if (isInline)
      {
        this.rtfWriter.EndContent();
        this.rtfWriter.EndContent();
      }
      else
      {
        this.rtfWriter.EndContent();
        EndNameValuePair();
      }
    }

    void RenderImageAttributes()
    {
      if (this.isInline)
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControlWithStar("picprop");
        RenderNameValuePair("shapeType", "75");
        RenderFillFormat();
        //REM: LineFormat is not completely supported in word.
        //RenderLineFormat();
        this.rtfWriter.EndContent();
      }
      RenderDimensionSettings();
      RenderSourceType();
    }

    private void RenderSourceType()
    {
      this.rtfWriter.WriteControl("pngblip");
    }

    /// <summary>
    /// Renders scaling, width and height for the image.
    /// </summary>
    private void RenderDimensionSettings()
    {
      this.rtfWriter.WriteControl("picscalex", 100);
      this.rtfWriter.WriteControl("picscaley", 100);


      RenderUnit("pichgoal", GetShapeHeight());
      RenderUnit("picwgoal", GetShapeWidth());

      //A bit obscure, but necessary for Word 2000:
      this.rtfWriter.WriteControl("pich", (int)(GetShapeHeight().Millimeter * 100));
      this.rtfWriter.WriteControl("picw", (int)(GetShapeWidth().Millimeter * 100));
    }

    bool StoreTempImage(string fileName)
    {
      try
      {
        float resolution = 96;
        int horzPixels = (int)(GetShapeWidth().Inch * resolution);
        int vertPixels = (int)(GetShapeHeight().Inch * resolution);
        Bitmap bmp = new Bitmap(horzPixels, vertPixels);
#if true
        XGraphics gfx = XGraphics.CreateMeasureContext(new XSize(horzPixels, vertPixels), XGraphicsUnit.Point, XPageDirection.Downwards);
#else
#if GDI
        XGraphics gfx = XGraphics.FromGraphics(Graphics.FromImage(bmp), new XSize(horzPixels, vertPixels));
#endif
#if WPF
        // TODOWPF
        XGraphics gfx = null; //XGraphics.FromGraphics(Graphics.FromImage(bmp), new XSize(horzPixels, vertPixels));
#endif
#endif
        //REM: Should not be necessary:
        gfx.ScaleTransform(resolution / 72);
        //gfx.PageUnit = XGraphicsUnit.Point;

        DocumentRenderer renderer = new DocumentRenderer(this.chart.Document);
        renderer.RenderObject(gfx, 0, 0, GetShapeWidth().Point, this.chart);
        bmp.SetResolution(resolution, resolution);
        bmp.Save(fileName, ImageFormat.Png);
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }
    /*private void CalculateImageDimensions()
    {
      try
      {
        this.imageFile = File.OpenRead(this.filePath);
        System.Drawing.Bitmap bip = new System.Drawing.Bitmap(imageFile);
        float horzResolution;
        float vertResolution;
        string ext = Path.GetExtension(this.filePath).ToLower();
        float origHorzRes = bip.HorizontalResolution;
        float origVertRes = bip.VerticalResolution;

        this.originalHeight = bip.Height * 72 / origVertRes;
        this.originalWidth = bip.Width * 72 / origHorzRes;

          horzResolution = bip.HorizontalResolution;
          vertResolution = bip.VerticalResolution;
        }
        else
        {
          horzResolution= (float)GetValueAsIntended("Resolution");
          vertResolution= horzResolution;
        }

        Unit origHeight = bip.Size.Height * 72 / vertResolution;
        Unit origWidth = bip.Size.Width * 72 / horzResolution;

        this.imageHeight = origHeight;
        this.imageWidth = origWidth;

        bool scaleWidthIsNull = this.image.IsNull("ScaleWidth");
        bool scaleHeightIsNull = this.image.IsNull("ScaleHeight");
        float sclHeight = scaleHeightIsNull ? 1 : (float)GetValueAsIntended("ScaleHeight");
        this.scaleHeight= sclHeight;
        float sclWidth = scaleWidthIsNull ? 1 : (float)GetValueAsIntended("ScaleWidth");
        this.scaleWidth = sclWidth;

        bool doLockAspectRatio = this.image.IsNull("LockAspectRatio") || this.image.LockAspectRatio;

        if (doLockAspectRatio && (scaleHeightIsNull || scaleWidthIsNull))
        {
          if (!this.image.IsNull("Width") && this.image.IsNull("Height"))
          {
            imageWidth = this.image.Width;
            imageHeight = origHeight * imageWidth / origWidth;
          }
          else if (!this.image.IsNull("Height") && this.image.IsNull("Width"))
          {
            imageHeight = this.image.Height;
            imageWidth = origWidth * imageHeight / origHeight;
          }
          else if (!this.image.IsNull("Height") && !this.image.IsNull("Width"))
          {
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;
          }
          if (scaleWidthIsNull && !scaleHeightIsNull)
            scaleWidth = scaleHeight;
          else if (scaleHeightIsNull && ! scaleWidthIsNull)
            scaleHeight =  scaleWidth;
        }
        else
        {
          if (!this.image.IsNull("Width"))
            imageWidth = this.image.Width;
          if (!this.image.IsNull("Height"))
            imageHeight = this.image.Height;
        }

        return;
      }
      catch(FileNotFoundException)
      {
        Trace.WriteLine(Messages.ImageNotFound(this.image.Name), "warning");
      }
      catch(Exception exc)
      {
        Trace.WriteLine(Messages.ImageNotReadable(this.image.Name, exc.Message), "warning");
      }

      //Setting defaults in case an error occured.
      this.imageFile = null;
      this.imageHeight = (Unit)GetValueOrDefault("Height", Unit.FromInch(1));
      this.imageWidth = (Unit)GetValueOrDefault("Width", Unit.FromInch(1));
      this.scaleHeight = (double)GetValueOrDefault("ScaleHeight", 1.0);
      this.scaleWidth = (double)GetValueOrDefault("ScaleWidth", 1.0);
    }*/

    /// <summary>
    /// Renders the image file as byte series.
    /// </summary>
    private void RenderByteSeries(string fileName)
    {
      FileStream imageFile = null;
      try
      {
        imageFile = new FileStream(fileName, FileMode.Open);

        imageFile.Seek(0, SeekOrigin.Begin);
        int byteVal;
        while ((byteVal = imageFile.ReadByte()) != -1)
        {
          string strVal = byteVal.ToString("x");
          if (strVal.Length == 1)
            this.rtfWriter.WriteText("0");
          this.rtfWriter.WriteText(strVal);
        }
      }
      catch
      {
        Trace.WriteLine("Chart image file not read", "warning");
      }
      finally
      {
        if (imageFile != null)
          imageFile.Close();
      }
    }

    protected override Unit GetShapeHeight()
    {
      return base.GetShapeHeight() + base.GetLineWidth();
    }

    protected override Unit GetShapeWidth()
    {
      return base.GetShapeWidth() + base.GetLineWidth(); ;
    }
    private Chart chart;
    bool isInline;
  }
}
