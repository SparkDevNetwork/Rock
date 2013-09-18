using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XDrawing.Drawings
{
  /// <summary>
  /// Interaction logic for TestControl.xaml
  /// </summary>
  public partial class TestControl : UserControl
  {
    public TestControl()
    {
      InitializeComponent();
      BuildDrawing();
    }

    void BuildDrawing()
    {
      this.drawing = new GeometryDrawing();

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
      this.drawing.Geometry = ellipses;

      // Specify the drawing's fill.
      this.drawing.Brush = Brushes.Blue;

      // Specify the drawing's stroke.
      Pen stroke = new Pen();
      stroke.Thickness = 10.0;
      stroke.Brush = new LinearGradientBrush(
          Colors.Black, Colors.Gray, new Point(0, 0), new Point(1, 1));
      this.drawing.Pen = stroke;

      // Create a DrawingBrush
      DrawingBrush myDrawingBrush = new DrawingBrush();
      myDrawingBrush.Drawing = this.drawing;

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
    }

    GeometryDrawing drawing;
    //internal Drawing drawing2;

    protected override void OnRender(DrawingContext dc)
    {
      //base.OnRender(dc);

      dc.DrawLine(new Pen(Brushes.Green, 10), new Point(0, 0), new Point(10000, 10000));
      dc.DrawEllipse(Brushes.Red, null, new Point(200, 200), 100, 100);
      dc.DrawDrawing(this.drawing);
    }

  }
}
