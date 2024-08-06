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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List" )]
    [Category( "Groups" )]
    [Description( "Lists all groups for the configured group types or all groups for the specified person context. Query string parameters: <ul><li>GroupTypeId - Filters to a specific group type.</li></ui>" )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]

    [BooleanField(
        "Limit to Security Role Groups",
        Key = AttributeKey.LimittoSecurityRoleGroups,
        Description = "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?",
        DefaultBooleanValue = false,
        Order = 2 )]

    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 3 )]
    [BooleanField( "Display Group Path", "Should the Group path be displayed?", false, "", 4 )]
    [BooleanField( "Display Group Type Column", "Should the Group Type column be displayed?", true, "", 5 )]
    [BooleanField( "Display Description Column", "Should the Description column be displayed?", true, "", 6 )]
    [BooleanField( "Display Active Status Column", "Should the Active Status column be displayed?", false, "", 7 )]
    [BooleanField( "Display Member Count Column", "Should the Member Count column be displayed? Does not affect lists with a person context.", true, "", 8 )]
    [BooleanField( "Display System Column", "Should the System column be displayed?", true, "", 9 )]
    [BooleanField( "Display Security Column", "Should the Security column be displayed?", false, "", 10 )]
    [BooleanField( "Display Filter", "Should filter be displayed to allow filtering by group type?", false, "", 11 )]
    [CustomDropdownListField( "Limit to Active Status", "Select which groups (and groupmembers) to show, based on active status. Select [All] to filter by any status. Selecting Active will not show inactive/archived groups/groupmembers.", "all^[All], active^Active, inactive^Inactive", false, "all", Order = 12, Key = AttributeKey.LimittoActiveStatus )]
    [TextField( "Set Panel Title", "The title to display in the panel header. Leave empty to have the title be set automatically based on the group type or block name.", required: false, order: 13 )]
    [TextField( "Set Panel Icon", "The icon to display in the panel header. Leave empty to have the icon be set automatically based on the group type or default icon.", required: false, order: 14 )]
    [BooleanField( "Allow Add", "Should block support adding new group?", true, "", 15 )]
    [CustomDropdownListField( "Group Picker Type",
        description: "Used to control which kind of picker is used when adding a person to a group.",
        listSource: "GroupPicker^Group Picker, Dropdown^Drop-down",
        IsRequired = false,
        DefaultValue = "Dropdown",
        Category = "Add Group",
        Order = 16,
        Key = AttributeKey.GroupPickerType )]
    [GroupField( "Root Group (for Add Group)",
        Description = "Select the root group to use as a starting point for the tree view when using the \"Group Picker\" Group Picker Type.",
        IsRequired = false,
        Category = "Add Group",
        Order = 17,
        Key = AttributeKey.RootGroup )]
    [ContextAware]
    [Rock.SystemGuid.BlockTypeGuid( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A" )]
    public partial class GroupList : RockBlock, ICustomGridColumns
    {
        private int _groupTypesCount = 0;
        private bool _showGroupPath = false;

        private HashSet<int> _groupsWithGroupHistory = null;
        private GridListGridMode _groupListGridMode = GridListGridMode.GroupList;

        public enum GridListGridMode
        {
            // Block has a Context of Person, so the grid is a list of groups that the person is a member of
            GroupsPersonMemberOf = 0,

            // Block doesn't have a context of person, so it is just a normal list of groups
            GroupList = 1
        }

        public GridListGridMode GroupListGridMode
        {
            get
            {
                return _groupListGridMode;
            }
            set
            {
                _groupListGridMode = value;
            }
        }

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string GroupPickerType = "GroupPickerType";
            public const string RootGroup = "RootGroup";
            public const string LimittoSecurityRoleGroups = "LimittoSecurityRoleGroups";
            public const string LimittoActiveStatus = "LimittoActiveStatus";
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ApplyBlockSettings();

            modalDetails.SaveClick += modalDetails_SaveClick;

            this.BlockUpdated += GroupList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupList );

            SecurityField securityField = gGroups.Columns.OfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Group ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            gfSettings.Visible = GetAttributeValue( "DisplayFilter" ).AsBooleanOrNull() ?? false;
            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            // only show the user active filter if the block setting doesn't already restrict it
            ddlActiveFilter.Visible = GetAttributeValue( "LimittoActiveStatus" ) == "all";

            gGroups.DataKeyNames = new string[] { "Id" };
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;
            gGroups.ExportSource = ExcelExportSource.DataSource;
            gGroups.ShowConfirmDeleteDialog = false;

            // set up Grid based on Block Settings and Context
            bool showDescriptionColumn = GetAttributeValue( "DisplayDescriptionColumn" ).AsBoolean();
            bool showActiveStatusColumn = GetAttributeValue( "DisplayActiveStatusColumn" ).AsBoolean();
            bool showSystemColumn = GetAttributeValue( "DisplaySystemColumn" ).AsBoolean();
            bool showSecurityColumn = GetAttributeValue( "DisplaySecurityColumn" ).AsBoolean();

            if ( !showDescriptionColumn )
            {
                gGroups.TooltipField = "Description";
            }

            _showGroupPath = GetAttributeValue( "DisplayGroupPath" ).AsBoolean();

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["Name"].Visible = !_showGroupPath;

            // The GroupPathName field is the RockTemplateField that has a headertext of "Name"
            var groupPathNameField = gGroups.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.HeaderText == "Name" );
            groupPathNameField.Visible = _showGroupPath;

            boundFields["GroupTypeName"].Visible = GetAttributeValue( "DisplayGroupTypeColumn" ).AsBoolean();
            boundFields["Description"].Visible = showDescriptionColumn;

            Dictionary<string, BoolField> boolFields = gGroups.Columns.OfType<BoolField>().ToDictionary( a => a.DataField );
            boolFields["IsActive"].Visible = showActiveStatusColumn;
            boolFields["IsSystem"].Visible = showSystemColumn;

            var securityField = gGroups.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.Visible = showSecurityColumn;
            }

            int personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
            bool allowAdd = GetAttributeValue( "AllowAdd" ).AsBooleanOrNull() ?? true;

            if ( ContextTypesRequired.Any( a => a.Id == personEntityTypeId ) )
            {
                // Grid is in 'Groups that Person is member of' mode
                GroupListGridMode = GridListGridMode.GroupsPersonMemberOf;
            }
            else
            {
                GroupListGridMode = GridListGridMode.GroupList;
            }

            if ( GroupListGridMode == GridListGridMode.GroupsPersonMemberOf )
            {
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    boundFields["GroupRole"].Visible = true;
                    boundFields["DateAdded"].Visible = true;
                    boundFields["MemberCount"].Visible = false;
                    gGroups.IsDeleteEnabled = true;
                    gGroups.Actions.ShowAdd = allowAdd;
                    gGroups.HideDeleteButtonForIsSystem = false;
                }

                gGroups.DataKeyNames = new string[] { "GroupMemberId" };
            }
            else
            {
                // Grid is in normal 'Group List' mode
                bool canEdit = IsUserAuthorized( Authorization.EDIT );
                gGroups.Actions.ShowAdd = canEdit && allowAdd;
                gGroups.IsDeleteEnabled = canEdit;
                gGroups.DataKeyNames = new string[] { "Id" };

                boundFields["GroupRole"].Visible = false;
                boundFields["DateAdded"].Visible = false;
                boundFields["MemberCount"].Visible = GetAttributeValue( "DisplayMemberCountColumn" ).AsBoolean();
            }

            SetPanelTitleAndIcon();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var groupInfo = ( GroupListRowInfo ) e.Row.DataItem;

                // Show inactive entries in a lighter font.
                if ( !groupInfo.IsActive || groupInfo.IsArchived )
                {
                    e.Row.AddCssClass( "is-inactive" );
                }

                if ( groupInfo.IsSecurityRole && this.GroupListGridMode == GridListGridMode.GroupList )
                {
                    var lElevatedSecurityLevel = e.Row.FindControl( "lElevatedSecurityLevel" ) as Literal;
                    if ( groupInfo.ElevatedSecurityLevel >= ElevatedSecurityLevel.High )
                    {
                        lElevatedSecurityLevel.Visible = true;
                        string cssClass;
                        if ( groupInfo.ElevatedSecurityLevel == ElevatedSecurityLevel.Extreme )
                        {
                            cssClass = "label label-danger";
                        }
                        else
                        {
                            cssClass = "label label-warning";
                        }

                        lElevatedSecurityLevel.Text = $"<span class='{cssClass}'>Security Level: {groupInfo.ElevatedSecurityLevel.ConvertToString( true )}</span>";
                    }
                    else
                    {
                        lElevatedSecurityLevel.Visible = false;
                    }
                }

                var deleteOrArchiveField = gGroups.ColumnsOfType<DeleteField>().FirstOrDefault();
                if ( deleteOrArchiveField != null && deleteOrArchiveField.Visible )
                {
                    var deleteFieldColumnIndex = gGroups.GetColumnIndex( deleteOrArchiveField );
                    var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                    if ( deleteButton != null )
                    {
                        var buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();

                        deleteButton.Enabled = groupInfo.IsAuthorizedToEditOrManageMembers;

                        if ( groupInfo.IsSynced )
                        {
                            deleteButton.Enabled = false;
                            buttonIcon.Attributes["class"] = "fa fa-exchange";

                            deleteButton.ToolTip = string.Format( "Managed by group sync for role \"{0}\".", groupInfo.GroupRole );
                        }
                        else if ( groupInfo.GroupType.EnableGroupHistory && _groupsWithGroupHistory.Contains( groupInfo.Id ) )
                        {
                            buttonIcon.Attributes["class"] = "fa fa-archive";
                            deleteButton.AddCssClass( "btn-danger" );
                            deleteButton.ToolTip = "Archive";
                            e.Row.AddCssClass( "js-has-grouphistory" );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the GroupList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void GroupList_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroup control to populate the ddlGroupRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlGroupRole.Items.Clear();
            int? groupId = ddlGroup.SelectedValue.AsIntegerOrNull();
            if ( groupId == null )
            {
                return;
            }

            BindGroupRoleDropDown( groupId );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroup control to populate the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            ddlGroupRole.Items.Clear();
            if ( gpGroup.GroupId == null )
            {
                return;
            }

            BindGroupRoleDropDown( gpGroup.GroupId );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SetFilterPreference( "Group Type", gtpGroupType.SelectedValue );

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SetFilterPreference( "Active Status", string.Empty );
            }
            else
            {
                gfSettings.SetFilterPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            gfSettings.SetFilterPreference( "Group Type Purpose", dvpGroupTypePurpose.SelectedValue );

            BindGrid();
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
                case "Group Type":

                    int id = e.Value.AsInteger();

                    var groupType = GroupTypeCache.Get( id );
                    if ( groupType != null )
                    {
                        e.Value = groupType.Name;
                    }

                    break;

                case "Active Status":

                    // if the ActiveFilter control is hidden (because there is a block setting that overrides it), don't filter by Active Status
                    if ( !ddlActiveFilter.Visible )
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "Group Type Purpose":
                    var groupTypePurposeTypeValueId = e.Value.AsIntegerOrNull();
                    if ( groupTypePurposeTypeValueId.HasValue )
                    {
                        var groupTypePurpose = DefinedValueCache.Get( groupTypePurposeTypeValueId.Value );
                        e.Value = groupTypePurpose != null ? groupTypePurpose.ToString() : string.Empty;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroups_Add( object sender, EventArgs e )
        {
            if ( GroupListGridMode == GridListGridMode.GroupsPersonMemberOf )
            {
                // Grid is in 'Groups that Person is member of' mode
                BindModelDropDown();
                modalDetails.Show();
            }
            else
            {
                NavigateToLinkedPage( "DetailPage", "GroupId", 0 );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Edit( object sender, RowEventArgs e )
        {
            int groupId;
            if ( gGroups.DataKeyNames[0] == "GroupMemberId" )
            {
                int groupMemberId = e.RowKeyId;
                groupId = new GroupMemberService( new RockContext() ).GetSelect( groupMemberId, a => a.GroupId );
            }
            else
            {
                groupId = e.RowKeyId;
            }

            NavigateToLinkedPage( "DetailPage", "GroupId", groupId );
        }

        /// <summary>
        /// Handles the Click event of the delete/archive button in the grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_DeleteOrArchive( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = null;
            GroupMember groupMember = null;
            if ( GroupListGridMode == GridListGridMode.GroupsPersonMemberOf )
            {
                // the DataKey Id of the grid is GroupMemberId
                groupMember = groupMemberService.Get( e.RowKeyId );
                if ( groupMember != null )
                {
                    group = groupMember.Group;
                }
            }
            else
            {
                // the DataKey Id of the grid is GroupId
                group = groupService.Get( e.RowKeyId );
            }

            if ( group != null )
            {
                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );

                if ( GroupListGridMode == GridListGridMode.GroupsPersonMemberOf )
                {
                    // Grid is in 'Groups that Person is member of' mode
                    GroupMemberHistoricalService groupMemberHistoricalService = new GroupMemberHistoricalService( rockContext );

                    bool archive = false;
                    if ( group.GroupType.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupMemberId == groupMember.Id ) )
                    {
                        if ( !( group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson ) ) )
                        {
                            mdGridWarning.Show( "You are not authorized to archive members from this group", ModalAlertType.Information );
                            return;
                        }

                        // if the group has GroupHistory enabled, and this group member has group member history snapshots, they were prompted to Archive
                        archive = true;
                    }
                    else
                    {
                        if ( !( group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson ) ) )
                        {
                            mdGridWarning.Show( "You are not authorized to delete members from this group", ModalAlertType.Information );
                            return;
                        }

                        string errorMessage;
                        if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }
                    }

                    int groupId = groupMember.GroupId;

                    if ( archive )
                    {
                        // NOTE: Delete will AutoArchive, but since we know that we need to archive, we can call .Archive directly 
                        groupMemberService.Archive( groupMember, this.CurrentPersonAliasId, true );
                    }
                    else
                    {
                        groupMemberService.Delete( groupMember, true );
                    }

                    rockContext.SaveChanges();
                }
                else
                {
                    // Grid is in 'Group List' mode
                    bool archive = false;
                    var groupMemberHistoricalService = new GroupHistoricalService( rockContext );
                    if ( group.GroupType.EnableGroupHistory == true && groupMemberHistoricalService.Queryable().Any( a => a.GroupId == group.Id ) )
                    {
                        // if the group has GroupHistory enabled and has history snapshots, and they were prompted to Archive
                        archive = true;
                    }

                    if ( archive )
                    {
                        if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                        {
                            mdGridWarning.Show( "You are not authorized to archive this group", ModalAlertType.Information );
                            return;
                        }

                        // NOTE: groupService.Delete will automatically Archive instead Delete if this Group has GroupHistory enabled, but since this block has UI logic for Archive vs Delete, we can do a direct Archive
                        groupService.Archive( group, this.CurrentPersonAliasId, true );
                    }
                    else
                    {
                        if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                        {
                            mdGridWarning.Show( "You are not authorized to delete this group", ModalAlertType.Information );
                            return;
                        }

                        string errorMessage;
                        if ( !groupService.CanDelete( group, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        groupService.Delete( group, true );
                    }
                }

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Clear();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroups_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void modalDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            Group group = null;
            if ( GetAttributeValue( AttributeKey.GroupPickerType ) == "GroupPicker" )
            {
                if ( gpGroup.GroupId.HasValue )
                {
                    group = groupService.Get( gpGroup.GroupId.Value );
                }
            }
            else
            {
                group = groupService.Get( ddlGroup.SelectedValue.AsInteger() );
            }

            if ( group == null )
            {
                nbModalDetailsMessage.Title = "Please select a Group";
                nbModalDetailsMessage.Visible = true;
                return;
            }

            var roleId = ddlGroupRole.SelectedValue.AsIntegerOrNull();
            if ( roleId == null )
            {
                nbModalDetailsMessage.Title = "Please select a role";
                nbModalDetailsMessage.Visible = true;
                return;
            }

            var personContext = ContextEntity<Person>();
            var groupMemberService = new GroupMemberService( rockContext );

            if ( groupMemberService.Queryable().Any( a => a.PersonId == personContext.Id && a.GroupId == group.Id && a.GroupRoleId == roleId ) )
            {
                nbModalDetailsMessage.Title = "Already added to the selected Group & Role";
                nbModalDetailsMessage.Visible = true;
                return;
            }

            if ( !( group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) || group.IsAuthorized( Authorization.MANAGE_MEMBERS, this.CurrentPerson ) ) )
            {
                // shouldn't happen because GroupList is limited to EDIT and MANAGE_MEMBERs, but just in case
                nbModalDetailsMessage.Title = "You are not authorized to add members to this group";
                nbModalDetailsMessage.Visible = true;
                return;
            }

            GroupMember groupMember = new GroupMember { Id = 0 };
            groupMember.GroupId = group.Id;
            groupMember.PersonId = personContext.Id;
            groupMember.GroupRoleId = ddlGroupRole.SelectedValue.AsInteger();
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            // Check if the group member is valid. This includes checking for met group requirements.
            cvGroupMember.IsValid = groupMember.IsValidGroupMember( rockContext );

            if ( !cvGroupMember.IsValid )
            {
                cvGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return;
            }

            groupMemberService.Add( groupMember );
            rockContext.SaveChanges();

            // Reload the page so that other blocks will know about any data that changed as a result of a new member added to a group.
            // On the PersonProfile page, this will help update the UserProtectionProfile label just in case it was changed.
            this.NavigateToCurrentPageReference();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var groupTypeIds = GetAvailableGroupTypes();

            if ( groupTypeIds.Count() == 1 )
            {
                // if this block only permits one GroupType, there is no reason to show the GroupType filter.  So hide it
                gtpGroupType.Visible = false;
                gtpGroupType.SelectedValue = null;
            }
            else
            {
                gtpGroupType.Visible = true;
                gtpGroupType.GroupTypes = new GroupTypeService( new RockContext() ).Queryable()
                    .Where( g => groupTypeIds.Contains( g.Id ) ).ToList();

                gtpGroupType.SelectedValue = gfSettings.GetFilterPreference( "Group Type" );
            }

            dvpGroupTypePurpose.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() ).Id;
            dvpGroupTypePurpose.SetValue( gfSettings.GetFilterPreference( "Group Type Purpose" ) );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetFilterPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Find all the Group Types
            var groupTypeIds = GetAvailableGroupTypes();

            if ( GetAttributeValue( "DisplayFilter" ).AsBooleanOrNull() ?? false )
            {
                int? groupTypeFilter = gfSettings.GetFilterPreference( "Group Type" ).AsIntegerOrNull();
                if ( groupTypeFilter.HasValue )
                {
                    groupTypeIds = groupTypeIds.Where( g => g == groupTypeFilter.Value ).ToList();
                }
            }

            // filter to a specific group type if provided in the query string
            if ( !string.IsNullOrWhiteSpace( RockPage.PageParameter( "GroupTypeId" ) ) )
            {
                int? groupTypeId = RockPage.PageParameter( "GroupTypeId" ).AsIntegerOrNull();

                if ( groupTypeId.HasValue )
                {
                    groupTypeIds.Clear();
                    groupTypeIds.Add( groupTypeId.Value );
                }
            }

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            SortProperty sortProperty = gGroups.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "Name", SortDirection.Ascending ) );
            }

            bool onlySecurityGroups = GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean();
            var lElevatedSecurityLevelField = gGroups.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lElevatedSecurityLevel" ).FirstOrDefault();
            if ( lElevatedSecurityLevelField != null )
            {
                lElevatedSecurityLevelField.Visible = onlySecurityGroups && GroupListGridMode == GridListGridMode.GroupList;
            }

            var qryGroups = groupService.AsNoFilter()
                .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

            string limitToActiveStatus = GetAttributeValue( AttributeKey.LimittoActiveStatus );

            bool showActive = true;
            bool showInactive = true;

            if ( limitToActiveStatus == "all" && gfSettings.Visible )
            {
                // Filter by active/inactive unless the block settings restrict it
                if ( ddlActiveFilter.SelectedIndex > -1 )
                {
                    switch ( ddlActiveFilter.SelectedValue )
                    {
                        case "active":
                            showInactive = false;
                            break;
                        case "inactive":
                            showActive = false;
                            break;
                    }
                }
            }
            else if ( limitToActiveStatus != "all" )
            {
                // filter by the block setting for Active Status
                if ( limitToActiveStatus == "active" )
                {
                    showInactive = false;
                }
                else
                {
                    showActive = false;
                }
            }

            var groupTypePurposeValue = gfSettings.GetFilterPreference( "Group Type Purpose" ).AsIntegerOrNull();

            var groupList = new List<GroupListRowInfo>();

            if ( GroupListGridMode == GridListGridMode.GroupsPersonMemberOf )
            {
                // Grid is in 'Groups that Person is member of' mode
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    // limit to Groups that the person is a member of
                    var qry = new GroupMemberService( rockContext ).Queryable( true, true )
                        .Where( m => m.PersonId == personContext.Id )
                        .Join( qryGroups, gm => gm.GroupId, g => g.Id, ( gm, g ) => new { Group = g, GroupMember = gm } );

                    // Filter by Active Status of Group and Group Membership.
                    if ( showActive && !showInactive )
                    {
                        // Show only active Groups and active Memberships.
                        qry = qry.Where( gmg => gmg.Group.IsActive && !gmg.Group.IsArchived && gmg.GroupMember.GroupMemberStatus == GroupMemberStatus.Active && !gmg.GroupMember.IsArchived );
                    }
                    else if ( !showActive )
                    {
                        // Show only inactive Groups or inactive Memberships.
                        qry = qry.Where( gmg => !gmg.Group.IsActive || gmg.Group.IsArchived || gmg.GroupMember.IsArchived || gmg.GroupMember.GroupMemberStatus == GroupMemberStatus.Inactive );
                    }

                    if ( groupTypePurposeValue.HasValue && gfSettings.Visible )
                    {
                        qry = qry.Where( t => t.Group.GroupType.GroupTypePurposeValueId == groupTypePurposeValue );
                    }

                    // load with Groups where the current person has GroupMemberHistory for
                    _groupsWithGroupHistory = new HashSet<int>( new GroupMemberHistoricalService( rockContext ).Queryable().Where( a => qry.Any( x => x.GroupMember.Id == a.GroupMemberId ) ).Select( a => a.GroupId ).ToList() );

                    groupList = qry
                        .AsEnumerable()
                        .Where( gm => gm.Group.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                        .Select( m => new GroupListRowInfo
                        {
                            Id = m.Group.Id,
                            GroupMemberId = m.GroupMember.Id,
                            Path = string.Empty,
                            Name = m.Group.Name,
                            GroupType = GroupTypeCache.Get( m.Group.GroupTypeId ),
                            GroupOrder = m.Group.Order,
                            Description = m.Group.Description,
                            IsSystem = m.Group.IsSystem,
                            IsAuthorizedToEditOrManageMembers = m.Group.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) || m.Group.IsAuthorized( Rock.Security.Authorization.MANAGE_MEMBERS, this.CurrentPerson ),
                            GroupRole = m.GroupMember.GroupRole.Name,
                            ElevatedSecurityLevel = m.GroupMember.Group.ElevatedSecurityLevel,
                            IsSecurityRole = m.GroupMember.Group.IsSecurityRole,
                            DateAdded = m.GroupMember.DateTimeAdded ?? m.GroupMember.CreatedDateTime,
                            IsActive = m.Group.IsActive && ( m.GroupMember.GroupMemberStatus == GroupMemberStatus.Active ),
                            IsArchived = m.Group.IsArchived || m.GroupMember.IsArchived,
                            IsActiveOrder = m.Group.IsActive && m.GroupMember.GroupMemberStatus == GroupMemberStatus.Active ? 1 : 2,
                            IsSynced = m.Group.GroupSyncs.Where( s => s.GroupTypeRoleId == m.GroupMember.GroupRoleId ).Any(),
                            MemberCount = 0
                        } )
                        .AsQueryable()
                        .Sort( sortProperty )
                        .ToList();
                }
            }
            else
            {
                // Grid is in normal 'Group List' mode
                var roleGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                int roleGroupTypeId = roleGroupType != null ? roleGroupType.Id : 0;
                bool useRolePrefix = onlySecurityGroups || groupTypeIds.Contains( roleGroupTypeId );

                if ( !showInactive )
                {
                    qryGroups = qryGroups.Where( x => x.IsActive );
                }
                else if ( !showActive )
                {
                    qryGroups = qryGroups.Where( x => !x.IsActive );
                }

                if ( groupTypePurposeValue.HasValue && gfSettings.Visible )
                {
                    qryGroups = qryGroups.Where( t => t.GroupType.GroupTypePurposeValueId == groupTypePurposeValue );
                }

                // load with groups that have Group History
                _groupsWithGroupHistory = new HashSet<int>( new GroupHistoricalService( rockContext ).Queryable().Where( a => qryGroups.Any( g => g.Id == a.GroupId ) ).Select( a => a.GroupId ).ToList() );

                var groupMemberService = new GroupMemberService( rockContext );
                var groupSyncService = new GroupSyncService( rockContext );

                groupList = qryGroups
                    .AsEnumerable()
                    .Where( g => g.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    .Select( g => new GroupListRowInfo
                    {
                        Id = g.Id,
                        Path = string.Empty,
                        Name = ( ( useRolePrefix && g.GroupType.Id != roleGroupTypeId ) ? "GROUP - " : string.Empty ) + g.Name,
                        GroupType = GroupTypeCache.Get( g.GroupTypeId ),
                        GroupOrder = g.Order,
                        Description = g.Description,
                        IsSystem = g.IsSystem,
                        IsActive = g.IsActive,
                        IsArchived = g.IsArchived,
                        IsActiveOrder = g.IsActive ? 1 : 2,
                        GroupRole = string.Empty,
                        ElevatedSecurityLevel = g.ElevatedSecurityLevel,
                        IsSecurityRole = g.IsSecurityRole,
                        DateAdded = DateTime.MinValue,
                        IsSynced = groupSyncService.Queryable().Any( gs => gs.GroupId == g.Id ),
                        MemberCount = groupMemberService.Queryable().Count( gm => gm.GroupId == g.Id )
                    } )
                    .AsQueryable()
                    .Sort( sortProperty )
                    .ToList();
            }

            if ( _showGroupPath )
            {
                foreach ( var groupRow in groupList )
                {
                    groupRow.Path = groupService.GroupAncestorPathName( groupRow.Id );
                }
            }

            gGroups.DataSource = groupList;
            gGroups.EntityTypeId = GroupListGridMode == GridListGridMode.GroupList ?
                EntityTypeCache.Get<Group>().Id :
                EntityTypeCache.Get<GroupMember>().Id;
            gGroups.DataBind();

            // hide the group type column if there's only one type; must come after DataBind()
            if ( _groupTypesCount == 1 )
            {
                var groupTypeColumn = this.gGroups.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "GroupTypeName" );
                groupTypeColumn.Visible = false;
            }
        }

        /// <summary>
        /// Gets the available group types.
        /// </summary>
        /// <returns></returns>
        private List<int> GetAvailableGroupTypes()
        {
            var groupTypeIds = new List<int>();

            var groupTypeService = new GroupTypeService( new RockContext() );
            var qry = groupTypeService.Queryable().Where( t => t.ShowInGroupList );

            /*
                04/20/2022 - KA

                The GroupType filtering should use an if/else clause with the IncludeGroupTypes taking priority over the ExcludeGroupTypes
                (refer to ReminderService.GetReminderEntityTypesByPerson for how it should work). Thus if any GroupTypes are selected as 
                part of the IncludeGroupTypes they should not be excluded even if they are selected as part of the ExcludeGroupTypes. This
                implementation has been left as it is because it would be too late/risky to change the behavior now since people/admins
                have already configured it and it is working the way it is working now.
            */

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                _groupTypesCount = includeGroupTypeGuids.Count;
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Guid ) );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue( "ExcludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( excludeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => !excludeGroupTypeGuids.Contains( t.Guid ) );
            }

            foreach ( int groupTypeId in qry.Select( t => t.Id ) )
            {
                var groupType = GroupTypeCache.Get( groupTypeId );
                if ( groupType != null && groupType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    groupTypeIds.Add( groupTypeId );
                }
            }

            groupTypeIds = qry.Select( t => t.Id ).ToList();

            return groupTypeIds;
        }

        /// <summary>
        /// Sets the panel title and icon.
        /// </summary>
        private void SetPanelTitleAndIcon()
        {
            List<int> groupTypeIds = GetAvailableGroupTypes();

            // automatically set the panel title and icon based on group type
            // If there's only one group type, use it's 'group term' in the panel title.
            if ( groupTypeIds.Count == 1 )
            {
                var singleGroupType = GroupTypeCache.Get( groupTypeIds.FirstOrDefault() );
                lTitle.Text = string.Format( "{0}", singleGroupType.GroupTerm.Pluralize() );
                iIcon.AddCssClass( singleGroupType.IconCssClass );
            }
            else
            {
                lTitle.Text = BlockName;
                iIcon.AddCssClass( "fa fa-users" );
            }

            // if a SetPanelTitle is specified in block settings, use that instead
            string customSetPanelTitle = this.GetAttributeValue( "SetPanelTitle" );
            if ( !string.IsNullOrEmpty( customSetPanelTitle ) )
            {
                lTitle.Text = customSetPanelTitle;
            }

            // if a SetPanelIcon is specified in block settings, use that instead
            string customSetPanelIcon = this.GetAttributeValue( "SetPanelIcon" );
            if ( !string.IsNullOrEmpty( customSetPanelIcon ) )
            {
                iIcon.Attributes["class"] = customSetPanelIcon;
            }
        }

        /// <summary>
        /// Sets the model dropdown.
        /// </summary>
        private void BindModelDropDown()
        {
            var groupTypeIds = GetAvailableGroupTypes();
            gpGroup.Visible = GetAttributeValue( AttributeKey.GroupPickerType ) == "GroupPicker";
            ddlGroup.Visible = GetAttributeValue( AttributeKey.GroupPickerType ) != "GroupPicker";
            if ( GetAttributeValue( AttributeKey.GroupPickerType ) == "GroupPicker" )
            {
                gpGroup.Required = true;
                gpGroup.IncludedGroupTypeIds = groupTypeIds;
                var rootGroupGuid = GetAttributeValue( AttributeKey.RootGroup ).AsGuidOrNull();
                if ( rootGroupGuid.HasValue )
                {
                    var group = new GroupService( new RockContext() ).Get( rootGroupGuid.Value );
                    if ( group != null )
                    {
                        gpGroup.RootGroupId = group.Id;
                    }
                }
            }
            else
            {
                ddlGroup.Items.Clear();
                ddlGroup.Required = true;

                var groupService = new GroupService( new RockContext() );
                bool onlySecurityGroups = GetAttributeValue( AttributeKey.LimittoSecurityRoleGroups ).AsBoolean();

                var qryGroups = groupService
                    .Queryable()
                    .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

                string limitToActiveStatus = GetAttributeValue( "LimittoActiveStatus" );
                if ( limitToActiveStatus == "active" )
                {
                    qryGroups = qryGroups.Where( a => a.IsActive );
                }

                // only show groups that the current person is authorized to add members to
                var groupList = qryGroups
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) || a.IsAuthorized( Rock.Security.Authorization.MANAGE_MEMBERS, this.CurrentPerson ) )
                    .ToList();

                ddlGroup.DataSource = groupList;
                ddlGroup.DataBind();
                ddlGroup.Items.Insert( 0, new ListItem() );
            }
            ddlGroupRole.Items.Clear();
        }

        /// <summary>
        /// Sets the group role dropdown dropdown.
        /// </summary>
        private void BindGroupRoleDropDown( int? groupId )
        {
            ddlGroupRole.Required = true;
            var rockContext = new RockContext();

            var groupSyncService = new GroupSyncService( rockContext );
            var syncList = groupSyncService
                .Queryable()
                .AsNoTracking()
                .Where( s => s.GroupId == groupId )
                .Select( s => s.GroupTypeRoleId )
                .ToList();

            nbModalDetailSyncMessage.Visible = syncList.Count > 0 ? true : false;

            var groupService = new GroupService( rockContext );
            var selectedGroup = groupService.GetNoTracking( groupId.Value );

            var groupTypeRoleService = new GroupTypeRoleService( rockContext );
            var qry = groupTypeRoleService
                .Queryable()
                .AsNoTracking()
                .Where( r => r.GroupTypeId == selectedGroup.GroupTypeId )
                .Where( r => !syncList.Contains( r.Id ) )
                .ToList();

            ddlGroupRole.DataSource = qry;
            ddlGroupRole.DataBind();
        }

        #endregion

        private class GroupListRowInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the group member identifier.
            /// </summary>
            /// <value>
            /// The group member identifier.
            /// </value>
            public int? GroupMemberId { get; set; }

            /// <summary>
            /// Gets or sets the value indicating if the current person can edit or manage members in the specified group.
            /// </summary>
            /// <value>
            /// The boolean value
            /// </value>
            public bool IsAuthorizedToEditOrManageMembers { get; set; }

            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string Path { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the type of the group.
            /// </summary>
            /// <value>
            /// The type of the group.
            /// </value>
            public GroupTypeCache GroupType { get; set; }

            /// <summary>
            /// Gets the name of the group type.
            /// </summary>
            /// <value>
            /// The name of the group type.
            /// </value>
            public string GroupTypeName
            {
                get
                {
                    return GroupType.Name;
                }
            }

            /// <summary>
            /// Gets the group type order.
            /// </summary>
            /// <value>
            /// The group type order.
            /// </value>
            public int GroupTypeOrder
            {
                get
                {
                    return GroupType.Order;
                }
            }

            /// <summary>
            /// Gets or sets the group order.
            /// </summary>
            /// <value>
            /// The group order.
            /// </value>
            public int GroupOrder { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is system.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
            /// </value>
            public bool IsSystem { get; set; }

            /// <summary>
            /// Gets or sets the group role.
            /// </summary>
            /// <value>
            /// The group role.
            /// </value>
            public string GroupRole { get; set; }

            /// <summary>
            /// Gets or sets the date added.
            /// </summary>
            /// <value>
            /// The date added.
            /// </value>
            public DateTime? DateAdded { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is archived.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
            /// </value>
            public bool IsArchived { get; set; }

            /// <summary>
            /// Gets or sets the is active order.
            /// </summary>
            /// <value>
            /// The is active order.
            /// </value>
            public int IsActiveOrder { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is synced.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is synced; otherwise, <c>false</c>.
            /// </value>
            public bool IsSynced { get; set; }

            /// <summary>
            /// Gets or sets the member count.
            /// </summary>
            /// <value>
            /// The member count.
            /// </value>
            public int MemberCount { get; set; }

            /// <summary>
            /// Gets or sets the elevated security level.
            /// </summary>
            /// <value>The elevated security level.</value>
            public ElevatedSecurityLevel ElevatedSecurityLevel { get; internal set; }

            /// <summary>
            /// Gets or sets the group is security role.
            /// </summary>
            /// <value>The group is security role.</value>
            public bool IsSecurityRole { get; internal set; }
        }
    }
}