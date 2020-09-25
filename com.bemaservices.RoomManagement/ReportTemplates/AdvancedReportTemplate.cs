// <copyright>
// Copyright by BEMA Software Services
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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Web;
using com.bemaservices.RoomManagement.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using Document = iTextSharp.text.Document;

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// 
    /// </summary>
    [System.ComponentModel.Description( "The advanced report template" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Advanced" )]
    public class AdvancedReportTemplate : ReportTemplate
    {

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public override List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="reservationSummaryList"></param>
        /// <param name="logoFileUrl"></param>
        /// <param name="font"></param>
        /// <param name="filterStartDate"></param>
        /// <param name="filterEndDate"></param>
        /// <param name="lavaTemplate"></param>
        /// <returns></returns>
        public override byte[] GenerateReport( List<ReservationService.ReservationSummary> reservationSummaryList, string logoFileUrl, string font, DateTime? filterStartDate, DateTime? filterEndDate, string lavaTemplate = "" )
        {
            //Fonts
            var titleFont = FontFactory.GetFont( font, 16, Font.BOLD );
            var listHeaderFont = FontFactory.GetFont( font, 12, Font.BOLD, Color.DARK_GRAY );
            var listSubHeaderFont = FontFactory.GetFont( font, 10, Font.BOLD, Color.DARK_GRAY );
            var listItemFontNormal = FontFactory.GetFont( font, 8, Font.NORMAL );
            var listItemFontUnapproved = FontFactory.GetFont( font, 8, Font.ITALIC, Color.MAGENTA );
            var noteFont = FontFactory.GetFont( font, 8, Font.NORMAL, Color.GRAY );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                ApprovalState = r.ApprovalState.ConvertToString(),
                Locations = r.ReservationLocations.ToList(),
                Resources = r.ReservationResources.ToList(),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventDateTimeDescription,
                ReservationDateTimeDescription = r.ReservationDateTimeDescription,
                Ministry = r.ReservationMinistry,
                ContactInfo = String.Format( "{0} {1}", r.EventContactPersonAlias?.Person.FullName, r.EventContactPhoneNumber ),
                SetupPhotoId = r.SetupPhotoId,
                Note = r.Note
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();


            //Setup the document
            var document = new Document( PageSize.A4.Rotate(), 25, 25, 25, 25 );

            var outputStream = new MemoryStream();
            var writer = PdfWriter.GetInstance( document, outputStream );

            // Our custom Header and Footer is done using Event Handler
            TwoColumnHeaderFooter PageEventHandler = new TwoColumnHeaderFooter();
            writer.PageEvent = PageEventHandler;

            // Define the page header
            PageEventHandler.HeaderFont = listHeaderFont;
            PageEventHandler.SubHeaderFont = listSubHeaderFont;
            PageEventHandler.HeaderLeft = "Group";
            PageEventHandler.HeaderRight = "1";
            document.Open();

            // Add logo
            try
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance( logoFileUrl );

                logo.Alignment = iTextSharp.text.Image.RIGHT_ALIGN;
                logo.ScaleToFit( 100, 55 );
                document.Add( logo );
            }
            catch { }

            // Write the document
            var today = RockDateTime.Today;
            var filterStartDateTime = filterStartDate.HasValue ? filterStartDate.Value : today;
            var filterEndDateTime = filterEndDate.HasValue ? filterEndDate.Value : today.AddMonths( 1 );
            String title = String.Format( "Reservations for: {0} - {1}", filterStartDateTime.ToString( "MMMM d" ), filterEndDateTime.ToString( "MMMM d" ) );
            document.Add( new Paragraph( title, titleFont ) );

            Font zapfdingbats = new Font( Font.ZAPFDINGBATS );

            // Populate the Lists            
            foreach ( var reservationDay in reservationSummaries )
            {
                var firstReservation = reservationDay.FirstOrDefault();
                if ( firstReservation != null )
                {
                    //Build Header
                    document.Add( Chunk.NEWLINE );
                    String listHeader = PageEventHandler.CalendarDate = firstReservation.CalendarDate;
                    document.Add( new Paragraph( listHeader, listHeaderFont ) );

                    //Build Subheaders
                    var listSubHeaderTable = new PdfPTable( 7 );
                    listSubHeaderTable.LockedWidth = true;
                    listSubHeaderTable.TotalWidth = PageSize.A4.Rotate().Width - document.LeftMargin - document.RightMargin;
                    listSubHeaderTable.HorizontalAlignment = 0;
                    listSubHeaderTable.SpacingBefore = 10;
                    listSubHeaderTable.SpacingAfter = 0;
                    listSubHeaderTable.DefaultCell.BorderWidth = 0;
                    listSubHeaderTable.DefaultCell.BorderWidthBottom = 1;
                    listSubHeaderTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;

                    listSubHeaderTable.AddCell( new Phrase( "Event Start/End", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Event", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Location", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Layout", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Description", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Photo", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Ministry / Contact Info", listSubHeaderFont ) );
                    PageEventHandler.IsHeaderShown = true;
                    document.Add( listSubHeaderTable );

                    foreach ( var reservationSummary in reservationDay )
                    {
                        foreach ( var reservationLocation in reservationSummary.Locations )
                        {
                            if ( reservationSummary == reservationDay.Last() &&
                                reservationLocation == reservationSummary.Locations.Last() )
                            {
                                PageEventHandler.IsHeaderShown = false;
                            }

                            //Build the list item table
                            var listItemTable = new PdfPTable( 7 );
                            listItemTable.LockedWidth = true;
                            listItemTable.TotalWidth = PageSize.A4.Rotate().Width - document.LeftMargin - document.RightMargin;
                            listItemTable.HorizontalAlignment = 0;
                            listItemTable.SpacingBefore = 0;
                            listItemTable.SpacingAfter = 1;
                            listItemTable.DefaultCell.BorderWidth = 0;
                            if ( string.IsNullOrWhiteSpace( reservationSummary.Note ) ||
                                reservationLocation != reservationSummary.Locations.First() )
                            {
                                listItemTable.DefaultCell.BorderWidthBottom = 1;
                                listItemTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;
                            }

                            //Add the list items
                            listItemTable.AddCell( new Phrase( reservationSummary.EventDateTimeDescription, listItemFontNormal ) );

                            listItemTable.AddCell( new Phrase( reservationSummary.ReservationName, listItemFontNormal ) );
                            listItemTable.AddCell( new Phrase( reservationLocation.Location != null ? reservationLocation.Location.Name : string.Empty, listItemFontNormal ) );
                            listItemTable.AddCell( new Phrase( reservationLocation.LocationLayout != null ? reservationLocation.LocationLayout.Name : string.Empty, listItemFontNormal ) );
                            listItemTable.AddCell( new Phrase( reservationLocation.LocationLayout != null ? reservationLocation.LocationLayout.Description : string.Empty, listItemFontNormal ) );

                            iTextSharp.text.Image layoutPhoto = null;

                            try
                            {
                                if ( reservationLocation.LocationLayout.LayoutPhotoId != null )
                                {
                                    var appRootUri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
                                    var photoUrl = appRootUri + reservationLocation.LocationLayout.LayoutPhotoUrl;

                                    layoutPhoto = iTextSharp.text.Image.GetInstance( photoUrl );
                                    layoutPhoto.ScaleToFit( 100, 100 );
                                }
                            }
                            catch
                            {
                            }

                            PdfPCell photoCell = new PdfPCell();
                            photoCell.Border = 0;
                            photoCell.PaddingTop = 2;
                            photoCell.PaddingBottom = 2;
                            photoCell.BorderWidth = 0;
                            if ( layoutPhoto != null )
                            {
                                photoCell.AddElement( layoutPhoto );
                            }

                            if ( string.IsNullOrWhiteSpace( reservationSummary.Note ) ||
                                reservationLocation != reservationSummary.Locations.First() )
                            {
                                photoCell.BorderWidthBottom = 1;
                                photoCell.BorderColorBottom = Color.DARK_GRAY;
                            }
                            listItemTable.AddCell( photoCell );

                            PdfPCell contactCell = new PdfPCell();
                            contactCell.Border = 0;
                            contactCell.PaddingTop = -2;
                            contactCell.PaddingBottom = 2;
                            contactCell.AddElement( new Phrase( reservationSummary.Ministry != null ? reservationSummary.Ministry.Name : string.Empty, listItemFontNormal ) );
                            contactCell.AddElement( new Phrase( reservationSummary.ContactInfo, listItemFontNormal ) );
                            contactCell.BorderWidth = 0;
                            if ( string.IsNullOrWhiteSpace( reservationSummary.Note ) ||
                                reservationLocation != reservationSummary.Locations.First() )
                            {
                                contactCell.BorderWidthBottom = 1;
                                contactCell.BorderColorBottom = Color.DARK_GRAY;
                            }
                            listItemTable.AddCell( contactCell );

                            document.Add( listItemTable );

                            if ( !string.IsNullOrWhiteSpace( reservationSummary.Note ) &&
                                reservationLocation == reservationSummary.Locations.First() )
                            {
                                //document.Add( Chunk.NEWLINE );
                                var listNoteTable = new PdfPTable( 4 );
                                listNoteTable.LockedWidth = true;
                                listNoteTable.TotalWidth = PageSize.A4.Rotate().Width - document.LeftMargin - document.RightMargin;
                                listNoteTable.HorizontalAlignment = 1;
                                listNoteTable.SpacingBefore = 0;
                                listNoteTable.SpacingAfter = 1;
                                listNoteTable.DefaultCell.BorderWidth = 0;
                                listNoteTable.DefaultCell.BorderWidthBottom = 1;
                                listNoteTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;
                                listNoteTable.AddCell( new Phrase( string.Empty, noteFont ) );
                                listNoteTable.AddCell( new Phrase( reservationSummary.Note, noteFont ) );
                                listNoteTable.AddCell( new Phrase( string.Empty, noteFont ) );

                                listNoteTable.AddCell( new Phrase( string.Empty, noteFont ) );

                                document.Add( listNoteTable );
                            }
                        }
                    }
                }
                document.NewPage();
            }

            document.Close();

            return outputStream.ToArray();
        }
    }
}

