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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the calendars that user is authorized to view.
    /// </summary>
    [DisplayName( "Calendar Types" )]
    [Category( "Event" )]
    [Description( "Displays the calendars that user is authorized to view." )]
    [LinkedPage( "Detail Page", "Page used to view details of an event calendar." )]
    public partial class CalendarTypes : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbAddEventCalendar.Visible = UserCanAdministrate;

            rptEventCalendars.ItemCommand += rptEventCalendars_ItemCommand;

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
        /// Handles the Click event of the lbAddEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddEventCalendar_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "EventCalendarId", 0 );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptEventCalendars control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptEventCalendars_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? eventCalendarId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( eventCalendarId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "EventCalendarId", eventCalendarId.Value );
            }

            GetData();
        }

        #endregion

        #region Methods

        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                var allowedCalendars = new List<EventCalendar>();

                // Get all of the event calendars that user is authorized to view
                foreach ( var calendar in new EventCalendarService( rockContext ).Queryable()
                    .OrderBy( w => w.Name ))
                {
                    if ( calendar.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        allowedCalendars.Add( calendar );
                    }
                }

                rptEventCalendars.DataSource = allowedCalendars;
                rptEventCalendars.DataBind();
            }
        }

        #endregion

    }
}