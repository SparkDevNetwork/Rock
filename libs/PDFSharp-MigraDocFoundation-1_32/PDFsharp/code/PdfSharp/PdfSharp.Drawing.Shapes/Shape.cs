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

#if under_constructioin
namespace PdfSharp.Drawing.Shapes
{
  public sealed class Ellipse : Shape
  {
    //    // Methods
    //    static Ellipse();
    //    public Ellipse();
    //    protected override Size ArrangeOverride(Size finalSize);
    //    protected override Size MeasureOverride(Size constraint);
    //    protected override void OnRender(DrawingContext dc);
    //
    //    // Properties
    //    public double CenterX { get; set; }
    //    public double CenterY { get; set; }
    //    public double RadiusX { get; set; }
    //    public double RadiusY { get; set; }
    //
    //    // Fields
    //    private Rect _shapeBounds;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty CenterXProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty CenterYProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty RadiusXProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty RadiusYProperty;
  }

  public sealed class Glyphs : Shape
  {
    //    // Methods
    //    static Glyphs();
    //    public Glyphs();
    //    protected override Size ArrangeOverride(Size finalSize);
    //    public GlyphRun ComputeGlyphRun();
    //    private void ComputeMeasurementGlyphRunAndOrigin();
    //    private static double GetAdvanceWidth(GlyphTypeface glyphTypeface, ushort glyphIndex, bool sideways);
    //    private static bool IsEmpty(string s);
    //    protected override Size MeasureOverride(Size constraint);
    //    protected override void OnRender(DrawingContext ctx);
    //    private IList<bool> ParseCaretStops();
    //    [SecurityCritical, SecurityTreatAsSafe]
    //    private void ParseGlyphRunProperties();
    //    private int ParseGlyphsProperty(GlyphTypeface fontFace, string unicodeString, bool sideways, out List<ParsedGlyphData> parsedGlyphs, out ushort[] clusterMap);
    //    private bool ReadGlyphIndex(string valueSpec, ref bool inCluster, ref int glyphClusterSize, ref int characterClusterSize, ref ushort glyphIndex);
    //
    //    // Properties
    //    public int BidiLevel { get; set; }
    //    public string CaretStops { get; set; }
    //    [Obsolete("This property is obsolete and will be removed in a future version. Use the fragment portion of FontUri property to specify TTC index instead.")]
    //    public int FontFaceIndex { get; set; }
    //    public double FontRenderingEmSize { get; set; }
    //    public Uri FontUri { get; set; }
    //    public string Indices { get; set; }
    //    public bool IsSideways { get; set; }
    //    public double OriginX { get; set; }
    //    public double OriginY { get; set; }
    //    public StyleSimulations StyleSimulations { get; set; }
    //    Uri IUriContext.BaseUri { get; set; }
    //    public string UnicodeString { get; set; }
    //
    //    // Fields
    //    private Point _glyphRunOrigin;
    //    private LayoutDependentGlyphRunProperties _glyphRunProperties;
    //    private GlyphRun _measurementGlyphRun;
    //    public static readonly DependencyProperty BidiLevelProperty;
    //    public static readonly DependencyProperty CaretStopsProperty;
    //    private const double EmMultiplier = 100;
    //    [Obsolete("This property is obsolete and will be removed in a future version. Use the fragment portion of FontUri property to specify TTC index instead.")]
    //    public static readonly DependencyProperty FontFaceIndexProperty;
    //    [TypeConverter("System.Windows.FontSizeConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty FontRenderingEmSizeProperty;
    //    public static readonly DependencyProperty FontUriProperty;
    //    public static readonly DependencyProperty IndicesProperty;
    //    public static readonly DependencyProperty IsSidewaysProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty OriginXProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty OriginYProperty;
    //    public static readonly DependencyProperty StyleSimulationsProperty;
    //    public static readonly DependencyProperty UnicodeStringProperty;
    //
    //    // Nested Types
    //    private class LayoutDependentGlyphRunProperties
    //    {
    //      // Methods
    //      public LayoutDependentGlyphRunProperties();
    //      public GlyphRun CreateGlyphRun(Point origin, CultureInfo culture);
    //
    //      // Fields
    //      public double[] advanceWidths;
    //      public int bidiLevel;
    //      public IList<bool> caretStops;
    //      public ushort[] clusterMap;
    //      public double fontRenderingSize;
    //      public ushort[] glyphIndices;
    //      public Point[] glyphOffsets;
    //      public GlyphTypeface glyphTypeface;
    //      public bool sideways;
    //      public string unicodeString;
    //    }
    //
    //    private class ParsedGlyphData
    //    {
    //      // Methods
    //      public ParsedGlyphData();
    //
    //      // Fields
    //      public double advanceWidth;
    //      public ushort glyphIndex;
    //      public double offsetX;
    //      public double offsetY;
    //     }
  }

  public sealed class Line : StretchableShape
  {
    //    // Methods
    //    static Line();
    //    public Line();
    //    internal override void CacheDefiningGeometry();
    //
    //    // Properties
    //    protected override Geometry DefiningGeometry { get; }
    //    public double X1 { get; set; }
    //    public double X2 { get; set; }
    //    public double Y1 { get; set; }
    //    public double Y2 { get; set; }
    //
    //    // Fields
    //    private LineGeometry _lineGeometry;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty X1Property;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty X2Property;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty Y1Property;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty Y2Property;
  }

