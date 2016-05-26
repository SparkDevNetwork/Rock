using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ConsoleApplication9
{
    class Program
    {
        static void Main( string[] args )
        {
            var reader = new PdfReader( new byte[5] );
            PdfReaderContentParser parser = new PdfReaderContentParser( reader );
            MyImageRenderListener listener = new MyImageRenderListener();

            for ( int i = 1; i <= reader.NumberOfPages; i++ )
            {
                parser.ProcessContent( i, listener );
            }

            for ( int i = 0; i < listener.Images.Count; ++i )
            {
                // todo
            }
        }
    }

    public class MyImageRenderListener : IRenderListener
    {
        public void RenderText( TextRenderInfo renderInfo ) { }
        public void BeginTextBlock() { }
        public void EndTextBlock() { }

        public List<byte[]> Images = new List<byte[]>();
        public List<string> ImageNames = new List<string>();
        public void RenderImage( ImageRenderInfo renderInfo )
        {
            PdfImageObject image = renderInfo.GetImage();
            try
            {
                image = renderInfo.GetImage();
                if ( image == null ) return;

                ImageNames.Add( string.Format(
                  "Image{0}.{1}", renderInfo.GetRef().Number, image.GetFileType()
                ) );
                using ( MemoryStream ms = new MemoryStream( image.GetImageAsBytes() ) )
                {
                    Images.Add( ms.ToArray() );
                }
            }
            catch ( IOException ie )
            {
                /*
                 * pass-through; image type not supported by iText[Sharp]; e.g. jbig2
                */
            }
        }
    }


}
