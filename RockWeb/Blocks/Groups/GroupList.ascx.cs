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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List" )]
    [Category( "Groups" )]
    [Description( "Lists all groups for the configured group types. Query string parameters: <ul><li>GroupTypeId - Filters to a specific group type.</li></ui>" )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]
    [BooleanField( "Limit to Security Role Groups", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", false, "", 2 )]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 3 )]
    [BooleanField( "Display Group Path", "Should the Group path be displayed?", false, "", 4 )]
    [BooleanField( "Display Group Type Column", "Should the Group Type column be displayed?", true, "", 5 )]
    [BooleanField( "Display Description Column", "Should the Description column be displayed?", true, "", 6 )]
    [BooleanField( "Display Active Status Column", "Should the Active Status column be displayed?", false, "", 7 )]
    [BooleanField( "Display Member Count Column", "Should the Member Count column be displayed? Does not affect lists with a person context.", true, "", 8 )]
    [BooleanField( "Display System Column", "Should the System column be displayed?", true, "", 9 )]
    [BooleanField( "Display Filter", "Should filter be displayed to allow filtering by group type?", false, "", 10 )]
    [CustomDropdownListField( "Limit to Active Status", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", "all^[All], active^Active, inactive^Inactive", false, "all", Order = 11 )]
    [TextField( "Set Panel Title", "The title to display in the panel header. Leave empty to have the title be set automatically based on the group type or block name.", required: false, order: 12 )]
    [TextField( "Set Panel Icon", "The icon to display in the panel header. Leave empty to have the icon be set automatically based on the group type or default icon.", required: false, order: 13 )]
    [ContextAware]
    public partial class GroupList : RockBlock, IAdditionalGridColumns
    {
        private int _groupTypesCount = 0;
        private bool _showGroupPath = false;

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                AddAdditionalColumns();
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
            gGroups.Actions.ShowAdd = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;
            gGroups.RowDataBound += gGroups_RowDataBound;
            gGroups.ExportSource = ExcelExportSource.DataSource;

            // set up Grid based on Block Settings and Context
            bool showDescriptionColumn = GetAttributeValue( "DisplayDescriptionColumn" ).AsBoolean();
            bool showActiveStatusColumn = GetAttributeValue( "DisplayActiveStatusColumn" ).AsBoolean();
            bool showSystemColumn = GetAttributeValue( "DisplaySystemColumn" ).AsBoolean();

            if ( !showDescriptionColumn )
            {
                gGroups.TooltipField = "Description";
            }

            _showGroupPath = GetAttributeValue( "DisplayGroupPath" ).AsBoolean();

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["Name"].Visible = !_showGroupPath;
            gGroups.Columns[1].Visible = _showGroupPath;
            boundFields["GroupTypeName"].Visible = GetAttributeValue( "DisplayGroupTypeColumn" ).AsBoolean();
            boundFields["Description"].Visible = showDescriptionColumn;

            Dictionary<string, BoolField> boolFields = gGroups.Columns.OfType<BoolField>().ToDictionary( a => a.DataField );
            boolFields["IsActive"].Visible = showActiveStatusColumn;
            boolFields["IsSystem"].Visible = showSystemColumn;

            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( a => a.Id == personEntityTypeId ) )
            {
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    boundFields["GroupRole"].Visible = true;
                    boundFields["DateAdded"].Visible = true;
                    boundFields["MemberCount"].Visible = false;
                    gGroups.IsDeleteEnabled = true;
                    gGroups.HideDeleteButtonForIsSystem = false;
                }
            }
            else
            {
                bool canEdit = IsUserAuthorized( Authorization.EDIT );
                gGroups.Actions.ShowAdd = canEdit;
                gGroups.IsDeleteEnabled = canEdit;

                boundFields["GroupRole"].Visible = false;
                boundFields["DateAdded"].Visible = false;
                boundFields["MemberCount"].Visible = GetAttributeValue( "DisplayMemberCountColumn" ).AsBoolean();
            }

            SetPanelTitleAndIcon();
        }

        #region IAdditionalGridColumns

        /// <summary>
        /// Adds AdditionalColumns to the grid
        /// </summary>
        public void AddAdditionalColumns()
        {
            var additionalColumns = this.GetAttributeValue( AdditionalGridColumnsConfig.AttributeKey ).FromJsonOrNull<AdditionalGridColumnsConfig>();
            if ( additionalColumns != null && additionalColumns.ColumnsConfig.Any() )
            {
                var deleteField = gGroups.ColumnsOfType<DeleteField>().FirstOrDefault();
                int insertPosition = gGroups.Columns.IndexOf( deleteField );

                foreach ( var column in additionalColumns.GetColumns() )
                {
                    gGroups.Columns.Insert( insertPosition, column );
                    insertPosition++;
                }
            }
        }

        #endregion IAdditionalGridColumns

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroups_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var groupInfo = (GroupListRowInfo)e.Row.DataItem;

                // Show inactive entries in a lighter font.
                if ( !groupInfo.IsActive )
                {
                    e.Row.AddCssClass( "is-inactive" );
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

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Group Type", gtpGroupType.SelectedValue );

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SaveUserPreference( "Active Status", string.Empty );
            }
            else
            {
                gfSettings.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            gfSettings.SaveUserPreference( "Group Type Purpose", ddlGroupTypePurpose.SelectedValue );

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

                    var groupType = GroupTypeCache.Read( id );
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
                        var groupTypePurpose = DefinedValueCache.Read( groupTypePurposeTypeValueId.Value );
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
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( a => a.Id == personEntityTypeId ) )
            {
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
            NavigateToLinkedPage( "DetailPage", "GroupId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            AuthService authService = new AuthService( rockContext );
            Group group = groupService.Get( e.RowKeyId );

            if ( group != null )
            {
                string errorMessage;
                bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;

                if ( ContextTypesRequired.Any( a => a.Id == personEntityTypeId ) )
                {
                    var personContext = ContextEntity<Person>();
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    RegistrationRegistrantService registrantService = new RegistrationRegistrantService( rockContext );
                    GroupMember groupMember = group.Members.SingleOrDefault( a => a.PersonId == personContext.Id );

                    if ( !groupMemberService.CanDelete( groupMember, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    foreach ( var registrant in registrantService.Queryable().Where( r => r.GroupMemberId == groupMember.Id ) )
                    {
                        registrant.GroupMemberId = null;
                    }

                    if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                    {
                        // person removed from SecurityRole, Flush
                        Rock.Security.Role.Flush( group.Id );
                    }

                    groupMemberService.Delete( groupMember );

                }
                else
                {
                    if ( !group.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                    {
                        mdGridWarning.Show( "You are not authorized to delete this group", ModalAlertType.Information );
                        return;
                    }


                    if ( !groupService.CanDelete( group, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }


                    if ( isSecurityRoleGroup )
                    {
                        Rock.Security.Role.Flush( group.Id );
                        foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                        {
                            authService.Delete( auth );
                        }
                    }

                    groupService.Delete( group );
                }

                rockContext.SaveChanges();

                if ( isSecurityRoleGroup )
                {
                    Rock.Security.Authorization.Flush();
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

            var group = groupService.Get( ddlGroup.SelectedValue.AsInteger() );
            if ( group == null )
            {
                nbMessage.Title = "Please select a Group";
                return;
            }

            var personContext = ContextEntity<Person>();
            var groupMemberService = new GroupMemberService( rockContext );

            if ( groupMemberService.Queryable().Any( a => a.PersonId == personContext.Id && a.GroupId == group.Id ) )
            {
                nbMessage.Title = "Member already added to selected Group";
                return;
            }

            var roleId = group.GroupType.DefaultGroupRoleId;

            if ( roleId == null )
            {
                nbMessage.Title = "No default role for particular group is assigned";
                return;
            }

            GroupMember groupMember = new GroupMember { Id = 0 };
            groupMember.GroupId = group.Id;
            groupMember.PersonId = personContext.Id;
            groupMember.GroupRoleId = roleId.Value;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMemberService.Add( groupMember );
            rockContext.SaveChanges();

            if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
            {
                Rock.Security.Role.Flush( group.Id );
            }

            modalDetails.Hide();
            BindFilter();
            BindGrid();
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

                gtpGroupType.SelectedValue = gfSettings.GetUserPreference( "Group Type" );
            }

            ddlGroupTypePurpose.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() ), true );
            ddlGroupTypePurpose.SetValue( gfSettings.GetUserPreference( "Group Type Purpose" ) );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetUserPreference( "Active Status" ) );
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
                int? groupTypeFilter = gfSettings.GetUserPreference( "Group Type" ).AsIntegerOrNull();
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

            bool onlySecurityGroups = GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean();

            var qryGroups = groupService.Queryable()
                .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

            string limitToActiveStatus = GetAttributeValue( "LimittoActiveStatus" );

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
            }


            var groupTypePurposeValue = gfSettings.GetUserPreference( "Group Type Purpose" ).AsIntegerOrNull();

            var groupList = new List<GroupListRowInfo>();

            // Person context will exist if used on a person detail page
            int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) )
            {
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    // limit to Groups that the person is a member of
                    var qry = new GroupMemberService( rockContext ).Queryable( true )
                        .Where( m => m.PersonId == personContext.Id )
                        .Join( qryGroups, gm => gm.GroupId, g => g.Id, ( gm, g ) => new { Group = g, GroupMember = gm } );

                    // Filter by Active Status of Group and Group Membership.
                    if ( showActive && !showInactive )
                    {
                        // Show only active Groups and active Memberships.
                        qry = qry.Where( gmg => gmg.Group.IsActive && gmg.GroupMember.GroupMemberStatus == GroupMemberStatus.Active );
                    }
                    else if ( !showActive )
                    {
                        // Show only inactive Groups or inactive Memberships.
                        qry = qry.Where( gmg => !gmg.Group.IsActive || gmg.GroupMember.GroupMemberStatus == GroupMemberStatus.Inactive );
                    }

                    if ( groupTypePurposeValue.HasValue && gfSettings.Visible )
                    {
                        qry = qry.Where( t => t.Group.GroupType.GroupTypePurposeValueId == groupTypePurposeValue );
                    }

                    groupList = qry
                        .AsEnumerable()
                        .Where( gm => gm.Group.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                        .Select( m => new GroupListRowInfo
                        {
                            Id = m.Group.Id,
                            Path = string.Empty,
                            Name = m.Group.Name,
                            GroupTypeName = m.Group.GroupType.Name,
                            GroupOrder = m.Group.Order,
                            GroupTypeOrder = m.Group.GroupType.Order,
                            Description = m.Group.Description,
                            IsSystem = m.Group.IsSystem,
                            GroupRole = m.GroupMember.GroupRole.Name,
                            DateAdded = m.GroupMember.CreatedDateTime,
                            IsActive = m.Group.IsActive && ( m.GroupMember.GroupMemberStatus == GroupMemberStatus.Active ),
                            IsActiveOrder = ( m.Group.IsActive && ( m.GroupMember.GroupMemberStatus == GroupMemberStatus.Active ) ? 1 : 2 ),
                            MemberCount = 0
                        } )
                        .AsQueryable()
                        .Sort( sortProperty )
                        .ToList();
                }
            }
            else
            {
                var roleGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
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

                groupList = qryGroups
                    .AsEnumerable()
                    .Where(g => g.IsAuthorized(Rock.Security.Authorization.VIEW, CurrentPerson))
                    .Select( g => new GroupListRowInfo
                    {
                        Id = g.Id,
                        Path = string.Empty,
                        Name = ( ( useRolePrefix && g.GroupType.Id != roleGroupTypeId ) ? "GROUP - " : "" ) + g.Name,
                        GroupTypeName = g.GroupType.Name,
                        GroupOrder = g.Order,
                        GroupTypeOrder = g.GroupType.Order,
                        Description = g.Description,
                        IsSystem = g.IsSystem,
                        IsActive = g.IsActive,
                        IsActiveOrder = g.IsActive ? 1 : 2,
                        GroupRole = string.Empty,
                        DateAdded = DateTime.MinValue,
                        MemberCount = g.Members.Count()
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
            gGroups.EntityTypeId = EntityTypeCache.Read<Group>().Id;
            gGroups.DataBind();

            // hide the group type column if there's only one type; must come after DataBind()
            if ( _groupTypesCount == 1 )
            {
                this.gGroups.Columns[2].Visible = false;
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
                var groupType = GroupTypeCache.Read( groupTypeId );
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
                var singleGroupType = GroupTypeCache.Read( groupTypeIds.FirstOrDefault() );
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
            ddlGroup.Items.Clear();
            ddlGroup.AutoPostBack = false;
            ddlGroup.Required = true;

            #region groupquery

            var groupTypeIds = GetAvailableGroupTypes();

            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );

            bool onlySecurityGroups = GetAttributeValue( "LimittoSecurityRoleGroups" ).AsBoolean();

            var qryGroups = groupService.Queryable()
                .Where( g => groupTypeIds.Contains( g.GroupTypeId ) && ( !onlySecurityGroups || g.IsSecurityRole ) );

            string limitToActiveStatus = GetAttributeValue( "LimittoActiveStatus" );

            if ( limitToActiveStatus == "active" )
            {
                qryGroups = qryGroups.Where( a => a.IsActive );
            }

            var personContext = ContextEntity<Person>();
            qryGroups = qryGroups.Where( a => !a.Members.Any( m => m.PersonId == personContext.Id ) );

            #endregion

            ddlGroup.DataSource = qryGroups
                .Select( g => new
                {
                    Id = g.Id,
                    Name = g.Name
                } ).OrderBy( a => a.Name ).ToList();
            ddlGroup.DataBind();
        }

        #endregion

        [DotLiquid.LiquidType( "Id", "Path", "Name", "GroupTypeName", "GroupOrder", "GroupTypeOrder", "Description", "IsSystem", "GroupRole", "DateAdded", "IsActive", "IsActiveOrder", "MemberCount"  )]
        private class GroupListRowInfo
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public string Name { get; set; }
            public string GroupTypeName { get; set; }
            public int GroupOrder { get; set; }
            public int GroupTypeOrder { get; set; }
            public string Description { get; set; }
            public bool IsSystem { get; set; }
            public string GroupRole { get; set; }
            public DateTime? DateAdded { get; set; }
            public bool IsActive { get; set; }
            public int IsActiveOrder { get; set; }
            public int MemberCount { get; set; }
        }
    }
}