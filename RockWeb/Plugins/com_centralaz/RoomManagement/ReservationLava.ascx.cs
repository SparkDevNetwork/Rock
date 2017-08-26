// <copyright>
// Copyright by the Central Christian Church
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
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

using com.centralaz.RoomManagement.Model;
using com.centralaz.RoomManagement.Web.Cache;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Lava" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Renders a list of reservations in lava." )]

    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week", order: 1 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 2 )]

    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 3 )]
    [CustomRadioListField( "Ministry Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "MinistryFilterDisplayMode", order: 4 )]
    [CustomRadioListField( "Approval Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "ApprovalFilterDisplayMode", order: 4 )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 6 )]

    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 7 )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 8 )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 9 )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 10 )]

    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Themes/Stark/Assets/Lava/CalendarGroupedOccurrence.lava' %}", "", 12 )]

    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 13 )]

    [TextField( "Report Font", "", true, "Gotham", "", 0 )]
    [TextField( "Report Logo", "URL to the logo (PNG) to display in the printed report.", true, "~/Plugins/com_centralaz/RoomManagement/Assets/Icons/Central_Logo_Black_rgb_165_90.png", "", 0 )]
    [TextField( "Font Awesome Ttf", "URL to the FontAwesome ttf to use for checkmarks in the printed report", true, "~/Assets/Fonts/FontAwesome/fontawesome-webfont.ttf", "", 0 )]

    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 14 )]
    public partial class ReservationLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        protected bool CampusPanelOpen { get; set; }
        protected bool CampusPanelClosed { get; set; }
        protected bool MinistryPanelOpen { get; set; }
        protected bool MinistryPanelClosed { get; set; }
        protected bool ApprovalPanelOpen { get; set; }
        protected bool ApprovalPanelClosed { get; set; }

        #endregion

        #region Properties

        private String ViewMode { get; set; }
        private DateTime? FilterStartDate { get; set; }
        private DateTime? FilterEndDate { get; set; }
        private List<DateTime> ReservationDates { get; set; }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ViewMode = ViewState["ViewMode"] as String;
            FilterStartDate = ViewState["FilterStartDate"] as DateTime?;
            FilterEndDate = ViewState["FilterEndDate"] as DateTime?;
            ReservationDates = ViewState["ReservationDates"] as List<DateTime>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _firstDayOfWeek = GetAttributeValue( "StartofWeekDay" ).ConvertToEnum<DayOfWeek>();

            lVersionText.Text = com.centralaz.RoomManagement.VersionInfo.GetPluginProductVersionNumber();

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";
            MinistryPanelOpen = GetAttributeValue( "MinistryFilterDisplayMode" ) == "3";
            MinistryPanelClosed = GetAttributeValue( "MinistryFilterDisplayMode" ) == "4";
            ApprovalPanelOpen = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "3";
            ApprovalPanelClosed = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "4";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // register lbPrint as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( lbPrint );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                if ( SetFilterControls() )
                {
                    pnlDetails.Visible = true;
                    BindData();
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }

        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ViewMode"] = ViewMode;
            ViewState["FilterStartDate"] = FilterStartDate;
            ViewState["FilterEndDate"] = FilterEndDate;
            ViewState["ReservationDates"] = ReservationDates;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            btnDay.CssClass = "btn btn-default" + ( ViewMode == "Day" ? " active" : "" );
            btnWeek.CssClass = "btn btn-default" + ( ViewMode == "Week" ? " active" : "" );
            btnMonth.CssClass = "btn btn-default" + ( ViewMode == "Month" ? " active" : "" );

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( SetFilterControls() )
            {
                pnlDetails.Visible = true;
                BindData();
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_SelectionChanged( object sender, EventArgs e )
        {
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the DayRender event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( ReservationDates != null && ReservationDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.AddCssClass( "calendar-hasevent" );
            }
        }

        /// <summary>
        /// Handles the VisibleMonthChanged event of the calReservationCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MonthChangedEventArgs"/> instance containing the event data.</param>
        protected void calReservationCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            calReservationCalendar.SelectedDate = e.NewDate;
            Session["CalendarVisibleDate"] = e.NewDate;
            ResetCalendarSelection();
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "Campuses", cblCampus.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblMinistry_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "Ministries", cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblApproval control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblApproval_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( "Approval State", cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>().ConvertToInt() ).Where( a => a != null ).ToList().AsDelimited( "," ) );
            BindData();
        }

        protected void lbDateRangeRefresh_Click( object sender, EventArgs e )
        {
            FilterStartDate = drpDateRange.LowerValue;
            FilterEndDate = drpDateRange.UpperValue;
            BindData();
        }

        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                this.SetUserPreference( "ViewMode", btnViewMode.Text );
                ViewMode = btnViewMode.Text;
                ResetCalendarSelection();
                BindData();
            }
        }

        protected void lbPrint_Click( object sender, EventArgs e )
        {
            //Fonts
            String font = GetAttributeValue( "ReportFont" );
            var titleFont = FontFactory.GetFont( font, 16, Font.BOLD );
            var listHeaderFont = FontFactory.GetFont( font, 12, Font.BOLD, Color.DARK_GRAY );
            var listSubHeaderFont = FontFactory.GetFont( font, 10, Font.BOLD, Color.DARK_GRAY );
            var listItemFontNormal = FontFactory.GetFont( font, 8, Font.NORMAL );
            var listItemFontUnapproved = FontFactory.GetFont( font, 8, Font.ITALIC, Color.MAGENTA );
            var noteFont = FontFactory.GetFont( font, 8, Font.NORMAL, Color.GRAY );

            List<ReservationService.ReservationSummary> reservationSummaryList = GetReservationSummaries();

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

            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance( document, output );

            document.Open();

            // Add logo
            try
            {
                string logoUri = GetAttributeValue( "ReportLogo" );
                string fileUrl = string.Empty;
                iTextSharp.text.Image logo;
                if ( logoUri.ToLower().StartsWith( "http" ) )
                {
                    logo = iTextSharp.text.Image.GetInstance( new Uri( logoUri ) );
                }
                else
                {
                    fileUrl = Server.MapPath( ResolveRockUrl( logoUri ) );
                    logo = iTextSharp.text.Image.GetInstance( fileUrl );
                }

                logo.Alignment = iTextSharp.text.Image.RIGHT_ALIGN;
                logo.ScaleToFit( 100, 55 );
                document.Add( logo );
            }
            catch { }

            // Write the document
            var today = RockDateTime.Today;
            var filterStartDateTime = FilterStartDate.HasValue ? FilterStartDate.Value : today;
            var filterEndDateTime = FilterEndDate.HasValue ? FilterEndDate.Value : today.AddMonths( 1 );
            String title = String.Format( "Reservations for: {0} - {1}", filterStartDateTime.ToString( "MMMM d" ), filterEndDateTime.ToString( "MMMM d" ) );
            document.Add( new Paragraph( title, titleFont ) );

            var fontAwesomeUrl = Server.MapPath( ResolveRockUrl( GetAttributeValue( "FontAwesomeTtf" ) ) );
            var fontAwesome = BaseFont.CreateFont( fontAwesomeUrl, BaseFont.IDENTITY_H, BaseFont.EMBEDDED );
            Font fontAwe = new Font( fontAwesome, 8 );

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
                                listItem.Add( new Phrase( "\uf00c", fontAwe ) );
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
                                listItem.Add( new Phrase( "\uf00c", fontAwe ) );
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

            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader( "Content-Disposition", string.Format( "attachment;filename=Reservation Schedule for {0} - {1}.pdf", filterStartDateTime.ToString( "MMMM d" ), filterEndDateTime.ToString( "MMMM d" ) ) );
            Response.BinaryWrite( output.ToArray() );
            Response.Flush();
            Response.End();
            return;
        }

        #endregion

        #region Methods

        private void BindData()
        {
            List<ReservationService.ReservationSummary> reservationSummaryList = GetReservationSummaries();

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
                EventDateTimeDescription = r.EventTimeDescription,
                ReservationDateTimeDescription = r.ReservationTimeDescription,
                SetupPhotoId = r.SetupPhotoId,
                SetupPhotoLink = ResolveRockUrl( String.Format( "~/GetImage.ashx?id={0}", r.SetupPhotoId ?? 0 ) )
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "ReservationSummaries", reservationSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
            else
            {
                lDebug.Visible = false;
                lDebug.Text = string.Empty;
            }

        }

        private List<ReservationService.ReservationSummary> GetReservationSummaries()
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable();

            // Filter by campus
            List<int> campusIds = cblCampus.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( campusIds.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.CampusId.HasValue ||    // All
                        campusIds.Contains( r.CampusId.Value ) );
            }

            // Filter by Ministry
            List<int> ministryIds = cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( ministryIds.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.ReservationMinistryId.HasValue ||    // All
                        ministryIds.Contains( r.ReservationMinistryId.Value ) );
            }

            // Filter by Approval
            List<ReservationApprovalState> approvalValues = cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>() ).Where( a => a != null ).ToList();
            if ( approvalValues.Any() )
            {
                qry = qry
                    .Where( r =>
                        approvalValues.Contains( r.ApprovalState ) );
            }

            // Filter by Time
            var today = RockDateTime.Today;
            var filterStartDateTime = FilterStartDate.HasValue ? FilterStartDate.Value : today;
            var filterEndDateTime = FilterEndDate.HasValue ? FilterEndDate.Value : today.AddMonths( 1 );
            var reservationSummaryList = reservationService.GetReservationSummaries( qry, filterStartDateTime, filterEndDateTime, true );
            return reservationSummaryList;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private bool SetFilterControls()
        {
            // Get and verify the view mode
            ViewMode = this.GetUserPreference( "ViewMode" );
            if ( string.IsNullOrWhiteSpace( ViewMode ) )
            {
                ViewMode = GetAttributeValue( "DefaultViewOption" );
            }

            if ( !GetAttributeValue( string.Format( "Show{0}View", ViewMode ) ).AsBoolean() )
            {
                ShowError( "Configuration Error", string.Format( "The Default View Option setting has been set to '{0}', but the Show {0} View setting has not been enabled.", ViewMode ) );
                return false;
            }

            // Show/Hide calendar control
            pnlCalendar.Visible = GetAttributeValue( "ShowSmallCalendar" ).AsBoolean();

            // Get the first/last dates based on today's date and the viewmode setting
            var today = RockDateTime.Today;

            // Use the CalendarVisibleDate if it's in session.
            if ( Session["CalendarVisibleDate"] != null )
            {
                today = (DateTime)Session["CalendarVisibleDate"];
                calReservationCalendar.VisibleDate = today;
            }

            FilterStartDate = today;
            FilterEndDate = today;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = today.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = today.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( today.Year, today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }

            // Setup small calendar Filter
            calReservationCalendar.FirstDayOfWeek = _firstDayOfWeek.ConvertToInt().ToString().ConvertToEnum<FirstDayOfWeek>();
            calReservationCalendar.SelectedDates.Clear();
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );

            // Setup Campus Filter
            rcwCampus.Visible = GetAttributeValue( "CampusFilterDisplayMode" ).AsInteger() > 1;
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( "Campuses" ) ) )
            {
                cblCampus.SetValues( this.GetUserPreference( "Campuses" ).SplitDelimitedValues() );
            }
            else
            {
                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Read( "Rock.Model.Campus" ) ) as Campus;
                    if ( contextCampus != null )
                    {
                        cblCampus.SetValue( contextCampus.Id );
                    }
                }
            }

            // Setup Ministry Filter
            rcwMinistry.Visible = GetAttributeValue( "MinistryFilterDisplayMode" ).AsInteger() > 1;
            cblMinistry.DataSource = ReservationMinistryCache.All();
            cblMinistry.DataBind();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( "Ministries" ) ) )
            {
                cblMinistry.SetValues( this.GetUserPreference( "Ministries" ).SplitDelimitedValues() );
            }

            // Setup Approval Filter
            rcwApproval.Visible = GetAttributeValue( "ApprovalFilterDisplayMode" ).AsInteger() > 1;
            cblApproval.BindToEnum<ReservationApprovalState>();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( "Approval State" ) ) )
            {
                cblApproval.SetValues( this.GetUserPreference( "Approval State" ).SplitDelimitedValues() );
            }

            // Date Range Filter
            drpDateRange.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            lbDateRangeRefresh.Visible = drpDateRange.Visible;
            drpDateRange.LowerValue = FilterStartDate;
            drpDateRange.UpperValue = FilterEndDate;

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean()
            };

            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];

            // Set filter visibility
            bool showFilter = ( pnlCalendar.Visible || rcwCampus.Visible || rcwMinistry.Visible || rcwApproval.Visible || drpDateRange.Visible );
            pnlFilters.Visible = showFilter;
            pnlList.CssClass = showFilter ? "col-md-9" : "col-md-12";

            return true;
        }

        /// <summary>
        /// Resets the calendar selection. The control is configured for day selection, but selection will be changed to the week or month if that is the viewmode being used
        /// </summary>
        private void ResetCalendarSelection()
        {
            // Even though selection will be a single date due to calendar's selection mode, set the appropriate days
            var selectedDate = calReservationCalendar.SelectedDate;
            calReservationCalendar.VisibleDate = selectedDate;
            Session["CalendarVisibleDate"] = selectedDate;
            FilterStartDate = selectedDate;
            FilterEndDate = selectedDate;
            if ( ViewMode == "Week" )
            {
                FilterStartDate = selectedDate.StartOfWeek( _firstDayOfWeek );
                FilterEndDate = selectedDate.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                FilterStartDate = new DateTime( selectedDate.Year, selectedDate.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 1 ).AddDays( -1 );
            }

            // Reset the selection
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );
        }

        private void SetCalendarFilterDates()
        {
            FilterStartDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[0] : (DateTime?)null;
            FilterEndDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[calReservationCalendar.SelectedDates.Count - 1] : (DateTime?)null;
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        #endregion
    }
}