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
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays icons to help with calendar administration navigation.
    /// </summary>
    [DisplayName( "Calendar Navigation" )]
    [Category( "Event" )]
    [Description( "Displays icons to help with calendar administration navigation." )]
    public partial class CalendarNavigation : RockBlock
    {

        #region Properties

        private int? EventCalendarId { get; set; }
        private int? EventItemId { get; set; }
        private int? EventItemOccurrenceId { get; set; }
        private int? ContentItemId { get; set; }

        private int PageNumber { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            EventCalendarId = ViewState["EventCalendarId"] as int?;
            EventItemId = ViewState["EventItemId"] as int?;
            EventItemOccurrenceId = ViewState["EventItemOccurrenceId"] as int?;
            ContentItemId = ViewState["ContentItemId"] as int?;

            PageNumber = ViewState["PageNumber"] as int? ?? 1;
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
                // Get any querystring variables
                ContentItemId = PageParameter( "ContentItemId" ).AsIntegerOrNull();
                EventItemOccurrenceId = PageParameter( "EventItemOccurrenceId" ).AsIntegerOrNull();
                EventItemId = PageParameter( "EventItemId" ).AsIntegerOrNull();
                EventCalendarId = PageParameter( "EventCalendarId" ).AsIntegerOrNull();

                // Determine current page number based on querystring values
                if ( ContentItemId.HasValue )
                {
                    PageNumber = 5;
                }
                else if ( EventItemOccurrenceId.HasValue )
                {
                    PageNumber = 4;
                }
                else if ( EventItemId.HasValue )
                {
                    PageNumber = 3;
                }
                else if ( EventCalendarId.HasValue )
                {
                    PageNumber = 2;
                }
                else
                {
                    PageNumber = 1;
                }

                // Load objects neccessary to display names
                using ( var rockContext = new RockContext() )
                {
                    ContentChannelItem contentItem = null;
                    EventItemOccurrence eventItemOccurrence = null;
                    EventItem eventItem = null;
                    EventCalendar eventCalendar = null;

                    if ( ContentItemId.HasValue && ContentItemId.Value > 0 )
                    {
                        var contentChannel = new ContentChannelItemService( rockContext ).Get( ContentItemId.Value );
                    }

                    if ( EventItemOccurrenceId.HasValue && EventItemOccurrenceId.Value > 0 )
                    {
                        eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( EventItemOccurrenceId.Value );
                        if ( eventItemOccurrence != null )
                        {
                            eventItem = eventItemOccurrence.EventItem;
                        }
                    }

                    if ( eventItem == null && EventItemId.HasValue && EventItemId.Value > 0 )
                    {
                        eventItem = new EventItemService( rockContext ).Get( EventItemId.Value );
                    }

                    if ( EventCalendarId.HasValue && EventCalendarId.Value > 0 )
                    {
                        eventCalendar = new EventCalendarService( rockContext ).Get( EventCalendarId.Value );
                    }

                    // Set the names based on current object values
                    lCalendarName.Text = eventCalendar != null ? eventCalendar.Name : "Calendar";
                    lCalendarItemName.Text = eventItem != null ? eventItem.Name : "Event";
                    lEventOccurrenceName.Text = eventItemOccurrence != null ?
                        ( eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses" ) :
                        "Event Occurrence";
                    lContentItemName.Text = contentItem != null ? contentItem.Title : "Content Item";
                }
            }

            divCalendars.Attributes["class"] = GetDivClass( 1 );
            divCalendar.Attributes["class"] = GetDivClass( 2 );
            divCalendarItem.Attributes["class"] = GetDivClass( 3 );
            divEventOccurrence.Attributes["class"] = GetDivClass( 4 );
            divContentItem.Attributes["class"] = GetDivClass( 5 );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["EventCalendarId"] = EventCalendarId;
            ViewState["EventItemId"] = EventItemId;
            ViewState["EventItemOccurrenceId"] = EventItemOccurrenceId;
            ViewState["ContentItemId"] = ContentItemId;

            ViewState["PageNumber"] = PageNumber;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbCalendars control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCalendars_Click( object sender, EventArgs e )
        {
            NavigateToParent(1);
        }

        /// <summary>
        /// Handles the Click event of the lbCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCalendar_Click( object sender, EventArgs e )
        {
            NavigateToParent(2);
        }

        /// <summary>
        /// Handles the Click event of the lbCalendarItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarItem_Click( object sender, EventArgs e )
        {
            NavigateToParent(3);
        }

        /// <summary>
        /// Handles the Click event of the lbEventOccurrence control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEventOccurrence_Click( object sender, EventArgs e )
        {
            NavigateToParent(4);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the div class.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        private string GetDivClass( int pageId )
        {
            return string.Format( "wizard-item{0}{1}", 
                pageId == PageNumber ? " active" : "", 
                pageId < PageNumber ? " complete" : "" );
        }

        /// <summary>
        /// Navigates to parent.
        /// </summary>
        /// <param name="targetPage">The target page.</param>
        private void NavigateToParent( int targetPage )
        {
            if ( PageNumber > targetPage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                if ( pageCache != null )
                {
                    // Build the querystring parameters
                    var qryParams = new Dictionary<string, string>();
                    if ( targetPage >= 2 && EventCalendarId.HasValue )
                    {
                        qryParams.Add( "EventCalendarId", EventCalendarId.Value.ToString() );
                    }
                    if ( targetPage >= 3 && EventItemId.HasValue )
                    {
                        qryParams.Add( "EventItemId", EventItemId.Value.ToString() );
                    }
                    if ( targetPage >= 4 && EventItemOccurrenceId.HasValue )
                    {
                        qryParams.Add( "EventItemOccurrenceId", EventItemOccurrenceId.Value.ToString() );
                    }

                    // Find the target page
                    var parentPage = pageCache.ParentPage;
                    int currentPage = PageNumber - 1;
                    while ( parentPage != null && currentPage > targetPage )
                    {
                        parentPage = parentPage.ParentPage;
                        currentPage--;
                    }

                    // Navigate to the parent page
                    if ( parentPage != null )
                    {
                        NavigateToPage( parentPage.Guid, qryParams );
                    }
                }
            }
        }

        #endregion

    }
}