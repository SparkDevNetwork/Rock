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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.RSVP
{
    /// <summary>
    /// Displays the details of the given RSVP occurrence.
    /// </summary>
    [DisplayName( "RSVP Detail" )]
    [Category( "RSVP" )]
    [Description( "Shows detailed RSVP information for a specific occurrence and allows editing RSVP details." )]

    [DefinedTypeField(
        "Decline Reasons Type",
        Key = AttributeKey.DeclineReasonsType,
        DefaultValue = Rock.SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.RSVP_DETAIL )]
    public partial class RSVPDetail : RockBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DeclineReasonsType = "DeclineReasonsType";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string OccurrenceId = "OccurrenceId";
            public const string OccurrenceDate = "OccurrenceDate";
        }

        private static class PageLabels
        {
            public const string MemberLocationTabTitle = "Member Location";
            public const string OtherLocationTabTitle = "Other Location";
        }

        private static class UserPreferenceKey
        {
            public const string Status = "Status";
            public const string DeclineReason = "DeclineReason";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tracks the currently active "Location Type" tab (Member Location or Other Location).
        /// </summary>
        private string LocationTypeTab { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            GetAvailableDeclineReasons();

            rFilter.ClearFilterClick += rFilter_ClearFilterClick;
            gAttendees.GridRebind += gAttendees_GridRebind;

            if ( !Page.IsPostBack )
            {
                rcblAvailableDeclineReasons.DataSource = _allDeclineReasons;
                rcblAvailableDeclineReasons.DataBind();
            }
        }

        /// <summary>
        /// Refreshes the block display in case the block settings are changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var newLocationId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            if ( newLocationId != null )
            {
                // If a new location was created, pass the value to the page to show it after reloading.
                NavigateToCurrentPageReference( new Dictionary<string, string> { { PageParameterKey.OccurrenceId, newLocationId.Value.ToString() } } );
                return;
            }
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( !Page.IsPostBack )
            {
                if ( groupId == null )
                {
                    NavigateToParentPage();
                }
                else
                {
                    var rockContext = new RockContext();
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    lHeading.Text = "RSVP Detail " + group.Name;

                    int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
                    if ( occurrenceId == null )
                    {
                        occurrenceId = 0;
                    }

                    // Display Occurrence
                    ShowDetails( rockContext, occurrenceId.Value, group );
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            LocationTypeTab = ViewState["LocationTypeTab"] as string ?? PageLabels.MemberLocationTabTitle;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["LocationTypeTab"] = LocationTypeTab;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                if ( SaveRSVPData() )
                {
                    NavigateToParentPage( new Dictionary<string, string> { { PageParameterKey.GroupId, groupId.Value.ToString() } } );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                NavigateToParentPage( new Dictionary<string, string> { { PageParameterKey.GroupId, groupId.Value.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gAttendees_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAttendeeGridAndChart( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees grid.
        /// </summary>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                bool isExporting = false;
                if ( e is RockGridViewRowEventArgs )
                {
                    isExporting = ( e as RockGridViewRowEventArgs ).IsExporting;
                }

                var rsvpData = ( RSVPAttendee ) e.Row.DataItem;
                if ( !isExporting )
                {

                    // Bind Decline Reason dropdown values.
                    RockDropDownList rddlDeclineReason = e.Row.FindControl( "rddlDeclineReason" ) as RockDropDownList;
                    rddlDeclineReason.DataSource = _availableDeclineReasons;
                    rddlDeclineReason.DataBind();

                    RockCheckBox rcbAccept = e.Row.FindControl( "rcbAccept" ) as RockCheckBox;
                    RockCheckBox rcbDecline = e.Row.FindControl( "rcbDecline" ) as RockCheckBox;
                    rcbAccept.InputAttributes.Add( "data-paired-checkbox", rcbDecline.ClientID );
                    rcbDecline.InputAttributes.Add( "data-paired-checkbox", rcbAccept.ClientID );

                    if ( rsvpData.DeclineReason.HasValue )
                    {
                        try
                        {
                            rddlDeclineReason.SelectedValue = rsvpData.DeclineReason.ToString();
                        }
                        catch
                        {
                            // This call may fail if the decline reason has been removed (from the DefinedType or from the individual occurrence).  Ignored.
                        }
                    }
                }
                else
                {
                    if ( rsvpData.DeclineReason.HasValue )
                    {
                        var lDeclineReason = e.Row.FindControl( "lDeclineReason" ) as Literal;
                        var declineReason = _availableDeclineReasons.FirstOrDefault( a => a.Id == rsvpData.DeclineReason.Value );
                        if ( declineReason != null )
                        {
                            lDeclineReason.Text = declineReason.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelOccurrence control.
        /// </summary>
        protected void lbCancelOccurrence_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbEditOccurrence control.
        /// </summary>
        protected void lbEditOccurrence_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbSaveOccurrence control.
        /// </summary>
        protected void lbSaveOccurrence_Click( object sender, EventArgs e )
        {
            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                // If the query string is 0, check to see if a new occurrence was already created.
                occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            }

            bool editSuccessful = false;
            if ( ( occurrenceId != null ) && ( occurrenceId != 0 ) )
            {
                editSuccessful = UpdateExistingOccurrence( occurrenceId.Value );
            }
            else
            {
                editSuccessful = CreateNewOccurrence();
            }

            if ( editSuccessful )
            {
                pnlEdit.Visible = false;
                pnlDetails.Visible = true;
                pnlAttendees.Visible = true;
            }
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.Status:
                    {
                        e.Value = ResolveValues( e.Value, cblStatus );
                    }
                    break;
            }
        }


        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( UserPreferenceKey.Status, cblStatus.SelectedValues.AsDelimited( ";" ) );
            BindAttendeeGridAndChart();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Stores the available Decline Reasons (filtered by the occurrence) for use in the grid drop down menus so they can be reused for binding multiple controls.
        /// </summary>
        private List<DefinedValue> _availableDeclineReasons;

        /// <summary>
        /// Gets the available Decline Reasons (filtered by the occurrence) for use in the grid drop down menus.
        /// </summary>
        /// <returns></returns>
        protected void GetAvailableDeclineReasons()
        {
            // If the collection is already initialized, this method is unnecessary.  Note that this means the collection property needs to be reset to null in order to refresh the values.
            if ( _availableDeclineReasons != null )
            {
                return;
            }

            GetAllDeclineReasons();
            List<DefinedValue> values = new List<DefinedValue>();
            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            }

            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                // No occurrence - just return an empty value for now.
                _availableDeclineReasons = values;
                _availableDeclineReasons.Insert( 0, new DefinedValue() { Id = 0, Value = "" } );
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId.Value );
                if ( occurrence.ShowDeclineReasons )
                {
                    if ( string.IsNullOrWhiteSpace( occurrence.DeclineReasonValueIds ) )
                    {
                        // if ShowDeclineReasons is true and no Decline Reasons were selected, show all available reasons.
                        foreach ( var value in _allDeclineReasons )
                        {
                            values.Add( value );
                        }
                    }
                    else
                    {
                        // Filter values by occurrence setting.
                        List<int> selectedDeclineReasons = occurrence.DeclineReasonValueIds.SplitDelimitedValues().Select( int.Parse ).ToList();
                        foreach ( DefinedValue value in _allDeclineReasons )
                        {
                            if ( selectedDeclineReasons.Contains( value.Id ) )
                            {
                                values.Add( value );
                            }
                        }
                    }
                }
            }

            _availableDeclineReasons = values;
            _availableDeclineReasons.Insert( 0, new DefinedValue() { Id = 0, Value = "" } );
        }

        /// <summary>
        /// Stores All Decline Reasons, for use in the occurrence edit panel.
        /// </summary>
        private List<DefinedValue> _allDeclineReasons;

        /// <summary>
        /// Gets all of the the Decline Reasons, for use in the occurrence edit panel
        /// </summary>
        /// <returns></returns>
        protected void GetAllDeclineReasons()
        {
            // If the collection is already initialized, this method is unnecessary.
            if ( _allDeclineReasons != null )
            {
                return;
            }

            List<DefinedValue> values = new List<DefinedValue>();

            var declineReasonsDefinedType = DefinedTypeCache.Get( GetAttributeValue( AttributeKey.DeclineReasonsType ) );
            if ( declineReasonsDefinedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    values = new DefinedValueService( rockContext ).Queryable()
                        .Where( v => v.DefinedTypeId == declineReasonsDefinedType.Id )
                        .Where( v => v.IsActive )
                        .AsNoTracking().ToList();
                }
            }

            _allDeclineReasons = values;
        }

        /// <summary>
        /// Display the RSVP etails of a specified occurrence.
        /// </summary>
        /// <param name="rockContext">The DbContext.</param>
        /// <param name="occurrenceId">The ID of the occurrence to display.</param>
        /// <param name="group">The group the occurrence belongs to.</param>
        private void ShowDetails( RockContext rockContext, int occurrenceId, Group group )
        {
            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            if ( occurrenceId == 0 )
            {
                ShowNewOccurrence( rockContext, group );
            }
            else
            {
                ShowExistingOccurrence( rockContext, occurrenceId, group );
            }
        }

        /// <summary>
        /// Displays the edit form for a new occurrence.
        /// </summary>
        private void ShowNewOccurrence( RockContext rockContext, Group group )
        {
            pnlEdit.Visible = true;
            pnlDetails.Visible = false;
            pnlAttendees.Visible = false;

            GetPreviousOccurrenceDetails( rockContext, group );

            string occurrenceDate = PageParameter( PageParameterKey.OccurrenceDate );
            if ( !string.IsNullOrWhiteSpace( occurrenceDate ) )
            {
                dpOccurrenceDate.SelectedDate = occurrenceDate.AsDateTime();
            }
        }

        private void GetPreviousOccurrenceDetails( RockContext rockContext, Group group )
        {
            var occurrence = new AttendanceOccurrenceService( rockContext ).Queryable().AsNoTracking()
                .Where( o => o.GroupId == group.Id )
                .OrderByDescending( o => o.Id ).FirstOrDefault();

            if ( occurrence != null )
            {
                heAcceptMessage.Text = occurrence.AcceptConfirmationMessage;
                heDeclineMessage.Text = occurrence.DeclineConfirmationMessage;

                rcbShowDeclineReasons.Checked = occurrence.ShowDeclineReasons;
                List<int> selectedDeclineReasons = occurrence.DeclineReasonValueIds.SplitDelimitedValues().Select( int.Parse ).ToList();
                foreach ( int declineReasonId in selectedDeclineReasons )
                {
                    foreach ( ListItem liItem in rcblAvailableDeclineReasons.Items )
                    {
                        if ( liItem.Value == declineReasonId.ToString() )
                        {
                            liItem.Selected = true;
                        }
                    }
                }

                if ( occurrence.LocationId.HasValue )
                {
                    var location = occurrence.Location;
                    if ( location == null )
                    {
                        location = new LocationService( rockContext ).Get( occurrence.LocationId.Value );
                    }
                    lLocation.Visible = true;
                    lLocation.Text = location.ToString();
                    locpLocation.Location = location;
                }
                else
                {
                    lLocation.Visible = false;
                    lLocation.Text = string.Empty;
                    locpLocation.Location = null;
                }

                if ( occurrence.ScheduleId.HasValue && occurrence.Schedule == null )
                {
                    occurrence.Schedule = new ScheduleService( rockContext ).GetNoTracking( occurrence.ScheduleId.Value );
                }

                if ( occurrence.Schedule == null )
                {
                    lSchedule.Visible = false;
                    lSchedule.Text = string.Empty;
                    lScheduleText.Text = string.Empty;
                    spSchedule.SetValue( null );
                }
                else
                {
                    lSchedule.Visible = true;
                    lSchedule.Text = occurrence.Schedule.FriendlyScheduleText;
                    lScheduleText.Text = occurrence.Schedule.FriendlyScheduleText;
                    spSchedule.SetValue( occurrence.Schedule );
                }

            }
        }

        /// <summary>
        /// Displays the detail form for an existing occurrence.
        /// </summary>
        /// <param name="rockContext">The DbContext.</param>
        /// <param name="occurrenceId">The ID of the occurrence to display.</param>
        /// <param name="group">The group the occurrence belongs to.</param>
        private void ShowExistingOccurrence( RockContext rockContext, int occurrenceId, Group group )
        {
            pnlEdit.Visible = false;
            pnlDetails.Visible = true;
            pnlAttendees.Visible = true;

            BindFilter();

            var groupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            var occurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrenceId );
            lOccurrenceName.Text = occurrence.Name;
            tbOccurrenceName.Text = occurrence.Name;
            lOccurrenceDate.Text = occurrence.OccurrenceDate.ToShortDateString();
            dpOccurrenceDate.SelectedDate = occurrence.OccurrenceDate;
            heAcceptMessage.Text = occurrence.AcceptConfirmationMessage;
            heDeclineMessage.Text = occurrence.DeclineConfirmationMessage;

            rcbShowDeclineReasons.Checked = occurrence.ShowDeclineReasons;
            cblDeclineReason.Visible = occurrence.ShowDeclineReasons;

            List<int> selectedDeclineReasons = occurrence.DeclineReasonValueIds.SplitDelimitedValues().Select( int.Parse ).ToList();
            foreach ( int declineReasonId in selectedDeclineReasons )
            {
                foreach ( ListItem liItem in rcblAvailableDeclineReasons.Items )
                {
                    if ( liItem.Value == declineReasonId.ToString() )
                    {
                        liItem.Selected = true;
                    }
                }
            }

            if ( occurrence.LocationId.HasValue )
            {
                var location = occurrence.Location;
                if ( location == null )
                {
                    location = new LocationService( rockContext ).Get( occurrence.LocationId.Value );
                }
                lLocation.Visible = true;
                lLocation.Text = location.ToString();
                locpLocation.Location = location;
            }
            else
            {
                lLocation.Visible = false;
                lLocation.Text = string.Empty;
                locpLocation.Location = null;
            }

            if ( occurrence.ScheduleId.HasValue && occurrence.Schedule == null )
            {
                occurrence.Schedule = new ScheduleService( rockContext ).GetNoTracking( occurrence.ScheduleId.Value );
            }

            if ( occurrence.Schedule == null )
            {
                lSchedule.Visible = false;
                lSchedule.Text = string.Empty;
                lScheduleText.Text = string.Empty;
                spSchedule.SetValue( null );
            }
            else
            {
                lSchedule.Visible = true;
                lSchedule.Text = occurrence.Schedule.FriendlyScheduleText;
                lScheduleText.Text = occurrence.Schedule.FriendlyScheduleText;
                spSchedule.SetValue( occurrence.Schedule );
            }

            BindAttendeeGridAndChart();
        }

        /// <summary>
        /// Shows the edit panel for the occurrence.
        /// </summary>
        private void ShowEdit()
        {
            pnlEdit.Visible = true;
            pnlDetails.Visible = false;
            pnlAttendees.Visible = false;
        }

        /// <summary>
        /// Binds the grid and chart with attendee data.
        /// </summary>
        private void BindAttendeeGridAndChart( bool isExporting = false )
        {
            using ( var rockContext = new RockContext() )
            {
                int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
                if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                {
                    occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
                }

                if ( ( occurrenceId != null ) && ( occurrenceId != 0 ) )
                {
                    var occurrenceService = new AttendanceOccurrenceService( rockContext );
                    var occurrence = occurrenceService.Get( occurrenceId.Value );
                    gAttendees.ColumnsOfType<RockTemplateField>()
                        .First( c => c.HeaderText == "Decline Reason" )
                        .Visible = occurrence.ShowDeclineReasons;
                    gAttendees.ColumnsOfType<RockTemplateField>()
                        .First( c => c.HeaderText == "Decline Note" )
                        .Visible = occurrence.ShowDeclineReasons;
                }

                gAttendees.ColumnsOfType<BoolField>()
                    .First( c => c.HeaderText == "Accept" )
                    .Visible = isExporting;
                gAttendees.ColumnsOfType<RockLiteralField>()
                  .First( c => c.HeaderText == "Decline Reason" )
                  .Visible = isExporting;
                gAttendees.ColumnsOfType<BoolField>()
                    .First( c => c.HeaderText == "Decline" )
                    .Visible = isExporting;


                var attendees = GetAttendees( rockContext );
                int acceptCount = attendees.Where( a => a.Accept ).Count();
                int declineCount = attendees.Where( a => a.Decline ).Count();
                int noResponseCount = attendees.Count() - acceptCount - declineCount;
                RegisterDoughnutChartScript( acceptCount, declineCount, noResponseCount );

                _availableDeclineReasons = null;
                GetAvailableDeclineReasons();

                gAttendees.DataSource = attendees;
                gAttendees.DataBind();

                if ( attendees.Count() < 1 )
                {
                    lbSave.Visible = false;
                }
                else
                {
                    lbSave.Visible = true;
                }
            }
        }

        /// <summary>
        /// Gets the attendee data (for use in the grid and chart).
        /// </summary>
        /// <param name="rockContext">The RockContext</param>
        /// <returns>A list of <see cref="RSVPAttendee"/> objects representing the attendees of an occurrence.</returns>
        private List<RSVPAttendee> GetAttendees( RockContext rockContext )
        {
            List<RSVPAttendee> result = new List<RSVPAttendee>();
            List<int> existingAttendanceRecords = new List<int>();

            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId != null ) && ( occurrenceId != 0 ) )
            {
                // Add RSVP responses for anyone who has an attendance record, already.
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId.Value );
                var attendees = occurrence.Attendees.AsEnumerable();

                var rsvpStatus = new List<Rock.Model.RSVP>();

                foreach ( string selectedStatuses in cblStatus.SelectedValues )
                {
                    var status = selectedStatuses.ConvertToEnum<Statuses>();

                    switch ( status )
                    {
                        case Statuses.Accept:
                            rsvpStatus.Add( Rock.Model.RSVP.Yes );
                            break;

                        case Statuses.Decline:
                            rsvpStatus.Add( Rock.Model.RSVP.No );
                            break;

                        case Statuses.NoResponse:
                            rsvpStatus.Add( Rock.Model.RSVP.Unknown );
                            break;
                    }
                }

                if ( rsvpStatus.Any() )
                {
                    attendees = attendees.Where( a => rsvpStatus.Contains( a.RSVP ) );
                }

                var declineReasonValueIds = cblDeclineReason.SelectedValuesAsInt;
                if ( declineReasonValueIds.Any() )
                {
                    attendees = attendees.Where( a => a.DeclineReasonValueId.HasValue && declineReasonValueIds.Contains( a.DeclineReasonValueId.Value ) );
                }

                var sortedAttendees = attendees.OrderBy( a => a.PersonAlias.Person.LastName ).ThenBy( a => a.PersonAlias.Person.FirstName );
                foreach ( var attendee in sortedAttendees )
                {
                    RSVPAttendee rsvp = new RSVPAttendee();
                    rsvp.PersonId = attendee.PersonAlias.PersonId;
                    rsvp.NickName = attendee.PersonAlias.Person.NickName;
                    rsvp.LastName = attendee.PersonAlias.Person.LastName;
                    rsvp.Accept = ( attendee.RSVP == Rock.Model.RSVP.Yes );
                    rsvp.Decline = ( attendee.RSVP == Rock.Model.RSVP.No );
                    rsvp.DeclineReason = attendee.DeclineReasonValueId;
                    rsvp.DeclineNote = attendee.Note;
                    rsvp.RSVPDateTime = attendee.RSVPDateTime;
                    result.Add( rsvp );
                    existingAttendanceRecords.Add( attendee.PersonAlias.PersonId );
                }
            }

            return result;
        }

        /// <summary>
        /// Registers the doughnut chart Chart.js script.
        /// </summary>
        private void RegisterDoughnutChartScript( int AcceptCount, int DeclineCount, int NoResponseCount )
        {
            string colors = "['#16C98D','#D4442E','#F3F3F3']";
            string rsvpData = "['"
                + AcceptCount.ToString() + "', '"
                + DeclineCount.ToString() + "', '"
                + NoResponseCount.ToString() + "']";

            string script = string.Format(
@"
var dnutCtx = $('#{0}')[0].getContext('2d');

var dnutChart = new Chart(dnutCtx, {{
    type: 'doughnut',
    data: {{
        labels: ['Accept', 'Decline', 'No Response'],
        datasets: [{{
            type: 'doughnut',
            data: {1},
            backgroundColor: {2}
        }}]
    }},
    options: {{
        responsive: true,
        legend: {{
            position: 'right',
            fullWidth: true
        }},
        cutoutPercentage: 75,
        animation: {{
			animateScale: true,
			animateRotate: true
		}}
    }}
}});",
                doughnutChartCanvas.ClientID,
                rsvpData,
                colors );

            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "groupSchedulerDoughnutChartScript", script, true );
        }

        /// <summary>
        /// Save RSVP response data from grid (to Attendance records).
        /// </summary>
        protected bool SaveRSVPData()
        {
            var attendees = new List<RSVPAttendee>();

            foreach ( GridViewRow row in gAttendees.Rows )
            {
                if ( row.RowType == DataControlRowType.DataRow )
                {
                    RockCheckBox rcbAccept = row.FindControl( "rcbAccept" ) as RockCheckBox;
                    RockCheckBox rcbDecline = row.FindControl( "rcbDecline" ) as RockCheckBox;
                    DataDropDownList rddlDeclineReason = row.FindControl( "rddlDeclineReason" ) as DataDropDownList;
                    RockTextBox tbDeclineNote = row.FindControl( "tbDeclineNote" ) as RockTextBox;
                    int declineReason = int.Parse( rddlDeclineReason.SelectedValue );
                    string declineNote = tbDeclineNote.Text;

                    attendees.Add(
                        new RSVPAttendee()
                        {
                            Accept = rcbAccept.Checked,
                            Decline = rcbDecline.Checked,
                            DeclineNote = declineNote,
                            DeclineReason = declineReason,
                            PersonId = ( int ) gAttendees.DataKeys[row.RowIndex].Value
                        }
                    );
                }
            }
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                AttendanceOccurrence occurrence = null;

                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();

                if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                {
                    occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
                    if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                    {
                        throw new Exception( "The AttendanceOccurrence does not exist." );
                    }
                }

                occurrence = occurrenceService.Get( occurrenceId.Value );

                var existingAttendees = occurrence.Attendees.ToList();

                foreach ( var attendee in attendees )
                {
                    var attendance = existingAttendees
                        .Where( a => a.PersonAlias.PersonId == attendee.PersonId )
                        .FirstOrDefault();

                    if ( attendance == null )
                    {
                        int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonId );
                        if ( personAliasId.HasValue )
                        {
                            attendance = new Attendance();
                            attendance.PersonAliasId = personAliasId;
                            attendance.StartDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule() ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay ) : occurrence.OccurrenceDate;
                            occurrence.Attendees.Add( attendance );
                        }
                    }

                    if ( attendance != null )
                    {
                        if ( attendee.Accept )
                        {
                            var groupMember = occurrence.Group.Members.Where( gm => gm.PersonId == attendee.PersonId ).FirstOrDefault();
                            if ( groupMember == null )
                            {
                                groupMember = new GroupMember();
                                groupMember.PersonId = attendee.PersonId;
                                groupMember.GroupId = occurrence.Group.Id;
                                groupMember.GroupRoleId = occurrence.Group.GroupType.DefaultGroupRoleId ?? 0;

                                new GroupMemberService( rockContext ).Add( groupMember );
                                rockContext.SaveChanges();
                            }

                            // only set the RSVP and Date if the value is changing 
                            if ( attendance.RSVP != Rock.Model.RSVP.Yes )
                            {
                                attendance.RSVPDateTime = RockDateTime.Now;
                                attendance.RSVP = Rock.Model.RSVP.Yes;
                            }
                            attendance.Note = string.Empty;
                            attendance.DeclineReasonValueId = null;
                        }
                        else if ( attendee.Decline )
                        {
                            // only set the RSVP and Date if the value is changing 
                            if ( attendance.RSVP != Rock.Model.RSVP.No )
                            {
                                attendance.RSVPDateTime = RockDateTime.Now;
                                attendance.RSVP = Rock.Model.RSVP.No;
                            }

                            attendance.Note = attendee.DeclineNote;
                            if ( attendee.DeclineReason != 0 )
                            {
                                attendance.DeclineReasonValueId = attendee.DeclineReason;
                            }
                        }
                        else
                        {
                            attendance.RSVPDateTime = null;
                            attendance.RSVP = Rock.Model.RSVP.Unknown;
                            attendance.Note = string.Empty;
                            attendance.DeclineReasonValueId = null;
                        }
                    }
                }

                rockContext.SaveChanges();

                if ( occurrence.LocationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Remove( occurrence.LocationId.Value );
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new AttendanceOccurrence record.
        /// </summary>
        private bool CreateNewOccurrence()
        {
            using ( var rockContext = new RockContext() )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var group = new GroupService( rockContext ).Get( groupId.Value );

                //Create new occurrence.
                var occurrence = new AttendanceOccurrence();
                occurrence.Name = tbOccurrenceName.Text;
                occurrence.GroupId = groupId;

                if ( locpLocation.Location != null )
                {
                    occurrence.Location = new LocationService( rockContext ).Get( locpLocation.Location.Id );
                    occurrence.LocationId = occurrence.Location.Id;
                }
                else
                {
                    occurrence.Location = null;
                    occurrence.LocationId = null;
                }

                // The schedule is OK to be null
                occurrence.ScheduleId = spSchedule.SelectedValueAsId();

                if ( dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrence.OccurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                var occurrenceService = new AttendanceOccurrenceService( rockContext );

                // If this occurrence has already been created, just use the existing one.
                var existingOccurrences = occurrenceService.Queryable()
                    .Where( o => o.GroupId == occurrence.GroupId )
                    .Where( o => o.OccurrenceDate == occurrence.OccurrenceDate )
                    .Where( o => o.ScheduleId == occurrence.ScheduleId )
                    .Where( o => o.LocationId == occurrence.LocationId )
                    .ToList();

                if ( existingOccurrences.Any() )
                {
                    occurrence = existingOccurrences.FirstOrDefault();
                }
                else
                {
                    occurrenceService.Add( occurrence );
                }

                occurrence.DeclineConfirmationMessage = heDeclineMessage.Text;
                occurrence.AcceptConfirmationMessage = heAcceptMessage.Text;
                occurrence.ShowDeclineReasons = rcbShowDeclineReasons.Checked;

                var selectedDeclineReasons = new List<string>();
                foreach ( ListItem listItem in rcblAvailableDeclineReasons.Items )
                {
                    if ( listItem.Selected )
                    {
                        selectedDeclineReasons.Add( listItem.Value );
                    }
                }
                occurrence.DeclineReasonValueIds = selectedDeclineReasons.AsDelimited( "," );

                rockContext.SaveChanges();

                occurrence = occurrenceService.Get( occurrence.Id );

                hfNewOccurrenceId.Value = occurrence.Id.ToString();
                ShowDetails( rockContext, occurrence.Id, group );
                return true;
            }
        }

        /// <summary>
        /// Saves changes to an existing AttendanceOccurrence record.
        /// </summary>
        private bool UpdateExistingOccurrence( int occurrenceId )
        {
            using ( var rockContext = new RockContext() )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var group = new GroupService( rockContext ).Get( groupId.Value );

                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId );

                occurrence.Name = tbOccurrenceName.Text;

                if ( locpLocation.Location != null )
                {
                    occurrence.Location = new LocationService( rockContext ).Get( locpLocation.Location.Id );
                    occurrence.LocationId = occurrence.Location.Id;
                }
                else
                {
                    occurrence.Location = null;
                    occurrence.LocationId = null;
                }

                // The schedule is OK to be null
                occurrence.ScheduleId = spSchedule.SelectedValueAsId();

                if ( dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrence.OccurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                //If this occurrence has already been created, just use the existing one.
                var existingOccurrences = occurrenceService.Queryable()
                    .Where( o => o.GroupId == occurrence.GroupId )
                    .Where( o => o.OccurrenceDate == occurrence.OccurrenceDate )
                    .Where( o => o.ScheduleId == occurrence.ScheduleId )
                    .Where( o => o.LocationId == occurrence.LocationId )
                    .Where( o => o.Id != occurrence.Id )
                    .ToList();

                if ( existingOccurrences.Any() )
                {
                    nbEditConflict.Visible = true;
                    return false;
                }
                else
                {
                    nbEditConflict.Visible = false;
                    occurrence.DeclineConfirmationMessage = heDeclineMessage.Text;
                    occurrence.AcceptConfirmationMessage = heAcceptMessage.Text;
                    occurrence.ShowDeclineReasons = rcbShowDeclineReasons.Checked;

                    var selectedDeclineReasons = new List<string>();
                    foreach ( ListItem listItem in rcblAvailableDeclineReasons.Items )
                    {
                        if ( listItem.Selected )
                        {
                            selectedDeclineReasons.Add( listItem.Value );
                        }
                    }
                    occurrence.DeclineReasonValueIds = selectedDeclineReasons.AsDelimited( "," );

                    rockContext.SaveChanges();

                    ShowDetails( rockContext, occurrence.Id, group );
                    return true;
                }
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cblStatus.Items.Clear();

            foreach ( Statuses status in Enum.GetValues( typeof( Statuses ) ) )
            {
                cblStatus.Items.Add( new ListItem( status.ConvertToString().SplitCase(), status.ConvertToInt().ToString() ) );
            }

            cblDeclineReason.DataSource = _availableDeclineReasons.Where( a => a.Id != default( int ) ).ToList();
            cblDeclineReason.DataBind();

            string statusValue = rFilter.GetFilterPreference( UserPreferenceKey.Status );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        #endregion

        #region Helper Class

        public enum Statuses
        {
            Accept,
            Decline,
            NoResponse
        }

        [Serializable]
        public class RSVPAttendee
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get { return NickName + " " + LastName; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RSVPAttendee"/> has accepted.
            /// </summary>
            /// <value>
            ///   <c>true</c> if accepted; otherwise, <c>false</c>.
            /// </value>
            public bool Accept { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RSVPAttendee"/> has declined.
            /// </summary>
            /// <value>
            ///   <c>true</c> if declined; otherwise, <c>false</c>.
            /// </value>
            public bool Decline { get; set; }

            /// <summary>
            /// Gets or sets the decline reason (defined value Id).
            /// </summary>
            /// <value>
            /// The decline reason (defined value Id).
            /// </value>
            public int? DeclineReason { get; set; }

            /// <summary>
            /// Gets or sets the decline note.
            /// </summary>
            /// <value>
            /// The decline note.
            /// </value>
            public string DeclineNote { get; set; }

            /// <summary>
            /// Gets or sets the RSVP date time.
            /// </summary>
            /// <value>
            /// The RSVP date time.
            /// </value>
            public DateTime? RSVPDateTime { get; set; }
        }

        #endregion

    }
}