/// <summary>
/// Class that manages parts of the PDF page.
/// </summary>
public class TwoColumnHeaderFooter : PdfPageEventHelper
{
    // This is the contentbyte object of the writer
    PdfContentByte cb;
    // we will put the final number of pages in a template
    PdfTemplate template;
    // this is the BaseFont we are going to use for the header / footer
    BaseFont bf = null;
    // This keeps track of the creation time
    DateTime PrintTime = DateTime.Now;

    #region Properties
    private string _Title;
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    public string Title
    {
        get { return _Title; }
        set { _Title = value; }
    }

    private string _CalendarDate;
    /// <summary>
    /// Gets or sets the calendar date.
    /// </summary>
    /// <value>
    /// The calendar date.
    /// </value>
    public string CalendarDate
    {
        get { return _CalendarDate; }
        set { _CalendarDate = value; }
    }

    private string _HeaderLeft;
    /// <summary>
    /// Gets or sets the header left.
    /// </summary>
    /// <value>
    /// The header left.
    /// </value>
    public string HeaderLeft
    {
        get { return _HeaderLeft; }
        set { _HeaderLeft = value; }
    }
    private string _HeaderRight;
    /// <summary>
    /// Gets or sets the header right.
    /// </summary>
    /// <value>
    /// The header right.
    /// </value>
    public string HeaderRight
    {
        get { return _HeaderRight; }
        set { _HeaderRight = value; }
    }
    private Font _HeaderFont;
    /// <summary>
    /// Gets or sets the header font.
    /// </summary>
    /// <value>
    /// The header font.
    /// </value>
    public Font HeaderFont
    {
        get { return _HeaderFont; }
        set { _HeaderFont = value; }
    }
    private Font _SubHeaderFont;
    /// <summary>
    /// Gets or sets the sub header font.
    /// </summary>
    /// <value>
    /// The sub header font.
    /// </value>
    public Font SubHeaderFont
    {
        get { return _SubHeaderFont; }
        set { _SubHeaderFont = value; }
    }

