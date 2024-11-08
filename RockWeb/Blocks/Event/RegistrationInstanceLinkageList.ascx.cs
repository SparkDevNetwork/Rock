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
    /// A Block that displays the linkages related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Linkage List" )]
    [Category( "Event" )]
    [Description( "Displays the linkages associated with an event registration instance." )]

    #region Block Attributes

    [LinkedPage(
        "Linkage Page",
        "The page for viewing details about a registration linkage",
        Key = AttributeKey.LinkageDetailPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_LINKAGE,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Group Detail Page",
        "The page for viewing details about a group",
        Key = AttributeKey.GroupDetailPage,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Calendar Item Page",
        "The page to view calendar item details",
        Key = AttributeKey.CalendarItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAIL,
        IsRequired = false,
        Order = 3 )]

    [LinkedPage( "Content Item Page",
        "The page for viewing details about a content channel item",
        Key = AttributeKey.ContentItemDetailPage,
        DefaultValue = Rock.SystemGuid.Page.CONTENT_DETAIL,
        IsRequired = false,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "E877FDE1-DEE6-48F8-8150-4E28D5ABB694" )]
    public partial class RegistrationInstanceLinkageList : RegistrationInstanceBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The page for editing the linkage details.
            /// </summary>
            public const string LinkageDetailPage = "LinkagePage";

            /// <summary>
            /// The page for editing the Group associated with a linkage.
            /// </summary>
            public const string GroupDetailPage = "GroupDetailPage";

            /// <summary>
            /// The page for editing the Calendar Item associated with a linkage.
            /// </summary>
            public const string CalendarItemDetailPage = "CalendarItemDetailPage";

            /// <summary>
            /// The page for editing the Content Channel Item associated with a linkage.
            /// </summary>
            public const string ContentItemDetailPage = "ContentItemDetailPage";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SetUserPreferencePrefix( RegistrationTemplateId.Value );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fLinkages.ApplyFilterClick += fLinkages_ApplyFilterClick;

            gLinkages.EmptyDataText = "No Linkages Found";
            gLinkages.DataKeyNames = new string[] { "Id" };
            gLinkages.Actions.ShowAdd = true;
            gLinkages.Actions.AddClick += gLinkages_AddClick;
            gLinkages.RowDataBound += gLinkages_RowDataBound;
            gLinkages.GridRebind += gLinkages_GridRebind;
            this.BlockUpdated += RegistrationInstanceLinkageList_BlockUpdated;

            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the RegistrationInstanceLinkageList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RegistrationInstanceLinkageList_BlockUpdated( object sender, EventArgs e )
        {
            BindLinkagesGrid();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }

            base.OnLoad( e );
        }


        #endregion

        #region Events

        #region Linkage Tab Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fLinkages_ApplyFilterClick( object sender, EventArgs e )
        {
            fLinkages.SetFilterPreference( UserPreferenceKeyBase.GridFilter_Campus, cblCampus.SelectedValues.AsDelimited( ";" ) );

            BindLinkagesGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the fLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fLinkages_ClearFilterClick( object sender, EventArgs e )
        {
            fLinkages.DeleteFilterPreferences();
            BindLinkagesFilter();
        }

        /// <summary>
        /// Gets the display value for a filter field.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fLinkages_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Campus":
                    var values = new List<string>();
                    foreach ( string value in e.Value.Split( ';' ) )
                    {
                        var item = cblCampus.Items.FindByValue( value );
                        if ( item != null )
                        {
                            values.Add( item.Text );
                        }
                    }

                    e.Value = values.AsDelimited( ", " );
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLinkages_GridRebind( object sender, EventArgs e )
        {
            var registrationInstance = this.RegistrationInstance;

            if ( registrationInstance == null )
            {
                return;
            }

            gLinkages.ExportTitleName = registrationInstance.Name + " - Registration Linkages";
            gLinkages.ExportFilename = gLinkages.ExportFilename ?? registrationInstance.Name + "RegistrationLinkages";

            BindLinkagesGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var eventItemOccurrenceGroupMap = e.Row.DataItem as EventItemOccurrenceGroupMap;

                if ( eventItemOccurrenceGroupMap != null && eventItemOccurrenceGroupMap.EventItemOccurrence != null )
                {
                    if ( eventItemOccurrenceGroupMap.EventItemOccurrence.EventItem != null )
                    {
                        var lCalendarItem = e.Row.FindControl( "lCalendarItem" ) as Literal;

                        if ( lCalendarItem != null )
                        {
                            var calendarItems = new List<string>();

                            foreach ( var calendarItem in eventItemOccurrenceGroupMap.EventItemOccurrence.EventItem.EventCalendarItems )
                            {
                                if ( calendarItem.EventItem != null && calendarItem.EventCalendar != null )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "EventCalendarId", calendarItem.EventCalendarId.ToString() );
                                    qryParams.Add( "EventItemId", calendarItem.EventItem.Id.ToString() );

                                    var calendarEventUrl = LinkedPageUrl( AttributeKey.CalendarItemDetailPage, qryParams );

                                    if ( string.IsNullOrWhiteSpace( calendarEventUrl ) )
                                    {
                                        calendarItems.Add( string.Format( "{0} ({1})", calendarItem.EventItem.Name, eventItemOccurrenceGroupMap.EventItemOccurrence.Campus?.Name ?? "All Campuses" ) );
                                    }
                                    else
                                    {
                                        calendarItems.Add( string.Format( "<a href='{0}'>{1}</a> ({2})", calendarEventUrl, calendarItem.EventItem.Name, eventItemOccurrenceGroupMap.EventItemOccurrence.Campus?.Name ?? "All Campuses" ) );
                                    }
                                }
                            }

                            lCalendarItem.Text = calendarItems.AsDelimited( "<br/>" );
                        }

                        if ( eventItemOccurrenceGroupMap.EventItemOccurrence.ContentChannelItems.Any() )
                        {
                            var lContentItem = e.Row.FindControl( "lContentItem" ) as Literal;

                            if ( lContentItem != null )
                            {
                                var contentItems = new List<string>();

                                foreach ( var contentItem in eventItemOccurrenceGroupMap.EventItemOccurrence.ContentChannelItems
                                    .Where( c => c.ContentChannelItem != null )
                                    .Select( c => c.ContentChannelItem ) )
                                {
                                    var qryParams = new Dictionary<string, string>();
                                    qryParams.Add( "ContentItemId", contentItem.Id.ToString() );

                                    var contentItemUrl = LinkedPageUrl( AttributeKey.ContentItemDetailPage, qryParams );

                                    if ( string.IsNullOrWhiteSpace( contentItemUrl ) )
                                    {
                                        contentItems.Add( contentItem.Title );
                                    }
                                    else
                                    {
                                        contentItems.Add( string.Format( "<a href='{0}'>{1}</a>", contentItemUrl, contentItem.Title ) );
                                    }
                                }

                                lContentItem.Text = contentItems.AsDelimited( "<br/>" );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLinkages_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LinkageDetailPage, "LinkageId", 0, "RegistrationInstanceId", this.RegistrationInstanceId );
        }

        /// <summary>
        /// Handles the Edit event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.LinkageDetailPage, "LinkageId", e.RowKeyId, "RegistrationInstanceId", this.RegistrationInstanceId );
        }

        /// <summary>
        /// Handles the Delete event of the gLinkages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLinkages_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var campusEventItemService = new EventItemOccurrenceGroupMapService( rockContext );
                var campusEventItem = campusEventItemService.Get( e.RowKeyId );
                if ( campusEventItem != null )
                {
                    string errorMessage;
                    if ( !campusEventItemService.CanDelete( campusEventItem, out errorMessage ) )
                    {
                        mdLinkagesGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    campusEventItemService.Delete( campusEventItem );
                    rockContext.SaveChanges();
                }
            }

            BindLinkagesGrid();
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            SetUserPreferencePrefix( this.RegistrationTemplateId.GetValueOrDefault( 0 ) );

            BindLinkagesFilter();
            BindLinkagesGrid();
        }

        /// <summary>
        /// Sets the user preference prefix.
        /// </summary>
        private void SetUserPreferencePrefix( int registrationTemplateId )
        {
            fLinkages.PreferenceKeyPrefix = string.Format( "{0}-", registrationTemplateId );
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindLinkagesFilter()
        {
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();

            string campusValue = fLinkages.GetFilterPreference( UserPreferenceKeyBase.GridFilter_Campus );

            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindLinkagesGrid()
        {
            int? instanceId = this.RegistrationInstanceId;

            if ( !instanceId.HasValue )
            {
                return;
            }

            var groupCol = gLinkages.Columns[2] as HyperLinkField;

            var groupDetailUrl = LinkedPageUrl( AttributeKey.GroupDetailPage );

            if ( !string.IsNullOrWhiteSpace( groupDetailUrl ) )
            {
                groupCol.DataNavigateUrlFormatString = groupDetailUrl + "?GroupID={0}";
            }

            using ( var rockContext = new RockContext() )
            {
                var qry = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "EventItemOccurrence.EventItem.EventCalendarItems.EventCalendar,EventItemOccurrence.ContentChannelItems.ContentChannelItem,Group" )
                    .AsNoTracking()
                    .Where( r => r.RegistrationInstanceId == instanceId.Value );

                List<int> campusIds = cblCampus.SelectedValuesAsInt;
                if ( campusIds.Any() )
                {
                    qry = qry
                        .Where( l => l.CampusId.HasValue && campusIds.Contains( l.CampusId.Value ) );
                }

                IOrderedQueryable<EventItemOccurrenceGroupMap> orderedQry = null;
                SortProperty sortProperty = gLinkages.SortProperty;
                if ( sortProperty != null )
                {
                    orderedQry = qry.Sort( sortProperty );
                }
                else
                {
                    orderedQry = qry.OrderByDescending( r => r.CreatedDateTime );
                }

                gLinkages.SetLinqDataSource( orderedQry );
                gLinkages.DataBind();
            }
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visibility of the block.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }

        #endregion
    }
}