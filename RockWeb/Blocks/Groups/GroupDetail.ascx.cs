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
    [BooleanField( "Show Edit", "", true, "", 2 )]
    [BooleanField( "Limit to Security Role Groups", "", false, "", 3 )]
    [BooleanField( "Limit to Group Types that are shown in navigation", "", false, "", 4, "LimitToShowInNavigationGroupTypes" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The style of maps to use", false, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK, "", 5 )]
    [LinkedPage("Group Map Page", "The page to display detailed group map.", true, "", "", 6)]
    [LinkedPage( "Attendance Page", "The page to display attendance list.", false, "", "", 7)]
    [LinkedPage( "Registration Instance Page", "The page to display registration details.", false, "", "", 7 )]
    public partial class GroupDetail : RockBlock, IDetailBlock
    {
        #region Constants

        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        #endregion

        #region Fields

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

        #endregion

        #region Properties

        private string LocationTypeTab { get; set; }
        private int CurrentGroupTypeId { get; set; }
        private List<GroupLocation> GroupLocationsState { get; set; }
        private List<InheritedAttribute> GroupMemberAttributesInheritedState { get; set; }
        private List<Attribute> GroupMemberAttributesState { get; set; }
        private List<GroupRequirement> GroupRequirementsState { get; set; }
        private bool AllowMultipleLocations { get; set; }
        private List<GroupMemberWorkflowTrigger> MemberWorkflowTriggersState { get; set; }

        private GroupTypeCache CurrentGroupTypeCache
        {
            get 
            {
                return GroupTypeCache.Read( CurrentGroupTypeId );
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
            if (groupRoleIds.Any())
            {
                var groupRoles = new GroupTypeRoleService( new RockContext() ).GetByIds( groupRoleIds );
                GroupRequirementsState.ForEach( a =>
                {
                    if (a.GroupRoleId.HasValue)
                    {
                        a.GroupRole = groupRoles.FirstOrDefault( b => b.Id == a.GroupRoleId );
                    }
                } );
            }

            AllowMultipleLocations = ViewState["AllowMultipleLocations"] as bool? ?? false;

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

            gGroupRequirements.DataKeyNames = new string[] { "Guid" };
            gGroupRequirements.Actions.ShowAdd = true;
            gGroupRequirements.Actions.AddClick += gGroupRequirements_Add;
            gGroupRequirements.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupRequirements.GridRebind += gGroupRequirements_GridRebind;


            gMemberWorkflowTriggers.DataKeyNames = new string[] { "Guid" };
            gMemberWorkflowTriggers.Actions.ShowAdd = true;
            gMemberWorkflowTriggers.Actions.AddClick += gMemberWorkflowTriggers_Add;
            gMemberWorkflowTriggers.EmptyDataText = Server.HtmlEncode( None.Text );
            gMemberWorkflowTriggers.GridRebind += gMemberWorkflowTriggers_GridRebind;
            gMemberWorkflowTriggers.GridReorder += gMemberWorkflowTriggers_GridReorder; 
            
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Group ) ).Id;

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

            if ( !Page.IsPostBack )
            {
                string groupId = PageParameter( "GroupId" );
                if (!string.IsNullOrWhiteSpace(groupId))
                {
                    ShowDetail( groupId.AsInteger(), PageParameter( "ParentGroupId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
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
            Group group = groupService.Get( int.Parse( hfGroupId.Value ) );

            if ( group != null )
            {
                if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this group.", ModalAlertType.Information );
                    return;
                }

                parentGroupId = group.ParentGroupId;
                string errorMessage;
                if ( !groupService.CanDelete( group, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Role.Flush( group.Id );
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
                        scheduleService.Delete(schedule);
                    }
                }

                groupService.Delete( group );

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Flush();
                }
            }

            // reload page, selecting the deleted group's parent
            var qryParams = new Dictionary<string, string>();
            if ( parentGroupId != null )
            {
                qryParams["GroupId"] = parentGroupId.ToString();
            }

            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

            NavigateToPage( RockPage.Guid, qryParams );
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

            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            GroupRequirementService groupRequirementService = new GroupRequirementService( rockContext );
            GroupMemberWorkflowTriggerService groupMemberWorkflowTriggerService = new GroupMemberWorkflowTriggerService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            if ( CurrentGroupTypeId == 0 )
            {
                ddlGroupType.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( GroupType.FriendlyTypeName ) );
                return;
            }

            int groupId = int.Parse( hfGroupId.Value );

            if ( groupId == 0 )
            {
                group = new Group();
                group.IsSystem = false;
                group.Name = string.Empty;
            }
            else
            {
                group = groupService.Queryable( "Schedule,GroupLocations.Schedules" ).Where( g => g.Id == groupId ).FirstOrDefault();
                wasSecurityRole = group.IsSecurityRole;

                // remove any locations that removed in the UI
                var selectedLocations = GroupLocationsState.Select( l => l.Guid );
                foreach ( var groupLocation in group.GroupLocations.Where( l => !selectedLocations.Contains( l.Guid ) ).ToList() )
                {
                    group.GroupLocations.Remove( groupLocation );
                    groupLocationService.Delete( groupLocation );
                }

                // remove any group requirements that removed in the UI
                var selectedGroupRequirements = GroupRequirementsState.Select( a => a.Guid );
                foreach ( var groupRequirement in group.GroupRequirements.Where( a => !selectedGroupRequirements.Contains( a.Guid ) ).ToList() )
                {
                    group.GroupRequirements.Remove( groupRequirement );
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

            }

            // add/update any group requirements that were added or changed in the UI (we already removed the ones that were removed above)
            foreach ( var groupRequirementState in GroupRequirementsState)
            {
                GroupRequirement groupRequirement = group.GroupRequirements.Where( a => a.Guid == groupRequirementState.Guid ).FirstOrDefault();
                if (groupRequirement == null)
                {
                    groupRequirement = new GroupRequirement();
                    group.GroupRequirements.Add( groupRequirement );
                }

                groupRequirement.CopyPropertiesFrom( groupRequirementState );
            }

            // add/update any group locations that were added or changed in the UI (we already removed the ones that were removed above)
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
                    foreach( var schedule in groupLocation.Schedules.Where( s => !selectedSchedules.Contains( s.Guid)).ToList())
                    {
                        groupLocation.Schedules.Remove( schedule );
                    }
                }

                groupLocation.CopyPropertiesFrom( groupLocationState );

                var existingSchedules = groupLocation.Schedules.Select( s => s.Guid ).ToList();
                foreach ( var scheduleState in groupLocationState.Schedules.Where( s => !existingSchedules.Contains( s.Guid )).ToList())
                {
                    var schedule = scheduleService.Get( scheduleState.Guid );
                    if ( schedule != null )
                    {
                        groupLocation.Schedules.Add( schedule );
                    }
                }
            }

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
            group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
            group.GroupTypeId = CurrentGroupTypeId;
            group.ParentGroupId = gpParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( gpParentGroup.SelectedValue );
            group.IsSecurityRole = cbIsSecurityRole.Checked;
            group.IsActive = cbIsActive.Checked;
            group.IsPublic = cbIsPublic.Checked;
            group.MustMeetRequirementsToAddMember = cbMembersMustMeetRequirementsOnAdd.Checked;

            // save sync settings
            group.SyncDataViewId = dvpSyncDataview.SelectedValue.AsIntegerOrNull();
            group.WelcomeSystemEmailId = ddlWelcomeEmail.SelectedValue.AsIntegerOrNull();
            group.ExitSystemEmailId = ddlExitEmail.SelectedValue.AsIntegerOrNull();
            group.AddUserAccountsDuringSync = rbCreateLoginDuringSync.Checked;
            
            string iCalendarContent = string.Empty;

            // If unique schedule option was selected, but a schedule was not defined, set option to 'None'
            var scheduleType = rblScheduleSelect.SelectedValueAsEnum<ScheduleType>( ScheduleType.None );
            if ( scheduleType == ScheduleType.Custom )
            {
                iCalendarContent = sbSchedule.iCalendarContent;
                var calEvent = ScheduleICalHelper.GetCalenderEvent( iCalendarContent );
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
                    if ( schedule != null && string.IsNullOrEmpty(schedule.Name) )
                    {
                        scheduleService.Delete(schedule);
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

            Rock.Attribute.Helper.GetEditValues( phGroupAttributes, group );

            group.GroupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            if ( group.ParentGroupId.HasValue )
            {
                group.ParentGroup = groupService.Get( group.ParentGroupId.Value );
            }

            // Check to see if group type is allowed as a child of new parent group.
            if ( group.ParentGroup != null )
            {
                var allowedGroupTypeIds = GetAllowedGroupTypes( group.ParentGroup, rockContext ).Select( t => t.Id ).ToList();
                if ( !allowedGroupTypeIds.Contains(group.GroupTypeId) )
                {
                    var groupType = CurrentGroupTypeCache;
                    nbInvalidParentGroup.Text = string.Format( "The '{0}' group does not allow child groups with a '{1}' group type.", group.ParentGroup.Name, groupType != null ? groupType.Name : "" );
                    nbInvalidParentGroup.Visible = true;
                    return;
                }
            }
            
            // Check to see if user is still allowed to edit with selected group type and parent group
            if ( !group.IsAuthorized( Authorization.EDIT, CurrentPerson ))
            {
                nbNotAllowedToEdit.Visible = true;
                return;
            }

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !group.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
            rockContext.WrapTransaction( () =>
            {
                var adding = group.Id.Equals( 0 );
                if ( adding )
                {
                    groupService.Add( group );
                }

                rockContext.SaveChanges();

                if (adding)
                {
                    // add ADMINISTRATE to the person who added the group 
                    Rock.Security.Authorization.AllowPerson( group, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
                }

                group.SaveAttributeValues( rockContext );

                /* Take care of Group Member Attributes */
                var entityTypeId = EntityTypeCache.Read( typeof( GroupMember ) ).Id;
                string qualifierColumn = "GroupId";
                string qualifierValue = group.Id.ToString();

                // Get the existing attributes for this entity type and qualifier value
                var attributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

                // Delete any of those attributes that were removed in the UI
                var selectedAttributeGuids = GroupMemberAttributesState.Select( a => a.Guid );
                foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
                {
                    Rock.Web.Cache.AttributeCache.Flush( attr.Id );

                    attributeService.Delete( attr );
                }

                // Update the Attributes that were assigned in the UI
                foreach ( var attributeState in GroupMemberAttributesState )
                {
                    Rock.Attribute.Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
                }

                rockContext.SaveChanges();
            } );

            if ( group != null && wasSecurityRole )
            {
                if ( !group.IsSecurityRole )
                {
                    // if this group was a SecurityRole, but no longer is, flush
                    Rock.Security.Role.Flush( group.Id );
                    Rock.Security.Authorization.Flush();
                }
            }
            else
            {
                if ( group.IsSecurityRole )
                {
                    // new security role, flush
                    Rock.Security.Authorization.Flush();
                }
            }

            AttributeCache.FlushEntityAttributes();

            if ( triggersUpdated )
            {
                GroupMemberWorkflowTriggerService.FlushCachedTriggers();
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
                    // Cancelling on Add.  Return to Grid
                    NavigateToPage( RockPage.Guid, null );
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ShowReadonlyDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
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
            // grouptype changed, so load up the new attributes and set controls to the default attribute values
            CurrentGroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0;
            if ( CurrentGroupTypeId > 0 )
            {
                var group = new Group { GroupTypeId = CurrentGroupTypeId };
                var groupType = CurrentGroupTypeCache;
                SetScheduleControls( groupType, null);
                ShowGroupTypeEditDetails( groupType, group, true );
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
            var groupTypeQry = GetAllowedGroupTypes(parentGroup, rockContext);

            List<GroupType> groupTypes = groupTypeQry.OrderBy( a => a.Name ).ToList();
            if ( groupTypes.Count() > 1 )
            {
                // add a empty option so they are forced to choose
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
            }

            if ( group == null )
            {
                group = new Group { Id = 0, IsActive = true, IsPublic = true, ParentGroupId = parentGroupId, Name = "" };
                wpGeneral.Expanded = true;

                if ( parentGroupId.HasValue )
                {
                    // Set the new group's parent group (so security checks work)
                    var parentGroup = new GroupService( rockContext ).Get(parentGroupId.Value);
                    if ( parentGroup != null )
                    {
                        // Start by setting the group type to the same as the parent
                        group.ParentGroup = parentGroup;

                        // If the parent group type is allowed, first set that as the selected group type and check security
                        var allowedGroupTypes = GetAllowedGroupTypes( parentGroup, rockContext ).ToList();
                        if ( allowedGroupTypes.Any( t => t.Id == parentGroup.Id ) )
                        {
                            group.GroupTypeId = parentGroup.GroupTypeId;
                            group.GroupType = parentGroup.GroupType;

                            editAllowed = editAllowed || group.IsAuthorized( Authorization.EDIT, CurrentPerson );
                        }

                        // parent group type was not allowed, or user is not allowed to edit
                        if ( !editAllowed || group.GroupType == null )
                        {
                            // Loop through the other allowed group types to determine if user is allowed to edit any
                            foreach ( var groupType in allowedGroupTypes.Where( g => g.Id != parentGroup.GroupTypeId ) )
                            {
                                group.GroupTypeId = groupType.Id;
                                group.GroupType = groupType;
                                if ( group.IsAuthorized(Authorization.EDIT, CurrentPerson))
                                {
                                    // Once a group type is found that allows user to edit, keep that
                                    // group type by default
                                    editAllowed = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            viewAllowed = editAllowed || group.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = IsUserAuthorized( Authorization.EDIT ) || group.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            hfGroupId.Value = group.Id.ToString();

            // render UI based on Authorized and IsSystem
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

            var roleLimitWarnings = new StringBuilder();

            if ( group.GroupType != null && group.GroupType.Roles != null && group.GroupType.Roles.Any() )
            {
                foreach ( var role in group.GroupType.Roles )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    int curCount = groupMemberService.Queryable().Where( m => m.GroupId == group.Id && m.GroupRoleId == role.Id && m.GroupMemberStatus == GroupMemberStatus.Active ).Count();

                    if ( role.MinCount.HasValue && role.MinCount.Value > curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently below its minimum requirement of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize(), role.Name, role.MinCount, role.MinCount == 1 ? group.GroupType.GroupMemberTerm : group.GroupType.GroupMemberTerm.Pluralize() );
                    }

                    if ( role.MaxCount.HasValue && role.MaxCount.Value < curCount )
                    {
                        string format = "The <strong>{1}</strong> role is currently above its maximum limit of {2:N0} active {3}.<br/>";
                        roleLimitWarnings.AppendFormat( format, role.Name.Pluralize(), role.Name, role.MaxCount, role.MaxCount == 1 ? group.GroupType.GroupMemberTerm : group.GroupType.GroupMemberTerm.Pluralize() );
                    }
                }
            }

            nbRoleLimitWarning.Text = roleLimitWarnings.ToString();
            nbRoleLimitWarning.Visible = roleLimitWarnings.Length > 0;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( group );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = !group.IsSystem;
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
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( Group group )
        {
            if ( group.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Group.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();
            }

            ddlGroupType.Visible = group.Id == 0;
            lGroupType.Visible = group.Id != 0;

            SetEditMode( true );

            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            cbIsSecurityRole.Checked = group.IsSecurityRole;
            cbIsActive.Checked = group.IsActive;
            cbIsPublic.Checked = group.IsPublic;

            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns();

            gpParentGroup.SetValue( group.ParentGroup ?? groupService.Get( group.ParentGroupId ?? 0 ) );

            // hide sync and requirements panel if no admin access
            wpGroupSync.Visible = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            wpGroupRequirements.Visible = group.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            
            var systemEmails = new SystemEmailService( new RockContext() ).Queryable().OrderBy( e => e.Title );

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

            // set dataview
            dvpSyncDataview.EntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            dvpSyncDataview.SetValue( group.SyncDataViewId );

            if ( group.AddUserAccountsDuringSync.HasValue )
            {
                rbCreateLoginDuringSync.Checked = group.AddUserAccountsDuringSync.Value;
            }

            if ( group.WelcomeSystemEmailId.HasValue )
            {
                ddlWelcomeEmail.SetValue( group.WelcomeSystemEmailId );
            }

            if ( group.ExitSystemEmailId.HasValue )
            {
                ddlExitEmail.SetValue( group.ExitSystemEmailId );
            }
            

            // GroupType depends on Selected ParentGroup
            ddlParentGroup_SelectedIndexChanged( null, null );
            gpParentGroup.Label = "Parent Group";

            if ( group.Id == 0 && group.GroupType == null && ddlGroupType.Items.Count > 1 )
            {
                if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean() )
                {
                    // default GroupType for new Group to "Security Roles"  if LimittoSecurityRoleGroups
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
                    // if this is a new group (and not "LimitToSecurityRoleGroups", and there is more than one choice for GroupType, default to no selection so they are forced to choose (vs unintentionallly choosing the default one)
                    ddlGroupType.SelectedIndex = 0;
                }
            }
            else
            {
                CurrentGroupTypeId = group.GroupTypeId;
                ddlGroupType.SetValue( group.GroupTypeId );
                var groupType = GroupTypeCache.Read( group.GroupTypeId, rockContext );
                lGroupType.Text = groupType != null ? groupType.Name : "";
            }

            ddlCampus.SetValue( group.CampusId );

            GroupRequirementsState = group.GroupRequirements.ToList();
            GroupLocationsState = group.GroupLocations.ToList();

            var groupTypeCache = CurrentGroupTypeCache;
            SetScheduleControls( groupTypeCache, group );
            ShowGroupTypeEditDetails( groupTypeCache, group, true );

            // if this block's attribute limit group to SecurityRoleGroups, don't let them edit the SecurityRole checkbox value
            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean() )
            {
                cbIsSecurityRole.Enabled = false;
                cbIsSecurityRole.Checked = true;
            }

            string qualifierValue = group.Id.ToString();
            GroupMemberAttributesState = attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();
            BindGroupMemberAttributesGrid();

            BindInheritedAttributes( group.GroupTypeId, attributeService );

            cbMembersMustMeetRequirementsOnAdd.Checked = group.MustMeetRequirementsToAddMember ?? false;

            BindGroupRequirementsGrid();

            MemberWorkflowTriggersState = new List<GroupMemberWorkflowTrigger>();
            foreach ( var trigger in group.GroupMemberWorkflowTriggers )
            {
                MemberWorkflowTriggersState.Add( trigger );
            }
            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Shows the group type edit details.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowGroupTypeEditDetails( GroupTypeCache groupType, Group group, bool setValues )
        {
            if ( group != null )
            {
                // Save value to viewstate for use later when binding location grid
                AllowMultipleLocations = groupType != null && groupType.AllowMultipleLocations;

                // show/hide group sync panel based on permissions from the group type
                if ( group.GroupTypeId != 0 )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        GroupType selectedGroupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
                        if ( selectedGroupType != null )
                        {
                            wpGroupSync.Visible = selectedGroupType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                        }
                    }
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

                phGroupAttributes.Controls.Clear();
                group.LoadAttributes();

                if ( group.Attributes != null && group.Attributes.Any() )
                {
                    wpGroupAttributes.Visible = true;
                    Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, setValues, BlockValidationGroup );
                }
                else
                {
                    wpGroupAttributes.Visible = false;
                }
            }
        }

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
            SetEditMode( false );
            var rockContext = new RockContext();

            string groupIconHtml = string.Empty;
            if ( group.GroupType != null )
            {
                groupIconHtml = !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) ?
                    string.Format( "<i class='{0}' ></i>", group.GroupType.IconCssClass ) : string.Empty;
                hlType.Text = group.GroupType.Name;
            }

            hfGroupId.SetValue( group.Id );
            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = group.Name.FormatAsHtmlTitle();

            hlInactive.Visible = !group.IsActive;
            hlIsPrivate.Visible = !group.IsPublic;

            lGroupDescription.Text = group.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( group.ParentGroup != null )
            {
                descriptionList.Add( "Parent Group", group.ParentGroup.Name );
            }

            if ( group.Schedule != null )
            {
                descriptionList.Add( "Schedule", group.Schedule.ToString() );
            }

            if ( group.Campus != null )
            {
                hlCampus.Visible = true;
                hlCampus.Text = group.Campus.Name;
            }
            else
            {
                hlCampus.Visible = false;
            }

            lblMainDetails.Text = descriptionList.Html;

            group.LoadAttributes();
            var attributes = group.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // Display Attribute Values that have the "Display as Grid Column" flag enabled.
            var attributeCategories = Helper.GetAttributeCategories( attributes );

            var excludedAttributes = attributes.Where(x => !x.IsGridColumn).Select(x => x.Name).ToList();

            Rock.Attribute.Helper.AddDisplayControls( group, attributeCategories, phAttributes, excludedAttributes, false );

            var pageParams = new Dictionary<string, string>();
            pageParams.Add("GroupId", group.Id.ToString());

            hlAttendance.Visible = group.GroupType != null && group.GroupType.TakesAttendance;
            hlAttendance.NavigateUrl = LinkedPageUrl( "AttendancePage", pageParams );

            string groupMapUrl = LinkedPageUrl("GroupMapPage", pageParams);

            if ( group.Linkages.Any() )
            {
                rcwLinkedRegistrations.Visible = true;
                rptLinkedRegistrations.DataSource = group.Linkages
                    .Where( l => l.RegistrationInstanceId.HasValue )
                    .ToList()
                    .Select( l => new
                    {
                        RegistrationInstanceId = l.RegistrationInstanceId.Value,
                        Title = l.ToString( true, true, false )
                    } )
                    .ToList();
                rptLinkedRegistrations.DataBind();
            }
            else
            {
                rcwLinkedRegistrations.Visible = false;
            }

            // Get Map Style
            phMaps.Controls.Clear();
            var mapStyleValue = DefinedValueCache.Read( GetAttributeValue( "MapStyle" ) );
            if ( mapStyleValue == null )
            {
                mapStyleValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK );
            }

            if ( mapStyleValue != null )
            {
                string mapStyle = mapStyleValue.GetAttributeValue( "StaticMapStyle" );
                if ( !string.IsNullOrWhiteSpace( mapStyle ) )
                {
                    foreach ( GroupLocation groupLocation in group.GroupLocations.OrderBy( gl => (gl.GroupLocationTypeValue != null) ? gl.GroupLocationTypeValue.Order : int.MaxValue) )
                    {
                        if ( groupLocation.Location != null )
                        {
                            if ( groupLocation.Location.GeoPoint != null )
                            {
                                string markerPoints = string.Format( "{0},{1}", groupLocation.Location.GeoPoint.Latitude, groupLocation.Location.GeoPoint.Longitude );
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", markerPoints );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", string.Empty );
                                mapLink += "&sensor=false&size=350x200&zoom=13&format=png";
                                var literalcontrol = new Literal()
                                {
                                    Text = string.Format(
                                    "<div class='group-location-map'>{0}<a href='{1}'><img src='{2}'/></a></div>",
                                    groupLocation.GroupLocationTypeValue != null ? ( "<h4>" + groupLocation.GroupLocationTypeValue.Value + "</h4>" ) : string.Empty,
                                    groupMapUrl,
                                    mapLink ),
                                    Mode = LiteralMode.PassThrough
                                };
                                phMaps.Controls.Add( literalcontrol );
                            }
                            else if ( groupLocation.Location.GeoFence != null )
                            {
                                string polygonPoints = "enc:" + groupLocation.Location.EncodeGooglePolygon();
                                string mapLink = System.Text.RegularExpressions.Regex.Replace( mapStyle, @"\{\s*MarkerPoints\s*\}", string.Empty );
                                mapLink = System.Text.RegularExpressions.Regex.Replace( mapLink, @"\{\s*PolygonPoints\s*\}", polygonPoints );
                                mapLink += "&sensor=false&size=350x200&format=png";
                                phMaps.Controls.Add(
                                    new LiteralControl( string.Format(
                                        "<div class='group-location-map'>{0}<a href='{1}'><img src='{2}'/></a></div>",
                                        groupLocation.GroupLocationTypeValue != null ? ( "<h4>" + groupLocation.GroupLocationTypeValue.Value + "</h4>" ) : string.Empty,
                                        groupMapUrl,
                                        mapLink ) ) );
                            }
                        }
                    }
                }
            }

            hlMap.Visible = !string.IsNullOrWhiteSpace( groupMapUrl );
            hlMap.NavigateUrl = groupMapUrl;
            
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
                group = new GroupService( rockContext ).Queryable( "GroupType,GroupLocations.Schedules" )
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
        private IQueryable<GroupType> GetAllowedGroupTypes ( Group parentGroup, RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );

            var groupTypeQry = groupTypeService.Queryable();

            // limit GroupType selection to what Block Attributes allow
            List<Guid> groupTypeIncludeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().AsGuidList();
            List<Guid> groupTypeExcludeGuids = GetAttributeValue( "GroupTypesExclude" ).SplitDelimitedValues().AsGuidList();
            if ( groupTypeIncludeGuids.Any() )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeIncludeGuids.Contains( a.Guid ) );
            }
            else if (groupTypeExcludeGuids.Any())
            {
                groupTypeQry = groupTypeQry.Where( a => !groupTypeExcludeGuids.Contains( a.Guid ) );
            }

            // next, limit GroupType to ChildGroupTypes that the ParentGroup allows
            if ( parentGroup != null )
            {
                List<int> allowedChildGroupTypeIds = parentGroup.GroupType.ChildGroupTypes.Select( a => a.Id ).ToList();
                groupTypeQry = groupTypeQry.Where( a => allowedChildGroupTypeIds.Contains( a.Id ) );
            }

            // limit to GroupTypes where ShowInNavigation=True depending on block setting
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
        protected string RegistrationInstanceUrl ( int registrationInstanceId )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "RegistrationInstanceId", registrationInstanceId.ToString() );
            return LinkedPageUrl( "RegistrationInstancePage", qryParams );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
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
                var inheritedGroupType = GroupTypeCache.Read( inheritedGroupTypeId.Value );
                if ( inheritedGroupType != null )
                {
                    string qualifierValue = inheritedGroupType.Id.ToString();

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new GroupMember().TypeId ).AsQueryable()
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

        #endregion

        #region Location Grid and Picker

        /// <summary>
        /// Handles the Add event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLocations_Add( object sender, EventArgs e )
        {
            gLocations_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Edit( object sender, RowEventArgs e )
        {
            Guid locationGuid = (Guid)e.RowKeyValue;
            gLocations_ShowEdit( locationGuid );
        }

        /// <summary>
        /// Gs the locations_ show edit.
        /// </summary>
        /// <param name="locationGuid">The location unique identifier.</param>
        protected void gLocations_ShowEdit( Guid locationGuid )
        {
            var rockContext = new RockContext();
            ddlMember.Items.Clear();

            int? groupTypeId = ddlGroupType.SelectedValueAsId();
            if ( groupTypeId.HasValue )
            {
                var groupType = GroupTypeCache.Read( groupTypeId.Value );
                if ( groupType != null )
                {
                    GroupLocationPickerMode groupTypeModes = groupType.LocationSelectionMode;
                    if ( groupTypeModes != GroupLocationPickerMode.None )
                    {
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
                            locpGroupLocation.CurrentPickerMode = locpGroupLocation.GetBestPickerModeForLocation( null );
                        }

                        ddlLocationType.DataSource = groupType.LocationTypeValues.ToList();
                        ddlLocationType.DataBind();

                        var groupLocation = GroupLocationsState.FirstOrDefault( l => l.Guid.Equals( locationGuid ) );
                        if ( groupLocation != null && groupLocation.Location != null )
                        {
                            if ( displayOtherTab )
                            {
                                locpGroupLocation.CurrentPickerMode = locpGroupLocation.GetBestPickerModeForLocation( groupLocation.Location );

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
                        ShowSelectedPane();

                        ShowDialog( "Locations", true );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
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
        protected void dlgLocations_SaveClick( object sender, EventArgs e )
        {
            Location location = null;
            int? memberPersonAliasId = null;
            RockContext rockContext = new RockContext();

            if ( LocationTypeTab.Equals( MEMBER_LOCATION_TAB_TITLE ) )
            {
                if ( ddlMember.SelectedValue != null )
                {
                    var ids = ddlMember.SelectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( ids.Length == 2 )
                    {
                        var dbLocation = new LocationService( rockContext ).Get( int.Parse( ids[0] ) );
                        if ( dbLocation != null )
                        {
                            location = new Location();
                            location.CopyPropertiesFrom( dbLocation );
                        }

                        memberPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( int.Parse( ids[1] ) );
                    }
                }
            }
            else
            {
                if ( locpGroupLocation.Location != null )
                {
                    location = new Location();
                    location.CopyPropertiesFrom( locpGroupLocation.Location );
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

            HideDialog();
        }

        /// <summary>
        /// Binds the locations grid.
        /// </summary>
        private void BindLocationsGrid()
        {
            gLocations.Actions.ShowAdd = AllowMultipleLocations || !GroupLocationsState.Any();

            gLocations.DataSource = GroupLocationsState
                .Select( gl => new
                {
                    gl.Guid,
                    gl.Location,
                    Type = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Value : "",
                    Order = gl.GroupLocationTypeValue != null ? gl.GroupLocationTypeValue.Order : 0,
                    Schedules = gl.Schedules != null ? gl.Schedules.Select( s => s.Name).ToList().AsDelimited(", ") : ""
                } )
                .OrderBy( i => i.Order)
                .ToList();
            gLocations.DataBind();
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
            Guid groupRequirementGuid = (Guid)e.RowKeyValue;
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
            foreach (var item in list)
            {
                ddlGroupRequirementType.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            var selectedGroupRequirement = this.GroupRequirementsState.FirstOrDefault( a => a.Guid == groupRequirementGuid );
            grpGroupRequirementGroupRole.GroupTypeId = ddlGroupType.SelectedValue.AsIntegerOrNull();
            if (selectedGroupRequirement != null)
            {
                ddlGroupRequirementType.SelectedValue = selectedGroupRequirement.GroupRequirementTypeId.ToString();
                grpGroupRequirementGroupRole.GroupRoleId = selectedGroupRequirement.GroupRoleId;
            }
            else
            {
                ddlGroupRequirementType.SelectedIndex = 0;
                grpGroupRequirementGroupRole.GroupRoleId = null;
            }
            
            nbDuplicateGroupRequirement.Visible = false;

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
            if (groupRequirement == null)
            {
                groupRequirement = new GroupRequirement();
                groupRequirement.Guid = Guid.NewGuid();
                this.GroupRequirementsState.Add( groupRequirement );
            }

            groupRequirement.GroupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();
            groupRequirement.GroupRequirementType = new GroupRequirementTypeService( rockContext ).Get( groupRequirement.GroupRequirementTypeId );
            groupRequirement.GroupRoleId = grpGroupRequirementGroupRole.GroupRoleId;
            if ( groupRequirement.GroupRoleId.HasValue )
            {
                groupRequirement.GroupRole = new GroupTypeRoleService( rockContext ).Get( groupRequirement.GroupRoleId.Value );
            }
            else
            {
                groupRequirement.GroupRole = null;
            }

            // make sure we aren't adding a duplicate group requirement (same group requirement type and role)
            var duplicateGroupRequirement = this.GroupRequirementsState.Any( a => 
                a.GroupRequirementTypeId == groupRequirement.GroupRequirementTypeId 
                && a.GroupRoleId == groupRequirement.GroupRoleId 
                && a.Guid != groupRequirement.Guid );

            if (duplicateGroupRequirement)
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
            Guid rowGuid = (Guid)e.RowKeyValue;
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
            Guid attributeGuid = (Guid)e.RowKeyValue;
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
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
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
            Guid attributeGuid = (Guid)e.RowKeyValue;
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
            gGroupMemberAttributesInherited.AddCssClass( "inherited-attribute-grid" );
            gGroupMemberAttributesInherited.DataSource = GroupMemberAttributesInheritedState;
            gGroupMemberAttributesInherited.DataBind();
            rcGroupMemberAttributesInherited.Visible = GroupMemberAttributesInheritedState.Any();
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
        /// Binds the group requirements grid.
        /// </summary>
        private void BindGroupRequirementsGrid()
        {
            gGroupRequirements.AddCssClass( "group-requirements-grid" );
            gGroupRequirements.DataSource = GroupRequirementsState.OrderBy( a => a.GroupRequirementType.Name ).ToList();
            gGroupRequirements.DataBind();
        }

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
            Guid attributeGuid = (Guid)e.RowKeyValue;
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
            ddlTriggerFromStatus.Items.Insert( 0, new ListItem( "Any", "" ) );

            ddlTriggerToStatus.BindToEnum<GroupMemberStatus>( false );
            ddlTriggerToStatus.Items.Insert( 0, new ListItem( "Any", "" ) );

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
            ddlTriggerFromRole.Items.Insert( 0, new ListItem( "Any", "" ) );
            ddlTriggerToRole.Items.Insert( 0, new ListItem( "Any", "" ) );

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

            var qualifierParts = ( memberWorkflowTrigger.TypeQualifier ?? "" ).Split( new char[] { '|' } );
            ddlTriggerToStatus.SetValue( qualifierParts.Length > 0 ? qualifierParts[0] : string.Empty );
            ddlTriggerToRole.SetValue( qualifierParts.Length > 1 ? qualifierParts[1] : string.Empty );
            ddlTriggerFromStatus.SetValue( qualifierParts.Length > 2 ? qualifierParts[2] : string.Empty );
            ddlTriggerFromRole.SetValue( qualifierParts.Length > 3 ? qualifierParts[3] : string.Empty );
            cbTriggerFirstTime.Checked = qualifierParts.Length > 4 ? qualifierParts[4].AsBoolean() : false;

            ShowTriggerQualifierControls();
            ShowDialog( "MemberWorkflowTriggers", true );
        }

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

                        break;
                    }
                case GroupMemberWorkflowTriggerType.MemberAttendedGroup:
                    {
                        ddlTriggerFromStatus.Visible = false;
                        ddlTriggerToStatus.Visible = false;

                        ddlTriggerFromRole.Visible = false;
                        ddlTriggerToRole.Visible = false;

                        cbTriggerFirstTime.Visible = true;
                        
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
            Guid rowGuid = (Guid)e.RowKeyValue;
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

            memberWorkflowTrigger.TypeQualifier = string.Format( "{0}|{1}|{2}|{3}|{4}",
                ddlTriggerToStatus.SelectedValue, 
                ddlTriggerToRole.SelectedValue,
                ddlTriggerFromStatus.SelectedValue, 
                ddlTriggerFromRole.SelectedValue,
                cbTriggerFirstTime.Checked.ToString()
            );

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

        protected string FormatTriggerType( object type, object qualifier )
        {
            var triggerType = type.ToString().ConvertToEnum<GroupMemberWorkflowTriggerType>();
            var typeQualifer = qualifier.ToString();

            var qualiferText = new List<string>();
            var qualifierParts = ( typeQualifer ?? "" ).Split( new char[] { '|' } );

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