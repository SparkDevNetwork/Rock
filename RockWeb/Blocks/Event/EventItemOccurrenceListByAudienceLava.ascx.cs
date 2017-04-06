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
    [DisplayName( "Event Item Occurrence List By Audience Lava" )]
    [Category( "Event" )]
    [Description( "Block that takes a audience and displays calendar item occurrences for it using Lava." )]
    
    [TextField("List Title", "The title to make available in the lava.", false, "Upcoming Events", order: 0)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, "Audience", "The audience to show calendar items for.", order: 0)]
    [EventCalendarField("Calendar", "Filters the events by a specific calendar.", false, order: 1)]
    [CampusesField("Campuses", "List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", required: false, order:2, includeInactive:true)]
    [BooleanField("Use Campus Context", "Determine if the campus should be read from the campus context of the page.", order: 3)]
    [SlidingDateRangeField("Date Range", "Optional date range to filter the occurrences on.", false, enabledSlidingDateRangeTypes: "Next,Upcoming,Current", order:4)]
    [IntegerField("Max Occurrences", "The maximum number of occurrences to show.", false, 100, order: 5)]
    [LinkedPage( "Event Detail Page", "The page to use for showing event details.", order: 6 )]
    [LinkedPage( "Registration Page", "The page to use for registrations.", order: 7 )]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue: "{% include '~~/Assets/Lava/EventItemOccurrenceListByAudience.lava' %}", order: 8 )]
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
                                            .Where(e => e.EventItem.EventItemAudiences.Any(a => a.DefinedValue.Guid == audienceGuid) && e.EventItem.IsActive);

                // filter occurrences for campus (always include the "All Campuses" events)
                if ( GetAttributeValue( "UseCampusContext" ).AsBoolean() )
                {
                    var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
                    var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                    if ( contextCampus != null )
                    {
                        qry = qry.Where( e => e.CampusId == contextCampus.Id || !e.CampusId.HasValue );
                    }
                }
                else
                {
                    if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "Campuses" ) ) )
                    {
                        var selectedCampuses = Array.ConvertAll( GetAttributeValue( "Campuses" ).Split( ',' ), s => new Guid( s ) ).ToList();
                        qry = qry.Where( e => selectedCampuses.Contains(e.Campus.Guid) || !e.CampusId.HasValue );
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
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, DateTime.Now.AddDays( 365 ) ).Count() == 0 );
                }

                // limit results
                int maxItems = GetAttributeValue( "MaxOccurrences" ).AsInteger();
                itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).Take( maxItems ).ToList();
                
                // make lava merge fields
                var mergeFields = new Dictionary<string, object>();

                var contextObjects = new Dictionary<string, object>();
                foreach (var contextEntityType in RockPage.GetContextEntityTypes())
                {
                    var contextEntity = RockPage.GetCurrentContext(contextEntityType);
                    if (contextEntity != null && contextEntity is DotLiquid.ILiquidizable)
                    {
                        var type = Type.GetType(contextEntityType.AssemblyName ?? contextEntityType.Name);
                        if (type != null)
                        {
                            contextObjects.Add(type.Name, contextEntity);
                        }
                    }

                }

                if (contextObjects.Any())
                {
                    mergeFields.Add("Context", contextObjects);
                }

                mergeFields.Add( "ListTitle", GetAttributeValue("ListTitle") );
                mergeFields.Add( "EventDetailPage", LinkedPageRoute( "EventDetailPage" ) );
                mergeFields.Add( "RegistrationPage", LinkedPageRoute( "RegistrationPage" ) );
                mergeFields.Add( "EventItemOccurrences", itemOccurrences );
               
                lContent.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>No audience is configured for this block.</div>";
            }
        }

        #endregion
    }
}