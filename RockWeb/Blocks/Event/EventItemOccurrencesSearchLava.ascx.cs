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

    [CodeEditorField(
        "Results Lava Template",
        Description = "The lava template for display the results of the search",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Order = 0,
        IsRequired = false,
        Key = AttributeKey.ResultsLavaTemplate,
        DefaultValue = @"
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
    [SlidingDateRangeField(
        "Default Date Range",
        Description = "The Default date range selection",
        IsRequired = false,
        DefaultValue = "Next|10|Week||",
        EnabledSlidingDateRangeTypes = "Next,Upcoming,Current",
        Key = AttributeKey.DefaultDateRange,
        Order = 1 )]
    [LinkedPage(
        "Event Detail Page",
        Description = "The page to use for showing event details.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.EventDetailPage )]
    [BooleanField(
        "Use Campus Context",
        Description = "Set this to true to set the campus filter based on the campus context.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.UseCampusContext )]
    [CustomDropdownListField(
        "Show Campus Filter",
        Description = "This setting will control if/when the campus dropdown filter shown.",
        ListSource = "0^Always,1^Never,2^When No Context",
        IsRequired = false,
        DefaultValue = "0",
        Key = AttributeKey.ShowCampusFilter,
        Order = 4 )]
    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down filter.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 5 )]
    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down filter.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 6 )]
    [EventCalendarField(
        "Event Calendar",
        Description = "This  setting would override any setting in the query string if provided.",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.EventCalendar )]
    [BooleanField(
        "Show Audience Filter",
        Description = "When enabled the audience filter will be shown.",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.ShowAudienceFilter )]
    [DefinedValueField(
        "Filter Audiences",
        Description = "Determines which audiences should be displayed in the filter.",
        IsRequired = false,
        AllowMultiple = true,
        Key = AttributeKey.FilterAudiences,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE,
        Order = 9 )]
    [BooleanField(
        "Show Date Range Filter",
        Description = "Determines whether the date range filters are shown.",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.ShowDateRangeFilter )]
    [Rock.SystemGuid.BlockTypeGuid( "01CA4723-8290-41C6-A2D2-88469FAA48E9" )]
    public partial class EventItemOccurrencesSearchLava : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ResultsLavaTemplate = "ResultsLavaTemplate";
            public const string ShowCampusFilter = "ShowCampusFilter";
            public const string DefaultDateRange = "DefaultDateRange";
            public const string EventDetailPage = "EventDetailPage";
            public const string UseCampusContext = "UseCampusContext";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string EventCalendar = "EventCalendar";
            public const string ShowAudienceFilter = "ShowAudienceFilter";
            public const string FilterAudiences = "FilterAudiences";
            public const string ShowDateRangeFilter = "ShowDateRangeFilter";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string EventCalendarId = "EventCalendarId";
        }

        #endregion Page Parameter Keys

        #region Properties

        /// <summary>
        /// Gets or sets the event calendar identifier.
        /// </summary>
        /// <value>
        /// The event calendar identifier.
        /// </value>
        public int? EventCalendarId
        {
            get { return ViewState[ViewStateKey.EventCalendarId] as int?; }
            set { ViewState[ViewStateKey.EventCalendarId] = value; }
        }

        #endregion

        private static class ViewStateKey
        {
            public const string EventCalendarId = "EventCalendarId";
        }

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
            // tell the browser to not cache this page so that it'll reload this page and apply the last search settings
            // when the user navigates back to it. If we don't do this, the browser will show the the initial full load of the page (without the results)
            this.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            this.Response.Cache.SetNoStore();

            if ( !Page.IsPostBack )
            {
                BindFilter();

                // load and apply the SearchSettings if this user navigates back to this page during the session
                GetEventCalendar();
                var searchSettings = this.Session[string.Format( "SearchSettings_Block_{0}_EventCalendarId_", this.BlockId, EventCalendarId )] as SearchSettings;
                if ( searchSettings != null )
                {
                    cpCampusPicker.SelectedCampusId = searchSettings.CampusId;
                    pDateRange.LowerValue = searchSettings.DateRange.Start;
                    pDateRange.UpperValue = searchSettings.DateRange.End;
                    cblAudience.SetValues( searchSettings.Audiences );
                }
                else
                {
                    var defaultDateRangeDelimitedValues = this.GetAttributeValue( AttributeKey.DefaultDateRange );
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( defaultDateRangeDelimitedValues );
                    pDateRange.LowerValue = dateRange.Start;
                    pDateRange.UpperValue = dateRange.End;
                }

                ShowResults();
            }

            base.OnLoad( e );
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
            // show campus filter
            int? showCampusFilter = GetAttributeValue( AttributeKey.ShowCampusFilter ).AsIntegerOrNull();
            bool showCampus = IsCampusEnabled( showCampusFilter );

            if ( showCampus )
            {
                var selectedCampusTypeIds = GetAttributeValue( AttributeKey.CampusTypes )
                  .SplitDelimitedValues( true )
                  .AsGuidList()
                  .Select( a => DefinedValueCache.Get( a ) )
                  .Where( a => a != null )
                  .Select( a => a.Id )
                  .ToList();

                cpCampusPicker.Campuses = CampusCache.All();

                if ( selectedCampusTypeIds.Any() )
                {
                    cpCampusPicker.CampusTypesFilter = selectedCampusTypeIds;
                }

                var selectedCampusStatusIds = GetAttributeValue( AttributeKey.CampusStatuses )
                    .SplitDelimitedValues( true )
                    .AsGuidList()
                    .Select( a => DefinedValueCache.Get( a ) )
                    .Where( a => a != null )
                    .Select( a => a.Id )
                    .ToList();

                if ( selectedCampusStatusIds.Any() )
                {
                    cpCampusPicker.CampusStatusFilter = selectedCampusStatusIds;
                }

                if ( showCampusFilter.Value == 0 )
                {
                    var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
                    var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                    if ( currentCampus != null )
                    {
                        cpCampusPicker.SelectedCampusId = currentCampus.Id;
                    }
                }
                cpCampusPicker.Items[0].Text = "All";
            }

            cpCampusPicker.Visible = divCampus.Visible = showCampus && cpCampusPicker.Items.Count > 2;

            // Date Range Filter
            pDateRange.Visible = divDateRange.Visible = GetAttributeValue( AttributeKey.ShowDateRangeFilter ).AsBoolean();

            // Setup Audience Filter
            var selectedCategoryGuids = GetAttributeValue( AttributeKey.FilterAudiences ).SplitDelimitedValues( true ).AsGuidList();
            var showAudienceFilter = GetAttributeValue( AttributeKey.ShowAudienceFilter ).AsBoolean() && selectedCategoryGuids.Any();
            cblAudience.Visible = showAudienceFilter;
            if ( showAudienceFilter )
            {
                var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
                if ( definedType != null )
                {
                    cblAudience.DataSource = definedType.DefinedValues.Where( v => selectedCategoryGuids.Contains( v.Guid ) );
                    cblAudience.DataBind();
                }
            }
        }

        /// <summary>
        /// Determine if campus is enabled
        /// </summary>
        private bool IsCampusEnabled( int? showCampusFilter )
        {
            var showCampus = true;
            if ( showCampusFilter.HasValue )
            {
                if ( showCampusFilter.Value == 1 )
                {
                    showCampus = false;
                }
                else if ( GetAttributeValue( AttributeKey.UseCampusContext ).AsBoolean() && showCampusFilter.Value == 2 )
                {
                    var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
                    var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                    if ( currentCampus != null )
                    {
                        showCampus = false;
                    }
                }
            }

            return showCampus;
        }

        /// <summary>
        /// Shows the results.
        /// </summary>
        private void ShowResults()
        {
            RockContext rockContext = new RockContext();

            var qry = new EventItemOccurrenceService( rockContext ).Queryable().Where( e => e.EventItem.IsActive && e.EventItem.IsApproved );

            if ( EventCalendarId.HasValue )
            {
                qry = qry.Where( e => e.EventItem.EventCalendarItems.Any( x => x.EventCalendarId == EventCalendarId.Value ) );
            }

            int? showCampusFilter = GetAttributeValue( AttributeKey.ShowCampusFilter ).AsIntegerOrNull();
            bool showCampus = IsCampusEnabled( showCampusFilter );
            if ( showCampus )
            {
                if ( cpCampusPicker.Visible && cpCampusPicker.SelectedCampusId.HasValue )
                {
                    int campusId = cpCampusPicker.SelectedCampusId.Value;

                    // If an EventItemOccurrence's CampusId is null, then the occurrence is an 'All Campuses' event occurrence, so include those
                    qry = qry.Where( a => a.CampusId == null || a.CampusId == campusId );
                }
                else
                {
                    var campusIds = cpCampusPicker
                        .Items
                        .Cast<ListItem>()
                        .Select( i => i.Value )
                        .AsIntegerList();
                    qry = qry.Where( a => a.CampusId == null || campusIds.Contains( a.CampusId.Value ) );
                }
            }
            else if ( GetAttributeValue( AttributeKey.UseCampusContext ).AsBoolean() )
            {
                var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
                var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;
                if ( currentCampus != null )
                {
                    qry = qry.Where( a => a.CampusId == null || a.CampusId == currentCampus.Id );
                }
            }

            if ( cblAudience.Visible )
            {
                // Filter by Category
                List<int> audiences = cblAudience.SelectedValuesAsInt;
                if ( audiences.Any() )
                {
                    qry = qry.Where( i => i.EventItem.EventItemAudiences.Any( c => audiences.Contains( c.DefinedValueId ) ) );
                }
            }

            // retrieve occurrences into a List so we can do additional filtering against the Calendar data
            List<EventItemOccurrence> itemOccurrences = qry.ToList();

            if ( pDateRange.Visible )
            {
                // filter by date range
                var dateRange = new DateRange( pDateRange.LowerValue, pDateRange.UpperValue );
                if ( dateRange.Start != null && dateRange.End != null )
                {
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( dateRange.Start.Value, dateRange.End.Value ).Count() == 0 );
                }
                else
                {
                    // default show all future
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, RockDateTime.Now.AddDays( 365 ) ).Count() == 0 );
                }
            }
            else
            {
                // default show all future
                itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, RockDateTime.Now.AddDays( 365 ) ).Count() == 0 );
            }

            // sort results
            itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).ToList();

            // make lava merge fields
            var mergeFields = new Dictionary<string, object>();

            mergeFields.Add( "EventDetailPage", LinkedPageRoute( AttributeKey.EventDetailPage ) );

            mergeFields.Add( "EventItemOccurrences", itemOccurrences );

            if ( EventCalendarId.HasValue )
            {
                mergeFields.Add( "EventCalendar", new EventCalendarService( rockContext ).Get( EventCalendarId.Value ) );
            }

            lResults.Text = GetAttributeValue( AttributeKey.ResultsLavaTemplate ).ResolveMergeFields( mergeFields );
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
            this.Session[string.Format( "SearchSettings_Block_{0}_EventCalendarId_", this.BlockId, EventCalendarId )] = new SearchSettings
            {
                CampusId = cpCampusPicker.SelectedCampusId,
                DateRange = new DateRange( pDateRange.LowerValue, pDateRange.UpperValue ),
                Audiences = cblAudience.SelectedValuesAsInt
            };
        }

        #region Private Methods

        /// <summary>
        /// Loads the Event Calendar
        /// </summary>
        private EventCalendarCache GetEventCalendar()
        {
            // Get the event calendar id (initial page request)
            if ( !EventCalendarId.HasValue )
            {
                // Get event calendar set by attribute value
                Guid eventCalendarGuid = GetAttributeValue( AttributeKey.EventCalendar ).AsGuid();

                EventCalendarCache _eventCalendarCache = null;
                if ( !eventCalendarGuid.IsEmpty() )
                {
                    _eventCalendarCache = EventCalendarCache.Get( eventCalendarGuid );
                }

                // If an attribute value was not provided, check for query/route value
                if ( _eventCalendarCache != null )
                {
                    EventCalendarId = _eventCalendarCache.Id;
                }
                else
                {
                    EventCalendarId = PageParameter( PageParameterKey.EventCalendarId ).AsIntegerOrNull();
                }
            }

            // Get the workflow type
            if ( EventCalendarId.HasValue )
            {
                return EventCalendarCache.Get( EventCalendarId.Value );
            }
            else
            {
                return null;
            }
        }

        #endregion Private Methods

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

            /// <summary>
            /// Gets or sets the date range.
            /// </summary>
            /// <value>
            /// The date range.
            /// </value>
            public List<int> Audiences { get; set; }
        }

        #endregion
    }
}