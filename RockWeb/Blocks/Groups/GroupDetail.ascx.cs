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
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;
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
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group." )]
    [GroupTypesField( "Group Types Include", "Select group types to show in this block.  Leave all unchecked to show all but the excluded group types.", false, key: "GroupTypes", order: 0 )]
    [GroupTypesField( "Group Types Exclude", "Select group types to exclude from this block.", false, key: "GroupTypesExclude", order: 1 )]
    [BooleanField( "Limit to Security Role Groups", "", false, "", 3 )]
    [BooleanField( "Limit to Group Types that are shown in navigation", "", false, "", 4, "LimitToShowInNavigationGroupTypes" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The style of maps to use", false, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK, "", 5 )]
    [LinkedPage( "Group Map Page", "The page to display detailed group map.", false, "", "", 6 )]
    [LinkedPage( "Attendance Page", "The page to display attendance list.", false, "", "", 7 )]
    [LinkedPage( "Registration Instance Page", "The page to display registration details.", false, "", "", 7 )]
    [LinkedPage( "Event Item Occurrence Page", "The page to display event item occurrence details.", false, "", "", 8 )]
    [LinkedPage( "Content Item Page", "The page to display registration details.", false, "", "", 9 )]
    [BooleanField( "Show Copy Button", "Copies the group and all of its associated authorization rules", false, "", 10 )]
    [LinkedPage( "Group List Page", "The page to display related Group List.", false, "", "", 11 )]
    [LinkedPage( "Fundraising Progress Page", "The page to display fundraising progress for all its members.", false, "", "", 12 )]
    [BooleanField( "Show Location Addresses", "Determines if the location address should be shown when viewing the group details.", true, order: 13 )]
    [BooleanField( "Prevent Selecting Inactive Campus", "Should inactive campuses be excluded from the campus field when editing a group?.", false, "", 14 )]
    [LinkedPage( "Group History Page", "The page to display group history.", false, "", "", 15 )]


    [LinkedPage( "Group Scheduler Page",
        Key = "GroupSchedulerPage",
        Description ="The page to schedule this group.",
        IsRequired = false,
        DefaultValue = "1815D8C6-7C4A-4C05-A810-CF23BA937477,D0F198E2-6111-4EC1-8D1D-55AC10E28D04",
        Order = 16)]

    [BooleanField( "Enable Group Tags", "If enabled, the tags will be shown.", true, "", 17 )]
    public partial class GroupDetail : RockBlock, IDetailBlock
    {
        #region Constants

        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";
        private const string ENABLE_GROUP_TAGS = "EnableGroupTags";

        #endregion

        #region Fields

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

        /// <summary>
        /// Used in binding data to the grid, also alows for detecting existing locations
        /// </summary>
        private class GridLocation
        {
            public Guid Guid { get; set; }
            public Location Location { get; set; }
            public string Type { get; set; }
            public int Order { get; set; }
            public string Schedules { get; set; }
        }

        #endregion

        #region Properties

        private string LocationTypeTab { get; set; }

        private int CurrentGroupTypeId { get; set; }

        private List<GroupLocation> GroupLocationsState { get; set; }

        private List<InheritedAttribute> GroupMemberAttributesInheritedState { get; set; }

        private List<Attribute> GroupMemberAttributesState { get; set; }

        private List<GroupRequirement> GroupRequirementsState { get; set; }

        private bool AllowMultipleLocations { get; set; }

        private List<GroupSync> GroupSyncState { get; set; }

        private List<GroupMemberWorkflowTrigger> MemberWorkflowTriggersState { get; set; }

        private GroupTypeCache CurrentGroupTypeCache
        {
            get
            {
                return GroupTypeCache.Get( CurrentGroupTypeId );
            }

            set
            {
                CurrentGroupTypeId = value != null ? value.Id : 0;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            LocationTypeTab = ViewState["LocationTypeTab"] as string ?? MEMBER_LOCATION_TAB_TITLE;
            CurrentGroupTypeId = ViewState["CurrentGroupTypeId"] as int? ?? 0;

            // NOTE: These things are converted to JSON prior to going into ViewState, so the json variable could be null or the string "null"!
            string json = ViewState["GroupLocationsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupLocationsState = new List<GroupLocation>();
            }
            else
            {
                GroupLocationsState = JsonConvert.DeserializeObject<List<GroupLocation>>( json );
            }

            json = ViewState["GroupMemberAttributesInheritedState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMemberAttributesInheritedState = new List<InheritedAttribute>();
            }
            else
            {
                GroupMemberAttributesInheritedState = JsonConvert.DeserializeObject<List<InheritedAttribute>>( json );
            }

            json = ViewState["GroupMemberAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMemberAttributesState = new List<Attribute>();
            }
            else
            {
                GroupMemberAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            json = ViewState["GroupRequirementsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupRequirementsState = new List<GroupRequirement>();
            }
            else
            {
                GroupRequirementsState = JsonConvert.DeserializeObject<List<GroupRequirement>>( json ) ?? new List<GroupRequirement>();
            }

            // get the GroupRole for each GroupRequirement from the database it case it isn't serialized, and we'll need it
            var groupRoleIds = GroupRequirementsState.Where( a => a.GroupRoleId.HasValue && a.GroupRole == null ).Select( a => a.GroupRoleId.Value ).Distinct().ToList();
            if ( groupRoleIds.Any() )
            {
                var groupRoles = new GroupTypeRoleService( new RockContext() ).GetByIds( groupRoleIds );
                GroupRequirementsState.ForEach( a =>
                {
                    if ( a.GroupRoleId.HasValue )
                    {
                        a.GroupRole = groupRoles.FirstOrDefault( b => b.Id == a.GroupRoleId );
                    }
                } );
            }

            AllowMultipleLocations = ViewState["AllowMultipleLocations"] as bool? ?? false;

            json = ViewState["GroupSyncState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupSyncState = new List<GroupSync>();
            }
            else
            {
                GroupSyncState = JsonConvert.DeserializeObject<List<GroupSync>>( json );
            }

            json = ViewState["MemberWorkflowTriggersState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MemberWorkflowTriggersState = new List<GroupMemberWorkflowTrigger>();
            }
            else
            {
                MemberWorkflowTriggersState = JsonConvert.DeserializeObject<List<GroupMemberWorkflowTrigger>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLocations.DataKeyNames = new string[] { "Guid" };
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.GridRebind += gLocations_GridRebind;

            gGroupMemberAttributesInherited.Actions.ShowAdd = false;
            gGroupMemberAttributesInherited.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupMemberAttributesInherited.GridRebind += gGroupMemberAttributesInherited_GridRebind;

            gGroupMemberAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupMemberAttributes.Actions.ShowAdd = true;
            gGroupMemberAttributes.Actions.AddClick += gGroupMemberAttributes_Add;
            gGroupMemberAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupMemberAttributes.GridRebind += gGroupMemberAttributes_GridRebind;
            gGroupMemberAttributes.GridReorder += gGroupMemberAttributes_GridReorder;

            SecurityField groupMemberAttributeSecurityField = gGroupMemberAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            groupMemberAttributeSecurityField.EntityTypeId = EntityTypeCache.GetId<Attribute>() ?? 0;

            gGroupRequirements.DataKeyNames = new string[] { "Guid" };
            gGroupRequirements.Actions.ShowAdd = true;
            gGroupRequirements.Actions.AddClick += gGroupRequirements_Add;
            gGroupRequirements.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupRequirements.GridRebind += gGroupRequirements_GridRebind;

            gGroupSyncs.DataKeyNames = new string[] { "Guid" };
            gGroupSyncs.Actions.ShowAdd = true;
            gGroupSyncs.Actions.AddClick += gGroupSyncs_Add;
            gGroupSyncs.GridRebind += gGroupSyncs_GridRebind;

            gMemberWorkflowTriggers.DataKeyNames = new string[] { "Guid" };
            gMemberWorkflowTriggers.Actions.ShowAdd = true;
            gMemberWorkflowTriggers.Actions.AddClick += gMemberWorkflowTriggers_Add;
            gMemberWorkflowTriggers.EmptyDataText = Server.HtmlEncode( None.Text );
            gMemberWorkflowTriggers.GridRebind += gMemberWorkflowTriggers_GridRebind;
            gMemberWorkflowTriggers.GridReorder += gMemberWorkflowTriggers_GridReorder;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Group ) ).Id;

            rblScheduleSelect.BindToEnum<ScheduleType>();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupDetail );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int? groupId = 0;
            if ( !string.IsNullOrWhiteSpace( PageParameter( "GroupId" ) ) )
            {
                groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            }

            if ( !Page.IsPostBack )
            {
                if ( groupId.HasValue )
                {
                    ShowDetail( groupId.Value, PageParameter( "ParentGroupId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }

                btnCopy.Visible = GetAttributeValue( "ShowCopyButton" ).AsBoolean();
            }
            else
            {
                nbNotAllowedToEdit.Visible = false;
                nbInvalidWorkflowType.Visible = false;
                nbInvalidParentGroup.Visible = false;
                ShowDialog();
            }

            // Rebuild the attribute controls on postback based on group type
            if ( pnlDetails.Visible )
            {
                if ( CurrentGroupTypeId > 0 )
                {
                    var group = new Group { GroupTypeId = CurrentGroupTypeId };

                    ShowGroupTypeEditDetails( CurrentGroupTypeCache, group, false );
                }
            }

            RockContext rockContext = new RockContext();

            if ( groupId.HasValue && groupId.Value != 0 )
            {
                var group = GetGroup( groupId.Value, rockContext );
                if ( group != null )
                {
                    // Handle tags
                    taglGroupTags.EntityTypeId = group.TypeId;
                    taglGroupTags.EntityGuid = group.Guid;
                    taglGroupTags.CategoryGuid = GetAttributeValue( "TagCategory" ).AsGuidOrNull();
                    taglGroupTags.GetTagValues( null );
                    taglGroupTags.Visible = GetAttributeValue( ENABLE_GROUP_TAGS ).AsBoolean() && group.GroupType.EnableGroupTag;

                    FollowingsHelper.SetFollowing( group, pnlFollowing, this.CurrentPerson );
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["LocationTypeTab"] = LocationTypeTab;
            ViewState["CurrentGroupTypeId"] = CurrentGroupTypeId;
            ViewState["GroupLocationsState"] = JsonConvert.SerializeObject( GroupLocationsState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesInheritedState"] = JsonConvert.SerializeObject( GroupMemberAttributesInheritedState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesState"] = JsonConvert.SerializeObject( GroupMemberAttributesState, Formatting.None, jsonSetting );
            ViewState["GroupRequirementsState"] = JsonConvert.SerializeObject( GroupRequirementsState, Formatting.None, jsonSetting );
            ViewState["AllowMultipleLocations"] = AllowMultipleLocations;
            ViewState["GroupSyncState"] = JsonConvert.SerializeObject( GroupSyncState, Formatting.None, jsonSetting );
            ViewState["MemberWorkflowTriggersState"] = JsonConvert.SerializeObject( MemberWorkflowTriggersState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupId = PageParameter( pageReference, "GroupId" ).AsIntegerOrNull();
            if ( groupId != null )
            {
                Group group = new GroupService( new RockContext() ).Get( groupId.Value );
                if ( group != null )
                {
                    breadCrumbs.Add( new BreadCrumb( group.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the btnArchive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing thmuch the same event data.</param>
        protected void btnArchive_Click( object sender, EventArgs e )
        {
            int? parentGroupId = null;
            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = groupService.Get( hfGroupId.Value.AsInteger() );

            if ( group != null )
            {
                if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to archive this group.", ModalAlertType.Information );
                    return;
                }

                parentGroupId = group.ParentGroupId;
                groupService.Archive( group, this.CurrentPersonAliasId, true );

                rockContext.SaveChanges();
            }

            NavigateAfterDeleteOrArchive( parentGroupId );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? parentGroupId = null;
            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = groupService.Get( hfGroupId.Value.AsInteger() );

            if ( group != null )
            {
                if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this group.", ModalAlertType.Information );
                    return;
                }

                parentGroupId = group.ParentGroupId;
                string errorMessage;
                if ( !groupService.CanDelete( group, out errorMessage, true ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                bool isSecurityRoleGroup = group.IsActive && ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) );
                if ( isSecurityRoleGroup )
                {
                    foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                    {
                        authService.Delete( auth );
                    }
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

                // NOTE: groupService.Delete will automatically Archive instead Delete if this Group has GroupHistory enabled, but since this block has UI logic for Archive vs Delete, we can do a direct Archive in btnArchive_Click
                groupService.Delete( group );

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Clear();
                }
            }

            NavigateAfterDeleteOrArchive( parentGroupId );
        }

        /// <summary>
        /// Navigates after a group is deleted or archived
        /// </summary>
        /// <param name="parentGroupId">The parent group identifier.</param>
        private void NavigateAfterDeleteOrArchive( int? parentGroupId )
        {
            // reload page, selecting the deleted group's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentGroupId != null )
            {
                qryParams["GroupId"] = parentGroupId.ToString();
            }

            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            if ( GetAttributeValue( "GroupListPage" ).AsGuid() != Guid.Empty )
            {
                NavigateToLinkedPage( "GroupListPage", qryParams );
            }
            else
            {
                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Group group;
            bool wasSecurityRole = false;
            bool triggersUpdated = false;
            bool checkinDataUpdated = false;

            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            GroupRequirementService groupRequirementService = new GroupRequirementService( rockContext );
            GroupMemberWorkflowTriggerService groupMemberWorkflowTriggerService = new GroupMemberWorkflowTriggerService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );
            GroupSyncService groupSyncService = new GroupSyncService( rockContext );

            var roleGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
            int roleGroupTypeId = roleGroupType != null ? roleGroupType.Id : int.MinValue;

            if ( CurrentGroupTypeId == 0 )
            {
                ddlGroupType.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( GroupType.FriendlyTypeName ) );
                return;
            }

            int groupId = hfGroupId.Value.AsInteger();

            if ( groupId == 0 )
            {
                group = new Group();
                group.IsSystem = false;
                group.Name = string.Empty;
            }
            else
            {
                group = groupService.Queryable( "Schedule,GroupLocations.Schedules" ).Where( g => g.Id == groupId ).FirstOrDefault();
                wasSecurityRole = group.IsActive && ( group.IsSecurityRole || group.GroupTypeId == roleGroupTypeId );

                // Remove any locations that removed in the UI
                var selectedLocations = GroupLocationsState.Select( l => l.Guid );
                foreach ( var groupLocation in group.GroupLocations.Where( l => !selectedLocations.Contains( l.Guid ) ).ToList() )
                {
                    List<GroupLocationScheduleConfig> accessModGroupLocationScheduleConfigsToRemove = new List<GroupLocationScheduleConfig>();

                    // Create a list of configurations to be removed cannot modify a collection that is being iterated
                    foreach ( var grouplocationScheduleConfig in groupLocation.GroupLocationScheduleConfigs )
                    {
                        accessModGroupLocationScheduleConfigsToRemove.Add( grouplocationScheduleConfig );
                    }

                    // Remove the dependent group location schedule configurations
                    foreach ( var deleteConfig in accessModGroupLocationScheduleConfigsToRemove )
                    {
                        groupLocation.GroupLocationScheduleConfigs.Remove( deleteConfig );
                    }
                    // Remove 
                    group.GroupLocations.Remove( groupLocation );
                    groupLocationService.Delete( groupLocation );
                    checkinDataUpdated = true;
                }

                // Remove any group requirements that removed in the UI
                var selectedGroupRequirements = GroupRequirementsState.Select( a => a.Guid );
                foreach ( var groupRequirement in group.GetGroupRequirements( rockContext ).Where( a => a.GroupId.HasValue ).Where( a => !selectedGroupRequirements.Contains( a.Guid ) ).ToList() )
                {
                    groupRequirementService.Delete( groupRequirement );
                }

                // Remove any triggers that were removed in the UI
                var selectedTriggerGuids = MemberWorkflowTriggersState.Select( r => r.Guid );
                foreach ( var trigger in group.GroupMemberWorkflowTriggers.Where( r => !selectedTriggerGuids.Contains( r.Guid ) ).ToList() )
                {
                    group.GroupMemberWorkflowTriggers.Remove( trigger );
                    groupMemberWorkflowTriggerService.Delete( trigger );
                    triggersUpdated = true;
                }

                // Remove any GroupSyncs that were removed in the UI
                var selectedGroupSyncs = GroupSyncState.Select( s => s.Guid );
                foreach ( var groupSync in group.GroupSyncs.Where( s => !selectedGroupSyncs.Contains( s.Guid ) ).ToList() )
                {
                    group.GroupSyncs.Remove( groupSync );
                    groupSyncService.Delete( groupSync );
                }
            }

            List<GroupRequirement> groupRequirementsToInsert = new List<GroupRequirement>();

            // Add/Update any group requirements that were added or changed in the UI (we already removed the ones that were removed above)
            foreach ( var groupRequirementState in GroupRequirementsState )
            {
                GroupRequirement groupRequirement = group.GetGroupRequirements( rockContext ).Where( a => a.GroupId.HasValue ).Where( a => a.Guid == groupRequirementState.Guid ).FirstOrDefault();
                if ( groupRequirement == null )
                {
                    groupRequirement = new GroupRequirement();
                    groupRequirementsToInsert.Add( groupRequirement );
                }

                groupRequirement.CopyPropertiesFrom( groupRequirementState );
            }

            var deletedSchedules = new List<int>();
            // Add/Update any group locations that were added or changed in the UI (we already removed the ones that were removed above)
            foreach ( var groupLocationState in GroupLocationsState )
            {
                GroupLocation groupLocation = group.GroupLocations.Where( l => l.Guid == groupLocationState.Guid ).FirstOrDefault();
                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation();
                    group.GroupLocations.Add( groupLocation );
                }
                else
                {
                    groupLocationState.Id = groupLocation.Id;
                    groupLocationState.Guid = groupLocation.Guid;

                    var selectedSchedules = groupLocationState.Schedules.Select( s => s.Guid ).ToList();
                    foreach ( var schedule in groupLocation.Schedules.Where( s => !selectedSchedules.Contains( s.Guid ) ).ToList() )
                    {
                        deletedSchedules.Add( schedule.Id );
                        groupLocation.Schedules.Remove( schedule );
                    }
                }

                groupLocation.CopyPropertiesFrom( groupLocationState );

                var existingSchedules = groupLocation.Schedules.Select( s => s.Guid ).ToList();
                var existingGroupLocationConfigs = groupLocation.GroupLocationScheduleConfigs.Select( g => g );

                var addedSchedules = new List<int>();

                foreach ( var scheduleState in groupLocationState.Schedules.Where( s => !existingSchedules.Contains( s.Guid ) ).ToList() )
                {
                    var schedule = scheduleService.Get( scheduleState.Guid );
                    if ( schedule != null )
                    {
                        addedSchedules.Add( schedule.Id );
                        groupLocation.Schedules.Add( schedule );
                    }
                }


                // Select group configs with min, desired or max values 
                var modifiedScheduleConfigs = groupLocationState.GroupLocationScheduleConfigs
                    .Where( s => groupLocation.GroupLocationScheduleConfigs
                        .Where( exs => ( exs.ScheduleId == s.ScheduleId )
                            && exs.GroupLocationId == s.GroupLocationId
                            && ( exs.MinimumCapacity != s.MinimumCapacity
                            || exs.DesiredCapacity != s.DesiredCapacity
                            || exs.MaximumCapacity != s.MaximumCapacity ) ).Any() );

                // Handles case where group location schedules exisited without group location schedule configs
                var newGroupLocationScheduleConfigs = existingGroupLocationConfigs.Count() > 0 
                    ? groupLocationState.GroupLocationScheduleConfigs
                    .Where( s => addedSchedules.Contains( s.ScheduleId ) ).ToList()
                    : groupLocationState.GroupLocationScheduleConfigs;

                // Add scheduling configs
                foreach ( var addedGroupLocationScheduleConfigs in newGroupLocationScheduleConfigs )
                {
                    groupLocation.GroupLocationScheduleConfigs.Add(
                        new GroupLocationScheduleConfig
                        {
                            ScheduleId = addedGroupLocationScheduleConfigs.ScheduleId,
                            MinimumCapacity = addedGroupLocationScheduleConfigs.MinimumCapacity,
                            DesiredCapacity = addedGroupLocationScheduleConfigs.DesiredCapacity,
                            MaximumCapacity = addedGroupLocationScheduleConfigs.MaximumCapacity
                        } );
                }

                // Update the scheduling configs
                foreach ( var updatedSchedulingConfig in modifiedScheduleConfigs )
                {
                    var currentSchedulingConfig = groupLocation.GroupLocationScheduleConfigs
                        .Where( curr => curr.ScheduleId == updatedSchedulingConfig.ScheduleId
                        && curr.GroupLocationId == updatedSchedulingConfig.GroupLocationId ).FirstOrDefault();

                    currentSchedulingConfig.MinimumCapacity = updatedSchedulingConfig.MinimumCapacity;
                    currentSchedulingConfig.DesiredCapacity = updatedSchedulingConfig.DesiredCapacity;
                    currentSchedulingConfig.MaximumCapacity = updatedSchedulingConfig.MaximumCapacity;
                }

                // Delete the scheduling configs
                foreach ( var deletedScheduleId in deletedSchedules )
                {
                    var associatedConfig = groupLocation.GroupLocationScheduleConfigs.Where( cfg => cfg.Schedule != null && cfg.Schedule.Id == deletedScheduleId ).FirstOrDefault();
                    groupLocation.GroupLocationScheduleConfigs.Remove( associatedConfig );
                }

                checkinDataUpdated = true;
            }

            // Add/update GroupSyncs
            foreach ( var groupSyncState in GroupSyncState )
            {
                GroupSync groupSync = group.GroupSyncs.Where( s => s.Guid == groupSyncState.Guid ).FirstOrDefault();
                if ( groupSync == null )
                {
                    groupSync = new GroupSync();
                    group.GroupSyncs.Add( groupSync );
                }

                groupSync.CopyPropertiesFrom( groupSyncState );
            }

            // Add/update workflow triggers
            foreach ( var triggerState in MemberWorkflowTriggersState )
            {
                GroupMemberWorkflowTrigger trigger = group.GroupMemberWorkflowTriggers.Where( r => r.Guid == triggerState.Guid ).FirstOrDefault();
                if ( trigger == null )
                {
                    trigger = new GroupMemberWorkflowTrigger();
                    group.GroupMemberWorkflowTriggers.Add( trigger );
                }
                else
                {
                    triggerState.Id = trigger.Id;
                    triggerState.Guid = trigger.Guid;
                }

                trigger.CopyPropertiesFrom( triggerState );
                triggersUpdated = true;
            }

            group.Name = tbName.Text;
            group.Description = tbDescription.Text;
            group.CampusId = cpCampus.SelectedCampusId;
            group.GroupTypeId = CurrentGroupTypeId;
            group.ParentGroupId = gpParentGroup.SelectedValueAsInt();
            group.StatusValueId = dvpGroupStatus.SelectedValueAsId();
            group.GroupCapacity = nbGroupCapacity.Text.AsIntegerOrNull();
            group.RequiredSignatureDocumentTemplateId = ddlSignatureDocumentTemplate.SelectedValueAsInt();
            group.IsSecurityRole = cbIsSecurityRole.Checked;
            group.IsActive = cbIsActive.Checked;
            group.IsPublic = cbIsPublic.Checked;

            group.SchedulingMustMeetRequirements = cbSchedulingMustMeetRequirements.Checked;
            group.AttendanceRecordRequiredForCheckIn = ddlAttendanceRecordRequiredForCheckIn.SelectedValueAsEnum<AttendanceRecordRequiredForCheckIn>();
            group.ScheduleCancellationPersonAliasId = ppScheduleCancellationPerson.PersonAliasId;

            string iCalendarContent = string.Empty;

            // If unique schedule option was selected, but a schedule was not defined, set option to 'None'
            var scheduleType = rblScheduleSelect.SelectedValueAsEnum<ScheduleType>( ScheduleType.None );
            if ( scheduleType == ScheduleType.Custom )
            {
                iCalendarContent = sbSchedule.iCalendarContent;
                var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
                if ( calEvent == null || calEvent.DTStart == null )
                {
                    scheduleType = ScheduleType.None;
                }
            }

            if ( scheduleType == ScheduleType.Weekly )
            {
                if ( !dowWeekly.SelectedDayOfWeek.HasValue )
                {
                    scheduleType = ScheduleType.None;
                }
            }

            int? oldScheduleId = hfUniqueScheduleId.Value.AsIntegerOrNull();
            if ( scheduleType == ScheduleType.Custom || scheduleType == ScheduleType.Weekly )
            {
                if ( !oldScheduleId.HasValue || group.Schedule == null )
                {
                    group.Schedule = new Schedule();

                    // NOTE: Schedule Name should be set to string.Empty to indicate that it is a Custom or Weekly schedule and not a "Named" schedule
                    group.Schedule.Name = string.Empty;
                }

                if ( scheduleType == ScheduleType.Custom )
                {
                    group.Schedule.iCalendarContent = iCalendarContent;
                    group.Schedule.WeeklyDayOfWeek = null;
                    group.Schedule.WeeklyTimeOfDay = null;
                }
                else
                {
                    group.Schedule.iCalendarContent = null;
                    group.Schedule.WeeklyDayOfWeek = dowWeekly.SelectedDayOfWeek;
                    group.Schedule.WeeklyTimeOfDay = timeWeekly.SelectedTime;
                }
            }
            else
            {
                // If group did have a unique schedule, delete that schedule
                if ( oldScheduleId.HasValue )
                {
                    var schedule = scheduleService.Get( oldScheduleId.Value );
                    if ( schedule != null && string.IsNullOrEmpty( schedule.Name ) )
                    {
                        // Make sure this is the only thing using this schedule.
                        string errorMessage;
                        if ( scheduleService.CanDelete( schedule, out errorMessage ) )
                        {
                            scheduleService.Delete( schedule );
                        }
                    }
                }

                if ( scheduleType == ScheduleType.Named )
                {
                    group.ScheduleId = spSchedule.SelectedValueAsId();
                }
                else
                {
                    group.ScheduleId = null;
                }
            }

            if ( group.ParentGroupId == group.Id )
            {
                gpParentGroup.ShowErrorMessage( "Group cannot be a Parent Group of itself." );
                return;
            }

            group.LoadAttributes();
            Helper.GetEditValues( phGroupAttributes, group );

            group.GroupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            if ( group.ParentGroupId.HasValue )
            {
                group.ParentGroup = groupService.Get( group.ParentGroupId.Value );
            }

            if ( group.GroupType.ShowAdministrator )
            {
                group.GroupAdministratorPersonAliasId = ppAdministrator.PersonAliasId;
            }

            // Check to see if group type is allowed as a child of new parent group.
            if ( group.ParentGroup != null )
            {
                var allowedGroupTypeIds = GetAllowedGroupTypes( group.ParentGroup, rockContext ).Select( t => t.Id ).ToList();
                if ( !allowedGroupTypeIds.Contains( group.GroupTypeId ) )
                {
                    var groupType = CurrentGroupTypeCache;
                    nbInvalidParentGroup.Text = string.Format( "The '{0}' group does not allow child groups with a '{1}' group type.", group.ParentGroup.Name, groupType != null ? groupType.Name : string.Empty );
                    nbInvalidParentGroup.Visible = true;
                    return;
                }
            }

            // Check to see if user is still allowed to edit with selected group type and parent group
            if ( !group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbNotAllowedToEdit.Visible = true;
                return;
            }

            if ( !Page.IsValid )
            {
                return;
            }

            // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
            // So, make sure a message is displayed in the validation summary
            cvGroup.IsValid = group.IsValid;

            if ( !cvGroup.IsValid )
            {
                cvGroup.ErrorMessage = group.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }


            // use WrapTransaction since SaveAttributeValues does its own RockContext.SaveChanges()
            rockContext.WrapTransaction( () =>
            {
                var adding = group.Id.Equals( 0 );
                if ( adding )
                {
                    groupService.Add( group );
                }

                // Save changes because we'll need the group's Id next...
                rockContext.SaveChanges();

                if ( adding )
                {
                    // Add ADMINISTRATE to the person who added the group 
                    Rock.Security.Authorization.AllowPerson( group, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
                }

                if ( groupRequirementsToInsert.Any() )
                {
                    groupRequirementsToInsert.ForEach( a => a.GroupId = group.Id );
                    groupRequirementService.AddRange( groupRequirementsToInsert );
                }

                group.SaveAttributeValues( rockContext );

                /* Take care of Group Member Attributes */
                var entityTypeId = EntityTypeCache.Get( typeof( GroupMember ) ).Id;
                string qualifierColumn = "GroupId";
                string qualifierValue = group.Id.ToString();

                // Get the existing attributes for this entity type and qualifier value
                var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

                // Delete any of those attributes that were removed in the UI
                var selectedAttributeGuids = GroupMemberAttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    attributeService.Delete( attr );
                }

                // Update the Attributes that were assigned in the UI
                foreach ( var attributeState in GroupMemberAttributesState )
                {
                    Rock.Attribute.Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
                }

                rockContext.SaveChanges();

                if ( group.IsActive == false && cbInactivateChildGroups.Checked )
                {
                    var allActiveChildGroupsId = groupService.GetAllDescendents( group.Id ).Where( a => a.IsActive ).Select( a => a.Id ).ToList();
                    var allActiveChildGroups = groupService.GetByIds( allActiveChildGroupsId );
                    foreach ( var childGroup in allActiveChildGroups )
                    {
                        if ( childGroup.IsActive )
                        {
                            childGroup.IsActive = false;
                        }
                    }

                    rockContext.SaveChanges();
                }
            } );

            bool isNowSecurityRole = group.IsActive && ( group.IsSecurityRole || group.GroupTypeId == roleGroupTypeId );

            if ( group != null && wasSecurityRole )
            {
                if ( !isNowSecurityRole )
                {
                    // If this group was a SecurityRole, but no longer is, flush
                    Rock.Security.Authorization.Clear();
                }
            }
            else
            {
                if ( isNowSecurityRole )
                {
                    // New security role, flush
                    Rock.Security.Authorization.Clear();
                }
            }

            if ( triggersUpdated )
            {
                GroupMemberWorkflowTriggerService.RemoveCachedTriggers();
            }

            // Flush the kiosk devices cache if this group updated check-in data and its group type takes attendance
            if ( checkinDataUpdated && group.GroupType.TakesAttendance )
            {
                Rock.CheckIn.KioskDevice.Clear();
            }

            var qryParams = new Dictionary<string, string>();
            qryParams["GroupId"] = group.Id.ToString();
            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupId.Value.Equals( "0" ) )
            {
                int? parentGroupId = PageParameter( "ParentGroupId" ).AsIntegerOrNull();
                if ( parentGroupId.HasValue )
                {
                    // Cancelling on Add, and we know the parentGroupID, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( parentGroupId != 0 )
                    {
                        qryParams["GroupId"] = parentGroupId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    if ( GetAttributeValue( "GroupListPage" ).AsGuid() != Guid.Empty )
                    {
                        NavigateToLinkedPage( "GroupListPage" );
                    }
                    else
                    {
                        NavigateToPage( RockPage.Guid, null );
                    }
                }
            }
            else
            {
                // Canceling on Edit.  Return to Details
                ShowReadonlyDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var authService = new AuthService( rockContext );
            var attributeService = new AttributeService( rockContext );

            int groupId = hfGroupId.ValueAsInt();
            var group = groupService.Queryable( "GroupType" )
                    .Where( g => g.Id == groupId )
                    .FirstOrDefault();

            if ( group != null )
            {
                group.LoadAttributes( rockContext );

                // Clone the group
                var newGroup = group.Clone( false );
                newGroup.CreatedByPersonAlias = null;
                newGroup.CreatedByPersonAliasId = null;
                newGroup.CreatedDateTime = RockDateTime.Now;
                newGroup.ModifiedByPersonAlias = null;
                newGroup.ModifiedByPersonAliasId = null;
                newGroup.ModifiedDateTime = RockDateTime.Now;
                newGroup.Id = 0;
                newGroup.Guid = Guid.NewGuid();
                newGroup.IsSystem = false;
                newGroup.Name = group.Name + " - Copy";

                if ( group.ScheduleId.HasValue && group.Schedule.ScheduleType != ScheduleType.Named )
                {
                    newGroup.Schedule = new Schedule();
                    // NOTE: Schedule Name should be set to string.Empty to indicate that it is a Custom or Weekly schedule and not a "Named" schedule
                    newGroup.Schedule.Name = string.Empty;
                    newGroup.Schedule.iCalendarContent = group.Schedule.iCalendarContent;
                    newGroup.Schedule.WeeklyDayOfWeek = group.Schedule.WeeklyDayOfWeek;
                    newGroup.Schedule.WeeklyTimeOfDay = group.Schedule.WeeklyTimeOfDay;

                }

                var auths = authService.GetByGroup( group.Id );
                rockContext.WrapTransaction( () =>
                {
                    groupService.Add( newGroup );
                    rockContext.SaveChanges();

                    newGroup.LoadAttributes( rockContext );
                    if ( group.Attributes != null && group.Attributes.Any() )
                    {
                        foreach ( var attributeKey in group.Attributes.Select( a => a.Key ) )
                        {
                            string value = group.GetAttributeValue( attributeKey );
                            newGroup.SetAttributeValue( attributeKey, value );
                        }
                    }

                    newGroup.SaveAttributeValues( rockContext );

                    /* Take care of Group Member Attributes */
                    var entityTypeId = EntityTypeCache.Get( typeof( GroupMember ) ).Id;
                    string qualifierColumn = "GroupId";
                    string qualifierValue = group.Id.ToString();

                    // Get the existing attributes for this entity type and qualifier value
                    var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

                    foreach ( var attribute in attributes )
                    {
                        var newAttribute = attribute.Clone( false );
                        newAttribute.Id = 0;
                        newAttribute.Guid = Guid.NewGuid();
                        newAttribute.IsSystem = false;
                        newAttribute.EntityTypeQualifierValue = newGroup.Id.ToString();

                        foreach ( var qualifier in attribute.AttributeQualifiers )
                        {
                            var newQualifier = qualifier.Clone( false );
                            newQualifier.Id = 0;
                            newQualifier.Guid = Guid.NewGuid();
                            newQualifier.IsSystem = false;

                            newAttribute.AttributeQualifiers.Add( qualifier );
                        }

                        attributeService.Add( newAttribute );
                    }

                    rockContext.SaveChanges();

                    foreach ( var auth in auths )
                    {
                        var newAuth = auth.Clone( false );
                        newAuth.Id = 0;
                        newAuth.Guid = Guid.NewGuid();
                        newAuth.GroupId = newGroup.Id;
                        newAuth.CreatedByPersonAlias = null;
                        newAuth.CreatedByPersonAliasId = null;
                        newAuth.CreatedDateTime = RockDateTime.Now;
                        newAuth.ModifiedByPersonAlias = null;
                        newAuth.ModifiedByPersonAliasId = null;
                        newAuth.ModifiedDateTime = RockDateTime.Now;
                        authService.Add( newAuth );
                    }

                    rockContext.SaveChanges();
                    Rock.Security.Authorization.Clear();
                } );

                NavigateToCurrentPage( new Dictionary<string, string> { { "GroupId", newGroup.Id.ToString() } } );
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Grouptype changed, so load up the new attributes and set controls to the default attribute values
            CurrentGroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0;
            if ( CurrentGroupTypeId > 0 )
            {
                var group = new Group { GroupTypeId = CurrentGroupTypeId };
                var groupType = CurrentGroupTypeCache;
                SetScheduleControls( groupType, null );
                ShowGroupTypeEditDetails( groupType, group, true );
                BindInheritedAttributes( CurrentGroupTypeId, new AttributeService( new RockContext() ) );
                BindGroupRequirementsGrid();
                BindAdministratorPerson( group, groupType );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlParentGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlParentGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            int? parentGroupId = gpParentGroup.SelectedValueAsInt();
            Group parentGroup = null;
            if ( parentGroupId.HasValue )
            {
                parentGroup = new GroupService( rockContext ).Queryable( "GroupType" )
                    .Where( g => g.Id == parentGroupId.Value )
                    .FirstOrDefault();
            }

            var groupTypeQry = GetAllowedGroupTypes( parentGroup, rockContext );

            List<GroupType> groupTypes = groupTypeQry.OrderBy( a => a.Name ).ToList();
            if ( groupTypes.Count() > 1 )
            {
                // Add a empty option so they are forced to choose
                groupTypes.Insert( 0, new GroupType { Id = 0, Name = string.Empty } );
            }

            // If the currently selected GroupType isn't an option anymore, set selected GroupType to null
            if ( ddlGroupType.Visible )
            {
                int? selectedGroupTypeId = ddlGroupType.SelectedValueAsInt();
                if ( ddlGroupType.SelectedValue != null )
                {
                    if ( !groupTypes.Any( a => a.Id.Equals( selectedGroupTypeId ?? 0 ) ) )
                    {
                        selectedGroupTypeId = null;
                    }
                }

                ddlGroupType.DataSource = groupTypes;
                ddlGroupType.DataBind();

                if ( selectedGroupTypeId.HasValue )
                {
                    CurrentGroupTypeId = selectedGroupTypeId.Value;
                    ddlGroupType.SelectedValue = selectedGroupTypeId.ToString();
                }
                else
                {
                    CurrentGroupTypeId = 0;
                    ddlGroupType.SelectedValue = null;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLocationType_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                LocationTypeTab = lb.Text;

                rptLocationTypes.DataSource = _tabs;
                rptLocationTypes.DataBind();
            }

            ShowSelectedPane();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            btnCopy.Visible = GetAttributeValue( "ShowCopyButton" ).AsBoolean();
            var currentGroup = GetGroup( hfGroupId.Value.AsInteger() );
            if ( currentGroup != null )
            {
                ShowReadonlyDetails( currentGroup );
            }
            else
            {
                string groupId = PageParameter( "GroupId" );
                if ( !string.IsNullOrWhiteSpace( groupId ) )
                {
                    ShowDetail( groupId.AsInteger(), PageParameter( "ParentGroupId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScheduleSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rblScheduleSelect_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetScheduleDisplay();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        public void ShowDetail( int groupId )
        {
            ShowDetail( groupId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="parentGroupId">The parent group identifier.</param>
        public void ShowDetail( int groupId, int? parentGroupId )
        {
            Group group = null;

            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            RockContext rockContext = new RockContext();

            if ( !groupId.Equals( 0 ) )
            {
                group = GetGroup( groupId, rockContext );
                if ( group == null )
                {
                    pnlDetails.Visible = false;
                    nbNotFoundOrArchived.Visible = true;
                    return;
                }
            }

            if ( group == null )
            {
                group = new Group { Id = 0, IsActive = true, IsPublic = true, ParentGroupId = parentGroupId, Name = string.Empty };
                wpGeneral.Expanded = true;

                if ( parentGroupId.HasValue )
                {
                    // Set the new group's parent group (so security checks work)
                    var parentGroup = new GroupService( rockContext ).Get( parentGroupId.Value );
                    if ( parentGroup != null )
                    {
                        // Start by setting the group type to the same as the parent
                        group.ParentGroup = parentGroup;

                        // Get all the allowed GroupTypes as defined by the parent group type
                        var allowedChildGroupTypesOfParentGroup = GetAllowedGroupTypes( parentGroup, rockContext ).ToList();

                        // Narrow it down to group types that the current user is allowed to edit 
                        var authorizedGroupTypes = new List<GroupType>();
                        foreach ( var allowedGroupType in allowedChildGroupTypesOfParentGroup )
                        {
                            // To see if the user is authorized for the group type, test by setting the new group's grouptype and see if they are authorized
                            group.GroupTypeId = allowedGroupType.Id;
                            group.GroupType = allowedGroupType;

                            if ( group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                authorizedGroupTypes.Add( allowedGroupType );

                                // They have EDIT auth to at least one GroupType, so they are allowed to try to add this group
                                editAllowed = true;
                            }
                        }

                        // Exactly one grouptype is allowed/authorized, so it is safe to default this new group to it
                        if ( authorizedGroupTypes.Count() == 1 )
                        {
                            group.GroupType = authorizedGroupTypes.First();
                            group.GroupTypeId = group.GroupType.Id;
                        }
                        else
                        {
                            // more than one grouptype is allowed/authorized, so don't default it so they are forced to pick which one
                            group.GroupType = null;
                            group.GroupTypeId = 0;
                        }
                    }
                }
            }

            viewAllowed = editAllowed || group.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = editAllowed || group.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            hfGroupId.Value = group.Id.ToString();

            // Render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( group.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( Group.FriendlyTypeName );
            }

            string roleLimitWarnings;
            nbRoleLimitWarning.Visible = group.GetGroupTypeRoleLimitWarnings( out roleLimitWarnings );
            nbRoleLimitWarning.Text = roleLimitWarnings;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnArchive.Visible = false;
                ShowReadonlyDetails( group );
            }
            else
            {
                btnEdit.Visible = true;
                if ( group.Id > 0 )
                {
                    ShowReadonlyDetails( group );
                }
                else
                {
                    ShowEditDetails( group );
                }
            }
        }

        /// <summary>
        /// Sets the highlight label visibility.
        /// </summary>
        /// <param name="group">The group.</param>
        private void SetHighlightLabelVisibility( Group group, bool readOnly )
        {
            if ( readOnly )
            {
                // If we are just showing readonly detail of the group, we don't have to worry about the highlight labels changing while editing on the client
                hlInactive.Visible = !group.IsActive;
                hlIsPrivate.Visible = !group.IsPublic;
            }
            else
            {
                // In edit mode, the labels will have javascript handle if/when they are shown
                hlInactive.Visible = true;
                hlIsPrivate.Visible = true;
            }

            hlArchived.Visible = group.IsArchived;

            if ( group.IsActive )
            {
                hlInactive.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if ( group.IsPublic )
            {
                hlIsPrivate.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( Group group )
        {
            if ( group.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Group.FriendlyTypeName ).FormatAsHtmlTitle();

                // Hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            }

            SetHighlightLabelVisibility( group, false );

            ddlGroupType.Visible = group.Id == 0;
            lGroupType.Visible = group.Id != 0;

            SetEditMode( true );

            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            nbGroupCapacity.Text = group.GroupCapacity.ToString();
            cbIsSecurityRole.Checked = group.IsSecurityRole;
            cbIsActive.Checked = group.IsActive;
            cbIsPublic.Checked = group.IsPublic;

            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns( rockContext );

            ddlSignatureDocumentTemplate.SetValue( group.RequiredSignatureDocumentTemplateId );
            gpParentGroup.SetValue( group.ParentGroup ?? groupService.Get( group.ParentGroupId ?? 0 ) );


            // Hide sync and requirements panel if no admin access
            bool canAdministrate = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            wpGroupSync.Visible = canAdministrate;
            wpGroupRequirements.Visible = canAdministrate;
            wpGroupMemberAttributes.Visible = canAdministrate;

            GroupSyncState = new List<GroupSync>();
            foreach ( var sync in group.GroupSyncs )
            {
                // Clone it first so that we don't end up with a giant JSON object in viewstate (that includes the Group and GroupMembers)
                var syncClone = sync.Clone( false );

                // add the stuff that the grid needs
                syncClone.GroupTypeRole = new GroupTypeRoleService( rockContext ).Get( syncClone.GroupTypeRoleId );
                syncClone.SyncDataView = new DataViewService( rockContext ).Get( syncClone.SyncDataViewId );

                GroupSyncState.Add( syncClone );
            }

            BindGroupSyncGrid();

            // Only Rock admins can alter if the group is a security role
            cbIsSecurityRole.Visible = groupService.GroupHasMember( new Guid( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS ), CurrentUser.PersonId );

            // GroupType depends on Selected ParentGroup
            ddlParentGroup_SelectedIndexChanged( null, null );
            gpParentGroup.Label = "Parent Group";

            if ( group.Id == 0 && group.GroupType == null && ddlGroupType.Items.Count > 1 )
            {
                if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean() )
                {
                    // Default GroupType for new Group to "Security Roles"  if LimittoSecurityRoleGroups
                    var securityRoleGroupType = GroupTypeCache.GetSecurityRoleGroupType();
                    if ( securityRoleGroupType != null )
                    {
                        CurrentGroupTypeId = securityRoleGroupType.Id;
                        ddlGroupType.SetValue( securityRoleGroupType.Id );
                    }
                    else
                    {
                        ddlGroupType.SelectedIndex = 0;
                    }
                }
                else
                {
                    // If this is a new group (and not "LimitToSecurityRoleGroups", and there is more than one choice for GroupType, default to no selection so they are forced to choose (vs unintentionally choosing the default one)
                    ddlGroupType.SelectedIndex = 0;
                }
            }
            else
            {
                CurrentGroupTypeId = group.GroupTypeId;
                if ( CurrentGroupTypeId == 0 )
                {
                    CurrentGroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0;
                }

                var groupType = GroupTypeCache.Get( CurrentGroupTypeId, rockContext );
                lGroupType.Text = groupType != null ? groupType.Name : string.Empty;
                ddlGroupType.SetValue( CurrentGroupTypeId );
            }

            cpCampus.IncludeInactive = !GetAttributeValue( "PreventSelectingInactiveCampus" ).AsBoolean();
            cpCampus.SelectedCampusId = group.CampusId;

            GroupRequirementsState = group.GetGroupRequirements( rockContext ).Where( a => a.GroupId.HasValue ).ToList();
            GroupLocationsState = group.GroupLocations.ToList();

            var groupTypeCache = CurrentGroupTypeCache;
            BindAdministratorPerson( group, groupTypeCache );
            nbGroupCapacity.Visible = groupTypeCache != null && groupTypeCache.GroupCapacityRule != GroupCapacityRule.None;
            SetScheduleControls( groupTypeCache, group );
            ShowGroupTypeEditDetails( groupTypeCache, group, true );

            cbSchedulingMustMeetRequirements.Checked = group.SchedulingMustMeetRequirements;
            ddlAttendanceRecordRequiredForCheckIn.SetValue( group.AttendanceRecordRequiredForCheckIn.ConvertToInt() );
            if ( group.ScheduleCancellationPersonAlias != null )
            {
                ppScheduleCancellationPerson.SetValue( group.ScheduleCancellationPersonAlias.Person );
            }
            else
            {
                ppScheduleCancellationPerson.SetValue( null );
            }

            // If this block's attribute limit group to SecurityRoleGroups, don't let them edit the SecurityRole checkbox value
            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean() )
            {
                cbIsSecurityRole.Enabled = false;
                cbIsSecurityRole.Checked = true;
            }

            string qualifierValue = group.Id.ToString();
            GroupMemberAttributesState = attributeService.GetByEntityTypeId( new GroupMember().TypeId, true ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();
            BindGroupMemberAttributesGrid();

            BindInheritedAttributes( group.GroupTypeId, attributeService );

            BindGroupRequirementsGrid();

            MemberWorkflowTriggersState = new List<GroupMemberWorkflowTrigger>();
            foreach ( var trigger in group.GroupMemberWorkflowTriggers )
            {
                MemberWorkflowTriggersState.Add( trigger );
            }

            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Bind the administrator person picker.
        /// </summary>
        /// <param name="group">The group.</param>
        private void BindAdministratorPerson( Group group, GroupTypeCache groupType )
        {
            var showAdministrator = groupType != null && groupType.ShowAdministrator;
            ppAdministrator.Visible = showAdministrator;
            if ( showAdministrator )
            {
                ppAdministrator.Label = groupType.AdministratorTerm;
                ppAdministrator.Help = string.Format( "Provide the person who is the {0} of the group.", groupType.AdministratorTerm );
                if ( group.GroupAdministratorPersonAliasId.HasValue )
                {
                    ppAdministrator.SetValue( group.GroupAdministratorPersonAlias.Person );
                }
            }
        }

        /// <summary>
        /// Shows the group type edit details.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowGroupTypeEditDetails( GroupTypeCache groupType, Group group, bool setValues )
        {
            if ( group == null )
            {
                // Shouldn't happen
                return;
            }

            // Save value to viewstate for use later when binding location grid
            AllowMultipleLocations = groupType != null && groupType.AllowMultipleLocations;

            // Show/Hide different Panel based on permissions from the group type
            if ( group.GroupTypeId != 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    GroupType selectedGroupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
                    if ( selectedGroupType != null )
                    {
                        wpGroupSync.Visible = selectedGroupType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) && ( selectedGroupType.AllowGroupSync || GroupSyncState.Any() );
                        wpMemberWorkflowTriggers.Visible = selectedGroupType.AllowSpecificGroupMemberWorkflows || group.GroupMemberWorkflowTriggers.Any();
                    }
                }
            }

            if ( groupType != null )
            {
                if ( setValues )
                {
                    dvpGroupStatus.DefinedTypeId = groupType.GroupStatusDefinedTypeId;
                    if ( groupType.GroupStatusDefinedType != null )
                    {
                        dvpGroupStatus.Label = groupType.GroupStatusDefinedType.ToString();
                    }

                    dvpGroupStatus.Visible = groupType.GroupStatusDefinedTypeId.HasValue;
                    dvpGroupStatus.SetValue( group.StatusValueId );
                }
            }
            else
            {
                dvpGroupStatus.Visible = false;
            }

            if ( groupType != null && groupType.LocationSelectionMode != GroupLocationPickerMode.None )
            {
                wpMeetingDetails.Visible = true;
                gLocations.Visible = true;
                BindLocationsGrid();
            }
            else
            {
                wpMeetingDetails.Visible = pnlSchedule.Visible;
                gLocations.Visible = false;
            }

            gLocations.Columns[2].Visible = groupType != null && ( groupType.EnableLocationSchedules ?? false );
            spSchedules.Visible = groupType != null && ( groupType.EnableLocationSchedules ?? false );


                if ( groupType != null && groupType.LocationSelectionMode != GroupLocationPickerMode.None )
                {
                    wpMeetingDetails.Visible = true;
                    gLocations.Visible = true;
                    BindLocationsGrid();
                }
                else
                {
                    wpMeetingDetails.Visible = pnlSchedule.Visible;
                    gLocations.Visible = false;
                }

                gLocations.Columns[2].Visible = groupType != null && ( groupType.EnableLocationSchedules ?? false );
                spSchedules.Visible = groupType != null && ( groupType.EnableLocationSchedules ?? false );

                phGroupAttributes.Controls.Clear();
                group.LoadAttributes();

            if ( group.Attributes != null && group.Attributes.Any() )
            {
                wpGroupAttributes.Visible = true;
                var excludeForEdit = group.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            }
            else
            {
                wpGroupAttributes.Visible = false;
            }

            wpScheduling.Visible = groupType != null && groupType.IsSchedulingEnabled;
        }

        /// <summary>
        /// Sets the schedule controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        private void SetScheduleControls( GroupTypeCache groupType, Group group )
        {
            if ( group != null )
            {
                dowWeekly.SelectedDayOfWeek = null;
                timeWeekly.SelectedTime = null;
                sbSchedule.iCalendarContent = string.Empty;
                spSchedule.SetValue( null );

                if ( group.Schedule != null )
                {
                    switch ( group.Schedule.ScheduleType )
                    {
                        case ScheduleType.Named:
                            spSchedule.SetValue( group.Schedule );
                            break;
                        case ScheduleType.Custom:
                            hfUniqueScheduleId.Value = group.Schedule.Id.ToString();
                            sbSchedule.iCalendarContent = group.Schedule.iCalendarContent;
                            break;
                        case ScheduleType.Weekly:
                            hfUniqueScheduleId.Value = group.Schedule.Id.ToString();
                            dowWeekly.SelectedDayOfWeek = group.Schedule.WeeklyDayOfWeek;
                            timeWeekly.SelectedTime = group.Schedule.WeeklyTimeOfDay;
                            break;
                    }
                }
            }

            pnlSchedule.Visible = false;
            rblScheduleSelect.Items.Clear();

            ListItem liNone = new ListItem( "None", "0" );
            liNone.Selected = group != null && ( group.Schedule == null || group.Schedule.ScheduleType == ScheduleType.None );
            rblScheduleSelect.Items.Add( liNone );

            if ( groupType != null && ( groupType.AllowedScheduleTypes & ScheduleType.Weekly ) == ScheduleType.Weekly )
            {
                ListItem li = new ListItem( "Weekly", "1" );
                li.Selected = group != null && group.Schedule != null && group.Schedule.ScheduleType == ScheduleType.Weekly;
                rblScheduleSelect.Items.Add( li );
                pnlSchedule.Visible = true;
            }

            if ( groupType != null && ( groupType.AllowedScheduleTypes & ScheduleType.Custom ) == ScheduleType.Custom )
            {
                ListItem li = new ListItem( "Custom", "2" );
                li.Selected = group != null && group.Schedule != null && group.Schedule.ScheduleType == ScheduleType.Custom;
                rblScheduleSelect.Items.Add( li );
                pnlSchedule.Visible = true;
            }

            if ( groupType != null && ( groupType.AllowedScheduleTypes & ScheduleType.Named ) == ScheduleType.Named )
            {
                ListItem li = new ListItem( "Named", "4" );
                li.Selected = group != null && group.Schedule != null && group.Schedule.ScheduleType == ScheduleType.Named;
                rblScheduleSelect.Items.Add( li );
                pnlSchedule.Visible = true;
            }

            SetScheduleDisplay();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Group group )
        {
            btnDelete.Visible = !group.IsSystem;
            btnArchive.Visible = false;

            var rockContext = new RockContext();
            GroupTypeCache groupType = GroupTypeCache.Get( group.GroupTypeId );

            // If History is enabled (and this isn't an IsSystem group), additional logic for if the Archive or Delete button is visible
            if ( !group.IsSystem )
            {
                if ( !group.IsArchived )
                {
                    if ( groupType != null && groupType.EnableGroupHistory )
                    {
                        bool hasGroupHistory = new GroupHistoricalService( rockContext ).Queryable().Any( a => a.GroupId == group.Id );
                        if ( hasGroupHistory )
                        {
                            // If the group has GroupHistory enabled, and has group history snapshots, prompt to archive instead of delete
                            btnDelete.Visible = false;
                            btnArchive.Visible = true;
                        }
                    }
                }
                else
                {
                    btnDelete.Visible = false;
                }
            }

            SetHighlightLabelVisibility( group, true );
            SetEditMode( false );

            string groupIconHtml = string.Empty;
            if ( groupType != null )
            {
                groupIconHtml = !string.IsNullOrWhiteSpace( groupType.IconCssClass ) ?
                    string.Format( "<i class='{0}' ></i>", groupType.IconCssClass ) : string.Empty;

                if ( groupType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    var groupTypeDetailPage = new PageReference( Rock.SystemGuid.Page.GROUP_TYPE_DETAIL ).BuildUrl();
                    hlType.Text = string.Format( "<a href='{0}?groupTypeId={1}'>{2}</a>", groupTypeDetailPage, groupType.Id, groupType.Name );
                }
                else
                {
                    hlType.Text = groupType.Name;
                }
                hlType.ToolTip = groupType.Description;
            }

            hfGroupId.SetValue( group.Id );
            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();

            pdAuditDetails.SetEntity( group, ResolveRockUrl( "~" ) );

            if ( group.Campus != null )
            {
                hlCampus.Visible = true;
                hlCampus.Text = group.Campus.Name;
            }
            else
            {
                hlCampus.Visible = false;
            }

            var pageParams = new Dictionary<string, string>();
            pageParams.Add( "GroupId", group.Id.ToString() );

            hlAttendance.Visible = groupType != null && groupType.TakesAttendance;
            hlAttendance.NavigateUrl = LinkedPageUrl( "AttendancePage", pageParams );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Group", group );
            mergeFields.Add( "RegistrationInstancePage", LinkedPageRoute( "RegistrationInstancePage" ) );
            mergeFields.Add( "EventItemOccurrencePage", LinkedPageRoute( "EventItemOccurrencePage" ) );
            mergeFields.Add( "ContentItemPage", LinkedPageRoute( "ContentItemPage" ) );
            mergeFields.Add( "ShowLocationAddresses", GetAttributeValue( "ShowLocationAddresses" ).AsBoolean() );

            var mapStyleValue = DefinedValueCache.Get( GetAttributeValue( "MapStyle" ) );
            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            mergeFields.Add( "MapStyle", mapStyleValue );

            string groupMapUrl = LinkedPageUrl( "GroupMapPage", pageParams );
            mergeFields.Add( "GroupMapUrl", groupMapUrl );

            if ( groupMapUrl.IsNotNullOrWhiteSpace() )
            {
                hlMap.Visible = true;
                hlMap.NavigateUrl = groupMapUrl;
            }
            else
            {
                hlMap.Visible = false;
            }

            string groupSchedulerUrl = LinkedPageUrl( "GroupSchedulerPage", pageParams );
            if ( groupSchedulerUrl.IsNotNullOrWhiteSpace() )
            {
                hlGroupScheduler.Visible = groupType != null && groupType.IsSchedulingEnabled;
                hlGroupScheduler.NavigateUrl = groupSchedulerUrl;
            }
            else
            {
                hlGroupScheduler.Visible = false;
            }


            string groupHistoryUrl = LinkedPageUrl( "GroupHistoryPage", pageParams );
            mergeFields.Add( "GroupHistoryUrl", groupHistoryUrl );
            if ( groupHistoryUrl.IsNotNullOrWhiteSpace() )
            {
                hlGroupHistory.Visible = groupType != null && groupType.EnableGroupHistory;
                hlGroupHistory.NavigateUrl = groupHistoryUrl;
            }
            else
            {
                hlGroupHistory.Visible = false;
            }

            if ( groupType != null )
            {
                string template = groupType.GroupViewLavaTemplate;
                lContent.Text = template.ResolveMergeFields( mergeFields ).ResolveClientIds( upnlGroupDetail.ClientID );
            }

            string fundraisingProgressUrl = LinkedPageUrl( "FundraisingProgressPage", pageParams );
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext ).Queryable().Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising ).Select( a => a.Id ).ToList();

            if ( fundraisingProgressUrl.IsNotNullOrWhiteSpace() && fundraisingGroupTypeIdList.Contains( group.GroupTypeId ) )
            {
                hlFundraisingProgress.NavigateUrl = fundraisingProgressUrl;
                hlFundraisingProgress.Visible = true;
            }
            else
            {
                hlFundraisingProgress.Visible = false;
            }

            btnSecurity.Visible = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.EntityId = group.Id;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        private Group GetGroup( int groupId, RockContext rockContext = null )
        {
            string key = string.Format( "Group:{0}", groupId );
            Group group = RockPage.GetSharedItem( key ) as Group;
            if ( group == null )
            {
                rockContext = rockContext ?? new RockContext();
                group = new GroupService( rockContext )
                    .Queryable()
                    .Include( g => g.GroupType )
                    .Include( g => g.GroupLocations.Select( s => s.Schedules ) )
                    .Include( g => g.GroupSyncs )
                    .Where( g => g.Id == groupId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, group );
            }

            return group;
        }

        /// <summary>
        /// Gets the allowed group types.
        /// </summary>
        /// <param name="parentGroup">The parent group.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IQueryable<GroupType> GetAllowedGroupTypes( Group parentGroup, RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );

            var groupTypeQry = groupTypeService.Queryable();

            // Limit GroupType selection to what Block Attributes allow
            List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();
            List<Guid> groupTypeExcludeGuids = GetAttributeValue( "GroupTypesExclude" ).SplitDelimitedValues().AsGuidList();
            if ( groupTypeIncludeGuids.Any() )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeIncludeGuids.Contains( a.Guid ) );
            }
            else if ( groupTypeExcludeGuids.Any() )
            {
                groupTypeQry = groupTypeQry.Where( a => !groupTypeExcludeGuids.Contains( a.Guid ) );
            }

            // Next, limit GroupType to ChildGroupTypes that the ParentGroup allows
            if ( parentGroup != null )
            {
                List<int> allowedChildGroupTypeIds = parentGroup.GroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
                groupTypeQry = groupTypeQry.Where( a => allowedChildGroupTypeIds.Contains( a.Id ) );
            }

            // Limit to GroupTypes where ShowInNavigation=True depending on block setting
            if ( GetAttributeValue( "LimitToShowInNavigationGroupTypes" ).AsBoolean() )
            {
                groupTypeQry = groupTypeQry.Where( a => a.ShowInNavigation );
            }

            return groupTypeQry;
        }

        /// <summary>
        /// Registrations the instance URL.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        protected string RegistrationInstanceUrl( int registrationInstanceId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "RegistrationInstanceId", registrationInstanceId.ToString() );
            return LinkedPageUrl( "RegistrationInstancePage", qryParams );
        }

        /// <summary>
        /// Events the item occurrence URL.
        /// </summary>
        /// <param name="eventItemOccurrenceId">The event item occurrence identifier.</param>
        /// <returns></returns>
        protected string EventItemOccurrenceUrl( int eventItemOccurrenceId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "EventItemOccurrenceId", eventItemOccurrenceId.ToString() );
            return LinkedPageUrl( "EventItemOccurrencePage", qryParams );
        }

        /// <summary>
        /// Contents the item URL.
        /// </summary>
        /// <param name="contentItemId">The content item identifier.</param>
        /// <returns></returns>
        protected string ContentItemUrl( int contentItemId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "ContentItemId", contentItemId.ToString() );
            return LinkedPageUrl( "ContentItemPage", qryParams );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( RockContext rockContext )
        {
            ddlSignatureDocumentTemplate.Items.Clear();
            ddlSignatureDocumentTemplate.Items.Add( new ListItem() );
            foreach ( var documentType in new SignatureDocumentTemplateService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( t => t.Name ) )
            {
                ddlSignatureDocumentTemplate.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }

            ddlAttendanceRecordRequiredForCheckIn.BindToEnum<AttendanceRecordRequiredForCheckIn>();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "LOCATIONS":
                    dlgLocations.Show();
                    break;
                case "GROUPMEMBERATTRIBUTES":
                    dlgGroupMemberAttribute.Show();
                    break;
                case "GROUPREQUIREMENTS":
                    mdGroupRequirement.Show();
                    break;
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgMemberWorkflowTriggers.Show();
                    break;
                case "GROUPSYNCSETTINGS":
                    mdGroupSyncSettings.Show();
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
                case "LOCATIONS":
                    dlgLocations.Hide();
                    break;
                case "GROUPMEMBERATTRIBUTES":
                    dlgGroupMemberAttribute.Hide();
                    break;
                case "GROUPREQUIREMENTS":
                    mdGroupRequirement.Hide();
                    break;
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgMemberWorkflowTriggers.Hide();
                    break;
                case "GROUPSYNCSETTINGS":
                    mdGroupSyncSettings.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected string GetTabClass( object property )
        {
            if ( property.ToString() == LocationTypeTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            if ( LocationTypeTab.Equals( MEMBER_LOCATION_TAB_TITLE ) )
            {
                pnlMemberSelect.Visible = true;
                pnlLocationSelect.Visible = false;
            }
            else if ( LocationTypeTab.Equals( OTHER_LOCATION_TAB_TITLE ) )
            {
                pnlMemberSelect.Visible = false;
                pnlLocationSelect.Visible = true;
            }
        }

        /// <summary>
        /// Binds the inherited attributes.
        /// </summary>
        /// <param name="inheritedGroupTypeId">The inherited group type identifier.</param>
        /// <param name="attributeService">The attribute service.</param>
        private void BindInheritedAttributes( int? inheritedGroupTypeId, AttributeService attributeService )
        {
            GroupMemberAttributesInheritedState = new List<InheritedAttribute>();

            while ( inheritedGroupTypeId.HasValue )
            {
                var inheritedGroupType = GroupTypeCache.Get( inheritedGroupTypeId.Value );
                if ( inheritedGroupType != null )
                {
                    string qualifierValue = inheritedGroupType.Id.ToString();

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new GroupMember().TypeId, false ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        GroupMemberAttributesInheritedState.Add( new InheritedAttribute(
                            attribute.Name,
                            attribute.Key,
                            attribute.Description,
                            Page.ResolveUrl( "~/GroupType/" + attribute.EntityTypeQualifierValue ),
                            inheritedGroupType.Name ) );
                    }

                    inheritedGroupTypeId = inheritedGroupType.InheritedGroupTypeId;
                }
                else
                {
                    inheritedGroupTypeId = null;
                }
            }

            BindGroupMemberAttributesInheritedGrid();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetAttributeListOrder( List<Attribute> attributeList )
        {
            int order = 0;
            attributeList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( List<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Sets the group type role list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetMemberWorkflowTriggerListOrder( List<GroupMemberWorkflowTrigger> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the group type role list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderMemberWorkflowTriggerList( List<GroupMemberWorkflowTrigger> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the SpSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void spSchedules_SelectItem( object sender, EventArgs e )
        {
            if ( !LocationSelected() )
            {
                nbGroupLocationEditMessage.Text = "Please select a location.";
                nbGroupLocationEditMessage.Visible = true;
                spSchedules.SetValue( 0 );
                return;
            }
            var rockContext = new RockContext();
            // Get the selected schedules
            var schedules = new ScheduleService( rockContext ).GetByIds( spSchedules.SelectedValuesAsInt().ToList() ).ToList();

            var locationGuid = Guid.Parse( hfLocationGuid.Value );
            var groupLocationState = GroupLocationsState.FirstOrDefault( l => l.Guid.Equals( locationGuid ) );

            List<GroupLocationScheduleConfig> currentgroupLocationScheduleConfigs;
         
            if ( groupLocationState != null  && groupLocationState.GroupLocationScheduleConfigs.Count() > 0)
            {
                // Schedules from view state
                var groupLocatinStateSchedules = groupLocationState.Schedules;
                var groupLocationStateScheduleIds = groupLocatinStateSchedules.Select( s => s.Id );

                // schedule from 
                var selectedScheduleIds = schedules.Select( n => n.Id );

                // GroupLocationScheduleConfigs from view state
                currentgroupLocationScheduleConfigs = groupLocationState.GroupLocationScheduleConfigs.ToList();
                var existingConfigCount = currentgroupLocationScheduleConfigs.Count();
                if ( existingConfigCount > schedules.Count() )
                {
                    List<GroupLocationScheduleConfig> accessModConfigsToRemove = new List<GroupLocationScheduleConfig>();

                    //Deleted
                    var removedIds = currentgroupLocationScheduleConfigs
                        .Where( cnfg => !selectedScheduleIds.Contains( cnfg.ScheduleId ) )
                        .Select( r => r.Schedule.Id );

                    // Build list of GroupLocationScheduleConfigs to be removed
                    foreach ( var Id in removedIds )
                    {
                        var configToRemove = currentgroupLocationScheduleConfigs.Where( cnfg => cnfg.ScheduleId == Id ).FirstOrDefault() as GroupLocationScheduleConfig;
                        if ( configToRemove != null )
                        {
                            accessModConfigsToRemove.Add( configToRemove );
                        }
                    }

                    // Remove GroupLocationScheduleConfigs from view state
                    foreach ( var config in accessModConfigsToRemove )
                    {
                        currentgroupLocationScheduleConfigs.Remove( config );
                        groupLocationState.GroupLocationScheduleConfigs.Remove( config );
                    }
                }

                if ( existingConfigCount < schedules.Count() )
                {
                    // Added
                    var addedSchedules = schedules.Where( s => groupLocationStateScheduleIds.Contains( s.Id ) ).ToList();
                    foreach ( var addedSchedule in addedSchedules )
                    {
                        // check if already exist
                        if ( !currentgroupLocationScheduleConfigs.Where( s => s.ScheduleId == addedSchedule.Id ).Any() )
                        {
                            var newSchedule = NewGroupLocationConfig( addedSchedule );
                            currentgroupLocationScheduleConfigs.Add( newSchedule );
                            groupLocationState.GroupLocationScheduleConfigs.Add( newSchedule );
                        }
                    }
                }
            }
            else
            {
                // Handles case where group location schedules existed without group location schedule configs
                currentgroupLocationScheduleConfigs = new List<GroupLocationScheduleConfig>();
                foreach ( var schedule in schedules )
                {
                    currentgroupLocationScheduleConfigs.Add( NewGroupLocationConfig( schedule ) );
                }
            }

            // Calculate the Next Start Date Time based on the start of the week so that schedules are in the correct order
            var occurrenceDate = RockDateTime.Now.SundayDate().AddDays( 1 );

            rptGroupLocationScheduleCapacities.DataSource = currentgroupLocationScheduleConfigs.OrderBy( s => s.Schedule.GetNextStartDateTime( occurrenceDate ));
            rptGroupLocationScheduleCapacities.Visible = true;
            rptGroupLocationScheduleCapacities.DataBind();

            rcwGroupLocationScheduleCapacities.Visible = currentgroupLocationScheduleConfigs.Any();
        }

        /// <summary>
        /// Used to determine if a user has selected a location
        /// </summary>
        /// <returns></returns>
        private bool LocationSelected()
        {
            var selectedLocation = locpGroupLocation.Location;
            return selectedLocation != null;
        }

        /// <summary>
        /// Creates new grouplocationconfig.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private GroupLocationScheduleConfig NewGroupLocationConfig( Schedule s )
        {
            return new GroupLocationScheduleConfig
            {
                Schedule = s,
                ScheduleId = s.Id
            };
        }
        #endregion

        #region Location Grid and Picker

        /// <summary>
        /// Handles the Add event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLocations_Add( object sender, EventArgs e )
        {
            hfAction.Value = "Add";
            gLocations_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Edit( object sender, RowEventArgs e )
        {
            hfAction.Value = "Edit";
            Guid locationGuid = ( Guid ) e.RowKeyValue;
            gLocations_ShowEdit( locationGuid );
        }

        /// <summary>
        /// Gs the locations_ show edit.
        /// </summary>
        /// <param name="locationGuid">The location unique identifier.</param>
        protected void gLocations_ShowEdit( Guid locationGuid )
        {
            ResetLocationDialog();
            hfLocationGuid.Value = locationGuid.ToString();

            var rockContext = new RockContext();
            ddlMember.Items.Clear();
            int? groupTypeId = this.CurrentGroupTypeId;
            if ( !groupTypeId.HasValue )
            {
                return;
            }

            var groupType = GroupTypeCache.Get( groupTypeId.Value );
            if ( groupType == null )
            {
                return;
            }

            GroupLocationPickerMode groupTypeModes = groupType.LocationSelectionMode;
            if ( groupTypeModes == GroupLocationPickerMode.None )
            {
                return;
            }

            // Set the location picker modes allowed based on the group type's allowed modes
            LocationPickerMode modes = LocationPickerMode.None;
            if ( ( groupTypeModes & GroupLocationPickerMode.Named ) == GroupLocationPickerMode.Named )
            {
                modes = modes | LocationPickerMode.Named;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Address ) == GroupLocationPickerMode.Address )
            {
                modes = modes | LocationPickerMode.Address;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Point ) == GroupLocationPickerMode.Point )
            {
                modes = modes | LocationPickerMode.Point;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Polygon ) == GroupLocationPickerMode.Polygon )
            {
                modes = modes | LocationPickerMode.Polygon;
            }

            bool displayMemberTab = ( groupTypeModes & GroupLocationPickerMode.GroupMember ) == GroupLocationPickerMode.GroupMember;
            bool displayOtherTab = modes != LocationPickerMode.None;

            ulNav.Visible = displayOtherTab && displayMemberTab;
            pnlMemberSelect.Visible = displayMemberTab;
            pnlLocationSelect.Visible = displayOtherTab && !displayMemberTab;

            if ( displayMemberTab )
            {
                int groupId = hfGroupId.ValueAsInt();
                if ( groupId != 0 )
                {
                    var personService = new PersonService( rockContext );
                    Guid previousLocationType = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid();

                    foreach ( GroupMember member in new GroupMemberService( rockContext ).GetByGroupId( groupId ) )
                    {
                        foreach ( Group family in personService.GetFamilies( member.PersonId ) )
                        {
                            foreach ( GroupLocation familyGroupLocation in family.GroupLocations
                                .Where( l => l.IsMappedLocation && !l.GroupLocationTypeValue.Guid.Equals( previousLocationType ) ) )
                            {
                                ListItem li = new ListItem(
                                    string.Format( "{0} {1} ({2})", member.Person.FullName, familyGroupLocation.GroupLocationTypeValue.Value, familyGroupLocation.Location.ToString() ),
                                    string.Format( "{0}|{1}", familyGroupLocation.Location.Id, member.PersonId ) );

                                ddlMember.Items.Add( li );
                            }
                        }
                    }
                }
            }

            if ( displayOtherTab )
            {
                locpGroupLocation.AllowedPickerModes = modes;
                locpGroupLocation.SetBestPickerModeForLocation( null );
            }

            ddlLocationType.DataSource = groupType.LocationTypeValues.ToList();
            ddlLocationType.DataBind();

            var groupLocation = GroupLocationsState.FirstOrDefault( l => l.Guid.Equals( locationGuid ) );
            if ( groupLocation != null && groupLocation.Location != null )
            {
                if ( displayOtherTab )
                {
                    locpGroupLocation.SetBestPickerModeForLocation( groupLocation.Location );

                    locpGroupLocation.MapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();

                    if ( groupLocation.Location != null )
                    {
                        locpGroupLocation.Location = new LocationService( rockContext ).Get( groupLocation.Location.Id );
                    }
                }

                if ( displayMemberTab && ddlMember.Items.Count > 0 && groupLocation.GroupMemberPersonAliasId.HasValue )
                {
                    LocationTypeTab = MEMBER_LOCATION_TAB_TITLE;
                    int? personId = new PersonAliasService( rockContext ).GetPersonId( groupLocation.GroupMemberPersonAliasId.Value );
                    if ( personId.HasValue )
                    {
                        ddlMember.SetValue( string.Format( "{0}|{1}", groupLocation.LocationId, personId.Value ) );
                    }
                }
                else if ( displayOtherTab )
                {
                    LocationTypeTab = OTHER_LOCATION_TAB_TITLE;
                }

                ddlLocationType.SetValue( groupLocation.GroupLocationTypeValueId );

                spSchedules.SetValues( groupLocation.Schedules );

                hfAddLocationGroupGuid.Value = locationGuid.ToString();
            }
            else
            {
                hfAddLocationGroupGuid.Value = string.Empty;
                LocationTypeTab = ( displayMemberTab && ddlMember.Items.Count > 0 ) ? MEMBER_LOCATION_TAB_TITLE : OTHER_LOCATION_TAB_TITLE;
            }

            rptLocationTypes.DataSource = _tabs;
            rptLocationTypes.DataBind();

            rcwGroupLocationScheduleCapacities.Visible = groupType.IsSchedulingEnabled;
            if ( groupType.IsSchedulingEnabled )
            {
                var schedules = new ScheduleService( rockContext ).GetByIds( spSchedules.SelectedValuesAsInt().ToList() );


                List<GroupLocationScheduleConfig> groupLocationScheduleConfigList = schedules.ToList().Select( s =>
                {
                    GroupLocationScheduleConfig groupLocationScheduleConfig = groupLocation == null ? null : groupLocation.GroupLocationScheduleConfigs.FirstOrDefault( a => a.ScheduleId == s.Id );
                    if ( groupLocationScheduleConfig != null )
                    {
                        return groupLocationScheduleConfig;
                    }
                    else
                    {
                        return NewGroupLocationConfig( s );
                    }
                } ).ToList();

                // Handle case where schedules are created and no group location configuration exists yet
                if ( groupLocationScheduleConfigList.Count() == 0 && schedules.Count() > 0  )
                {
                    // No schedules have been saved yet.
                    groupLocationScheduleConfigList = new List<GroupLocationScheduleConfig>();
                    foreach ( var schedule in schedules )
                    {
                        groupLocationScheduleConfigList.Add( NewGroupLocationConfig( schedule ) );
                    }
                }

                rptGroupLocationScheduleCapacities.DataSource = groupLocationScheduleConfigList.OrderBy( s => s.ScheduleId );
                rptGroupLocationScheduleCapacities.DataBind();
                rptGroupLocationScheduleCapacities.Visible = true;
                rcwGroupLocationScheduleCapacities.Visible = groupLocationScheduleConfigList.Any();
            }

            ShowSelectedPane();
            ShowDialog( "Locations", true );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptGroupLocationScheduleCapacities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptGroupLocationScheduleCapacities_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            // Builds the display of schedules for the Grid
            GroupLocationScheduleConfig groupLocationScheduleConfig = e.Item.DataItem as GroupLocationScheduleConfig;
            if ( groupLocationScheduleConfig == null )
            {
                return;
            }

            Literal lScheduleName = e.Item.FindControl( "lScheduleName" ) as Literal;
            lScheduleName.Text = groupLocationScheduleConfig.Schedule == null ? string.Empty : groupLocationScheduleConfig.Schedule.Name;

            HiddenField hfScheduleId = e.Item.FindControl( "hfScheduleId" ) as HiddenField;
            NumberBox nbMinimumCapacity = e.Item.FindControl( "nbMinimumCapacity" ) as NumberBox;
            NumberBox nbDesiredCapacity = e.Item.FindControl( "nbDesiredCapacity" ) as NumberBox;
            NumberBox nbMaximumCapacity = e.Item.FindControl( "nbMaximumCapacity" ) as NumberBox;

            hfScheduleId.Value = groupLocationScheduleConfig.ScheduleId.ToString();
            nbMinimumCapacity.Text = groupLocationScheduleConfig.MinimumCapacity.ToString();
            nbDesiredCapacity.Text = groupLocationScheduleConfig.DesiredCapacity.ToString();
            nbMaximumCapacity.Text = groupLocationScheduleConfig.MaximumCapacity.ToString();

        }

        /// <summary>
        /// Handles the Delete event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            GroupLocationsState.RemoveEntity( rowGuid );
            BindLocationsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLocations_GridRebind( object sender, EventArgs e )
        {
            BindLocationsGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgLocations_OkClick( object sender, EventArgs e )
        {
            Location location = null;
            int? memberPersonAliasId = null;
            RockContext rockContext = new RockContext();

            if ( LocationTypeTab.Equals( MEMBER_LOCATION_TAB_TITLE ) )
            {
                if ( ddlMember.SelectedValue != null )
                {
                    var ids = ddlMember.SelectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList().ToArray();
                    if ( ids.Length == 2 )
                    {
                        int locationId = ids[0];
                        int primaryAliasId = ids[1];
                        var dbLocation = new LocationService( rockContext ).Get( locationId );
                        if ( dbLocation != null )
                        {
                            location = new Location();
                            location.CopyPropertiesFrom( dbLocation );
                        }

                        memberPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( primaryAliasId );
                    }
                }
            }
            else
            {
                if ( locpGroupLocation.Location != null )
                {
                    var selectedLocation = locpGroupLocation.Location;

                    location = new Location();
                    location.CopyPropertiesFrom( selectedLocation );
                }
            }

            if ( location != null )
            {
                GroupLocation groupLocation = null;

                Guid guid = hfAddLocationGroupGuid.Value.AsGuid();
                if ( !guid.IsEmpty() )
                {
                    groupLocation = GroupLocationsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
                }

                if ( groupLocation == null )
                {
                    groupLocation = new GroupLocation();
                    GroupLocationsState.Add( groupLocation );
                }

                var schedules = new ScheduleService( rockContext ).GetByIds( spSchedules.SelectedValuesAsInt().ToList() ).ToList();
                // Builds the display of capacities for the group location edit dialog
                foreach ( RepeaterItem rItem in rptGroupLocationScheduleCapacities.Items )
                {
                    var hfScheduleId = rItem.FindControl( "hfScheduleId" ) as HiddenField;
                    var nbMinimumCapacity = rItem.FindControl( "nbMinimumCapacity" ) as NumberBox;
                    var nbDesiredCapacity = rItem.FindControl( "nbDesiredCapacity" ) as NumberBox;
                    var nbMaximumCapacity = rItem.FindControl( "nbMaximumCapacity" ) as NumberBox;
                    var iScheduleId = hfScheduleId.Value.AsIntegerOrNull();
                    var iMinCapacity = nbMinimumCapacity.Text.AsIntegerOrNull();
                    var iDesiredCapacity = nbDesiredCapacity.Text.AsIntegerOrNull();
                    var iMaxCapacity = nbMaximumCapacity.Text.AsIntegerOrNull();
                    var schedule = schedules.Where( s => s.Id == iScheduleId ).FirstOrDefault();

                    if ( iScheduleId != null )
                    {
                        var currentgroupLocationScheduleConfig = groupLocation.GroupLocationScheduleConfigs.Where( i => i.ScheduleId == iScheduleId ).FirstOrDefault();
                        if ( currentgroupLocationScheduleConfig != null )
                        {
                            currentgroupLocationScheduleConfig.Schedule = schedule;
                            currentgroupLocationScheduleConfig.MinimumCapacity = iMinCapacity;
                            currentgroupLocationScheduleConfig.DesiredCapacity = iDesiredCapacity;
                            currentgroupLocationScheduleConfig.MaximumCapacity = iMaxCapacity;
                        }
                        else
                        {
                            currentgroupLocationScheduleConfig = new GroupLocationScheduleConfig
                            {
                                ScheduleId = ( int ) iScheduleId,
                                Schedule = schedule,
                                MinimumCapacity = iMinCapacity,
                                DesiredCapacity = iDesiredCapacity,
                                MaximumCapacity = iMaxCapacity
                            };
                            groupLocation.GroupLocationScheduleConfigs.Add( currentgroupLocationScheduleConfig );
                        }

                    }
                }

                groupLocation.GroupMemberPersonAliasId = memberPersonAliasId;
                groupLocation.Location = location;
                groupLocation.LocationId = groupLocation.Location.Id;
                groupLocation.GroupLocationTypeValueId = ddlLocationType.SelectedValueAsId();

                var selectedIds = spSchedules.SelectedValuesAsInt();
                groupLocation.Schedules = new ScheduleService( rockContext ).Queryable()
                    .Where( s => selectedIds.Contains( s.Id ) ).ToList();

                if ( groupLocation.GroupLocationTypeValueId.HasValue )
                {
                    groupLocation.GroupLocationTypeValue = new DefinedValue();
                    var definedValue = new DefinedValueService( rockContext ).Get( groupLocation.GroupLocationTypeValueId.Value );
                    if ( definedValue != null )
                    {
                        groupLocation.GroupLocationTypeValue.CopyPropertiesFrom( definedValue );
                    }
                }
            }

            BindLocationsGrid();
            spSchedules.SetValue( 0 );
            HideDialog();
        }

        /// <summary>
        /// Detect  a existing the location on add.
        /// </summary>
        /// <param name="selectedLocation">The selected location.</param>
        /// <returns>return false if not an add or location not selected</returns>
        private bool ExistingLocationOnAdd( Location selectedLocation )
        {
            if ( hfAction.Value == "Add" && selectedLocation != null )
            {
                List<GridLocation> existingLocations = gLocations.DataSourceAsList as List<GridLocation>;

                return existingLocations.Where( x => x.Location.Name == selectedLocation.Name && x.Location.Guid == selectedLocation.Guid ).Any();
            }

            return false;
        }

        /// <summary>
        /// Binds the locations grid.
        /// </summary>
        private void BindLocationsGrid()
        {
            gLocations.Actions.ShowAdd = AllowMultipleLocations || !GroupLocationsState.Any();

            gLocations.DataSource = GroupLocationsState
                .Select( gl => new GridLocation
                {
                    Guid = gl.Guid,
                    Location = gl.Location,
                    Type = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : string.Empty,
                    Order = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Order : 0,
                    Schedules = gl.Schedules != null ? gl.Schedules.Select( s => s.Name ).ToList().AsDelimited( ", " ) : string.Empty
                } )
                .OrderBy( i => i.Order )
                .ToList();
            gLocations.DataBind();
        }

        /// <summary>
        /// Resets the location dialog.
        /// </summary>
        private void ResetLocationDialog()
        {
            locpGroupLocation.Location = null;
            spSchedules.SetValue( 0 );
            rptGroupLocationScheduleCapacities.DataSource = null;
            rptGroupLocationScheduleCapacities.DataBind();
            rptGroupLocationScheduleCapacities.Visible = false;
            nbEditModeMessage.Text = string.Empty;
            nbEditModeMessage.Visible = false;
        }

        /// <summary>
        /// Handles the SelectLocation event of the locpGroupLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void locpGroupLocation_SelectLocation( object sender, EventArgs e )
        {
            nbGroupLocationEditMessage.Text = string.Empty;
            nbGroupLocationEditMessage.Visible = false;
            nbGroupLocationEditMessage.Visible = true;
            var selectedLocation = locpGroupLocation.Location;
            if ( ExistingLocationOnAdd( selectedLocation ) )
            {
                nbGroupLocationEditMessage.Text = string.Format( "{0} already exists in meeting details and can not be selected again.", selectedLocation.Name );
                nbGroupLocationEditMessage.Visible = true;
                locpGroupLocation.Location = null;
            }
        }

        #endregion

        #region GroupRequirements Grid and Picker

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
            Guid groupRequirementGuid = ( Guid ) e.RowKeyValue;
            gGroupRequirements_ShowEdit( groupRequirementGuid );
        }

        /// <summary>
        /// Shows the modal dialog to add/edit a Group Requirement
        /// </summary>
        /// <param name="groupRequirementGuid">The group requirement unique identifier.</param>
        protected void gGroupRequirements_ShowEdit( Guid groupRequirementGuid )
        {
            var rockContext = new RockContext();

            var groupRequirementTypeService = new GroupRequirementTypeService( rockContext );
            var list = groupRequirementTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlGroupRequirementType.Items.Clear();
            ddlGroupRequirementType.Items.Add( new ListItem() );
            foreach ( var item in list )
            {
                ddlGroupRequirementType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            var selectedGroupRequirement = this.GroupRequirementsState.FirstOrDefault( a => a.Guid == groupRequirementGuid );
            grpGroupRequirementGroupRole.GroupTypeId = this.CurrentGroupTypeId;
            if ( selectedGroupRequirement != null )
            {
                ddlGroupRequirementType.SelectedValue = selectedGroupRequirement.GroupRequirementTypeId.ToString();
                grpGroupRequirementGroupRole.GroupRoleId = selectedGroupRequirement.GroupRoleId;
                cbMembersMustMeetRequirementOnAdd.Checked = selectedGroupRequirement.MustMeetRequirementToAddMember;
            }
            else
            {
                ddlGroupRequirementType.SelectedIndex = 0;
                grpGroupRequirementGroupRole.GroupRoleId = null;
                cbMembersMustMeetRequirementOnAdd.Checked = false;
            }

            nbDuplicateGroupRequirement.Visible = false;

            hfGroupRequirementGuid.Value = groupRequirementGuid.ToString();

            ShowDialog( "GroupRequirements", true );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupRequirement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupRequirement_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            Guid groupRequirementGuid = hfGroupRequirementGuid.Value.AsGuid();

            var groupRequirement = this.GroupRequirementsState.FirstOrDefault( a => a.Guid == groupRequirementGuid );
            if ( groupRequirement == null )
            {
                groupRequirement = new GroupRequirement();
                groupRequirement.Guid = Guid.NewGuid();
                this.GroupRequirementsState.Add( groupRequirement );
            }

            groupRequirement.GroupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();
            groupRequirement.GroupRequirementType = new GroupRequirementTypeService( rockContext ).Get( groupRequirement.GroupRequirementTypeId );
            groupRequirement.GroupRoleId = grpGroupRequirementGroupRole.GroupRoleId;
            groupRequirement.MustMeetRequirementToAddMember = cbMembersMustMeetRequirementOnAdd.Checked;
            if ( groupRequirement.GroupRoleId.HasValue )
            {
                groupRequirement.GroupRole = new GroupTypeRoleService( rockContext ).Get( groupRequirement.GroupRoleId.Value );
            }
            else
            {
                groupRequirement.GroupRole = null;
            }

            // Make sure we aren't adding a duplicate group requirement (same group requirement type and role)
            var duplicateGroupRequirement = this.GroupRequirementsState.Any( a =>
                a.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId
                && a.GroupRoleId == groupRequirement.GroupRoleId
                && a.Guid != groupRequirement.Guid );

            if ( duplicateGroupRequirement )
            {
                nbDuplicateGroupRequirement.Text = string.Format(
                    "This group already has a group requirement of {0} {1}",
                    groupRequirement.GroupRequirementType.Name,
                    groupRequirement.GroupRoleId.HasValue ? "for group role " + groupRequirement.GroupRole.Name : string.Empty );
                nbDuplicateGroupRequirement.Visible = true;
                this.GroupRequirementsState.Remove( groupRequirement );
                return;
            }
            else
            {
                nbDuplicateGroupRequirement.Visible = false;
                BindGroupRequirementsGrid();
                HideDialog();
            }
        }

        /// <summary>
        /// Handles the Delete event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            GroupRequirementsState.RemoveEntity( rowGuid );

            BindGroupRequirementsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupRequirements_GridRebind( object sender, EventArgs e )
        {
            BindGroupRequirementsGrid();
        }

        #endregion

        #region Group Syncs Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupSyncs control.
        /// Shows the GroupSync modal and populates the controls without explicitly assigning
        /// a value
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupSyncs_Add( object sender, EventArgs e )
        {
            ClearGroupSyncModal();

            RockContext rockContext = new RockContext();

            CreateRoleDropDownList( rockContext );
            CreateGroupSyncEmailDropDownLists( rockContext );

            dvipSyncDataView.EntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;

            ShowDialog( "GROUPSYNCSETTINGS", true );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupSyncs control.
        /// Shows the GroupSync modal, populates the controls, and selects
        /// the data for the selected group sync.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupSyncs_Edit( object sender, RowEventArgs e )
        {
            ClearGroupSyncModal();

            Guid syncGuid = ( Guid ) e.RowKeyValue;
            GroupSync groupSync = GroupSyncState.Where( s => s.Guid == syncGuid ).FirstOrDefault();
            RockContext rockContext = new RockContext();

            hfGroupSyncGuid.Value = syncGuid.ToString();

            dvipSyncDataView.EntityTypeId = EntityTypeCache.Get( typeof( Person ) ).Id;
            dvipSyncDataView.SetValue( groupSync.SyncDataViewId );

            CreateRoleDropDownList( rockContext, groupSync.GroupTypeRoleId );
            ddlGroupRoles.SetValue( groupSync.GroupTypeRoleId );

            CreateGroupSyncEmailDropDownLists( rockContext );
            ddlWelcomeEmail.SetValue( groupSync.WelcomeSystemEmailId );
            ddlExitEmail.SetValue( groupSync.ExitSystemEmailId );

            cbCreateLoginDuringSync.Checked = groupSync.AddUserAccountsDuringSync;

            ShowDialog( "GROUPSYNCSETTINGS", true );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupSyncs control.
        /// Removes the group sync from the List<> in the current state
        /// and the grid. Will not be removed from the group until the group
        /// is saved.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupSyncs_Delete( object sender, RowEventArgs e )
        {
            Guid guid = ( Guid ) e.RowKeyValue;
            GroupSyncState.RemoveEntity( guid );
            BindGroupSyncGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupSyncs control.
        /// This call BindGroupSyncGrid()
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupSyncs_GridRebind( object sender, EventArgs e )
        {
            BindGroupSyncGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupSyncSettings control.
        /// Adds the group sync to the List for the current state and to the grid.
        /// Won't be added to the group until the group is saved.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupSyncSettings_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            Guid syncGuid = hfGroupSyncGuid.Value.AsGuid();

            // Create a new obj
            var groupSync = GroupSyncState.Where( s => s.Guid == syncGuid ).FirstOrDefault();
            if ( groupSync == null )
            {
                groupSync = new GroupSync();
                groupSync.Guid = Guid.NewGuid();
                GroupSyncState.Add( groupSync );
            }

            groupSync.GroupId = hfGroupId.ValueAsInt();
            groupSync.GroupTypeRoleId = ddlGroupRoles.SelectedValue.AsInteger();
            groupSync.GroupTypeRole = new GroupTypeRoleService( rockContext ).Get( groupSync.GroupTypeRoleId );
            groupSync.SyncDataViewId = dvipSyncDataView.SelectedValueAsInt() ?? 0;
            groupSync.SyncDataView = new DataViewService( rockContext ).Get( groupSync.SyncDataViewId );
            groupSync.ExitSystemEmailId = ddlExitEmail.SelectedValue.AsIntegerOrNull();
            groupSync.WelcomeSystemEmailId = ddlWelcomeEmail.SelectedValue.AsIntegerOrNull();
            groupSync.AddUserAccountsDuringSync = cbCreateLoginDuringSync.Checked;

            hfGroupSyncGuid.Value = string.Empty;

            BindGroupSyncGrid();

            HideDialog();
        }

        /// <summary>
        /// Binds the GroupSync grid to the List stored in the current state
        /// </summary>
        private void BindGroupSyncGrid()
        {
            gGroupSyncs.DataSource = GroupSyncState;
            gGroupSyncs.DataBind();
        }

        /// <summary>
        /// Creates the group sync email drop down lists. Does not set a selected value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void CreateGroupSyncEmailDropDownLists( RockContext rockContext )
        {
            // Populate the email fields
            var systemEmails = new SystemEmailService( rockContext )
                .Queryable()
                .OrderBy( e => e.Title )
                .Select( a => new { a.Id, a.Title } );

            // add a blank for the first option
            ddlWelcomeEmail.Items.Add( new ListItem() );
            ddlExitEmail.Items.Add( new ListItem() );

            if ( systemEmails.Any() )
            {
                foreach ( var systemEmail in systemEmails )
                {
                    ddlWelcomeEmail.Items.Add( new ListItem( systemEmail.Title, systemEmail.Id.ToString() ) );
                    ddlExitEmail.Items.Add( new ListItem( systemEmail.Title, systemEmail.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Creates the role drop down list.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="RoleId">The role identifier if editing, otherwise leave default</param>
        private void CreateRoleDropDownList( RockContext rockContext, int roleId = -1 )
        {
            List<int> currentSyncdRoles = new List<int>();
            int groupTypeId = ddlGroupType.SelectedValue.AsInteger();
            int groupId = hfGroupId.ValueAsInt();

            // If not 0 then get the existing roles to remove, if 0 then this is a new group that has not yet been saved.
            if ( groupId > 0 )
            {
                currentSyncdRoles = GroupSyncState
                    .Where( s => s.GroupId == groupId )
                    .Select( s => s.GroupTypeRoleId )
                    .ToList();

                currentSyncdRoles.Remove( roleId );

                groupTypeId = new GroupService( rockContext ).Get( groupId ).GroupTypeId;
            }

            var roles = new GroupTypeRoleService( rockContext )
                .Queryable()
                .Where( r => r.GroupTypeId == groupTypeId && !currentSyncdRoles.Contains( r.Id ) )
                .ToList();

            // Give a blank for the first selection
            ddlGroupRoles.Items.Add( new ListItem() );

            foreach ( var role in roles )
            {
                ddlGroupRoles.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Clears the values from the group sync modal controls.
        /// </summary>
        private void ClearGroupSyncModal()
        {
            //ddlSyncDataView.Items.Clear();
            ddlGroupRoles.Items.Clear();
            ddlWelcomeEmail.Items.Clear();
            ddlExitEmail.Items.Clear();
            cbCreateLoginDuringSync.Checked = false;
        }

        #endregion

        #region GroupMemberAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Add( object sender, EventArgs e )
        {
            gGroupMemberAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gGroupMemberAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group member attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupMemberAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Add( "attribute for group members of " + tbName.Text );
            }
            else
            {
                attribute = GroupMemberAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Edit( "attribute for group members of " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            GroupMemberAttributesInheritedState.Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            GroupMemberAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtGroupMemberAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtGroupMemberAttributes.SetAttributeProperties( attribute, typeof( GroupMember ) );

            ShowDialog( "GroupMemberAttributes", true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gGroupMemberAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( GroupMemberAttributesState, e.OldIndex, e.NewIndex );
            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            GroupMemberAttributesState.RemoveEntity( attributeGuid );

            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMemberAttributesInherited control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributesInherited_GridRebind( object sender, EventArgs e )
        {
            BindGroupMemberAttributesInheritedGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMemberAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupMemberAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupMemberAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupMemberAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupMemberAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupMemberAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( GroupMemberAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = GroupMemberAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                GroupMemberAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = GroupMemberAttributesState.Any() ? GroupMemberAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            GroupMemberAttributesState.Add( attribute );

            BindGroupMemberAttributesGrid();

            HideDialog();
        }

        /// <summary>
        /// Binds the group member attributes inherited grid.
        /// </summary>
        private void BindGroupMemberAttributesInheritedGrid()
        {
            if ( CurrentGroupTypeCache != null )
            {
                wpGroupMemberAttributes.Visible = GroupMemberAttributesInheritedState.Any() || GroupMemberAttributesState.Any() || CurrentGroupTypeCache.AllowSpecificGroupMemberAttributes;
                rcwGroupMemberAttributes.Visible = GroupMemberAttributesInheritedState.Any() || GroupMemberAttributesState.Any() || CurrentGroupTypeCache.AllowSpecificGroupMemberAttributes;
            }

            gGroupMemberAttributesInherited.AddCssClass( "inherited-attribute-grid" );
            gGroupMemberAttributesInherited.DataSource = GroupMemberAttributesInheritedState;
            gGroupMemberAttributesInherited.DataBind();
            rcwGroupMemberAttributesInherited.Visible = GroupMemberAttributesInheritedState.Any();

            rcwGroupMemberAttributes.Label = GroupMemberAttributesInheritedState.Any() ? "Group Member Attributes" : string.Empty;
        }

        /// <summary>
        /// Binds the group member attributes grid.
        /// </summary>
        private void BindGroupMemberAttributesGrid()
        {
            gGroupMemberAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( GroupMemberAttributesState );
            gGroupMemberAttributes.DataSource = GroupMemberAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gGroupMemberAttributes.DataBind();
        }

        /// <summary>
        /// Binds the group requirements grids
        /// </summary>
        private void BindGroupRequirementsGrid()
        {
            var rockContext = new RockContext();
            var groupTypeGroupRequirements = new GroupRequirementService( rockContext ).Queryable().Where( a => a.GroupTypeId.HasValue && a.GroupTypeId == CurrentGroupTypeId ).ToList();
            var groupGroupRequirements = GroupRequirementsState.ToList();

            rcwGroupTypeGroupRequirements.Visible = groupTypeGroupRequirements.Any();
            rcwGroupRequirements.Label = groupGroupRequirements.Any() ? "Specific Group Requirements" : string.Empty;

            if ( CurrentGroupTypeCache != null )
            {
                lGroupTypeGroupRequirementsFrom.Text = string.Format( "(From <a href='{0}' target='_blank'>{1}</a>)", this.ResolveUrl( "~/GroupType/" + CurrentGroupTypeCache.Id ), CurrentGroupTypeCache.Name );
                rcwGroupRequirements.Visible = CurrentGroupTypeCache.EnableSpecificGroupRequirements || groupGroupRequirements.Any() || groupTypeGroupRequirements.Any();
                wpGroupRequirements.Visible = groupTypeGroupRequirements.Any() || groupGroupRequirements.Any() || CurrentGroupTypeCache.EnableSpecificGroupRequirements;
            }

            gGroupTypeGroupRequirements.AddCssClass( "grouptype-group-requirements-grid" );
            gGroupTypeGroupRequirements.DataSource = groupTypeGroupRequirements.OrderBy( a => a.GroupRequirementType.Name ).ToList();
            gGroupTypeGroupRequirements.DataBind();

            gGroupRequirements.AddCssClass( "group-requirements-grid" );
            gGroupRequirements.DataSource = groupGroupRequirements.OrderBy( a => a.GroupRequirementType.Name ).ToList();
            gGroupRequirements.DataBind();
        }

        /// <summary>
        /// Sets the schedule display.
        /// </summary>
        private void SetScheduleDisplay()
        {
            dowWeekly.Visible = false;
            timeWeekly.Visible = false;
            spSchedule.Visible = false;
            sbSchedule.Visible = false;

            if ( !string.IsNullOrWhiteSpace( rblScheduleSelect.SelectedValue ) )
            {
                switch ( rblScheduleSelect.SelectedValueAsEnum<ScheduleType>() )
                {
                    case ScheduleType.None:
                        {
                            break;
                        }

                    case ScheduleType.Weekly:
                        {
                            dowWeekly.Visible = true;
                            timeWeekly.Visible = true;
                            break;
                        }

                    case ScheduleType.Custom:
                        {
                            sbSchedule.Visible = true;
                            break;
                        }

                    case ScheduleType.Named:
                        {
                            spSchedule.Visible = true;
                            break;
                        }
                }
            }
        }

        #endregion

        #region Group Member Workflow Trigger Grid and Picker

        /// <summary>
        /// Handles the Add event of the gMemberWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMemberWorkflowTriggers_Add( object sender, EventArgs e )
        {
            gMemberWorkflowTriggers_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gMemberWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMemberWorkflowTriggers_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gMemberWorkflowTriggers_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gMemberWorkflowTriggers_ShowEdit( Guid memberWorkflowTriggersGuid )
        {
            ddlTriggerType.BindToEnum<GroupMemberWorkflowTriggerType>( false );

            ddlTriggerFromStatus.BindToEnum<GroupMemberStatus>( false );
            ddlTriggerFromStatus.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            ddlTriggerToStatus.BindToEnum<GroupMemberStatus>( false );
            ddlTriggerToStatus.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            ddlTriggerFromRole.Items.Clear();
            ddlTriggerToRole.Items.Clear();
            var groupType = CurrentGroupTypeCache;
            if ( groupType != null )
            {
                ddlTriggerFromRole.DataSource = groupType.Roles;
                ddlTriggerFromRole.DataBind();

                ddlTriggerToRole.DataSource = groupType.Roles;
                ddlTriggerToRole.DataBind();
            }

            ddlTriggerFromRole.Items.Insert( 0, new ListItem( "Any", string.Empty ) );
            ddlTriggerToRole.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            GroupMemberWorkflowTrigger memberWorkflowTrigger = MemberWorkflowTriggersState.FirstOrDefault( a => a.Guid.Equals( memberWorkflowTriggersGuid ) );
            if ( memberWorkflowTrigger == null )
            {
                memberWorkflowTrigger = new GroupMemberWorkflowTrigger { IsActive = true };
                dlgMemberWorkflowTriggers.Title = "Add Trigger";
            }
            else
            {
                dlgMemberWorkflowTriggers.Title = "Edit Trigger";
            }

            hfTriggerGuid.Value = memberWorkflowTrigger.Guid.ToString();
            tbTriggerName.Text = memberWorkflowTrigger.Name;
            cbTriggerIsActive.Checked = memberWorkflowTrigger.IsActive;

            if ( memberWorkflowTrigger.WorkflowTypeId != 0 )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Queryable().FirstOrDefault( a => a.Id == memberWorkflowTrigger.WorkflowTypeId );
                wtpWorkflowType.SetValue( workflowType );
            }
            else
            {
                wtpWorkflowType.SetValue( null );
            }

            ddlTriggerType.SetValue( memberWorkflowTrigger.TriggerType.ConvertToInt() );

            var qualifierParts = ( memberWorkflowTrigger.TypeQualifier ?? string.Empty ).Split( new char[] { '|' } );
            ddlTriggerToStatus.SetValue( qualifierParts.Length > 0 ? qualifierParts[0] : string.Empty );
            ddlTriggerToRole.SetValue( qualifierParts.Length > 1 ? qualifierParts[1] : string.Empty );
            ddlTriggerFromStatus.SetValue( qualifierParts.Length > 2 ? qualifierParts[2] : string.Empty );
            ddlTriggerFromRole.SetValue( qualifierParts.Length > 3 ? qualifierParts[3] : string.Empty );
            cbTriggerFirstTime.Checked = qualifierParts.Length > 4 ? qualifierParts[4].AsBoolean() : false;
            cbTriggerPlacedElsewhereShowNote.Checked = qualifierParts.Length > 5 ? qualifierParts[5].AsBoolean() : false;
            cbTriggerPlacedElsewhereRequireNote.Checked = qualifierParts.Length > 6 ? qualifierParts[6].AsBoolean() : false;

            ShowTriggerQualifierControls();
            ShowDialog( "MemberWorkflowTriggers", true );
        }

        /// <summary>
        /// Shows the trigger qualifier controls.
        /// </summary>
        protected void ShowTriggerQualifierControls()
        {
            var triggerType = ddlTriggerType.SelectedValueAsEnum<GroupMemberWorkflowTriggerType>();
            switch ( triggerType )
            {
                case GroupMemberWorkflowTriggerType.MemberAddedToGroup:
                case GroupMemberWorkflowTriggerType.MemberRemovedFromGroup:
                    {
                        ddlTriggerFromStatus.Visible = false;
                        ddlTriggerToStatus.Label = "With Status of";
                        ddlTriggerToStatus.Visible = true;

                        ddlTriggerFromRole.Visible = false;
                        ddlTriggerToRole.Label = "With Role of";
                        ddlTriggerToRole.Visible = true;

                        cbTriggerFirstTime.Visible = false;

                        cbTriggerPlacedElsewhereShowNote.Visible = false;
                        cbTriggerPlacedElsewhereRequireNote.Visible = false;

                        break;
                    }

                case GroupMemberWorkflowTriggerType.MemberAttendedGroup:
                    {
                        ddlTriggerFromStatus.Visible = false;
                        ddlTriggerToStatus.Visible = false;

                        ddlTriggerFromRole.Visible = false;
                        ddlTriggerToRole.Visible = false;

                        cbTriggerFirstTime.Visible = true;

                        cbTriggerPlacedElsewhereShowNote.Visible = false;
                        cbTriggerPlacedElsewhereRequireNote.Visible = false;

                        break;
                    }

                case GroupMemberWorkflowTriggerType.MemberPlacedElsewhere:
                    {
                        ddlTriggerFromStatus.Visible = false;
                        ddlTriggerToStatus.Visible = false;

                        ddlTriggerFromRole.Visible = false;
                        ddlTriggerToRole.Visible = false;

                        cbTriggerFirstTime.Visible = false;

                        cbTriggerPlacedElsewhereShowNote.Visible = true;
                        cbTriggerPlacedElsewhereRequireNote.Visible = true;

                        break;
                    }

                case GroupMemberWorkflowTriggerType.MemberRoleChanged:
                    {
                        ddlTriggerFromStatus.Visible = false;
                        ddlTriggerToStatus.Visible = false;

                        ddlTriggerFromRole.Visible = true;
                        ddlTriggerToRole.Label = "To Role of";
                        ddlTriggerToRole.Visible = true;

                        cbTriggerFirstTime.Visible = false;

                        cbTriggerPlacedElsewhereShowNote.Visible = false;
                        cbTriggerPlacedElsewhereRequireNote.Visible = false;

                        break;
                    }

                case GroupMemberWorkflowTriggerType.MemberStatusChanged:
                    {
                        ddlTriggerFromStatus.Visible = true;
                        ddlTriggerToStatus.Label = "To Status of";
                        ddlTriggerToStatus.Visible = true;

                        ddlTriggerFromRole.Visible = false;
                        ddlTriggerToRole.Visible = false;

                        cbTriggerFirstTime.Visible = false;

                        cbTriggerPlacedElsewhereShowNote.Visible = false;
                        cbTriggerPlacedElsewhereRequireNote.Visible = false;

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gMemberWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gMemberWorkflowTriggers_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderMemberWorkflowTriggerList( MemberWorkflowTriggersState, e.OldIndex, e.NewIndex );
            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gMemberWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gMemberWorkflowTriggers_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            MemberWorkflowTriggersState.RemoveEntity( rowGuid );

            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMemberWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMemberWorkflowTriggers_GridRebind( object sender, EventArgs e )
        {
            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTriggerType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTriggerType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowTriggerQualifierControls();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupMemberAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgMemberWorkflowTriggers_SaveClick( object sender, EventArgs e )
        {
            var memberWorkflowTrigger = new GroupMemberWorkflowTrigger();

            var existingMemberWorkflowTrigger = MemberWorkflowTriggersState.FirstOrDefault( r => r.Guid.Equals( hfTriggerGuid.Value.AsGuid() ) );
            if ( existingMemberWorkflowTrigger != null )
            {
                memberWorkflowTrigger.CopyPropertiesFrom( existingMemberWorkflowTrigger );
            }
            else
            {
                memberWorkflowTrigger.Order = MemberWorkflowTriggersState.Any() ? MemberWorkflowTriggersState.Max( a => a.Order ) + 1 : 0;
                memberWorkflowTrigger.GroupId = hfGroupId.ValueAsInt();
            }

            memberWorkflowTrigger.Name = tbTriggerName.Text;
            memberWorkflowTrigger.IsActive = cbTriggerIsActive.Checked;

            var workflowTypeId = wtpWorkflowType.SelectedValueAsInt();
            if ( workflowTypeId.HasValue )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Queryable().FirstOrDefault( a => a.Id == workflowTypeId.Value );
                if ( workflowType != null )
                {
                    memberWorkflowTrigger.WorkflowType = workflowType;
                    memberWorkflowTrigger.WorkflowTypeId = workflowType.Id;
                }
                else
                {
                    memberWorkflowTrigger.WorkflowType = null;
                    memberWorkflowTrigger.WorkflowTypeId = 0;
                }
            }
            else
            {
                memberWorkflowTrigger.WorkflowTypeId = 0;
            }

            if ( memberWorkflowTrigger.WorkflowTypeId == 0 )
            {
                nbInvalidWorkflowType.Visible = true;
                return;
            }

            memberWorkflowTrigger.TriggerType = ddlTriggerType.SelectedValueAsEnum<GroupMemberWorkflowTriggerType>();

            memberWorkflowTrigger.TypeQualifier = string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                ddlTriggerToStatus.SelectedValue,
                ddlTriggerToRole.SelectedValue,
                ddlTriggerFromStatus.SelectedValue,
                ddlTriggerFromRole.SelectedValue,
                cbTriggerFirstTime.Checked.ToString(),
                cbTriggerPlacedElsewhereShowNote.Checked.ToString(),
                cbTriggerPlacedElsewhereRequireNote.Checked.ToString() );

            // Controls will show warnings
            if ( !memberWorkflowTrigger.IsValid )
            {
                return;
            }

            MemberWorkflowTriggersState.RemoveEntity( memberWorkflowTrigger.Guid );
            MemberWorkflowTriggersState.Add( memberWorkflowTrigger );

            BindMemberWorkflowTriggersGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindMemberWorkflowTriggersGrid()
        {
            SetMemberWorkflowTriggerListOrder( MemberWorkflowTriggersState );
            gMemberWorkflowTriggers.DataSource = MemberWorkflowTriggersState.OrderBy( a => a.Order ).ToList();
            gMemberWorkflowTriggers.DataBind();
        }

        /// <summary>
        /// Formats the type of the trigger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        protected string FormatTriggerType( object type, object qualifier )
        {
            var triggerType = type.ToString().ConvertToEnum<GroupMemberWorkflowTriggerType>();
            var typeQualifer = qualifier.ToString();

            var qualiferText = new List<string>();
            var qualifierParts = ( typeQualifer ?? string.Empty ).Split( new char[] { '|' } );

            if ( qualifierParts.Length > 2 && !string.IsNullOrWhiteSpace( qualifierParts[2] ) )
            {
                var status = qualifierParts[2].ConvertToEnum<GroupMemberStatus>();
                qualiferText.Add( string.Format( " from status of <strong>{0}</strong>", status.ConvertToString() ) );
            }

            if ( qualifierParts.Length > 0 && !string.IsNullOrWhiteSpace( qualifierParts[0] ) )
            {
                var status = qualifierParts[0].ConvertToEnum<GroupMemberStatus>();
                if ( triggerType == GroupMemberWorkflowTriggerType.MemberStatusChanged )
                {
                    qualiferText.Add( string.Format( " to status of <strong>{0}</strong>", status.ConvertToString() ) );
                }
                else
                {
                    qualiferText.Add( string.Format( " with status of <strong>{0}</strong>", status.ConvertToString() ) );
                }
            }

            var groupType = CurrentGroupTypeCache;
            if ( groupType != null )
            {
                if ( qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
                {
                    Guid roleGuid = qualifierParts[3].AsGuid();
                    var role = groupType.Roles.FirstOrDefault( r => r.Guid.Equals( roleGuid ) );
                    if ( role != null )
                    {
                        qualiferText.Add( string.Format( " from role of <strong>{0}</strong>", role.Name ) );
                    }
                }

                if ( qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
                {
                    Guid roleGuid = qualifierParts[1].AsGuid();
                    var role = groupType.Roles.FirstOrDefault( r => r.Guid.Equals( roleGuid ) );
                    if ( role != null )
                    {
                        if ( triggerType == GroupMemberWorkflowTriggerType.MemberRoleChanged )
                        {
                            qualiferText.Add( string.Format( " to role of <strong>{0}</strong>", role.Name ) );
                        }
                        else
                        {
                            qualiferText.Add( string.Format( " with role of <strong>{0}</strong>", role.Name ) );
                        }
                    }
                }
            }

            if ( qualifierParts.Length > 4 && qualifierParts[4].AsBoolean() )
            {
                qualiferText.Add( " for the first time" );
            }

            return triggerType.ConvertToString() + qualiferText.AsDelimited( " and " );
        }

        #endregion

    }

}