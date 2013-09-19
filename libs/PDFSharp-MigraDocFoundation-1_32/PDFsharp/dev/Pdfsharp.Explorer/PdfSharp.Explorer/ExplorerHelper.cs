using System;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfSharp.Explorer
{
  /// <summary>
  /// Static helper functions
  /// </summary>
  class ExplorerHelper
  {
    ExplorerHelper() {}

    /// <summary>
    /// Converts a byte array into a hex dump.
    /// </summary>
    public static string HexDump(byte[] data)
    {
      // "00000000  xx xx xx xx xx xx xx xx  xx xx xx xx xx xx xx xx  ................";
      StringBuilder dump = new StringBuilder();
      int count = data.Length;
      int lines = count / 16 + (count % 16 == 0 ? 0 : 1);
      int idx = 0;
      for (int line = 0; line < lines; line++)
      {
        dump.AppendFormat("{0:X8}  ", idx);
        char[] text = new string(' ', 66).ToCharArray();
        int ichHex = 0;
        int ichChr = 50;
        for (int clm = 0; clm < 16; clm++, idx++)
        {
          if (idx >= count)
            break;
          byte b = data[idx];
          text[ichHex++] = (char)((b >> 4) + ((b >> 4) < 10 ? (byte)'0' : (byte)('A' - 10)));
          text[ichHex++] = (char)((b & 0xF) + ((b & 0xF) < 10 ? (byte)'0' : (byte)('A' - 10)));
          if (b < 32)
            b = (byte)'.';
          text[ichChr++] = (char)b;
          ichHex += clm == 7 ? 2 : 1;
        }
        dump.Append(text);
        dump.Append('\n');
      }
      return dump.ToString();
    }

    /// <summary>
    /// Gets page size in mm or inch.
    /// </summary>
    public static string PageSize(PdfPage page, bool metric)
    {
      if (metric)
        return String.Format("{0:0.#} x {1:0.#} mm", 
          XUnit.FromPoint(page.Width).Millimeter,
          XUnit.FromPoint(page.Height).Millimeter);
      else
        return String.Format("{0:0.#} x {1:0.#} inch", 
          XUnit.FromPoint(page.Width).Inch,
          XUnit.FromPoint(page.Height).Inch);
    }
  }
}
