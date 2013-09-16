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
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif
using PdfSharp;
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;

// WPFHACK
#pragma warning disable 162

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Defines an object used to draw image files (bmp, png, jpeg, gif) and PDF forms.
  /// An abstract base class that provides functionality for the Bitmap and Metafile descended classes.
  /// </summary>
  public class XImage : IDisposable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XImage"/> class.
    /// </summary>
    protected XImage()
    {
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the <see cref="XImage"/> class from a GDI+ image.
    /// </summary>
    XImage(Image image)
    {
      this.gdiImage = image;
#if WPF
      this.wpfImage = ImageHelper.CreateBitmapSource(image);
#endif
      Initialize();
    }
#endif

#if WPF && !SILVERLIGHT
    /// <summary>
    /// Initializes a new instance of the <see cref="XImage"/> class from a WPF image.
    /// </summary>
    XImage(BitmapSource image)
    {
      this.wpfImage = image;
      Initialize();
    }
#endif

    XImage(string path)
    {
      path = Path.GetFullPath(path);
      if (!File.Exists(path))
        throw new FileNotFoundException(PSSR.FileNotFound(path), path);

      this.path = path;

      //FileStream file = new FileStream(filename, FileMode.Open);
      //BitsLength = (int)file.Length;
      //Bits = new byte[BitsLength];
      //file.Read(Bits, 0, BitsLength);
      //file.Close();
#if GDI
      this.gdiImage = Image.FromFile(path);
#endif
#if WPF && !SILVERLIGHT
      //BitmapSource.Create()
      // BUG: BitmapImage locks the file
      this.wpfImage = new BitmapImage(new Uri(path));  // AGHACK
#endif

#if false
      float vres = this.image.VerticalResolution;
      float hres = this.image.HorizontalResolution;
      SizeF size = this.image.PhysicalDimension;
      int flags  = this.image.Flags;
      Size sz    = this.image.Size;
      GraphicsUnit units = GraphicsUnit.Millimeter;
      RectangleF rect = this.image.GetBounds(ref units);
      int width = this.image.Width;
#endif
      Initialize();
    }

    XImage(Stream stream)
    {
      // Create a dummy unique path
      this.path = "*" + Guid.NewGuid().ToString("B");

#if GDI
      this.gdiImage = Image.FromStream(stream);
#endif
#if WPF
      throw new NotImplementedException();
      //this.wpfImage = new BitmapImage(new Uri(path));
#endif

#if true_
      float vres = this.image.VerticalResolution;
      float hres = this.image.HorizontalResolution;
      SizeF size = this.image.PhysicalDimension;
      int flags  = this.image.Flags;
      Size sz    = this.image.Size;
      GraphicsUnit units = GraphicsUnit.Millimeter;
      RectangleF rect = this.image.GetBounds(ref units);
      int width = this.image.Width;
#endif
      Initialize();
    }

#if GDI
#if UseGdiObjects
    /// <summary>
    /// Implicit conversion from Image to XImage.
    /// </summary>
    public static implicit operator XImage(Image image)
    {
      return new XImage(image);
    }
#endif

    /// <summary>
    /// Conversion from Image to XImage.
    /// </summary>
    public static XImage FromGdiPlusImage(Image image)
    {
      return new XImage(image);
    }
#endif

#if WPF && !SILVERLIGHT
    /// <summary>
    /// Conversion from BitmapSource to XImage.
    /// </summary>
    public static XImage FromBitmapSource(BitmapSource image)
    {
      return new XImage(image);
    }
#endif

    /// <summary>
    /// Creates an image from the specified file.
    /// </summary>
    /// <param name="path">The path to a BMP, PNG, GIF, JPEG, TIFF, or PDF file.</param>
    public static XImage FromFile(string path)
    {
      if (PdfReader.TestPdfFile(path) > 0)
        return new XPdfForm(path);
      return new XImage(path);
    }

    /// <summary>
    /// Tests if a file exist. Supports PDF files with page number suffix.
    /// </summary>
    /// <param name="path">The path to a BMP, PNG, GIF, JPEG, TIFF, or PDF file.</param>
    public static bool ExistsFile(string path)
    {
      if (PdfReader.TestPdfFile(path) > 0)
        return true;
      return File.Exists(path);
    }

    void Initialize()
    {
#if GDI
      if (this.gdiImage != null)
      {
        // ImageFormat has no overridden Equals...
        string guid = this.gdiImage.RawFormat.Guid.ToString("B").ToUpper();
        switch (guid)
        {
          case "{B96B3CAA-0728-11D3-9D7B-0000F81EF32E}":  // memoryBMP
          case "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}":  // bmp
          case "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}":  // png
            this.format = XImageFormat.Png;
            break;

          case "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}":  // jpeg
            this.format = XImageFormat.Jpeg;
            break;

          case "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}":  // gif
            this.format = XImageFormat.Gif;
            break;

          case "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}":  // tiff
            this.format = XImageFormat.Tiff;
            break;

          case "{B96B3CB5-0728-11D3-9D7B-0000F81EF32E}":  // icon
            this.format = XImageFormat.Icon;
            break;

          case "{B96B3CAC-0728-11D3-9D7B-0000F81EF32E}":  // emf
          case "{B96B3CAD-0728-11D3-9D7B-0000F81EF32E}":  // wmf
          case "{B96B3CB2-0728-11D3-9D7B-0000F81EF32E}":  // exif
          case "{B96B3CB3-0728-11D3-9D7B-0000F81EF32E}":  // photoCD
          case "{B96B3CB4-0728-11D3-9D7B-0000F81EF32E}":  // flashPIX

          default:
            throw new InvalidOperationException("Unsupported image format.");
        }
        return;
      }
#endif
#if WPF
#if !SILVERLIGHT
      if (this.wpfImage != null)
      {
        string pixelFormat = this.wpfImage.Format.ToString();
        string filename = GetImageFilename(this.wpfImage);
        // WPF treats all images as images.
        // We give JPEG images a special treatment.
        // Test if it's a JPEG:
        bool isJpeg = IsJpeg; // TestJpeg(filename);
        if (isJpeg)
        {
          this.format = XImageFormat.Jpeg;
          return;
        }

        switch (pixelFormat)
        {
          case "Bgr32":
          case "Bgra32":
          case "Pbgra32":
            this.format = XImageFormat.Png;
            break;

          //case "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}":  // jpeg
          //  this.format = XImageFormat.Jpeg;
          //  break;

          //case "{B96B3CB0-0728-11D3-9D7B-0000F81EF32E}":  // gif
          case "BlackWhite":
          case "Indexed1":
          case "Indexed4":
          case "Indexed8":
          case "Gray8":
            this.format = XImageFormat.Gif;
            break;

          //case "{B96B3CB1-0728-11D3-9D7B-0000F81EF32E}":  // tiff
          //  this.format = XImageFormat.Tiff;
          //  break;

          //case "{B96B3CB5-0728-11D3-9D7B-0000F81EF32E}":  // icon
          //  this.format = XImageFormat.Icon;
          //  break;

          //case "{B96B3CAC-0728-11D3-9D7B-0000F81EF32E}":  // emf
          //case "{B96B3CAD-0728-11D3-9D7B-0000F81EF32E}":  // wmf
          //case "{B96B3CB2-0728-11D3-9D7B-0000F81EF32E}":  // exif
          //case "{B96B3CB3-0728-11D3-9D7B-0000F81EF32E}":  // photoCD
          //case "{B96B3CB4-0728-11D3-9D7B-0000F81EF32E}":  // flashPIX

          default:
            Debug.Assert(false, "Unknown pixel format: " + pixelFormat);
            this.format = XImageFormat.Gif;
            break;// throw new InvalidOperationException("Unsupported image format.");
        }
      }
#else
      // AGHACK
#endif
#endif
    }

#if WPF
    /// <summary>
    /// Gets the image filename.
    /// </summary>
    /// <param name="bitmapSource">The bitmap source.</param>
    internal static string GetImageFilename(BitmapSource bitmapSource)
    {
      string filename = bitmapSource.ToString();
      filename = UrlDecodeStringFromStringInternal(filename);
      if (filename.StartsWith("file:///"))
        filename = filename.Substring(8); // Remove all 3 slashes!
      else if (filename.StartsWith("file://"))
        filename = filename.Substring(5); // Keep 2 slashes (UNC path)
      return filename;
    }

    private static string UrlDecodeStringFromStringInternal(string s/*, Encoding e*/)
    {
      int length = s.Length;
      string result = "";
      for (int i = 0; i < length; i++)
      {
        char ch = s[i];
        if (ch == '+')
        {
          ch = ' ';
        }
        else if ((ch == '%') && (i < (length - 2)))
        {
          if ((s[i + 1] == 'u') && (i < (length - 5)))
          {
            int num3 = HexToInt(s[i + 2]);
            int num4 = HexToInt(s[i + 3]);
            int num5 = HexToInt(s[i + 4]);
            int num6 = HexToInt(s[i + 5]);
            if (((num3 < 0) || (num4 < 0)) || ((num5 < 0) || (num6 < 0)))
            {
              goto AddByte;
            }
            ch = (char)((((num3 << 12) | (num4 << 8)) | (num5 << 4)) | num6);
            i += 5;
            result += ch;
            continue;
          }
          int num7 = HexToInt(s[i + 1]);
          int num8 = HexToInt(s[i + 2]);
          if ((num7 >= 0) && (num8 >= 0))
          {
            byte b = (byte)((num7 << 4) | num8);
            i += 2;
            result += (char)b;
            continue;
          }
        }
      AddByte:
        if ((ch & 0xff80) == 0)
        {
          result += ch;
        }
        else
        {
          result += ch;
        }
      }
      return result;
    }

    private static int HexToInt(char h)
    {
      if ((h >= '0') && (h <= '9'))
      {
        return (h - '0');
      }
      if ((h >= 'a') && (h <= 'f'))
      {
        return ((h - 'a') + 10);
      }
      if ((h >= 'A') && (h <= 'F'))
      {
        return ((h - 'A') + 10);
      }
      return -1;
    }
#endif

#if WPF
    /// <summary>
    /// Tests if a file is a JPEG.
    /// </summary>
    /// <param name="filename">The filename.</param>
    internal static bool TestJpeg(string filename)
    {
      byte[] imageBits = null;
      return ReadJpegFile(filename, 16, ref imageBits);
    }

    /// <summary>
    /// Reads the JPEG file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="maxRead">The maximum count of bytes to be read.</param>
    /// <param name="imageBits">The bytes read from the file.</param>
    /// <returns>False, if file could not be read or is not a JPEG file.</returns>
    internal static bool ReadJpegFile(string filename, int maxRead, ref byte[] imageBits)
    {
      if (File.Exists(filename))
      {
        FileStream fs = null;
        try
        {
          fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        catch
        {
          return false;
        }

        if (fs.Length < 16)
        {
          fs.Close();
          return false;
        }
        int len = maxRead == -1 ? (int)fs.Length : maxRead;
        imageBits = new byte[len];
        fs.Read(imageBits, 0, len);
        fs.Close();
        if (imageBits[0] == 0xff &&
            imageBits[1] == 0xd8 &&
            imageBits[2] == 0xff &&
            imageBits[3] == 0xe0 &&
            imageBits[6] == 0x4a &&
            imageBits[7] == 0x46 &&
            imageBits[8] == 0x49 &&
            imageBits[9] == 0x46 &&
            imageBits[10] == 0x0)
        {
          return true;
        }
        // TODO: Exif: find JFIF header
        if (imageBits[0] == 0xff &&
            imageBits[1] == 0xd8 &&
            imageBits[2] == 0xff &&
            imageBits[3] == 0xe1 /*&&
            imageBits[6] == 0x4a &&
            imageBits[7] == 0x46 &&
            imageBits[8] == 0x49 &&
            imageBits[9] == 0x46 &&
            imageBits[10] == 0x0*/)
        {
          // Hack: store the file in PDF if extension matches ...
          string str = filename.ToLower();
          if (str.EndsWith(".jpg") || str.EndsWith(".jpeg"))
            return true;
        }
      }
      return false;
    }
#endif

    /// <summary>
    /// Under construction
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      //GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes underlying GDI+ object.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
        this.disposed = true;

#if GDI
      if (this.gdiImage != null)
      {
        this.gdiImage.Dispose();
        this.gdiImage = null;
      }
#endif
#if WPF
      if (wpfImage != null)
      {
        wpfImage = null;
      }
#endif
    }
    bool disposed;


    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    [Obsolete("Use either PixelWidth or PointWidth. Temporarily obsolete because of rearrangements for WPF. Currently same as PixelWidth, but will become PointWidth in future releases of PDFsharp.")]
    public virtual double Width
    {
      get
      {
#if GDI && WPF
        double gdiWidth = this.gdiImage.Width;
        double wpfWidth = this.wpfImage.PixelWidth;
        Debug.Assert(gdiWidth == wpfWidth);
        return wpfWidth;
#endif
#if GDI && !WPF
        return this.gdiImage.Width;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.PixelWidth;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    [Obsolete("Use either PixelHeight or PointHeight. Temporarily obsolete because of rearrangements for WPF. Currently same as PixelHeight, but will become PointHeight in future releases of PDFsharp.")]
    public virtual double Height
    {
      get
      {
#if GDI && WPF
        double gdiHeight = this.gdiImage.Height;
        double wpfHeight = this.wpfImage.PixelHeight;
        Debug.Assert(gdiHeight == wpfHeight);
        return wpfHeight;
#endif
#if GDI && !WPF
        return this.gdiImage.Height;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.PixelHeight;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the width of the image in point.
    /// </summary>
    public virtual double PointWidth
    {
      get
      {
#if GDI && WPF
        double gdiWidth = this.gdiImage.Width * 72 / this.gdiImage.HorizontalResolution;
        double wpfWidth = this.wpfImage.Width * 72.0 / 96.0;
        //Debug.Assert(gdiWidth == wpfWidth);
        Debug.Assert(DoubleUtil.AreRoughlyEqual(gdiWidth, wpfWidth, 5));
        return wpfWidth;
#endif
#if GDI && !WPF
        return this.gdiImage.Width * 72 / this.gdiImage.HorizontalResolution;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        Debug.Assert(Math.Abs(this.wpfImage.PixelWidth * 72 / this.wpfImage.DpiX - this.wpfImage.Width * 72.0 / 96.0) < 0.001);
        return this.wpfImage.Width * 72.0 / 96.0;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the height of the image in point.
    /// </summary>
    public virtual double PointHeight
    {
      get
      {
#if GDI && WPF
        double gdiHeight = this.gdiImage.Height * 72 / this.gdiImage.HorizontalResolution;
        double wpfHeight = this.wpfImage.Height * 72.0 / 96.0;
        Debug.Assert(DoubleUtil.AreRoughlyEqual(gdiHeight, wpfHeight, 5));
        return wpfHeight;
#endif
#if GDI && !WPF
        return this.gdiImage.Height * 72 / this.gdiImage.HorizontalResolution;
#endif
#if WPF || SILVERLIGHT && !GDI
#if !SILVERLIGHT
        Debug.Assert(Math.Abs(this.wpfImage.PixelHeight * 72 / this.wpfImage.DpiY - this.wpfImage.Height * 72.0 / 96.0) < 0.001);
        return this.wpfImage.Height * 72.0 / 96.0;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public virtual int PixelWidth
    {
      get
      {
#if GDI && WPF
        int gdiWidth = this.gdiImage.Width;
        int wpfWidth = this.wpfImage.PixelWidth;
        Debug.Assert(gdiWidth == wpfWidth);
        return wpfWidth;
#endif
#if GDI && !WPF
        return this.gdiImage.Width;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.PixelWidth;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public virtual int PixelHeight
    {
      get
      {
#if GDI && WPF
        int gdiHeight = this.gdiImage.Height;
        int wpfHeight = this.wpfImage.PixelHeight;
        Debug.Assert(gdiHeight == wpfHeight);
        return wpfHeight;
#endif
#if GDI && !WPF
        return this.gdiImage.Height;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.PixelHeight;
#else
        // AGHACK
        return 100;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the size in point of the image.
    /// </summary>
    public virtual XSize Size
    {
      get { return new XSize(PointWidth, PointHeight); }
    }

    /// <summary>
    /// Gets the horizontal resolution of the image.
    /// </summary>
    public virtual double HorizontalResolution
    {
      get
      {
#if GDI && WPF
        double gdiResolution = this.gdiImage.HorizontalResolution;
        double wpfResolution = this.wpfImage.PixelWidth * 96.0 / this.wpfImage.Width;
        Debug.Assert(gdiResolution == wpfResolution);
        return wpfResolution;
#endif
#if GDI && !WPF
        return this.gdiImage.HorizontalResolution;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.DpiX; //.PixelWidth * 96.0 / this.wpfImage.Width;
#else
        // AGHACK
        return 96;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets the vertical resolution of the image.
    /// </summary>
    public virtual double VerticalResolution
    {
      get
      {
#if GDI && WPF
        double gdiResolution = this.gdiImage.VerticalResolution;
        double wpfResolution = this.wpfImage.PixelHeight * 96.0 / this.wpfImage.Height;
        Debug.Assert(gdiResolution == wpfResolution);
        return wpfResolution;
#endif
#if GDI && !WPF
        return this.gdiImage.VerticalResolution;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        return this.wpfImage.DpiY; //.PixelHeight * 96.0 / this.wpfImage.Height;
#else
        // AGHACK
        return 96;
#endif
#endif
      }
    }

    /// <summary>
    /// Gets or sets a flag indicating whether image interpolation is to be performed. 
    /// </summary>
    public virtual bool Interpolate
    {
      get { return this.interpolate; }
      set { this.interpolate = value; }
    }
    bool interpolate = true;

    /// <summary>
    /// Gets the format of the image.
    /// </summary>
    public XImageFormat Format
    {
      get { return this.format; }
    }
    XImageFormat format;

#if WPF
    /// <summary>
    /// Gets a value indicating whether this image is JPEG.
    /// </summary>
    /// <value><c>true</c> if this image is JPEG; otherwise, <c>false</c>.</value>
    public virtual bool IsJpeg
    {
#if !SILVERLIGHT
      //get { if (!isJpeg.HasValue) InitializeGdiHelper(); return isJpeg.HasValue ? isJpeg.Value : false; }
      get { if (!isJpeg.HasValue) InitializeJpegQuickTest(); return isJpeg.HasValue ? isJpeg.Value : false; }
      //set { isJpeg = value; }
#else
      get { return false; } // AGHACK
#endif
    }
    private bool? isJpeg;

    /// <summary>
    /// Gets a value indicating whether this image is cmyk.
    /// </summary>
    /// <value><c>true</c> if this image is cmyk; otherwise, <c>false</c>.</value>
    public virtual bool IsCmyk
    {
#if !SILVERLIGHT
      get { if (!isCmyk.HasValue) InitializeGdiHelper(); return isCmyk.HasValue ? isCmyk.Value : false; }
      //set { isCmyk = value; }
#else
      get { return false; } // AGHACK
#endif
    }
    private bool? isCmyk;

#if !SILVERLIGHT
    /// <summary>
    /// Gets the JPEG memory stream (if IsJpeg returns true).
    /// </summary>
    /// <value>The memory.</value>
    public virtual MemoryStream Memory
    {
      get { if (!isCmyk.HasValue) InitializeGdiHelper(); return memory; }
      //set { memory = value; }
    }
    MemoryStream memory = null;

    /// <summary>
    /// Determines if an image is JPEG w/o creating an Image object.
    /// </summary>
    private void InitializeJpegQuickTest()
    {
      isJpeg = TestJpeg(GetImageFilename(wpfImage));
    }

    /// <summary>
    /// Initializes the GDI helper.
    /// We use GDI+ to detect if image is JPEG.
    /// If so, we also determine if it's CMYK and we read the image bytes.
    /// </summary>
    private void InitializeGdiHelper()
    {
      if (!isCmyk.HasValue)
      {
        try
        {
          using (System.Drawing.Image image = new System.Drawing.Bitmap(GetImageFilename(wpfImage)))
          {
            string guid = image.RawFormat.Guid.ToString("B").ToUpper();
            isJpeg = guid == "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";
            isCmyk = (image.Flags & ((int)System.Drawing.Imaging.ImageFlags.ColorSpaceCmyk | (int)System.Drawing.Imaging.ImageFlags.ColorSpaceYcck)) != 0;
            if (isJpeg.Value)
            {
              memory = new MemoryStream();
              image.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
              if ((int)memory.Length == 0)
              {
                memory = null;
              }
            }
          }
        }
        catch { }
      }
    }
#endif    
#endif    


#if DEBUG_
    // TEST
    internal void CreateAllImages(string name)
    {
      if (this.image != null)
      {
        this.image.Save(name + ".bmp", ImageFormat.Bmp);
        this.image.Save(name + ".emf", ImageFormat.Emf);
        this.image.Save(name + ".exif", ImageFormat.Exif);
        this.image.Save(name + ".gif", ImageFormat.Gif);
        this.image.Save(name + ".ico", ImageFormat.Icon);
        this.image.Save(name + ".jpg", ImageFormat.Jpeg);
        this.image.Save(name + ".png", ImageFormat.Png);
        this.image.Save(name + ".tif", ImageFormat.Tiff);
        this.image.Save(name + ".wmf", ImageFormat.Wmf);
        this.image.Save(name + "2.bmp", ImageFormat.MemoryBmp);
      }
    }
#endif
#if GDI
    internal Image gdiImage;
#endif
#if WPF
    internal BitmapSource wpfImage;
#endif

    /// <summary>
    /// If path starts with '*' the image is created from a stream and the path is a GUID.
    /// </summary>
    internal string path;

    /// <summary>
    /// Cache PdfImageTable.ImageSelector to speed up finding the right PdfImage
    /// if this image is used more than once.
    /// </summary>
    internal PdfImageTable.ImageSelector selector;
  }
}