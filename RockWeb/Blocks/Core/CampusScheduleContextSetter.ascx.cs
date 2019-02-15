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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site or page
    /// </summary>
    [DisplayName( "Campus Schedule Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default campus context and/or schedule for the site or page." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    [TextField( "Campus Current Item Template", "Lava template for the current item. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 1 )]
    [TextField( "Campus Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 2 )]
    [TextField( "No Campus Text", "The text displayed when no campus context is selected.", false, "Select Campus", order: 3 )]
    [TextField( "Campus Clear Selection Text", "The text displayed when a campus can be unselected. This will not display when the text is empty.", true, "All Campuses", order: 4 )]

    [SchedulesField( "Schedule Group", "Choose a schedule group to populate the dropdown", order: 5 )]
    [TextField( "Schedule Current Item Template", "Lava template for the current item. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 6 )]
    [TextField( "Schedule Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 7 )]
    [TextField( "No Schedule Text", "The text to show when there is no schedule in the context.", true, "All Schedules", order: 8 )]
    [TextField( "Schedule Clear Selection Text", "The text displayed when a schedule can be unselected. This will not display when the text is empty.", false, "All Schedules", order: 9 )]


    [BooleanField( "Display Query Strings", "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", false, "", order: 10 )]
    [BooleanField("Default To Current User's Campus", "Will use the campus of the current user if no context is provided.", key:"DefaultToCurrentUser", order:11)]
    public partial class CampusScheduleContextSetter : RockBlock
    {
        #region Fields
        string _currentCampusText = string.Empty;
        string _currentScheduleText = string.Empty;
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // repaint the screen after block settings are updated
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

            LoadCampusDropdowns();
            LoadScheduleDropdowns();
            
            lCurrentSelections.Text = string.Format( "{0} <br/> {1}", _currentCampusText, _currentScheduleText );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadCampusDropdowns();
            LoadScheduleDropdowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the campuses
        /// </summary>
        protected void LoadCampusDropdowns()
        {
            var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
            var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            var campusIdString = Request.QueryString["campusId"];
            if ( campusIdString != null )
            {
                var campusId = campusIdString.AsInteger();

                // if there is a query parameter, ensure that the Campus Context cookie is set (and has an updated expiration)
                // note, the Campus Context might already match due to the query parameter, but has a different cookie context, so we still need to ensure the cookie context is updated
                currentCampus = SetCampusContext( campusId, false );
            }

            if ( currentCampus == null && GetAttributeValue( "DefaultToCurrentUser" ).AsBoolean() && CurrentPerson != null )
            {
                currentCampus = CurrentPerson.GetFamily().Campus;
            }

            if ( currentCampus != null )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "CampusName", currentCampus.Name );
                lCurrentCampusSelection.Text = GetAttributeValue( "CampusCurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
                _currentCampusText = GetAttributeValue( "CampusCurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentCampusSelection.Text = GetAttributeValue( "NoCampusText" );
                _currentCampusText = GetAttributeValue( "NoCampusText" );
            }

            var campusList = CampusCache.All()
                .Select( a => new CampusItem { Name = a.Name, Id = a.Id } )
                .ToList();

            // run lava on each campus
            string dropdownItemTemplate = GetAttributeValue( "CampusDropdownItemTemplate" );
            if ( !string.IsNullOrWhiteSpace( dropdownItemTemplate ) )
            {
                foreach ( var campus in campusList )
                {
                    var mergeObjects = new Dictionary<string, object>();
                    mergeObjects.Add( "CampusName", campus.Name );
                    campus.Name = dropdownItemTemplate.ResolveMergeFields( mergeObjects );
                }
            }

            // check if the campus can be unselected
            if ( !string.IsNullOrEmpty( GetAttributeValue( "CampusClearSelectionText" ) ) )
            {
                var blankCampus = new CampusItem
                {
                    Name = GetAttributeValue( "CampusClearSelectionText" ),
                    Id = Rock.Constants.All.Id
                };

                campusList.Insert( 0, blankCampus );
            }

            rptCampuses.DataSource = campusList;
            rptCampuses.DataBind();
        }

        /// <summary>
        /// Loads the schedules
        /// </summary>
        private void LoadScheduleDropdowns()
        {
            var scheduleEntityType = EntityTypeCache.Get( typeof( Schedule ) );
            var currentSchedule = RockPage.GetCurrentContext( scheduleEntityType ) as Schedule;

            var scheduleIdString = Request.QueryString["scheduleId"];
            if ( scheduleIdString != null )
            {
                var scheduleId = scheduleIdString.AsInteger();

                // if there is a query parameter, ensure that the Schedule Context cookie is set (and has an updated expiration)
                // note, the Schedule Context might already match due to the query parameter, but has a different cookie context, so we still need to ensure the cookie context is updated
                currentSchedule = SetScheduleContext( scheduleId, false );
            }

            if ( currentSchedule != null )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "ScheduleName", currentSchedule.Name );
                lCurrentScheduleSelection.Text = GetAttributeValue( "ScheduleCurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
                _currentScheduleText = GetAttributeValue( "ScheduleCurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentScheduleSelection.Text = GetAttributeValue( "NoScheduleText" );
                _currentScheduleText = GetAttributeValue( "NoScheduleText" );
            }

            var schedules = new List<ScheduleItem>();

            if ( GetAttributeValue( "ScheduleGroup" ) != null )
            {
                var selectedSchedule = GetAttributeValue( "ScheduleGroup" );
                var selectedScheduleList = selectedSchedule.Split( ',' ).AsGuidList();

                schedules.AddRange( new ScheduleService( new RockContext() ).Queryable()
                    .Where( a => selectedScheduleList.Contains( a.Guid ) )
                    .Select( a => new ScheduleItem { Name = a.Name, Id = a.Id } )
                    .OrderBy( s => s.Name )
                    .ToList()
                );
            }

            var formattedSchedule = new Dictionary<int, string>();
            // run lava on each campus
            foreach ( var schedule in schedules )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Clear();
                mergeObjects.Add( "ScheduleName", schedule.Name );
                schedule.Name = GetAttributeValue( "ScheduleDropdownItemTemplate" ).ResolveMergeFields( mergeObjects );
            }

            // check if the schedule can be unselected
            if ( !string.IsNullOrEmpty( GetAttributeValue( "ScheduleClearSelectionText" ) ) )
            {
                var blankCampus = new ScheduleItem
                {
                    Name = GetAttributeValue( "ScheduleClearSelectionText" ),
                    Id = Rock.Constants.All.Id
                };

                schedules.Insert( 0, blankCampus );
            }

            rptSchedules.DataSource = schedules;
            rptSchedules.DataBind();
        }

        /// <summary>
        /// Sets the campus context.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Campus SetCampusContext( int campusId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var campus = new CampusService( new RockContext() ).Get( campusId );
            if ( campus == null )
            {
                // clear the current campus context
                campus = new Campus()
                {
                    Name = GetAttributeValue( "NoCampusText" ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( campus, pageScope, false );

            // Only redirect if refreshPage is true
            if ( refreshPage )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "campusId" ) ) || GetAttributeValue( "DisplayQueryStrings" ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "campusId", campusId.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return campus;
        }

        /// <summary>
        /// Sets the schedule context.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Schedule SetScheduleContext( int scheduleId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var schedule = new ScheduleService( new RockContext() ).Get( scheduleId );
            if ( schedule == null )
            {
                // clear the current schedule context
                schedule = new Schedule()
                {
                    Name = GetAttributeValue( "NoScheduleText" ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( schedule, pageScope, false );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "scheduleId" ) ) || GetAttributeValue( "DisplayQueryStrings" ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "scheduleId", scheduleId.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return schedule;
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var campusId = e.CommandArgument.ToString();

            if ( campusId != null )
            {
                SetCampusContext( campusId.AsInteger(), true );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSchedules_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var scheduleId = e.CommandArgument.ToString();

            if ( scheduleId != null )
            {
                SetScheduleContext( scheduleId.AsInteger(), true );
            }
        }

        #endregion

        /// <summary>
        /// Campus Item
        /// </summary>
        public class CampusItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }

        /// <summary>
        /// Schedule Item
        /// </summary>
        public class ScheduleItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }
    }
}