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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using static Rock.Reporting.ComparisonHelper;

namespace RockWeb.Blocks.Engagement.SignUp
{
    [DisplayName( "Sign-Up Overview" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Displays an overview of sign-up projects with upcoming and recently-occurred opportunities." )]

    #region Block Attributes

    [LinkedPage( "Project Detail Page",
        Key = AttributeKey.ProjectDetailPage,
        Description = "Page used for viewing details about the scheduled opportunities for a given project group. Clicking a row in the grid will take you to this page.",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Sign-Up Opportunity Attendee List Page",
        Key = AttributeKey.SignUpOpportunityAttendeeListPage,
        Description = "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.",
        IsRequired = false,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B" )]
    public partial class SignUpOverview : RockBlock, IPostBackEventHandler
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string ProjectDetailPage = "ProjectDetailPage";
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        private class ViewStateKey
        {
            public const string EntitySetItemEntityId = "EntitySetItemEntityId";
            public const string OpportunitiesState = "OpportunitiesState";
        }

        private static class GridFilterKey
        {
            public const string DateRange = "DateRange";
            public const string ParentGroup = "ParentGroup";
            public const string SlotsAvailable = "SlotsAvailable";
        }

        private static class DataKeyName
        {
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class GridAction
        {
            public const string EmailLeaders = "EMAIL_LEADERS";
            public const string EmailAll = "EMAIL_ALL";
            public const string None = "";
        }

        private static class PostbackEventArgument
        {
            public const string GridActionChanged = "GridActionChanged";
        }

        private static class MergeFieldKey
        {
            public const string Opportunities = "Opportunities";
        }

        private static class SortExpression
        {
            public const string ProjectName = "ProjectName";
            public const string NextOrLastStartDateTime = "NextOrLastStartDateTime";
            public const string LeaderCount = "LeaderCount";
            public const string ParticipantCount = "ParticipantCount";
        }

        #endregion

        #region Fields

        private RockDropDownList _ddlAction;
        private int _entitySetItemEntityId;

        #endregion

        #region Properties

        private GroupTypeCache SignUpGroupType
        {
            get
            {
                return GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP );
            }
        }


        private int SignUpGroupTypeId
        {
            get
            {
                return this.SignUpGroupType?.Id ?? 0;
            }
        }

        private List<Opportunity> OpportunitiesState { get; set; }

        #endregion

        #region Control Life-Cycle Events

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _entitySetItemEntityId = ViewState[ViewStateKey.EntitySetItemEntityId].ToIntSafe();

            var json = ViewState[ViewStateKey.OpportunitiesState] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                this.OpportunitiesState = new List<Opportunity>();
            }
            else
            {
                this.OpportunitiesState = JsonConvert.DeserializeObject<List<Opportunity>>( json ) ?? new List<Opportunity>();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            InitializeGrid();

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSignUpOverview );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                SetGridFilters();
                BindOpportunitiesGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument != PostbackEventArgument.GridActionChanged || hfAction.Value.IsNullOrWhiteSpace() )
            {
                return;
            }

            var selectedGuids = gOpportunities.SelectedKeys.Select( k => ( Guid ) k ).ToList();
            if ( selectedGuids.Any() )
            {
                var selectedOpportunities = this.OpportunitiesState
                    .Where( o => selectedGuids.Contains( o.Guid ) )
                    .ToList();

                var shouldOnlyEmailLeaders = hfAction.Value == GridAction.EmailLeaders;
                EmailParticipants( selectedOpportunities, shouldOnlyEmailLeaders );
            }

            _ddlAction.SelectedIndex = 0;
            hfAction.Value = string.Empty;
            gOpportunities.SelectedKeys.Clear();
            BindOpportunitiesGrid();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.EntitySetItemEntityId] = _entitySetItemEntityId;

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.OpportunitiesState] = JsonConvert.SerializeObject( this.OpportunitiesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Opportunities Grid Events

        /// <summary>
        /// Displays the gfOpportunities filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfOpportunities_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == GridFilterKey.DateRange )
            {
                e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
            }
            else if ( e.Key == GridFilterKey.ParentGroup )
            {
                int? groupId = e.Value.AsIntegerOrNull();
                e.Value = string.Empty;
                if ( groupId.HasValue )
                {
                    Group group;
                    using ( var rockContext = new RockContext() )
                    {
                        group = new GroupService( rockContext ).GetNoTracking( groupId.Value );
                    }

                    if ( group != null )
                    {
                        e.Value = group.ToString();
                    }
                }
            }
            else if ( e.Key == GridFilterKey.SlotsAvailable )
            {
                e.Value = NumberComparisonFilter.ValueAsFriendlyString( e.Value );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void gfOpportunities_ApplyFilterClick( object sender, System.EventArgs e )
        {
            gfOpportunities.SetFilterPreference( GridFilterKey.DateRange, "Date Range", sdrpDateRange.DelimitedValues );
            gfOpportunities.SetFilterPreference( GridFilterKey.ParentGroup, "Parent Group", gpParentGroup.SelectedValue );
            gfOpportunities.SetFilterPreference( GridFilterKey.SlotsAvailable, "Slots Available", NumberComparisonFilter.SelectedValueAsDelimited(
                ddlSlotsAvailableComparisonType,
                nbSlotsAvailableFilterCompareValue
            ) );

            BindOpportunitiesGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void gfOpportunities_ClearFilterClick( object sender, System.EventArgs e )
        {
            gfOpportunities.DeleteFilterPreferences();

            SetGridFilters();
        }

        /// <summary>
        /// Handles the TextChanged event of the ddlSlotsAvailableComparisonType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSlotsAvailableComparisonType_TextChanged( object sender, EventArgs e )
        {
            if ( ddlSlotsAvailableComparisonType.SelectedValue == None.IdValue )
            {
                nbSlotsAvailableFilterCompareValue.Text = default;
            }
        }

        /// <summary>
        /// Handles the DataBinding event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gOpportunities_DataBinding( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.SignUpOpportunityAttendeeListPage ) ) )
            {
                var linkButtonField = gOpportunities.ColumnsOfType<LinkButtonField>().FirstOrDefault( c => c.ID == "lbOpportunityDetail" );
                if ( linkButtonField != null )
                {
                    linkButtonField.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gOpportunities_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var opportunity = e.Row.DataItem as Opportunity;
            if ( opportunity == null )
            {
                return;
            }

            if ( !opportunity.CanDelete )
            {
                e.Row.AddCssClass( "js-cannot-delete" );
            }

            if ( opportunity.ParticipantCount > 0 )
            {
                e.Row.AddCssClass( "js-has-participants" );
            }

            if ( e.Row.FindControl( "lParticipantCountBadgeHtml" ) is Literal lParticipantCountBadgeHtml )
            {
                lParticipantCountBadgeHtml.Text = opportunity.ParticipantCountBadgeHtml;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gOpportunities_RowSelected( object sender, RowEventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ProjectDetailPage ) ) )
            {
                var keys = e.RowKeyValues;
                var qryParams = new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId,  keys[DataKeyName.GroupId].ToString() }
                };

                NavigateToLinkedPage( AttributeKey.ProjectDetailPage, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbOpportunityDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lbOpportunityDetail_Click( object sender, RowEventArgs e )
        {
            var keys = e.RowKeyValues;
            var qryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId,  keys[DataKeyName.GroupId].ToString() },
                { PageParameterKey.LocationId, keys[DataKeyName.LocationId].ToString() },
                { PageParameterKey.ScheduleId, keys[DataKeyName.ScheduleId].ToString() }
            };

            NavigateToLinkedPage( AttributeKey.SignUpOpportunityAttendeeListPage, qryParams );
        }

        /// <summary>
        /// Handles the GridRebind event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs" /> instance containing the event data.</param>
        protected void gOpportunities_GridRebind( object sender, GridRebindEventArgs e )
        {
            List<Opportunity> opportunities = null;

            if ( e.IsExporting )
            {
                opportunities = this.OpportunitiesState;

                var selectedGuids = gOpportunities.SelectedKeys.Select( k => ( Guid ) k ).ToList();
                if ( selectedGuids.Any() )
                {
                    opportunities = opportunities.Where( o => selectedGuids.Contains( o.Guid ) ).ToList();
                }
            }

            BindOpportunitiesGrid( opportunities );
        }

        /// <summary>
        /// Handles the Click event of the dfOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void dfOpportunities_Click( object sender, RowEventArgs e )
        {
            nbNotAuthorizedToDelete.Visible = false;

            var groupId = e.RowKeyValues[DataKeyName.GroupId].ToIntSafe();
            var locationId = e.RowKeyValues[DataKeyName.LocationId].ToIntSafe();
            var scheduleId = e.RowKeyValues[DataKeyName.ScheduleId].ToIntSafe();

            // We should consider moving this logic to a service (probably the GroupLocationService), as this code block is identical
            // to that found within the SignUpDetail block's gOpportunities_Delete() method.

            using ( var rockContext = new RockContext() )
            {
                // An Opportunity is a GroupLocationSchedule with possible GroupMemberAssignments (and therefore, GroupMembers).
                // When deleting an Opportunity we should delete the following:
                // 1) GroupMemberAssignments
                // 2) GroupMembers (if no more GroupMemberAssignments for a given GroupMember)
                // 3) GroupLocationSchedule & GroupLocationScheduleConfig
                // 4) GroupLocation (if no more Schedules tied to it)
                // 5) Schedule (if non-named and nothing else is using it)

                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
                var groupMemberAssignments = groupMemberAssignmentService
                    .Queryable()
                    .Include( gma => gma.GroupMember )
                    .Where( gma => gma.GroupMember.GroupId == groupId
                        && gma.LocationId == locationId
                        && gma.ScheduleId == scheduleId
                    )
                    .ToList();

                // Set these aside so we can try to delete them next.
                var groupMembers = groupMemberAssignments
                    .Select( gma => gma.GroupMember )
                    .ToList();

                // For now, this is safe, as GroupMemberAssignment is a pretty low-level Entity with no child Entities.
                // We'll need to check `GroupMemberAssignmentService.CanDelete()` for each assignment (and abandon the bulk
                // delete approach) if this changes in the future.
                groupMemberAssignmentService.DeleteRange( groupMemberAssignments );

                // Get the GroupType to check if this Group has history enabled below, so we know whether to call GroupMemberService.CanDelete() for each GroupMember.
                var group = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( g => g.ParentGroup ) // ParentGroup may be needed for a proper authorization check.
                    .FirstOrDefault( g => g.Id == groupId );

                // Because sign-ups are a special usage of groups, people with "schedule" authorization may delete opportunities.
                var canDelete = group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || group.IsAuthorized( Authorization.SCHEDULE, this.CurrentPerson );

                if ( !canDelete )
                {
                    nbNotAuthorizedToDelete.Visible = true;
                    return;
                }

                var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                var groupMemberService = new GroupMemberService( rockContext );
                foreach ( var groupMember in groupMembers.Where( gm => !gm.GroupMemberAssignments.Any() ) )
                {
                    if ( !groupTypeCache.EnableGroupHistory && !groupMemberService.CanDelete( groupMember, out string groupMemberErrorMessage ) )
                    {
                        // The Attendee (Group Member Assignment) record itself will be deleted, but we cannot delete the underlying GroupMember record.
                        continue;
                    }

                    // We need to delete these one-by-one, as the individual Delete call will dynamically archive if necessary (whereas the bulk delete calls will not).
                    groupMemberService.Delete( groupMember );
                }

                // Now go get the GroupLocation, Schedule & GroupLocationScheduleConfig.
                var groupLocationService = new GroupLocationService( rockContext );
                var groupLocation = groupLocationService
                    .Queryable()
                    .Include( gl => gl.Schedules )
                    .Include( gl => gl.GroupLocationScheduleConfigs )
                    .FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );

                // We'll have to delete these last, since we reference the Schedule.Id in the GroupLocationSchedule & GroupLocationScheduleConfig tables.
                var schedulesToDelete = groupLocation.Schedules
                    .Where( s => s.Id == scheduleId )
                    .ToList();

                foreach ( var schedule in schedulesToDelete )
                {
                    groupLocation.Schedules.Remove( schedule );
                }

                foreach ( var config in groupLocation.GroupLocationScheduleConfigs.Where( gls => gls.ScheduleId == scheduleId ).ToList() )
                {
                    groupLocation.GroupLocationScheduleConfigs.Remove( config );
                }

                // If this GroupLocation has no more Schedules, delete it.
                if ( !groupLocation.Schedules.Any() )
                {
                    // Note that if there happen to be any lingering GroupLocationScheduleConfig records that somehow weren't deleted yet, a cascade delete will get rid of them here.
                    groupLocationService.Delete( groupLocation );
                }

                rockContext.WrapTransaction( () =>
                {
                    // Initial save to release FK constraints tied to referenced entities we'll be deleting.
                    rockContext.SaveChanges();

                    var scheduleService = new ScheduleService( rockContext );
                    foreach ( var schedule in schedulesToDelete )
                    {
                        // Remove the schedule if custom (non-named) and nothing else is using it.
                        if ( schedule.ScheduleType != ScheduleType.Named && scheduleService.CanDelete( schedule, out string scheduleErrorMessage ) )
                        {
                            scheduleService.Delete( schedule );
                        }
                    }

                    // We cannot safely remove referenced Locations (even non-named ones):
                    //  1) because of the way we reuse/share Locations across entities (the LocationPicker control auto-searches/matches and saves Locations).
                    //  2) because of the cascade deletes many of the referencing entities have on their LocationId FK constraints (we might accidentally delete a lot of unintended stuff).

                    // Follow-up save for deleted referenced entities.
                    rockContext.SaveChanges();
                } );
            }

            BindOpportunitiesGrid();
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            gOpportunities.ExportFilename = $"{this.SignUpGroupType.Name} Opportunities";
            gOpportunities.EntityIdField = "Id";

            // We'll have custom JavaScript (see SignUpOverview.ascx) do this instead.
            gOpportunities.ShowConfirmDeleteDialog = false;

            // We'll assume we can delete (so all delete buttons are enabled by default); then disable per row, via JavaScript if necessary.
            gOpportunities.IsDeleteEnabled = true;

            _ddlAction = new RockDropDownList();
            _ddlAction.ID = "ddlAction";
            _ddlAction.CssClass = "pull-left input-width-xl";
            _ddlAction.Items.Add( new ListItem( "-- Select Action --", GridAction.None ) );
            _ddlAction.Items.Add( new ListItem( "Email Leaders of Selected Schedules", GridAction.EmailLeaders ) );
            _ddlAction.Items.Add( new ListItem( "Email All Participants of Selected Schedules", GridAction.EmailAll ) );

            gOpportunities.Actions.AddCustomActionControl( _ddlAction );

            gfOpportunities.PreferenceKeyPrefix = $"{this.SignUpGroupTypeId}-";
        }

        /// <summary>
        /// Sets the grid filters.
        /// </summary>
        private void SetGridFilters()
        {
            sdrpDateRange.DelimitedValues = gfOpportunities.GetFilterPreference( GridFilterKey.DateRange );

            var signUpGroupTypeIds = GroupTypeCache
                .All()
                .Where( gt => gt.Id == SignUpGroupTypeId || gt.InheritedGroupTypeId == SignUpGroupTypeId )
                .Select( gt => gt.Id )
                .ToList();

            gpParentGroup.IncludedGroupTypeIds = signUpGroupTypeIds;

            var parentGroupId = gfOpportunities.GetFilterPreference( GridFilterKey.ParentGroup ).AsIntegerOrNull();
            Group group = null;
            if ( parentGroupId.HasValue )
            {
                // Don't wrap this RockContext in a using statement.
                // The GroupPicker performs somewhat unpredictable lookups throughout its lifetime, and
                // a disposed context will likely lead to an unhandled exception somewhere along the way.
                group = new GroupService( new RockContext() )
                    .Queryable()
                    .AsNoTracking()
                    .Include( g => g.ParentGroup )
                    .FirstOrDefault( g => g.Id == parentGroupId.Value );
            }

            gpParentGroup.SetValue( group );

            NumberComparisonFilter.SetValue(
                gfOpportunities.GetFilterPreference( GridFilterKey.SlotsAvailable ),
                ddlSlotsAvailableComparisonType,
                nbSlotsAvailableFilterCompareValue
            );
        }

        /// <summary>
        /// This runtime class will be used to populate the Opportunities grid within this block.
        /// </summary>
        private class Opportunity
        {
            private class ProgressState
            {
                public const string Success = "success";
                public const string Warning = "warning";
                public const string Critical = "critical";
                public const string Danger = "danger";
            }

            private string ParticipantCountBadgeType
            {
                get
                {
                    var min = this.SlotsMin.GetValueOrDefault();
                    var desired = this.SlotsDesired.GetValueOrDefault();
                    var max = this.SlotsMax.GetValueOrDefault();
                    var filled = this.ParticipantCount;

                    var progressState = ProgressState.Danger;
                    if ( filled > 0 )
                    {
                        progressState = ProgressState.Success;

                        if ( max > 0 && filled > max )
                        {
                            progressState = ProgressState.Critical;
                        }
                        else if ( filled < min )
                        {
                            progressState = ProgressState.Danger;
                        }
                        else if ( filled < desired )
                        {
                            progressState = ProgressState.Warning;
                        }
                    }

                    return progressState;
                }
            }

            /// <summary>
            /// This is a runtime Id, not related to any Entity in particular (this is needed for merge templates to work).
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// This is a runtime Guid, not related to any Entity in particular.
            /// </summary>
            public Guid Guid { get; set; }

            public bool CanDelete { get; set; }

            public int GroupId { get; set; }

            public int GroupLocationId { get; set; }

            public int LocationId { get; set; }

            public int ScheduleId { get; set; }

            public string ProjectName { get; set; }

            public DateTime? NextStartDateTime { get; set; }

            public DateTime? LastStartDateTime { get; set; }

            public int? SlotsMin { get; set; }

            public int? SlotsDesired { get; set; }

            public int? SlotsMax { get; set; }

            public int LeaderCount { get; set; }

            public int ParticipantCount { get; set; }

            public string ParticipantCountBadgeHtml
            {
                get
                {
                    return $"<span class='badge badge-{this.ParticipantCountBadgeType} participant-count-badge'>{this.ParticipantCount}</span>";
                }
            }

            // Give preference to NextStartDateTime, but if not available, fall back to LastStartDateTime. We need something to sort on and display.
            public DateTime? NextOrLastStartDateTime
            {
                get
                {
                    return this.NextStartDateTime.HasValue
                        ? this.NextStartDateTime
                        : this.LastStartDateTime;
                }
            }

            public string FriendlySchedule
            {
                get
                {
                    var friendlySchedule = this.NextOrLastStartDateTime?.ToString( "dddd, MMM d" );

                    if ( this.NextOrLastStartDateTime.HasValue && this.NextOrLastStartDateTime.Value.Year != RockDateTime.Now.Year )
                    {
                        friendlySchedule = $"{friendlySchedule} ({this.NextOrLastStartDateTime.Value.Year})";
                    }

                    return friendlySchedule;
                }
            }

            public int SlotsAvailable
            {
                get
                {
                    // This more complex approach uses a dynamic/floating minuend:
                    // 1) If the max value is defined, use that;
                    // 2) Else, if the desired value is defined, use that;
                    // 3) Else, if the min value is defined, use that;
                    // 4) Else, use int.MaxValue (there is no limit to the slots available).
                    //var minuend = this.SlotsMax.GetValueOrDefault() > 0
                    //    ? this.SlotsMax.Value
                    //    : this.SlotsDesired.GetValueOrDefault() > 0
                    //        ? this.SlotsDesired.Value
                    //        : this.SlotsMin.GetValueOrDefault() > 0
                    //            ? this.SlotsMin.Value
                    //            : int.MaxValue;

                    // Simple approach:
                    // 1) If the max value is defined, subtract participant count from that;
                    // 2) Otherwise, use int.MaxValue (there is no limit to the slots available).
                    var available = int.MaxValue;
                    if ( this.SlotsMax.GetValueOrDefault() > 0 )
                    {
                        available = this.SlotsMax.Value - this.ParticipantCount;
                    }

                    return available < 0 ? 0 : available;
                }
            }
        }

        /// <summary>
        /// This runtime class will be used to populate [CommunicationRecipient].[AdditionalMergeValuesJson] when sending bulk Communications.
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class OpportunitySummary : RockDynamic
        {
            public string ProjectName { get; set; }

            public string OpportunityName { get; set; }

            public string FormattedAddress { get; set; }

            public DateTime? NextStartDateTime { get; set; }

            public int LeaderCount { get; set; }

            public int ParticipantCount { get; set; }
        }

        /// <summary>
        /// Binds the opportunities grid.
        /// </summary>
        /// <param name="opportunities">The opportunities.</param>
        private void BindOpportunitiesGrid( List<Opportunity> opportunities = null )
        {
            if ( opportunities == null )
            {
                using ( var rockContext = new RockContext() )
                {
                    opportunities = GetOpportunities( rockContext );
                }
            }

            gOpportunities.DataSource = opportunities;
            gOpportunities.DataBind();

            RegisterJavaScriptForGridActions();
        }

        /// <summary>
        /// Gets the opportunities.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Opportunity> GetOpportunities( RockContext rockContext )
        {
            // Get the active opportunities (GroupLocationSchedules).
            var qryGroupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.IsActive // Should we care about [IsActive] here? I think so.. as we want this list to reflect what is publicly visible.
                    && ( gl.Group.GroupTypeId == this.SignUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == this.SignUpGroupTypeId )
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    GroupLocationId = gl.Id,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.ScheduleId == s.Id )
                } );

            // Filter by date range.
            DateTime fromDateTime = RockDateTime.Now;
            DateTime? toDateTime = null;

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );
            if ( dateRange != null && ( dateRange.Start.HasValue || dateRange.End.HasValue ) )
            {
                if ( dateRange.Start.HasValue )
                {
                    // Allow displaying past opportunities since we're on the administrative side.
                    fromDateTime = dateRange.Start.Value;
                }

                if ( dateRange.End.HasValue )
                {
                    // Set this to the end of the selected day to perform a search fully-inclusive of the day the individual
                    // selected. Note also that we cannot apply this filter during the query phase; we need to wait until we
                    // materialize Schedule objects so we can compare this value to Schedule.Next[or last]StartDateTime, which
                    // is a runtime-calculated value. If we instead applied this filter to the Schedule.EffectiveEndDate,
                    // we could accidentally rule out opportunities that the individual might otherwise be interested in managing.
                    // We'll apply this filter value below.
                    toDateTime = dateRange.End.Value.EndOfDay();
                }
            }

            // Get just the date portion of the "from" date so we can compare it against the stored Schedules' EffectiveEndDates, which hold
            // only a date value (without the time component). Return any Schedules whose EffectiveEndDate:
            //  1) is not defined (this should never happen, but get them just in case), OR
            //  2) is greater than or equal to the "from" date being filtered against.
            // 
            // We'll do this to rule out any Schedules that have already ended, therefore making the initial results record set smaller,
            // since we still have to do additional Schedule-based filtering below: once we materialize the Schedule objects, we'll use their
            // runtime-calculated "Start[Date]Time" properties and methods to ensure we're only showing Schedules that actually qualify to
            // be shown, based on the DateTime filter criteria provided to this method (either RockDateTime.Now OR the "from" date selected
            // by the individual performing the search).
            DateTime fromDate = fromDateTime.Date;
            qryGroupLocationSchedules = qryGroupLocationSchedules
                .Where( gls => !gls.Schedule.EffectiveEndDate.HasValue || gls.Schedule.EffectiveEndDate >= fromDate );

            // Filter by parent group.
            var parentGroupId = gpParentGroup.SelectedValueAsId();
            if ( parentGroupId.HasValue )
            {
                qryGroupLocationSchedules = qryGroupLocationSchedules.Where( gls => gls.Group.Id == parentGroupId.Value );
            }

            // Get all group member assignments for all filtered opportunities; we'll hook them up to their respective opportunities below.
            var assignments = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gma => gma.GroupMember.GroupRole )
                .Include( gma => gma.GroupMember.Group.ParentGroup ) // ParentGroup may be needed for a proper authorization check.
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && qryGroupLocationSchedules.Any( gls =>
                        gls.Group.Id == gma.GroupMember.GroupId
                        && gls.Location.Id == gma.LocationId
                        && gls.Schedule.Id == gma.ScheduleId
                    )
                )
                .ToList();

            var opportunities = qryGroupLocationSchedules
                .ToList() // Execute the query.
                .Select( gls =>
                {
                    // Because sign-ups are a special usage of groups, people with "schedule" authorization may delete opportunities.
                    var canDelete = gls.Group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || gls.Group.IsAuthorized( Authorization.SCHEDULE, this.CurrentPerson );

                    var locationId = gls.Location.Id;
                    var scheduleId = gls.Schedule.Id;

                    var participants = assignments
                        .Where( a => a.LocationId == locationId && a.ScheduleId == scheduleId )
                        .ToList();

                    DateTime? nextStartDateTime = gls.Schedule.NextStartDateTime;
                    DateTime? lastStartDateTime = null;

                    if ( !nextStartDateTime.HasValue )
                    {
                        // Give preference to NextStartDateTime, but if not available, fall back to LastStartDateTime. We need something to sort on and display.
                        var startDateTimes = gls.Schedule.GetScheduledStartTimes( fromDateTime, toDateTime ?? DateTime.MaxValue );
                        var lastScheduledStartDateTime = startDateTimes.LastOrDefault();
                        if ( lastScheduledStartDateTime != default )
                        {
                            lastStartDateTime = lastScheduledStartDateTime;
                        }
                    }

                    return new Opportunity
                    {
                        Id = ++_entitySetItemEntityId,
                        Guid = Guid.NewGuid(),
                        CanDelete = canDelete,
                        GroupId = gls.Group.Id,
                        GroupLocationId = gls.GroupLocationId,
                        LocationId = locationId,
                        ScheduleId = scheduleId,
                        ProjectName = gls.Group.Name,
                        NextStartDateTime = nextStartDateTime,
                        LastStartDateTime = lastStartDateTime,
                        SlotsMin = gls.Config?.MinimumCapacity,
                        SlotsDesired = gls.Config?.DesiredCapacity,
                        SlotsMax = gls.Config?.MaximumCapacity,
                        LeaderCount = participants.Count( p => p.GroupMember.GroupRole.IsLeader ),
                        ParticipantCount = participants.Count
                    };
                } );

            // Now that we have materialized Schedule objects in memory, let's further apply DateTime filtering using the Schedules' runtime-calculated
            // "Start[Date]Time" method and property values.
            opportunities = opportunities
                .Where( o =>
                    o.NextOrLastStartDateTime.HasValue
                    && o.NextOrLastStartDateTime.Value >= fromDateTime
                    && (
                        !toDateTime.HasValue // The individual didn't select an end date.
                        || o.NextOrLastStartDateTime.Value < toDateTime.Value // The project's [next or last] start date time is/was less than the [end of the] end date they selected.
                    )
                );

            // Filter by slots available.
            var comparisonType = NumberComparisonFilter.SelectedComparisonType( ddlSlotsAvailableComparisonType );
            var comparisonValue = NumberComparisonFilter.SelectedComparisonValue( nbSlotsAvailableFilterCompareValue );
            if ( comparisonType.HasValue && comparisonValue.HasValue )
            {
                opportunities = opportunities
                    .Where( o =>
                    {
                        switch ( comparisonType )
                        {
                            case ComparisonType.EqualTo:
                                return o.SlotsAvailable == comparisonValue.Value;
                            case ComparisonType.NotEqualTo:
                                return o.SlotsAvailable != comparisonValue.Value;
                            case ComparisonType.GreaterThan:
                                return o.SlotsAvailable > comparisonValue.Value;
                            case ComparisonType.GreaterThanOrEqualTo:
                                return o.SlotsAvailable >= comparisonValue.Value;
                            case ComparisonType.LessThan:
                                return o.SlotsAvailable < comparisonValue.Value;
                            case ComparisonType.LessThanOrEqualTo:
                                return o.SlotsAvailable <= comparisonValue.Value;
                            default:
                                return false;
                        }
                    } );
            }

            // Sort.
            List<Opportunity> sortedOpportunities = null;

            if ( gOpportunities.SortProperty != null )
            {
                // Because this is a runtime object that we've constructed from a few different Entities, sorting needs to be one manually.
                var direction = gOpportunities.SortProperty.Direction;

                switch ( gOpportunities.SortProperty.Property )
                {
                    case SortExpression.ProjectName:
                        sortedOpportunities = direction == SortDirection.Ascending
                            ? opportunities.OrderBy( o => o.ProjectName ).ToList()
                            : opportunities.OrderByDescending( o => o.ProjectName ).ToList();
                        break;
                    case SortExpression.NextOrLastStartDateTime:
                        sortedOpportunities = direction == SortDirection.Ascending
                            ? opportunities.OrderBy( o => o.NextOrLastStartDateTime ).ToList()
                            : opportunities.OrderByDescending( o => o.NextOrLastStartDateTime ).ToList();
                        break;
                    case SortExpression.LeaderCount:
                        sortedOpportunities = direction == SortDirection.Ascending
                            ? opportunities.OrderBy( o => o.LeaderCount ).ToList()
                            : opportunities.OrderByDescending( o => o.LeaderCount ).ToList();
                        break;
                    case SortExpression.ParticipantCount:
                        sortedOpportunities = direction == SortDirection.Ascending
                            ? opportunities.OrderBy( o => o.ParticipantCount ).ToList()
                            : opportunities.OrderByDescending( o => o.ParticipantCount ).ToList();
                        break;
                }
            }

            if ( sortedOpportunities == null )
            {
                // Default sort.
                sortedOpportunities = opportunities
                    .OrderBy( o => o.NextOrLastStartDateTime ?? DateTime.MaxValue )
                    .ThenBy( o => o.ProjectName )
                    .ThenByDescending( o => o.ParticipantCount )
                    .ToList();
            }

            this.OpportunitiesState = sortedOpportunities;

            return sortedOpportunities;
        }

        /// <summary>
        /// Registers the JavaScript for grid actions.
        /// NOTE: This needs to be done after binding the grid.
        /// </summary>
        private void RegisterJavaScriptForGridActions()
        {
            string script = $@"
                $('#{_ddlAction.ClientID}').on('change', function (e){{
                    var $ddl = $(this);
                    var action = $ddl.val();
                    $('#{hfAction.ClientID}').val(action);

                    var count = $(""#{gOpportunities.ClientID} input[id$='_cbSelect_0']:checked"").length;
                    if (action === '{GridAction.None}' || count === 0) {{
                        return;
                    }}

                    window.location = ""javascript:{Page.ClientScript.GetPostBackEventReference( this, PostbackEventArgument.GridActionChanged )}"";
                    $ddl.val('');
                }});";

            ScriptManager.RegisterStartupScript( _ddlAction, _ddlAction.GetType(), "ProcessGridActionChange", script, true );
        }

        /// <summary>
        /// Emails the participants.
        /// </summary>
        /// <param name="opportunities">The opportunities.</param>
        /// <param name="shouldOnlyEmailLeaders">if set to <c>true</c> [should only email leaders].</param>
        private void EmailParticipants( List<Opportunity> opportunities, bool shouldOnlyEmailLeaders = false )
        {
            // These lists of selected Group/Location/Schedule IDs should be pretty small; SQL WHERE IN clauses should be safe here.
            var distinctGroupIds = opportunities.Select( o => o.GroupId ).Distinct().ToList();
            var distinctLocationIds = opportunities.Select( o => o.LocationId ).Distinct().ToList();
            var distinctScheduleIds = opportunities.Select( o => o.ScheduleId ).Distinct().ToList();

            using ( var rockContext = new RockContext() )
            using ( var communicationRecipientRockContext = new RockContext() )
            {
                var qry = new GroupMemberAssignmentService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( gma =>
                        !gma.GroupMember.Person.IsDeceased
                        && distinctGroupIds.Contains( gma.GroupMember.GroupId )
                        && gma.LocationId.HasValue && distinctLocationIds.Contains( gma.LocationId.Value )
                        && gma.ScheduleId.HasValue && distinctScheduleIds.Contains( gma.ScheduleId.Value )
                    );

                if ( shouldOnlyEmailLeaders )
                {
                    qry = qry.Where( gma => gma.GroupMember.GroupRole.IsLeader );
                }

                var participants = qry
                    .Select( gma => new
                    {
                        gma.GroupMember.PersonId,
                        gma.GroupMember.GroupId,
                        gma.LocationId,
                        gma.ScheduleId
                    } )
                    .ToList();

                var distinctPersonIds = participants
                    .Select( p => p.PersonId )
                    .Distinct()
                    .ToList();

                if ( !distinctPersonIds.Any() )
                {
                    mdSignUpOverview.Show( "Unable to send email, as no recipients were found.", ModalAlertType.Information );
                    return;
                }

                // Get the primary aliases.
                var personAliasService = new PersonAliasService( rockContext );
                var distinctPrimaryAliases = new List<PersonAlias>( distinctPersonIds.Count );

                // Get the data in chunks just in case we have a large list of PersonIds (to avoid a SQL Expression limit error).
                var chunkedPersonIds = distinctPersonIds.Take( 1000 );
                var skipCount = 0;
                while ( chunkedPersonIds.Any() )
                {
                    var chunkedPrimaryAliases = personAliasService
                        .Queryable()
                        .AsNoTracking()
                        .Where( pa => pa.PersonId == pa.AliasPersonId && chunkedPersonIds.Contains( pa.PersonId ) )
                        .ToList();

                    distinctPrimaryAliases.AddRange( chunkedPrimaryAliases );

                    skipCount += 1000;
                    chunkedPersonIds = distinctPersonIds.Skip( skipCount ).Take( 1000 );
                }

                // Get Groups, Locations, Schedules & GroupLocationScheduleConfigs.
                var groupLocations = new GroupLocationService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( gl => gl.Group )
                    .Include( gl => gl.Location )
                    .Include( gl => gl.Schedules )
                    .Include( gl => gl.GroupLocationScheduleConfigs )
                    .Where( gl => distinctGroupIds.Contains( gl.GroupId )
                        && distinctLocationIds.Contains( gl.LocationId )
                    )
                    .ToList();

                var currentPersonAliasId = this.RockPage.CurrentPersonAliasId;

                // Add custom merge fields.
                var mergeFields = new List<string>
                {
                    MergeFieldKey.Opportunities
                };

                // Create communication.
                var communication = new Communication
                {
                    IsBulkCommunication = true,
                    Status = CommunicationStatus.Transient,
                    SenderPersonAliasId = currentPersonAliasId,
                    AdditionalMergeFields = mergeFields
                };

                communication.UrlReferrer = this.RockPage.Request?.UrlProxySafe()?.AbsoluteUri?.TrimForMaxLength( communication, "UrlReferrer" );

                var communicationService = new CommunicationService( rockContext );
                communicationService.Add( communication );

                // Save Communication to get ID.
                rockContext.SaveChanges();

                var now = RockDateTime.Now;

                var communicationRecipientList = distinctPrimaryAliases
                    .Select( alias =>
                    {
                        var opportunitySummaries = new List<OpportunitySummary>();
                        foreach ( var participant in participants.Where( p => p.PersonId == alias.PersonId ) )
                        {
                            var opportunity = opportunities.FirstOrDefault( o =>
                                o.GroupId == participant.GroupId
                                && o.LocationId == participant.LocationId.ToIntSafe()
                                && o.ScheduleId == participant.ScheduleId.ToIntSafe()
                            );

                            if ( opportunity != null )
                            {
                                var groupLocation = groupLocations.FirstOrDefault( gl =>
                                    gl.GroupId == opportunity.GroupId
                                    && gl.LocationId == opportunity.LocationId
                                );

                                Group group = null;
                                Location location = null;
                                Schedule schedule = null;
                                GroupLocationScheduleConfig config = null;

                                if ( groupLocation != null )
                                {
                                    group = groupLocation.Group;
                                    location = groupLocation.Location;
                                    schedule = groupLocation.Schedules.FirstOrDefault( s => s.Id == opportunity.ScheduleId );
                                    config = groupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == opportunity.ScheduleId );
                                }

                                opportunitySummaries.Add( new OpportunitySummary
                                {
                                    ProjectName = group?.Name,
                                    OpportunityName = config?.ConfigurationName,
                                    FormattedAddress = location?.FormattedAddress,
                                    NextStartDateTime = schedule?.NextStartDateTime,
                                    LeaderCount = opportunity.LeaderCount,
                                    ParticipantCount = opportunity.ParticipantCount
                                } );
                            }
                        }

                        return new CommunicationRecipient
                        {
                            CommunicationId = communication.Id,
                            PersonAliasId = alias.Id,
                            AdditionalMergeValues = new Dictionary<string, object>
                            {
                                { MergeFieldKey.Opportunities, opportunitySummaries }
                            },
                            CreatedByPersonAliasId = currentPersonAliasId,
                            ModifiedByPersonAliasId = currentPersonAliasId,
                            CreatedDateTime = now,
                            ModifiedDateTime = now
                        };
                    } )
                    .ToList();

                // BulkInsert to quickly insert the CommunicationRecipient records. Note: This is much faster, but will bypass EF and Rock processing.
                communicationRecipientRockContext.BulkInsert( communicationRecipientList );

                // Get the URL to the communication page.
                var communicationPageRef = this.RockPage.Site.CommunicationPageReference;
                string communicationUrl;
                if ( communicationPageRef.PageId > 0 )
                {
                    communicationPageRef.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
                    communicationUrl = communicationPageRef.BuildUrl();
                }
                else
                {
                    communicationUrl = "~/Communication/{0}";
                }

                Page.Response.Redirect( communicationUrl, false );
                this.Context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion
    }
}
