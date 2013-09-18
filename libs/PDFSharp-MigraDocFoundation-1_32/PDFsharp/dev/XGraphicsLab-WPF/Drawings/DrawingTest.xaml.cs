using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Markup;

namespace XDrawing.Drawings
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class DrawingTest : Window
  {
    public DrawingTest()
    {
      InitializeComponent();
      //BuildDrawing();
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      Preview.InvalidateVisual();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      string filename = "Test.txt";
      File.Delete(filename);
      XpsDocument doc = new XpsDocument(filename, System.IO.FileAccess.ReadWrite);
      XpsDocumentWriter xpsdw = XpsDocument.CreateXpsDocumentWriter(doc);

      //PageContent content = CreateFifthPageContent();
      FixedPage page = CreateFifthPageContent();
      //page.conten
//      page.
      xpsdw.Write(page);
      doc.Close();
    }

    private FixedPage CreateFifthPageContent()
    {
      //PageContent pageContent = new PageContent();
      FixedPage fixedPage = new FixedPage();
      UIElement visual = BuildDrawing(); // CreateThirdVisual(false);

      FixedPage.SetLeft(visual, 0);
      FixedPage.SetTop(visual, 0);

      double pageWidth = 96 * 8.5;
      double pageHeight = 96 * 11;

      fixedPage.Width = pageWidth;
      fixedPage.Height = pageHeight;

      fixedPage.Children.Add((UIElement)visual);

      Size sz = new Size(8.5 * 96, 11 * 96);
      fixedPage.Measure(sz);
      fixedPage.Arrange(new Rect(new Point(), sz));
      fixedPage.UpdateLayout();

      //((IAddChild)pageContent).AddChild(fixedPage);
      return fixedPage;
    }

    StackPanel BuildDrawing()
    {
      GeometryDrawing drawing = new GeometryDrawing();

      // Use geometries to describe two overlapping ellipses.
      EllipseGeometry ellipse1 = new EllipseGeometry();
      ellipse1.RadiusX = 20;
      ellipse1.RadiusY = 45;
      ellipse1.Center = new Point(50, 50);
      EllipseGeometry ellipse2 = new EllipseGeometry();
      ellipse2.RadiusX = 45;
      ellipse2.RadiusY = 20;
      ellipse2.Center = new Point(50, 50);
      GeometryGroup ellipses = new GeometryGroup();
      ellipses.Children.Add(ellipse1);
      ellipses.Children.Add(ellipse2);

      // Add the geometry to the drawing.
      drawing.Geometry = ellipses;

      // Specify the drawing's fill.
      drawing.Brush = Brushes.Blue;

      // Specify the drawing's stroke.
      Pen stroke = new Pen();
      stroke.Thickness = 10.0;
      stroke.Brush = new LinearGradientBrush(
          Colors.Black, Colors.Gray, new Point(0, 0), new Point(1, 1));
      drawing.Pen = stroke;

      // Create a DrawingBrush
      DrawingBrush myDrawingBrush = new DrawingBrush();
      myDrawingBrush.Drawing = drawing;

      // Create a Rectangle element.
      Rectangle aRectangle = new Rectangle();
      aRectangle.Width = 150;
      aRectangle.Height = 150;
      aRectangle.Stroke = Brushes.Black;
      aRectangle.StrokeThickness = 1.0;

      // Use the DrawingBrush to paint the rectangle's
      // background.
      aRectangle.Fill = myDrawingBrush;

      StackPanel mainPanel = new StackPanel();
      mainPanel.Children.Add(aRectangle);

      mainPanel.Arrange(new Rect(100, 100, 500, 500));
      //this.drawing2 = mainPanel;
      //this.Content = mainPanel;
      return mainPanel;
    }

    //void BuildDrawing()
    //{
    //  this.drawing = new GeometryDrawing();

    //  // Use geometries to describe two overlapping ellipses.
    //  EllipseGeometry ellipse1 = new EllipseGeometry();
    //  ellipse1.RadiusX = 20;
    //  ellipse1.RadiusY = 45;
    //  ellipse1.Center = new Point(50, 50);
    //  EllipseGeometry ellipse2 = new EllipseGeometry();
    //  ellipse2.RadiusX = 45;
    //  ellipse2.RadiusY = 20;
    //  ellipse2.Center = new Point(50, 50);
    //  GeometryGroup ellipses = new GeometryGroup();
    //  ellipses.Children.Add(ellipse1);
    //  ellipses.Children.Add(ellipse2);

    //  // Add the geometry to the drawing.
    //  this.drawing.Geometry = ellipses;

    //  // Specify the drawing's fill.
    //  this.drawing.Brush = Brushes.Blue;

    //  // Specify the drawing's stroke.
    //  Pen stroke = new Pen();
    //  stroke.Thickness = 10.0;
    //  stroke.Brush = new LinearGradientBrush(
    //      Colors.Black, Colors.Gray, new Point(0, 0), new Point(1, 1));
    //  this.drawing.Pen = stroke;

    //  // Create a DrawingBrush
    //  DrawingBrush myDrawingBrush = new DrawingBrush();
    //  myDrawingBrush.Drawing = this.drawing;

    //  // Create a Rectangle element.
    //  Rectangle aRectangle = new Rectangle();
    //  aRectangle.Width = 150;
    //  aRectangle.Height = 150;
    //  aRectangle.Stroke = Brushes.Black;
    //  aRectangle.StrokeThickness = 1.0;

    //  // Use the DrawingBrush to paint the rectangle's
    //  // background.
    //  aRectangle.Fill = myDrawingBrush;

    //  StackPanel mainPanel = new StackPanel();
    //  mainPanel.Children.Add(aRectangle);

    //  mainPanel.Arrange(new Rect(100, 100, 500, 500));
    //  //this.drawing2 = mainPanel;
    //  //this.Content = mainPanel;
    //}

    //GeometryDrawing drawing;

    //Drawing drawing2;
    //protected override void OnRender(DrawingContext dc)
    //{
    //  //base.OnRender(dc);

    //  dc.DrawLine(new Pen(Brushes.Green, 10), new Point(0, 0), new Point(10000, 10000));
    //  dc.DrawEllipse(Brushes.Red, null, new Point(200, 200), 100, 100);
    //  dc.DrawDrawing(this.drawing);
    //}
  }
}