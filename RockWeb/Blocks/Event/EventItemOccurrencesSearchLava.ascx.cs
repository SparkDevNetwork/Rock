// <copyright>
// Copyright by the Spark Development Network
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// </summary>
    [DisplayName( "Event Item Occurrences Search Lava" )]
    [Category( "Event" )]
    [Description( "Block does a search for occurrences of events based on the EventCalendarId specified in the URL" )]

    [CodeEditorField( "Results Lava Template", "The lava template for display the results of the search", CodeEditorMode.Lava, CodeEditorTheme.Rock, order: 2, required: false, defaultValue: @"
{% for occurrence in EventItemOccurrences %}
        
    <div class='row margin-b-lg'>
        <div class='col-md-4'>
            <h1>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'MMM d' }}</h1>
            <h2>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'dddd h:mmtt' }}</h2>
        </div>  
        <div class='col-md-8'>    
            <h3>{{ occurrence.EventItem.Name }}</h3>
            <p>{{ occurrence.EventItem.Description}}</p>                
        </div>
    </div>
{% endfor %}
" )]

    [SlidingDateRangeField( "Default Date Range", "The Default date range selection", false, "Next|10|Week||", enabledSlidingDateRangeTypes: "Next,Upcoming,Current", order: 3 )]

    [LinkedPage( "Event Detail Page", "The page to use for showing event details.", required: false, order: 4 )]
    [BooleanField( "Use Campus Context", "Set this to true to set the campus filter based on the campus context.", defaultValue: false, order: 5 )]
    public partial class EventItemOccurrencesSearchLava : RockBlock
    {
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

            // tell the browser to not cache this page so that it'll reload this page and apply the last search settings
            // when the user navigates back to it. If we don't do this, the browser will show the the initial full load of the page (without the results)
            this.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            this.Response.Cache.SetNoStore();

            if ( !Page.IsPostBack )
            {
                BindFilter();

                // load and apply the SearchSettings if this user navigates back to this page during the session
                var searchSettings = this.Session[string.Format( "SearchSettings_Block_{0}_EventCalendarId_", this.BlockId, this.PageParameter( "EventCalendarId" ) )] as SearchSettings;
                if ( searchSettings != null )
                {
                    cpCampusPicker.SelectedCampusId = searchSettings.CampusId;
                    pDateRange.LowerValue = searchSettings.DateRange.Start;
                    pDateRange.UpperValue = searchSettings.DateRange.End;
                }
                else
                {
                    var defaultDateRangeDelimitedValues = this.GetAttributeValue( "DefaultDateRange" );
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( defaultDateRangeDelimitedValues );
                    pDateRange.LowerValue = dateRange.Start;
                    pDateRange.UpperValue = dateRange.End;
                }

                ShowResults();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cpCampusPicker.Campuses = CampusCache.All();
            cpCampusPicker.Items[0].Text = "All";

            var campusEntityType = EntityTypeCache.Read( typeof( Campus ) );

            if ( this.GetAttributeValue( "UseCampusContext" ).AsBoolean() )
            {
                var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( currentCampus != null )
                {
                    cpCampusPicker.SelectedCampusId = currentCampus.Id;
                }
            }
        }

        /// <summary>
        /// Shows the results.
        /// </summary>
        private void ShowResults()
        {
            RockContext rockContext = new RockContext();

            var qry = new EventItemOccurrenceService( rockContext ).Queryable().Where( e => e.EventItem.IsActive );

            int? eventCalendarId = this.PageParameter( "EventCalendarId" ).AsIntegerOrNull();
            if ( eventCalendarId.HasValue )
            {
                qry = qry.Where( e => e.EventItem.EventCalendarItems.Any( x => x.EventCalendarId == eventCalendarId ) );
            }

            // filter by Campus (filter)
            if ( cpCampusPicker.SelectedCampusId.HasValue )
            {
                int campusId = cpCampusPicker.SelectedCampusId.Value;
                qry = qry.Where( a => a.CampusId == campusId );
            }

            // retrieve occurrences into a List so we can do additional filtering against the Calendar data
            List<EventItemOccurrence> itemOccurrences = qry.ToList();

            // filter by date range
            var dateRange = new DateRange( pDateRange.LowerValue, pDateRange.UpperValue );
            if ( dateRange.Start != null && dateRange.End != null )
            {
                itemOccurrences.RemoveAll( o => o.GetStartTimes( dateRange.Start.Value, dateRange.End.Value ).Count() == 0 );
            }
            else
            {
                // default show all future
                itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, DateTime.Now.AddDays( 365 ) ).Count() == 0 );
            }

            // sort results
            itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).ToList();

            // make lava merge fields
            var mergeFields = new Dictionary<string, object>();

            mergeFields.Add( "EventDetailPage", LinkedPageRoute( "EventDetailPage" ) );

            mergeFields.Add( "EventItemOccurrences", itemOccurrences );

            if ( eventCalendarId.HasValue )
            {
                mergeFields.Add( "EventCalendar", new EventCalendarService( rockContext ).Get( eventCalendarId.Value ) );
            }

            lResults.Text = GetAttributeValue( "ResultsLavaTemplate" ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            ShowResults();

            // Make it so that the page displays the search results after navigating back from the Event Details page
            // Use this approach: Do a no-cache so the browser won't cache (see this.OnLoad()), and then reload the page with the same search parameters and reload the results

            // Use Session since this is a public facing page and there probably isn't a logged in user
            this.Session[string.Format( "SearchSettings_Block_{0}_EventCalendarId_", this.BlockId, this.PageParameter( "EventCalendarId" ) )] = new SearchSettings
            {
                CampusId = cpCampusPicker.SelectedCampusId,
                DateRange = new DateRange( pDateRange.LowerValue, pDateRange.UpperValue )
            };
        }

        /// <summary>
        /// The Search settings for this block
        /// </summary>
        private class SearchSettings
        {
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the date range.
            /// </summary>
            /// <value>
            /// The date range.
            /// </value>
            public DateRange DateRange { get; set; }
        }

        #endregion
    }
}