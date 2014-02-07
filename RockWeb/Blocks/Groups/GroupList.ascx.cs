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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [DisplayName( "Group List" )]
    [Category( "Groups" )]
    [Description( "Lists all groups for the configured group types." )]

    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]
    [BooleanField( "Limit to Security Role Groups", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", false, "", 2)]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 3 )]
    [BooleanField( "Display Group Type Column", "Should the Group Type column be displayed?", true, "", 4 )]
    [BooleanField( "Display Description Column", "Should the Description column be displayed?", true, "", 5)]
    [BooleanField( "Display Filter", "Should filter be displayed to allow filtering by group type?", false, "", 6)]
    [ContextAware]
    public partial class GroupList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.Visible = (GetAttributeValue("DisplayFilter") ?? "false").AsBoolean();
            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gGroups.DataKeyNames = new string[] { "id" };
            gGroups.Actions.ShowAdd = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;

            BindFilter();
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

        #endregion

        #region Events

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Group Type", gtpGroupType.SelectedValue );
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

                    int id = int.MinValue;
                    if ( int.TryParse( e.Value, out id ) )
                    {
                        var groupType = GroupTypeCache.Read( id );
                        if ( groupType != null )
                        {
                            e.Value = groupType.Name;
                        }
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
            NavigateToLinkedPage( "DetailPage", "groupId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Delete( object sender, RowEventArgs e )
        {
            // NOTE: Very similar code in GroupDetail.btnDelete_Click
            RockTransactionScope.WrapTransaction( () =>
            {
                GroupService groupService = new GroupService();
                AuthService authService = new AuthService();
                Group group = groupService.Get( (int)e.RowKeyValue );

                if ( group != null )
                {
                    string errorMessage;
                    if ( !groupService.CanDelete( group, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    bool isSecurityRoleGroup = group.IsSecurityRole || group.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
                    if (isSecurityRoleGroup)
                    {
                        Rock.Security.Role.Flush( group.Id );
                        foreach ( var auth in authService.Queryable().Where( a => a.GroupId == group.Id ).ToList() )
                        {
                            authService.Delete( auth, CurrentPersonAlias );
                            authService.Save( auth, CurrentPersonAlias );
                        }
                    }

                    groupService.Delete( group, CurrentPersonAlias );
                    groupService.Save( group, CurrentPersonAlias );

                    if ( isSecurityRoleGroup )
                    {
                        Rock.Security.Authorization.Flush();
                    }
                }
            } );

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

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var groupTypeIds = GetAvailableGroupTypes();
            gtpGroupType.GroupTypes = new GroupTypeService().Queryable()
                .Where( g => groupTypeIds.Contains( g.Id ) ).ToList();

            gtpGroupType.SelectedValue = gfSettings.GetUserPreference( "Group Type" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Find all the Group Types
            var groupTypeIds = GetAvailableGroupTypes();

            if ( ( GetAttributeValue( "DisplayFilter" ) ?? "false" ).AsBoolean() )
            {
                int groupTypeFilter = int.MinValue;
                if ( int.TryParse( gfSettings.GetUserPreference( "Group Type" ), out groupTypeFilter ) )
                {
                    groupTypeIds = groupTypeIds.Where( g => g == groupTypeFilter ).ToList();
                }
            }

            using ( new UnitOfWorkScope() )
            {
                SortProperty sortProperty = gGroups.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "GroupTypeOrder, GroupTypeName, GroupOrder, Name", SortDirection.Ascending ) );
                }
                bool onlySecurityGroups =  GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse();
                bool showDescriptionColumn = GetAttributeValue( "DisplayDescriptionColumn" ).FromTrueFalse();
                if ( !showDescriptionColumn )
                {
                    gGroups.TooltipField = "Description";
                }
                Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
                boundFields["GroupTypeName"].Visible = GetAttributeValue( "DisplayGroupTypeColumn" ).FromTrueFalse();
                boundFields["Description"].Visible = showDescriptionColumn;

                // Person context will exist if used on a person detail page
                var personContext = ContextEntity<Person>();
                if ( personContext != null )
                {
                    boundFields["GroupRole"].Visible = true;
                    boundFields["DateAdded"].Visible = true;
                    boundFields["MemberCount"].Visible = false;

                    gGroups.Actions.ShowAdd = false;
                    gGroups.IsDeleteEnabled = false;
                    gGroups.Columns.OfType<DeleteField>().ToList().ForEach( f => f.Visible = false);

                    gGroups.DataSource = new GroupMemberService().Queryable()
                        .Where( m => 
                            m.PersonId == personContext.Id &&
                            groupTypeIds.Contains(m.Group.GroupTypeId) &&
                            (!onlySecurityGroups || m.Group.IsSecurityRole) )
                        .Select( m => new 
                            {
                                Id = m.Group.Id,
                                Name = m.Group.Name,
                                GroupTypeName = m.Group.GroupType.Name,
                                GroupOrder = m.Group.Order,
                                GroupTypeOrder = m.Group.GroupType.Order,
                                Description = m.Group.Description,
                                IsSystem = m.Group.IsSystem,
                                GroupRole = m.GroupRole.Name,
                                DateAdded = m.CreatedDateTime,
                                MemberCount = 0
                            })
                        .Sort(sortProperty)
                        .ToList();
                }
                else
                {
                    bool canEdit = IsUserAuthorized( "Edit" );
                    gGroups.Actions.ShowAdd = canEdit;
                    gGroups.IsDeleteEnabled = canEdit;

                    boundFields["GroupRole"].Visible = false;
                    boundFields["DateAdded"].Visible = false;
                    boundFields["MemberCount"].Visible = true;

                    gGroups.DataSource = new GroupService().Queryable()
                        .Where( g => 
                            groupTypeIds.Contains(g.GroupTypeId) &&
                            (!onlySecurityGroups || g.IsSecurityRole) )
                        .Select( g => new 
                            {
                                Id = g.Id,
                                Name = g.Name,
                                GroupTypeName = g.GroupType.Name,
                                GroupOrder = g.Order,
                                GroupTypeOrder = g.GroupType.Order,
                                Description = g.Description,
                                IsSystem = g.IsSystem,
                                GroupRole = "",
                                DateAdded = DateTime.MinValue,
                                MemberCount = g.Members.Count()
                            })
                        .Sort(sortProperty)
                        .ToList();
                }

                gGroups.DataBind();
            }
        }


        private List<int> GetAvailableGroupTypes()
        {
            var groupTypeIds = new List<int>();

            using ( new UnitOfWorkScope() )
            {
                var groupTypeService = new GroupTypeService();
                var qry = groupTypeService.Queryable().Where( t => t.ShowInGroupList );

                List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
                if ( includeGroupTypeGuids.Count > 0 )
                {
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
                    if ( groupType != null && groupType.IsAuthorized( "View", CurrentPerson ) )
                    {
                        groupTypeIds.Add( groupTypeId );
                    }
                }

                groupTypeIds = qry.Select( t => t.Id ).ToList();
            }

            return groupTypeIds;
        }

        #endregion
    }
}