    private bool _IsHeaderShown;
    /// <summary>
    /// Gets or sets a value indicating whether this instance is header shown.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is header shown; otherwise, <c>false</c>.
    /// </value>
    public bool IsHeaderShown
    {
        get { return _IsHeaderShown; }
        set { _IsHeaderShown = value; }
    }

    #endregion
    
    /// <summary>
    /// we override the onOpenDocument method. Called when on opening of the document.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="document">The document.</param>
    public override void OnOpenDocument( PdfWriter writer, Document document )
    {
        try
        {
            PrintTime = DateTime.Now;
            bf = BaseFont.CreateFont( BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED );
            cb = writer.DirectContent;
            template = cb.CreateTemplate( 50, 50 );
        }
        catch
        {
            // not implemented
        }
    }

    /// <summary>
    /// Called when the start page event occurs.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="document">The document.</param>
    public override void OnStartPage( PdfWriter writer, Document document )
    {
        base.OnStartPage( writer, document );
        int pageN = writer.PageNumber;
        if ( pageN > 1 && IsHeaderShown )
        {
            document.Add( new Paragraph( CalendarDate, HeaderFont ) );

            //Build Subheaders
            var listSubHeaderTable = new PdfPTable( 4 );
            listSubHeaderTable.LockedWidth = true;
            listSubHeaderTable.TotalWidth = PageSize.A4.Rotate().Width - document.LeftMargin - document.RightMargin;
            listSubHeaderTable.HorizontalAlignment = 0;
            listSubHeaderTable.SpacingBefore = 10;
            listSubHeaderTable.SpacingAfter = 0;
            listSubHeaderTable.DefaultCell.BorderWidth = 0;
            listSubHeaderTable.DefaultCell.BorderWidthBottom = 1;
            listSubHeaderTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;

            listSubHeaderTable.AddCell( new Phrase( "Event Start/End", SubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Event", SubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Location", SubHeaderFont ) );
            listSubHeaderTable.AddCell( new Phrase( "Ministry / Contact Info", SubHeaderFont ) );

            document.Add( listSubHeaderTable );
        }
    }

