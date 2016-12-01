using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ImageResizer;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Rock.Data;
using Rock.Model;

/// <summary>
/// 
/// </summary>
namespace CleanupBinaryFileData
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class Form1 : Form
    {
        const int binaryFileTypeCovenantId = 11;
        double totalCount = 0;
        double processedCount = 0;
        double percentComplete = 0;
        ResizeSettings settings = new ResizeSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

            //Parallel.ForEach( convenantPdfList, ( convenantPdf ) =>
            foreach ( var convenantPdf in convenantPdfList )
            {
                string mergedFileName = string.Format( "C:\\output\\merged_resized\\{0}_mergedImages.jpg", convenantPdf.Id );
                if ( !File.Exists( mergedFileName ) )
                {
                    try
                    {
                        ExtractMergedJPG( convenantPdf.Id, mergedFileName );
                    }
                    catch ( OutOfMemoryException )
                    {
                        GC.Collect();
                    }
                }

                processedCount++;
            }
            //);

            MessageBox.Show( "Done!" );
        }

        /// <summary>
        /// Extracts the merged JPG.
        /// </summary>
        /// <param name="convenantPdfId">The convenant PDF identifier.</param>
        /// <param name="mergedFileName">Name of the merged file.</param>
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

            //var origPdf = string.Format( "C:\\output\\orig_pdfs\\{0}.pdf", convenantPdfId );
            //File.WriteAllBytes( origPdf, content );
            var reader = new PdfReader( content );
            PdfReaderContentParser parser = new PdfReaderContentParser( reader );
            MyImageRenderListener listener = new MyImageRenderListener( convenantPdfId, reader );

            for ( int i = 1; i <= reader.NumberOfPages; i++ )
            {
                parser.ProcessContent( i, listener );
            }

            int mergedImageWidth = 0;
            int mergedImageHeight = 0;
            var imagesToMerge = new List<Bitmap>();
            bool successfullyExtracted = listener.SuccessfullyExtracted;

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
                        successfullyExtracted = false;
                        Debug.WriteLine( string.Format( "C:\\output\\{0}_{1}_Orig.jpg {2}", convenantPdfId, i, ex ) );
                    }
                }
            }

            if ( imagesToMerge.Any() && successfullyExtracted )
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

                    var rockContext = new RockContext();


                    using ( var imageStream = new MemoryStream() )
                    {
                        mergedImage.Save( imageStream, jpgEncoder, myEncoderParameters );

                        var binaryFile = new BinaryFileService( rockContext ).Get( convenantPdfId );
                        binaryFile.MimeType = jpgEncoder.MimeType;
                        binaryFile.FileName = System.IO.Path.ChangeExtension( binaryFile.FileName, ".jpg" );
                        binaryFile.ContentStream = imageStream;
                        rockContext.SaveChanges();
                    }
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

        /// <summary>
        /// Gets the encoder.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
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


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="iTextSharp.text.pdf.parser.IRenderListener" />
    public class MyImageRenderListener : IRenderListener
    {
        public MyImageRenderListener( int convenantPdfId, PdfReader reader )
        {
            _convenantPdfId = convenantPdfId;
            _reader = reader;
        }

        private int _convenantPdfId;
        private PdfReader _reader;
        public void RenderText( TextRenderInfo renderInfo ) { }
        public void BeginTextBlock() { }
        public void EndTextBlock() { }

        public List<byte[]> Images = new List<byte[]>();
        public bool SuccessfullyExtracted { get; set; }
        public void RenderImage( ImageRenderInfo renderInfo )
        {

            try
            {
                SuccessfullyExtracted = false;
                PdfImageObject image = renderInfo.GetImage();

                if ( image == null ) return;


                using ( MemoryStream ms = new MemoryStream( image.GetImageAsBytes() ) )
                {
                    Images.Add( ms.ToArray() );
                    SuccessfullyExtracted = true;
                }
            }
            catch ( IOException ex )
            {
                Debug.WriteLine( string.Format( "Unable to extract image for convenantPdfId = {0} {1}", _convenantPdfId, ex.Message ) );
            }
        }
    }
}
