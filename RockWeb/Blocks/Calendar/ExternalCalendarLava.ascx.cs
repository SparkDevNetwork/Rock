﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Calendar
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Event Calendar Lava" )]
    [Category( "Event Calendar" )]
    [Description( "Displays details for a specific package." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Assets/Lava/EventCalendar/ExternalCalendar.lava' %}", "", 2 )]
    [BooleanField( "Show Campus Filter", "Determines whether the campus filters are shown", true )]
    [BooleanField( "Show Category Filter", "Determines whether the campus filters are shown", true )]
    [BooleanField( "Show Date Range Filter", "Determines whether the campus filters are shown", true )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar name.", false )]
    [IntegerField( "Event Calendar Id", "The Id of the event calendar to be displayed", true, 1 )]
    public partial class ExternalCalendarLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the accounts that are available for user to add to the list.
        /// </summary>
        protected String CurrentViewMode
        {
            get
            {
                var CurrentViewMode = ViewState["CurrentViewMode"] as String;
                if ( String.IsNullOrWhiteSpace( CurrentViewMode ) )
                {
                    CurrentViewMode = "Day";
                }

                return CurrentViewMode;
            }

            set
            {
                ViewState["CurrentViewMode"] = value;
            }
        }

        protected DateTime? SelectedDate
        {
            get
            {
                var SelectedDate = ViewState["SelectedDate"] as DateTime?;
                if ( SelectedDate == null)
                {
                    SelectedDate = DateTime.Today;
                }

                return SelectedDate;
            }

            set
            {
                ViewState["SelectedDate"] = value;
            }
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            calEventCalendar.SelectedDate = SelectedDate.Value;
            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                DisplayCalendarItemList();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayCalendarItemList();
        }

        #endregion

        protected void calEventCalendar_SelectionChanged( object sender, EventArgs e )
        {
            SelectedDate = calEventCalendar.SelectedDate;
            drpDateRange.UpperValue = null;
            drpDateRange.LowerValue = null;
            DisplayCalendarItemList();
        }

        protected void calEventCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( day == calEventCalendar.SelectedDate )
            {
                e.Cell.Style.Add( "font-weight", "bold" );
                e.Cell.AddCssClass( "alert-success" );
            }
            if ( CurrentViewMode == "Week" )
            {
                if ( day.StartOfWeek( DayOfWeek.Sunday ) == calEventCalendar.SelectedDate.StartOfWeek( DayOfWeek.Sunday ) )
                {
                    e.Cell.AddCssClass( "alert-success" );
                }
            }
            if ( CurrentViewMode == "Month" )
            {
                if ( day.Month == calEventCalendar.SelectedDate.Month )
                {
                    e.Cell.AddCssClass( "alert-success" );
                }
            }

        }

        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            DisplayCalendarItemList();
        }

        protected void cblCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            DisplayCalendarItemList();
        }

        protected void btnDay_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Day";
            DisplayCalendarItemList();
        }

        protected void btnWeek_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Week";
            DisplayCalendarItemList();
        }

        protected void btnMonth_Click( object sender, EventArgs e )
        {
            CurrentViewMode = "Month";
            DisplayCalendarItemList();
        }

        #region

        private void LoadDropDowns()
        {
            cblCategory.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() ) );
            //TODO: Fix this.
            cblCategory.Items.RemoveAt( 0 );
            cblCategory.Items.RemoveAt( 0 );
            cblCategory.Items.RemoveAt( 0 );
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
        }
        private void DisplayCalendarItemList()
        {
            cblCampus.Visible = GetAttributeValue( "ShowCampusFilter" ).AsBoolean();
            cblCategory.Visible = GetAttributeValue( "ShowCategoryFilter" ).AsBoolean();
            drpDateRange.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            // get package id
            int eventCalendarId = -1;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "EventCalendarId" ) ) )
            {
                eventCalendarId = Convert.ToInt32( GetAttributeValue( "EventCalendarId" ) );
            }

            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( new RockContext() );

            // Grab events
            var qry = eventCalendarItemService.Queryable( "EventItem, EventCalendar, EventItem.EventItemCampuses, EventItem.EventItemAudiences" )
                    .Where( m => m.EventCalendarId == eventCalendarId ).ToList();

            // Filter by campus
            List<int> campusIds = cblCampus.SelectedValuesAsInt;
            if ( campusIds.Any() )
            {
                qry = qry.Where( e => e.EventItem.EventItemCampuses.Any( c => ( c.CampusId.HasValue && campusIds.Contains( c.CampusId.Value ) ) || c.CampusId == null ) ).ToList();
            }

            //Filter by Category
            List<int> categories = cblCategory.SelectedValuesAsInt;
            if ( categories.Any() )
            {
                qry = qry.Where( i => i.EventItem.EventItemAudiences.Any( c => categories.Contains( c.DefinedValueId ) ) ).ToList();
            }
            // Filter by date

            //Daterange filter
            DateTime minusOneMonth = DateTime.Now.AddDays( -30 );
            DateTime plusOneMonth = DateTime.Now.AddDays( 30 );
            if ( drpDateRange.LowerValue.HasValue && drpDateRange.UpperValue.HasValue )
            {
                qry = qry.Where( i =>
                    i.EventItem.GetEarliestStartDate().HasValue
                    && i.EventItem.GetEarliestStartDate() > drpDateRange.LowerValue.Value
                    && i.EventItem.GetEarliestStartDate() < drpDateRange.UpperValue.Value
                    ).ToList();
            }
            else
            {
                if ( drpDateRange.LowerValue.HasValue )
                {
                    qry = qry.Where( i =>
                        i.EventItem.GetEarliestStartDate().HasValue
                        && i.EventItem.GetEarliestStartDate() > drpDateRange.LowerValue.Value
                        && i.EventItem.GetEarliestStartDate() < plusOneMonth
                        ).ToList();
                }
                if ( drpDateRange.UpperValue.HasValue )
                {
                    qry = qry.Where( i =>
                        i.EventItem.GetEarliestStartDate().HasValue
                        && i.EventItem.GetEarliestStartDate() > minusOneMonth
                        && i.EventItem.GetEarliestStartDate() < drpDateRange.UpperValue.Value
                        ).ToList();
                }
            }

            //Calendar filter
            if ( !drpDateRange.LowerValue.HasValue && !drpDateRange.UpperValue.HasValue )
            {
                if ( CurrentViewMode == "Day" )
                {
                    qry = qry.Where( i =>
                        i.EventItem.GetEarliestStartDate().HasValue
                        && i.EventItem.GetEarliestStartDate() == calEventCalendar.SelectedDate
                        ).ToList();
                }
                if ( CurrentViewMode == "Week" )
                {
                    qry = qry.Where( i =>
                        i.EventItem.GetEarliestStartDate().HasValue
                        && i.EventItem.GetEarliestStartDate().Value.StartOfWeek( DayOfWeek.Sunday ) == calEventCalendar.SelectedDate.StartOfWeek( DayOfWeek.Sunday )
                        ).ToList();
                }
                if ( CurrentViewMode == "Month" )
                {
                    qry = qry.Where( i =>
                        i.EventItem.GetEarliestStartDate().HasValue
                        && i.EventItem.GetEarliestStartDate().Value.Month == calEventCalendar.SelectedDate.Month
                        ).ToList();
                }
            }
            qry = qry.OrderByDescending( a => a.EventItem.GetEarliestStartDate() ).ToList();
            var events = qry.Select( e => new EventSummary { Event = e.EventItem, Date = e.EventItem.GetEarliestStartDate().Value.Date.ToShortDateString(), Time = e.EventItem.GetEarliestStartDate().Value.Date.ToShortTimeString(), Location = e.EventItem.EventItemCampuses.ToList().Select( c => c.Campus != null ? c.Campus.Name : "All Campuses" ).ToList().AsDelimited( "<br>" ), Description = e.EventItem.Description, DetailPage = String.IsNullOrWhiteSpace( e.EventItem.DetailsUrl ) ? null : e.EventItem.DetailsUrl } ).ToList();

            var mergeFields = new Dictionary<string, object>();

            mergeFields.Add( "Events", events );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() )
            {
                string pageTitle = "Calendar";
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }
        }

        [DotLiquid.LiquidType( "Event", "Date", "Time", "Location", "Description", "DetailPage" )]
        public class EventSummary
        {
            public EventItem Event { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }
            public String Location { get; set; }
            public String Description { get; set; }
            public String DetailPage { get; set; }
        }

        #endregion
    }
}