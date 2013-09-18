using System;
using System.Globalization;
using System.Drawing;
using System.Xml.Serialization;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace XDrawing.TestLab
{
  /// <summary>
  /// 
  /// </summary>
  [XmlRootAttribute("GraphicsProperties", Namespace = "http://www.pdfsharp.com", IsNullable = false)]
  public class GraphicsProperties
  {
    //public TestPoint<int> TEST;

    public GraphicsProperties()
    {
      //TEST = new TestPoint<int>(10, 20);

      this.general = new GeneralProperties();
      this.general.BackColor.Color = XColor.FromArgb(255, 253, 253);

      this.pen1 = new PenProperty();
      this.pen1.Color.Color = XColors.Black;
      this.pen1.Width = 1;

      this.pen2 = new PenProperty();
      this.pen2.Color.Color = XColors.Red;
      this.pen2.Width = 3;

      this.pen3 = new PenProperty();
      this.pen3.Color.Color = XColors.Green;
      this.pen3.Width = 5;

      this.brush1 = new BrushProperty();
      this.brush1.Color.Color = XColors.DarkBlue;

      this.brush2 = new BrushProperty();
      this.brush2.Color.Color = XColors.Orange;

      this.brush3 = new BrushProperty();
      this.brush3.Color.Color = XColors.Yellow;

      this.font1 = new FontProperty();
      this.font1.FamilyName = "Verdana";
      this.font1.Color.Color = XColors.Black;
      this.font1.Size = 15;
      this.font1.Text = "ABCgfw";

      this.font2 = new FontProperty();
      this.font2.FamilyName = "Times New Roman";
      this.font2.Color.Color = XColors.Red;
      this.font2.Size = 10;
      this.font2.Bold = true;
      this.font2.Text = "Qwerty";

      this.font3 = new FontProperty();
      this.font3.FamilyName = "Courier New";
      this.font3.Color.Color = XColors.SpringGreen;
      this.font3.Size = 15;
      this.font3.Bold = true;
      this.font3.Italic = true;
      this.font3.Text = "Qwerty";
    }

    public bool AutoApply = true;

    public System.Drawing.Rectangle rect = new Rectangle(0, 1, 2, 3);

    public GeneralProperties General
    {
      get { return this.general; }
      set { this.general = value; }
    }
    GeneralProperties general;

    public PenProperty Pen1
    {
      get { return this.pen1; }
      set { this.pen1 = value; }
    }
    PenProperty pen1;

    public PenProperty Pen2
    {
      get { return this.pen2; }
      set { this.pen2 = value; }
    }
    PenProperty pen2;

    public PenProperty Pen3
    {
      get { return this.pen3; }
      set { this.pen3 = value; }
    }
    PenProperty pen3;

    public BrushProperty Brush1
    {
      get { return this.brush1; }
      set { this.brush1 = value; }
    }
    BrushProperty brush1;

    public BrushProperty Brush2
    {
      get { return this.brush2; }
      set { this.brush2 = value; }
    }
    BrushProperty brush2;

    public BrushProperty Brush3
    {
      get { return this.brush3; }
      set { this.brush3 = value; }
    }
    BrushProperty brush3;

    public FontProperty Font1
    {
      get { return this.font1; }
      set { this.font1 = value; }
    }
    FontProperty font1;

    public FontProperty Font2
    {
      get { return this.font2; }
      set { this.font2 = value; }
    }
    FontProperty font2;

    public FontProperty Font3
    {
      get { return this.font3; }
      set { this.font3 = value; }
    }
    FontProperty font3;
  }

  public class GeneralProperties
  {
    public GeneralProperties()
    {
      this.backColor = new ColorProperty();
      this.backColor.Color = XColors.Ivory;
      this.tension = 0.5;
    }

    public ColorProperty BackColor
    {
      get { return this.backColor; }
      set { this.backColor = value; }
    }
    ColorProperty backColor;

    public XFillMode FillMode
    {
      get { return this.fillMode; }
      set { this.fillMode = value; }
    }
    XFillMode fillMode;

    public double Tension
    {
      get { return this.tension; }
      set { this.tension = value; }
    }
    double tension;

    public XGraphicsUnit PageUnit
    {
      get { return this.pageUnit; }
      set { this.pageUnit = value; }
    }
    XGraphicsUnit pageUnit;

    public XPageDirection PageDirection
    {
      get { return this.pageDirection; }
      set { this.pageDirection = value; }
    }
    XPageDirection pageDirection;

    public PdfColorMode ColorMode 
    {
      get { return this.colorMode; }
      set { this.colorMode = value; }
    }
    PdfColorMode colorMode;
  }

  public class ColorProperty
  {
    public ColorProperty()
    {
      this.color = XColors.Transparent;
    }

    [XmlIgnore]
    public XColor Color
    {
      get { return this.color; }
      set { this.color = value; }
    }
    XColor color;

    [XmlIgnore]
    public double A
    {
      get { return this.color.A; }
      set { this.color.A = value; }
    }

    [XmlIgnore]
    public byte R
    {
      get { return this.color.R; }
      set { this.color.R = value; }
    }

    [XmlIgnore]
    public byte G
    {
      get { return this.color.G; }
      set { this.color.G = value; }
    }

    [XmlIgnore]
    public byte B
    {
      get { return this.color.B; }
      set { this.color.B = value; }
    }

    [XmlIgnore]
    public double C
    {
      get { return this.color.C; }
      set { this.color.C = value; }
    }

    [XmlIgnore]
    public double M
    {
      get { return this.color.M; }
      set { this.color.M = value; }
    }

    [XmlIgnore]
    public double Y
    {
      get { return this.color.Y; }
      set { this.color.Y = value; }
    }

    [XmlIgnore]
    public double K
    {
      get { return this.color.K; }
      set { this.color.K = value; }
    }

    [XmlIgnore]
    public double GS
    {
      get { return this.color.GS; }
      set { this.color.GS = value; }
    }

    /// <summary>
    /// Special property for XmlSerializer only.
    /// </summary>
    [XmlAttribute("color")]
    public string rgbcmykgs
    {
      get { return this.color.RgbCmykG; }
      set { this.color.RgbCmykG = value; }
    }
  }

  public class PenProperty
  {
    public PenProperty()
    {
      this.color = new ColorProperty();
      this.color.Color = XColors.OliveDrab;
      this.width = 1;
    }

    [XmlIgnore]
    public XPen Pen
    {
      get
      {
        if (this.pen == null)
          this.pen = new XPen(this.color.Color, this.Width);
        else //if (this.dirty)
        {
          this.pen.Color = this.color.Color;
          this.pen.Width = this.Width;
          this.pen.DashStyle = this.DashStyle;
          this.pen.LineCap = this.LineCap;
          this.pen.LineJoin = this.LineJoin;
          //this.dirty = false;
        }
        return this.pen;
      }
    }
    //bool dirty = true;
    XPen pen;

    public ColorProperty Color
    {
      get { return this.color; }
      set { this.color = value; }
    }
    ColorProperty color;

    [XmlAttribute]
    public float Width
    {
      get { return this.width; }
      set { this.width = value; }
    }
    float width;

    [XmlAttribute]
    public XDashStyle DashStyle
    {
      get { return this.dashStyle; }
      set { this.dashStyle = value; }
    }
    internal XDashStyle dashStyle;

    [XmlAttribute]
    public XLineCap LineCap
    {
      get { return this.lineCap; }
      set { this.lineCap = value; }
    }
    internal XLineCap lineCap;

    [XmlAttribute]
    public XLineJoin LineJoin
    {
      get { return this.lineJoin; }
      set { this.lineJoin = value; }
    }
    internal XLineJoin lineJoin;
  }

  public class BrushProperty
  {
    public BrushProperty()
    {
      this.color = new ColorProperty();
      this.color.Color = XColors.GreenYellow;
    }

    [XmlIgnore]
    public XSolidBrush Brush
    {
      get
      {
        if (this.brush == null)
          this.brush = new XSolidBrush(this.color.Color);
        else //if (this.dirty)
        {
          this.brush.Color = this.color.Color;
          //this.dirty = false;
        }
        return this.brush;
      }
    }
    //bool dirty = true;
    XSolidBrush brush;

    public ColorProperty Color
    {
      get { return this.color; }
      set { this.color = value; }
    }
    ColorProperty color;
  }

  public class FontProperty
  {
    public FontProperty()
    {
      this.color = new ColorProperty();
      this.color.Color = XColors.DarkBlue;
      this.familyName = "Verdana";
    }

    [XmlIgnore]
    public XFont Font
    {
      get
      {
        XPdfFontOptions options = new XPdfFontOptions(
          Unicode ? PdfFontEncoding.Unicode : PdfFontEncoding.WinAnsi,
          Embed ? PdfFontEmbedding.Always : PdfFontEmbedding.None);

        this.font = new XFont(this.familyName, this.size, Style, options);
        //else //if (this.dirty)
        //{
        //  this.font.Color = this.color.Color;
        //  this.font.Size = this.size;
        //  this.font.Bold = this.bold;
        //  this.font.Italic = this.italic;
        //  //this.dirty = false;
        //}
        return this.font;
      }
    }
    //bool dirty = true;
    XFont font;

    [XmlIgnore]
    public XFontStyle Style
    {
      get
      {
        return (Bold ? XFontStyle.Bold : XFontStyle.Regular) |
              (Italic ? XFontStyle.Italic : XFontStyle.Regular) |
              (Underline ? XFontStyle.Underline : XFontStyle.Regular) |
              (Strikeout ? XFontStyle.Strikeout : XFontStyle.Regular);
      }
    }

    [XmlIgnore]
    public XSolidBrush Brush
    {
      get
      {
        if (this.brush == null)
          this.brush = new XSolidBrush(this.color.Color);
        else //if (this.dirty)
        {
          this.brush.Color = this.color.Color;
          //this.dirty = false;
        }
        return this.brush;
      }
    }
    //bool dirty = true;
    XSolidBrush brush;

    public ColorProperty Color
    {
      get { return this.color; }
      set { this.color = value; }
    }
    ColorProperty color;

    public string FamilyName
    {
      get { return this.familyName; }
      set { this.familyName = value; }
    }
    string familyName;

    public float Size
    {
      get { return this.size; }
      set { this.size = value; }
    }
    float size;

    public bool Bold
    {
      get { return this.bold; }
      set { this.bold = value; }
    }
    bool bold;

    public bool Italic
    {
      get { return this.italic; }
      set { this.italic = value; }
    }
    bool italic;

    public bool Underline
    {
      get { return this.underline; }
      set { this.underline = value; }
    }
    bool underline;

    public bool Strikeout
    {
      get { return this.strikeout; }
      set { this.strikeout = value; }
    }
    bool strikeout;

    public bool Unicode
    {
      get { return this.unicode; }
      set { this.unicode = value; }
    }
    bool unicode;

    public bool Embed
    {
      get { return this.embed; }
      set { this.embed = value; }
    }
    bool embed;

    public string Text
    {
      get { return this.text; }
      set { this.text = value; }
    }
    string text;
  }
}