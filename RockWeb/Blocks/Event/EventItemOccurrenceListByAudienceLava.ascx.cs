// <copyright>
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
using System.Data.Entity;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Calendar Item Occurrence List By Audience Lava" )]
    [Category( "Event" )]
    [Description( "Block that takes a audience and displays calendar item occurrences for it using Lava." )]
    
    [TextField("List Title", "The title to make available in the lava.", false, "Upcoming Events", order: 0)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Audience", "The audience to show calendar items for.", order: 0)]
    [EventCalendarField("Calendar", "Filters the events by a specific calendar.", false, order: 1)]
    [CampusesField("Campuses", "List of which campuses to show occurrences for. This setting will be ignored in the 'Use Campus Context' is enabled.", order:2)]
    [BooleanField("Use Campus Context", "Determine if the campus should be read from the campus context of the page.", order: 3)]
    [SlidingDateRangeField("Date Range", "Optional date range to filter the occurrences on.", false, enabledSlidingDateRangeTypes: "Next,Upcoming,Current", order:4)]
    [IntegerField("Max Occurrences", "The maximum number of occurrences to show.", false, 100, order: 5)]
    [LinkedPage( "Registration Page", "The page to use for registrations.", order: 6 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue: "{% include '~~/Assets/Lava/EventItemOccurrenceListByAudience.lava' %}", order: 7 )]
    [BooleanField("Enable Debug", "Show the lava merge fields.", order: 8)]
    public partial class EventItemOccurrenceListByAudienceLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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

            if ( !Page.IsPostBack )
            {
                LoadContent();
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
            LoadContent();
        }

        #endregion

        #region Methods

        private void LoadContent()
        {
            var audienceGuid = GetAttributeValue( "Audience" ).AsGuid();

            if ( audienceGuid != Guid.Empty )
            {
                lMessages.Text = string.Empty;
                RockContext rockContext = new RockContext();

                // get event occurrences
                var qry = new EventItemOccurrenceService( rockContext ).Queryable()
                                            .Where( e => e.EventItem.EventItemAudiences.Any(a => a.DefinedValue.Guid == audienceGuid) );

                // filter occurrences for campus
                if ( GetAttributeValue( "UseCampusContext" ).AsBoolean() )
                {
                    var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
                    var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                    if ( contextCampus != null )
                    {
                        qry = qry.Where( e => e.CampusId == contextCampus.Id );
                    }
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Campuses" ) ) )
                    {
                        var selectedCampuses = Array.ConvertAll( GetAttributeValue( "Campuses" ).Split( ',' ), s => new Guid( s ) ).ToList();
                        qry = qry.Where( e => selectedCampuses.Contains( e.Campus.Guid ) );
                    }
                }

                // filter by calendar
                var calendarGuid = GetAttributeValue( "Calendar" ).AsGuid();

                if ( calendarGuid != Guid.Empty )
                {
                    qry = qry.Where( e => e.EventItem.EventCalendarItems.Any( c => c.EventCalendar.Guid == calendarGuid ) );
                }

                // retrieve occurrences
                var itemOccurrences = qry.ToList();

                // filter by date range
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) );
                if ( dateRange.Start != null && dateRange.End != null )
                {
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( dateRange.Start.Value, dateRange.End.Value ).Count() == 0 );
                }
                else
                {
                    // default show all future
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, DateTime.MaxValue ).Count() == 0 );
                }

                // limit results
                int maxItems = GetAttributeValue( "MaxOccurrences" ).AsInteger();
                itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).Take( maxItems ).ToList();
                
                // make lava merge fields
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "ListTitle", GetAttributeValue("ListTitle") );
                mergeFields.Add( "RegistrationPage", LinkedPageUrl( "RegistrationPage", null ) );
                mergeFields.Add( "EventItemOccurrences", itemOccurrences );
               
                lContent.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = @"<div class='alert alert-info'>Due to the size of the lava members the debug info for this block has been supressed. Below are high-level details of
                                    the merge objects available.
                                    <ul>
                                        <li>List Title - The title to pass to lava.
                                        <li>EventItemOccurrences - A list of EventItemOccurrences. View the EvenItemOccurrence model for these properties.</li>
                                        <li>RegistrationPage  - String that contains the relative path to the registration page.</li>
                                        <li>Global Attribute  - Access to the Global Attributes.</li>
                                    </ul>
                                    </div>";
                }
                else
                {
                    lDebug.Visible = false;
                    lDebug.Text = string.Empty;
                }
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>No audience is configured for this block.</div>";
            }
        }

        #endregion
    }
}