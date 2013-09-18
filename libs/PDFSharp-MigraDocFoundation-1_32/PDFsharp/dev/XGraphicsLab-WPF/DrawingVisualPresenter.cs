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
using System.Windows.Shapes;

namespace XDrawing
{
  public class DrawingVisualPresenter : FrameworkElement
  {
    public DrawingVisualPresenter(DrawingVisual visual)
    {
      this.visual = visual;
    }

    protected override int VisualChildrenCount
    {
      get { return 1; }
    }

    protected override Visual GetVisualChild(int index)
    {
      if (index != 0)
        throw new ArgumentOutOfRangeException("index");
      return visual;
    }

    DrawingVisual visual;
  }
}