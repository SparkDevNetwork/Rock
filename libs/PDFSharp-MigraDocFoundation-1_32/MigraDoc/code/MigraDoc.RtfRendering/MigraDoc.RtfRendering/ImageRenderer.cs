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
using System.Reflection;
using System.IO;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Render an image to RTF.
  /// </summary>
  internal class ImageRenderer : ShapeRenderer
  {
    internal ImageRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.image = domObj as Image;
      this.filePath = this.image.GetFilePath(this.docRenderer.WorkingDirectory);
      this.isInline = DocumentRelations.HasParentOfType(this.image, typeof(Paragraph)) ||
         RenderInParagraph();

      CalculateImageDimensions();
    }

    /// <summary>
    /// Renders an image to RTF.
    /// </summary>
    internal override void Render()
    {
      bool renderInParagraph = RenderInParagraph();
      DocumentElements elms = DocumentRelations.GetParent(this.image) as DocumentElements;
      if (elms != null && !renderInParagraph && !(DocumentRelations.GetParent(elms) is Section || DocumentRelations.GetParent(elms) is HeaderFooter))
      {
        Trace.WriteLine(Messages.ImageFreelyPlacedInWrongContext(this.image.Name), "warning");
        return;
      }
      if (renderInParagraph)
        StartDummyParagraph();

      if (!this.isInline)
        StartShapeArea();

      RenderImage();
      if (!this.isInline)
        EndShapeArea();

      if (renderInParagraph)
        EndDummyParagraph();
    }

    /// <summary>
    /// Renders image specific attributes and the image byte series to RTF.
    /// </summary>
    void RenderImage()
    {
      StartImageDescription();
      RenderImageAttributes();
      RenderByteSeries();
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
        RenderNameValuePair("shapeType", "75"); // 75 entspr. Bildrahmen.
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
      if (isInline)
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControlWithStar("picprop");
        RenderNameValuePair("shapeType", "75");
        RenderFillFormat();
        //REM: LineFormat is not completely supported in word.
        RenderLineFormat();
        this.rtfWriter.EndContent();
      }
      RenderDimensionSettings();
      RenderCropping();
      RenderSourceType();
    }

    private void RenderSourceType()
    {
      string extension = Path.GetExtension(this.filePath);
      if (extension == null)
      {
        this.imageFile = null;
        Trace.WriteLine("No Image type given.", "warning");
        return;
      }
      switch (extension.ToLower())
      {
        case ".jpeg":
        case ".jpg":
          this.rtfWriter.WriteControl("jpegblip");
          break;

        case ".png":
          this.rtfWriter.WriteControl("pngblip");
          break;

        case ".gif":
          this.rtfWriter.WriteControl("pngblip");
          break;

        case ".pdf":
          // Show a PDF logo in RTF document
          this.imageFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("MigraDoc.RtfRendering.Resources.PDF.png");
          this.rtfWriter.WriteControl("pngblip");
          break;

        default:
          Trace.WriteLine(Messages.ImageTypeNotSupported(this.image.Name), "warning");
          this.imageFile = null;
          break;
      }
    }

    /// <summary>
    /// Renders scaling, width and height for the image.
    /// </summary>
    private void RenderDimensionSettings()
    {
      float scaleX = (GetShapeWidth() / originalWidth);
      float scaleY = (GetShapeHeight() / originalHeight);
      this.rtfWriter.WriteControl("picscalex", (int)(scaleX * 100));
      this.rtfWriter.WriteControl("picscaley", (int)(scaleY * 100));

      RenderUnit("pichgoal", GetShapeHeight() / scaleY);
      RenderUnit("picwgoal", GetShapeWidth() / scaleX);

      //A bit obscure, but necessary for Word 2000:
      this.rtfWriter.WriteControl("pich", (int)(originalHeight.Millimeter * 100));
      this.rtfWriter.WriteControl("picw", (int)(originalWidth.Millimeter * 100));
    }

    private void CalculateImageDimensions()
    {
      try
      {
        this.imageFile = File.OpenRead(this.filePath);
        //System.Drawing.Bitmap bip2 = new System.Drawing.Bitmap(imageFile);
        XImage bip = XImage.FromFile(this.filePath);

        float horzResolution;
        float vertResolution;
        string ext = Path.GetExtension(this.filePath).ToLower();
        float origHorzRes = (float)bip.HorizontalResolution;
        float origVertRes = (float)bip.VerticalResolution;

        this.originalHeight = bip.PixelHeight * 72 / origVertRes;
        this.originalWidth = bip.PixelWidth * 72 / origHorzRes;

        if (this.image.IsNull("Resolution"))
        {
          horzResolution = (ext == ".gif") ? 72 : (float)bip.HorizontalResolution;
          vertResolution = (ext == ".gif") ? 72 : (float)bip.VerticalResolution;
        }
        else
        {
          horzResolution = (float)GetValueAsIntended("Resolution");
          vertResolution = horzResolution;
        }

        Unit origHeight = bip.Size.Height * 72 / vertResolution;
        Unit origWidth = bip.Size.Width * 72 / horzResolution;

        this.imageHeight = origHeight;
        this.imageWidth = origWidth;

        bool scaleWidthIsNull = this.image.IsNull("ScaleWidth");
        bool scaleHeightIsNull = this.image.IsNull("ScaleHeight");
        float sclHeight = scaleHeightIsNull ? 1 : (float)GetValueAsIntended("ScaleHeight");
        this.scaleHeight = sclHeight;
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
          else if (scaleHeightIsNull && !scaleWidthIsNull)
            scaleHeight = scaleWidth;
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
      catch (FileNotFoundException)
      {
        Trace.WriteLine(Messages.ImageNotFound(this.image.Name), "warning");
      }
      catch (Exception exc)
      {
        Trace.WriteLine(Messages.ImageNotReadable(this.image.Name, exc.Message), "warning");
      }

      //Setting defaults in case an error occured.
      this.imageFile = null;
      this.imageHeight = (Unit)GetValueOrDefault("Height", Unit.FromInch(1));
      this.imageWidth = (Unit)GetValueOrDefault("Width", Unit.FromInch(1));
      this.scaleHeight = (double)GetValueOrDefault("ScaleHeight", 1.0);
      this.scaleWidth = (double)GetValueOrDefault("ScaleWidth", 1.0);
    }

    /// <summary>
    /// Renders the image file as byte series.
    /// </summary>
    private void RenderByteSeries()
    {
      if (this.imageFile != null)
      {
        this.imageFile.Seek(0, SeekOrigin.Begin);
        int byteVal;
        while ((byteVal = this.imageFile.ReadByte()) != -1)
        {
          string strVal = byteVal.ToString("x");
          if (strVal.Length == 1)
            this.rtfWriter.WriteText("0");
          this.rtfWriter.WriteText(strVal);
        }
        this.imageFile.Close();
      }
    }

    protected override Unit GetShapeHeight()
    {
      return this.imageHeight * this.scaleHeight;
    }


    protected override Unit GetShapeWidth()
    {
      return this.imageWidth * this.scaleWidth;
    }

    /// <summary>
    /// Renders the image cropping at all edges.
    /// </summary>
    void RenderCropping()
    {
      this.Translate("PictureFormat.CropLeft", "piccropl");
      this.Translate("PictureFormat.CropRight", "piccropr");
      this.Translate("PictureFormat.CropTop", "piccropt");
      this.Translate("PictureFormat.CropBottom", "piccropb");
    }
    private string filePath;
    private Image image;
    bool isInline;
    //FileStream imageFile;
    Stream imageFile;
    Unit imageWidth;
    Unit imageHeight;

    Unit originalHeight;
    Unit originalWidth;

    //this is the user defined scaling, not the stuff to render as scalex, scaley
    double scaleHeight;
    double scaleWidth;
  }
}