    /// <summary>
    /// Called when the end page event occurs.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="document">The document.</param>
    public override void OnEndPage( PdfWriter writer, Document document )
    {
        base.OnEndPage( writer, document );
        int pageN = writer.PageNumber;
        String text = "Page " + pageN + " of ";
        float len = bf.GetWidthPoint( text, 8 );
        Rectangle pageSize = document.PageSize;
        cb.SetRGBColorFill( 100, 100, 100 );
        cb.BeginText();
        cb.SetFontAndSize( bf, 8 );
        cb.SetTextMatrix( pageSize.GetLeft( 40 ), pageSize.GetBottom( 30 ) );
        cb.ShowText( text );
        cb.EndText();
        cb.AddTemplate( template, pageSize.GetLeft( 40 ) + len, pageSize.GetBottom( 30 ) );

        cb.BeginText();
        cb.SetFontAndSize( bf, 8 );
        cb.ShowTextAligned( PdfContentByte.ALIGN_RIGHT,
        "Printed On " + PrintTime.ToString(),
        pageSize.GetRight( 40 ),
        pageSize.GetBottom( 30 ), 0 );
        cb.EndText();
    }
    /// <summary>
    /// Called when the close document event occurs.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="document">The document.</param>
    public override void OnCloseDocument( PdfWriter writer, Document document )
    {
        base.OnCloseDocument( writer, document );
        template.BeginText();
        template.SetFontAndSize( bf, 8 );
        template.SetTextMatrix( 0, 0 );
        template.ShowText( "" + ( writer.PageNumber - 1 ) );
        template.EndText();
    }

}