using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageResizer;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Rock.Data;
using Rock.Model;

namespace CleanupBinaryFileData
{
    public partial class Form1 : Form
    {
        const int binaryFileTypeCovenantId = 11;
        double totalCount = 0;
        double processedCount = 0;
        double percentComplete = 0;
        ResizeSettings settings = new ResizeSettings();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click( object sender, EventArgs e )
        {
            settings.MaxWidth = 600;

            var binaryFileService = new BinaryFileService( new RockContext() );
            
            var convenantPdfQuery = binaryFileService.Queryable().Where( a => a.BinaryFileTypeId == binaryFileTypeCovenantId && a.MimeType == "application/pdf" );
            var convenantPdfList = convenantPdfQuery.Select( a => new
            {
                a.Id
            } ).OrderBy( a => a ).ToList();
            totalCount = convenantPdfList.Count();
            
            Parallel.ForEach( convenantPdfList, ( convenantPdf ) =>
            {
                string mergedFileName = string.Format( "C:\\output\\merged_resized\\{0}_mergedImages.jpg", convenantPdf.Id);
                if ( !File.Exists(mergedFileName))
                {
                    try
                    {
                        ExtractMergedJPG( convenantPdf.Id,  mergedFileName );
                    }
                    catch (OutOfMemoryException)
                    {
                        GC.Collect();
                    }
                }
                
                processedCount++;
            } );
        }

        private void ExtractMergedJPG( int convenantPdfId, string mergedFileName )
        {
            ImageCodecInfo jpgEncoder = GetEncoder( ImageFormat.Jpeg );

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;
            
            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters( 1 );

            EncoderParameter myEncoderParameter = new EncoderParameter( myEncoder, 50L );
            myEncoderParameters.Param[0] = myEncoderParameter;
            
            var binaryFileDataService = new BinaryFileDataService( new RockContext() );
            var content = binaryFileDataService.Queryable().Where( a => a.Id == convenantPdfId ).Select( a => a.Content ).FirstOrDefault();

            var origPdf = string.Format( "C:\\output\\orig_pdfs\\{0}.pdf", convenantPdfId );
            File.WriteAllBytes( origPdf, content );
            var reader = new PdfReader( content );
            PdfReaderContentParser parser = new PdfReaderContentParser( reader );
            MyImageRenderListener listener = new MyImageRenderListener();

            for ( int i = 1; i <= reader.NumberOfPages; i++ )
            {
                parser.ProcessContent( i, listener );
            }

            int mergedImageWidth = 0;
            int mergedImageHeight = 0;
            var imagesToMerge = new List<Bitmap>();

            for ( int i = 0; i < listener.Images.Count; ++i )
            {
                var imageData = listener.Images[i];
                using ( var imageStream = new MemoryStream( imageData ) )
                {
                    try
                    {
                        using ( Bitmap image = new Bitmap( imageStream ) )
                        {
                            //image.Save( string.Format( "C:\\output\\{0}_{1}_Orig.jpg", convenantPdf.Id, i ) );
                            MemoryStream resizedStream = new MemoryStream();

                            ImageBuilder.Current.Build( image, resizedStream, settings );
                            Bitmap jpgImageResized = new Bitmap( resizedStream );
                            mergedImageHeight += jpgImageResized.Height;
                            mergedImageWidth = Math.Max( mergedImageWidth, jpgImageResized.Width );

                            imagesToMerge.Add( jpgImageResized );
                        }
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( string.Format( "C:\\output\\{0}_{1}_Orig.jpg {2}", convenantPdfId, i, ex ) );
                    }
                }
            }

            if ( imagesToMerge.Any() )
            {
                using ( Bitmap mergedImage = new Bitmap( mergedImageWidth, mergedImageHeight ) )
                {
                    using ( Graphics g = Graphics.FromImage( mergedImage ) )
                    {
                        int topPosition = 0;
                        foreach ( var image in imagesToMerge )
                        {
                            g.DrawImage( image, 0, topPosition );
                            topPosition += image.Height;
                        }
                    }

                    mergedImage.Save( mergedFileName,jpgEncoder, myEncoderParameters );
                }
            }

            foreach ( var image in imagesToMerge )
            {
                image.Dispose();
            }

            
            var updatedPercentComplete = Math.Round( ( processedCount / totalCount ) * 100, 2 );

            if ( percentComplete != updatedPercentComplete )
            {
                percentComplete = updatedPercentComplete;
                Debug.WriteLine( string.Format( "{0}%", percentComplete ) );
            }


        }

        private ImageCodecInfo GetEncoder( ImageFormat format )
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach ( ImageCodecInfo codec in codecs )
            {
                if ( codec.FormatID == format.Guid )
                {
                    return codec;
                }
            }
            return null;
        }

    }


    public class MyImageRenderListener : IRenderListener
    {
        public void RenderText( TextRenderInfo renderInfo ) { }
        public void BeginTextBlock() { }
        public void EndTextBlock() { }

        public List<byte[]> Images = new List<byte[]>();
        public void RenderImage( ImageRenderInfo renderInfo )
        {

            try
            {
                PdfImageObject image = renderInfo.GetImage();

                if ( image == null ) return;


                using ( MemoryStream ms = new MemoryStream( image.GetImageAsBytes() ) )
                {
                    Images.Add( ms.ToArray() );
                }
            }
            catch ( IOException )
            {
                /*
                 * pass-through; image type not supported by iText[Sharp]; e.g. jbig2
                */
            }
        }
    }
}
