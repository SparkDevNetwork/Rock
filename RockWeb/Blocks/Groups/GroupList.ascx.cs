//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    [LinkedPage( "Detail Page", "", true, "", "", 0 )]
    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]
    [BooleanField( "Limit to Security Role Groups", "Any groups can be flagged as a security group (even if they're not a security role).  Should the list of groups be limited to these groups?", false, "", 2)]
    [GroupTypesField( "Exclude Group Types", "The group types to exclude from the list (only valid if including all groups).", false, "", "", 3 )]
    [BooleanField( "Display Group Type Column", "Should the Group Type column be displayed?", true, "", 4 )]
    [BooleanField( "Display Description Column", "Should the Description column be displayed?", true, "", 5)]
    [BooleanField( "Display Member Count Column", "Should the Members column be displayed?", true, "", 6 )]
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

            gGroups.DataKeyNames = new string[] { "id" };
            gGroups.Actions.ShowAdd = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;

            bool canEdit = IsUserAuthorized( "Edit" );
            gGroups.Actions.ShowAdd = canEdit;
            gGroups.IsDeleteEnabled = canEdit;

            bool showDescriptionColumn = GetAttributeValue( "DisplayDescriptionColumn" ).FromTrueFalse();
            if (!showDescriptionColumn)
            {
                gGroups.TooltipField = "Description";
            }
            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["GroupTypeName"].Visible = GetAttributeValue( "DisplayGroupTypeColumn" ).FromTrueFalse();
            boundFields["Description"].Visible = showDescriptionColumn;
            boundFields["MembersCount"].Visible = GetAttributeValue( "DisplayMemberCountColumn" ).FromTrueFalse();
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
                            authService.Delete( auth, CurrentPersonId );
                            authService.Save( auth, CurrentPersonId );
                        }
                    }

                    groupService.Delete( group, CurrentPersonId );
                    groupService.Save( group, CurrentPersonId );

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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            GroupService groupService = new GroupService();
            SortProperty sortProperty = gGroups.SortProperty;

            var qry = groupService.Queryable().Where( a => a.GroupType.ShowInGroupList );

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( a => includeGroupTypeGuids.Contains( a.GroupType.Guid ) );
            }

            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            List<Guid> excludeGroupTypeGuids = GetAttributeValue( "ExcludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( a => !excludeGroupTypeGuids.Contains( a.GroupType.Guid ) );
            }

            /// Using Members.Count in a grid boundfield causes the entire Members list to be populated (select * ...) and then counted
            /// Having the qry do the count just does a "select count(1) ..." which is much much faster, especially if the members list is large (a large list will lockup the webserver)
            var selectQry = qry.Select( a =>
                new
                {
                    a.Id,
                    a.Order,
                    a.Name,
                    GroupTypeName = a.GroupType.Name,
                    MembersCount = a.Members.Count(),
                    a.Description,
                    a.IsSystem
                } );
            
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty(new GridViewSortEventArgs( "Order, Name", SortDirection.Ascending));
            }

            var list = selectQry.Sort( sortProperty ).ToList();

            // if there is a Group context, limit groups to ones that have the Group context as an ancestor
            Group parentGroup = ContextEntity<Group>();
            if ( parentGroup != null )
            {
                var descendentIds = groupService.GetAllDescendents( parentGroup.Id ).Select( a => a.Id );
                list = list.Where( a => descendentIds.Contains( a.Id ) ).ToList();
            }

            gGroups.DataSource = list;
            gGroups.DataBind();
        }

        #endregion
    }
}