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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.cc_newspring.Blocks.ScheduleContextSetter
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Schedule Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default schedule context for the site." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    [SchedulesField( "Schedule Group", "Choose a schedule group to populate the dropdown", order: 1 )]
    [TextField( "Current Item Template", "Lava template for the current item. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 2 )]
    [TextField( "Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.", true, "{{ ScheduleName }}", order: 2 )]
    [TextField( "No Schedule Text", "The text to show when there is no schedule in the context.", true, "Select Schedule", order: 3 )]
    [TextField( "Clear Selection Text", "The text displayed when a schedule can be unselected. This will not display when the text is empty.", true, "", order: 4 )]
    [BooleanField( "Display Query Strings", "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", false, "", order: 5 )]
    public partial class ScheduleContextSetter : Rock.Web.UI.RockBlock
    {
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
            var scheduleEntityType = EntityTypeCache.Read( typeof( Schedule ) );
            var currentSchedule = RockPage.GetCurrentContext( scheduleEntityType ) as Schedule;

            var scheduleIdString = Request.QueryString["scheduleId"];
            if ( scheduleIdString != null )
            {
                var scheduleId = scheduleIdString.AsInteger();

                if ( currentSchedule == null || currentSchedule.Id != scheduleId )
                {
                    currentSchedule = SetScheduleContext( scheduleId, false );
                }
            }

            if ( currentSchedule != null )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "ScheduleName", currentSchedule.Name );
                lCurrentSelection.Text = GetAttributeValue( "CurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( "NoScheduleText" );
            }

            var schedules = new List<ScheduleItem>();

            if ( GetAttributeValue( "ScheduleGroup" ) != null )
            {
                var selectedSchedule = GetAttributeValue( "ScheduleGroup" );
                var selectedScheduleList = selectedSchedule.Split( ',' ).AsGuidList();

                schedules.AddRange( new ScheduleService( new RockContext() ).Queryable()
                    .Where( a => selectedScheduleList.Contains( a.Guid ) )
                    .Select( a => new ScheduleItem { Name = a.Name, Id = a.Id } )
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
                schedule.Name = GetAttributeValue( "DropdownItemTemplate" ).ResolveMergeFields( mergeObjects );
            }

            // check if the schedule can be unselected
            if ( !string.IsNullOrEmpty( GetAttributeValue( "ClearSelectionText" ) ) )
            {
                var blankCampus = new ScheduleItem
                {
                    Name = GetAttributeValue( "ClearSelectionText" ),
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