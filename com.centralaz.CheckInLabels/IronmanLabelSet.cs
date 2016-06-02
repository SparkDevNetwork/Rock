// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Drawing;
using System.Text;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace com.centralaz.CheckInLabels
{
    /// <summary>
    /// This class represents the ironman check-in nametag.
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

        protected string _LogoImageFile = @"C:\temp\ironman_w_text_304x74@203.bmp";
        public string LogoImageFile
        {
            get { return _LogoImageFile; }
            set { _LogoImageFile = value; }
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
            pDoc.DefaultPageSettings.Landscape = true;

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
            int labelheight = 400;  // 4 inches * 96dpi
            int labelwidth = 220;   // 2 inches * 96dpi

            //String format used to center text on label
            StringFormat format = new StringFormat();
            format.FormatFlags = StringFormatFlags.NoClip;
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
            rectangle.X = 10;
            rectangle.Y = 36;

            // Set rectangle's width to width of label
            rectangle.Width = labelheight;

            string firstName = this.FirstName;

            // Resize based on the length of the person's firstname
            int fontSize = 48; // size for names 4 chars in length or less
            if ( 5 < this.FirstName.Length && this.FirstName.Length <= 10 )
            {
                fontSize = 42;
            }
            else if ( 11 <= this.FirstName.Length )
            {
                fontSize = 36; // max size
                if ( firstName.Length >= 13 )
                {
                    firstName = firstName.Substring( 0, 13 );
                }
            }

            g.DrawString( firstName, new Font( "Arial Black", fontSize, FontStyle.Regular ), br, rectangle, format );

            /**************************************************************************
             * This creates a sort of alignment template so you can visually see where
             * everything goes on a label
             **************************************************************************/
            /*
            fontSize = 6;
            rectangle.X = 0;
            rectangle.Y = 0;
            g.DrawString( "0,0", new Font( "Arial", fontSize, FontStyle.Regular ), br, rectangle, format );
			
            rectangle.X = 50;
            rectangle.Y = 0;
            g.DrawString( "50,0", new Font( "Arial", fontSize, FontStyle.Regular ), br, rectangle, format );
			
            rectangle.X = 380;
            rectangle.Y = 0;
            g.DrawString( "380,0", new Font( "Arial", fontSize, FontStyle.Regular ), br, rectangle, format );
			
            rectangle.X = 0;
            rectangle.Y = 50;
            g.DrawString( "0,50", new Font( "Arial", fontSize, FontStyle.Regular ), br, rectangle, format );
			
            rectangle.X = 0;
            rectangle.Y = 180;
            g.DrawString( "0,180", new Font( "Arial", fontSize, FontStyle.Regular ), br, rectangle, format );
			
            Pen pen = new Pen( Color.Black, 4F );
            g.DrawRectangle( pen, 0, 0, 400, 200 );
            */

            /*******************************************************************/
            /*                             Separator Line                      */
            /*******************************************************************/
            br.Color = Color.Black;
            g.FillRectangle( br, 30, 140, 340, 2 );

            /*******************************************************************/
            /*                             First Time Line                     */
            /*******************************************************************/
            if ( this.FirstTime )
            {
                g.FillRectangle( br, 30, 150, 170, 2 );
            }

            /*******************************************************************/
            /*                             Logo                                */
            /*******************************************************************/
            // Try to process the images, but don't die if unable to find them
            try
            {
                System.Drawing.Image img;
                img = System.Drawing.Image.FromFile( this._LogoImageFile, true );
                // Add the image to the document
                g.DrawImageUnscaled( img, 220, 150 ); // 220 over, 150 down
            }
            catch { }
        }

        #endregion
    }
}