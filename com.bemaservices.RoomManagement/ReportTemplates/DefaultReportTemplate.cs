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
using com.bemaservices.RoomManagement.Model;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rock;
using Rock.Data;
using Rock.Model;

using Document = iTextSharp.text.Document;

namespace com.bemaservices.RoomManagement.ReportTemplates
{
    /// <summary>
    /// 
    /// </summary>
    [System.ComponentModel.Description( "The default report template" )]
    [Export( typeof( ReportTemplate ) )]
    [ExportMetadata( "ComponentName", "Default" )]
    public class DefaultReportTemplate : ReportTemplate
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
            Font zapfdingbats = new Font( Font.ZAPFDINGBATS );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                ReservationType = r.ReservationType,
                ApprovalState = r.ApprovalState.ConvertToString(),
                Locations = r.ReservationLocations.ToList(),
                Resources = r.ReservationResources.ToList(),
                CalendarDate = r.EventStartDateTime.ToLongDateString(),
                EventStartDateTime = r.EventStartDateTime,
                EventEndDateTime = r.EventEndDateTime,
                ReservationStartDateTime = r.ReservationStartDateTime,
                ReservationEndDateTime = r.ReservationEndDateTime,
                EventDateTimeDescription = r.EventTimeDescription,
                ReservationDateTimeDescription = r.ReservationTimeDescription,
                SetupPhotoId = r.SetupPhotoId,
                Note = r.Note
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();

            //Setup the document
            var document = new Document( PageSize.A4, 25, 25, 25, 25 );

            var outputStream = new MemoryStream();
            var writer = PdfWriter.GetInstance( document, outputStream );

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

            // Populate the Lists            
            foreach ( var reservationDay in reservationSummaries )
            {
                var firstReservation = reservationDay.FirstOrDefault();
                if ( firstReservation != null )
                {
                    //Build Header
                    document.Add( Chunk.NEWLINE );
                    String listHeader = firstReservation.CalendarDate;
                    document.Add( new Paragraph( listHeader, listHeaderFont ) );

                    //Build Subheaders
                    var listSubHeaderTable = new PdfPTable( 7 );
                    listSubHeaderTable.LockedWidth = true;
                    listSubHeaderTable.TotalWidth = PageSize.A4.Width - document.LeftMargin - document.RightMargin;
                    listSubHeaderTable.HorizontalAlignment = 0;
                    listSubHeaderTable.SpacingBefore = 10;
                    listSubHeaderTable.SpacingAfter = 0;
                    listSubHeaderTable.DefaultCell.BorderWidth = 0;
                    listSubHeaderTable.DefaultCell.BorderWidthBottom = 1;
                    listSubHeaderTable.DefaultCell.BorderColorBottom = Color.DARK_GRAY;

                    listSubHeaderTable.AddCell( new Phrase( "Name", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Event Time", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Reservation Time", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Locations", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Resources", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Has Layout?", listSubHeaderFont ) );
                    listSubHeaderTable.AddCell( new Phrase( "Status", listSubHeaderFont ) );

                    document.Add( listSubHeaderTable );

                    foreach ( var reservationSummary in reservationDay )
                    {
                        //Build the list item table
                        var listItemTable = new PdfPTable( 7 );
                        listItemTable.LockedWidth = true;
                        listItemTable.TotalWidth = PageSize.A4.Width - document.LeftMargin - document.RightMargin;
                        listItemTable.HorizontalAlignment = 0;
                        listItemTable.SpacingBefore = 0;
                        listItemTable.SpacingAfter = 1;
                        listItemTable.DefaultCell.BorderWidth = 0;

                        //Add the list items
                        listItemTable.AddCell( new Phrase( reservationSummary.ReservationName, listItemFontNormal ) );

                        listItemTable.AddCell( new Phrase( reservationSummary.EventDateTimeDescription, listItemFontNormal ) );

                        listItemTable.AddCell( new Phrase( reservationSummary.ReservationDateTimeDescription, listItemFontNormal ) );

                        List locationList = new List( List.UNORDERED, 8f );
                        locationList.SetListSymbol( "\u2022" );

                        foreach ( var reservationLocation in reservationSummary.Locations )
                        {
                            var listItem = new iTextSharp.text.ListItem( reservationLocation.Location.Name, listItemFontNormal );
                            if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Approved )
                            {
                                listItem.Add( new Phrase( "\u0034", zapfdingbats ) );
                            }
                            locationList.Add( listItem );
                        }

                        PdfPCell locationCell = new PdfPCell();
                        locationCell.Border = 0;
                        locationCell.PaddingTop = -2;
                        locationCell.AddElement( locationList );
                        listItemTable.AddCell( locationCell );

                        List resourceList = new List( List.UNORDERED, 8f );
                        resourceList.SetListSymbol( "\u2022" );

                        foreach ( var reservationResource in reservationSummary.Resources )
                        {
                            var listItem = new iTextSharp.text.ListItem( String.Format( "{0}({1})", reservationResource.Resource.Name, reservationResource.Quantity ), listItemFontNormal );
                            if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Approved )
                            {
                                listItem.Add( new Phrase( "\u0034", zapfdingbats ) );
                            }
                            resourceList.Add( listItem );
                        }

                        PdfPCell resourceCell = new PdfPCell();
                        resourceCell.Border = 0;
                        resourceCell.PaddingTop = -2;
                        resourceCell.AddElement( resourceList );
                        listItemTable.AddCell( resourceCell );

                        listItemTable.AddCell( new Phrase( reservationSummary.SetupPhotoId.HasValue.ToYesNo(), listItemFontNormal ) );

                        var listItemFont = ( reservationSummary.ApprovalState == "Unapproved" ) ? listItemFontUnapproved : listItemFontNormal;
                        listItemTable.AddCell( new Phrase( reservationSummary.ApprovalState, listItemFont ) );

                        document.Add( listItemTable );

                        if ( !string.IsNullOrWhiteSpace( reservationSummary.Note ) )
                        {
                            //document.Add( Chunk.NEWLINE );
                            var listNoteTable = new PdfPTable( 1 );
                            listNoteTable.LockedWidth = true;
                            listNoteTable.TotalWidth = PageSize.A4.Width - document.LeftMargin - document.RightMargin - 50;
                            listNoteTable.HorizontalAlignment = 1;
                            listNoteTable.SpacingBefore = 0;
                            listNoteTable.SpacingAfter = 1;
                            listNoteTable.DefaultCell.BorderWidth = 0;
                            listNoteTable.AddCell( new Phrase( reservationSummary.Note, noteFont ) );
                            document.Add( listNoteTable );
                        }
                    }
                }
            }

            document.Close();

            return outputStream.ToArray();
        }
    }
}
