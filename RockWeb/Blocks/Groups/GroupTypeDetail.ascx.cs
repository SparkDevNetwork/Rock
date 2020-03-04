﻿// <copyright>
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
using Rock.UniversalSearch;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group Type Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group type for editing." )]
    public partial class GroupTypeDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private List<int> ChildGroupTypesList { get; set; }

        private Dictionary<Guid, DateRange> ScheduleExclusionDictionary { get; set; }

        private Dictionary<int, string> LocationTypesDictionary { get; set; }

        private List<InheritedAttribute> GroupTypeAttributesInheritedState { get; set; }

        private List<InheritedAttribute> GroupAttributesInheritedState { get; set; }

        private List<InheritedAttribute> GroupMemberAttributesInheritedState { get; set; }

        private List<Attribute> GroupTypeAttributesState { get; set; }

        private List<GroupRequirement> GroupTypeGroupRequirementsState { get; set; }

        private List<Attribute> GroupAttributesState { get; set; }

        private List<Attribute> GroupMemberAttributesState { get; set; }

        private List<GroupTypeRole> GroupTypeRolesState { get; set; }

        /// <summary>
        /// Gets or sets the default role unique identifier.
        /// </summary>
        /// <value>
        /// The default role unique identifier.
        /// </value>
        protected Guid DefaultRoleGuid { get; set; }

        private List<GroupMemberWorkflowTrigger> MemberWorkflowTriggersState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ChildGroupTypesList = ViewState["ChildGroupTypeList"] as List<int> ?? new List<int>();
            ScheduleExclusionDictionary = ViewState["ScheduleExclusionDictionary"] as Dictionary<Guid, DateRange> ?? new Dictionary<Guid, DateRange>();
            LocationTypesDictionary = ViewState["LocationTypesDictionary"] as Dictionary<int, string> ?? new Dictionary<int, string>();
            GroupTypeAttributesInheritedState = ViewState["GroupTypeAttributesInheritedState"] as List<InheritedAttribute> ?? new List<InheritedAttribute>();
            GroupAttributesInheritedState = ViewState["GroupAttributesInheritedState"] as List<InheritedAttribute> ?? new List<InheritedAttribute>();
            GroupMemberAttributesInheritedState = ViewState["GroupMemberAttributesInheritedState"] as List<InheritedAttribute> ?? new List<InheritedAttribute>();

            string json = ViewState["GroupTypeAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupTypeAttributesState = new List<Attribute>();
            }
            else
            {
                GroupTypeAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            json = ViewState["GroupTypeGroupRequirementsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupTypeGroupRequirementsState = new List<GroupRequirement>();
            }
            else
            {
                GroupTypeGroupRequirementsState = JsonConvert.DeserializeObject<List<GroupRequirement>>( json ) ?? new List<GroupRequirement>();
            }

            // get the GroupRole for each GroupRequirement from the database it case it isn't serialized, and we'll need it
            var groupRoleIds = GroupTypeGroupRequirementsState.Where( a => a.GroupRoleId.HasValue && a.GroupRole == null ).Select( a => a.GroupRoleId.Value ).Distinct().ToList();
            if ( groupRoleIds.Any() )
            {
                var groupRoles = new GroupTypeRoleService( new RockContext() ).GetByIds( groupRoleIds );
                GroupTypeGroupRequirementsState.ForEach( a =>
                {
                    if ( a.GroupRoleId.HasValue )
                    {
                        a.GroupRole = groupRoles.FirstOrDefault( b => b.Id == a.GroupRoleId );
                    }
                } );
            }

            json = ViewState["GroupAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupAttributesState = new List<Attribute>();
            }
            else
            {
                GroupAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
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

            json = ViewState["GroupTypeRolesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupTypeRolesState = new List<GroupTypeRole>();
            }
            else
            {
                GroupTypeRolesState = JsonConvert.DeserializeObject<List<GroupTypeRole>>( json );
            }

            DefaultRoleGuid = ViewState["DefaultRoleGuid"] as Guid? ?? Guid.Empty;

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

            gGroupTypeRoles.DataKeyNames = new string[] { "Guid" };
            gGroupTypeRoles.Actions.ShowAdd = true;
            gGroupTypeRoles.Actions.AddClick += gGroupTypeRoles_Add;
            gGroupTypeRoles.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupTypeRoles.GridRebind += gGroupTypeRoles_GridRebind;
            gGroupTypeRoles.GridReorder += gGroupTypeRoles_GridReorder;

            gChildGroupTypes.DataKeyNames = new string[] { "Id" };
            gChildGroupTypes.Actions.ShowAdd = true;
            gChildGroupTypes.Actions.AddClick += gChildGroupTypes_Add;
            gChildGroupTypes.GridRebind += gChildGroupTypes_GridRebind;
            gChildGroupTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gScheduleExclusions.DataKeyNames = new string[] { "key" };
            gScheduleExclusions.Actions.ShowAdd = true;
            gScheduleExclusions.Actions.AddClick += gScheduleExclusions_Add;
            gScheduleExclusions.GridRebind += gScheduleExclusions_GridRebind;
            gScheduleExclusions.EmptyDataText = Server.HtmlEncode( None.Text );

            gLocationTypes.DataKeyNames = new string[] { "key" };
            gLocationTypes.Actions.ShowAdd = true;
            gLocationTypes.Actions.AddClick += gLocationTypes_Add;
            gLocationTypes.GridRebind += gLocationTypes_GridRebind;
            gLocationTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gGroupTypeAttributesInherited.Actions.ShowAdd = false;
            gGroupTypeAttributesInherited.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupTypeAttributesInherited.GridRebind += gGroupTypeAttributesInherited_GridRebind;

            gGroupTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupTypeAttributes.Actions.ShowAdd = true;
            gGroupTypeAttributes.Actions.AddClick += gGroupTypeAttributes_Add;
            gGroupTypeAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupTypeAttributes.GridRebind += gGroupTypeAttributes_GridRebind;
            gGroupTypeAttributes.GridReorder += gGroupTypeAttributes_GridReorder;

            SecurityField groupTypeAttributeSecurityField = gGroupTypeAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            groupTypeAttributeSecurityField.EntityTypeId = EntityTypeCache.GetId<Attribute>() ?? 0;

            gGroupAttributesInherited.Actions.ShowAdd = false;
            gGroupAttributesInherited.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupAttributesInherited.GridRebind += gGroupAttributesInherited_GridRebind;

            gGroupAttributes.DataKeyNames = new string[] { "Guid" };
            gGroupAttributes.Actions.ShowAdd = true;
            gGroupAttributes.Actions.AddClick += gGroupAttributes_Add;
            gGroupAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupAttributes.GridRebind += gGroupAttributes_GridRebind;
            gGroupAttributes.GridReorder += gGroupAttributes_GridReorder;

            SecurityField groupAttributeSecurityField = gGroupAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            groupAttributeSecurityField.EntityTypeId = EntityTypeCache.GetId<Attribute>() ?? 0;

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

            gMemberWorkflowTriggers.DataKeyNames = new string[] { "Guid" };
            gMemberWorkflowTriggers.Actions.ShowAdd = true;
            gMemberWorkflowTriggers.Actions.AddClick += gMemberWorkflowTriggers_Add;
            gMemberWorkflowTriggers.EmptyDataText = Server.HtmlEncode( None.Text );
            gMemberWorkflowTriggers.GridRebind += gMemberWorkflowTriggers_GridRebind;
            gMemberWorkflowTriggers.GridReorder += gMemberWorkflowTriggers_GridReorder;

            gGroupTypeGroupRequirements.DataKeyNames = new string[] { "Guid" };
            gGroupTypeGroupRequirements.Actions.ShowAdd = true;
            gGroupTypeGroupRequirements.Actions.AddClick += gGroupTypeGroupRequirements_Add;
            gGroupTypeGroupRequirements.EmptyDataText = Server.HtmlEncode( None.Text );
            gGroupTypeGroupRequirements.GridRebind += gGroupTypeGroupRequirements_GridRebind;
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
                ShowDetail( PageParameter( "groupTypeId" ).AsInteger() );
            }
            else
            {
                Guid? newDefaultRole = Request.Form["GroupTypeDefaultRole"].AsGuidOrNull();
                if ( newDefaultRole.HasValue )
                {
                    DefaultRoleGuid = newDefaultRole.Value;
                }

                nbInvalidWorkflowType.Visible = false;
                ShowDialog();
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
            ViewState["ChildGroupTypeList"] = ChildGroupTypesList;
            ViewState["ScheduleExclusionDictionary"] = ScheduleExclusionDictionary;
            ViewState["LocationTypesDictionary"] = LocationTypesDictionary;
            ViewState["GroupTypeAttributesInheritedState"] = GroupTypeAttributesInheritedState;
            ViewState["GroupAttributesInheritedState"] = GroupAttributesInheritedState;
            ViewState["GroupMemberAttributesInheritedState"] = GroupMemberAttributesInheritedState;

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["GroupTypeAttributesState"] = JsonConvert.SerializeObject( GroupTypeAttributesState, Formatting.None, jsonSetting );
            ViewState["GroupAttributesState"] = JsonConvert.SerializeObject( GroupAttributesState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesState"] = JsonConvert.SerializeObject( GroupMemberAttributesState, Formatting.None, jsonSetting );
            ViewState["GroupTypeRolesState"] = JsonConvert.SerializeObject( GroupTypeRolesState, Formatting.None, jsonSetting );

            ViewState["DefaultRoleGuid"] = GroupMemberAttributesInheritedState;

            ViewState["MemberWorkflowTriggersState"] = JsonConvert.SerializeObject( MemberWorkflowTriggersState, Formatting.None, jsonSetting );

            ViewState["GroupTypeGroupRequirementsState"] = JsonConvert.SerializeObject( GroupTypeGroupRequirementsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupTypeId = PageParameter( pageReference, "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeId != null )
            {
                GroupType groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId.Value );
                if ( groupType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( groupType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Group Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        #region Action Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            GroupType groupType;

            bool triggersUpdated = false;

            var rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
            GroupMemberWorkflowTriggerService groupMemberWorkflowTriggerService = new GroupMemberWorkflowTriggerService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );
            GroupScheduleExclusionService scheduleExclusionService = new GroupScheduleExclusionService( rockContext );

            GroupRequirementService groupRequirementService = new GroupRequirementService( rockContext );

            int groupTypeId = hfGroupTypeId.Value.AsInteger();

            var groupTypeGroupRequirementsListInDatabase = groupRequirementService.Queryable().Where( a => a.GroupTypeId == groupTypeId ).ToList(); 

            if ( groupTypeId == 0 )
            {
                groupType = new GroupType();
                groupTypeService.Add( groupType );
            }
            else
            {
                groupType = groupTypeService.Get( groupTypeId );

                // remove any roles that were removed in the UI
                var selectedRoleGuids = GroupTypeRolesState.Select( r => r.Guid );
                foreach ( var role in groupType.Roles.Where( r => !selectedRoleGuids.Contains( r.Guid ) ).ToList() )
                {
                    groupType.Roles.Remove( role );
                    groupTypeRoleService.Delete( role );
                }

                // Remove any triggers that were removed in the UI
                var selectedTriggerGuids = MemberWorkflowTriggersState.Select( r => r.Guid );
                foreach ( var trigger in groupType.GroupMemberWorkflowTriggers.Where( r => !selectedTriggerGuids.Contains( r.Guid ) ).ToList() )
                {
                    groupType.GroupMemberWorkflowTriggers.Remove( trigger );
                    groupMemberWorkflowTriggerService.Delete( trigger );
                    triggersUpdated = true;
                }

                // remove any grouptype group requirements that removed in the UI
                var selectedGroupTypeGroupRequirements = GroupTypeGroupRequirementsState.Select( a => a.Guid );

                foreach ( var groupTypeGroupRequirement in groupTypeGroupRequirementsListInDatabase.Where( a => !selectedGroupTypeGroupRequirements.Contains( a.Guid ) ).ToList() )
                {
                    groupRequirementService.Delete( groupTypeGroupRequirement );
                }
            }

            // add/update any groupType group requirements that were added or changed in the UI (we already removed the ones that were removed above)
            var groupRequirementsToInsert = new List<GroupRequirement>();
            foreach ( var groupGroupTypeRequirementState in GroupTypeGroupRequirementsState )
            {
                GroupRequirement groupTypeGroupRequirement = groupTypeGroupRequirementsListInDatabase.Where( a => a.Guid == groupGroupTypeRequirementState.Guid ).FirstOrDefault();
                if ( groupTypeGroupRequirement == null )
                {
                    groupTypeGroupRequirement = new GroupRequirement();
                    groupRequirementsToInsert.Add( groupTypeGroupRequirement );
                }

                groupTypeGroupRequirement.CopyPropertiesFrom( groupGroupTypeRequirementState );
            }

            foreach ( var roleState in GroupTypeRolesState )
            {
                GroupTypeRole role = groupType.Roles.Where( r => r.Guid == roleState.Guid ).FirstOrDefault();
                if ( role == null )
                {
                    role = new GroupTypeRole();
                    groupType.Roles.Add( role );
                }
                else
                {
                    roleState.Id = role.Id;
                    roleState.Guid = role.Guid;
                }

                role.CopyPropertiesFrom( roleState );
            }

            foreach ( var triggerState in MemberWorkflowTriggersState )
            {
                GroupMemberWorkflowTrigger trigger = groupType.GroupMemberWorkflowTriggers.Where( r => r.Guid == triggerState.Guid ).FirstOrDefault();
                if ( trigger == null )
                {
                    trigger = new GroupMemberWorkflowTrigger();
                    groupType.GroupMemberWorkflowTriggers.Add( trigger );
                }
                else
                {
                    triggerState.Id = trigger.Id;
                    triggerState.Guid = trigger.Guid;
                }

                trigger.CopyPropertiesFrom( triggerState );

                triggersUpdated = true;
            }

            ScheduleType allowedScheduleTypes = ScheduleType.None;
            foreach ( ListItem li in cblScheduleTypes.Items )
            {
                if ( li.Selected )
                {
                    allowedScheduleTypes = allowedScheduleTypes | (ScheduleType)li.Value.AsInteger();
                }
            }

            GroupLocationPickerMode locationSelectionMode = GroupLocationPickerMode.None;
            foreach ( ListItem li in cblLocationSelectionModes.Items )
            {
                if ( li.Selected )
                {
                    locationSelectionMode = locationSelectionMode | (GroupLocationPickerMode)li.Value.AsInteger();
                }
            }

            groupType.Name = tbName.Text;
            groupType.Description = tbDescription.Text;
            groupType.GroupTerm = tbGroupTerm.Text;
            groupType.GroupMemberTerm = tbGroupMemberTerm.Text;
            groupType.AdministratorTerm = tbAdministratorTerm.Text;
            groupType.ShowInGroupList = cbShowInGroupList.Checked;
            groupType.ShowInNavigation = cbShowInNavigation.Checked;
            groupType.ShowConnectionStatus = cbShowConnectionStatus.Checked;
            groupType.ShowMaritalStatus = cbShowMaritalStatus.Checked;
            groupType.GroupViewLavaTemplate = ceGroupLavaTemplate.Text;
            groupType.IconCssClass = tbIconCssClass.Text;
            groupType.GroupTypeColor = cpGroupTypeColor.Text;
            groupType.TakesAttendance = cbTakesAttendance.Checked;
            groupType.GroupsRequireCampus = cbGroupsRequireCampus.Checked;
            groupType.EnableGroupTag = cbEnableGroupTag.Checked;
            groupType.AllowAnyChildGroupType = cbAllowAnyChildGroupType.Checked;
            groupType.ShowAdministrator = cbShowAdministrator.Checked;
            groupType.GroupAttendanceRequiresLocation = cbGroupAttendanceRequiresLocation.Checked;
            groupType.GroupAttendanceRequiresSchedule = cbGroupAttendanceRequiresSchedule.Checked;
            groupType.AttendanceCountsAsWeekendService = cbWeekendService.Checked;
            groupType.SendAttendanceReminder = cbSendAttendanceReminder.Checked;
            groupType.AttendanceRule = ddlAttendanceRule.SelectedValueAsEnum<AttendanceRule>();
            groupType.GroupCapacityRule = ddlGroupCapacityRule.SelectedValueAsEnum<GroupCapacityRule>();
            groupType.AttendancePrintTo = ddlPrintTo.SelectedValueAsEnum<PrintTo>();
            groupType.AllowedScheduleTypes = allowedScheduleTypes;
            groupType.LocationSelectionMode = locationSelectionMode;
            groupType.GroupTypePurposeValueId = dvpGroupTypePurpose.SelectedValueAsInt();
            groupType.AllowMultipleLocations = cbAllowMultipleLocations.Checked;
            groupType.InheritedGroupTypeId = gtpInheritedGroupType.SelectedGroupTypeId;
            groupType.IgnorePersonInactivated = cbDontInactivateMembers.Checked;
            groupType.IsIndexEnabled = cbEnableIndexing.Checked;
            groupType.EnableLocationSchedules = cbEnableLocationSchedules.Checked;
            groupType.AllowSpecificGroupMemberAttributes = cbAllowSpecificGroupMemberAttributes.Checked;
            groupType.AllowGroupSync = cbAllowGroupSync.Checked;
            groupType.EnableSpecificGroupRequirements = cbEnableSpecificGroupReq.Checked;
            groupType.AllowSpecificGroupMemberWorkflows = cbAllowSpecificGrpMemWorkFlows.Checked;
            groupType.GroupStatusDefinedTypeId = ddlGroupStatusDefinedType.SelectedValueAsInt();
            groupType.EnableInactiveReason = cbEnableInactiveReason.Checked;
            groupType.RequiresInactiveReason = cbRequireInactiveReason.Checked;

            // RSVP
            groupType.EnableRSVP = cbGroupRSVPEnabled.Checked;
            groupType.RSVPReminderSystemCommunicationId = ddlRsvpReminderSystemCommunication.SelectedValueAsInt();
            if ( rsRsvpReminderOffsetDays.SelectedValue == null || rsRsvpReminderOffsetDays.SelectedValue == 0 )
            {
                groupType.RSVPReminderOffsetDays = null;
            }
            else
            {
                groupType.RSVPReminderOffsetDays = rsRsvpReminderOffsetDays.SelectedValue;
            }

            // Scheduling
            groupType.IsSchedulingEnabled = cbSchedulingEnabled.Checked;
            groupType.ScheduleConfirmationSystemCommunicationId = ddlScheduleConfirmationSystemCommunication.SelectedValue.AsIntegerOrNull();
            groupType.RequiresReasonIfDeclineSchedule = cbRequiresReasonIfDeclineSchedule.Checked;
            groupType.ScheduleConfirmationEmailOffsetDays = nbScheduleConfirmationOffsetDays.Text.AsIntegerOrNull();
            groupType.ScheduleCancellationWorkflowTypeId = wtpScheduleCancellationWorkflowType.SelectedValueAsId();
            groupType.ScheduleReminderSystemCommunicationId = ddlScheduleReminderSystemCommunication.SelectedValue.AsIntegerOrNull();
            groupType.ScheduleReminderEmailOffsetDays = nbScheduleReminderOffsetDays.Text.AsIntegerOrNull();

            // if GroupHistory is turned off, we'll delete group and group member history for this group type
            bool deleteGroupHistory = false;
            if ( groupType.EnableGroupHistory && cbEnableGroupHistory.Checked == false )
            {
                deleteGroupHistory = true;
            }

            groupType.EnableGroupHistory = cbEnableGroupHistory.Checked;
            groupType.ChildGroupTypes = new List<GroupType>();
            groupType.ChildGroupTypes.Clear();
            foreach ( var item in ChildGroupTypesList )
            {
                var childGroupType = groupTypeService.Get( item );
                if ( childGroupType != null )
                {
                    groupType.ChildGroupTypes.Add( childGroupType );
                }
            }

            // Delete any removed exclusions
            foreach ( var exclusion in groupType.GroupScheduleExclusions.Where( s => !ScheduleExclusionDictionary.Keys.Contains( s.Guid ) ).ToList() )
            {
                groupType.GroupScheduleExclusions.Remove( exclusion );
                scheduleExclusionService.Delete( exclusion );
            }

            // Update exclusions
            foreach ( var keyVal in ScheduleExclusionDictionary )
            {
                var scheduleExclusion = groupType.GroupScheduleExclusions
                    .FirstOrDefault( s => s.Guid.Equals( keyVal.Key ) );
                if ( scheduleExclusion == null )
                {
                    scheduleExclusion = new GroupScheduleExclusion();
                    groupType.GroupScheduleExclusions.Add( scheduleExclusion );
                }

                scheduleExclusion.StartDate = keyVal.Value.Start;
                scheduleExclusion.EndDate = keyVal.Value.End;
            }

            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            groupType.LocationTypes = new List<GroupTypeLocationType>();
            groupType.LocationTypes.Clear();
            foreach ( var item in LocationTypesDictionary )
            {
                var locationType = definedValueService.Get( item.Key );
                if ( locationType != null )
                {
                    groupType.LocationTypes.Add( new GroupTypeLocationType { LocationTypeValueId = locationType.Id } );
                }
            }

            avcEditAttributes.GetEditValues( groupType );

            if ( !groupType.IsValid )
            {
                cvGroupType.IsValid = groupType.IsValid;
                cvGroupType.ErrorMessage = groupType.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            // need WrapTransaction due to Attribute saves    
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                if ( groupRequirementsToInsert.Any() )
                {
                    groupRequirementsToInsert.ForEach( a => a.GroupTypeId = groupType.Id );
                    groupRequirementService.AddRange( groupRequirementsToInsert );
                }

                groupType.SaveAttributeValues( rockContext );

                /* Save Attributes */
                string qualifierValue = groupType.Id.ToString();
                SaveAttributes( new GroupType().TypeId, "Id", qualifierValue, GroupTypeAttributesState, rockContext );
                SaveAttributes( new Group().TypeId, "GroupTypeId", qualifierValue, GroupAttributesState, rockContext );
                SaveAttributes( new GroupMember().TypeId, "GroupTypeId", qualifierValue, GroupMemberAttributesState, rockContext );

                // Reload to save default role
                groupType = groupTypeService.Get( groupType.Id );
                groupType.DefaultGroupRole = groupType.Roles.FirstOrDefault( r => r.Guid.Equals( DefaultRoleGuid ) );
                if ( groupType.DefaultGroupRole == null )
                {
                    groupType.DefaultGroupRole = groupType.Roles.FirstOrDefault();
                }

                rockContext.SaveChanges();

                // Reload the roles and apply their attribute values
                foreach ( var role in groupTypeRoleService.GetByGroupTypeId( groupType.Id ).ToList() )
                {
                    role.LoadAttributes( rockContext );
                    var roleState = GroupTypeRolesState.Where( r => r.Guid.Equals( role.Guid ) ).FirstOrDefault();
                    if ( roleState != null && roleState.AttributeValues != null )
                    {
                        foreach ( var attributeValue in roleState.AttributeValues )
                        {
                            role.SetAttributeValue( attributeValue.Key, roleState.GetAttributeValue( attributeValue.Key ) );
                        }

                        role.SaveAttributeValues( rockContext );
                    }
                }
            } );

            if ( triggersUpdated )
            {
                GroupMemberWorkflowTriggerService.RemoveCachedTriggers();
            }

            if ( deleteGroupHistory )
            {
                groupTypeService.BulkDeleteGroupHistory( groupType.Id );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the gtpInheritedGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gtpInheritedGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );
            var attributeService = new AttributeService( rockContext );
            BindInheritedAttributes( gtpInheritedGroupType.SelectedValueAsInt(), groupTypeService, attributeService );
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void ShowDetail( int groupTypeId )
        {
            // determine if indexing is enabled
            var groupEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.GROUP.AsGuid() );
            cbEnableIndexing.Visible = IndexContainer.IndexingEnabled && groupEntityType.IsIndexingEnabled;

            GroupType groupType = null;

            if ( !groupTypeId.Equals( 0 ) )
            {
                groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId );
                pdAuditDetails.SetEntity( groupType, ResolveRockUrl( "~" ) );
            }

            if ( groupType == null )
            {
                groupType = new GroupType { Id = 0, ShowInGroupList = true, GroupTerm = "Group", GroupMemberTerm = "Member" };
                groupType.ChildGroupTypes = new List<GroupType>();
                groupType.LocationTypes = new List<GroupTypeLocationType>();

                Guid defaultRoleGuid = Guid.NewGuid();
                var memberRole = new GroupTypeRole { Guid = defaultRoleGuid, Name = "Member" };
                groupType.Roles.Add( memberRole );
                groupType.DefaultGroupRole = memberRole;

                groupType.AllowedScheduleTypes = ScheduleType.None;
                groupType.LocationSelectionMode = GroupLocationPickerMode.None;

                var systemCommunicationIdLookup = new SystemCommunicationService( new RockContext() ).Queryable().ToDictionary( k => k.Guid, v => v.Id );

                groupType.ScheduleConfirmationSystemCommunicationId = systemCommunicationIdLookup.GetValueOrNull( Rock.SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION.AsGuid() );
                groupType.ScheduleReminderSystemCommunicationId = systemCommunicationIdLookup.GetValueOrNull( Rock.SystemGuid.SystemCommunication.SCHEDULING_REMINDER.AsGuid() );

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            bool editAllowed = groupType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            DefaultRoleGuid = groupType.DefaultGroupRole != null ? groupType.DefaultGroupRole.Guid : Guid.Empty;

            hfGroupTypeId.Value = groupType.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupType.FriendlyTypeName );
            }

            if ( groupType.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( GroupType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( groupType );
            }
            else
            {
                ShowEditDetails( groupType );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        private void ShowEditDetails( GroupType groupType )
        {
            hlType.Visible = false;
            if ( groupType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( GroupType.FriendlyTypeName ).FormatAsHtmlTitle();
                if ( groupType.GroupTypePurposeValue != null )
                {
                    hlType.Text = groupType.GroupTypePurposeValue.Value;
                    hlType.Visible = true;
                }
            }
            else
            {
                lReadOnlyTitle.Text = groupType.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            var rockContext = new RockContext();

            var groupTypeService = new GroupTypeService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns( groupType.Id );

            // General
            tbName.ReadOnly = groupType.IsSystem;
            tbName.Text = groupType.Name;

            tbDescription.ReadOnly = groupType.IsSystem;
            tbDescription.Text = groupType.Description;

            groupType.LoadAttributes();
            avcEditAttributes.ShowCategoryLabel = false;
            avcEditAttributes.ExcludedAttributes = groupType.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();
            avcEditAttributes.AddEditControls( groupType );

            tbGroupTerm.ReadOnly = groupType.IsSystem;
            tbGroupTerm.Text = groupType.GroupTerm;

            tbGroupMemberTerm.ReadOnly = groupType.IsSystem;
            tbGroupMemberTerm.Text = groupType.GroupMemberTerm;

            tbAdministratorTerm.ReadOnly = groupType.IsSystem;
            tbAdministratorTerm.Text = groupType.AdministratorTerm;

            dvpGroupTypePurpose.Enabled = !groupType.IsSystem;
            dvpGroupTypePurpose.SetValue( groupType.GroupTypePurposeValueId );

            ddlGroupStatusDefinedType.Enabled = !groupType.IsSystem;
            ddlGroupStatusDefinedType.SetValue( groupType.GroupStatusDefinedTypeId );

            ddlGroupCapacityRule.SetValue( (int)groupType.GroupCapacityRule );

            cbAllowAnyChildGroupType.Checked = groupType.AllowAnyChildGroupType;
            rcwAllowedChildGroupTypes.Visible = !cbAllowAnyChildGroupType.Checked;

            ChildGroupTypesList = new List<int>();
            groupType.ChildGroupTypes.ToList().ForEach( a => ChildGroupTypesList.Add( a.Id ) );
            BindChildGroupTypesGrid();

            cbEnableGroupTag.Checked = groupType.EnableGroupTag;
            cbGroupsRequireCampus.Checked = groupType.GroupsRequireCampus;
            cbShowAdministrator.Checked = groupType.ShowAdministrator;

            // Display
            cbShowInGroupList.Checked = groupType.ShowInGroupList;
            cbShowInNavigation.Checked = groupType.ShowInNavigation;
            cbShowConnectionStatus.Checked = groupType.ShowConnectionStatus;
            cbShowMaritalStatus.Checked = groupType.ShowMaritalStatus;
            if ( !string.IsNullOrEmpty( groupType.GroupViewLavaTemplate ) )
            {
                ceGroupLavaTemplate.Text = groupType.GroupViewLavaTemplate;
            }
            else
            {
                ceGroupLavaTemplate.Text = SystemSettings.GetValue( "core_templates_GroupViewTemplate" );
            }

            tbIconCssClass.Text = groupType.IconCssClass;
            cpGroupTypeColor.Text = groupType.GroupTypeColor;

            // Locations
            cbAllowMultipleLocations.Enabled = !groupType.IsSystem;
            cbAllowMultipleLocations.Checked = groupType.AllowMultipleLocations;

            cblScheduleTypes.Enabled = !groupType.IsSystem;
            foreach ( ListItem li in cblScheduleTypes.Items )
            {
                ScheduleType scheduleType = (ScheduleType)li.Value.AsInteger();
                li.Selected = ( groupType.AllowedScheduleTypes & scheduleType ) == scheduleType;
            }

            ScheduleExclusionDictionary = new Dictionary<Guid, DateRange>();
            groupType.GroupScheduleExclusions.ToList().ForEach( s => ScheduleExclusionDictionary.Add( s.Guid, new DateRange( s.StartDate, s.EndDate ) ) );
            BindScheduleExclusionsGrid();

            cblLocationSelectionModes.Enabled = !groupType.IsSystem;
            foreach ( ListItem li in cblLocationSelectionModes.Items )
            {
                GroupLocationPickerMode mode = (GroupLocationPickerMode)li.Value.AsInteger();
                li.Selected = ( groupType.LocationSelectionMode & mode ) == mode;
            }

            LocationTypesDictionary = new Dictionary<int, string>();
            groupType.LocationTypes.ToList().ForEach( a => LocationTypesDictionary.Add( a.LocationTypeValueId, a.LocationTypeValue.Value ) );
            BindLocationTypesGrid();

            // Support Location Schedules
            cbEnableLocationSchedules.Enabled = !groupType.IsSystem;
            cbEnableLocationSchedules.Checked = groupType.EnableLocationSchedules ?? false;

            // Check In
            cbTakesAttendance.Checked = groupType.TakesAttendance;
            cbWeekendService.Checked = groupType.AttendanceCountsAsWeekendService;
            cbSendAttendanceReminder.Checked = groupType.SendAttendanceReminder;
            ddlAttendanceRule.SetValue( (int)groupType.AttendanceRule );
            ddlPrintTo.SetValue( (int)groupType.AttendancePrintTo );
            cbGroupAttendanceRequiresSchedule.Checked = groupType.GroupAttendanceRequiresSchedule;
            cbGroupAttendanceRequiresLocation.Checked = groupType.GroupAttendanceRequiresLocation;

            // RSVP
            ddlRsvpReminderSystemCommunication.SetValue( groupType.RSVPReminderSystemCommunicationId );
            rsRsvpReminderOffsetDays.SelectedValue = groupType.RSVPReminderOffsetDays;

            SetRsvpEnabledState( groupType.EnableRSVP );

            // Scheduling
            cbSchedulingEnabled.Checked = groupType.IsSchedulingEnabled;

            ddlScheduleConfirmationSystemCommunication.SetValue( groupType.ScheduleConfirmationSystemCommunicationId );
            cbRequiresReasonIfDeclineSchedule.Checked = groupType.RequiresReasonIfDeclineSchedule;
            nbScheduleConfirmationOffsetDays.Text = groupType.ScheduleConfirmationEmailOffsetDays.ToString();
            wtpScheduleCancellationWorkflowType.SetValue( groupType.ScheduleCancellationWorkflowTypeId );
            ddlScheduleReminderSystemCommunication.SetValue( groupType.ScheduleReminderSystemCommunicationId );
            nbScheduleReminderOffsetDays.Text = groupType.ScheduleReminderEmailOffsetDays.ToString();

            // Attributes
            gtpInheritedGroupType.Enabled = !groupType.IsSystem;
            gtpInheritedGroupType.SelectedGroupTypeId = groupType.InheritedGroupTypeId;

            cbDontInactivateMembers.Checked = groupType.IgnorePersonInactivated;
            cbEnableIndexing.Checked = groupType.IsIndexEnabled;

            cbAllowSpecificGroupMemberAttributes.Checked = groupType.AllowSpecificGroupMemberAttributes;
            cbAllowGroupSync.Checked = groupType.AllowGroupSync;
            cbEnableSpecificGroupReq.Checked = groupType.EnableSpecificGroupRequirements;
            cbAllowSpecificGrpMemWorkFlows.Checked = groupType.AllowSpecificGroupMemberWorkflows;
            cbEnableGroupHistory.Checked = groupType.EnableGroupHistory;

            cbEnableInactiveReason.Checked = groupType.EnableInactiveReason;
            cbRequireInactiveReason.Enabled = groupType.EnableInactiveReason;
            cbRequireInactiveReason.Checked = groupType.RequiresInactiveReason;

            GroupTypeRolesState = new List<GroupTypeRole>();
            foreach ( var role in groupType.Roles )
            {
                role.LoadAttributes();
                GroupTypeRolesState.Add( role );
            }

            BindGroupTypeRolesGrid();

            string qualifierValue = groupType.Id.ToString();

            GroupTypeAttributesState = attributeService.GetByEntityTypeId( new GroupType().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            BindGroupTypeAttributesGrid();

            GroupAttributesState = attributeService.GetByEntityTypeId( new Group().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            BindGroupAttributesGrid();

            GroupTypeGroupRequirementsState = new GroupRequirementService(rockContext).Queryable().Where(a => a.GroupTypeId.HasValue && a.GroupTypeId == groupType.Id ).ToList();
            BindGroupTypeGroupRequirementsGrid();

            GroupMemberAttributesState = attributeService.GetByEntityTypeId( new GroupMember().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            BindGroupMemberAttributesGrid();

            BindInheritedAttributes( groupType.InheritedGroupTypeId, groupTypeService, attributeService );

            MemberWorkflowTriggersState = new List<GroupMemberWorkflowTrigger>();
            foreach ( var trigger in groupType.GroupMemberWorkflowTriggers )
            {
                MemberWorkflowTriggersState.Add( trigger );
            }

            BindMemberWorkflowTriggersGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( GroupType groupType )
        {
            SetEditMode( false );

            hfGroupTypeId.SetValue( groupType.Id );
            lReadOnlyTitle.Text = groupType.Name.FormatAsHtmlTitle();
            if ( groupType.GroupTypePurposeValue != null )
            {
                hlType.Text = groupType.GroupTypePurposeValue.Value;
                hlType.Visible = true;
            }
            else
            {
                hlType.Visible = false;
            }

            lGroupTypeDescription.Text = groupType.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( string.Empty, string.Empty );
            lblMainDetails.Text = descriptionList.Html;

            groupType.LoadAttributes();
            avcDisplayAttributes.ExcludedAttributes = groupType.Attributes.Where( a => !a.Value.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();

            // Don't show the Categories, since they will probably be 'Start of Registration' or 'End of Registration';
            avcDisplayAttributes.ShowCategoryLabel = false;
            avcDisplayAttributes.AddDisplayControls( groupType );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( int? groupTypeId )
        {
            ddlAttendanceRule.BindToEnum<Rock.Model.AttendanceRule>();

            ddlGroupCapacityRule.BindToEnum<GroupCapacityRule>();

            cblScheduleTypes.Items.Clear();
            cblScheduleTypes.Items.Add( new ListItem( "Weekly", "1" ) );
            cblScheduleTypes.Items.Add( new ListItem( "Custom", "2" ) );
            cblScheduleTypes.Items.Add( new ListItem( "Named", "4" ) );

            cblLocationSelectionModes.Items.Clear();
            cblLocationSelectionModes.Items.Add( new ListItem( "Named", "2" ) );
            cblLocationSelectionModes.Items.Add( new ListItem( "Address", "1" ) );
            cblLocationSelectionModes.Items.Add( new ListItem( "Point", "4" ) );
            cblLocationSelectionModes.Items.Add( new ListItem( "Geo-fence", "8" ) );
            cblLocationSelectionModes.Items.Add( new ListItem( "Group Member Address", "16" ) );

            var rockContext = new RockContext();
            gtpInheritedGroupType.GroupTypes = new GroupTypeService( rockContext ).Queryable()
                .Where( g => g.Id != groupTypeId )
                .ToList();

            var groupTypePurposeDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() ).Id;
            dvpGroupTypePurpose.DefinedTypeId = groupTypePurposeDefinedTypeId;

            ddlGroupStatusDefinedType.Items.Clear();
            ddlGroupStatusDefinedType.Items.Add( new ListItem() );
            foreach ( var definedType in DefinedTypeCache.All().OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                ddlGroupStatusDefinedType.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            // Initialize list and add empty value selection items.
            ddlScheduleConfirmationSystemCommunication.Items.Clear();
            ddlScheduleConfirmationSystemCommunication.Items.Add( new ListItem() );
            ddlScheduleReminderSystemCommunication.Items.Clear();
            ddlScheduleReminderSystemCommunication.Items.Add( new ListItem() );
            ddlRsvpReminderSystemCommunication.Items.Clear();
            ddlRsvpReminderSystemCommunication.Items.Add( new ListItem() );

            // Get a list of System Communications - these are used for Group Scheduling and RSVP Reminders.
            var systemCommunications = new SystemCommunicationService( new RockContext() )
                .Queryable()
                .OrderBy( t => t.Title )
                .ToList();

            // Group Scheduling System Communications.
            var scheduleReminderCommunications = systemCommunications
                .Select( a => new
                {
                    a.Id,
                    a.Title
                } );

            if ( scheduleReminderCommunications.Any() )
            {
                foreach ( var scheduleReminder in scheduleReminderCommunications )
                {
                    ddlScheduleConfirmationSystemCommunication.Items.Add( new ListItem(scheduleReminder.Title, scheduleReminder.Id.ToString() ) );
                    ddlScheduleReminderSystemCommunication.Items.Add( new ListItem(scheduleReminder.Title, scheduleReminder.Id.ToString() ) );
                }
            }

            // RSVP Reminder System Communications.
            var rsvpReminderCategoryId = CategoryCache.GetId( Rock.SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION.AsGuid() );
            var rsvpReminderCommunications = systemCommunications
                .Where( c => c.CategoryId == rsvpReminderCategoryId );

            foreach ( var rsvpReminder in rsvpReminderCommunications )
            {
                ddlRsvpReminderSystemCommunication.Items.Add( new ListItem( rsvpReminder.Title, rsvpReminder.Id.ToString() ) );
            }

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
                case "GROUPTYPEROLES":

                    var role = GroupTypeRolesState.FirstOrDefault( r => r.Guid.Equals( hfRoleGuid.Value.AsGuid() ) );
                    if ( role == null )
                    {
                        role = new GroupTypeRole();
                        role.GroupTypeId = hfGroupTypeId.ValueAsInt();
                        role.LoadAttributes();
                    }

                    Helper.AddEditControls( role, phGroupTypeRoleAttributes, setValues, dlgGroupTypeRoles.ValidationGroup );
                    SetValidationGroup( phGroupTypeRoleAttributes.Controls, dlgGroupTypeRoles.ValidationGroup );

                    dlgGroupTypeRoles.Show();
                    break;

                case "CHILDGROUPTYPES":
                    dlgChildGroupType.Show();
                    break;
                case "SCHEDULEEXCLUSION":
                    dlgScheduleExclusion.Show();
                    break;
                case "LOCATIONTYPE":
                    dlgLocationType.Show();
                    break;
                case "GROUPTYPEATTRIBUTES":
                    dlgGroupTypeAttribute.Show();
                    break;
                case "GROUPATTRIBUTES":
                    if (cbEnableIndexing.Visible && cbEnableIndexing.Checked )
                    {
                        edtGroupAttributes.IsIndexingEnabledVisible = true;
                    }
                    else
                    {
                        edtGroupAttributes.IsIndexingEnabledVisible = false;
                    }

                    dlgGroupAttribute.Show();
                    break;
                case "GROUPMEMBERATTRIBUTES":
                    dlgGroupMemberAttribute.Show();
                    break;
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgMemberWorkflowTriggers.Show();
                    break;
                case "GROUPTYPEGROUPREQUIREMENTS":
                    mdGroupTypeGroupRequirement.Show();
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
                case "GROUPTYPEROLES":
                    dlgGroupTypeRoles.Hide();
                    break;
                case "CHILDGROUPTYPES":
                    dlgChildGroupType.Hide();
                    break;
                case "SCHEDULEEXCLUSION":
                    dlgScheduleExclusion.Hide();
                    break;
                case "LOCATIONTYPE":
                    dlgLocationType.Hide();
                    break;
                case "GROUPTYPEATTRIBUTES":
                    dlgGroupTypeAttribute.Hide();
                    break;
                case "GROUPATTRIBUTES":
                    dlgGroupAttribute.Hide();
                    break;
                case "GROUPMEMBERATTRIBUTES":
                    dlgGroupMemberAttribute.Hide();
                    break;
                case "MEMBERWORKFLOWTRIGGERS":
                    dlgMemberWorkflowTriggers.Hide();
                    break;
                case "GROUPTYPEGROUPREQUIREMENTS":
                    mdGroupTypeGroupRequirement.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Binds the inherited attributes.
        /// </summary>
        /// <param name="inheritedGroupTypeId">The inherited group type identifier.</param>
        /// <param name="groupTypeService">The group type service.</param>
        /// <param name="attributeService">The attribute service.</param>
        private void BindInheritedAttributes( int? inheritedGroupTypeId, GroupTypeService groupTypeService, AttributeService attributeService )
        {
            GroupTypeAttributesInheritedState = new List<InheritedAttribute>();
            GroupAttributesInheritedState = new List<InheritedAttribute>();
            GroupMemberAttributesInheritedState = new List<InheritedAttribute>();

            while ( inheritedGroupTypeId.HasValue )
            {
                var inheritedGroupType = groupTypeService.Get( inheritedGroupTypeId.Value );
                if ( inheritedGroupType != null )
                {
                    string qualifierValue = inheritedGroupType.Id.ToString();

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new GroupType().TypeId, true ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "Id", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        GroupTypeAttributesInheritedState.Add( new InheritedAttribute(
                            attribute.Name,
                            attribute.Key,
                            attribute.Description,
                            Page.ResolveUrl( "~/GroupType/" + attribute.EntityTypeQualifierValue ),
                            inheritedGroupType.Name ) );
                    }

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new Group().TypeId, true ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "GroupTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList() )
                    {
                        GroupAttributesInheritedState.Add( new InheritedAttribute(
                            attribute.Name,
                            attribute.Key,
                            attribute.Description,
                            Page.ResolveUrl( "~/GroupType/" + attribute.EntityTypeQualifierValue ),
                            inheritedGroupType.Name ) );
                    }

                    foreach ( var attribute in attributeService.GetByEntityTypeId( new GroupMember().TypeId, true ).AsQueryable()
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

            BindGroupTypeAttributesInheritedGrid();
            BindGroupAttributesInheritedGrid();
            BindGroupMemberAttributesInheritedGrid();
        }

        /// <summary>
        /// Sets the group type role list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetGroupTypeRoleListOrder( List<GroupTypeRole> itemList )
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
        private void ReorderGroupTypeRoleList( List<GroupTypeRole> itemList, int oldIndex, int newIndex )
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
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetAttributeListOrder( List<Attribute> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
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
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="viewStateAttributes">The view state attributes.</param>
        /// <param name="attributeService">The attribute service.</param>
        /// <param name="qualifierService">The qualifier service.</param>
        /// <param name="categoryService">The category service.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Attribute> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var regFieldService = new RegistrationTemplateFormFieldService( rockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                foreach ( var field in regFieldService.Queryable().Where( f => f.AttributeId.HasValue && f.AttributeId.Value == attr.Id ).ToList() )
                {
                    regFieldService.Delete( field );
                }

                attributeService.Delete( attr );
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        #endregion

        #region GroupTypeRoles Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupTypeRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeRoles_Add( object sender, EventArgs e )
        {
            gGroupTypeRoles_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupTypeRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeRoles_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupTypeRoles_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupTypeRoles_ShowEdit( Guid groupTypeRoleGuid )
        {
            GroupTypeRole groupTypeRole = GroupTypeRolesState.FirstOrDefault( a => a.Guid.Equals( groupTypeRoleGuid ) );
            if ( groupTypeRole == null )
            {
                groupTypeRole = new GroupTypeRole();
                dlgGroupTypeRoles.Title = "Add Role";
            }
            else
            {
                dlgGroupTypeRoles.Title = "Edit Role";
            }

            hfRoleGuid.Value = groupTypeRole.Guid.ToString();
            tbRoleName.Text = groupTypeRole.Name;
            tbRoleDescription.Text = groupTypeRole.Description;

            string groupTerm = string.IsNullOrWhiteSpace( tbGroupTerm.Text ) ? "Group" : tbGroupTerm.Text;
            string memberTerm = string.IsNullOrWhiteSpace( tbGroupMemberTerm.Text ) ? "Member" : tbGroupMemberTerm.Text;

            cbIsLeader.Checked = groupTypeRole.IsLeader;
            cbReceiveRequirementsNotifications.Checked = groupTypeRole.ReceiveRequirementsNotifications;
            cbCanView.Checked = groupTypeRole.CanView;
            cbCanEdit.Checked = groupTypeRole.CanEdit;
            cbCanManageMembers.Checked = groupTypeRole.CanManageMembers;

            nbMinimumRequired.Text = groupTypeRole.MinCount.HasValue ? groupTypeRole.MinCount.ToString() : string.Empty;
            nbMinimumRequired.Help = string.Format(
                "The minimum number of {0} in this {1} that are required to have this role.",
                memberTerm.Pluralize(),
                groupTerm );

            nbMaximumAllowed.Text = groupTypeRole.MaxCount.HasValue ? groupTypeRole.MaxCount.ToString() : string.Empty;
            nbMaximumAllowed.Help = string.Format(
                "The maximum number of {0} in this {1} that are allowed to have this role.",
                memberTerm.Pluralize(),
                groupTerm );

            ShowDialog( "GroupTypeRoles", true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gGroupTypeRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeRoles_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderGroupTypeRoleList( GroupTypeRolesState, e.OldIndex, e.NewIndex );
            BindGroupTypeRolesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gGroupTypeRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupTypeRoles_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;

            string errorMessage = string.Empty;
            var groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
            var groupTypeRole = groupTypeRoleService.Get( rowGuid );
            if ( !groupTypeRoleService.CanDelete( groupTypeRole, out errorMessage ) )
            {
                mdGroupTypeRolesDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            GroupTypeRolesState.RemoveEntity( rowGuid );
            BindGroupTypeRolesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupTypeRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeRoles_GridRebind( object sender, EventArgs e )
        {
            BindGroupTypeRolesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupMemberAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeRoles_SaveClick( object sender, EventArgs e )
        {
            var groupTypeRole = new GroupTypeRole();

            var groupTypeRoleState = GroupTypeRolesState.FirstOrDefault( r => r.Guid.Equals( hfRoleGuid.Value.AsGuid() ) );
            if ( groupTypeRoleState != null )
            {
                groupTypeRole.CopyPropertiesFrom( groupTypeRoleState );
            }
            else
            {
                groupTypeRole.Order = GroupTypeRolesState.Any() ? GroupTypeRolesState.Max( a => a.Order ) + 1 : 0;
                groupTypeRole.GroupTypeId = hfGroupTypeId.ValueAsInt();
            }

            groupTypeRole.Name = tbRoleName.Text;
            groupTypeRole.Description = tbRoleDescription.Text;
            groupTypeRole.IsLeader = cbIsLeader.Checked;
            groupTypeRole.ReceiveRequirementsNotifications = cbReceiveRequirementsNotifications.Checked;
            groupTypeRole.CanView = cbCanView.Checked;
            groupTypeRole.CanEdit = cbCanEdit.Checked;
            groupTypeRole.CanManageMembers = cbCanManageMembers.Checked;
            groupTypeRole.MinCount = nbMinimumRequired.Text.AsIntegerOrNull();
            groupTypeRole.MaxCount = nbMaximumAllowed.Text.AsIntegerOrNull();
            groupTypeRole.LoadAttributes();

            Helper.GetEditValues( phGroupTypeRoleAttributes, groupTypeRole );

            // Controls will show warnings
            if ( !groupTypeRole.IsValid )
            {
                return;
            }

            if ( groupTypeRoleState != null )
            {
                GroupTypeRolesState.RemoveEntity( groupTypeRoleState.Guid );
            }

            GroupTypeRolesState.Add( groupTypeRole );

            BindGroupTypeRolesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupTypeRolesGrid()
        {
            SetGroupTypeRoleListOrder( GroupTypeRolesState );
            gGroupTypeRoles.DataSource = GroupTypeRolesState.OrderBy( a => a.Order ).ToList();
            gGroupTypeRoles.DataBind();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvAllowed control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvAllowed_ServerValidate( object source, System.Web.UI.WebControls.ServerValidateEventArgs args )
        {
            int? lowerValue = nbMinimumRequired.Text.AsIntegerOrNull();
            int? upperValue = nbMaximumAllowed.Text.AsIntegerOrNull();

            if ( lowerValue.HasValue && upperValue.HasValue && upperValue.Value < lowerValue.Value )
            {
                args.IsValid = false;
            }
            else
            {
                args.IsValid = true;
            }
        }

        #endregion

        #region Child GroupType Grid and Picker

        /// <summary>
        /// Handles the CheckedChanged event of the cbAllowAnyChildGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAllowAnyChildGroupType_CheckedChanged( object sender, EventArgs e )
        {
            rcwAllowedChildGroupTypes.Visible = !cbAllowAnyChildGroupType.Checked;
        }

        /// <summary>
        /// Handles the Add event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_Add( object sender, EventArgs e )
        {
            // populate dropdown with all grouptypes that aren't already childgroups
            var groupTypeList = new GroupTypeService( new RockContext() )
                .Queryable()
                .Where( t => !ChildGroupTypesList.Contains( t.Id ) )
                .OrderBy( t => t.Name )
                .ToList();

            if ( groupTypeList.Count == 0 )
            {
                modalAlert.Show( "There are not any other group types that can be added", ModalAlertType.Warning );
            }
            else
            {
                ddlChildGroupType.DataSource = groupTypeList;
                ddlChildGroupType.DataBind();
                ShowDialog( "ChildGroupTypes" );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_Delete( object sender, RowEventArgs e )
        {
            int childGroupTypeId = e.RowKeyId;
            ChildGroupTypesList.Remove( childGroupTypeId );
            BindChildGroupTypesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gChildGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildGroupTypes_GridRebind( object sender, EventArgs e )
        {
            BindChildGroupTypesGrid();
        }

        /// <summary>
        /// Binds the child group types grid.
        /// </summary>
        private void BindChildGroupTypesGrid()
        {
            var groupTypeList = new GroupTypeService( new RockContext() )
                .Queryable()
                .Where( t => ChildGroupTypesList.Contains( t.Id ) )
                .OrderBy( t => t.Order )
                .ToList();

            gChildGroupTypes.DataSource = groupTypeList;
            gChildGroupTypes.DataBind();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgChildGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgChildGroupType_SaveClick( object sender, EventArgs e )
        {
            ChildGroupTypesList.Add( ddlChildGroupType.SelectedValueAsId() ?? 0 );
            BindChildGroupTypesGrid();
            HideDialog();
        }

        #endregion

        #region ScheduleExclusion Grid and Picker

        /// <summary>
        /// Handles the Add event of the gScheduleExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScheduleExclusions_Add( object sender, EventArgs e )
        {
            hfScheduleExclusion.Value = Guid.NewGuid().ToString();
            drpScheduleExclusion.LowerValue = null;
            drpScheduleExclusion.UpperValue = null;
            ShowDialog( "ScheduleExclusion" );
        }

        /// <summary>
        /// Handles the Edit event of the gScheduleExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduleExclusions_Edit( object sender, RowEventArgs e )
        {
            Guid guid = e.RowKeyValue.ToString().AsGuid();
            if ( ScheduleExclusionDictionary.Keys.Contains( guid ) )
            {
                hfScheduleExclusion.Value = guid.ToString();
                drpScheduleExclusion.LowerValue = ScheduleExclusionDictionary[guid].Start;
                drpScheduleExclusion.UpperValue = ScheduleExclusionDictionary[guid].End;
                ShowDialog( "ScheduleExclusion" );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gScheduleExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduleExclusions_Delete( object sender, RowEventArgs e )
        {
            Guid guid = e.RowKeyValue.ToString().AsGuid();
            if ( ScheduleExclusionDictionary.Keys.Contains( guid ) )
            {
                ScheduleExclusionDictionary.Remove( guid );
            }

            BindScheduleExclusionsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gScheduleExclusions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScheduleExclusions_GridRebind( object sender, EventArgs e )
        {
            BindScheduleExclusionsGrid();
        }

        /// <summary>
        /// Binds the location types grid.
        /// </summary>
        private void BindScheduleExclusionsGrid()
        {
            gScheduleExclusions.DataSource = ScheduleExclusionDictionary.OrderBy( a => a.Value.Start ).ToList();
            gScheduleExclusions.DataBind();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgScheduleExclusion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgScheduleExclusion_SaveClick( object sender, EventArgs e )
        {
            Guid guid = hfScheduleExclusion.Value.AsGuid();
            if ( ScheduleExclusionDictionary.ContainsKey( guid ) )
            {
                ScheduleExclusionDictionary[guid].Start = drpScheduleExclusion.LowerValue;
                ScheduleExclusionDictionary[guid].End = drpScheduleExclusion.UpperValue;
            }
            else
            {
                ScheduleExclusionDictionary.Add( guid, new DateRange( drpScheduleExclusion.LowerValue, drpScheduleExclusion.UpperValue ) );
            }

            BindScheduleExclusionsGrid();
            HideDialog();
        }

        #endregion

        #region LocationType Grid and Picker

        /// <summary>
        /// Handles the Add event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_Add( object sender, EventArgs e )
        {
            DefinedValueService definedValueService = new DefinedValueService( new RockContext() );

            // populate dropdown with all locationtypes that aren't already locationtypes
            var qry = from dlt in definedValueService.GetByDefinedTypeGuid( new Guid( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE ) )
                      where !( from lt in LocationTypesDictionary.Keys
                               select lt ).Contains( dlt.Id )
                      select dlt;

            List<DefinedValue> list = qry.ToList();
            if ( list.Count == 0 )
            {
                modalAlert.Show( "There are not any location types defined.  Before you can add location types to a group type, you will first need to add them using Defined Type/Values", ModalAlertType.Warning );
            }
            else
            {
                ddlLocationType.DataSource = list;
                ddlLocationType.DataBind();
                ShowDialog( "LocationType" );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_Delete( object sender, RowEventArgs e )
        {
            int locationTypeId = e.RowKeyId;
            LocationTypesDictionary.Remove( locationTypeId );
            BindLocationTypesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLocationTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gLocationTypes_GridRebind( object sender, EventArgs e )
        {
            BindLocationTypesGrid();
        }

        /// <summary>
        /// Binds the location types grid.
        /// </summary>
        private void BindLocationTypesGrid()
        {
            gLocationTypes.DataSource = LocationTypesDictionary.OrderBy( a => a.Value ).ToList();
            gLocationTypes.DataBind();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgLocationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgLocationType_SaveClick( object sender, EventArgs e )
        {
            LocationTypesDictionary.Add( ddlLocationType.SelectedValue.AsInteger(), ddlLocationType.SelectedItem.Text );
            BindLocationTypesGrid();
            HideDialog();
        }

        #endregion

        #region GroupTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_Add( object sender, EventArgs e )
        {
            gGroupTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtGroupTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for group type " + tbName.Text );
            }
            else
            {
                attribute = GroupTypeAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for group type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            GroupTypeAttributesInheritedState.Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            GroupTypeAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtGroupTypeAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtGroupTypeAttributes.SetAttributeProperties( attribute, typeof( GroupType ) );

            ShowDialog( "GroupTypeAttributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( GroupTypeAttributesState, e.OldIndex, e.NewIndex );
            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupTypeAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            GroupTypeAttributesState.RemoveEntity( attributeGuid );

            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupTypeAttributesInherited control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributesInherited_GridRebind( object sender, EventArgs e )
        {
            BindGroupTypeAttributesInheritedGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupTypeAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( GroupTypeAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                // get the non-editable stuff from the state and put it back into the object...
                var attributeState = GroupTypeAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault();
                if ( attributeState != null )
                {
                    attribute.Order = attributeState.Order;
                    attribute.CreatedDateTime = attributeState.CreatedDateTime;
                    attribute.CreatedByPersonAliasId = attributeState.CreatedByPersonAliasId;
                    attribute.ForeignGuid = attributeState.ForeignGuid;
                    attribute.ForeignId = attributeState.ForeignId;
                    attribute.ForeignKey = attributeState.ForeignKey;
                }

                GroupTypeAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = GroupTypeAttributesState.Any() ? GroupTypeAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            GroupTypeAttributesState.Add( attribute );

            BindGroupTypeAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the group type attributes inherited grid.
        /// </summary>
        private void BindGroupTypeAttributesInheritedGrid()
        {
            gGroupTypeAttributesInherited.AddCssClass( "inherited-attribute-grid" );
            gGroupTypeAttributesInherited.DataSource = GroupTypeAttributesInheritedState;
            gGroupTypeAttributesInherited.DataBind();
            rcGroupTypeAttributesInherited.Visible = GroupTypeAttributesInheritedState.Any();
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupTypeAttributesGrid()
        {
            gGroupTypeAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( GroupTypeAttributesState );
            gGroupTypeAttributes.DataSource = GroupTypeAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gGroupTypeAttributes.DataBind();
        }

        #endregion

        #region GroupAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_Add( object sender, EventArgs e )
        {
            gGroupAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gGroupAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                if (hfGroupTypeId.Value.AsInteger() > 0)
                {
                    attribute.EntityTypeQualifierColumn = "GroupTypeId";
                    attribute.EntityTypeQualifierValue = hfGroupTypeId.Value;
                }

                edtGroupAttributes.ActionTitle = ActionTitle.Add( "attribute for groups of group type " + tbName.Text );
            }
            else
            {
                attribute = GroupAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupAttributes.ActionTitle = ActionTitle.Edit( "attribute for groups of group type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            GroupAttributesInheritedState.Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            GroupAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtGroupAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtGroupAttributes.SetAttributeProperties( attribute, typeof( Group ) );

            ShowDialog( "GroupAttributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gGroupAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( GroupAttributesState, e.OldIndex, e.NewIndex );
            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gGroupAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            GroupAttributesState.RemoveEntity( attributeGuid );

            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupAttributesInherited control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributesInherited_GridRebind( object sender, EventArgs e )
        {
            BindGroupAttributesInheritedGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupAttributes_GridRebind( object sender, EventArgs e )
        {
            BindGroupAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtGroupAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( GroupAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                // get the non-editable stuff from the state and put it back into the object...
                var attributeState = GroupAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault();
                if ( attributeState != null )
                {
                    attribute.Order = attributeState.Order;
                    attribute.CreatedDateTime = attributeState.CreatedDateTime;
                    attribute.CreatedByPersonAliasId = attributeState.CreatedByPersonAliasId;
                    attribute.ForeignGuid = attributeState.ForeignGuid;
                    attribute.ForeignId = attributeState.ForeignId;
                    attribute.ForeignKey = attributeState.ForeignKey;
                }

                GroupAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = GroupAttributesState.Any() ? GroupAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            GroupAttributesState.Add( attribute );

            BindGroupAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the group attributes inherited grid.
        /// </summary>
        private void BindGroupAttributesInheritedGrid()
        {
            gGroupAttributesInherited.AddCssClass( "inherited-attribute-grid" );
            gGroupAttributesInherited.DataSource = GroupAttributesInheritedState;
            gGroupAttributesInherited.DataBind();
            rcGroupAttributesInherited.Visible = GroupAttributesInheritedState.Any();
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupAttributesGrid()
        {
            gGroupAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( GroupAttributesState );
            gGroupAttributes.DataSource = GroupAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gGroupAttributes.DataBind();
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
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gGroupMemberAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Add( "attribute for members in groups of group type " + tbName.Text );
            }
            else
            {
                attribute = GroupMemberAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtGroupMemberAttributes.ActionTitle = ActionTitle.Edit( "attribute for members in groups of group type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            GroupMemberAttributesInheritedState.Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            GroupMemberAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtGroupMemberAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtGroupMemberAttributes.SetAttributeProperties( attribute, typeof( GroupMember ) );

            ShowDialog( "GroupMemberAttributes" );
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
        /// <exception cref="System.NotImplementedException"></exception>
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
                // get the non-editable stuff from the state and put it back into the object...
                var attributeState = GroupMemberAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault();
                if ( attributeState != null )
                {
                    attribute.Order = attributeState.Order;
                    attribute.CreatedDateTime = attributeState.CreatedDateTime;
                    attribute.CreatedByPersonAliasId = attributeState.CreatedByPersonAliasId;
                    attribute.ForeignGuid = attributeState.ForeignGuid;
                    attribute.ForeignId = attributeState.ForeignId;
                    attribute.ForeignKey = attributeState.ForeignKey;
                }

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
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindGroupMemberAttributesGrid()
        {
            gGroupMemberAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( GroupMemberAttributesState );
            gGroupMemberAttributes.DataSource = GroupMemberAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gGroupMemberAttributes.DataBind();
        }

        #endregion

        #region GroupTypeGroupRequirements Grid and Picker

        /// <summary>
        /// Handles the Add event of the gGroupTypeGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeGroupRequirements_Add( object sender, EventArgs e )
        {
            gGroupTypeGroupRequirements_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupTypeGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeGroupRequirements_Edit( object sender, RowEventArgs e )
        {
            Guid groupTypeGroupRequirementGuid = ( Guid ) e.RowKeyValue;
            gGroupTypeGroupRequirements_ShowEdit( groupTypeGroupRequirementGuid );
        }

        /// <summary>
        /// gs the group type group requirements show edit.
        /// </summary>
        /// <param name="groupTypeGroupRequirementGuid">The group type group requirement unique identifier.</param>
        protected void gGroupTypeGroupRequirements_ShowEdit( Guid groupTypeGroupRequirementGuid )
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

            var selectedGroupTypeGroupRequirement = this.GroupTypeGroupRequirementsState.FirstOrDefault( a => a.Guid == groupTypeGroupRequirementGuid );
            grpGroupRequirementGroupRole.GroupTypeId = hfGroupTypeId.Value.AsIntegerOrNull();
            if ( selectedGroupTypeGroupRequirement != null )
            {
                ddlGroupRequirementType.SelectedValue = selectedGroupTypeGroupRequirement.GroupRequirementTypeId.ToString();
                grpGroupRequirementGroupRole.GroupRoleId = selectedGroupTypeGroupRequirement.GroupRoleId;
                cbMembersMustMeetRequirementOnAdd.Checked = selectedGroupTypeGroupRequirement.MustMeetRequirementToAddMember;
            }
            else
            {
                ddlGroupRequirementType.SelectedIndex = 0;
                grpGroupRequirementGroupRole.GroupRoleId = null;
                cbMembersMustMeetRequirementOnAdd.Checked = false;
            }

            nbDuplicateGroupRequirement.Visible = false;

            hfGroupTypeGroupRequirementGuid.Value = groupTypeGroupRequirementGuid.ToString();

            ShowDialog( "GroupTypeGroupRequirements", true );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdGroupTypeGroupRequirement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdGroupTypeGroupRequirement_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            Guid groupTypeGroupRequirementGuid = hfGroupTypeGroupRequirementGuid.Value.AsGuid();

            var groupTypeGroupRequirement = this.GroupTypeGroupRequirementsState.FirstOrDefault( a => a.Guid == groupTypeGroupRequirementGuid );
            if ( groupTypeGroupRequirement == null )
            {
                groupTypeGroupRequirement = new GroupRequirement();
                groupTypeGroupRequirement.Guid = Guid.NewGuid();
                this.GroupTypeGroupRequirementsState.Add( groupTypeGroupRequirement );
            }

            groupTypeGroupRequirement.GroupRequirementTypeId = ddlGroupRequirementType.SelectedValue.AsInteger();
            groupTypeGroupRequirement.GroupRequirementType = new GroupRequirementTypeService( rockContext ).Get( groupTypeGroupRequirement.GroupRequirementTypeId );
            groupTypeGroupRequirement.GroupRoleId = grpGroupRequirementGroupRole.GroupRoleId;
            groupTypeGroupRequirement.MustMeetRequirementToAddMember = cbMembersMustMeetRequirementOnAdd.Checked;
            if ( groupTypeGroupRequirement.GroupRoleId.HasValue )
            {
                groupTypeGroupRequirement.GroupRole = new GroupTypeRoleService( rockContext ).Get( groupTypeGroupRequirement.GroupRoleId.Value );
            }
            else
            {
                groupTypeGroupRequirement.GroupRole = null;
            }

            // make sure we aren't adding a duplicate group requirement (same group requirement type and role)
            var duplicateGroupRequirement = this.GroupTypeGroupRequirementsState.Any( a =>
                a.GroupRequirementTypeId == groupTypeGroupRequirement.GroupRequirementTypeId
                && a.GroupRoleId == groupTypeGroupRequirement.GroupRoleId
                && a.Guid != groupTypeGroupRequirement.Guid );

            if ( duplicateGroupRequirement )
            {
                nbDuplicateGroupRequirement.Text = string.Format(
                    "This group type already has a group requirement of {0} {1}",
                    groupTypeGroupRequirement.GroupRequirementType.Name,
                    groupTypeGroupRequirement.GroupRoleId.HasValue ? "for group role " + groupTypeGroupRequirement.GroupRole.Name : string.Empty );
                nbDuplicateGroupRequirement.Visible = true;
                this.GroupTypeGroupRequirementsState.Remove( groupTypeGroupRequirement );
                return;
            }
            else
            {
                nbDuplicateGroupRequirement.Visible = false;
                BindGroupTypeGroupRequirementsGrid();
                HideDialog();
            }
        }

        /// <summary>
        /// Handles the Delete event of the gGroupTypeGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeGroupRequirements_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            GroupTypeGroupRequirementsState.RemoveEntity( rowGuid );

            BindGroupTypeGroupRequirementsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupTypeGroupRequirements control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGroupTypeGroupRequirements_GridRebind( object sender, EventArgs e )
        {
            BindGroupTypeGroupRequirementsGrid();
        }

        /// <summary>
        /// Binds the group type group requirements grid.
        /// </summary>
        private void BindGroupTypeGroupRequirementsGrid()
        {
            gGroupTypeGroupRequirements.AddCssClass( "group-requirements-grid" );
            gGroupTypeGroupRequirements.DataSource = GroupTypeGroupRequirementsState.OrderBy( a => a.GroupRequirementType.Name ).ToList();
            gGroupTypeGroupRequirements.DataBind();
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
            ddlTriggerFromStatus.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            ddlTriggerToStatus.BindToEnum<GroupMemberStatus>( false );
            ddlTriggerToStatus.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            ddlTriggerFromRole.DataSource = GroupTypeRolesState;
            ddlTriggerFromRole.DataBind();
            ddlTriggerFromRole.Items.Insert( 0, new ListItem( "Any", string.Empty ) );

            ddlTriggerToRole.DataSource = GroupTypeRolesState;
            ddlTriggerToRole.DataBind();
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
        /// Handles the CheckedChanged event of the cbEnableGroupHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEnableGroupHistory_CheckedChanged( object sender, EventArgs e )
        {
            if ( cbEnableGroupHistory.Checked == false )
            {
                // if toggling EnableGroupHistory from True to False, show a warning if Group/GroupMember History will be deleted when this group type is saved
                int groupTypeId = hfGroupTypeId.Value.AsInteger();
                var groupType = GroupTypeCache.Get( groupTypeId );
                if ( groupType != null && groupType.EnableGroupHistory )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        if ( new GroupHistoricalService( rockContext ).Queryable().Any( a => a.GroupTypeId == groupTypeId ) )
                        {
                            nbGroupHistoryWarning.Visible = true;
                        }
                        else if ( new GroupMemberHistoricalService( rockContext ).Queryable().Any( a => a.Group.GroupTypeId == groupTypeId ) )
                        {
                            nbGroupHistoryWarning.Visible = true;
                        }
                    }
                }
            }
            else
            {
                nbGroupHistoryWarning.Visible = false;
            }
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
                memberWorkflowTrigger.GroupTypeId = hfGroupTypeId.ValueAsInt();
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

            if ( qualifierParts.Length > 3 && !string.IsNullOrWhiteSpace( qualifierParts[3] ) )
            {
                Guid roleGuid = qualifierParts[3].AsGuid();
                var role = GroupTypeRolesState.FirstOrDefault( r => r.Guid.Equals( roleGuid ) );
                if ( role != null )
                {
                    qualiferText.Add( string.Format( " from role of <strong>{0}</strong>", role.Name ) );
                }
            }

            if ( qualifierParts.Length > 1 && !string.IsNullOrWhiteSpace( qualifierParts[1] ) )
            {
                Guid roleGuid = qualifierParts[1].AsGuid();
                var role = GroupTypeRolesState.FirstOrDefault( r => r.Guid.Equals( roleGuid ) );
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

            if ( qualifierParts.Length > 4 && qualifierParts[4].AsBoolean() )
            {
                qualiferText.Add( " for the first time" );
            }

            return triggerType.ConvertToString() + qualiferText.AsDelimited( " and " );
        }

        #endregion

        protected void cbEnableInactiveReason_CheckedChanged( object sender, EventArgs e )
        {
            cbRequireInactiveReason.Enabled = cbEnableInactiveReason.Checked;
        }

        #region RSVP Controls

        /// <summary>
        /// Handles the CheckedChanged event of the cbRsvp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cbRsvp_CheckedChanged( object sender, EventArgs e )
        {
            SetRsvpEnabledState( cbGroupRSVPEnabled.Checked );
        }

        /// <summary>
        /// Set the enabled state of the RSVP controls.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetRsvpEnabledState( bool isEnabled )
        {
            if ( cbGroupRSVPEnabled.Checked != isEnabled )
            {
                cbGroupRSVPEnabled.Checked = isEnabled;
            }

            pnlRsvpSettings.Visible = isEnabled;
        }

        #endregion
    }
}