using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Checks some internal optimizations in PDF creation.
  /// </summary>
  public class Unicode : TesterBase
  {
    public Unicode()
    {
    }

    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XSize page = gfx.PageSize;

      XTextFormatter tf = new XTextFormatter(gfx);
      XRect rect;
      string text = this.properties.Font1.Text;
      text = "English: The quick brown fox jumps over the lazy dog.\n\n" +
        "Brazilian Portuguese: A rápida raposa marrom ataca o cão preguiçoso.\n\n" +
        "Czech: Příliš žluťoučký kůň úpěl ďábelské ódy.\n\n" +
        "Portuguese: A rápida raposa castanha salta em cima do cão lento.\n\n" +
        "German: Franz jagt im total verwahrlosten Taxi quer durch Bayern.\n\n" +
        "Hungarian: Árvíztűrő tükörfúrógép ÁRVÍZTŰRŐ TÜKÖRFÚRÓGÉP.\n\n" +
        "Swedish: Flygande beckasiner söka hwila på mjuka tufvor.\n\n" +
        "Danish: Quizdeltagerne spiste jordbær med flød.\n\n" +
        "Russian: Съешь еще этих мягких французских булок, да выпей чаю.\n\n" +
        "Romanian: Agera vulpe maronie sare peste câinele cel leneş.\n\n" +
        "Spanish: El veloz murciélago hindú comía feliz cardillo y kiwi. La cigüeña tocaba el saxofón detrás del palenque de paja.\n\n";

      rect = new XRect(50, 100, page.Width - 100, page.Height - 300);
      //gfx.DrawRectangle(XBrushes.SeaShell, rect);
      tf.DrawString(text, this.properties.Font1.Font, this.properties.Font1.Brush, 
        rect, XStringFormats.TopLeft);

      if (!this.properties.Font1.Unicode)
      {
        rect = new XRect(50, page.Height - 200, page.Width - 100, 110);
        gfx.DrawRectangle(XBrushes.LightPink, rect);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString("\nBefore creating a PDF file go to the Font tab in the XGraphics Properties window and check the Unicode option.\n\n" + 
          "Otherwise Cyrillic characters will not be visible in the PDF file.",
          this.properties.Font1.Font, this.properties.Font1.Brush, 
          rect, XStringFormats.TopLeft);
      }
    }

    public override string Description
    {
      get {return "Unicode";}
    }
  }
}
