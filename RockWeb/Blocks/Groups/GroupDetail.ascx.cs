﻿// <copyright>
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

    [GroupTypesField( "Group Types", "Select group types to show in this block.  Leave all unchecked to show all group types.", false, "", "", 0 )]
    [BooleanField( "Show Edit", "", true, "", 1 )]
    [BooleanField( "Limit to Security Role Groups", "", false, "", 2 )]
    [BooleanField( "Limit to Group Types that are shown in navigation", "", false, "", 3, "LimitToShowInNavigationGroupTypes" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The style of maps to use", false, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK, "", 4 )]
    [LinkedPage("Group Map Page", "The page to display detailed group map.")]
    public partial class GroupDetail : RockBlock, IDetailBlock
    {
        #region Constants

        private const string MEMBER_LOCATION_TAB_TITLE = "Member Location";
        private const string OTHER_LOCATION_TAB_TITLE = "Other Location";

        #endregion

        #region Fields

        private readonly List<string> _tabs = new List<string> { MEMBER_LOCATION_TAB_TITLE, OTHER_LOCATION_TAB_TITLE };

        private string LocationTypeTab
        {
            get
            {
                object currentProperty = ViewState["LocationTypeTab"];
                return currentProperty != null ? currentProperty.ToString() : MEMBER_LOCATION_TAB_TITLE;
            }

            set
            {
                ViewState["LocationTypeTab"] = value;
            }
        }

        #endregion

        #region Properties

        private List<GroupLocation> GroupLocationsState { get; set; }
        private List<InheritedAttribute> GroupMemberAttributesInheritedState { get; set; }
        private List<Attribute> GroupMemberAttributesState { get; set; }
        private bool AllowMultipleLocations { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

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

            AllowMultipleLocations = ViewState["AllowMultipleLocations"] as bool? ?? false;
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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Group ) ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupList );
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
                
                ShowDialog();
            }

            // Rebuild the attribute controls on postback based on group type
            if ( pnlDetails.Visible )
            {
                var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
                if ( group.GroupTypeId > 0 )
                {
                    ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, false );
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

            ViewState["GroupLocationsState"] = JsonConvert.SerializeObject( GroupLocationsState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesInheritedState"] = JsonConvert.SerializeObject( GroupMemberAttributesInheritedState, Formatting.None, jsonSetting );
            ViewState["GroupMemberAttributesState"] = JsonConvert.SerializeObject( GroupMemberAttributesState, Formatting.None, jsonSetting );
            ViewState["AllowMultipleLocations"] = AllowMultipleLocations;

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

            RockContext rockContext = new RockContext();

            GroupService groupService = new GroupService( rockContext );
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            if ( ( ddlGroupType.SelectedValueAsInt() ?? 0 ) == 0 )
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
                group = groupService.Queryable( "GroupLocations.Schedules" ).Where( g => g.Id == groupId ).FirstOrDefault();
                wasSecurityRole = group.IsSecurityRole;

                var selectedLocations = GroupLocationsState.Select( l => l.Guid );
                foreach ( var groupLocation in group.GroupLocations.Where( l => !selectedLocations.Contains( l.Guid ) ).ToList() )
                {
                    group.GroupLocations.Remove( groupLocation );
                    groupLocationService.Delete( groupLocation );
                }
            }

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

            group.Name = tbName.Text;
            group.Description = tbDescription.Text;
            group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
            group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
            group.ParentGroupId = gpParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( gpParentGroup.SelectedValue );
            group.IsSecurityRole = cbIsSecurityRole.Checked;
            group.IsActive = cbIsActive.Checked;

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
                    NavigateToParentPage();
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
            var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
            if ( group.GroupTypeId > 0 )
            {
                ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, true );
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
                ddlGroupType.SelectedValue = selectedGroupTypeId.ToString();
            }
            else
            {
                ddlGroupType.SelectedValue = null;
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
            ShowReadonlyDetails( GetGroup( hfGroupId.Value.AsInteger() ) );
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

            RockContext rockContext = null;

            if ( !groupId.Equals( 0 ) )
            {
                group = GetGroup( groupId, rockContext );
            }

            if ( group == null )
            {
                group = new Group { Id = 0, IsActive = true, ParentGroupId = parentGroupId, Name = "" };
                wpGeneral.Expanded = true;

                if ( parentGroupId.HasValue )
                {
                    rockContext = rockContext ?? new RockContext();

                    // Set the new group's parent group (so security checks work)
                    var parentGroup = new GroupService( rockContext ).Get(parentGroupId.Value);
                    if ( parentGroup != null )
                    {
                        // Start by setting the group type to the same as the parent
                        group.ParentGroup = parentGroup;
                        group.GroupTypeId = parentGroup.GroupTypeId;
                        group.GroupType = parentGroup.GroupType;

                        if ( !editAllowed )
                        {
                            // If user is not allowed to edit this new group (with paren't group type),
                            // check to see if they'd be allowed to edit any other of the allowed
                            // child group types
                            editAllowed = group.IsAuthorized( Authorization.EDIT, CurrentPerson );
                            if ( !editAllowed )
                            {
                                foreach( var groupType in GetAllowedGroupTypes( parentGroup, rockContext )
                                    .Where( g => g.Id != parentGroup.GroupTypeId ) )
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
            }

            viewAllowed = editAllowed || group.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = IsUserAuthorized( Authorization.EDIT ) || group.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            hfGroupId.Value = group.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed  )
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
                    int curCount = 0;
                    if ( group.Members != null )
                    {
                        curCount = group.Members
                            .Where( m => m.GroupRoleId == role.Id && m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Count();
                    }

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

            var rockContext = new RockContext();

            var groupService = new GroupService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns();

            gpParentGroup.SetValue( group.ParentGroup ?? groupService.Get( group.ParentGroupId ?? 0 ) );

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
                ddlGroupType.SetValue( group.GroupTypeId );
                var groupType = GroupTypeCache.Read( group.GroupTypeId, rockContext );
                lGroupType.Text = groupType != null ? groupType.Name : "";
            }

            ddlCampus.SetValue( group.CampusId );

            //GroupLocationsState = groupLocations;
            GroupLocationsState = group.GroupLocations.ToList();

            ShowGroupTypeEditDetails( GroupTypeCache.Read( group.GroupTypeId ), group, true );

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

                if ( groupType != null && groupType.LocationSelectionMode != GroupLocationPickerMode.None )
                {
                    wpLocations.Visible = true;
                    BindLocationsGrid();
                }
                else
                {
                    wpLocations.Visible = false;
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

            lGroupDescription.Text = group.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( group.ParentGroup != null )
            {
                descriptionList.Add( "Parent Group", group.ParentGroup.Name );
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

            var attributes = new List<Rock.Web.Cache.AttributeCache>();

            // Get the attributes inherited from group type
            GroupType groupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            groupType.LoadAttributes();
            attributes = groupType.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();

            // Combine with the group attributes
            group.LoadAttributes();
            attributes.AddRange( group.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) );

            // display attribute values
            var attributeCategories = Helper.GetAttributeCategories( attributes );
            Rock.Attribute.Helper.AddDisplayControls( group, attributeCategories, phAttributes, null, false );

            var pageParams = new Dictionary<string, string>();
            pageParams.Add("GroupId", group.Id.ToString());
            string groupMapUrl = LinkedPageUrl("GroupMapPage", pageParams);

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

        private IQueryable<GroupType> GetAllowedGroupTypes ( Group parentGroup, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            var groupTypeQry = groupTypeService.Queryable();

            // limit GroupType selection to what Block Attributes allow
            List<Guid> groupTypeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( groupTypeGuids.Count > 0 )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeGuids.Contains( a.Guid ) );
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

        #endregion
    }
}