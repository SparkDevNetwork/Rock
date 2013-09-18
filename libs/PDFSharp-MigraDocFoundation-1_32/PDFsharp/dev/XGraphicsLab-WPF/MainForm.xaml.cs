using System;
using System.Diagnostics;
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
using PdfSharp.Pdf;
using System.Windows.Interop;
using XDrawing.TestLab.Tester;

namespace XDrawing
{
  /// <summary>
  /// Interaction logic for MainForm.xaml
  /// </summary>
  public partial class MainForm : Window, System.Windows.Forms.IWin32Window, IWin32Window
  {
    public MainForm()
    {
      XDrawing.TestLab.XGraphicsLab.mainForm = this;
      InitializeComponent();
      Show();
      //Window2 dlg = new Window2();
      //dlg.ShowDialog();
      //if (this.previewGdi == null)
      //{
      //  this.previewGdi = new XDrawing.Forms.PreviewGdi();
      //  this.previewGdi.Show(this);
      //}
      if (this.preview == null)
      {
        this.preview = new XDrawing.TestLab.PreviewForm();
        this.preview.Show(this);
      }

      if (this.propertiesForm == null)
      {
        this.propertiesForm = new XDrawing.TestLab.PropertiesForm();
        this.propertiesForm.Show(this);
      }

      if (this.wpfPreview == null)
      {
        this.wpfPreview = new WpfPreview();
        this.wpfPreview.Owner = this;
        this.wpfPreview.Show();
      }

      //if (this.drawingTest == null)
      //{
      //  this.drawingTest = new XDrawing.Drawings.DrawingTest();
      //  this.drawingTest.Owner = this;
      //  this.drawingTest.Show();
      //}
      Test_Click();
    }

    public IntPtr Handle
    {
      get
      {
        if (this.windowInteropHelper == null)
          this.windowInteropHelper = new WindowInteropHelper(this);
        return this.windowInteropHelper.Handle;
      }
    }
    WindowInteropHelper windowInteropHelper;

    //Forms.PreviewGdi previewGdi;

    internal XDrawing.TestLab.PreviewForm preview;

    XDrawing.TestLab.PropertiesForm propertiesForm;

    XDrawing.TestLab.Tester.TesterBase tester;

    internal XDrawing.WpfPreview wpfPreview;

    //internal XDrawing.Drawings.DrawingTest drawingTest;

    private void Test_Click()
    {
      this.tester = new XDrawing.TestLab.Tester.LinesStraightLines();
      this.preview.SetRenderEvent(new PdfSharp.Forms.PagePreview.RenderEvent(this.tester.RenderPage));
      this.wpfPreview.SetRenderEvent(new RenderEvent(this.tester.RenderPage));
    }

    protected override void OnActivated(EventArgs e)
    {
      base.OnActivated(e);

      //if (this.previewGdi == null)
      //{
      //  this.previewGdi = new XGraphicsOnWpf.Forms.PreviewGdi();
      //  this.previewGdi.Show(this);
      //}
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      //PdfDocument document = new PdfDocument();
      //PdfPage page = document.AddPage();
      //page.Width = XUnit.FromCentimeter(21);
      //page.Height = XUnit.FromCentimeter(29.7);
      //XGraphics gfx = XGraphics.FromPdfPage(page);

      //this.UserControl1.Draw(gfx);
      //string file = Guid.NewGuid().ToString() + ".pdf";
      //document.Save(file);
      //Process.Start(file);
    }

    private void tvTest_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      TreeViewItem item = this.tvTest.SelectedItem as TreeViewItem;
      if (item != null && item.Tag != null)
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(typeof(TesterBase).FullName) + "." + item.Tag.ToString();
        Type type = Type.GetType(name);
        if (type != null)
        {
          this.tester = (TesterBase)type.GetConstructor(Type.EmptyTypes).Invoke(null);
          this.preview.SetRenderEvent(new PdfSharp.Forms.PagePreview.RenderEvent(this.tester.RenderPage));
          this.wpfPreview.SetRenderEvent(new RenderEvent(this.tester.RenderPage));
          (this.statusBar1.Items[0] as System.Windows.Controls.Primitives.StatusBarItem).Content = this.tester.Description;

          //this.tester = new XDrawing.TestLab.Tester.LinesStraightLines();
          //this.preview.SetRenderEvent(new PdfSharp.Forms.PagePreview.RenderEvent(this.tester.RenderPage));

        }

      }
    }

  }
}