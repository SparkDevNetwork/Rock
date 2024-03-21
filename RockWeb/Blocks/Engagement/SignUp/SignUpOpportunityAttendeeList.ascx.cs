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

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Engagement.SignUp
{
    [DisplayName( "Sign-Up Opportunity Attendee List" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Lists all the group members for the selected group, location and schedule." )]

    #region Block Attributes

    [LinkedPage( "Group Member Detail Page",
        Key = AttributeKey.GroupMemberDetailPage,
        Description = "Page used for viewing an attendee's group member detail for this Sign-Up project. Clicking a row in the grid will take you to this page.",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Person Profile Page",
        Key = AttributeKey.PersonProfilePage,
        Description = "Page used for viewing a person's profile. If set, a view profile button will show for each group member.",
        IsRequired = false,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "EE652767-5070-4EAB-8BB7-BB254DD01B46" )]
    public partial class SignUpOpportunityAttendeeList : RockBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string GroupMemberDetailPage = "GroupMemberDetailPage";
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class ViewStateKey
        {
            public const string GroupMemberIdByGroupMemberAssignmentIds = "GroupMemberIdByGroupMemberAssignmentIds";
            public const string MemberAttributeIds = "MemberAttributeIds";
            public const string MemberOpportunityAttributeIds = "MemberOpportunityAttributeIds";
        }

        private static class GridFilterKey
        {
            public const string FirstName = "FirstName";
            public const string LastName = "LastName";
            public const string Role = "Role";
            public const string Status = "Status";
            public const string Campus = "Campus";
            public const string Gender = "Gender";

            public const string MemberAttribute = "MemberAttribute";
            public const string MemberOpportunityAttribute = "MemberOpportunityAttribute";
        }

        private static class DataKeyName
        {
            public const string Id = "Id";
        }

        #endregion

        #region Fields

        private int _groupId;
        private int _locationId;
        private int _scheduleId;

        private bool _canView;
        private bool _canManageMembers;

        private GroupTypeCache _groupTypeCache;

        private bool _hasGroupRequirements = false;
        private bool _isCommunicating = false;
        private bool _isExporting = false;

        private HashSet<int> _groupMemberIdsNotMeetingRequirements = null;
        private Dictionary<int, List<GroupRequirementType>> _unmetRequirementTypesByGroupMemberId = null;

        private Dictionary<int, Location> _personIdHomeLocationLookup = null;
        private Dictionary<int, Dictionary<int, string>> _personIdPhoneNumberTypePhoneNumberLookup = null;

        private HashSet<int> _groupMembersWithGroupMemberHistory = null;
        private HashSet<int> _groupTypeRoleIdsWithGroupSync = null;

        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;

        #endregion

        #region Properties

        private int SignUpGroupTypeId
        {
            get
            {
                return GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP )?.Id ?? 0;
            }
        }

        private int? InactiveRecordStatusValueId
        {
            get
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE )?.Id;
            }
        }

        private int? FamilyGroupTypeId
        {
            get
            {
                return GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY )?.Id;
            }
        }

        private DefinedValueCache HomeAddressDefinedValue
        {
            get
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            }
        }

        private int? HomePhoneTypeId
        {
            get
            {
                return DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            }
        }

        private int? CellPhoneTypeId
        {
            get
            {
                return DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            }
        }

        public Dictionary<int, int> GroupMemberIdByGroupMemberAssignmentIds { get; set; }

        public List<AttributeCache> MemberAttributes { get; set; }

        public List<AttributeCache> MemberOpportunityAttributes { get; set; }

        #endregion

        #region Control Life-Cycle Events

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState[ViewStateKey.GroupMemberIdByGroupMemberAssignmentIds] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                this.GroupMemberIdByGroupMemberAssignmentIds = new Dictionary<int, int>();
            }
            else
            {
                this.GroupMemberIdByGroupMemberAssignmentIds = JsonConvert.DeserializeObject<Dictionary<int, int>>( json ) ?? new Dictionary<int, int>();
            }

            if ( ViewState[ViewStateKey.MemberAttributeIds] != null )
            {
                MemberAttributes = ( ViewState[ViewStateKey.MemberAttributeIds] as int[] ).Select( id => AttributeCache.Get( id ) ).ToList();
            }

            if ( ViewState[ViewStateKey.MemberOpportunityAttributeIds] != null )
            {
                MemberOpportunityAttributes = ( ViewState[ViewStateKey.MemberOpportunityAttributeIds] as int[] ).Select( id => AttributeCache.Get( id ) ).ToList();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            nbNotAuthorizedToView.Text = EditModeMessage.NotAuthorizedToView( Group.FriendlyTypeName );

            _groupId = PageParameter( PageParameterKey.GroupId ).ToIntSafe();
            _locationId = PageParameter( PageParameterKey.LocationId ).ToIntSafe();
            _scheduleId = PageParameter( PageParameterKey.ScheduleId ).ToIntSafe();

            using ( var rockContext = new RockContext() )
            {
                var group = GetSharedGroup( rockContext );
                if ( group != null )
                {
                    _canView = group.IsAuthorized( Authorization.VIEW, this.CurrentPerson );
                    _canManageMembers = group.IsAuthorized( Authorization.EDIT, this.CurrentPerson )
                        || group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson )
                        || group.IsAuthorized( Authorization.SCHEDULE, this.CurrentPerson );

                    InitializeGrid( group );
                }
            }

            // Add lazy load so that person-link-popover JavaScript works
            RockPage.AddScriptLink( "~/Scripts/jquery.lazyload.min.js" );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSignUpOpportunityAttendeeList );
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
                ShowDetails();
            }
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
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.GroupMemberIdByGroupMemberAssignmentIds] = JsonConvert.SerializeObject( this.GroupMemberIdByGroupMemberAssignmentIds, Formatting.None, jsonSetting );

            ViewState[ViewStateKey.MemberAttributeIds] = MemberAttributes?.Select( a => a.Id ).ToArray();
            ViewState[ViewStateKey.MemberOpportunityAttributeIds] = MemberOpportunityAttributes?.Select( a => a.Id ).ToArray();

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var groupId = PageParameter( PageParameterKey.GroupId ).ToIntSafe();
            var locationId = PageParameter( PageParameterKey.LocationId ).ToIntSafe();
            var scheduleId = PageParameter( PageParameterKey.ScheduleId ).ToIntSafe();

            string opportunityName = null;

            if ( groupId > 0 && locationId > 0 && scheduleId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupLocation = new GroupLocationService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( gl => gl.Group )
                        .Include( gl => gl.GroupLocationScheduleConfigs )
                        .FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );

                    var config = groupLocation?.GroupLocationScheduleConfigs?.FirstOrDefault( c => c.ScheduleId == scheduleId );

                    if ( config != null )
                    {
                        // Prefer the name provided at the opportunity level.
                        opportunityName = config.ConfigurationName;

                        if ( opportunityName.IsNullOrWhiteSpace() )
                        {
                            // Fall back to the name provided at the group/project level.
                            opportunityName = groupLocation.Group.Name;
                        }
                    }
                }
            }

            if ( opportunityName.IsNotNullOrWhiteSpace() )
            {
                var breadCrumbName = $"{opportunityName} Attendee List";
                breadCrumbs.Add( new BreadCrumb( breadCrumbName, pageReference ) );
            }
            else
            {
                // Don't show a breadcrumb if we couldn't find an opportunity.
            }

            return breadCrumbs;
        }

        #endregion

        #region Attendees Grid Events

        /// <summary>
        /// Displays the gfAttendees filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfAttendees_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            // A local function to try and get the filter value from an attribute.
            bool TryGetAttributeFilterValue( List<AttributeCache> attributes, string gridFilterKeyPrefix )
            {
                if ( attributes?.Any() == true )
                {
                    var attribute = attributes.FirstOrDefault( a => $"{gridFilterKeyPrefix}-{a.Key}" == e.Key );
                    if ( attribute != null )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                            e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                            return true;
                        }
                        catch
                        {
                            // Intentionally ignore.
                        }
                    }
                }

                return false;
            }

            if ( TryGetAttributeFilterValue( MemberAttributes, GridFilterKey.MemberAttribute ) )
            {
                return;
            }
            else if ( TryGetAttributeFilterValue( MemberOpportunityAttributes, GridFilterKey.MemberOpportunityAttribute ) )
            {
                return;
            }
            else if ( e.Key == GridFilterKey.FirstName )
            {
                return;
            }
            else if ( e.Key == GridFilterKey.LastName )
            {
                return;
            }
            else if ( e.Key == GridFilterKey.Role )
            {
                e.Value = ResolveCheckBoxListValues( e.Value, cblRole );
            }
            else if ( e.Key == GridFilterKey.Status )
            {
                e.Value = ResolveCheckBoxListValues( e.Value, cblGroupMemberStatus );
            }
            else if ( e.Key == GridFilterKey.Campus )
            {
                var campusId = e.Value.AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    var campusCache = CampusCache.Get( campusId.Value );
                    if ( campusCache != null )
                    {
                        e.Value = campusCache.Name;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
            else if ( e.Key == GridFilterKey.Gender )
            {
                e.Value = ResolveCheckBoxListValues( e.Value, cblGenderFilter );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAttendees_ApplyFilterClick( object sender, EventArgs e )
        {
            gfAttendees.SetFilterPreference( GridFilterKey.FirstName, "First Name", tbFirstName.Text );
            gfAttendees.SetFilterPreference( GridFilterKey.LastName, "Last Name", tbLastName.Text );
            gfAttendees.SetFilterPreference( GridFilterKey.Role, "Role", cblRole.SelectedValues.AsDelimited( ";" ) );
            gfAttendees.SetFilterPreference( GridFilterKey.Status, "Status", cblGroupMemberStatus.SelectedValues.AsDelimited( ";" ) );
            gfAttendees.SetFilterPreference( GridFilterKey.Campus, "Campus", cpCampusFilter.SelectedCampusId.ToString() );
            gfAttendees.SetFilterPreference( GridFilterKey.Gender, "Gender", cblGenderFilter.SelectedValues.AsDelimited( ";" ) );

            ApplyAttributeFilters( MemberAttributes, GridFilterKey.MemberAttribute );
            ApplyAttributeFilters( MemberOpportunityAttributes, GridFilterKey.MemberOpportunityAttribute );

            BindAttendeesGrid();
        }

        /// <summary>
        /// Applies dynamic attribute filters.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="gridFilterKeyPrefix">The grid filter key prefix for the attributes.</param>
        private void ApplyAttributeFilters( List<AttributeCache> attributes, string gridFilterKeyPrefix )
        {
            if ( attributes?.Any() != true )
            {
                return;
            }

            foreach ( var attribute in attributes )
            {
                var filterPreferenceKey = $"{gridFilterKeyPrefix}-{attribute.Key}";

                var filterControl = phAttributeFilters.FindControl( $"attributeFilter_{attribute.Id}" );
                if ( filterControl == null )
                {
                    // No filter control, so clear out the person preference.
                    gfAttendees.SetFilterPreference( filterPreferenceKey, attribute.Name, null );
                    continue;
                }

                try
                {
                    var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                    gfAttendees.SetFilterPreference( filterPreferenceKey, attribute.Name, values.ToJson() );
                }
                catch
                {
                    // Intentionally ignore.
                }
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfAttendees_ClearFilterClick( object sender, EventArgs e )
        {
            gfAttendees.DeleteFilterPreferences();

            using ( var rockContext = new RockContext() )
            {
                SetGridFilters( GetSharedGroup( rockContext ) );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var groupMemberAssignment = e.Row.DataItem as GroupMemberAssignment;
            GroupMember groupMember = groupMemberAssignment?.GroupMember;
            if ( groupMemberAssignment == null || groupMember == null || groupMember.Person == null )
            {
                return;
            }

            if ( e.Row.FindControl( "lNameWithHtml" ) is Literal lNameWithHtml )
            {
                var nameHtml = new StringBuilder();

                if ( _hasGroupRequirements && _groupMemberIdsNotMeetingRequirements.Contains( groupMember.Id ) )
                {
                    nameHtml.Append( $"<i class='fa fa-exclamation-triangle text-warning unmet-group-requirements margin-r-md' data-tip='{GetUnmetRequirementsTooltipId( groupMember.Id )}'></i>{GetUnmetRequirementsTooltip( groupMember.Id )}" );
                }

                nameHtml.Append( $"<div class=\"photo-icon photo-round photo-round-xs margin-r-sm js-person-popover\" personid=\"{groupMember.PersonId}\" data-original=\"{groupMember.Person.PhotoUrl}&w=50\" style=\"background-image: url( 'ResolveUrl( \"~/Assets/Images/person-no-photo-unknown.svg\" )' ); background-size: cover; background-repeat: no-repeat;\"></div>" );

                nameHtml.Append( groupMember.Person.FullName );
                if ( groupMember.Person.TopSignalColor.IsNotNullOrWhiteSpace() )
                {
                    nameHtml.Append( $" {groupMember.Person.GetSignalMarkup()}" );
                }

                if ( groupMember.Note.IsNotNullOrWhiteSpace() )
                {
                    nameHtml.Append( $" <span class='js-group-member-note' data-toggle='tooltip' data-placement='top' title='{groupMember.Note.EncodeHtml()}'><i class='fa fa-file-text-o text-info'></i></span>" );
                }

                lNameWithHtml.Text = nameHtml.ToString();
            }

            if ( _isExporting )
            {
                if ( e.Row.FindControl( "lExportFullName" ) is Literal lExportFullName )
                {
                    lExportFullName.Text = groupMember.Person.FullNameReversed;
                }

                var personPhoneNumbers = _personIdPhoneNumberTypePhoneNumberLookup.GetValueOrNull( groupMember.PersonId );
                if ( personPhoneNumbers != null )
                {
                    if ( this.HomePhoneTypeId.HasValue && e.Row.FindControl( "lExportHomePhone" ) is Literal lExportHomePhone )
                    {
                        lExportHomePhone.Text = personPhoneNumbers.GetValueOrNull( this.HomePhoneTypeId.Value );
                    }

                    if ( this.CellPhoneTypeId.HasValue && e.Row.FindControl( "lExportCellPhone" ) is Literal lExportCellPhone )
                    {
                        lExportCellPhone.Text = personPhoneNumbers.GetValueOrNull( this.CellPhoneTypeId.Value );
                    }
                }

                var homeLocation = _personIdHomeLocationLookup.GetValueOrNull( groupMember.PersonId );
                if ( homeLocation != null )
                {
                    if ( e.Row.FindControl( "lExportHomeAddress" ) is Literal lExportHomeAddress )
                    {
                        lExportHomeAddress.Text = homeLocation.FormattedAddress;
                    }

                    if ( e.Row.FindControl( "lExportLatitude" ) is Literal lExportLatitude )
                    {
                        lExportLatitude.Text = homeLocation.Latitude.ToString();
                    }

                    if ( e.Row.FindControl( "lExportLongitude" ) is Literal lExportLongitude )
                    {
                        lExportLongitude.Text = homeLocation.Longitude.ToString();
                    }
                }
            }

            if ( _deleteField?.Visible == true )
            {
                LinkButton deleteButton = null;
                HtmlGenericControl buttonIcon = null;

                if ( !_deleteFieldColumnIndex.HasValue )
                {
                    _deleteFieldColumnIndex = gAttendees.GetColumnIndex( gAttendees.Columns.OfType<DeleteField>().First() );
                }

                if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                {
                    deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                }

                if ( deleteButton != null )
                {
                    buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();
                }

                if ( buttonIcon != null )
                {
                    if ( _groupTypeRoleIdsWithGroupSync.Contains( groupMember.GroupRoleId ) )
                    {
                        deleteButton.Enabled = false;
                        buttonIcon.Attributes["class"] = "fa fa-exchange";
                        var groupTypeRole = _groupTypeCache.Roles.FirstOrDefault( a => a.Id == groupMember.GroupRoleId );
                        deleteButton.ToolTip = string.Format( "Managed by group sync for role \"{0}\".", groupTypeRole );
                    }
                    else if ( _groupTypeCache.EnableGroupHistory == true && _groupMembersWithGroupMemberHistory.Contains( groupMember.Id ) )
                    {
                        buttonIcon.Attributes["class"] = "fa fa-archive";
                        deleteButton.AddCssClass( "btn-danger" );
                        deleteButton.ToolTip = "Archive";
                        e.Row.AddCssClass( "js-has-grouphistory" );
                    }
                }
            }

            if ( groupMember.Person.IsDeceased )
            {
                e.Row.AddCssClass( "is-deceased" );
            }

            if ( this.InactiveRecordStatusValueId.HasValue && groupMember.Person.RecordStatusValueId == this.InactiveRecordStatusValueId.Value )
            {
                e.Row.AddCssClass( "is-inactive-person" );
            }

            if ( groupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
            {
                e.Row.AddCssClass( "is-inactive" );
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_RowSelected( object sender, RowEventArgs e )
        {
            if ( this.GroupMemberIdByGroupMemberAssignmentIds.TryGetValue( e.RowKeyValue.ToIntSafe(), out int groupMemberId ) )
            {
                NavigateToGroupMemberDetailPage( groupMemberId );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindAttendeesGrid( isCommunicating: e.IsCommunication, isExporting: e.IsExporting );
        }

        /// <summary>
        /// Handles the GetRecipientMergeFields event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GetRecipientMergeFieldsEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_GetRecipientMergeFields( object sender, GetRecipientMergeFieldsEventArgs e )
        {
            var groupMemberAssignment = e.DataItem as GroupMemberAssignment;

            if ( groupMemberAssignment == null )
            {
                return;
            }

            var entityTypeMergeField = MergeFieldPicker.EntityTypeInfo.GetMergeFieldId<GroupMemberAssignment>(
                new MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier[]
                {
                    new MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier( "GroupId", _groupId.ToString() ),
                    new MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier( "GroupTypeId", _groupTypeCache.Id.ToString() ),
                    new MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier( "LocationId", groupMemberAssignment.LocationId.ToString() ),
                    new MergeFieldPicker.EntityTypeInfo.EntityTypeQualifier( "ScheduleId", groupMemberAssignment.ScheduleId.ToString() )
                } );

            e.MergeValues.Add( entityTypeMergeField, groupMemberAssignment.Id );
        }

        /// <summary>
        /// Handles the AddClick event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttendees_AddClick( object sender, EventArgs e )
        {
            NavigateToGroupMemberDetailPage();
        }

        /// <summary>
        /// Handles the Click event of the DeleteOrArchiveGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteOrArchiveGroupMember_Click( object sender, RowEventArgs e )
        {
            var groupMemberAssignmentId = e.RowKeyValue.ToIntSafe();
            if ( !this.GroupMemberIdByGroupMemberAssignmentIds.TryGetValue( groupMemberAssignmentId, out int groupMemberId ) )
            {
                mdGridWarning.Show( "Unable to delete attendee.", ModalAlertType.Warning );
                return;
            }

            var shouldRebindGrid = false;

            using ( var rockContext = new RockContext() )
            {
                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                var groupMemberAssignments = groupMemberAssignmentService
                    .Queryable()
                    .Where( gma => gma.GroupMemberId == groupMemberId )
                    .ToList();

                var thisAssignment = groupMemberAssignments.FirstOrDefault( gma => gma.Id == groupMemberAssignmentId );

                if ( thisAssignment != null )
                {
                    if ( !groupMemberAssignmentService.CanDelete( thisAssignment, out string groupMemberAssignmentErrorMessage ) )
                    {
                        mdGridWarning.Show( groupMemberAssignmentErrorMessage, ModalAlertType.Information );
                        return;
                    }
                    else
                    {
                        groupMemberAssignments.Remove( thisAssignment );
                        groupMemberAssignmentService.Delete( thisAssignment );
                        shouldRebindGrid = true;
                    }
                }

                // If no other assignments remain for this GroupMember, try to delete or archive the GroupMember record as well.
                if ( !groupMemberAssignments.Any() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    var groupMember = groupMemberService.Get( groupMemberId );

                    if ( groupMember != null )
                    {
                        var shouldArchive = false;
                        var shouldDelete = false;

                        // This call loads up the GroupTypeCache and GroupMemberHistorical records we need.
                        GetSharedGroup( rockContext );

                        if ( _groupTypeCache.EnableGroupHistory && _groupMembersWithGroupMemberHistory.Contains( groupMemberId ) )
                        {
                            shouldArchive = true;
                            shouldRebindGrid = true;
                        }
                        else
                        {
                            if ( !groupMemberService.CanDelete( groupMember, out string groupMemberErrorMessage ) )
                            {
                                // Just swallow this error, as there's really no need to show the user.
                                // The Attendee (Group Member Assignment) record itself will be deleted, but we cannot delete the underlying GroupMember record.
                            }
                            else
                            {
                                shouldDelete = true;
                                shouldRebindGrid = true;
                            }
                        }

                        if ( shouldArchive )
                        {
                            // NOTE: Delete will AutoArchive, but since we know that we need to archive, we can call .Archive directly
                            groupMemberService.Archive( groupMember, this.CurrentPersonAliasId, false );
                        }
                        else if ( shouldDelete )
                        {
                            groupMemberService.Delete( groupMember );
                        }
                    }
                }

                rockContext.SaveChanges();
            }

            if ( shouldRebindGrid )
            {
                BindAttendeesGrid();
            }
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Gets the shared group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Group GetSharedGroup( RockContext rockContext )
        {
            var key = $"Group:{_groupId}";
            var group = RockPage.GetSharedItem( key ) as Group;

            if ( group == null && _groupId > 0 )
            {
                group = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( g => g.Campus )
                    .Include( g => g.GroupSyncs )
                    .Include( g => g.GroupType )
                    .Include( g => g.ParentGroup ) // ParentGroup may be needed for a proper authorization check.
                    .FirstOrDefault( g => g.Id == _groupId );

                RockPage.SaveSharedItem( key, group );
            }

            if ( group != null )
            {
                _groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );
                _groupMembersWithGroupMemberHistory = new HashSet<int>(
                    new GroupMemberHistoricalService( rockContext )
                        .Queryable()
                        .Where( a => a.GroupId == _groupId )
                        .Select( a => a.GroupMemberId )
                        .ToList()
                );

                _groupTypeRoleIdsWithGroupSync = new HashSet<int>(
                    group.GroupSyncs
                        .Select( a => a.GroupTypeRoleId )
                        .ToList()
                );
            }

            return group;
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        /// <param name="group">The group.</param>
        private void InitializeGrid( Group group )
        {
            // Prevent the "Launch Workflow" button from showing for now; this needs a bit more thought: what exactly will we send a workflow from this grid?
            //gAttendees.EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP_MEMBER_ASSIGNMENT ).Id;

            gAttendees.PersonIdField = "PersonId";
            gAttendees.ExportFilename = group.Name;
            gAttendees.GetRecipientMergeFields += gAttendees_GetRecipientMergeFields;
            gAttendees.Actions.AddClick += gAttendees_AddClick;

            // we'll have custom JavaScript (see SignUpOpportunityAttendeeList.ascx ) do this instead.
            gAttendees.ShowConfirmDeleteDialog = false;

            gAttendees.Actions.ShowAdd = _canManageMembers;
            gAttendees.IsDeleteEnabled = _canManageMembers;

            gfAttendees.PreferenceKeyPrefix = $"{_groupId}-{_locationId}-{_scheduleId}-";
            SetGridFilters( group );
        }

        /// <summary>
        /// Resets the message boxes.
        /// </summary>
        private void ResetMessageBoxes()
        {
            nbMissingIds.Visible = false;
            nbNotFoundOrArchived.Visible = false;
            nbNotAuthorizedToView.Visible = false;
            nbInvalidGroupType.Visible = false;
            nbOpportunityNotFound.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Shows the missing ids message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMissingIdsMessage( string message )
        {
            nbMissingIds.Text = message;
            nbMissingIds.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the group not found message.
        /// </summary>
        private void ShowGroupNotFoundMessage()
        {
            nbNotFoundOrArchived.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the not authorized to view message.
        /// </summary>
        private void ShowNotAuthorizedToViewMessage()
        {
            nbNotAuthorizedToView.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the invalid group type message.
        /// </summary>
        private void ShowInvalidGroupTypeMessage()
        {
            nbInvalidGroupType.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the opportunity not found message.
        /// </summary>
        private void ShowOpportunityNotFoundMessage()
        {
            nbOpportunityNotFound.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            ResetMessageBoxes();

            if ( !EnsureRequiredIdsAreProvided() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var group = GetSharedGroup( rockContext );
                if ( group == null )
                {
                    ShowGroupNotFoundMessage();
                    return;
                }

                if ( !EnsureGroupIsAllowed( group ) )
                {
                    return;
                }

                var opportunity = GetOpportunity( rockContext, group );
                if ( opportunity == null )
                {
                    ShowOpportunityNotFoundMessage();
                    return;
                }

                InitializeSummary( opportunity );

                BindAttendeesGrid( opportunity );
            }
        }

        /// <summary>
        /// Ensures the required ids are provided.
        /// </summary>
        /// <returns></returns>
        private bool EnsureRequiredIdsAreProvided()
        {
            var missingIds = new List<string>();
            if ( _groupId <= 0 )
            {
                missingIds.Add( "Group ID" );
            }

            if ( _locationId <= 0 )
            {
                missingIds.Add( "Location ID" );
            }

            if ( _scheduleId <= 0 )
            {
                missingIds.Add( "Schedule ID" );
            }

            if ( missingIds.Any() )
            {
                ShowMissingIdsMessage( $"The following required ID{( missingIds.Count > 1 ? "s were" : " was" )} not provided: {string.Join( ", ", missingIds )}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures the group is allowed.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private bool EnsureGroupIsAllowed( Group group )
        {
            if ( !_canView )
            {
                ShowNotAuthorizedToViewMessage();
                return false;
            }

            if ( group.GroupTypeId != this.SignUpGroupTypeId && group.GroupType.InheritedGroupTypeId != this.SignUpGroupTypeId )
            {
                ShowInvalidGroupTypeMessage();
                return false;
            }

            return true;
        }

        private class Opportunity
        {
            private int? SlotsMin => Config?.MinimumCapacity;
            private int? SlotsDesired => Config?.DesiredCapacity;
            private int? SlotsMax => Config?.MaximumCapacity;

            public int SlotsFilled { get; set; }

            public Group Group { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public List<GroupMemberAssignment> Attendees { get; set; }

            public string Name
            {
                get
                {
                    var title = Group?.Name;

                    if ( !string.IsNullOrWhiteSpace( Config?.ConfigurationName ) )
                    {
                        title = Config.ConfigurationName;
                    }

                    var scheduleName = Schedule?.ScheduleType == ScheduleType.Named
                        && !string.IsNullOrWhiteSpace( Schedule?.Name )
                            ? Schedule.Name
                            : string.Empty;

                    var separator = !string.IsNullOrWhiteSpace( title )
                        && !string.IsNullOrWhiteSpace( scheduleName )
                            ? " - "
                            : string.Empty;

                    return $"{title}{separator}{scheduleName}";
                }
            }

            public string ConfiguredSlots
            {
                get
                {
                    return string.Join( " | ", new List<string>
                    {
                        SlotsMin.GetValueOrDefault().ToString("N0"),
                        SlotsDesired.GetValueOrDefault().ToString("N0"),
                        SlotsMax.GetValueOrDefault().ToString("N0"),
                    } );
                }
            }

            private class ProgressState
            {
                public const string Success = "success";
                public const string Warning = "warning";
                public const string Critical = "critical";
                public const string Danger = "danger";
            }

            public string SlotsFilledBadgeType
            {
                get
                {
                    var min = this.SlotsMin.GetValueOrDefault();
                    var desired = this.SlotsDesired.GetValueOrDefault();
                    var max = this.SlotsMax.GetValueOrDefault();
                    var filled = this.SlotsFilled;

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
        }

        /// <summary>
        /// Gets the opportunity.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private Opportunity GetOpportunity( RockContext rockContext, Group group = null )
        {
            group = group ?? GetSharedGroup( rockContext );

            var groupLocation = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .FirstOrDefault( gl => gl.GroupId == _groupId && gl.LocationId == _locationId );

            if ( groupLocation == null
                || groupLocation.Location == null
                || !groupLocation.Schedules.Any( s => s.Id == _scheduleId ) )
            {
                return null;
            }

            var schedule = groupLocation.Schedules.First( s => s.Id == _scheduleId );
            var config = groupLocation.GroupLocationScheduleConfigs.First( c => c.GroupLocationId == groupLocation.Id && c.ScheduleId == _scheduleId );

            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var qry = groupMemberAssignmentService.Queryable()
                .AsNoTracking()
                .Include( gma => gma.GroupMember )
                .Include( gma => gma.GroupMember.GroupRole )
                .Include( gma => gma.GroupMember.Person )
                .Where( gma =>
                    gma.GroupMember.GroupId == _groupId
                    && gma.LocationId == _locationId
                    && gma.ScheduleId == _scheduleId
                );

            var slotsFilled = qry.Count( gma => !gma.GroupMember.Person.IsDeceased );
            bSlotsFilled.Text = slotsFilled.ToString( "N0" );

            if ( _isCommunicating )
            {
                qry = qry.Where( gma => gma.GroupMember.GroupMemberStatus != GroupMemberStatus.Inactive );
            }

            // Filter by first name.
            var firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( gma =>
                    gma.GroupMember.Person.FirstName.StartsWith( firstName ) ||
                    gma.GroupMember.Person.NickName.StartsWith( firstName ) );
            }

            // Filter by last name.
            var lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( gma => gma.GroupMember.Person.LastName.StartsWith( lastName ) );
            }

            // Filter by role.
            var validGroupTypeRoles = _groupTypeCache.Roles.Select( r => r.Id ).ToList();
            var roles = new List<int>();
            foreach ( var roleId in cblRole.SelectedValues.AsIntegerList() )
            {
                if ( validGroupTypeRoles.Contains( roleId ) )
                {
                    roles.Add( roleId );
                }
            }

            if ( roles.Any() )
            {
                qry = qry.Where( gma => roles.Contains( gma.GroupMember.GroupRoleId ) );
            }

            // Filter by GroupMemberStatus.
            var statuses = new List<GroupMemberStatus>();
            foreach ( var status in cblGroupMemberStatus.SelectedValues )
            {
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    statuses.Add( status.ConvertToEnum<GroupMemberStatus>() );
                }
            }

            if ( statuses.Any() )
            {
                qry = qry.Where( gma => statuses.Contains( gma.GroupMember.GroupMemberStatus ) );
            }

            // Filter by Campus.
            if ( cpCampusFilter.SelectedCampusId.HasValue )
            {
                var campusId = cpCampusFilter.SelectedCampusId.Value;
                var qryCampusMembers = new GroupMemberService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( gm => gm.Group.GroupTypeId == this.FamilyGroupTypeId && gm.Group.CampusId == campusId );

                qry = qry.Where( gma => qryCampusMembers.Any( cm => cm.PersonId == gma.GroupMember.PersonId ) );
            }

            // Filter by gender.
            var genders = new List<Gender>();
            foreach ( var selectedGender in cblGenderFilter.SelectedValues )
            {
                var gender = selectedGender.ConvertToEnum<Gender>();
                genders.Add( gender );
            }

            if ( genders.Any() )
            {
                qry = qry.Where( gma => genders.Contains( gma.GroupMember.Person.Gender ) );
            }

            if ( MemberAttributes?.Any() == true )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMemberQry = groupMemberService.Queryable()
                    .AsNoTracking()
                    .Where( gm =>
                        gm.GroupId == _groupId
                    );

                foreach ( var attribute in MemberAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( $"attributeFilter_{attribute.Id}" );
                    if ( filterControl != null )
                    {
                        groupMemberQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( groupMemberQry, filterControl, attribute, groupMemberService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }

                qry = qry.Where( gma => groupMemberQry.Contains( gma.GroupMember ) );
            }

            if ( MemberOpportunityAttributes?.Any() == true )
            {
                foreach ( var attribute in MemberOpportunityAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( $"attributeFilter_{attribute.Id}" );
                    if ( filterControl != null )
                    {
                        qry = attribute.FieldType.Field.ApplyAttributeQueryFilter( qry, filterControl, attribute, groupMemberAssignmentService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }
            }

            // Take note of any group requirements (and any members not yet meeting them).
            _hasGroupRequirements = new GroupRequirementService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gr =>
                    ( gr.GroupId.HasValue && gr.GroupId == _groupId ) ||
                    ( gr.GroupTypeId.HasValue && gr.GroupTypeId == _groupTypeCache.Id ) )
                .Any();

            _unmetRequirementTypesByGroupMemberId = new GroupService( rockContext )
                .GroupMembersNotMeetingRequirements( group, true, true )
                .Select( kvp => new
                {
                    GroupMemberId = kvp.Key.Id,
                    GroupRequirementType = kvp.Value.Select(
                        kvpInner => ( ( PersonGroupRequirementStatus ) kvpInner.Key )?.GroupRequirement?.GroupRequirementType
                    )
                } )
                .ToDictionary( x => x.GroupMemberId, x => x.GroupRequirementType.ToList() ); // Dictionary<int, List<GroupRequirementType>>

            _groupMemberIdsNotMeetingRequirements = new HashSet<int>(
                _unmetRequirementTypesByGroupMemberId
                    .Select( kvp => kvp.Key )
                    .Distinct()
                    .ToList()
            );

            if ( _isExporting )
            {
                if ( !this.FamilyGroupTypeId.HasValue )
                {
                    _personIdPhoneNumberTypePhoneNumberLookup = new Dictionary<int, Dictionary<int, string>>();
                    _personIdHomeLocationLookup = new Dictionary<int, Location>();
                }
                else
                {
                    var personFamily = new GroupMemberService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( gm => qry.Any( gma => gma.GroupMember.PersonId == gm.PersonId ) )
                        .Where( gm => gm.Group.GroupTypeId == this.FamilyGroupTypeId )
                        .Select( gm => new
                        {
                            gm.PersonId,
                            gm.Group
                        } );

                    // Preload all phone numbers for members in the query.
                    _personIdPhoneNumberTypePhoneNumberLookup = new PhoneNumberService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( n => personFamily.Any( pf => pf.PersonId == n.PersonId ) && n.NumberTypeValueId.HasValue )
                        .GroupBy( n => new
                        {
                            n.PersonId,
                            n.NumberTypeValueId
                        } )
                        .Select( a => new
                        {
                            a.Key.PersonId,
                            a.Key.NumberTypeValueId,
                            NumberFormatted = a.Select( n => n.NumberFormatted ).FirstOrDefault()
                        } )
                        .GroupBy( a => a.PersonId )
                        .ToDictionary( k => k.Key, v => v.ToDictionary( xk => xk.NumberTypeValueId.Value, xv => xv.NumberFormatted ) );

                    if ( this.HomeAddressDefinedValue == null )
                    {
                        _personIdHomeLocationLookup = new Dictionary<int, Location>();
                    }
                    else
                    {
                        // Preload all mapped home locations for members in the query.
                        var locationsQry = personFamily
                            .Select( pf => new
                            {
                                HomeLocation = pf.Group.GroupLocations
                                    .Where( l => l.GroupLocationTypeValueId == this.HomeAddressDefinedValue.Id && l.IsMappedLocation )
                                    .Select( l => l.Location ).FirstOrDefault(),
                                GroupOrder = pf.Group.Order,
                                pf.PersonId
                            } );

                        _personIdHomeLocationLookup = locationsQry
                            .GroupBy( a => a.PersonId )
                            .ToDictionary( k => k.Key, v => v.OrderBy( a => a.GroupOrder )
                                .Select( x => x.HomeLocation ).FirstOrDefault() );
                    }
                }
            }

            if ( gAttendees.SortProperty != null )
            {
                qry = qry.Sort( gAttendees.SortProperty );
            }
            else
            {
                qry = qry
                    .OrderBy( gma => gma.GroupMember.Person.LastName )
                    .ThenBy( gma => gma.GroupMember.Person.AgeClassification )
                    .ThenBy( gma => gma.GroupMember.Person.Gender );
            }

            var groupMemberAssignments = qry.ToList();

            groupMemberAssignments.LoadAttributes();
            groupMemberAssignments.Select( gma => gma.GroupMember ).LoadAttributes();

            this.GroupMemberIdByGroupMemberAssignmentIds = new Dictionary<int, int>();

            foreach ( var groupMemberAssignment in groupMemberAssignments )
            {
                this.GroupMemberIdByGroupMemberAssignmentIds.Add( groupMemberAssignment.Id, groupMemberAssignment.GroupMemberId );

                // Conflate the group member assignment's and group member's attributes (and values) collections
                // into shared collections on the group member assignment instance so the grid can find them all
                // for dynamic columns. Also, differentiate their keys with a prefix to prevent the possibility
                // of key collisions between the two collections.
                var attributes = new Dictionary<string, AttributeCache>();
                var attributeValues = new Dictionary<string, AttributeValueCache>();
                var keyPrefix = GridFilterKey.MemberAttribute;

                foreach ( var attribute in groupMemberAssignment.GroupMember.Attributes )
                {
                    attributes.Add( $"{keyPrefix}-{attribute.Key}", attribute.Value );
                }

                foreach ( var attributeValue in groupMemberAssignment.GroupMember.AttributeValues )
                {
                    attributeValues.Add( $"{keyPrefix}-{attributeValue.Key}", attributeValue.Value );
                }

                keyPrefix = GridFilterKey.MemberOpportunityAttribute;

                foreach ( var attribute in groupMemberAssignment.Attributes )
                {
                    attributes.Add( $"{keyPrefix}-{attribute.Key}", attribute.Value );
                }

                foreach ( var attributeValue in groupMemberAssignment.AttributeValues )
                {
                    attributeValues.Add( $"{keyPrefix}-{attributeValue.Key}", attributeValue.Value );
                }

                groupMemberAssignment.Attributes = attributes;
                groupMemberAssignment.AttributeValues = attributeValues;
            }

            return new Opportunity
            {
                SlotsFilled = slotsFilled,
                Group = group,
                Location = groupLocation.Location,
                Schedule = schedule,
                Config = config,
                Attendees = groupMemberAssignments
            };
        }

        /// <summary>
        /// Initializes the summary.
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        private void InitializeSummary( Opportunity opportunity )
        {
            lTitle.Text = opportunity.Name;
            lLocation.Text = opportunity.Location.ToString();
            lSchedule.Text = opportunity.Schedule.ToFriendlyScheduleText( true );
            lConfiguredSlots.Text = opportunity.ConfiguredSlots;
            bSlotsFilled.Text = opportunity.SlotsFilled.ToString( "N0" );
            bSlotsFilled.BadgeType = opportunity.SlotsFilledBadgeType;

            InitializeLabels( opportunity.Group );
        }

        /// <summary>
        /// Initializes the labels.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private void InitializeLabels( Group group )
        {
            hlGroupType.Text = group.GroupType?.Name;

            if ( group.Campus != null )
            {
                hlCampus.Text = group.Campus.Name;
                hlCampus.Visible = true;
            }
            else
            {
                hlCampus.Text = string.Empty;
                hlCampus.Visible = false;
            }

            hlInactive.Visible = !group.IsActive;
        }

        /// <summary>
        /// Resolves the CheckBox list values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <returns></returns>
        private string ResolveCheckBoxListValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( var value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        /// <summary>
        /// Sets the grid filters.
        /// </summary>
        /// <param name="group">The group.</param>
        private void SetGridFilters( Group group )
        {
            tbFirstName.Text = gfAttendees.GetFilterPreference( GridFilterKey.FirstName );
            tbLastName.Text = gfAttendees.GetFilterPreference( GridFilterKey.LastName );

            if ( group != null )
            {
                cblRole.DataSource = group.GroupType.Roles.OrderBy( r => r.Order ).ToList();
                cblRole.DataBind();
            }

            var roleValue = gfAttendees.GetFilterPreference( GridFilterKey.Role );
            if ( !string.IsNullOrWhiteSpace( roleValue ) )
            {
                cblRole.SetValues( roleValue.Split( ';' ).ToList() );
            }

            cblGroupMemberStatus.BindToEnum<GroupMemberStatus>();

            var statusValue = gfAttendees.GetFilterPreference( GridFilterKey.Status );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblGroupMemberStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }

            cpCampusFilter.Campuses = CampusCache.All();
            cpCampusFilter.SelectedCampusId = gfAttendees.GetFilterPreference( "Campus" ).AsIntegerOrNull();

            string genderValue = gfAttendees.GetFilterPreference( GridFilterKey.Gender );
            if ( !string.IsNullOrWhiteSpace( genderValue ) )
            {
                cblGenderFilter.SetValues( genderValue.Split( ';' ).ToList() );
            }
            else
            {
                cblGenderFilter.ClearSelection();
            }

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            MemberAttributes = new List<AttributeCache>();
            MemberOpportunityAttributes = new List<AttributeCache>();

            if ( _groupId <= 0 )
            {
                return;
            }

            var rockContext = new RockContext();

            foreach ( var attribute in ( new GroupMember { GroupId = _groupId } ).GetInheritedAttributes( rockContext )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList() )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    MemberAttributes.Add( attribute );
                }
            }

            var entityTypeId = new GroupMember().TypeId;
            var groupQualifier = _groupId.ToString();
            var attributeService = new AttributeService( rockContext );

            foreach ( var attribute in attributeService.GetByEntityTypeQualifier( entityTypeId, "GroupId", groupQualifier, true )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList() )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    MemberAttributes.Add( attribute );
                }
            }

            entityTypeId = new GroupMemberAssignment().TypeId;

            foreach ( var attribute in attributeService.GetByEntityTypeQualifier( entityTypeId, "GroupId", groupQualifier, true )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList() )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    MemberOpportunityAttributes.Add( attribute );
                }
            }
        }

        /// <summary>
        /// Adds dynamic grid filters and columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls.
            phAttributeFilters.Controls.Clear();

            // Clear dynamic controls so we can re-add them.
            RemoveAttributeAndButtonColumns();

            AddAttributeFiltersAndColumns( MemberAttributes, GridFilterKey.MemberAttribute );
            AddAttributeFiltersAndColumns( MemberOpportunityAttributes, GridFilterKey.MemberOpportunityAttribute );

            AddRowButtonsToEnd();
        }

        /// <summary>
        /// Removes dynamic attribute and button columns.
        /// </summary>
        private void RemoveAttributeAndButtonColumns()
        {
            // Remove delete button column.
            DataControlField buttonColumn = gAttendees.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gAttendees.Columns.Remove( buttonColumn );
            }

            // Remove person profile link column.
            buttonColumn = gAttendees.Columns.OfType<HyperLinkField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gAttendees.Columns.Remove( buttonColumn );
            }

            // Remove attribute columns.
            foreach ( var column in gAttendees.Columns.OfType<AttributeField>().ToList() )
            {
                gAttendees.Columns.Remove( column );
            }
        }

        /// <summary>
        /// Adds dynamic attribute filters and columns.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="gridFilterKeyPrefix">The grid filter key prefix for the attributes.</param>
        private void AddAttributeFiltersAndColumns( List<AttributeCache> attributes, string gridFilterKeyPrefix )
        {
            if ( attributes?.Any() != true )
            {
                return;
            }

            foreach ( var attribute in attributes )
            {
                var attributeKey = $"{gridFilterKeyPrefix}-{attribute.Key}";
                var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, $"attributeFilter_{attribute.Id}", false, Rock.Reporting.FilterMode.SimpleFilter );
                if ( control != null )
                {
                    if ( control is IRockControl rockControl )
                    {
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        phAttributeFilters.Controls.Add( control );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = $"{control.ID}_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.Help = attribute.Description;
                        wrapper.Controls.Add( control );
                        phAttributeFilters.Controls.Add( wrapper );
                    }

                    var savedValue = gfAttendees.GetFilterPreference( attributeKey );
                    if ( savedValue.IsNotNullOrWhiteSpace() )
                    {
                        try
                        {
                            var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                            attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                        }
                        catch
                        {
                            // Intentionally ignore.
                        }
                    }
                }

                var columnExists = gAttendees.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField
                    {
                        DataField = attributeKey,
                        AttributeId = attribute.Id,
                        HeaderText = attribute.Name
                    };

                    boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                    gAttendees.Columns.Add( boundField );
                }
            }
        }

        /// <summary>
        /// Adds the grid row buttons.
        /// </summary>
        private void AddRowButtonsToEnd()
        {
            var personProfileLinkField = new PersonProfileLinkField();
            personProfileLinkField.LinkedPageAttributeKey = AttributeKey.PersonProfilePage;
            gAttendees.Columns.Add( personProfileLinkField );

            _deleteField = new DeleteField();
            _deleteField.Click += DeleteOrArchiveGroupMember_Click;
            gAttendees.Columns.Add( _deleteField );
        }

        /// <summary>
        /// Binds the attendees grid.
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        /// <param name="isCommunicating">if set to <c>true</c> [is communicating].</param>
        /// <param name="isExporting">if set to <c>true</c> [is exporting].</param>
        private void BindAttendeesGrid( Opportunity opportunity = null, bool isCommunicating = false, bool isExporting = false )
        {
            _isCommunicating = isCommunicating;
            _isExporting = isExporting;

            if ( opportunity == null )
            {
                using ( var rockContext = new RockContext() )
                {
                    opportunity = GetOpportunity( rockContext );
                }
            }

            gAttendees.DataSource = opportunity.Attendees;
            gAttendees.DataBind();
        }

        /// <summary>
        /// Gets the unmet requirements tooltip identifier.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <returns></returns>
        private string GetUnmetRequirementsTooltipId( int groupMemberId )
        {
            return $"unmet-group-requirements-tooltip-{groupMemberId}";
        }

        /// <summary>
        /// Gets the unmet requirements tooltip.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <returns></returns>
        private string GetUnmetRequirementsTooltip( int groupMemberId )
        {
            var friendlyNames = new List<string>();

            foreach ( var unmetRequirementTypes in _unmetRequirementTypesByGroupMemberId.Where( kvp => kvp.Key == groupMemberId ) )
            {
                foreach ( var groupRequirementType in unmetRequirementTypes.Value )
                {
                    if ( groupRequirementType == null )
                    {
                        continue;
                    }

                    var friendlyName = groupRequirementType.NegativeLabel;
                    if ( string.IsNullOrWhiteSpace( friendlyName ) )
                    {
                        friendlyName = groupRequirementType?.Name;
                    }

                    if ( !string.IsNullOrWhiteSpace( friendlyName ) )
                    {
                        friendlyNames.Add( friendlyName );
                    }
                }
            }

            if ( !friendlyNames.Any() )
            {
                return string.Empty;
            }

            var tooltipHtmlSb = new StringBuilder( $"<div id='{GetUnmetRequirementsTooltipId( groupMemberId )}' class='hide'>" );
            tooltipHtmlSb.Append( "<p class='text-center'><strong>Unmet Group Requirements</strong></><ul>" );

            foreach ( var friendlyName in friendlyNames )
            {
                tooltipHtmlSb.Append( $"<li>{friendlyName}</li>" );
            }

            tooltipHtmlSb.Append( "</ul></div>" );

            return tooltipHtmlSb.ToString();
        }

        /// <summary>
        /// Navigates to group member detail page.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void NavigateToGroupMemberDetailPage( int? groupMemberId = null )
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.GroupMemberDetailPage ) ) )
            {
                var qryParams = new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, _groupId.ToString() },
                    { PageParameterKey.LocationId, _locationId.ToString() },
                    { PageParameterKey.ScheduleId, _scheduleId.ToString() },
                    { PageParameterKey.GroupMemberId, groupMemberId.GetValueOrDefault().ToString() },
                };

                NavigateToLinkedPage( AttributeKey.GroupMemberDetailPage, qryParams );
            }
        }

        #endregion
    }
}
