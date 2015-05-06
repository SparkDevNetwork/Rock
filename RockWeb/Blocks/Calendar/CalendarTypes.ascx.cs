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

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Calendar
{
    /// <summary>
    /// Block to display the evnet calendars that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "Calendar Types" )]
    [Category( "Calendar" )]
    [Description( "Block to display the calendar types." )]

    [LinkedPage( "Detail Page", "Page used to view status of an event calendar." )]
    public partial class CalendarTypes : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptEventCalendarTypes.ItemCommand += rptEventCalendarTypes_ItemCommand;
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
                GetData();
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
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptEventCalendarTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptEventCalendarTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? eventCalendarId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( eventCalendarId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "CalendarTypeId", eventCalendarId.Value );
            }

            GetData();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            var rockContext = new RockContext();

            int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

            // Get all of the event calendars
            var allEventCalendars = new EventCalendarService( rockContext ).Queryable()
                .OrderBy( w => w.Name )
                .ToList();

            var displayedTypes = new List<EventCalendar>();
            foreach ( var eventCalendar in allEventCalendars )
            {
                displayedTypes.Add( eventCalendar );
            }

            // Create a query to return workflow type, the count of active action forms, and the selected class
            var qry = displayedTypes
                .Select( w => new
                {
                    EventCalendar = w
                } );

            rptEventCalendarTypes.DataSource = qry.ToList();
            rptEventCalendarTypes.DataBind();
        }
        #endregion

    }

}