  public sealed class Path : StretchableShape
  {
    //    // Methods
    //    static Path();
    //    public Path();
    //    internal override void CacheDefiningGeometry();
    //    private static void PathDataInvalidator(DependencyObject d);
    //
    //    // Properties
    //    public Geometry Data { get; set; }
    //    protected override Geometry DefiningGeometry { get; }
    //    public FillRule FillRule { get; set; }
    //
    //    // Fields
    //    private Geometry _cachedGeometry;
    //    public static readonly DependencyProperty DataProperty;
    //    public static readonly DependencyProperty FillRuleProperty;
  }

  public sealed class Polygon : StretchableShape
  {
    //    // Methods
    //    static Polygon();
    //    public Polygon();
    //    internal override void CacheDefiningGeometry();
    //
    //    // Properties
    //    protected override Geometry DefiningGeometry { get; }
    //    public FillRule FillRule { get; set; }
    //    public PointCollection Points { get; set; }
    //
    //    // Fields
    //    private PathGeometry _polygonGeometry;
    //    public static readonly DependencyProperty FillRuleProperty;
    //    public static readonly DependencyProperty PointsProperty;
  }

  public sealed class Polyline : StretchableShape
  {
    //    // Methods
    //    static Polyline();
    //    public Polyline();
    //    internal override void CacheDefiningGeometry();
    //    protected override void OnRender(DrawingContext dc);
    //
    //    // Properties
    //    protected override Geometry DefiningGeometry { get; }
    //    public FillRule FillRule { get; set; }
    //    public PointCollection Points { get; set; }
    //
    //    // Fields
    //    private PathGeometry _polylineGeometry;
    //    public static readonly DependencyProperty FillRuleProperty;
    //    public static readonly DependencyProperty PointsProperty;
  }

  public sealed class Rectangle : StretchableShape
  {
    //    // Methods
    //    static Rectangle();
    //    public Rectangle();
    //    protected override Size ArrangeOverride(Size finalSize);
    //    internal override void CacheDefiningGeometry();
    //    internal override Rect GetDefiningGeometryBounds();
    //    internal override Size GetNaturalSize();
    //    protected override Size MeasureOverride(Size constraint);
    //    protected override void OnRender(DrawingContext dc);
    //
    //    // Properties
    //    protected override Geometry DefiningGeometry { get; }
    //    public override Transform GeometryTransform { get; }
    //    public double RadiusX { get; set; }
    //    public double RadiusY { get; set; }
    //    public override Geometry RenderedGeometry { get; }
    //
    //    // Fields
    //    private Rect _rect;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty RadiusXProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty RadiusYProperty;
  }

  public abstract class Shape
  {
    //    // Methods
    //    static Shape();
    //    protected Shape();
    //    internal Pen GetPen(Size sizeReference);
    //    internal double GetStrokeThickness();
    //    internal static bool IsDoubleFinite(object o);
    //    internal static bool IsDoubleFiniteNonNegative(object o);
    //    private static void PenInvalidated(DependencyObject d);
    //
    //    // Properties
    //    public Brush Fill { get; set; }
    //    internal bool IsPenNoOp { get; }
    //    public Brush Stroke { get; set; }
    //    public DoubleCollection StrokeDashArray { get; set; }
    //    public PenLineCap StrokeDashCap { get; set; }
    //    public double StrokeDashOffset { get; set; }
    //    public PenLineCap StrokeEndLineCap { get; set; }
    //    public PenLineJoin StrokeLineJoin { get; set; }
    //    public double StrokeMiterLimit { get; set; }
    //    public PenLineCap StrokeStartLineCap { get; set; }
    //    public double StrokeThickness { get; set; }
    //
    //    // Fields
    //    private Pen _pen;
    //    public static readonly DependencyProperty FillProperty;
    //    public static readonly DependencyProperty StrokeDashArrayProperty;
    //    public static readonly DependencyProperty StrokeDashCapProperty;
    //    public static readonly DependencyProperty StrokeDashOffsetProperty;
    //    public static readonly DependencyProperty StrokeEndLineCapProperty;
    //    public static readonly DependencyProperty StrokeLineJoinProperty;
    //    public static readonly DependencyProperty StrokeMiterLimitProperty;
    //    public static readonly DependencyProperty StrokeProperty;
    //    public static readonly DependencyProperty StrokeStartLineCapProperty;
    //    [TypeConverter("System.Windows.LengthConverter, PresentationFramework, Version=6.0.4030.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
    //    public static readonly DependencyProperty StrokeThicknessProperty;
  }

  public abstract class StretchableShape : Shape
  {
    //    // Methods
    //    static StretchableShape();
    //    internal StretchableShape();
    //    protected override Size ArrangeOverride(Size finalSize);
    //    internal abstract void CacheDefiningGeometry();
    //    internal Stretch GetAdjustedStretch(Size size);
    //    internal virtual Rect GetDefiningGeometryBounds();
    //    internal virtual Size GetNaturalSize();
    //    protected override Size MeasureOverride(Size constraint);
    //    protected override void OnRender(DrawingContext dc);
    //    internal void Render(DrawingContext dc, Brush brush);
    //    internal void ResetStretchMatrix();
    //    internal void SetStretchedGeometry(Size finalSize);
    //    internal void SetStretchMatrix(Size size, Stretch mode, double penThickness);
    //
    //    // Properties
    //    protected abstract Geometry DefiningGeometry { get; }
    //    public virtual Transform GeometryTransform { get; }
    //    public virtual Geometry RenderedGeometry { get; }
    //    public Stretch Stretch { get; set; }
    //
    //    // Fields
    //    private Geometry _renderedGeometry;
    //    private Matrix _stretchMatrix;
    //    public static readonly DependencyProperty StretchProperty;
  }
}
#endif