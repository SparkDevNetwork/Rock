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

namespace RockWeb.Blocks.Crm
{
    [GroupTypesField( "Group Types", "Select group types to show in this block.  Leave all unchecked to show all group types.", false )]
    [BooleanField( "Show User Count", "", true )]
    [BooleanField( "Show Description", "", true )]
    [BooleanField( "Show GroupType", "", true )]
    [BooleanField( "Show IsSystem", "", true )]
    [BooleanField( "Show Edit", "", true )]
    [BooleanField( "Show Notification" )]
    [BooleanField( "Limit to Security Role Groups" )]
    [ContextAware( typeof( Group ) )]
    [LinkedPage("Detail Page")]
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

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" ) && GetAttributeValue( "ShowEdit" ).FromTrueFalse();
            gGroups.Actions.ShowAdd = canAddEditDelete;
            gGroups.IsDeleteEnabled = canAddEditDelete;

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["MembersCount"].Visible = GetAttributeValue( "ShowUserCount" ).FromTrueFalse();
            boundFields["Description"].Visible = GetAttributeValue( "ShowDescription" ).FromTrueFalse();
            boundFields["GroupTypeName"].Visible = GetAttributeValue( "ShowGroupType" ).FromTrueFalse();
            boundFields["IsSystem"].Visible = GetAttributeValue( "ShowIsSystem" ).FromTrueFalse();
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

                    bool isSecurityRoleGroup = group.IsSecurityRole;
                    if ( isSecurityRoleGroup )
                    {
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
                        Rock.Security.Role.Flush( group.Id );
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

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            GroupService groupService = new GroupService();
            SortProperty sortProperty = gGroups.SortProperty;
            var qry = groupService.Queryable();

            // limit GroupType selection to what Block Attributes allow

            List<Guid> groupTypeGuids = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();

            if ( groupTypeGuids.Count > 0 )
            {
                qry = qry.Where( a => groupTypeGuids.Contains( a.GroupType.Guid ) );
            }

            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            qry = qry.Where( a => a.GroupType.ShowInGroupList );

            /// Using Members.Count in a grid boundfield causes the entire Members list to be populated (select * ...) and then counted
            /// Having the qry do the count just does a "select count(1) ..." which is much much faster, especially if the members list is large (a large list will lockup the webserver)
            var selectQry = qry.Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    GroupTypeName = a.GroupType.Name,
                    MembersCount = a.Members.Count(),
                    a.Description,
                    a.IsSystem
                } );
            
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty(new GridViewSortEventArgs( "Name", SortDirection.Descending));
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