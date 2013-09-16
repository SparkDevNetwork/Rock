using System;
using System.Collections.Generic;
using System.Text;
//using MigraDoc.DocumentObjectModel;
//using MigraDoc.Rendering;
using PdfSharp.Drawing;
namespace XDrawing.TestLab.Tester
{
  class ShapesMigraDocObject : TesterBase
  {
    public override void RenderPage(PdfSharp.Drawing.XGraphics gfx)
    {
      base.RenderPage(gfx);

      //Document doc = new Document();
      //Section sec = doc.AddSection();
      //Paragraph para = sec.AddParagraph("MigraDoc Paragraph");
      //para.Format.Borders.Color = Colors.Gold;

      //MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
      //docRenderer.PrepareDocument();

      //docRenderer.RenderObject(gfx, XUnit.FromCentimeter(10), XUnit.FromCentimeter(11), "5cm", para);
    }
    public override string Description
    {
      get { return "MigraDoc Document Object"; }
    }
  }
}
