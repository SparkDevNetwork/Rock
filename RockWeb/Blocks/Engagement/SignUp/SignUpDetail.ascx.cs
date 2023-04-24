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
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Engagement.SignUp
{

    [DisplayName( "Sign-Up Detail" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Displays details about the scheduled opportunities for a given project group." )]

    #region Block Attributes

    [LinkedPage( "Sign-Up Opportunity Attendee List Page",
        Key = AttributeKey.SignUpOpportunityAttendeeListPage,
        Description = "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.",
        IsRequired = false,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "69F5C6BD-7A22-42FE-8285-7C8E586E746A" )]
    public partial class SignUpDetail : RockBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ExpandedIds = "ExpandedIds";
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ParentGroupId = "ParentGroupId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string ProjectType = "ProjectType";
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        private static class ViewStateKey
        {
            public const string EditGroupLocationId = "EditGroupLocationId";
            public const string EditScheduleId = "EditScheduleId";
            public const string GroupId = "GroupId";
            public const string GroupTypeId = "GroupTypeId";
            public const string GroupRequirementsState = "GroupRequirementsState";
            public const string IsAuthorizedToEdit = "IsAuthorizedToEdit";
            public const string IsProjectTypeInPerson = "IsProjectTypeInPerson";
            public const string OpportunitiesState = "OpportunitiesState";
            public const string ProjectName = "ProjectName";
            public const string ProjectTypeHelpText = "ProjectTypeHelpText";
        }

        private static class DialogKey
        {
            public const string AddOpportunity = "AddOpportunity";
            public const string GroupRequirements = "GroupRequirements";
        }

        private static class DataKeyName
        {
            public const string GroupId = "GroupId";
            public const string GroupLocationId = "GroupLocationId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        #endregion

        #region Fields

        private bool _canEdit;
        private bool _isProjectTypeInPerson;

        #endregion

        #region Properties

        private int GroupId
        {
            get
            {
                return ViewState[ViewStateKey.GroupId].ToIntSafe();
            }
            set
            {
                ViewState[ViewStateKey.GroupId] = value;
            }
        }

        private int? PageParentGroupId
        {
            get
            {
                return PageParameter( PageParameterKey.ParentGroupId ).AsIntegerOrNull();
            }
        }

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

        private int GroupTypeId
        {
            get
            {
                return ViewState[ViewStateKey.GroupTypeId].ToIntSafe();
            }
            set
            {
                ViewState[ViewStateKey.GroupTypeId] = value;
            }
        }

        private GroupTypeCache CurrentGroupType
        {
            get
            {
                return this.GroupTypeId > 0
                    ? GroupTypeCache.Get( this.GroupTypeId )
                    : null;
            }
        }

        private string CurrentGroupTypeUrl
        {
            get
            {
                return this.ResolveUrl( $"~/GroupType/{this.GroupTypeId}" );
            }
        }

        private ScheduleType AllowedScheduleTypes
        {
            get
            {
                return this.CurrentGroupType.AllowedScheduleTypes;
            }
        }

        private bool AnyAllowedScheduleTypes
        {
            get
            {
                /*
                 * 12/16/2022 - JPH
                 * 
                 * For the first release of this feature, we are allowing at most "Custom" and/or "Named" schedule types.
                 * 
                 * Reason: Sign-Up Groups / short term serving projects - simplified UI (minus weekly schedule types).
                 */
                return this.AllowedScheduleTypes.HasFlag( ScheduleType.Custom ) || this.CurrentGroupType.AllowedScheduleTypes.HasFlag( ScheduleType.Named );
            }
        }

        private LocationPickerMode AllowedLocationPickerModes
        {
            get
            {
                var allowedPickerModes = LocationPickerMode.None;
                var groupTypeModes = this.CurrentGroupType.LocationSelectionMode;

                if ( groupTypeModes.HasFlag( GroupLocationPickerMode.Address ) )
                {
                    allowedPickerModes |= LocationPickerMode.Address;
                }

                if ( groupTypeModes.HasFlag( GroupLocationPickerMode.Named ) )
                {
                    allowedPickerModes |= LocationPickerMode.Named;
                }

                if ( groupTypeModes.HasFlag( GroupLocationPickerMode.Point ) )
                {
                    allowedPickerModes |= LocationPickerMode.Point;
                }

                if ( groupTypeModes.HasFlag( GroupLocationPickerMode.Polygon ) )
                {
                    allowedPickerModes |= LocationPickerMode.Polygon;
                }

                return allowedPickerModes;
            }
        }

        private bool AnyAllowedLocationPickerModes
        {
            get
            {
                return this.AllowedLocationPickerModes != LocationPickerMode.None;
            }
        }

        private string ProjectName
        {
            get
            {
                return ViewState[ViewStateKey.ProjectName]?.ToString();
                ;
            }
            set
            {
                ViewState[ViewStateKey.ProjectName] = value;
            }
        }

        private int EditGroupLocationId
        {
            get
            {
                return ViewState[ViewStateKey.EditGroupLocationId].ToIntSafe();
            }
            set
            {
                ViewState[ViewStateKey.EditGroupLocationId] = value;
            }
        }

        private int EditScheduleId
        {
            get
            {
                return ViewState[ViewStateKey.EditScheduleId].ToIntSafe();
            }
            set
            {
                ViewState[ViewStateKey.EditScheduleId] = value;
            }
        }

        private List<GroupRequirement> GroupRequirementsState { get; set; }

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

            _canEdit = ( bool ) ViewState[ViewStateKey.IsAuthorizedToEdit];
            gOpportunities.Actions.ShowAdd = _canEdit;

            _isProjectTypeInPerson = ( bool ) ViewState[ViewStateKey.IsProjectTypeInPerson];

            rblProjectType.Help = ViewState[ViewStateKey.ProjectTypeHelpText]?.ToString();

            var json = ViewState[ViewStateKey.GroupRequirementsState] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                this.GroupRequirementsState = new List<GroupRequirement>();
            }
            else
            {
                this.GroupRequirementsState = JsonConvert.DeserializeObject<List<GroupRequirement>>( json ) ?? new List<GroupRequirement>();
            }

            // Get any GroupRole records from the database that weren't serialized.
            var groupRoleIds = this.GroupRequirementsState
                .Where( r => r.GroupRoleId.HasValue && r.GroupRole == null )
                .Select( r => r.GroupRoleId.Value )
                .Distinct()
                .ToList();

            if ( groupRoleIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupRoles = new GroupTypeRoleService( rockContext ).GetByIds( groupRoleIds );
                    this.GroupRequirementsState.ForEach( r =>
                    {
                        if ( r.GroupRoleId.HasValue )
                        {
                            r.GroupRole = groupRoles.FirstOrDefault( gr => gr.Id == r.GroupRoleId );
                        }
                    } );
                }
            }

            json = ViewState[ViewStateKey.OpportunitiesState] as string;
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
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            nbNotAuthorizedToView.Text = EditModeMessage.NotAuthorizedToView( Group.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Group ) ).Id;

            gGroupRequirements.Actions.AddClick += gGroupRequirements_Add;
            gGroupRequirements.EmptyDataText = Server.HtmlEncode( None.Text );

            gOpportunities.Actions.AddClick += gOpportunities_Add;
            gOpportunities.EmptyDataText = Server.HtmlEncode( None.Text );

            // we'll have custom JavaScript (see SignUpDetail.ascx ) do this instead.
            gOpportunities.ShowConfirmDeleteDialog = false;

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSignUpDetail );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int? groupId = 0;
            if ( !string.IsNullOrWhiteSpace( PageParameter( PageParameterKey.GroupId ) ) )
            {
                groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            }

            if ( !Page.IsPostBack )
            {
                if ( groupId.HasValue )
                {
                    ShowDetails( groupId.Value, false );
                }
                else
                {
                    ShowGroupNotFoundMessage();
                }
            }
            else if ( groupId.HasValue && groupId.Value != 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).GetNoTracking( groupId.Value );
                    if ( group != null )
                    {
                        FollowingsHelper.SetFollowing( group, pnlFollowing, this.CurrentPerson );
                    }
                }
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
            ViewState[ViewStateKey.IsAuthorizedToEdit] = _canEdit;
            ViewState[ViewStateKey.IsProjectTypeInPerson] = _isProjectTypeInPerson;

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[ViewStateKey.ProjectTypeHelpText] = rblProjectType.Help;
            ViewState[ViewStateKey.GroupRequirementsState] = JsonConvert.SerializeObject( this.GroupRequirementsState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.OpportunitiesState] = JsonConvert.SerializeObject( this.OpportunitiesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            this.GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0;

            SetIsCampusRequired();

            using ( var rockContext = new RockContext() )
            {
                BuildGroupRequirementsList( true, rockContext );

                Group group;
                if ( this.GroupId == 0 )
                {
                    group = new Group();
                }
                else
                {
                    group = new GroupService( rockContext ).GetNoTracking( this.GroupId );
                }

                group.GroupTypeId = this.GroupTypeId;
                group.LoadAttributes();

                BuildEditModeAttributesControls( group );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblProjectType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblProjectType_SelectedIndexChanged( object sender, EventArgs e )
        {
            _isProjectTypeInPerson = GetIsProjectTypeInPerson( rblProjectType.SelectedValue );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ResetNotificationBoxes();

            Group group = null;

            using ( var rockContext = new RockContext() )
            {
                var isNewGroup = false;
                var groupService = new GroupService( rockContext );
                var groupRequirementService = new GroupRequirementService( rockContext );

                if ( this.GroupId == 0 )
                {
                    isNewGroup = true;
                    group = new Group();
                    groupService.Add( group );
                }
                else
                {
                    group = groupService.Queryable()
                        .Include( g => g.ParentGroup ) // Parent group is needed to properly check for edit authorization.
                        .Include( g => g.GroupRequirements )
                        .FirstOrDefault( g => g.Id == this.GroupId );

                    // Make sure the user is authorized to edit the retrieved group with the existing parent group and group type.
                    if ( !IsAuthorizedToEdit( group ) )
                    {
                        ShowNotAuthorizedToEditMessage();
                        return;
                    }
                }

                var parentGroup = group.ParentGroup;
                if ( parentGroup == null && this.PageParentGroupId.HasValue )
                {
                    parentGroup = groupService.Get( this.PageParentGroupId.Value );
                }

                if ( parentGroup == null )
                {
                    ShowParentNotFoundMessage();
                    return;
                }

                // Re-check edit authorization with new parent group and group type assigned.
                group.ParentGroup = parentGroup;
                group.ParentGroupId = parentGroup.Id;
                group.GroupTypeId = this.GroupTypeId;

                if ( !IsAuthorizedToEdit( group ) )
                {
                    ShowNotAuthorizedToEditMessage();
                    return;
                }

                // Ensure the group type is allowed.
                var allowedGroupTypeIds = GetAllowedGroupTypeIds( group.ParentGroup.GroupTypeId );
                if ( !allowedGroupTypeIds.Contains( this.GroupTypeId ) )
                {
                    nbGroupTypeNotAllowed.Text = $"The {group.ParentGroup.Name} group does not allow child groups with a {this.CurrentGroupType.Name} group type.";
                    nbGroupTypeNotAllowed.Visible = true;

                    this.GroupTypeId = allowedGroupTypeIds.FirstOrDefault();
                    InitializeGroupTypesDropDownList( group, rockContext );
                    return;
                }

                group.GroupType = new GroupTypeService( rockContext ).Get( this.GroupTypeId );
                group.Name = tbName.Text;
                group.IsActive = cbIsActive.Checked;
                group.Description = tbDescription.Text;

                int? campusId = cpCampus.SelectedCampusId;
                if ( !campusId.HasValue && group.GroupType?.GroupsRequireCampus == true )
                {
                    // If the CampusPicker doesn't have a selected value AND there is only one campus, grab its ID from the cache.
                    campusId = CampusCache.SingleCampusId;
                }

                group.CampusId = campusId;

                avcEditAttributes.GetEditValues( group );

                var hasProjectTypeAttribute = false;
                if ( group.Attributes.ContainsKey( AttributeKey.ProjectType ) )
                {
                    group.SetAttributeValue( AttributeKey.ProjectType, rblProjectType.SelectedValue );
                    hasProjectTypeAttribute = true;
                }

                if ( hasProjectTypeAttribute && _isProjectTypeInPerson )
                {
                    group.ReminderSystemCommunicationId = ddlReminderCommunication.SelectedValue.AsIntegerOrNull();
                    group.ReminderOffsetDays = nbReminderOffsetDays.Text.AsIntegerOrNull();
                    group.ReminderAdditionalDetails = htmlReminderAddlDetails.Text;
                }
                else
                {
                    group.ReminderSystemCommunicationId = default;
                    group.ReminderOffsetDays = default;
                    group.ReminderAdditionalDetails = default;
                }

                group.ConfirmationAdditionalDetails = htmlConfirmationDetails.Text;

                if ( !Page.IsValid )
                {
                    return;
                }

                cvGroup.IsValid = group.IsValid;
                if ( !cvGroup.IsValid )
                {
                    cvGroup.ErrorMessage = GetFormattedErrorMessages( group.ValidationResults.Select( r => r.ErrorMessage ).ToList() );
                    return;
                }

                if ( !isNewGroup )
                {
                    // Remove any group requirements that were removed in the UI.
                    var currentGroupRequirementGuids = this.GroupRequirementsState.Select( r => r.Guid );
                    foreach ( var existingGroupRequirement in group.GroupRequirements.Where( r => !currentGroupRequirementGuids.Contains( r.Guid ) ).ToList() )
                    {
                        groupRequirementService.Delete( existingGroupRequirement );
                    }
                }

                // Add/Update any group requirements that were added or changed in the UI.
                var groupRequirementsToInsert = new List<GroupRequirement>();
                foreach ( var currentGroupRequirement in this.GroupRequirementsState )
                {
                    var groupRequirement = group.GroupRequirements.FirstOrDefault( r => r.Guid.Equals( currentGroupRequirement.Guid ) );
                    if ( groupRequirement == null )
                    {
                        groupRequirement = new GroupRequirement();
                        groupRequirementsToInsert.Add( groupRequirement );
                    }

                    groupRequirement.CopyPropertiesFrom( currentGroupRequirement );
                }

                rockContext.WrapTransaction( () =>
                {
                    // Initial save to get the group ID for any referenced entities.
                    rockContext.SaveChanges();

                    if ( groupRequirementsToInsert.Any() )
                    {
                        groupRequirementsToInsert.ForEach( r => r.GroupId = group.Id );
                        groupRequirementService.AddRange( groupRequirementsToInsert );
                    }

                    // Follow-up save for newly-added referenced entities.
                    rockContext.SaveChanges();

                    group.SaveAttributeValues( rockContext );
                } );
            }

            var qryParams = GetCommonQueryParams();
            qryParams[PageParameterKey.GroupId] = group.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( this.GroupId == 0 )
            {
                TryNavigateToParentGroup();
            }
            else
            {
                ShowDetails( this.GroupId, false );
            }
        }

        #endregion

        #region Add/Edit Group Requirements Events

        /// <summary>
        /// Handles the OnDataBound event of the lAppliesToDataViewId control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lAppliesToDataViewId_OnDataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            Literal lAppliesToDataViewId = sender as Literal;
            GroupRequirement requirement = e.Row.DataItem as GroupRequirement;
            if ( requirement != null )
            {
                lAppliesToDataViewId.Text = requirement.AppliesToDataViewId.HasValue ? "<i class=\"fa fa-check\"></i>" : string.Empty;
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_Add( object sender, EventArgs e )
        {
            gGroupRequirements_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_Edit( object sender, RowEventArgs e )
        {
            var rowGuid = ( Guid ) e.RowKeyValue;
            gGroupRequirements_ShowEdit( rowGuid );
        }

        /// <summary>
        /// Shows the modal dialog to add/edit a Group Requirement
        /// </summary>
        /// <param name="groupRequirementGuid">The group requirement unique identifier.</param>
        protected void gGroupRequirements_ShowEdit( Guid groupRequirementGuid )
        {
            List<GroupRequirementType> groupRequirementTypes;
            using ( var rockContext = new RockContext() )
            {
                groupRequirementTypes = new GroupRequirementTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .OrderBy( t => t.Name )
                    .ToList();

                ddlGroupRequirementType.Items.Clear();
                ddlGroupRequirementType.Items.Add( new ListItem() );
                foreach ( var item in groupRequirementTypes )
                {
                    ddlGroupRequirementType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
                }

                rblAppliesToAgeClassification.Items.Clear();
                foreach ( var ageClassification in Enum.GetValues( typeof( AppliesToAgeClassification ) ).Cast<AppliesToAgeClassification>() )
                {
                    rblAppliesToAgeClassification.Items.Add( new ListItem( ageClassification.ConvertToString( true ), ageClassification.ConvertToString( false ) ) );
                }

                // Get a list of Field Types that are for dates.
                HashSet<int> fieldTypeIds = new HashSet<int>
                {
                    FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE.AsGuid() ).Value,
                    FieldTypeCache.GetId( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ).Value
                };

                if ( this.GroupId > 0 )
                {
                    var group = new GroupService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( g => g.Id == this.GroupId );

                    if ( group == null )
                    {
                        ShowGroupNotFoundMessage();
                        return;
                    }

                    group.LoadAttributes();

                    foreach ( var attribute in group.Attributes.Select( a => a.Value ).Where( a => fieldTypeIds.Contains( a.FieldTypeId ) ).ToList() )
                    {
                        ddlDueDateGroupAttribute.Items.Add( new ListItem( attribute.Name, attribute.Id.ToString() ) );
                    }
                }
            }

            var selectedGroupRequirement = this.GroupRequirementsState.FirstOrDefault( r => r.Guid.Equals( groupRequirementGuid ) );
            if ( selectedGroupRequirement != null )
            {
                // Edit existing group requirement.
                ddlGroupRequirementType.SelectedValue = selectedGroupRequirement.GroupRequirementTypeId.ToString();
                ToggleGroupRequirementDueDateControls( groupRequirementTypes );

                grpGroupRequirementGroupRole.GroupRoleId = selectedGroupRequirement.GroupRoleId;
                rblAppliesToAgeClassification.SetValue( selectedGroupRequirement.AppliesToAgeClassification.ToString() );
                dvpAppliesToDataView.SetValue( selectedGroupRequirement.AppliesToDataViewId );
                cbAllowLeadersToOverride.Checked = selectedGroupRequirement.AllowLeadersToOverride;

                var groupRequirementType = groupRequirementTypes.First( r => r.Id == selectedGroupRequirement.GroupRequirementTypeId );
                if ( groupRequirementType.DueDateType == DueDateType.ConfiguredDate )
                {
                    dpDueDate.SelectedDate = selectedGroupRequirement.DueDateStaticDate.Value;
                }
                if ( groupRequirementType.DueDateType == DueDateType.GroupAttribute )
                {
                    ddlDueDateGroupAttribute.SetValue( selectedGroupRequirement.DueDateAttributeId.HasValue ? selectedGroupRequirement.DueDateAttributeId.ToString() : string.Empty );
                }

                cbMembersMustMeetRequirementOnAdd.Checked = selectedGroupRequirement.MustMeetRequirementToAddMember;
            }
            else
            {
                // Add new group requirement.
                ddlGroupRequirementType.SelectedIndex = 0;
                ToggleGroupRequirementDueDateControls( groupRequirementTypes );

                grpGroupRequirementGroupRole.GroupTypeId = this.GroupTypeId;
                rblAppliesToAgeClassification.SetValue( AppliesToAgeClassification.All.ToString() );
                dvpAppliesToDataView.SetValue( null );
                cbAllowLeadersToOverride.Checked = false;
                cbMembersMustMeetRequirementOnAdd.Checked = false;
            }

            nbDuplicateGroupRequirement.Visible = false;

            hfGroupRequirementGuid.Value = groupRequirementGuid.ToString();

            ShowDialog( DialogKey.GroupRequirements );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupRequirementType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupRequirementType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ToggleGroupRequirementDueDateControls();
        }

        /// <summary>
        /// Handles the OkClick event of the mdGroupRequirement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupRequirement_OkClick( object sender, EventArgs e )
        {
            var groupRequirementGuid = hfGroupRequirementGuid.Value.AsGuid();

            var groupRequirement = this.GroupRequirementsState.FirstOrDefault( r => r.Guid.Equals( groupRequirementGuid ) );
            if ( groupRequirement == null )
            {
                groupRequirement = new GroupRequirement
                {
                    Guid = Guid.NewGuid()
                };
                this.GroupRequirementsState.Add( groupRequirement );
            }

            using ( var rockContext = new RockContext() )
            {
                groupRequirement.GroupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();
                groupRequirement.GroupRequirementType = new GroupRequirementTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( t => t.Id == groupRequirement.GroupRequirementTypeId );

                groupRequirement.GroupRoleId = grpGroupRequirementGroupRole.GroupRoleId;
                groupRequirement.GroupRole = groupRequirement.GroupRoleId.HasValue
                    ? new GroupTypeRoleService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( r => r.Id == groupRequirement.GroupRoleId.Value )
                    : default;

                groupRequirement.AppliesToAgeClassification = rblAppliesToAgeClassification.SelectedValue.ConvertToEnum<AppliesToAgeClassification>();
                groupRequirement.AppliesToDataViewId = dvpAppliesToDataView.SelectedValueAsId();

                if ( groupRequirement.GroupRequirementType.DueDateType == DueDateType.GroupAttribute )
                {
                    // Set this due date attribute if it exists.
                    var groupDueDateAttribute = AttributeCache.AllForEntityType<Group>().FirstOrDefault( a => a.Id == ddlDueDateGroupAttribute.SelectedValue.AsIntegerOrNull() );
                    if ( groupDueDateAttribute != null )
                    {
                        groupRequirement.DueDateAttributeId = groupDueDateAttribute.Id;
                    }
                }

                if ( groupRequirement.GroupRequirementType.DueDateType == DueDateType.ConfiguredDate )
                {
                    groupRequirement.DueDateStaticDate = dpDueDate.SelectedDate;
                }

                groupRequirement.AllowLeadersToOverride = cbAllowLeadersToOverride.Checked;
                groupRequirement.MustMeetRequirementToAddMember = cbMembersMustMeetRequirementOnAdd.Checked;

                // Make sure we aren't adding a duplicate group requirement (same group requirement type and role)
                var duplicateGroupRequirementExists = this.GroupRequirementsState.Any( r =>
                    r.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId
                        && r.GroupRoleId == groupRequirement.GroupRoleId
                        && r.Guid != groupRequirement.Guid );

                if ( duplicateGroupRequirementExists )
                {
                    nbDuplicateGroupRequirement.Text = $"This group already has a group requirement of {groupRequirement.GroupRequirementType.Name}{( groupRequirement.GroupRoleId.HasValue ? $" for group role {groupRequirement.GroupRole.Name}" : string.Empty )}.";
                    nbDuplicateGroupRequirement.Visible = true;
                    this.GroupRequirementsState.Remove( groupRequirement );
                }
                else
                {
                    nbDuplicateGroupRequirement.Visible = false;
                    BuildGroupRequirementsList( true, rockContext );
                    HideDialog();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_Delete( object sender, RowEventArgs e )
        {
            var rowGuid = ( Guid ) e.RowKeyValue;
            this.GroupRequirementsState.RemoveEntity( rowGuid );

            using ( var context = new RockContext() )
            {
                BuildGroupRequirementsList( true, context );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_GridRebind( object sender, GridRebindEventArgs e )
        {
            using ( var context = new RockContext() )
            {
                BuildGroupRequirementsList( true, context );
            }
        }

        #endregion

        #region View Events

        /// <summary>
        /// Handles the ItemDataBound event of the rptRequirementsList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptRequirementsList_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( !( e.Item.DataItem is GroupRequirement groupRequirement )
                || groupRequirement == null
                || string.IsNullOrWhiteSpace( groupRequirement.GroupRequirementType?.Name )
                || !( e.Item.FindControl( "lRequirement" ) is Literal lRequirement )
                || !( e.Item.FindControl( "bRequiresLogin" ) is Rock.Web.UI.Controls.Badge bRequiresLogin ) )
            {
                if ( e.Item.FindControl( "liRequirement" ) is Control liRequirement )
                {
                    liRequirement.Visible = false;
                }

                return;
            }

            lRequirement.Text = groupRequirement.GroupRequirementType.Name;
            bRequiresLogin.Visible = groupRequirement.MustMeetRequirementToAddMember;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgOpportunitiesTimeframe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgOpportunitiesTimeframe_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindOpportunitiesGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gOpportunities_RowDataBound( object sender, GridViewRowEventArgs e )
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

            if ( opportunity.SlotsFilled > 0 )
            {
                e.Row.AddCssClass( "js-has-participants" );
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

            if ( !_canEdit )
            {
                var editField = gOpportunities.ColumnsOfType<EditField>().FirstOrDefault( c => c.ID == "efOpportunities" );
                if ( editField != null )
                {
                    editField.Visible = false;
                }

                var deleteField = gOpportunities.ColumnsOfType<DeleteField>().FirstOrDefault( c => c.ID == "dfOpportunities" );
                if ( deleteField != null )
                {
                    deleteField.Visible = false;
                }
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
                { PageParameterKey.GroupId, this.GroupId.ToString() },
                { PageParameterKey.LocationId, keys[DataKeyName.LocationId].ToString() },
                { PageParameterKey.ScheduleId, keys[DataKeyName.ScheduleId].ToString() }
            };

            NavigateToLinkedPage( AttributeKey.SignUpOpportunityAttendeeListPage, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowDetails( this.GroupId, true );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            ResetNotificationBoxes();

            int? parentGroupId = null;
            using ( var rockContext = new RockContext() )
            {
                var groupService = new GroupService( rockContext );
                var group = groupService.Queryable()
                    .Include( g => g.ParentGroup ) // Parent group is needed to properly check for edit authorization.
                    .FirstOrDefault( g => g.Id == this.GroupId );

                if ( group == null )
                {
                    ShowGroupNotFoundMessage();
                    return;
                }

                parentGroupId = group.ParentGroupId;

                if ( !IsAuthorizedToEdit( group ) )
                {
                    if ( group.IsSystem )
                    {
                        ShowIsSystemGroupMessage();
                    }
                    else
                    {
                        mdDeleteWarning.Show( $"You are not authorized to delete this {Group.FriendlyTypeName}.", ModalAlertType.Information );
                    }

                    return;
                }

                if ( !groupService.CanDelete( group, out string errorMessage, true ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // If group has a non-named schedule, delete the schedule record.
                if ( group.ScheduleId.HasValue )
                {
                    var scheduleService = new ScheduleService( rockContext );
                    var schedule = scheduleService.Get( group.ScheduleId.Value );
                    if ( schedule != null && schedule.ScheduleType != ScheduleType.Named )
                    {
                        // Make sure this is the only group trying to use this schedule.
                        if ( !groupService.Queryable().Where( g => g.ScheduleId == schedule.Id && g.Id != group.Id ).Any() )
                        {
                            scheduleService.Delete( schedule );
                        }
                    }
                }

                // Delete any non-named schedules tied to opportunities we'll be deleting.
                var schedulesToDelete = new GroupLocationService( rockContext )
                    .Queryable()
                    .Where( gl => gl.GroupId == this.GroupId )
                    .SelectMany( gl => gl.Schedules )
                    .ToList();

                groupService.Delete( group );

                rockContext.WrapTransaction( () =>
                {
                    // Initial save to release FK constraints tied to referenced schedules we'll be deleting.
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

                    // Follow-up save for deleted referenced entities.
                    rockContext.SaveChanges();
                } );
            }

            TryNavigateToParentGroup( parentGroupId );
        }

        #endregion

        #region Add/Edit Opportunity (GroupLocationSchedule) Events

        /// <summary>
        /// Handles the Add event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gOpportunities_Add( object sender, EventArgs e )
        {
            gOpportunities_ShowEdit();
        }

        /// <summary>
        /// Handles the Edit event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gOpportunities_Edit( object sender, RowEventArgs e )
        {
            var keys = e.RowKeyValues;
            gOpportunities_ShowEdit( keys[DataKeyName.GroupLocationId].ToIntSafe(), keys[DataKeyName.ScheduleId].ToIntSafe() );
        }

        /// <summary>
        /// Shows the modal dialog to add/edit an opportunity.
        /// </summary>
        /// <param name="groupLocationId">The group location identifier.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        protected void gOpportunities_ShowEdit( int? groupLocationId = null, int? scheduleId = null )
        {
            Opportunity opportunity = null;

            if ( groupLocationId.HasValue && scheduleId.HasValue )
            {
                opportunity = this.OpportunitiesState.FirstOrDefault( o => o.GroupLocationId == groupLocationId.Value && o.ScheduleId == scheduleId.Value );
            }

            mdAddOpportunity.Title = opportunity != null ? "Edit Opportunity" : "Add Opportunity";
            nbNoAllowedScheduleTypes.Visible = false;
            nbNoAllowedLocationPickerModes.Visible = false;
            var shouldShowDialog = true;

            if ( !this.AnyAllowedScheduleTypes )
            {
                ShowNoAllowedScheduleTypesMessage();
                shouldShowDialog = false;
            }

            if ( !this.AnyAllowedLocationPickerModes )
            {
                ShowNoAllowedLocationPickerModesMessage();
                shouldShowDialog = false;
            }

            if ( !shouldShowDialog )
            {
                return;
            }

            Schedule schedule = null;
            Location location = null;

            if ( opportunity == null )
            {
                opportunity = new Opportunity();
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    schedule = new ScheduleService( rockContext ).GetNoTracking( opportunity.ScheduleId );
                    location = new LocationService( rockContext ).GetNoTracking( opportunity.LocationId );
                }
            }

            this.EditGroupLocationId = opportunity.GroupLocationId;
            this.EditScheduleId = opportunity.ScheduleId;

            tbOpportunityName.Text = opportunity.Name;

            if ( !TryInitializeScheduleControls( schedule ) )
            {
                return;
            }

            InitializeLocationPicker( location );

            tbMinimumAttendance.Text = opportunity.SlotsMin.ToString();
            tbDesiredAttendance.Text = opportunity.SlotsDesired.ToString();
            tbMaximumAttendance.Text = opportunity.SlotsMax.ToString();

            if ( !_isProjectTypeInPerson )
            {
                htmlOpportunityReminderAddlDetails.Text = default;
                htmlOpportunityReminderAddlDetails.Visible = false;
            }
            else
            {
                htmlOpportunityReminderAddlDetails.Text = opportunity.ReminderAdditionalDetails;
                htmlOpportunityReminderAddlDetails.Visible = true;
            }

            htmlOpportunityConfirmationDetails.Text = opportunity.ConfirmationAdditionalDetails;

            ShowDialog( DialogKey.AddOpportunity );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgScheduleType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgScheduleType_SelectedIndexChanged( object sender, EventArgs e )
        {
            Enum.TryParse( bgScheduleType.SelectedValue, out ScheduleType scheduleType );
            ToggleScheduleControls( scheduleType );
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            InitializeFriendlyScheduleLabel();
            EnsureNoDuplicateOpportunityExists();
        }

        /// <summary>
        /// Handles the SelectLocation event of the lpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lpLocation_SelectLocation( object sender, EventArgs e )
        {
            EnsureNoDuplicateOpportunityExists();
        }

        /// <summary>
        /// Handles the SelectItem event of the spSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void spSchedule_SelectItem( object sender, EventArgs e )
        {
            EnsureNoDuplicateOpportunityExists();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddOpportunity_SaveClick( object sender, EventArgs e )
        {
            var parsed = ParseNewScheduleAndLocation();

            cvOpportunity.IsValid = parsed.IsValid;
            if ( !cvOpportunity.IsValid )
            {
                cvOpportunity.ErrorMessage = GetFormattedErrorMessages( parsed.ErrorMessages );
                return;
            }

            if ( !EnsureNoDuplicateOpportunityExists() )
            {
                return;
            }

            var newScheduleType = parsed.NewScheduleType;
            var newICalendarContent = parsed.NewICalendarContent;
            var newScheduleId = parsed.NewScheduleId;
            var newLocationId = parsed.NewLocationId;

            using ( var rockContext = new RockContext() )
            {
                GroupLocation groupLocationToSave = null;
                GroupLocation existingGroupLocation = null;
                var shouldDeleteExistingGroupLocation = false;

                Schedule existingSchedule = null;
                var shouldDeleteExistingSchedule = false;

                var shouldAddNewScheduleConfig = true;
                var shouldDeleteExistingScheduleConfig = false;

                var membersToReassign = new List<GroupMemberAssignment>();

                var newOpportunityName = tbOpportunityName.Text;
                var newMinimumAttendance = tbMinimumAttendance.Text.AsIntegerOrNull();
                var newDesiredAttendance = tbDesiredAttendance.Text.AsIntegerOrNull();
                var newMaximumAttendance = tbMaximumAttendance.Text.AsIntegerOrNull();
                var newReminderAddlDetails = _isProjectTypeInPerson ? htmlOpportunityReminderAddlDetails.Text : default;
                var newConfirmationAddlDetails = htmlOpportunityConfirmationDetails.Text;

                var scheduleService = new ScheduleService( rockContext );
                var groupLocationService = new GroupLocationService( rockContext );
                var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );

                // Local function to consistently query for a GroupLocation based on different criteria.
                GroupLocation GetExistingGroupLocation( Expression<Func<GroupLocation, bool>> whereExpression )
                {
                    return groupLocationService
                        .Queryable()
                        .Include( gl => gl.Schedules )
                        .Include( gl => gl.GroupLocationScheduleConfigs )
                        .FirstOrDefault( whereExpression );
                }

                // Local function to 1) find a GroupLocation whose Group and Location match the new opportunity or 2) create and add a new GroupLocation.
                GroupLocation GetNewOrMatchingGroupLocation()
                {
                    var groupLocation = GetExistingGroupLocation( gl => gl.GroupId == this.GroupId && gl.LocationId == newLocationId.Value );
                    if ( groupLocation == null )
                    {
                        groupLocation = new GroupLocation { GroupId = this.GroupId, LocationId = newLocationId.Value };
                        groupLocationService.Add( groupLocation );
                    }

                    return groupLocation;
                }

                // Local function to add a new Schedule to the appropriate GroupLocation.
                Schedule newSchedule = null;
                void AddNewSchedule( GroupLocation groupLocation )
                {
                    if ( newScheduleType == ScheduleType.Custom )
                    {
                        newSchedule = new Schedule { iCalendarContent = newICalendarContent };
                        groupLocation.Schedules.Add( newSchedule );
                    }

                    if ( newScheduleType == ScheduleType.Named )
                    {
                        newSchedule = scheduleService.Get( newScheduleId.Value );
                        groupLocation.Schedules.Add( newSchedule );
                    }

                    shouldDeleteExistingScheduleConfig = true;
                }

                if ( this.EditGroupLocationId > 0 && this.EditScheduleId > 0 )
                {
                    existingGroupLocation = GetExistingGroupLocation( gl => gl.Id == this.EditGroupLocationId );
                }

                if ( existingGroupLocation == null )
                {
                    // Add new opportunity.

                    groupLocationToSave = GetNewOrMatchingGroupLocation();
                    AddNewSchedule( groupLocationToSave );
                }
                else
                {
                    // Edit existing opportunity.

                    /*
                     * GroupMemberAssignments are tied to a specific Schedule & Location combo.
                     * If either of these change for an existing opportunity, we'll have to reassign any members.
                     */
                    var shouldReassignMembers = false;
                    var groupLocationChanged = false;

                    if ( !newLocationId.Equals( existingGroupLocation.LocationId ) )
                    {
                        /*
                         * The Location changed from what was previously saved.
                         * Let's try to find another existing GroupLocation matching this opportunity's Group and newly-specified Location, or create a new one if needed.
                         */
                        groupLocationToSave = GetNewOrMatchingGroupLocation();
                        shouldReassignMembers = true;
                        groupLocationChanged = true;
                    }
                    else
                    {
                        // The Location stayed the same.
                        groupLocationToSave = existingGroupLocation;
                    }

                    existingSchedule = existingGroupLocation.Schedules.FirstOrDefault( s => s.Id == this.EditScheduleId );
                    if ( existingSchedule != null )
                    {
                        if ( groupLocationChanged )
                        {
                            existingGroupLocation.Schedules.Remove( existingSchedule );
                            AddNewSchedule( groupLocationToSave );
                        }
                        else
                        {
                            /*
                             * If we got here, this means:
                             *   1) The existing opportunity's GroupLocation did not change (however, groupLocationToSave and exisingGroupLocation now point to the same object).
                             *   2) We found an existing Schedule based on the previous Schedule Id.
                             *   3) The last thing we need to do is compare the existing Schedule with the new/edited instance.
                             */

                            if ( newScheduleType != existingSchedule.ScheduleType )
                            {
                                // ScheduleType changed from what was previously saved.
                                existingGroupLocation.Schedules.Remove( existingSchedule );
                                shouldReassignMembers = true;

                                if ( existingSchedule.ScheduleType == ScheduleType.Custom )
                                {
                                    // As long as nothing else is using this old custom Schedule (which we'll verify below), we should delete it.
                                    shouldDeleteExistingSchedule = true;
                                }

                                AddNewSchedule( existingGroupLocation );
                            }
                            else // ScheduleType remained the same as what was previously saved.
                            {
                                var shouldUpdateExistingScheduleConfig = false;

                                if ( newScheduleType == ScheduleType.Custom )
                                {
                                    // Ensure we update to the latest custom schedule content, but no need to reassign members in this case.
                                    existingSchedule.iCalendarContent = newICalendarContent;
                                    shouldUpdateExistingScheduleConfig = true;
                                }

                                if ( newScheduleType == ScheduleType.Named )
                                {
                                    if ( !newScheduleId.Equals( existingSchedule.Id ) )
                                    {
                                        // Named Schedule changed from what was previously saved.
                                        existingGroupLocation.Schedules.Remove( existingSchedule );
                                        shouldReassignMembers = true;

                                        AddNewSchedule( existingGroupLocation );
                                    }
                                    else
                                    {
                                        // Schedule stayed the same.
                                        shouldUpdateExistingScheduleConfig = true;
                                    }
                                }

                                if ( shouldUpdateExistingScheduleConfig )
                                {
                                    // Ensure we update to the latest Config values.
                                    var config = existingGroupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == this.EditScheduleId );
                                    if ( config != null )
                                    {
                                        shouldAddNewScheduleConfig = false;

                                        config.ConfigurationName = newOpportunityName;
                                        config.MinimumCapacity = newMinimumAttendance;
                                        config.DesiredCapacity = newDesiredAttendance;
                                        config.MaximumCapacity = newMaximumAttendance;
                                        config.ReminderAdditionalDetails = newReminderAddlDetails;
                                        config.ConfirmationAdditionalDetails = newConfirmationAddlDetails;
                                    }
                                }
                            }
                        }
                    }

                    if ( shouldReassignMembers )
                    {
                        // Save these members for later; we'll add them to the new Schedule/Location combo below.
                        membersToReassign = groupMemberAssignmentService
                            .Queryable()
                            .Where( gma => gma.ScheduleId == this.EditScheduleId
                                && gma.LocationId == existingGroupLocation.LocationId
                                && gma.GroupMember.GroupId == this.GroupId )
                            .ToList();

                        groupMemberAssignmentService.DeleteRange( membersToReassign );
                    }

                    if ( groupLocationChanged )
                    {
                        // Since we removed the existing schedule (if found) from the existing GroupLocation, check if it has any remaining Schedules; if not: delete it.
                        if ( !existingGroupLocation.Schedules.Any() )
                        {
                            // This will also delete any lingering GroupLocationScheduleConfig records below.
                            shouldDeleteExistingGroupLocation = true;
                        }
                        else
                        {
                            shouldDeleteExistingScheduleConfig = true;
                        }
                    }

                    if ( shouldDeleteExistingScheduleConfig )
                    {
                        // Manually delete this Config record, as it's parent GroupLocation might be sticking around (so it won't get auto-deleted in this case).
                        var existingConfig = existingGroupLocation.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == this.EditScheduleId );
                        if ( existingConfig != null )
                        {
                            existingGroupLocation.GroupLocationScheduleConfigs.Remove( existingConfig );
                        }
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    // Initial save to get needed parent Id values && release FK constraints tied to referenced entities we'll be adding/updating/deleting.
                    rockContext.SaveChanges();

                    if ( newSchedule != null )
                    {
                        if ( shouldAddNewScheduleConfig )
                        {
                            groupLocationToSave.GroupLocationScheduleConfigs.Add( new GroupLocationScheduleConfig
                            {
                                GroupLocationId = groupLocationToSave.Id,
                                ScheduleId = newSchedule.Id,
                                ConfigurationName = newOpportunityName,
                                MinimumCapacity = newMinimumAttendance,
                                DesiredCapacity = newDesiredAttendance,
                                MaximumCapacity = newMaximumAttendance,
                                ReminderAdditionalDetails = newReminderAddlDetails,
                                ConfirmationAdditionalDetails = newConfirmationAddlDetails
                            } );
                        }

                        if ( membersToReassign.Any() )
                        {
                            groupMemberAssignmentService.AddRange( membersToReassign
                                .Select( m =>
                                {
                                    var reassignedMember = new GroupMemberAssignment();
                                    reassignedMember.CopyPropertiesFrom( m );
                                    reassignedMember.LocationId = newLocationId;
                                    reassignedMember.ScheduleId = newSchedule.Id;

                                    return reassignedMember;
                                } ) );
                        }
                    }

                    if ( shouldDeleteExistingSchedule )
                    {
                        if ( scheduleService.CanDelete( existingSchedule, out string scheduleErrorMessage ) )
                        {
                            scheduleService.Delete( existingSchedule );
                        }
                    }

                    if ( shouldDeleteExistingGroupLocation )
                    {
                        if ( groupLocationService.CanDelete( existingGroupLocation, out string groupLocationErrorMessage ) )
                        {
                            groupLocationService.Delete( existingGroupLocation );
                        }
                    }

                    // Follow-up save for added/updated/deleted referenced entities.
                    rockContext.SaveChanges();
                } );
            }

            BindOpportunitiesGrid( shouldForceRefresh: true );
            HideDialog();
        }

        /// <summary>
        /// Handles the Delete event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gOpportunities_Delete( object sender, RowEventArgs e )
        {
            var groupId = e.RowKeyValues[DataKeyName.GroupId].ToIntSafe();
            var locationId = e.RowKeyValues[DataKeyName.LocationId].ToIntSafe();
            var scheduleId = e.RowKeyValues[DataKeyName.ScheduleId].ToIntSafe();

            /*
             * We should consider moving this logic to a service (probably the GroupLocationService), as this code block is identical
             * to that found within the SignUpOverview block's dfOpportunities_Click() method.
             */

            using ( var rockContext = new RockContext() )
            {
                /*
                 * An Opportunity is a GroupLocationSchedule with possible GroupMemberAssignments (and therefore, GroupMembers).
                 * When deleting an Opportunity we should delete the following:
                 * 
                 * 1) GroupMemberAssignments
                 * 2) GroupMembers (if no more GroupMemberAssignments for a given GroupMember)
                 * 3) GroupLocationSchedule & GroupLocationScheduleConfig
                 * 4) GroupLocation (if no more Schedules tied to it)
                 * 5) Schedule (if non-named and nothing else is using it)
                 */

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

                /*
                 * For now, this is safe, as GroupMemberAssignment is a pretty low-level Entity with no child Entities.
                 * We'll need to check `GroupMemberAssignmentService.CanDelete()` for each assignment (and abandon the bulk
                 * delete approach) if this changes in the future.
                 */
                groupMemberAssignmentService.DeleteRange( groupMemberAssignments );

                // Get the GroupType to check if this Group has history enabled below, so we know whether to call GroupMemberService.CanDelete() for each GroupMember.
                var group = new GroupService( rockContext ).GetNoTracking( groupId );
                var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );

                var groupMemberService = new GroupMemberService( rockContext );
                foreach ( var groupMember in groupMembers.Where( gm => !gm.GroupMemberAssignments.Any() ) )
                {
                    if ( !groupTypeCache.EnableGroupHistory && !groupMemberService.CanDelete( groupMember, out string groupMemberErrorMessage ) )
                    {
                        // The Attendee (Group Member Assignment) record itself will be deleted, but we cannot delete the corresponding GroupMember record.
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

                    /*
                     * We cannot safely remove referenced Locations (even non-named ones):
                     *   1) because of the way we reuse/share Locations across entities (the LocationPicker control auto-searches/matches and saves Locations).
                     *   2) because of the cascade deletes many of the referencing entities have on their LocationId FK constraints (we might accidentally delete a lot of unintended stuff).
                     */

                    // Follow-up save for deleted referenced entities.
                    rockContext.SaveChanges();
                } );
            }

            BindOpportunitiesGrid( shouldForceRefresh: true );
        }

        /// <summary>
        /// Handles the GridRebind event of the gOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gOpportunities_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindOpportunitiesGrid();
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Resets the control visibility.
        /// </summary>
        private void ResetControlVisibility()
        {
            ResetNotificationBoxes();

            pnlDetails.Visible = true;
            pnlLabelsAndFollow.Visible = true;
            pdAuditDetails.Visible = true;
        }

        /// <summary>
        /// Resets the notification boxes.
        /// </summary>
        private void ResetNotificationBoxes()
        {
            nbNotFoundOrArchived.Visible = false;
            nbInvalidGroupType.Visible = false;
            nbParentNotFound.Visible = false;
            nbGroupTypeNotAllowed.Visible = false;
            nbNotAuthorizedToView.Visible = false;
            nbEditModeMessage.Visible = false;
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Info;
        }

        /// <summary>
        /// Determines whether [is authorized to edit] [the specified group].
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>
        ///   <c>true</c> if [is authorized to edit] [the specified group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAuthorizedToEdit( Group group )
        {
            _canEdit = IsUserAuthorized( Authorization.EDIT )
                && group?.IsSystem == false
                && group?.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) == true;
            return _canEdit;
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
        /// Shows the invalid group type message.
        /// </summary>
        private void ShowInvalidGroupTypeMessage()
        {
            nbInvalidGroupType.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Shows the parent not found message.
        /// </summary>
        private void ShowParentNotFoundMessage()
        {
            nbParentNotFound.Visible = true;
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
        /// Shows the edit mode message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowEditModeMessage( string message )
        {
            nbEditModeMessage.Text = message;
            nbEditModeMessage.Visible = true;
        }

        /// <summary>
        /// Shows the is system group message.
        /// </summary>
        private void ShowIsSystemGroupMessage()
        {
            ShowEditModeMessage( EditModeMessage.System( Group.FriendlyTypeName ) );
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Info;
        }

        /// <summary>
        /// Shows the not authorized to edit message.
        /// </summary>
        private void ShowNotAuthorizedToEditMessage()
        {
            ShowEditModeMessage( EditModeMessage.NotAuthorizedToEdit( Group.FriendlyTypeName ) );
            nbEditModeMessage.NotificationBoxType = NotificationBoxType.Warning;
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="isEditMode">if set to <c>true</c> [is edit mode].</param>
        private void ShowDetails( int groupId, bool isEditMode )
        {
            ResetControlVisibility();

            using ( var rockContext = new RockContext() )
            {
                Group group = null;

                if ( groupId != 0 )
                {
                    group = new GroupService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Include( g => g.Campus )
                        .Include( g => g.GroupType )
                        .Include( g => g.GroupRequirements )
                        .Include( g => g.GroupRequirements.Select( gr => gr.GroupRequirementType ) )
                        .Include( g => g.GroupRequirements.Select( gr => gr.GroupRole ) )
                        .Include( g => g.ParentGroup )
                        .FirstOrDefault( g => g.Id == groupId );

                    if ( group == null )
                    {
                        ShowGroupNotFoundMessage();
                        return;
                    }

                    group.LoadAttributes();

                    if ( !IsGroupSignUpType( group ) )
                    {
                        ShowInvalidGroupTypeMessage();
                        return;
                    }

                    FollowingsHelper.SetFollowing( group, pnlFollowing, this.CurrentPerson );
                }

                this.GroupRequirementsState = new List<GroupRequirement>();

                if ( group == null )
                {
                    // Create new group.
                    pnlLabelsAndFollow.Visible = false;
                    pdAuditDetails.Visible = false;

                    group = new Group();

                    if ( !TryCopyParentValues( group, rockContext ) )
                    {
                        var rootSignUpGroupId = new GroupService( rockContext ).GetId( Rock.SystemGuid.Group.GROUP_SIGNUP_GROUPS.AsGuid() );
                        if ( rootSignUpGroupId.HasValue )
                        {
                            var rootGroupIdString = rootSignUpGroupId.ToString();
                            var qryParams = new Dictionary<string, string>
                            {
                                { PageParameterKey.GroupId, "0" },
                                { PageParameterKey.ParentGroupId, rootGroupIdString },
                                { PageParameterKey.ExpandedIds, rootGroupIdString }
                            };

                            NavigateToPage( RockPage.Guid, qryParams );
                        }
                        else
                        {
                            ShowParentNotFoundMessage();
                        }

                        return;
                    }

                    if ( !IsGroupSignUpType( group ) )
                    {
                        ShowInvalidGroupTypeMessage();
                        return;
                    }

                    if ( !group.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                    {
                        ShowNotAuthorizedToViewMessage();
                        return;
                    }

                    ShowEditDetails( group, rockContext );
                }
                else
                {
                    // Edit/View existing group.
                    if ( !group.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                    {
                        ShowNotAuthorizedToViewMessage();
                        return;
                    }

                    this.ProjectName = group.Name;
                    pdAuditDetails.SetEntity( group, ResolveRockUrl( "~" ) );

                    var isReadOnly = !IsAuthorizedToEdit( group );
                    if ( group.IsSystem )
                    {
                        ShowIsSystemGroupMessage();
                        isEditMode = false;
                    }
                    else if ( isEditMode && isReadOnly )
                    {
                        ShowNotAuthorizedToEditMessage();
                        isEditMode = false;
                    }

                    if ( group.GroupRequirements?.Any() == true )
                    {
                        this.GroupRequirementsState = group.GroupRequirements.ToList();
                    }

                    if ( isEditMode )
                    {
                        ShowEditDetails( group, rockContext );
                    }
                    else
                    {
                        ShowViewDetails( group, isReadOnly, rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [is group sign up type] [the specified group].
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns>
        ///   <c>true</c> if [is group sign up type] [the specified group]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsGroupSignUpType( Group group )
        {
            var isSignUpType = group.GroupTypeId == this.SignUpGroupTypeId
                || group.GroupType.InheritedGroupTypeId.GetValueOrDefault() == this.SignUpGroupTypeId;

            return isSignUpType;
        }

        /// <summary>
        /// Tries to copy parent values.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private bool TryCopyParentValues( Group group, RockContext rockContext )
        {
            if ( !PageParentGroupId.HasValue )
            {
                return false;
            }

            var parentGroup = new GroupService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( g => g.GroupType )
                .FirstOrDefault( g => g.Id == this.PageParentGroupId.Value );

            if ( parentGroup == null )
            {
                return false;
            }

            group.ParentGroup = parentGroup;
            group.ParentGroupId = parentGroup.Id;
            group.GroupType = parentGroup.GroupType;
            group.GroupTypeId = parentGroup.GroupTypeId;
            group.CampusId = parentGroup.CampusId;
            group.ReminderSystemCommunicationId = parentGroup.ReminderSystemCommunicationId;
            group.ReminderOffsetDays = parentGroup.ReminderOffsetDays;
            group.ReminderAdditionalDetails = parentGroup.ReminderAdditionalDetails;
            group.ConfirmationAdditionalDetails = parentGroup.ConfirmationAdditionalDetails;

            parentGroup.LoadAttributes();
            group.LoadAttributes();

            var projectTypeAttributeValue = parentGroup.GetAttributeValue( AttributeKey.ProjectType );
            group.SetAttributeValue( AttributeKey.ProjectType, projectTypeAttributeValue );

            this.GroupTypeId = parentGroup.GroupTypeId;
            _isProjectTypeInPerson = GetIsProjectTypeInPerson( projectTypeAttributeValue );

            return true;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( Group group, RockContext rockContext )
        {
            SetEditMode( true );

            lTitle.Text = group.Id > 0 ? group.Name.FormatAsHtmlTitle() : "Add New Project";
            tbName.Text = group.Name;
            cbIsActive.Checked = group.IsActive;
            tbDescription.Text = group.Description;

            if ( group.Id == 0 )
            {
                InitializeGroupTypesDropDownList( group, rockContext );
                lGroupType.Visible = false;
            }
            else
            {
                lGroupType.Text = this.CurrentGroupType.Name;
                lGroupType.Visible = true;
                ddlGroupType.Visible = false;
            }

            cpCampus.SelectedCampusId = group.CampusId;
            SetIsCampusRequired();

            BuildGroupRequirementsList( true, rockContext );

            BuildEditModeAttributesControls( group );

            InitializeSystemCommunicationsDropDownList( group, rockContext );

            nbReminderOffsetDays.Text = group.ReminderOffsetDays.ToString();

            htmlReminderAddlDetails.Text = group.ReminderAdditionalDetails;
            htmlConfirmationDetails.Text = group.ConfirmationAdditionalDetails;
        }

        /// <summary>
        /// Shows the view details.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="isReadOnly">if set to <c>true</c> [is read only].</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowViewDetails( Group group, bool isReadOnly, RockContext rockContext )
        {
            SetEditMode( false );

            this.GroupId = group.Id;
            this.GroupTypeId = group.GroupTypeId;
            _isProjectTypeInPerson = GetIsProjectTypeInPerson( group.GetAttributeValue( AttributeKey.ProjectType ) );

            SetHighlightLabelVisibility( group );

            lTitle.Text = group.Name.FormatAsHtmlTitle();

            if ( string.IsNullOrWhiteSpace( group.Description ) )
            {
                lDescription.Text = string.Empty;
                pnlDescription.Visible = false;
            }
            else
            {
                lDescription.Text = group.Description;
                pnlDescription.Visible = true;
            }

            InitializeAttributeValuesContainer( group, false );

            BuildGroupRequirementsList( false, rockContext );

            BindOpportunitiesGrid( group );

            pnlActions.Visible = !isReadOnly;

            btnSecurity.Visible = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.EntityId = group.Id;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="isEditable">if set to <c>true</c> [is editable].</param>
        private void SetEditMode( bool isEditable )
        {
            pnlEditDetails.Visible = isEditable;
            pnlViewDetails.Visible = !isEditable;

            this.HideSecondaryBlocks( isEditable );
        }

        /// <summary>
        /// Sets the highlight label visibility.
        /// </summary>
        /// <param name="group">The group.</param>
        private void SetHighlightLabelVisibility( Group group )
        {
            var projectTypeValue = GetGroupProjectType( group )?.Value;

            if ( !string.IsNullOrWhiteSpace( projectTypeValue ) )
            {
                hlProjectType.Text = projectTypeValue;
                hlProjectType.Visible = true;
            }
            else
            {
                hlProjectType.Text = string.Empty;
                hlProjectType.Visible = false;
            }

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
        /// Gets the group project type.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private DefinedValueCache GetGroupProjectType( Group group )
        {
            DefinedValueCache definedValue = null;
            var projectTypeGuid = group.GetAttributeValue( AttributeKey.ProjectType );
            if ( !string.IsNullOrWhiteSpace( projectTypeGuid ) )
            {
                definedValue = DefinedValueCache.Get( projectTypeGuid );
            }

            return definedValue;
        }

        /// <summary>
        /// Initializes the group types drop down list.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private void InitializeGroupTypesDropDownList( Group group, RockContext rockContext )
        {
            var allowedGroupTypes = GroupTypeCache
                .All( rockContext )
                .Where( gt =>
                    GetAllowedGroupTypeIds( group.ParentGroup.GroupTypeId ).Contains( gt.Id )
                    && ( gt.Id == this.SignUpGroupTypeId || gt.InheritedGroupTypeId == this.SignUpGroupTypeId ) )
                .OrderBy( gt => gt.Name )
                .ToList();

            ddlGroupType.DataSource = allowedGroupTypes;
            ddlGroupType.DataBind();

            ddlGroupType.SetValue( group.GroupTypeId );
            ddlGroupType.Visible = true;
        }

        /// <summary>
        /// Gets the allowed group type ids.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns></returns>
        private List<int> GetAllowedGroupTypeIds( int parentGroupTypeId )
        {
            return GroupTypeCache
                .Get( parentGroupTypeId )
                .ChildGroupTypes
                .Select( t => t.Id )
                .ToList();
        }

        /// <summary>
        /// Sets whether a campus is required.
        /// </summary>
        private void SetIsCampusRequired()
        {
            cpCampus.Required = this.CurrentGroupType?.GroupsRequireCampus ?? false;
        }

        /// <summary>
        /// Initializes the system communications drop down list.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        private void InitializeSystemCommunicationsDropDownList( Group group, RockContext rockContext )
        {
            ddlReminderCommunication.Items.Clear();
            ddlReminderCommunication.Items.Add( new ListItem() );

            var communicationService = new SystemCommunicationService( rockContext );

            var signUpGroupReminderCategoryId = CategoryCache.GetId( Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_SIGNUP_GROUP_CONFIRMATION.AsGuid() );
            var communications = communicationService
                .Queryable()
                .AsNoTracking()
                .Where( c => c.CategoryId == signUpGroupReminderCategoryId )
                .OrderBy( c => c.Title )
                .ToList();

            foreach ( var communication in communications )
            {
                ddlReminderCommunication.Items.Add( new ListItem( communication.Title, communication.Id.ToString() ) );
            }

            ddlReminderCommunication.SetValue( group.ReminderSystemCommunicationId );
        }

        /// <summary>
        /// Builds the group requirements list.
        /// </summary>
        /// <param name="isEditMode">if set to <c>true</c> [is edit mode].</param>
        /// <param name="rockContext">The rock context.</param>
        private void BuildGroupRequirementsList( bool isEditMode, RockContext rockContext )
        {
            var groupTypeGroupRequirements = new GroupRequirementService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( r => r.GroupRequirementType )
                .Where( r => r.GroupTypeId == this.GroupTypeId )
                .OrderBy( r => r.GroupRequirementType.Name )
                .ToList();

            var anyGroupTypeGroupRequirements = groupTypeGroupRequirements.Any();
            var groupGroupRequirements = this.GroupRequirementsState.OrderBy( r => r.GroupRequirementType.Name ).ToList();
            var anyGroupGroupRequirments = groupGroupRequirements.Any();
            var anyGroupRequirements = anyGroupTypeGroupRequirements || anyGroupGroupRequirments;

            if ( isEditMode )
            {
                var areSpecificGroupRequirementsEnabled = this.CurrentGroupType.EnableSpecificGroupRequirements;

                if ( !anyGroupRequirements && !areSpecificGroupRequirementsEnabled )
                {
                    pnlEditGroupRequirements.Visible = false;
                    return;
                }

                pnlEditGroupRequirements.Visible = true;

                if ( anyGroupTypeGroupRequirements )
                {
                    lGroupTypeGroupRequirements.Text = $"(From <a href='{this.CurrentGroupTypeUrl}' target='_blank'>{this.CurrentGroupType.Name}</a>)";

                    gGroupTypeGroupRequirements.DataSource = groupTypeGroupRequirements;
                    gGroupTypeGroupRequirements.DataBind();
                    rcwGroupTypeGroupRequirements.Visible = true;
                }
                else
                {
                    rcwGroupTypeGroupRequirements.Visible = false;
                }

                rcwGroupRequirements.Visible = anyGroupGroupRequirments || areSpecificGroupRequirementsEnabled;
                if ( !rcwGroupRequirements.Visible )
                {
                    return;
                }

                rcwGroupRequirements.Label = anyGroupTypeGroupRequirements ? "Specific Group Requirements" : "Group Requirements";
                gGroupRequirements.DataSource = groupGroupRequirements;
                gGroupRequirements.DataBind();

                if ( areSpecificGroupRequirementsEnabled )
                {
                    gGroupRequirements.Actions.ShowAdd = true;
                }
                else
                {
                    gGroupRequirements.Actions.ShowAdd = false;
                }
            }
            else
            {
                if ( !anyGroupRequirements )
                {
                    pnlViewGroupRequirements.Visible = false;
                    return;
                }

                pnlViewGroupRequirements.Visible = true;

                var groupRequirements = new List<GroupRequirement>( groupTypeGroupRequirements );
                groupRequirements.AddRange( groupGroupRequirements );

                rptRequirementsList.DataSource = groupRequirements;
                rptRequirementsList.DataBind();
            }
        }

        /// <summary>
        /// Builds the edit mode attributes controls.
        /// </summary>
        /// <param name="group">The group.</param>
        private void BuildEditModeAttributesControls( Group group )
        {
            InitializeAttributeValuesContainer( group, true );

            BuildProjectTypeRadioButtonList( group );
        }

        /// <summary>
        /// Initializes the attribute values container.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="isEditMode">if set to <c>true</c> [is edit mode].</param>
        private void InitializeAttributeValuesContainer( Group group, bool isEditMode )
        {
            if ( isEditMode )
            {
                /*
                 * 12/7/2022 - JPH
                 * 
                 * We're excluding the "Project Type" attribute from this <Rock:AttributeValuesContainer /> control,
                 * and will add it manually as a control managed outside of the AVC. This is for two reasons:
                 * 
                 *   1. This block should only show reminder-related controls if the Project Type == "In-Person",
                 *      and the AVC doesn't provide an easy way to hook into the change event of its child controls.
                 *      Building the Project Type's control outside of the AVC will give us more flexibility in this regard.
                 *   2. We want to display the Project Type attribute as a set of radio buttons,
                 *      which the defined value field type doesn't currently support.
                 * 
                 * Note that any/all other attributes tied to this group/group type WILL be managed by the AVC.
                 * 
                 * Reason: Sign-Up Groups / short term serving projects
                 */
                var excludedAttributes = group.Attributes
                    .Where( a => a.Key == AttributeKey.ProjectType || !a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                    .ToList();

                if ( group.Attributes.Any( a => !excludedAttributes.Contains( a ) ) )
                {
                    avcEditAttributes.ExcludedAttributes = excludedAttributes.Select( a => a.Value ).ToArray();
                    avcEditAttributes.AddEditControls( group );
                    pnlEditAttributes.Visible = true;
                }
                else
                {
                    pnlEditAttributes.Visible = false;
                }
            }
            else
            {
                var excludedAttributes = group.Attributes
                    .Where( a => !a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                    .ToList();

                if ( group.Attributes.Any( a => !excludedAttributes.Contains( a ) ) )
                {
                    avcDisplayAttributes.ExcludedAttributes = excludedAttributes.Select( a => a.Value ).ToArray();
                    avcDisplayAttributes.AddDisplayControls( group );
                    pnlDisplayAttributes.Visible = true;
                }
                else
                {
                    pnlDisplayAttributes.Visible = false;
                }
            }
        }

        /// <summary>
        /// Builds the project type RadioButton list.
        /// </summary>
        /// <param name="group">The group.</param>
        private void BuildProjectTypeRadioButtonList( Group group )
        {
            var projectTypeAttribute = group.Attributes.FirstOrDefault( a => a.Key == AttributeKey.ProjectType ).Value;
            if ( projectTypeAttribute == null || !projectTypeAttribute.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
            {
                pnlProjectType.Visible = false;
                return;
            }

            rblProjectType.Items.Clear();

            var definedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PROJECT_TYPE ).DefinedValues;
            foreach ( var definedValue in definedValues )
            {
                rblProjectType.Items.Add( new ListItem( definedValue.Value, definedValue.Guid.ToString().ToUpper() ) );
            }

            var projectType = GetGroupProjectType( group );
            rblProjectType.SetValue( projectType?.Guid );
            rblProjectType.Required = projectTypeAttribute.IsRequired;
            rblProjectType.Help = projectTypeAttribute.Description;

            pnlProjectType.Visible = true;
        }

        /// <summary>
        /// Gets whether the project type is in person.
        /// </summary>
        /// <param name="projectTypeGuidString">The project type unique identifier string.</param>
        /// <returns></returns>
        private bool GetIsProjectTypeInPerson( string projectTypeGuidString )
        {
            return projectTypeGuidString?.AsGuidOrNull()?.Equals( Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON.AsGuid() ) == true;
        }

        /// <summary>
        /// Tries the navigate to parent group.
        /// </summary>
        /// <param name="parentGroupId">The parent group identifier.</param>
        private void TryNavigateToParentGroup( int? parentGroupId = null )
        {
            var qryParams = GetCommonQueryParams();

            parentGroupId = parentGroupId.HasValue ? parentGroupId : this.PageParentGroupId;
            if ( parentGroupId.HasValue && parentGroupId.Value > 0 )
            {
                qryParams[PageParameterKey.GroupId] = parentGroupId.ToString();
            }

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Gets the common query parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetCommonQueryParams()
        {
            var qryParams = new Dictionary<string, string>();

            var expandedIds = PageParameter( PageParameterKey.ExpandedIds );
            if ( !string.IsNullOrWhiteSpace( expandedIds ) )
            {
                qryParams[PageParameterKey.ExpandedIds] = expandedIds;
            }

            return qryParams;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialogKey">The dialog key.</param>
        private void ShowDialog( string dialogKey )
        {
            hfActiveDialog.Value = dialogKey;

            switch ( dialogKey )
            {
                case DialogKey.AddOpportunity:
                    mdAddOpportunity.Show();
                    break;
                case DialogKey.GroupRequirements:
                    mdGroupRequirement.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case DialogKey.AddOpportunity:
                    mdAddOpportunity.Hide();
                    break;
                case DialogKey.GroupRequirements:
                    mdGroupRequirement.Hide();
                    break;
            }
        }

        /// <summary>
        /// Toggles the group requirement due date controls.
        /// </summary>
        /// <param name="groupRequirementTypes">The group requirement types.</param>
        private void ToggleGroupRequirementDueDateControls( List<GroupRequirementType> groupRequirementTypes = null )
        {
            var groupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();

            GroupRequirementType groupRequirementType = groupRequirementTypes?.FirstOrDefault( t => t.Id == groupRequirementTypeId );
            if ( groupRequirementType == null && groupRequirementTypeId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    groupRequirementType = new GroupRequirementTypeService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .FirstOrDefault( t => t.Id == groupRequirementTypeId );
                }
            }

            if ( groupRequirementType == null )
            {
                ddlDueDateGroupAttribute.Visible = false;
                ddlDueDateGroupAttribute.Required = false;
                dpDueDate.Visible = false;
                dpDueDate.Required = false;
                return;
            }

            switch ( groupRequirementType.DueDateType )
            {
                case DueDateType.Immediate:
                case DueDateType.DaysAfterJoining:
                    {
                        ddlDueDateGroupAttribute.Visible = false;
                        ddlDueDateGroupAttribute.Required = false;
                        dpDueDate.Visible = false;
                        dpDueDate.Required = false;
                        break;
                    }

                case DueDateType.ConfiguredDate:
                    {
                        ddlDueDateGroupAttribute.Visible = false;
                        ddlDueDateGroupAttribute.Required = false;
                        dpDueDate.Visible = true;
                        dpDueDate.Required = true;
                        break;
                    }

                case DueDateType.GroupAttribute:
                    {
                        ddlDueDateGroupAttribute.Visible = true;
                        ddlDueDateGroupAttribute.Required = true;
                        dpDueDate.Visible = false;
                        dpDueDate.Required = false;
                        break;
                    }
            }
        }

        private class Opportunity
        {
            public int GroupId { get; set; }

            public int GroupLocationId { get; set; }

            public int LocationId { get; set; }

            public int ScheduleId { get; set; }

            public string Name { get; set; }

            public DateTime? NextStartDateTime { get; set; }

            public DateTime? LastStartDateTime { get; set; }

            public string FriendlyDateTime { get; set; }

            public string FriendlyLocation { get; set; }

            public int? SlotsMin { get; set; }

            public int? SlotsDesired { get; set; }

            public int? SlotsMax { get; set; }

            public int? SlotsFilled { get; set; }

            public string ReminderAdditionalDetails { get; set; }

            public string ConfirmationAdditionalDetails { get; set; }

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

            public bool IsUpcoming
            {
                get
                {
                    return this.NextStartDateTime.HasValue && this.NextStartDateTime >= RockDateTime.Now;
                }
            }

            private class ProgressState
            {
                public const string Success = "success";
                public const string Warning = "warning";
                public const string Critical = "critical";
                public const string Danger = "danger";
            }

            public string ProgressBar
            {
                get
                {
                    var min = this.SlotsMin.GetValueOrDefault();
                    var desired = this.SlotsDesired.GetValueOrDefault();
                    var max = this.SlotsMax.GetValueOrDefault();
                    var filled = this.SlotsFilled.GetValueOrDefault();
                    var whole = 0;

                    if ( max > 0 )
                    {
                        whole = max;
                    }
                    else if ( desired > 0 )
                    {
                        whole = desired;
                    }
                    else if ( min > 0 )
                    {
                        whole = min;
                    }

                    if ( filled > whole )
                    {
                        whole = filled;
                    }

                    var minPercentage = 0;
                    var desiredPercentage = 0;
                    var maxPercentage = 0;
                    var filledPercentage = 0;

                    if ( whole > 0 )
                    {
                        int GetPercentageOfWhole( int part, bool isThreshold = true )
                        {
                            if ( isThreshold )
                            {
                                // Show threshold "ticks" to the left of the spot that will satisfy a given value.
                                part = part > 1 ? part - 1 : part;
                            }

                            var percentage = ( int ) ( ( double ) part / whole * 100 );
                            return percentage > 100 ? 100 : percentage;
                        }

                        minPercentage = GetPercentageOfWhole( min );
                        desiredPercentage = GetPercentageOfWhole( desired );
                        maxPercentage = GetPercentageOfWhole( max );
                        filledPercentage = GetPercentageOfWhole( filled, false );
                    }

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

                    string GetIndicator( int percentage, bool isMaxIndicator = false )
                    {
                        var shouldShow = percentage > 0 && percentage < 100;
                        if ( shouldShow && isMaxIndicator )
                        {
                            shouldShow = filled > max;
                        }

                        if ( !shouldShow )
                        {
                            return string.Empty;
                        }

                        return $@"
    <div class=""indicator"" style=""left: {percentage}%;""></div>";
                    }

                    return $@"<div class=""progress progress-sign-ups text-{progressState} m-0 flex-fill js-progress-sign-ups"" role=""progressbar"" title=""{ProgressBarTooltip.EncodeHtml()}"" aria-label=""Sign-Ups Progress"">
    <div class=""progress-bar progress-bar-sign-ups bg-{progressState}"" style=""width: {filledPercentage}%""></div>{GetIndicator( minPercentage )}{GetIndicator( desiredPercentage )}{GetIndicator( maxPercentage, true )}
</div>";
                }
            }

            private string ProgressBarTooltip
            {
                get
                {
                    return $@"<div class='d-flex justify-content-between align-items-center'>
    <span class='text-nowrap mr-5'>Slots Filled: {SlotsFilled.GetValueOrDefault():N0}</span>
    <span class='text-nowrap'>
        <div class='{( SlotsMin.GetValueOrDefault() > 0 ? "text-nowrap" : "hide" )}'>Minimum: {SlotsMin.GetValueOrDefault():N0}</div>
        <div class='{( SlotsDesired.GetValueOrDefault() > 0 ? "text-nowrap" : "hide" )}'>Desired: {SlotsDesired.GetValueOrDefault():N0}</div>
        <div class='{( SlotsMax.GetValueOrDefault() > 0 ? "text-nowrap" : "hide" )}'>Maximum: {SlotsMax.GetValueOrDefault():N0}</div>
    </span>
</div>";
                }
            }
        }

        private enum OpportunityTimeframe
        {
            Upcoming = 1,
            Past = 2
        }

        /// <summary>
        /// Binds the opportunities grid.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="shouldForceRefresh">if set to <c>true</c> [should force refresh].</param>
        private void BindOpportunitiesGrid( Group group = null, bool shouldForceRefresh = false )
        {
            using ( var rockContext = new RockContext() )
            {
                group = group ?? new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( g => g.Campus )
                    .FirstOrDefault( g => g.Id == this.GroupId );

                if ( shouldForceRefresh || this.OpportunitiesState?.Any() != true )
                {
                    // Get this Group's opportunities (GroupLocationSchedules).
                    var qryGroupLocationSchedules = new GroupLocationService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( gl => gl.GroupId == this.GroupId )
                        .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                        {
                            gl.Group,
                            GroupLocationId = gl.Id,
                            gl.Location,
                            Schedule = s,
                            Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( glsc => glsc.Schedule.Id == s.Id )
                        } );

                    // Get all participant counts for all opportunities; we'll hook them up to their respective opportunities below.
                    var participantCounts = new GroupMemberAssignmentService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( gma =>
                            !gma.GroupMember.Person.IsDeceased
                            && qryGroupLocationSchedules.Any( gls =>
                                gls.Group.Id == gma.GroupMember.GroupId
                                && gls.Location.Id == gma.LocationId
                                && gls.Schedule.Id == gma.ScheduleId
                            )
                        )
                        .GroupBy( gma => new
                        {
                            gma.GroupMember.GroupId,
                            gma.LocationId,
                            gma.ScheduleId
                        } )
                       .Select( g => new
                       {
                           g.Key.GroupId,
                           g.Key.LocationId,
                           g.Key.ScheduleId,
                           Count = g.Count()
                       } )
                       .ToList();

                    var totalParticipantCount = 0;

                    this.OpportunitiesState = qryGroupLocationSchedules
                        .ToList() // Execute the query.
                        .Select( gls =>
                        {
                            DateTime? nextStartDateTime = gls.Schedule.NextStartDateTime;
                            DateTime? lastStartDateTime = null;

                            if ( !nextStartDateTime.HasValue )
                            {
                                // Give preference to NextStartDateTime, but if not available, fall back to LastStartDateTime. We need something to sort on and display.
                                var startDateTimes = gls.Schedule.GetScheduledStartTimes( RockDateTime.Now, DateTime.MaxValue );
                                var lastScheduledStartDateTime = startDateTimes.LastOrDefault();
                                if ( lastScheduledStartDateTime != default )
                                {
                                    lastStartDateTime = lastScheduledStartDateTime;
                                }
                            }

                            var particpantCount = participantCounts.FirstOrDefault( c =>
                                c.GroupId == gls.Group.Id
                                && c.LocationId == gls.Location.Id
                                && c.ScheduleId == gls.Schedule.Id
                            )?.Count ?? 0;

                            totalParticipantCount += particpantCount;

                            return new Opportunity
                            {
                                GroupId = this.GroupId,
                                GroupLocationId = gls.GroupLocationId,
                                LocationId = gls.Location.Id,
                                ScheduleId = gls.Schedule.Id,
                                Name = gls.Config?.ConfigurationName,
                                NextStartDateTime = nextStartDateTime,
                                LastStartDateTime = lastStartDateTime,
                                FriendlyDateTime = gls.Schedule.ToString(),
                                FriendlyLocation = gls.Location.ToString( true ),
                                SlotsMin = gls.Config?.MinimumCapacity,
                                SlotsDesired = gls.Config?.DesiredCapacity,
                                SlotsMax = gls.Config?.MaximumCapacity,
                                SlotsFilled = particpantCount,
                                ReminderAdditionalDetails = gls.Config?.ReminderAdditionalDetails,
                                ConfirmationAdditionalDetails = gls.Config?.ConfirmationAdditionalDetails
                            };
                        } )
                        .ToList();

                    hfTotalOpportunitiesCount.Value = this.OpportunitiesState.Count().ToString();
                    hfTotalParticipantsCount.Value = totalParticipantCount.ToString();
                }

                Enum.TryParse( bgOpportunitiesTimeframe.SelectedValue, out OpportunityTimeframe timeframe );

                var timeframeOpportunities = this.OpportunitiesState.Where( o => o.IsUpcoming == ( timeframe == OpportunityTimeframe.Upcoming ) );

                List<Opportunity> sortedOpportunities;
                if ( timeframe == OpportunityTimeframe.Upcoming )
                {
                    sortedOpportunities = timeframeOpportunities
                        .OrderBy( o => o.NextOrLastStartDateTime.HasValue ? o.NextOrLastStartDateTime.Value : DateTime.MaxValue )
                        .ThenBy( o => o.Name )
                        .ThenByDescending( o => o.SlotsFilled )
                        .ToList();
                }
                else
                {
                    sortedOpportunities = timeframeOpportunities
                        .OrderByDescending( o => o.NextOrLastStartDateTime.HasValue ? o.NextOrLastStartDateTime.Value : DateTime.MinValue )
                        .ThenBy( o => o.Name )
                        .ThenByDescending( o => o.SlotsFilled )
                        .ToList();
                }

                gOpportunities.RowItemText = timeframe == OpportunityTimeframe.Upcoming ? "Upcoming Opportunity" : "Past Opportunity";
                gOpportunities.Actions.ShowAdd = IsAuthorizedToEdit( group );

                var nameColumn = gOpportunities.ColumnsOfType<RockBoundField>().FirstOrDefault( c => c.DataField == "Name" );
                if ( nameColumn != null )
                {
                    nameColumn.Visible = sortedOpportunities.Any( o => !string.IsNullOrWhiteSpace( o.Name ) );
                }

                gOpportunities.DataSource = sortedOpportunities;
                gOpportunities.DataBind();
            }
        }

        /// <summary>
        /// Shows the opportunity already exists message.
        /// </summary>
        private void ShowOpportunityAlreadyExistsMessage()
        {
            nbOpportunityAlreadyExists.Text = $"A {this.ProjectName} opportunity already exists for the selected Location & Schedule combination.<br />Please edit the existing opportunity or choose a different Location and/or Schedule to add a new opportunity.";
            nbOpportunityAlreadyExists.Visible = true;
        }

        /// <summary>
        /// Shows the no allowed schedule types message.
        /// </summary>
        private void ShowNoAllowedScheduleTypesMessage()
        {
            nbNoAllowedScheduleTypes.Text = $"The <b>{this.CurrentGroupType.Name}</b> group type does not allow Custom or Named Group Schedule Options. Please <a href='{this.CurrentGroupTypeUrl}' target='_blank'>enable at least one of these types</a> to edit opportunities.";
            nbNoAllowedScheduleTypes.Visible = true;
        }

        /// <summary>
        /// Shows the no allowed location picker modes message.
        /// </summary>
        private void ShowNoAllowedLocationPickerModesMessage()
        {
            nbNoAllowedLocationPickerModes.Text = $"The <b>{this.CurrentGroupType.Name}</b> group type does not allow any Location Selection Modes. Please <a href='{this.CurrentGroupTypeUrl}' target='_blank'>enable at least one mode</a> to edit opportunities.";
            nbNoAllowedLocationPickerModes.Visible = true;
        }

        /// <summary>
        /// Tries to initialize the schedule controls.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        private bool TryInitializeScheduleControls( Schedule schedule = null )
        {
            nbScheduleTypeNotAllowed.Visible = false;

            sbSchedule.iCalendarContent = string.Empty;
            InitializeFriendlyScheduleLabel();

            spSchedule.SetValue( null );

            // If we got here, one of these (and maybe both of them) are allowed by this group type.
            var isCustomScheduleAllowed = this.AllowedScheduleTypes.HasFlag( ScheduleType.Custom );
            var isNamedScheduleAllowed = this.AllowedScheduleTypes.HasFlag( ScheduleType.Named );

            ScheduleType selectedScheduleType;

            if ( isCustomScheduleAllowed )
            {
                // This is the preferred default if allowed by the group type, and we're in "Add" mode.
                selectedScheduleType = ScheduleType.Custom;
            }
            else if ( isNamedScheduleAllowed )
            {
                selectedScheduleType = ScheduleType.Named;
            }
            else
            {
                // This is a fail-safe: just in case someone changed the 'gOpportunities_ShowEdit()' logic without also modifying this method.
                ShowNoAllowedScheduleTypesMessage();
                return false;
            }

            // Only show the button group if both types are allowed; otherwise we'll default to the appropriate control below.
            pnlScheduleTypeButtonGroup.Visible = isCustomScheduleAllowed && isNamedScheduleAllowed;

            if ( schedule != null )
            {
                var currentScheduleType = schedule.ScheduleType;

                if ( currentScheduleType == ScheduleType.Custom && isCustomScheduleAllowed )
                {
                    sbSchedule.iCalendarContent = schedule.iCalendarContent;
                    selectedScheduleType = ScheduleType.Custom;
                    InitializeFriendlyScheduleLabel();
                }
                else if ( currentScheduleType == ScheduleType.Named && isNamedScheduleAllowed )
                {
                    spSchedule.SetValue( schedule );
                    selectedScheduleType = ScheduleType.Named;
                }
                else
                {
                    // The Schedule's current ScheduleType is not allowed.
                    ShowScheduleTypeNotAllowed();
                }
            }

            bgScheduleType.SetValue( ( int ) selectedScheduleType );

            ToggleScheduleControls( selectedScheduleType );

            return true;
        }

        /// <summary>
        /// Initializes the friendly schedule label.
        /// </summary>
        private void InitializeFriendlyScheduleLabel()
        {
            lScheduleText.Visible = false;

            if ( !string.IsNullOrWhiteSpace( sbSchedule.iCalendarContent ) )
            {
                var calEvent = InetCalendarHelper.CreateCalendarEvent( sbSchedule.iCalendarContent );
                if ( sbSchedule.IsValid && calEvent != null && calEvent.DtStart != null )
                {
                    var tempSchedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
                    lScheduleText.Text = $"<span class='text-sm'>{tempSchedule.FriendlyScheduleText ?? "Custom"}</span>";
                    lScheduleText.Visible = true;
                }
            }
        }

        /// <summary>
        /// Toggles the schedule controls.
        /// </summary>
        /// <param name="scheduleType">Type of the schedule.</param>
        private void ToggleScheduleControls( ScheduleType scheduleType )
        {
            rcwScheduleBuilder.Visible = scheduleType == ScheduleType.Custom;
            spSchedule.Visible = scheduleType == ScheduleType.Named;
        }

        /// <summary>
        /// Shows the schedule type not allowed.
        /// </summary>
        private void ShowScheduleTypeNotAllowed()
        {
            nbScheduleTypeNotAllowed.Text = $"The <b>{this.CurrentGroupType.Name}</b> group type does not allow this opportunity's current schedule; please add a new schedule below.";
            nbScheduleTypeNotAllowed.Visible = true;
        }

        /// <summary>
        /// Initializes the location picker.
        /// </summary>
        /// <param name="location">The location.</param>
        private void InitializeLocationPicker( Location location )
        {
            nbLocationModeNotAllowed.Visible = false;
            lpLocation.Location = null;
            lpLocation.AllowedPickerModes = AllowedLocationPickerModes;
            lpLocation.SetBestPickerModeForLocation( null );

            if ( location != null )
            {
                var currentPickerMode = lpLocation.GetBestPickerModeForLocation( location );

                if ( AllowedLocationPickerModes.HasFlag( currentPickerMode ) )
                {
                    lpLocation.CurrentPickerMode = currentPickerMode;
                    lpLocation.Location = location;
                }
                else
                {
                    // The Location's current LocationPickerMode is not allowed.
                    ShowLocationPickerModeNotAllowed();
                }
            }
        }

        /// <summary>
        /// Shows the location picker mode not allowed message.
        /// </summary>
        private void ShowLocationPickerModeNotAllowed()
        {
            nbLocationModeNotAllowed.Text = $"The <b>{this.CurrentGroupType.Name}</b> group type does not allow this opportunity's current location; please add a new location below.";
            nbLocationModeNotAllowed.Visible = true;
        }

        private class ParsedScheduleAndLocation
        {
            public ScheduleType NewScheduleType { get; set; }

            public string NewICalendarContent { get; set; }

            public int? NewScheduleId { get; set; }

            public int? NewLocationId { get; set; }

            public List<string> ErrorMessages { get; } = new List<string>();

            public bool IsValid => !ErrorMessages.Any();
        }

        /// <summary>
        /// Parses the new schedule and location.
        /// </summary>
        /// <returns></returns>
        private ParsedScheduleAndLocation ParseNewScheduleAndLocation()
        {
            var parsed = new ParsedScheduleAndLocation();

            Enum.TryParse( bgScheduleType.SelectedValue, out ScheduleType newScheduleType );

            parsed.NewScheduleType = newScheduleType;

            // Ensure a valid Schedule was selected.
            if ( newScheduleType == ScheduleType.Custom )
            {
                parsed.NewICalendarContent = sbSchedule.iCalendarContent;
                var calEvent = InetCalendarHelper.CreateCalendarEvent( parsed.NewICalendarContent );
                if ( !sbSchedule.IsValid || calEvent == null || calEvent.DtStart == null )
                {
                    parsed.ErrorMessages.Add( "Schedule is required." );
                }
            }

            if ( newScheduleType == ScheduleType.Named )
            {
                parsed.NewScheduleId = spSchedule.SelectedValueAsId();
                if ( !spSchedule.IsValid || !parsed.NewScheduleId.HasValue )
                {
                    parsed.ErrorMessages.Add( "Schedule is required." );
                }
            }

            // Ensure a valid Location was selected.
            parsed.NewLocationId = lpLocation.Location?.Id;
            if ( !lpLocation.IsValid || !parsed.NewLocationId.HasValue )
            {
                parsed.ErrorMessages.Add( "Location is required." );
            }

            return parsed;
        }

        /// <summary>
        /// Ensures no duplicate opportunity exists.
        /// </summary>
        /// <param name="parsed">The parsed.</param>
        /// <returns></returns>
        private bool EnsureNoDuplicateOpportunityExists( ParsedScheduleAndLocation parsed = null )
        {
            nbOpportunityAlreadyExists.Visible = false;

            parsed = parsed ?? ParseNewScheduleAndLocation();

            if ( !parsed.IsValid )
            {
                // The visible controls aren't valid, so we can't determine if there is a duplicate yet.
                return true;
            }

            // If selected Schedule is Named, ensure Schedule & Location combo doesn't already exist for the current Sign-Up Group.
            if ( parsed.NewScheduleId.HasValue && this.OpportunitiesState.Any( o => o.GroupLocationId != this.EditGroupLocationId
                && o.ScheduleId == parsed.NewScheduleId
                && o.LocationId == parsed.NewLocationId.Value ) )
            {
                ShowOpportunityAlreadyExistsMessage();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the formatted error messages.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private string GetFormattedErrorMessages( List<string> errorMessages )
        {
            var items = new StringBuilder();
            foreach ( var errorMessage in errorMessages )
            {
                items.AppendLine( $"        <li>{errorMessage}</li>" );
            }

            return $@"<div class='alert alert-validation'>
    Please correct the following:
    <ul>
{items}
    </ul>
</div>";
        }

        #endregion
    }
}
