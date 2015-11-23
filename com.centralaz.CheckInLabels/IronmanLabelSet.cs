using System;
using System.Drawing;
using System.Text;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace com.centralaz.CheckInLabels
{
    /// <summary>
    /// This class represents the set of ironman check-in labels.
    /// </summary>
    public class IronmanLabelSet
    {
        #region Constructors

        //Default constructor
        public IronmanLabelSet()
        {
        }

        #endregion

        #region Protected Members
        private const string possibleInvalidReason = "If you believe the printer is valid and your server is virtualized, this may be caused by incompatible printer drivers.";
        #endregion

        #region NameTag Properties


        protected DateTime _CheckInDate = DateTime.Now;
        public DateTime CheckInDate
        {
            get { return _CheckInDate; }
            set { _CheckInDate = value; }
        }

        protected string _FirstName = string.Empty;
        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; }
        }

        protected string _LastName = string.Empty;
        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; }
        }

        protected string _FullName = string.Empty;
        public string FullName
        {
            get { return _FullName; }
            set { _FullName = value; }
        }

        protected bool _FirstTime = false;
        public bool FirstTime
        {
            get { return _FirstTime; }
            set { _FirstTime = value; }
        }

        protected string _LogoImageFile = @"C:\Inetpub\wwwroot\CheckIn\images\xlogo_bw_lg.bmp";
        public string LogoImageFile
        {
            get { return _LogoImageFile; }
            set { _LogoImageFile = value; }
        }

        protected string _BirthdayImageFile = @"C:\Inetpub\wwwroot\CheckIn\images\cake.bmp";
        public string BirthdayImageFile
        {
            get { return _BirthdayImageFile; }
            set { _BirthdayImageFile = value; }
        }

        protected DateTime _BirthdayDate = DateTime.MinValue;
        public DateTime BirthdayDate
        {
            get { return _BirthdayDate; }
            set { _BirthdayDate = value; }
        }

        protected string _RoomName = string.Empty;
        public string RoomName
        {
            get { return _RoomName; }
            set { _RoomName = value; }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// This method will print the Attendance Label and Claim Card to the given printer.
        /// </summary>
        /// <param name="printerURL">the URI/URL of the printer.</param>
        /// <exception cref="Exception">is thrown if the given printer is invalid or
        /// if a problem occurs when printing.</exception>
        public void PrintLabel( string printerURL )
        {

            PrintDocument pDoc = new PrintDocument();

            // hook up the event handler for the PrintPage
            // method, which is where we will build our	document
            pDoc.PrintPage += new PrintPageEventHandler( pEvent_PrintLabel );

            pDoc.PrinterSettings.PrinterName = printerURL;

            // Now check to see if the printer is available
            // and call the Print method
            if ( pDoc.PrinterSettings.IsValid )
            {
                pDoc.Print();
            }
            else
            {
                throw new Exception( "The printer, " + printerURL + ", is not valid. " + possibleInvalidReason );
            }

        }

        /// <summary>
        /// A string representation of a PrinterLabel.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "AttendanceLabel -> " );

            sb.Append( "FirstName [" + this.FirstName + "] : " );
            sb.Append( "FullName [" + this.FullName + "] : " );

            return ( sb.ToString() );
        }

        public IronmanLabelSet ShallowCopy()
        {
            return (IronmanLabelSet)this.MemberwiseClone();
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// This is the event handler for printing all the labels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pEvent_PrintLabel( object sender, PrintPageEventArgs e )
        {

            int labelwidth = 384;  // 4 inches * 96dpi
            int labelheight = 220;//192;  // 2 inches * 96dpi

            //String format used to center text on label
            StringFormat format = new StringFormat();
            // Define the "brush" for printing
            SolidBrush br = new SolidBrush( Color.Black );
            Rectangle rectangle = new Rectangle();

            Bitmap bmp = new Bitmap( labelwidth, labelheight, PixelFormat.Format8bppIndexed );

            Graphics g = e.Graphics;

            // smothing mode on
            g.SmoothingMode = SmoothingMode.AntiAlias;

            /*******************************************************************/
            /*                           FirstName                             */
            /*******************************************************************/
            br.Color = Color.Black;

            //String format used to center text on label
            format.Alignment = StringAlignment.Near;

            //Set X Position to 0 (left) and Y position down a bit
            rectangle.X = 36;
            rectangle.Y = 256;

            // Set rectangle's width to width of label
            rectangle.Width = labelwidth;

            string firstName = this.FirstName;

            // Resize based on the length of the person's firstname
            int fontSize = 48; // size for names 4 chars in length or less
            if ( 5 < this.FirstName.Length && this.FirstName.Length <= 10 )
            {
                fontSize = 24;
            }
            else if ( 11 <= this.FirstName.Length )
            {
                fontSize = 20; // max size
                if ( firstName.Length >= 13 )
                    firstName = firstName.Substring( 0, 13 );
            }

            g.DrawString( firstName, new Font( "Gotham", fontSize, FontStyle.Bold ), br, rectangle, format );

            /*******************************************************************/
            /*                             Separator Line                      */
            /*******************************************************************/

            //Set color to black
            br.Color = Color.Black;
            g.FillRectangle( br, 35, 292, 928, 2 );

            /*******************************************************************/
            /*                             First Time Line                      */
            /*******************************************************************/
            if ( this.FirstTime )
            {
                g.FillRectangle( br, 39, 292, 928, 2 );
            }

            /*******************************************************************/
            /*                             Birthday Cake or Logo               */
            /*******************************************************************/
            // Try to process the images, but don't die if unable to find them
            try
            {
                System.Drawing.Image img;

                img = System.Drawing.Image.FromFile( this._LogoImageFile, true );


                // Define a rectangle to locate the graphic:
                // x,y ,width, height (where x,y is the coord of the upper left corner of the rectangle)
                RectangleF rect = new RectangleF( 608F, 608F, 56.0F, 56.0F );

                // Add the image to the document
                g.DrawImage( img, rect );
            }
            catch { }

        }

        #endregion
    }
}