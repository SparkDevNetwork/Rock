using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using System.Windows.Controls.Primitives;

namespace DocumentViewer
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      // Create a new MigraDoc document
      Document document = SampleDocuments.CreateSample1();

      // HACK
      string ddl = DdlWriter.WriteToString(document);
      preview.Ddl = ddl;
    }

    private void Sample1_Click(object sender, RoutedEventArgs e)
    {
      Document document = SampleDocuments.CreateSample1();
      preview.Ddl = DdlWriter.WriteToString(document);
    }

    private void Sample2_Click(object sender, RoutedEventArgs e)
    {
      Directory.SetCurrentDirectory(GetProgramDirectory());
      Document document = SampleDocuments.CreateSample2();
      preview.Ddl = DdlWriter.WriteToString(document);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void CreatePdf_Click(object sender, RoutedEventArgs e)
    {
      PdfDocumentRenderer printer = new PdfDocumentRenderer();
      printer.DocumentRenderer = preview.Renderer;
      printer.Document = preview.Document;
      printer.RenderDocument();
      preview.Document.BindToRenderer(null);
      printer.Save("test.pdf");

      Process.Start("test.pdf");
    }

    private void OpenDDL_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog dialog = null;
      try
      {
        dialog = new OpenFileDialog();
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        dialog.Filter = "MigraDoc DDL (*.mdddl)|*.mdddl|All Files (*.*)|*.*";
        dialog.FilterIndex = 1;
        dialog.InitialDirectory = System.IO.Path.Combine(GetProgramDirectory(), "..\\..");
        //dialog.RestoreDirectory = true;
        if (dialog.ShowDialog() == true)
        {
          Document document = DdlReader.DocumentFromFile(dialog.FileName);
          string ddl = DdlWriter.WriteToString(document);
          preview.Ddl = ddl;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, Title);
        preview.Ddl = null; // TODO has no effect
      }
      finally
      {
        //if (dialog != null)
        //  dialog.Dispose();
      }
      //UpdateStatusBar();
    }

    private string GetProgramDirectory()
    {
      System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
      return System.IO.Path.GetDirectoryName(assembly.Location);
    }
  }
}
