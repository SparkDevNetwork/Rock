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

namespace RockWeb.Plugins.org_hfbc.Legacy685
{
    [DisplayName( "Family Group List" )]
    [Category( "org_hfbc > Legacy 685" )]
    [Description( "Lists all groups for the configured group types. Query string parameters: <ul><li>GroupTypeId - Filters to a specific group type.</li><li>GroupId - Filters to groups that members of the GroupId are in</li><li>PersonId - Filters to groups that the person's family members are in</li></ui>" )]

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
    public partial class FamilyGroupList : RockBlock
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

            BindFilter();

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

            // set up Grid based on Block Settings
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

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gGroups.Actions.ShowAdd = canEdit;
            gGroups.IsDeleteEnabled = canEdit;

            boundFields["GroupRole"].Visible = false;
            boundFields["DateAdded"].Visible = false;
            boundFields["MemberCount"].Visible = GetAttributeValue( "DisplayMemberCountColumn" ).AsBoolean();

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
                if ( !groupInfo.IsActive )
                {
                    e.Row.AddCssClass( "inactive" );
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
            ApplyBlockSettings();
            BindFilter();
            BindGrid();
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
            }
        }

        /// <summary>
        /// Handles the Add event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroups_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var isOnPersonPage = false;
            var groupId = GetGroupId( rockContext );
            if ( groupId != null )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    Person preferredContact = GetPreferredContact( group );
                    if ( preferredContact != null )
                    {
                        isOnPersonPage = true;
                    }
                }
            }

            if ( isOnPersonPage )
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
                nbModalDetailsMessage.Title = "Please select a Group";
                nbModalDetailsMessage.Visible = true;
                return;
            }

            var groupId = GetGroupId( rockContext );
            if ( groupId != null )
            {
                var familyGroup = new GroupService( rockContext ).Get( groupId.Value );
                if ( familyGroup != null )
                {
                    Person preferredContact = GetPreferredContact( familyGroup );
                    if ( preferredContact != null )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );

                        if ( groupMemberService.Queryable().Any( a => a.PersonId == preferredContact.Id && a.GroupId == group.Id ) )
                        {
                            nbModalDetailsMessage.Title = "Member already added to selected Group";
                            nbModalDetailsMessage.Visible = true;
                            return;
                        }

                        var roleId = group.GroupType.DefaultGroupRoleId;

                        if ( roleId == null )
                        {
                            nbModalDetailsMessage.Title = "No default role for particular group is assigned";
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
                        groupMember.PersonId = preferredContact.Id;
                        groupMember.GroupRoleId = roleId.Value;
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                        groupMemberService.Add( groupMember );
                        rockContext.SaveChanges();

                        if ( group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() ) )
                        {
                            Rock.Security.Role.Flush( group.Id );
                        }
                    }
                }
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

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetUserPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }
        private int? GetGroupId( RockContext rockContext = null )
        {
            int? groupId = null;

            groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
            if ( !groupId.HasValue )
            {
                var personId = PageParameter( "PersonId" ).AsIntegerOrNull();

                if ( personId != null )
                {
                    if ( rockContext == null )
                    {
                        rockContext = new RockContext();
                    }

                    var person = new PersonService( rockContext ).Get( personId.Value );
                    if ( person != null )
                    {
                        groupId = person.GetFamily().Id;
                    }
                }
            }

            return groupId;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var groupId = GetGroupId();
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

            if ( groupId.HasValue )
            {
                var group = groupService.Get( groupId.Value );
                if ( group != null )
                {
                    var personIdList = group.Members
                        .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( gm => gm.PersonId )
                        .Distinct()
                        .ToList();

                    SortProperty sortProperty = gGroups.SortProperty;
                    if ( sortProperty == null )
                    {
                        sortProperty = new SortProperty( new GridViewSortEventArgs( "IsActiveOrder, GroupTypeOrder, GroupTypeName, GroupOrder, Name", SortDirection.Ascending ) );
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

                    var groupList = new List<GroupListRowInfo>();

                    // limit to Groups that the person is a member of
                    var qry = new GroupMemberService( rockContext ).Queryable( true )
                        .Where( m => personIdList.Contains( m.PersonId ) )
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

                    qry = qry.DistinctBy( m => m.Group ).AsQueryable();

                    groupList = qry
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
                        .Sort( sortProperty )
                        .ToList();


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

            groupTypeIds = qry.Select( t => t.Id ).ToList();

            return groupTypeIds;
        }

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

            var groupId = GetGroupId( rockContext );
            if ( groupId != null )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    Person preferredContact = GetPreferredContact( group );
                    if ( preferredContact != null )
                    {
                        qryGroups = qryGroups.Where( a => !a.Members.Any( m => m.PersonId == preferredContact.Id ) );

                        #endregion

                        // only show groups that the current person is authorized to add members to
                        var groupList = qryGroups.OrderBy( a => a.Name ).ToList()
                            .Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) || a.IsAuthorized( Rock.Security.Authorization.MANAGE_MEMBERS, this.CurrentPerson ) ).ToList();

                        ddlGroup.DataSource = groupList;
                        ddlGroup.DataBind();
                    }
                }
            }
        }

        private static Person GetPreferredContact( Group group )
        {
            Person preferredContact = null;
            if ( group != null )
            {
                group.Members = group.Members.OrderBy( m => m.Person.LastName ).ThenBy( m => m.Person.FirstName ).ToList();

                var adultMemberList = group.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.GroupRoleId == 3 ).ToList();
                foreach ( var member in adultMemberList )
                {
                    var person = member.Person;
                    person.LoadAttributes();
                    var isPreferredContact = person.GetAttributeValue( "IsPreferredContact" ).AsBoolean();
                    if ( isPreferredContact )
                    {
                        preferredContact = person;
                        break;
                    }
                }

                if ( preferredContact == null )
                {
                    preferredContact = adultMemberList.Where( gm => gm.Person.Gender == Gender.Female ).Select( gm => gm.Person ).FirstOrDefault();
                }

                if ( preferredContact == null )
                {
                    preferredContact = adultMemberList.Select( gm => gm.Person ).FirstOrDefault();
                }
            }


            return preferredContact;
        }

        #endregion

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