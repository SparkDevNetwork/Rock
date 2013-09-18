using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PdfSharp.Drawing;
using System.Windows.Forms;

namespace XDrawing
{
  /// <summary>
  /// Interaction logic for WpfPreview.xaml
  /// </summary>
  public partial class WpfPreview : Window
  {
    public WpfPreview()
    {
      InitializeComponent();

      System.Drawing.Rectangle rect = Screen.PrimaryScreen.Bounds;
      Left = (double)rect.Width / 2;
      Top = (double)rect.Height / 2;
      Width = (double)rect.Width / 2;
      Height = (double)rect.Height / 2;
    }

    public void SetRenderEvent(RenderEvent e)
    {
      RenderEvent = e;
      //Preview.InvalidateVisual();
      OnRender();
    }
    public RenderEvent RenderEvent;

    internal void OnRender()
    {
      if (RenderEvent != null)
      {
        //IDocumentPaginatorSource source = this.documentViewer.Document;

        DrawingVisual dv = new DrawingVisual();
        DrawingContext dc = dv.RenderOpen();

        XGraphics gfx = XGraphics.FromDrawingContext(dc,
          new XSize(XUnit.FromMillimeter(210).Point, XUnit.FromMillimeter(297).Point), XGraphicsUnit.Point);
        try
        {
          RenderEvent(gfx);
        }
        catch { }
        dc.Close();
        //DrawingGroup dg = dv.Drawing;

        // Create page content
        PageContent pageContent = new PageContent();
        FixedPage fixedPage = new FixedPage();
        fixedPage.Background = Brushes.GhostWhite;
        //UIElement visual = dv; // CreateSecondVisual(false);

        UIElement visual = new DrawingVisualPresenter(dv);
        FixedPage.SetLeft(visual, 0);
        FixedPage.SetTop(visual, 0);

        double pageWidth = XUnit.FromMillimeter(210).Presentation;
        double pageHeight = XUnit.FromMillimeter(297).Presentation;

        fixedPage.Width = pageWidth;
        fixedPage.Height = pageHeight;

        fixedPage.Children.Add((UIElement)visual);

        Size size = new Size(pageWidth, pageHeight);
        fixedPage.Measure(size);
        fixedPage.Arrange(new Rect(new Point(), size));
        fixedPage.UpdateLayout();

        ((IAddChild)pageContent).AddChild(fixedPage);

        FixedDocument fixedDocument = new FixedDocument();
        fixedDocument.DocumentPaginator.PageSize = size;

        fixedDocument.Pages.Add(pageContent);

        this.documentViewer.Document = fixedDocument;

        string savedButton = System.Windows.Markup.XamlWriter.Save(fixedDocument);
      }
      else
        this.documentViewer.Document = null;



      //base.OnRender(drawingContext);
      ////drawingContext.DrawLine(new Pen(Brushes.Green, 10), new Point(10, 10), new Point(100, 150));

      //drawingContext.PushTransform(new ScaleTransform(0.75, 0.75));
      //XGraphics gfx = XGraphics.FromDrawingContext(drawingContext, new XSize(100, 100), XGraphicsUnit.Millimeter);
      //if (RenderEvent != null)
      //{
      //  try
      //  {
      //    RenderEvent(gfx);
      //  }
      //  catch
      //  {
      //    RenderEvent = null;
      //  }
      //}
      //else
      //  Draw(gfx);
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      //Preview.InvalidateVisual();
    }
  }
}