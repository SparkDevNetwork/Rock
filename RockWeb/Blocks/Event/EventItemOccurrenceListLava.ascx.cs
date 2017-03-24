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
    [DisplayName( "Event Item Occurrence List Lava" )]
    [Category( "Event" )]
    [Description( "Block that takes a calendar item and displays occurrences for it using Lava." )]
    
    [EventItemField("Event Item", "The event item to use to display occurrences for.", order: 0)]
    [CampusesField("Campuses", "List of which campuses to show occurrences for. This setting will be ignored in the 'Use Campus Context' is enabled.", order:1, includeInactive:true)]
    [BooleanField("Use Campus Context", "Determine if the campus should be read from the campus context of the page.", order: 2)]
    [SlidingDateRangeField("Date Range", "Optional date range to filter the occurrences on.", false, enabledSlidingDateRangeTypes: "Next,Upcoming,Current", order:3)]
    [IntegerField("Max Occurrences", "The maximum number of occurrences to show.", false, 100, order: 4)]
    [LinkedPage( "Registration Page", "The page to use for registrations.", order: 5 )]
    [CodeEditorField("Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue:"{% include '~~/Assets/Lava/EventItemOccurrenceList.lava' %}", order:6)]
    public partial class EventItemOccurrenceListLava : Rock.Web.UI.RockBlock
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
            var eventItemGuid = GetAttributeValue( "EventItem" ).AsGuid();

            if ( eventItemGuid != Guid.Empty )
            {
                lMessages.Text = string.Empty;
                RockContext rockContext = new RockContext();

                // get event occurrences
                var qry = new EventItemOccurrenceService( rockContext ).Queryable()
                                            .Where( e => e.EventItem.Guid == eventItemGuid );

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
                    // default show all future (max 1 year)
                    itemOccurrences.RemoveAll( o => o.GetStartTimes( RockDateTime.Now, RockDateTime.Now.AddDays(365) ).Count() == 0 );
                }

                // limit results
                int maxItems = GetAttributeValue( "MaxOccurrences" ).AsInteger();
                itemOccurrences = itemOccurrences.OrderBy( i => i.NextStartDateTime ).Take( maxItems ).ToList();
                
                // load event item
                var eventItem = new EventItemService( rockContext ).Get( eventItemGuid );

                // make lava merge fields
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "RegistrationPage", LinkedPageRoute( "RegistrationPage" ) );
                mergeFields.Add( "EventItem", eventItem);
                mergeFields.Add( "EventItemOccurrences", itemOccurrences );

                // add context to merge fields
                var contextEntityTypes = RockPage.GetContextEntityTypes();

                var contextObjects = new Dictionary<string, object>();
                foreach (var conextEntityType in contextEntityTypes)
                {
                    var contextObject = RockPage.GetCurrentContext(conextEntityType);
                    contextObjects.Add(conextEntityType.FriendlyName, contextObject);
                }

                mergeFields.Add("Context", contextObjects);

                lContent.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning'>No event item is configured for this block.</div>";
            }
        }

        #endregion
    }
}