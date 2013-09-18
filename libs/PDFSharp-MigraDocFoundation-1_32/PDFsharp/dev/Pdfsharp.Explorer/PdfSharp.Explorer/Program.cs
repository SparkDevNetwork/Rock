using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// Entry class of PDFshpar Explorer.
  /// </summary>
  public class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args) 
    {
      ExplorerProcess process = new ExplorerProcess();
      if (args.Length < 2)
      {
        if (args.Length == 1)
        {
          try
          {
            process.OpenDocument(args[0]);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.Message, VersionInfo.Title);
            return;
          }
        }
        Application.Run(new MainForm(process));
      }
      else
      {
        switch (args[1])
        {
          case "/format":
            process.FormatDocument(args[0]);
            break;

          default:
            MessageBox.Show("Invalid command: '" + args[0] + "'.", VersionInfo.Title);
            break;
        }
      }
    }
  }
}
