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
using com.centralaz.RoomManagement.Attribute;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using com.centralaz.RoomManagement.ReportTemplates;
using System.Web;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Lava" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Renders a list of reservations in lava." )]

    [CustomRadioListField( "Location Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 1, category: "Filter Settings" )]
    [CustomRadioListField( "Resource Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 2, category: "Filter Settings" )]
    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 3, category: "Filter Settings" )]
    [CustomRadioListField( "Ministry Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "MinistryFilterDisplayMode", order: 4, category: "Filter Settings" )]
    [CustomRadioListField( "Approval Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "ApprovalFilterDisplayMode", order: 5, category: "Filter Settings" )]
    [CustomRadioListField( "Reservation Type Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "ReservationTypeFilterDisplayMode", order: 6, category: "Filter Settings" )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 7, category: "Filter Settings" )]

    [LinkedPage( "Details Page", "Detail page for events", order: 8, category: "Lava Settings" )]
    [DefinedValueField( "13B169EA-A090-45FF-8B11-A9E02776E35E", "Visible Printable Report Options", "The Printable Reports that the user is able to select", true, true, "5D53E2F0-BA82-4154-B996-085C979FACB0,46C855B0-E50E-49E7-8B99-74561AFB3DD2", "Lava Settings", 9 )]
    [DefinedValueField( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Visible Reservation View Options", "The Reservation Views that the user is able to select", true, true, "67EA36B0-D861-4399-998E-3B69F7700DC0", "Lava Settings", 10 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "Lava Settings", 11 )]

    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week", order: 12, category: "View Settings" )]
    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 13, category: "View Settings" )]
    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 14, category: "View Settings" )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 15, category: "View Settings" )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 16, category: "View Settings" )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 17, category: "View Settings" )]
    [BooleanField( "Show Year View", "Determines whether the year view option is shown", false, order: 18, category: "View Settings" )]

    public partial class ReservationLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        protected bool LocationPanelOpen { get; set; }
        protected bool LocationPanelClosed { get; set; }
        protected bool ResourcePanelOpen { get; set; }
        protected bool ResourcePanelClosed { get; set; }
        protected bool CampusPanelOpen { get; set; }
        protected bool CampusPanelClosed { get; set; }
        protected bool MinistryPanelOpen { get; set; }
        protected bool MinistryPanelClosed { get; set; }
        protected bool ApprovalPanelOpen { get; set; }
        protected bool ApprovalPanelClosed { get; set; }
        protected bool ReservationTypePanelOpen { get; set; }
        protected bool ReservationTypePanelClosed { get; set; }

        #endregion

        #region Properties

        private String ViewMode { get; set; }

        private int? ReservationViewId { get; set; }
        private DateTime? FilterStartDate { get; set; }
        private DateTime? FilterEndDate { get; set; }
        private List<DateTime> ReservationDates { get; set; }

        private String PreferenceKey
        {
            get
            {
                return string.Format( "reservation-lava-{0}-", this.BlockId );
            }
        }

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
            ReservationViewId = ViewState["ReservationViewId"] as int?;
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

            LocationPanelOpen = GetAttributeValue( "LocationFilterDisplayMode" ) == "3";
            LocationPanelClosed = GetAttributeValue( "LocationFilterDisplayMode" ) == "4";

            ResourcePanelOpen = GetAttributeValue( "ResourceFilterDisplayMode" ) == "3";
            ResourcePanelClosed = GetAttributeValue( "ResourceFilterDisplayMode" ) == "4";

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";

            MinistryPanelOpen = GetAttributeValue( "MinistryFilterDisplayMode" ) == "3";
            MinistryPanelClosed = GetAttributeValue( "MinistryFilterDisplayMode" ) == "4";

            ApprovalPanelOpen = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "3";
            ApprovalPanelClosed = GetAttributeValue( "ApprovalFilterDisplayMode" ) == "4";

            ReservationTypePanelOpen = GetAttributeValue( "ReservationTypeFilterDisplayMode" ) == "3";
            ReservationTypePanelClosed = GetAttributeValue( "ReservationTypeFilterDisplayMode" ) == "4";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // register lbPrint as a PostBackControl since it is returning a File download
            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( rptReports );

            RockPage.AddScriptLink( "~/Plugins/com_centralaz/RoomManagement/Assets/Scripts/circle-progress.js", fingerprint: false );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/RoomManagement/Assets/Scripts/event-calendar.js", fingerprint: false );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/RoomManagement/Assets/Scripts/moment.js", fingerprint: false );
            RockPage.AddCSSLink( "~/Plugins/com_centralaz/RoomManagement/Assets/Styles/event-calendar.css", fingerprint: false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            var reportCache = DefinedTypeCache.Get( "13B169EA-A090-45FF-8B11-A9E02776E35E" );
            var selectedReports = GetAttributeValue( "VisiblePrintableReportOptions" ).SplitDelimitedValues().AsGuidList();
            rptReports.DataSource = reportCache.DefinedValues.Where( dv => selectedReports.Contains( dv.Guid ) ).ToList();
            rptReports.DataBind();

            var viewCache = DefinedTypeCache.Get( "32EC3B34-01CF-4513-BC2E-58ECFA91D010" );
            var selectedViews = GetAttributeValue( "VisibleReservationViewOptions" ).SplitDelimitedValues().AsGuidList();
            var definedValueList = viewCache.DefinedValues.Where( dv => selectedViews.Contains( dv.Guid ) ).ToList();
            rptViews.DataSource = definedValueList;
            rptViews.DataBind();

            // Set User Preference
            ReservationViewId = this.GetUserPreference( PreferenceKey + "ReservationViewId" ).AsIntegerOrNull();
            if ( ReservationViewId == null && definedValueList.FirstOrDefault() != null )
            {
                ReservationViewId = definedValueList.FirstOrDefault().Id;
            }

            if ( !Page.IsPostBack )
            {
                if ( SetFilterControls() )
                {
                    if ( ReservationViewId != null && hfSelectedView.ValueAsInt() == 0 )
                    {
                        hfSelectedView.Value = ReservationViewId.ToString();
                    }

                    if ( definedValueList.Count > 1 )
                    {
                        var selectedValue = DefinedValueCache.Get( hfSelectedView.ValueAsInt() );
                        lSelectedView.Text = string.Format( "View As: {0}", selectedValue.Value );
                        divViewDropDown.Visible = true;
                    }
                    else
                    {
                        divViewDropDown.Visible = false;
                    }

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
            ViewState["ReservationViewId"] = ReservationViewId;
            ViewState["FilterStartDate"] = FilterStartDate;
            ViewState["FilterEndDate"] = FilterEndDate;
            ViewState["ReservationDates"] = ReservationDates;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            btnDay.RemoveCssClass( "active" );
            btnWeek.RemoveCssClass( "active" );
            btnMonth.RemoveCssClass( "active" );
            btnYear.RemoveCssClass( "active" );
            pnlCalendar.RemoveCssClass( "hidden" );
            ypYearPicker.Visible = false;

            switch ( ViewMode )
            {
                case "Day":
                    btnDay.AddCssClass( "active" );
                    break;

                case "Week":
                    btnWeek.AddCssClass( "active" );
                    break;

                case "Month":
                    btnMonth.AddCssClass( "active" );
                    break;

                case "Year":
                    btnYear.AddCssClass( "active" );
                    pnlCalendar.AddCssClass( "hidden" );
                    ypYearPicker.Visible = true;

                    if ( ypYearPicker.SelectedYear.HasValue )
                    {
                        dpEndDate.SelectedDate = new DateTime( ypYearPicker.SelectedYear.Value, 12, 31 );
                        // Start at the current date if they have the current year selected.
                        if ( ypYearPicker.SelectedYear.Value == RockDateTime.Today.Year )
                        {
                            FilterStartDate = RockDateTime.Today;
                        }
                        else
                        {
                            FilterStartDate = new DateTime( ypYearPicker.SelectedYear.Value, 1, 1 );
                        }
                        FilterEndDate = dpEndDate.SelectedDate;
                        BindData();
                    }
                    else
                    {
                        ypYearPicker.SelectedYear = RockDateTime.Now.Year;
                    }

                    break;

                default:
                    break;
            }

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

        protected void lipLocation_SelectItem( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Locations", lipLocation.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            BindData();
        }

        protected void rpResource_SelectItem( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Resources", rpResource.SelectedValues.AsIntegerList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Campuses", cblCampus.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblMinistry_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Ministries", cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblApproval control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblApproval_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Approval State", cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>().ConvertToInt() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblReservationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblReservationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Reservation Type", cblReservationType.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList().AsDelimited( "," ) );
            BindData();
        }

        protected void dpStartDate_TextChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "Start Date", dpStartDate.SelectedDate.ToString() );
            FilterStartDate = dpStartDate.SelectedDate;
            BindData();
        }

        protected void dpEndDate_TextChanged( object sender, EventArgs e )
        {
            this.SetUserPreference( PreferenceKey + "End Date", dpEndDate.SelectedDate.ToString() );
            FilterEndDate = dpEndDate.SelectedDate;
            BindData();
        }

        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                this.SetUserPreference( PreferenceKey + "ViewMode", btnViewMode.Text );
                ViewMode = btnViewMode.Text;
                ResetCalendarSelection();
                BindData();
            }
        }

        protected void btnAllReservations_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.All ).ToString();
            BindData( ShowBy.All );
        }

        protected void btnMyReservations_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.MyReservations ).ToString();
            BindData( ShowBy.MyReservations );
        }

        protected void btnMyApprovals_Click( object sender, EventArgs e )
        {
            hfShowBy.Value = ( ( int ) ShowBy.MyApprovals ).ToString();
            BindData( ShowBy.MyApprovals );
        }
        protected void rptReports_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var definedValueId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                PrintReport( definedValueId.Value );
            }
        }
        protected void rptViews_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var definedValueId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                var selectedValue = DefinedValueCache.Get( definedValueId.Value );
                this.SetUserPreference( PreferenceKey + "ReservationViewId", selectedValue.Id.ToString() );
                lSelectedView.Text = string.Format( "View As: {0}", selectedValue.Value );
                hfSelectedView.Value = definedValueId.ToString();
                BindData();
            }
        }

        #endregion

        #region Methods
        private void BindData()
        {
            var showBy = ( ShowBy ) hfShowBy.ValueAsInt();
            BindData( showBy );
        }

        private void BindData( ShowBy showBy )
        {
            HighlightActionButtons( showBy );

            List<ReservationService.ReservationSummary> reservationSummaryList = GetReservationSummaries( showBy );

            // Bind to Grid
            var reservationSummaries = reservationSummaryList.Select( r => new
            {
                Id = r.Id,
                ReservationName = r.ReservationName,
                ReservationType = r.ReservationType,
                ApprovalState = r.ApprovalState.ConvertToString(),
                ApprovalStateInt = r.ApprovalState.ConvertToInt(),
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
                SetupPhotoLink = ResolveRockUrl( String.Format( "~/GetImage.ashx?id={0}", r.SetupPhotoId ?? 0 ) ),
                Note = r.Note,
                RequesterAlias = r.RequesterAlias,
                EventContactPersonAlias = r.EventContactPersonAlias,
                EventContactEmail = r.EventContactEmail,
                EventContactPhoneNumber = r.EventContactPhoneNumber,
                MinistryName = r.ReservationMinistry != null ? r.ReservationMinistry.Name : string.Empty,
            } )
            .OrderBy( r => r.EventStartDateTime )
            .GroupBy( r => r.EventStartDateTime.Date )
            .Select( r => r.ToList() )
            .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "FilterStartDate", FilterStartDate );
            mergeFields.Add( "FilterEndDate", FilterEndDate );
            mergeFields.Add( "DetailsPage", LinkedPageUrl( "DetailsPage", null ) );
            mergeFields.Add( "ReservationSummaries", reservationSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            var definedValue = new DefinedValueService( new RockContext() ).Get( hfSelectedView.ValueAsInt() );
            definedValue.LoadAttributes();

            var lavaTemplate = definedValue.GetAttributeValue( "Lava" );
            var lavaCommands = definedValue.GetAttributeValue( "LavaCommands" );

            lOutput.Text = lavaTemplate.ResolveMergeFields( mergeFields, lavaCommands );

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

        protected void PrintReport( int definedValueId )
        {
            var definedValue = new DefinedValueService( new RockContext() ).Get( definedValueId );
            definedValue.LoadAttributes();

            var logoFileUrl = definedValue.GetAttributeValue( "ReportLogo" );
            var reportTemplateGuid = definedValue.GetAttributeValue( "ReportTemplate" ).AsGuidOrNull();
            var reportFont = definedValue.GetAttributeValue( "ReportFont" );
            var reportLava = definedValue.GetAttributeValue( "Lava" );

            var showBy = ( ShowBy ) hfShowBy.ValueAsInt();
            List<ReservationService.ReservationSummary> reservationSummaryList = GetReservationSummaries( showBy );

            if ( !logoFileUrl.ToLower().StartsWith( "http" ) )
            {
                logoFileUrl = Server.MapPath( ResolveRockUrl( logoFileUrl ) );
            }

            var reportTemplate = GetReportTemplate( reportTemplateGuid );

            var outputArray = reportTemplate.GenerateReport( reservationSummaryList, logoFileUrl, reportFont, FilterStartDate, FilterEndDate, reportLava );

            Response.ClearHeaders();
            Response.ClearContent();
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader( "Content-Disposition", string.Format( "attachment;filename=Reservation Schedule for {0} - {1}.pdf", FilterStartDate.Value.ToString( "MMMM d" ), FilterEndDate.Value.ToString( "MMMM d" ) ) );
            Response.BinaryWrite( outputArray );
            Response.Flush();
            Response.End();
            return;
        }

        private void HighlightActionButtons( ShowBy showBy )
        {
            switch ( showBy )
            {
                case ShowBy.All:
                    btnAllReservations.AddCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
                case ShowBy.MyReservations:
                    btnAllReservations.RemoveCssClass( "active btn-primary" );
                    btnMyReservations.AddCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
                case ShowBy.MyApprovals:
                    btnAllReservations.RemoveCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.AddCssClass( "active btn-primary" );
                    break;
                default:
                    btnAllReservations.AddCssClass( "active btn-primary" );
                    btnMyReservations.RemoveCssClass( "active btn-primary" );
                    btnMyApprovals.RemoveCssClass( "active btn-primary" );
                    break;
            }
        }
        private List<ReservationService.ReservationSummary> GetReservationSummaries()
        {
            return GetReservationSummaries( ShowBy.All );
        }

        private List<ReservationService.ReservationSummary> GetReservationSummaries( ShowBy showBy )
        {
            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );
            var qry = reservationService.Queryable();

            // Do additional filtering based on the ShowBy selection (My Reservations, My Approvals)
            switch ( showBy )
            {
                case ShowBy.MyReservations:
                    qry = qry.Where( r => r.CreatedByPersonAliasId == CurrentPersonAliasId || r.AdministrativeContactPersonAliasId == CurrentPersonAliasId || r.EventContactPersonAliasId == CurrentPersonAliasId );
                    break;
                case ShowBy.MyApprovals:
                    if ( CurrentPersonId.HasValue )
                    {
                        var myLocationsToApproveIds = new List<int>();
                        var myResourcesToApproveIds = new List<int>();

                        // NICK TODO: GetLocationsByApprovalGroupMembership is not returning the locations correctly, I'll probably will need to re-write it
                        var myLocationsToApprove = reservationService.GetLocationsByApprovalGroupMembership( CurrentPersonId.Value );
                        if ( myLocationsToApprove != null )
                        {
                            myLocationsToApproveIds = myLocationsToApprove.Select( l => l.Id ).ToList();
                        }

                        var myResourcesToApprove = reservationService.GetResourcesByApprovalGroupMembership( CurrentPersonId.Value );
                        if ( myResourcesToApprove != null )
                        {
                            myResourcesToApproveIds = myResourcesToApprove.Select( r => r.Id ).ToList();
                        }

                        qry = qry.Where( r => r.ReservationLocations.Any( rl => ( myLocationsToApproveIds.Contains( rl.LocationId ) ) ) ||
                                            r.ReservationResources.Any( rr => ( myResourcesToApproveIds.Contains( rr.ResourceId ) ) ||
                                            ( r.ReservationType.FinalApprovalGroup != null && r.ReservationType.FinalApprovalGroup.Members.Any( m => m.PersonId == CurrentPersonId.Value && m.GroupMemberStatus == GroupMemberStatus.Active ) ) )
                                        );
                    }
                    break;
                default:
                    break;
            }
            // Filter by Resources
            var resourceIdList = rpResource.SelectedValuesAsInt().ToList();
            if ( resourceIdList.Where( r => r != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationResources.Any( rr => resourceIdList.Contains( rr.ResourceId ) ) );
            }

            // Filter by Locations
            var locationIdList = lipLocation.SelectedValuesAsInt().ToList();
            if ( locationIdList.Where( r => r != 0 ).Any() )
            {
                qry = qry.Where( r => r.ReservationLocations.Any( rr => locationIdList.Contains( rr.LocationId ) ) );
            }

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
            List<String> ministryNames = cblMinistry.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Text ).ToList();
            if ( ministryNames.Any() )
            {
                qry = qry
                    .Where( r =>
                        !r.ReservationMinistryId.HasValue ||    // All
                        ministryNames.Contains( r.ReservationMinistry.Name ) );
            }

            // Filter by Approval
            List<ReservationApprovalState> approvalValues = cblApproval.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.ConvertToEnum<ReservationApprovalState>() ).ToList();
            if ( approvalValues.Any() )
            {
                qry = qry
                    .Where( r =>
                        approvalValues.Contains( r.ApprovalState ) );
            }

            // Filter by Reservation Type
            List<int> reservationTypeIds = cblReservationType.Items.OfType<System.Web.UI.WebControls.ListItem>().Where( l => l.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            if ( reservationTypeIds.Any() )
            {
                qry = qry
                    .Where( r => reservationTypeIds.Contains( r.ReservationTypeId ) );
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
            ViewMode = this.GetUserPreference( PreferenceKey + "ViewMode" );
            if ( string.IsNullOrWhiteSpace( ViewMode ) )
            {
                ViewMode = GetAttributeValue( "DefaultViewOption" );
            }

            if ( !GetAttributeValue( string.Format( "Show{0}View", ViewMode ) ).AsBoolean() )
            {
                ViewMode = GetAttributeValue( "DefaultViewOption" );
                //ShowError( "Configuration Error", string.Format( "The Default View Option setting has been set to '{0}', but the Show {0} View setting has not been enabled.", ViewMode ) );
                //return false;
            }

            // Show/Hide calendar control
            pnlCalendar.Visible = GetAttributeValue( "ShowSmallCalendar" ).AsBoolean();

            // Get the first/last dates based on today's date and the viewmode setting
            var today = RockDateTime.Today;

            if ( PageParameter( "SelectedDate" ).AsDateTime() != null )
            {
                today = PageParameter( "SelectedDate" ).AsDateTime().Value;
                calReservationCalendar.VisibleDate = today;
            }

            // Use the CalendarVisibleDate if it's in session.
            if ( Session["CalendarVisibleDate"] != null )
            {
                today = ( DateTime ) Session["CalendarVisibleDate"];
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
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( RockDateTime.Today.Year, RockDateTime.Today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 12 );
            }

            // Setup small calendar Filter
            calReservationCalendar.FirstDayOfWeek = _firstDayOfWeek.ConvertToInt().ToString().ConvertToEnum<FirstDayOfWeek>();
            calReservationCalendar.SelectedDates.Clear();
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );

            // Setup Location Filter
            lipLocation.Visible = GetAttributeValue( "LocationFilterDisplayMode" ).AsInteger() > 1;
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Locations" ) ) )
            {
                lipLocation.SetValues( this.GetUserPreference( PreferenceKey + "Locations" ).Split( ',' ).AsIntegerList() );
            }

            // Setup Resource Filter
            rpResource.Visible = GetAttributeValue( "ResourceFilterDisplayMode" ).AsInteger() > 1;
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Resources" ) ) )
            {
                rpResource.SetValues( this.GetUserPreference( PreferenceKey + "Resources" ).Split( ',' ).AsIntegerList() );
            }

            // Setup Campus Filter
            rcwCampus.Visible = GetAttributeValue( "CampusFilterDisplayMode" ).AsInteger() > 1;
            cblCampus.DataSource = CampusCache.All( false );
            cblCampus.DataBind();
            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Campuses" ) ) )
            {
                cblCampus.SetValues( this.GetUserPreference( PreferenceKey + "Campuses" ).SplitDelimitedValues() );
            }
            else
            {
                if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                {
                    var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus;
                    if ( contextCampus != null )
                    {
                        cblCampus.SetValue( contextCampus.Id );
                    }
                }
            }

            // Setup Ministry Filter
            rcwMinistry.Visible = GetAttributeValue( "MinistryFilterDisplayMode" ).AsInteger() > 1;
            cblMinistry.DataSource = ReservationMinistryCache.All().DistinctBy( rmc => rmc.Name ).OrderBy( m => m.Name );
            cblMinistry.DataBind();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Ministries" ) ) )
            {
                cblMinistry.SetValues( this.GetUserPreference( PreferenceKey + "Ministries" ).SplitDelimitedValues() );
            }

            // Setup Approval Filter
            rcwApproval.Visible = GetAttributeValue( "ApprovalFilterDisplayMode" ).AsInteger() > 1;
            cblApproval.BindToEnum<ReservationApprovalState>();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Approval State" ) ) )
            {
                cblApproval.SetValues( this.GetUserPreference( PreferenceKey + "Approval State" ).SplitDelimitedValues() );
            }

            // Setup Reservation Type Filter
            rcwReservationType.Visible = GetAttributeValue( "ReservationTypeFilterDisplayMode" ).AsInteger() > 1;
            cblReservationType.DataSource = new ReservationTypeService( new RockContext() ).Queryable().ToList();
            cblReservationType.DataBind();

            if ( !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Reservation Type" ) ) )
            {
                cblReservationType.SetValues( this.GetUserPreference( PreferenceKey + "Reservation Type" ).SplitDelimitedValues() );
            }

            // Date Range Filter
            dpStartDate.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            if ( dpStartDate.Visible && !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "Start Date" ) ) )
            {
                dpStartDate.SelectedDate = this.GetUserPreference( PreferenceKey + "Start Date" ).AsDateTime();
                if ( dpStartDate.SelectedDate.HasValue )
                {
                    FilterStartDate = dpStartDate.SelectedDate;
                }
            }

            dpEndDate.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            if ( dpEndDate.Visible && !string.IsNullOrWhiteSpace( this.GetUserPreference( PreferenceKey + "End Date" ) ) )
            {
                dpEndDate.SelectedDate = this.GetUserPreference( PreferenceKey + "End Date" ).AsDateTime();
                if ( dpEndDate.SelectedDate.HasValue )
                {
                    FilterEndDate = dpEndDate.SelectedDate;
                }
            }

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean(),
                GetAttributeValue( "ShowYearView" ).AsBoolean()
            };

            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];
            btnYear.Visible = howManyVisible > 1 && viewsVisible[3];

            // Set filter visibility
            bool showFilter = ( pnlCalendar.Visible || lipLocation.Visible || rpResource.Visible || rcwCampus.Visible || rcwMinistry.Visible || rcwApproval.Visible || dpStartDate.Visible || dpEndDate.Visible );
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
            else if ( ViewMode == "Year" )
            {
                FilterStartDate = new DateTime( RockDateTime.Today.Year, RockDateTime.Today.Month, 1 );
                FilterEndDate = FilterStartDate.Value.AddMonths( 12 );
            }

            dpStartDate.SelectedDate = dpEndDate.SelectedDate = null;
            this.SetUserPreference( PreferenceKey + "Start Date", null );
            this.SetUserPreference( PreferenceKey + "End Date", null );

            // Reset the selection
            calReservationCalendar.SelectedDates.SelectRange( FilterStartDate.Value, FilterEndDate.Value );
        }

        private void SetCalendarFilterDates()
        {
            FilterStartDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[0] : ( DateTime? ) null;
            FilterEndDate = calReservationCalendar.SelectedDates.Count > 0 ? calReservationCalendar.SelectedDates[calReservationCalendar.SelectedDates.Count - 1] : ( DateTime? ) null;
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


        /// <summary>
        /// Gets the report template.
        /// </summary>
        /// <param name="reportTemplateEntityTypeId">The report template entity type identifier.</param>
        /// <returns></returns>
        private ReportTemplate GetReportTemplate( Guid? reportTemplateGuid )
        {
            if ( reportTemplateGuid.HasValue )
            {
                var reportTemplateEntityType = EntityTypeCache.Get( reportTemplateGuid.Value );
                if ( reportTemplateEntityType == null )
                {
                    return null;
                }
                else
                {
                    return ReportTemplateContainer.GetComponent( reportTemplateEntityType.Name );
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Helper Classes, etc.

        /// <summary>
        ///
        /// </summary>
        private enum ShowBy
        {
            /// <summary>
            /// All reservations
            /// </summary>
            All = 0,

            /// <summary>
            /// Only my reservations
            /// </summary>
            MyReservations = 1,

            /// <summary>
            /// Only resevations that need my approval
            /// </summary>
            MyApprovals = 2

        }
        #endregion

    }
}
