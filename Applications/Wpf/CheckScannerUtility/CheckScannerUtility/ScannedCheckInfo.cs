//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Drawing;
using System.IO;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    public class ScannedCheckInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScannedCheckInfo"/> class.
        /// </summary>
        public ScannedCheckInfo()
        {
            Uploaded = false;
        }
        
        /// <summary>
        /// Gets or sets the front image.
        /// </summary>
        /// <value>
        /// The front image.
        /// </value>
        public byte[] FrontImageData { get; set; }

        public byte[] FrontImagePngBytes {
            get
            {
                Bitmap bmp = new Bitmap( new MemoryStream( this.FrontImageData ) );
                MemoryStream pngStream = new MemoryStream();
                bmp.Save( pngStream, System.Drawing.Imaging.ImageFormat.Png );
                return pngStream.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the back image.
        /// </summary>
        /// <value>
        /// The back image.
        /// </value>
        public byte[] BackImageData { get; set; }

        /// <summary>
        /// Gets the back image PNG bytes.
        /// </summary>
        /// <value>
        /// The back image PNG bytes.
        /// </value>
        public byte[] BackImagePngBytes
        {
            get
            {
                Bitmap bmp = new Bitmap( new MemoryStream( this.BackImageData ) );
                MemoryStream pngStream = new MemoryStream();
                bmp.Save( pngStream, System.Drawing.Imaging.ImageFormat.Png );
                return pngStream.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the routing number.
        /// </summary>
        /// <value>
        /// The routing number.
        /// </value>
        public string RoutingNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the check number.
        /// </summary>
        /// <value>
        /// The check number.
        /// </value>
        public string CheckNumber { get; set; }

        /// <summary>
        /// Gets the masked account number.
        /// </summary>
        /// <value>
        /// The masked account number.
        /// </value>
        public string MaskedAccountNumber
        {
            get
            {
                int length = AccountNumber.Length;
                string result = new string( 'x', length - 4 ) + AccountNumber.Substring( length - 4 );
                return result;
            }
        }

        public bool Uploaded { get; set; }
    }
}
