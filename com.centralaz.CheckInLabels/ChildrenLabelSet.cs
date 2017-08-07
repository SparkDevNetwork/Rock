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
	/// This class represents the set of children's check-in labels.
	/// </summary>
	public class ChildrenLabelSet
	{
		#region Constructors

		//Default constructor
        public ChildrenLabelSet()
		{
		}

		#endregion

		#region Protected Members
		private int _pageNum = 0;
		private const string possibleInvalidReason = "If you believe the printer is valid and your server is virtualized, this may be caused by incompatible printer drivers.";
		#endregion

		#region ClaimCard and Attendance Label Properties

		protected string _ClaimCardSubTitle = string.Empty;

		protected string _HealthNotes = string.Empty;
		public string HealthNotes
		{
			get { return _HealthNotes; }
			set { _HealthNotes = value; }
		}

		protected string _HealthNotesTitle = string.Empty;
		public string HealthNotesTitle
		{
			get { return _HealthNotesTitle; }
			set { _HealthNotesTitle = value; }
		}

		protected DateTime _CheckInDate = DateTime.Now;
		public DateTime CheckInDate
		{
			get { return _CheckInDate; }
			set { _CheckInDate = value; }
		}

		protected string _Footer = string.Empty;
		public string Footer
		{
			get { return _Footer; }
			set { _Footer = value; }
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

		protected string _ParentsInitialsTitle = string.Empty;
		public string ParentsInitialsTitle
		{
			get { return _ParentsInitialsTitle; }
			set { _ParentsInitialsTitle = value; }
		}

		protected string _SecurityToken = string.Empty;
		public string SecurityToken
		{
			get { return _SecurityToken; }
			set { _SecurityToken = value; }
		}

		protected bool _SelfCheckOutFlag = false;
		public bool SelfCheckOutFlag
		{
			get { return _SelfCheckOutFlag; }
			set { _SelfCheckOutFlag = value; }
		}

		protected bool _EpiPenFlag = false;
		public bool EpiPenFlag
		{
			get { return _EpiPenFlag; }
			set { _EpiPenFlag = value; }
		}

        protected bool _SpecialNeedsIntakeFlag = false;
        public bool SpecialNeedsIntakeFlag
        {
            get { return _SpecialNeedsIntakeFlag; }
            set { _SpecialNeedsIntakeFlag = value; }
        }

        protected bool _HealthNoteFlag = false;
		public bool HealthNoteFlag
		{
			get { return _HealthNoteFlag; }
			set { _HealthNoteFlag = value; }
		}

		protected bool _LegalNoteFlag = false;
		public bool LegalNoteFlag
		{
			get { return _LegalNoteFlag; }
			set { _LegalNoteFlag = value; }
		}

		protected string _Services = string.Empty;
		public string Services
		{
			get { return _Services; }
			set { _Services = value; }
		}

		protected string _ServicesTitle = string.Empty;
		public string ServicesTitle
		{
			get { return _ServicesTitle; }
			set { _ServicesTitle = value; }
		}

		protected string _ClaimCardTitle = string.Empty;
		public string ClaimCardTitle
		{
			get { return _ClaimCardTitle; }
			set { _ClaimCardTitle = value; }
		}

		protected string _AttendanceLabelTitle = string.Empty;
		public string AttendanceLabelTitle
		{
			get { return _AttendanceLabelTitle; }
			set { _AttendanceLabelTitle = value; }
		}
		#endregion

		#region Nametag Properties

		protected string _AgeGroup = string.Empty;
		public string AgeGroup
		{
			get { return _AgeGroup; }
			set { _AgeGroup = value; }
		}

		protected string _LogoImageFile = @"C:\inetpub\wwwroot\RockWeb\Content\InternalSite\Check-in\xlogo_bw_lg.bmp";
		public string LogoImageFile
		{
			get { return _LogoImageFile; }
			set { _LogoImageFile = value; }
		}

		protected string _InfoIconFile = @"C:\inetpub\wwwroot\RockWeb\Content\InternalSite\Check-in\info.bmp";
		public string InfoIconFile
        {
			get { return _InfoIconFile; }
			set { _InfoIconFile = value; }
		}

        protected string _BirthdayImageFile = @"C:\inetpub\wwwroot\RockWeb\Content\InternalSite\Check-in\cake.bmp";
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

		#region Public Static Methods

		#endregion

		#region Protected Static Methods

		#endregion

		#region Public Instance Methods

		/// <summary>
		/// This method will print the Attendance Label and Claim Card to the given printer.
		/// </summary>
		/// <param name="printerURL">the URI/URL of the printer.</param>
		/// <exception cref="Exception">is thrown if the given printer is invalid or
		/// if a problem occurs when printing.</exception>
		public void PrintLabels( string printerURL, bool shouldPrintNametag )
		{
			if ( shouldPrintNametag )
			{
				PrintAllLabels( printerURL );
			}
			else
			{
				PrintAttendanceLabel( printerURL );
				PrintClaimCard( printerURL );
			}
		}

		/// <summary>
		/// This method will print the attendance label to the given printer.
		/// </summary>
		/// <param name="printerURL">the URI/URL of the printer.</param>
		/// <exception cref="Exception">is thrown if the given printer is invalid or
		/// if a problem occurs when printing.</exception>
		public void PrintAttendanceLabel( string printerURL )
		{
			PrintDocument pDoc = new PrintDocument();

			// hook up the event handler for the PrintPage
			// method, which is where we will build our	document
			pDoc.PrintPage += new PrintPageEventHandler( pEvent_PrintAttendanceLabelPage );

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
		/// This method will print the Claim Card to the given printer.
		/// </summary>
		/// <param name="printerURL">the URI/URL of the printer.</param>
		/// <exception cref="Exception">is thrown if the given printer is invalid or
		/// if a problem occurs when printing.</exception>
		public void PrintClaimCard( string printerURL )
		{
			PrintDocument pDoc = new PrintDocument();

			// hook up the event handler for the PrintPage
			// method, which is where we will build our	document
			pDoc.PrintPage += new PrintPageEventHandler( pEvent_PrintClaimCardPage );

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
		/// This method will print the Nametag to the given printer.
		/// </summary>
		/// <param name="printerURL">the URI/URL of the printer.</param>
		/// <exception cref="Exception">is thrown if the given printer is invalid or
		/// if a problem occurs when printing.</exception>
		public void PrintNametag( string printerURL )
		{
			PrintDocument pDoc = new PrintDocument();

			// hook up the event handler for the PrintPage
			// method, which is where we will build our	document
			pDoc.PrintPage += new PrintPageEventHandler( pEvent_PrintNameTag );

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
		/// This method will print all the labels to the given printer.
		/// </summary>
		/// <param name="printerURL">the URI/URL of the printer.</param>
		/// <exception cref="Exception">is thrown if the given printer is invalid or
		/// if a problem occurs when printing.</exception>
		public void PrintAllLabels( string printerURL )
		{
			PrintDocument pDoc = new PrintDocument();

			// hook up the event handler for the PrintPage
			// method, which is where we will build our	document
			pDoc.PrintPage += new PrintPageEventHandler( pEvent_PrintAllLabels );

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

			sb.Append( "SecurityToken [" + this.SecurityToken + "] : " );
			sb.Append( "ParentsInitialsTitle [" + this.ParentsInitialsTitle + "] : " );
			sb.Append( "HealthNotes [" + this.HealthNotes + "] : " );
			sb.Append( "EpiPen [" + this.EpiPenFlag + "] : " );
			sb.Append( "SelfCheckOut [" + this.SelfCheckOutFlag + "] : " );
			sb.Append( "ServicesTitle [" + this.ServicesTitle + "] : " );
			sb.Append( "CheckInDate [" + this.CheckInDate + "] : " );
			sb.Append( "HealthNotesTitle [" + this.HealthNotesTitle + "] : " );
			sb.Append( "Services [" + this.Services + "] : " );
			sb.Append( "FirstName [" + this.FirstName + "] : " );
			sb.Append( "FullName [" + this.FullName + "] : " );
			sb.Append( "Footer [" + this.Footer + "] : " );

			return ( sb.ToString() );
		}

		public ChildrenLabelSet ShallowCopy()
		{
            return (ChildrenLabelSet)this.MemberwiseClone();
		}

		#endregion

		#region Protected Instance Methods

		/// <summary>
		/// This is the event handler for printing all the labels.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pEvent_PrintAllLabels( object sender, PrintPageEventArgs e )
		{
			this._pageNum++;

			switch ( _pageNum )
			{
				case 1:
					pEvent_PrintAttendanceLabelPage( sender, e );
					e.HasMorePages = true;
					break;

				case 2:
					pEvent_PrintClaimCardPage( sender, e );
					e.HasMorePages = true;
					break;

				case 3:
					pEvent_PrintNameTag( sender, e );
					e.HasMorePages = false;
					break;
			}
		}

		/// <summary>
		/// This is the event handler for printing only a Claim Card page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pEvent_PrintClaimCardPage( object sender, PrintPageEventArgs e )
		{
			// Create a Graphics object and add it to the
			// document

			int labelwidth = 195; //2.25 inches
			int labelheight = 144; //2 inches
			int xpos = 0; //temp x position placeholder
			int ypos = 0;  //temp y position placeholder

			Bitmap bmp = new Bitmap( labelwidth, labelheight, PixelFormat.Format24bppRgb );

			Graphics g = e.Graphics;

			// Define the "brush" for printing
			SolidBrush br = new SolidBrush( Color.Black );

			//String format used to center text on label
			StringFormat format = new StringFormat();

			format.Alignment = StringAlignment.Center;

			//Alignment / placement rectangle used to place all	text 
			Rectangle Rf = new Rectangle();
			//Set rectangle X Position to 0 (left)
			Rf.X = 0;
			//Set y position to 5 - drop down 5 pixels from the top
			Rf.Y = 5;
			//Set width of the position rectangle to the width variable
			Rf.Width = labelwidth;

			/*******************************************************************/
			/*                   Claim Card Title                              */
			/*******************************************************************/
			//Brush color to black 
			//br.Color=Color.Black;
			//g.DrawString( "", new Font("Arial Black",9,FontStyle.Italic ), br,Rf,format );

			//draw black rectangle
			int barwidth = 140; //black bar at the top			
			int barheight = 20;  //height of bar at top

			xpos = ( labelwidth - barwidth ) / 2;//center x position of rectangle
			ypos = 22;  //set y pos of rectangle
			g.FillRectangle( br, xpos, ypos, barwidth, barheight );

			//draw white text over rectangle
			Rf.X = 0;
			Rf.Y = ypos;
			//Set width of the position rectangle to the width variable
			Rf.Width = labelwidth;

			//set the alignment of the text 
			format.Alignment = StringAlignment.Center;
			//Set text color to white
			br.Color = Color.White;
			g.DrawString( ClaimCardTitle, new Font( "Arial Black", 11 ), br, Rf, format );

			/*******************************************************************/
			/* Claim Card       DATE                 TIME                      */
			/*******************************************************************/
			ypos += 25; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			g.DrawString( this.CheckInDate.ToShortDateString() + "  " + this.CheckInDate.ToShortTimeString(), new Font( "Arial", 8 ), br, Rf, format );

			/*******************************************************************/
			/*Claim Card        Service(s): Time                 Time          */
			/*******************************************************************/
			ypos += 20; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			g.DrawString( this.ServicesTitle + " " + this.Services, new Font( "Arial", 8 ), br, Rf, format );

			/*******************************************************************/
			/*Claim Card                      FirstName                        */
			/*******************************************************************/
			ypos += 15; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			g.DrawString( this.FirstName, new Font( "Arial", 15, FontStyle.Bold ), br, Rf, format );

			/*******************************************************************/
			/*Claim Card   Rotated Text (Security Code)                         */
			/*******************************************************************/
			int rotTextYpos = ypos + 43;  //set y position of rotated text
			int rotTextXpos = 35;  //set x postion of rotated text

			//not really sure what this does
			g.TranslateTransform( rotTextXpos, rotTextYpos );
			//rotate the text -90 degrees
			g.RotateTransform( -90 );
			//fomrat the text
			format.FormatFlags = StringFormatFlags.NoClip;
			format.LineAlignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			//write the text
			g.DrawString( this.SecurityToken.Substring( 0, 2 ), new Font( "Arial", 12 ), br, 0, 0, format );
			//Reset the transform (thing i know nothing about)
			g.ResetTransform();

			ypos -= 10; //adjust ypos height of the rotated text 

			/*******************************************************************/
			/*Claim Card         [Security Token ]                             */
			/*******************************************************************/
			//draw black rectangle

			int secbarheight = 30; //Black Security Bar Height
			int secbarwidth = 100; //Black Security Bar width

			xpos = ( labelwidth - secbarwidth ) / 2;//center x position of rectangle
			ypos = ypos + secbarheight + 10;  //set y pos of rectangle
			g.FillRectangle( br, xpos, ypos, secbarwidth, secbarheight );

			//draw white text over rectangle
			Rf.X = 0;
			Rf.Y = ypos + ( secbarheight / 2 );
			//Set width of the position rectangle to the width variable
			Rf.Width = labelwidth;

			//set the alignment of the text 
			format.Alignment = StringAlignment.Center;
			//Set text color to white
			br.Color = Color.White;
			g.DrawString( this.SecurityToken.Substring( 2 ), new Font( "Arial Black", 18 ), br, Rf, format );

			/*******************************************************************/
			/*Claim Card                     Footer                            */
			/*******************************************************************/
			ypos += 65; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			g.DrawString( this.Footer, new Font( "Arial", 7 ), br, Rf, format );

		}

		/// <summary>
		/// This is the event handler for printing only the Attendance Label page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pEvent_PrintAttendanceLabelPage( object sender, PrintPageEventArgs e )
		{
			// Create a Graphics object and add it to the
			// document

			int labelwidth = 195; //2.25 inches
			int labelheight = 144; //2 inches
			int xpos = 0; //temp x position placeholder
			int ypos = 0;  //temp y position placeholder

			Bitmap bmp = new Bitmap( labelwidth, labelheight, PixelFormat.Format24bppRgb );

			Graphics g = e.Graphics;

			// Define the "brush" for printing
			SolidBrush br = new SolidBrush( Color.Black );

			//String format used to center text on label
			StringFormat format = new StringFormat();

			format.Alignment = StringAlignment.Center;

			//Alignment / placement rectangle used to place all	text 
			Rectangle Rf = new Rectangle();
			//Set rectangle X Position to 0 (left)
			Rf.X = 0;
			//Set y position to 5 - drop down 5 pixels from the top
			Rf.Y = 7;
			//Set width of the position rectangle to the width variable
			Rf.Width = labelwidth;

			/*******************************************************************/
			/*                   Attendance Card Title                         */
			/*******************************************************************/
			//Brush color to black 
			br.Color = Color.Black;
			//g.DrawString(this.AttendanceLabelTitle , new Font("Arial Black",9,FontStyle.Italic ), br,Rf,format );
			// Use the AgeGroup as the title (1/5/2009 per Kevin & Laurie)

			g.DrawString( this.AgeGroup, new Font( "Arial Black", 9, FontStyle.Regular ), br, Rf, format );

			// Draw a line underneath the age group
			g.DrawRectangle( new Pen( Color.Black ), Rf.X, Rf.Y + 15, 200.0F, 0.1F );

			/*******************************************************************/
			/*Attendance        DATE                 TIME                      */
			/*******************************************************************/
			ypos += 25; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			g.DrawString( this.CheckInDate.ToShortDateString() + "  " + this.CheckInDate.ToShortTimeString(), new Font( "Arial", 8 ), br, Rf, format );

			/*******************************************************************/
			/*Attendance        Service(s): Time                 Time          */
			/*******************************************************************/
			ypos += 20; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;

			// Change the font based on whether or not there are multiple service times
			Font serviceFont = new Font( "Arial", 8 );
			if ( this.Services.IndexOf( ",", 0 ) > 0 || this.Services.IndexOf( "&", 0 ) > 0 )
			{
				Pen blackPen = new Pen( Color.Black );
				g.DrawRectangle( blackPen, Rf.X, Rf.Y, 200.0F, 13.0F );

				this.ServicesTitle = "Transfer:";
				if ( Services.Length < 20 )
				{
					serviceFont = new Font( "Arial Black", 7, FontStyle.Italic );
				}
				else
				{
					serviceFont = new Font( "Arial", 7, FontStyle.Italic );
				}
			}

			g.DrawString( this.ServicesTitle + "  " + this.Services, serviceFont, br, Rf, format );

			/*******************************************************************/
			/*Attendance                     FullName                         */
			/*******************************************************************/
			ypos += 15; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos;

			//set the alignment
			format.Alignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			// BUG Fix: use smaller font if full name longer than 19 characters.
			int fontSize = ( this.FullName.Length > 19 ) ? 9 : 12;

			g.DrawString( this.FullName, new Font( "Arial", fontSize, FontStyle.Bold ), br, Rf, format );

			/*******************************************************************/
			/*Attendance  Rotated Text (Security Code)                         */
			/*******************************************************************/
			int rotTextYpos = ypos + 35;  //set y position of rotated text
			int rotTextXpos = 35;  //set x postion of rotated text

			//not really sure what this does
			g.TranslateTransform( rotTextXpos, rotTextYpos );
			//rotate the text -90 degrees
			g.RotateTransform( -90 );
			//fomrat the text
			format.FormatFlags = StringFormatFlags.NoClip;
			format.LineAlignment = StringAlignment.Center;
			//Set text color back to black
			br.Color = Color.Black;
			//write the text
			g.DrawString( this.SecurityToken.Substring( 0, 2 ), new Font( "Arial", 12 ), br, 0, 0, format );
			//Reset the transform (thing i know nothing about)
			g.ResetTransform();

			ypos -= 10; //adjust ypos height of the rotated text 

			/*******************************************************************/
			/*Attendance         [Security Token ]                             */
			/*******************************************************************/
			//draw black rectangle

			int secbarheight = 30; //Black Security Bar Height
			int secbarwidth = 100; //Black Security Bar width

			xpos = ( labelwidth - secbarwidth ) / 2;//center x position of rectangle
			ypos = ypos + secbarheight;  //set y pos of rectangle
			g.FillRectangle( br, xpos, ypos, secbarwidth, secbarheight );

			//draw white text over rectangle
			Rf.X = 0;
			Rf.Y = ypos + ( secbarheight / 2 );
			//Set width of the position rectangle to the width variable
			Rf.Width = labelwidth;

			//set the alignment of the text 
			format.Alignment = StringAlignment.Center;
			//Set text color to white
			br.Color = Color.White;
			g.DrawString( this.SecurityToken.Substring( 2 ), new Font( "Arial Black", 18 ), br, Rf, format );

			/* Required Y adjustment to add flags */
			if ( this.HealthNoteFlag || this.LegalNoteFlag || this.SelfCheckOutFlag || this.EpiPenFlag )
			{
				ypos += 10;
				xpos += 22;
			}

			/*******************************************************************/
			/*Attendance              Self Check Out flag                      */
			/*******************************************************************/
			if ( this.SelfCheckOutFlag )
			{
				Rf.X = xpos;
				Rf.Y = ypos;
				//set the alignment
				format.Alignment = StringAlignment.Center;
				//Set text color back to black
				br.Color = Color.Black;
				g.DrawString( LabelSymbols.SelfCheckOutFlag, new Font( "Arial", 20, FontStyle.Bold ), br, Rf, format );
			}

			/*******************************************************************/
			/*Attendance               Legal note flag                         */
			/*******************************************************************/
			if ( this.LegalNoteFlag )
			{
				xpos += 12;
				//ypos+=10;

				Rf.X = xpos;
				Rf.Y = ypos;
				//set the alignment
				format.Alignment = StringAlignment.Center;
				//Set text color back to black
				br.Color = Color.Black;
				g.DrawString( LabelSymbols.LegalNote, new Font( "Arial", 20, FontStyle.Bold ), br, Rf, format );
			}

			/*******************************************************************/
			/*Attendance                 EpiPen flag                           */
			/*******************************************************************/
			if ( this.EpiPenFlag )
			{
				xpos += 12;
				//ypos+=10;

				Rf.X = xpos;
				Rf.Y = ypos;
				//set the alignment
				format.Alignment = StringAlignment.Center;
				//Set text color back to black
				br.Color = Color.Black;
                g.DrawString( LabelSymbols.EpiPenFlag, new Font( "Arial", 20, FontStyle.Bold ), br, Rf, format );
            }

			/*******************************************************************/
			/*Attendance              [HealthNotes ]                             */
			/*******************************************************************/
			//draw black rectangle

			int allergybarheight = 30; //Black Security Bar Height (20 ~ 2mm; 30 ~ 4mm)
			if ( this.HealthNoteFlag )
			{
				// Truncate HealthNotes if greater than 113 characters.
				string truncatedHealthNotes = this.HealthNotes;
				if ( truncatedHealthNotes.Length > 113 )
				{
					truncatedHealthNotes = truncatedHealthNotes.Substring( 0, 113 ) + "...";
				}

				int squishyness = 3; // amount to squish the black box text in 
				// from the side of the label edge
				int allergybarwidth = labelwidth - squishyness; //Black Security Bar width

				xpos = ( labelwidth - allergybarwidth ) / 2 + 10;//center x position of rectangle
				ypos = ypos + 30;  //set y pos of rectangle
				//Set color to black
				br.Color = Color.Black;
				g.FillRectangle( br, xpos-2, ypos-2, allergybarwidth+4, allergybarheight+4 );  // black
				br.Color = Color.White;
				g.FillRectangle( br, xpos-1, ypos-1, allergybarwidth+2, allergybarheight+2 );  // white
				br.Color = Color.Black;
				g.FillRectangle( br, xpos, ypos, allergybarwidth, allergybarheight );  // black

				//draw white text over rectangle
				Rf.X = (int)( squishyness / 2 ) + 10;
				Rf.Y = ypos + 15;
				//Set width of the position rectangle to the width variable
				Rf.Width = labelwidth - squishyness;

				//set the alignment of the text 
				format.Alignment = StringAlignment.Near;
				//Set text color to white
				br.Color = Color.White;
				g.DrawString( this.HealthNotesTitle + " " + truncatedHealthNotes, new Font( "Arial", 6, FontStyle.Bold ), br, Rf, format );
			}
			else
			{
				ypos = ypos + 30;  //set y pos of rectangle
			}

			/*******************************************************************/
			/*Attendance           Parents Initials Title                      */
			/*******************************************************************/
			ypos += allergybarheight + 25; //advance the y position to do more drawing

			Rf.X = 0;
			//set new y position (move down for next text)
			Rf.Y = ypos + 5;

			//set the alignment
			format.Alignment = StringAlignment.Near;
			//Set text color back to black
			br.Color = Color.Black;
			Rf.Width = labelwidth + 30; // let the signature go all the way to the edge
			g.DrawString( this.ParentsInitialsTitle, new Font( "Arial", 7 ), br, Rf, format );
		}

		/// <summary>
		/// This is the event handler for printing only the Nametag page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pEvent_PrintNameTag( object sender, PrintPageEventArgs e )
		{
			int labelwidth = 216;  // 2.25 inches * 96dpi
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
			/*                              Age Group                          */
			/*******************************************************************/

			//Set color to black
			br.Color = Color.Black;
			g.FillRectangle( br, 0, 0, labelwidth, 20 );

			//draw white text over rectangle, starting at top and about 15 down
			rectangle.X = 0;
			rectangle.Y = 2;
			//Set width of the position rectangle to the width variable
			rectangle.Width = labelwidth;

			//set the alignment of the text 
			format.Alignment = StringAlignment.Far;

			//Set text color to white
			br.Color = Color.White;
			g.DrawString( this.AgeGroup, new Font( "Arial", 9, FontStyle.Bold ), br, rectangle, format );

            /*******************************************************************/
            /*                              RoomName                           */
            /*******************************************************************/

		    //bool printRoomName;
            //
            //if (bool.TryParse(BlahBlahBlah.SomeSortOf.Settings["Cccev.DisplayRoomNameOnNameTag"], out printRoomName))
            //{
            //    if (printRoomName)
            //    {
            //        format.Alignment = StringAlignment.Near;
            //        g.DrawString(RoomName.Length > 20 ? RoomName.Substring(0, 20) : RoomName, 
            //            new Font("Arial", 9, FontStyle.Bold), br, rectangle, format);
            //    }
            //}

		    /*******************************************************************/
			/*                           FirstName                             */
			/*******************************************************************/
			br.Color = Color.Black;

			//String format used to center text on label
			format.Alignment = StringAlignment.Near;

			//Set X Position to 0 (left) and Y position down a bit
			rectangle.X = -5;
			rectangle.Y = 20;

			// Set rectangle's width to width of label
			rectangle.Width = labelwidth;

			string firstName = this.FirstName;

			// Resize based on the length of the person's firstname
			int fontSize = 35; // size for names 4 chars in length or less
			if ( 5 < this.FirstName.Length && this.FirstName.Length <= 7 )
			{
				rectangle.X = -3;
				rectangle.Y = 30;
				fontSize = 30;
			}
			else if ( 8 <= this.FirstName.Length && this.FirstName.Length <= 10 )
			{
				rectangle.X = 0;
				rectangle.Y = 35;
				fontSize = 25; //
			}
			else if ( 11 <= this.FirstName.Length )
			{
				rectangle.X = 2;
				rectangle.Y = 40;
				fontSize = 20; // max size
				if ( firstName.Length >= 13 )
					firstName = firstName.Substring( 0, 13 );
			}

			g.DrawString( firstName, new Font( "Arial", fontSize, FontStyle.Bold ), br, rectangle, format );

			/*******************************************************************/
			/*                           Lastname                              */
			/*******************************************************************/
			br.Color = Color.Black;

			//String format used to center text on label
			format.Alignment = StringAlignment.Near;

			//Set X Position to 0 (left) and Y position down a bit
			rectangle.X = 5;  // from left
			rectangle.Y = 70; // from top

			// Set rectangle's width to width of label
			rectangle.Width = labelwidth;

			// Resize based on the length of the person's firstname
			fontSize = 15;
			if ( 16 <= this.LastName.Length )
			{
				fontSize = 10;
			}

			// 7/9/2007 Per Julie B and Steve H, don't print lastnames.
			//g.DrawString(this.LastName, new Font("Arial", fontSize, FontStyle.Bold), br, rectangle, format);

			/*******************************************************************/
			/* Health note, Self Check Out flag, Legal Note flag, Epi Pen flag */
			/*******************************************************************/
			rectangle.X = 82; // from left
			rectangle.Y = 65; // from top

			format.Alignment = StringAlignment.Center;
			string flags = "";

			if ( this.SelfCheckOutFlag )
			{
				flags = LabelSymbols.SelfCheckOutFlag;
			}

			if ( this.LegalNoteFlag )
			{
				flags = flags + LabelSymbols.LegalNote;
			}

			if ( this.HealthNoteFlag )
			{
				flags = flags + LabelSymbols.HealthNote;
			}

			if ( this.EpiPenFlag )
			{
				flags = flags + LabelSymbols.EpiPenFlag;
			}

            g.DrawString( flags, new Font( "Arial", 20, FontStyle.Bold ), br, rectangle, format );

            if ( this.SpecialNeedsIntakeFlag )
            {
                var img = System.Drawing.Image.FromFile( this._InfoIconFile, true );

                // Define a rectangle to locate the graphic:
                // x,y ,width, height (where x,y is the coord of the upper left corner of the rectangle)
                RectangleF rect = new RectangleF( rectangle.X + 64, rectangle.Y + 10, 16.0F, 16.0F );

                // Add the image to the document
                g.DrawImage( img, rect );
            }

            /*******************************************************************/
            /*                             Separator Line                      */
            /*******************************************************************/

            //Set color to black
            br.Color = Color.Black;
			g.FillRectangle( br, 0, 95, labelwidth, 1 );

			/*******************************************************************/
			/*                             Birthday Cake or Logo               */
			/*******************************************************************/
			// Try to process the images, but don't die if unable to find them
			try
			{
				System.Drawing.Image img;
				// Load a graphic from a file...
				// based on whether it is the person's birthday this week.
				// BUG FIX: #466 http://redmine.refreshcache.com/issues/466
				var nextBirthday = this.BirthdayDate.AddYears( DateTime.Today.Year - this.BirthdayDate.Year );
				if ( nextBirthday < DateTime.Today )
				{
					nextBirthday = nextBirthday.AddYears( 1 );
				}
				var numDays = ( nextBirthday - DateTime.Today ).Days;
				if ( this.BirthdayDate != DateTime.MinValue && numDays <= 7 )
				{
					img = System.Drawing.Image.FromFile( this._BirthdayImageFile, true );

					// determine which day of the week the birthday falls on this year:
					string dowBirthdayThisYear = new DateTime( DateTime.Now.Year, this.BirthdayDate.Month, this.BirthdayDate.Day ).DayOfWeek.ToString();
					if ( numDays == 0 )
					{
						dowBirthdayThisYear = "Today!";
					}
					// write the DayOfWeek that the birthday occurs under the image
					br.Color = Color.Black;
					format.Alignment = StringAlignment.Center;
					RectangleF dayOfWeekRect = new RectangleF( 130.0F, 173.0F, 56.0F, 13.0F );
					g.DrawString( dowBirthdayThisYear, new Font( "Arial", 7 ), br, dayOfWeekRect, format );
				}
				else
				{
					img = System.Drawing.Image.FromFile( this._LogoImageFile, true );
				}

				// Define a rectangle to locate the graphic:
				// x,y ,width, height (where x,y is the coord of the upper left corner of the rectangle)
				RectangleF rect = new RectangleF( 130.0F, 115.0F, 56.0F, 56.0F );

				// Add the image to the document
				g.DrawImage( img, rect );
			}
			catch { }

		}

		#endregion
	}

	public static class LabelSymbols
	{
		public static string EpiPenFlag = "e";
		public static string HealthNote = "+";
		public static string LegalNote = "!";
		public static string SelfCheckOutFlag = "*";
    }
}