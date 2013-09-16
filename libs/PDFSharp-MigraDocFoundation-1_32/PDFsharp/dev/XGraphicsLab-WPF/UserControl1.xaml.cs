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
using PdfSharp.Drawing;

namespace XDrawing
{
  /// <summary>
  /// A delegate for invoking the render function.
  /// </summary>
  public delegate void RenderEvent(XGraphics gfx);

  public partial class UserControl1 : UserControl
  {
    public UserControl1()
    {
      InitializeComponent();
    }

    public RenderEvent RenderEvent;

    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
      //drawingContext.DrawLine(new Pen(Brushes.Green, 10), new Point(10, 10), new Point(100, 150));

      drawingContext.PushTransform(new ScaleTransform(0.75, 0.75));
      XGraphics gfx = XGraphics.FromDrawingContext(drawingContext, new XSize(100, 100), XGraphicsUnit.Millimeter);
      if (RenderEvent != null)
      {
        try
        {
          RenderEvent(gfx);
        }
        catch
        {
          RenderEvent = null;
        }
      }
      else
        Draw(gfx);
    }

    public void Draw(XGraphics gfx)
    {
      string s = "Testtext";
      //gfx.DrawLine(XPens.GreenYellow, 5, 100, 30, 50);
      //gfx.DrawEllipse(XBrushes.DarkBlue, new XRect(30, 40, 250, 235));
      XFont font = new XFont("Arial", 40, XFontStyle.Italic);
      gfx.DrawString(s, font, XBrushes.Firebrick, 40, 60);
      XSize size = gfx.MeasureString(s, font);
      gfx.DrawLine(XPens.DarkBlue, 40, 60, 40 + size.Width, 60);
      gfx.DrawLine(XPens.DarkBlue, 40, 60, 40, 60 + size.Height);
    }
  }
}