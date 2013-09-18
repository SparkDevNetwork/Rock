using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace XDrawing.TestLab
{
  public delegate void UpdateDrawing();

  public class XGraphicsLab
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

      string s = "At least a user or an owner password is required to encrypt the document."; // PdfSharp.PSSR.UserOrOwnerPasswordRequired;
      s.GetType();
      XGraphicsLab.visualStyles = true;
      if (XGraphicsLab.visualStyles)
        Application.EnableVisualStyles();

      //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
      //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-AT");

      ReadProperties();
      //TODO validate ...
      new MainForm();
      Application.Run(mainForm);
      WriteProperties();
    }

    static void x()
    {
      StreamWriter sw = new StreamWriter("ansi.txt");
      for (int idx = 0; idx < 256; idx++)
      {
        byte[] bytes = new byte[1];
        bytes[0] = (byte)idx;
        sw.WriteLine(String.Format("0x{0:X4}", (int)(System.Text.Encoding.Default.GetString(bytes, 0, 1))[0]));
      }
      sw.Close();
    }

    public static MainForm mainForm;

    static void ReadProperties()
    {
      XmlReader reader = null;
      try
      {
        XmlSerializer ser = new XmlSerializer((typeof(GraphicsProperties)));
        if (File.Exists(propertyFilename))
        {
          reader = new XmlTextReader(propertyFilename);
          XGraphicsLab.properties = ser.Deserialize(reader) as GraphicsProperties;
        }
        else
          XGraphicsLab.properties = new GraphicsProperties();
      }
      catch
      {
        // ignore
      }
      finally
      {
        if (reader != null)
          reader.Close();
      }
    }

    static void WriteProperties()
    {
      XmlWriter writer = null;
      try
      {
        XmlSerializer ser = new XmlSerializer((typeof(GraphicsProperties)));
        writer = new XmlTextWriter(propertyFilename, Encoding.UTF8);
        ser.Serialize(writer, XGraphicsLab.properties);
        writer.Close();
      }
      catch
      {
        // ignore
      }
      finally
      {
        if (writer != null)
          writer.Close();
      }
    }

    static string propertyFilename = "GraphicsProperties.xml";

    internal static GraphicsProperties properties = new GraphicsProperties();

    static internal bool visualStyles;
  }
}
