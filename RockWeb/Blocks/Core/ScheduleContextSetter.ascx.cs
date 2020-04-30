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
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Schedule Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default schedule context for the site." )]

    [CustomRadioListField( "Context Scope",
        Description = "The scope of context to set",
        ListSource = "Site,Page",
        IsRequired = true,
        DefaultValue = "Site",
        Order = 0,
        Key = AttributeKey.ContextScope )]

    [SchedulesField( "Schedule Group",
        Description = "Choose a schedule group to populate the dropdown",
        Order = 1,
        Key = AttributeKey.ScheduleGroup )]

    [TextField( "Current Item Template",
        Description = "Lava template for the current item. The only merge field is {{ ScheduleName }}.",
        IsRequired = true,
        DefaultValue = "{{ ScheduleName }}",
        Order = 2,
        Key = AttributeKey.CurrentItemTemplate )]

    [TextField( "Dropdown Item Template",
        Description = "Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.",
        IsRequired = true,
        DefaultValue = "{{ ScheduleName }}",
        Order = 2,
        Key = AttributeKey.DropdownItemTemplate )]

    [TextField( "No Schedule Text",
        Description = "The text to show when there is no schedule in the context.",
        IsRequired = true,
        DefaultValue = "Select Schedule",
        Order = 3,
        Key = AttributeKey.NoScheduleText )]

    [TextField( "Clear Selection Text",
        Description = "The text displayed when a schedule can be unselected. This will not display when the text is empty.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.ClearSelectionText )]

    [BooleanField( "Display Query Strings",
        Description = "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.DisplayQueryStrings )]

    public partial class ScheduleContextSetter : Rock.Web.UI.RockBlock
    {
        public static class AttributeKey
        {
            public const string ContextScope = "ContextScope";
            public const string ScheduleGroup = "ScheduleGroup";
            public const string CurrentItemTemplate = "CurrentItemTemplate";
            public const string DropdownItemTemplate = "DropdownItemTemplate";
            public const string NoScheduleText = "NoScheduleText";
            public const string ClearSelectionText = "ClearSelectionText";
            public const string DisplayQueryStrings = "DisplayQueryStrings";
        }

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

            LoadDropdowns();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropdowns();
        }

        /// <summary>
        /// Loads the schedules
        /// </summary>
        private void LoadDropdowns()
        {
            var scheduleEntityType = EntityTypeCache.Get( typeof( Schedule ) );
            var currentSchedule = RockPage.GetCurrentContext( scheduleEntityType ) as Schedule;

            var scheduleIdString = Request.QueryString["ScheduleId"];
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
                lCurrentSelection.Text = GetAttributeValue( AttributeKey.CurrentItemTemplate ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( AttributeKey.NoScheduleText );
            }

            var schedules = new List<ScheduleItem>();

            if ( GetAttributeValue( AttributeKey.ScheduleGroup ) != null )
            {
                var selectedSchedule = GetAttributeValue( AttributeKey.ScheduleGroup );
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
                schedule.Name = GetAttributeValue( AttributeKey.DropdownItemTemplate ).ResolveMergeFields( mergeObjects );
            }

            // check if the schedule can be unselected
            if ( !string.IsNullOrEmpty( GetAttributeValue( AttributeKey.ClearSelectionText ) ) )
            {
                var blankCampus = new ScheduleItem
                {
                    Name = GetAttributeValue( AttributeKey.ClearSelectionText ),
                    Id = Rock.Constants.All.Id
                };

                schedules.Insert( 0, blankCampus );
            }

            rptSchedules.DataSource = schedules;
            rptSchedules.DataBind();
        }

        /// <summary>
        /// Sets the schedule context.
        /// </summary>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Schedule SetScheduleContext( int scheduleId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( AttributeKey.ContextScope ) == "Page";
            var schedule = new ScheduleService( new RockContext() ).Get( scheduleId );
            if ( schedule == null )
            {
                // clear the current schedule context
                schedule = new Schedule()
                {
                    Name = GetAttributeValue( AttributeKey.NoScheduleText ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( schedule, pageScope, false );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "ScheduleId" ) ) || GetAttributeValue( AttributeKey.DisplayQueryStrings ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "ScheduleId", scheduleId.ToString() );
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

        #region Methods

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