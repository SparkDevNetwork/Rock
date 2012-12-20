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
    [GroupTypesField( 0, "Group Types", false, "", "", "", "Select group types to show in this block.  Leave all unchecked to show all group types." )]
    [BooleanField( 1, "Show User Count", true )]
    [BooleanField( 2, "Show Description", true )]
    [BooleanField( 3, "Show Edit", true )]
    [BooleanField( 5, "Show Notification", false )]
    [BooleanField( 6, "Limit to Security Role Groups", false )]
    [ContextAware( "Rock.Model.Group" )]
    [DetailPage]
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
            gGroups.Actions.IsAddEnabled = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" ) && AttributeValue( "ShowEdit" ).FromTrueFalse();
            gGroups.Actions.IsAddEnabled = canAddEditDelete;
            gGroups.IsDeleteEnabled = canAddEditDelete;

            Dictionary<string, BoundField> boundFields = gGroups.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["Members.Count"].Visible = AttributeValue( "ShowUserCount" ).FromTrueFalse();
            boundFields["Description"].Visible = AttributeValue( "ShowDescription" ).FromTrueFalse();
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
            NavigateToDetailPage( "groupId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "groupId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroups_Delete( object sender, RowEventArgs e )
        {
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
                        foreach ( var auth in authService.Queryable().Where( a => a.GroupId.Equals( group.Id ) ).ToList() )
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

            List<int> groupTypeIds = AttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
            if ( groupTypeIds.Count > 0 )
            {
                qry = qry.Where( a => groupTypeIds.Contains( a.GroupTypeId ) );
            }

            if ( AttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            if ( ContextEntities.ContainsKey( "Rock.Model.Group" ) )
            {
                Group parentGroup = ContextEntities["Rock.Model.Group"] as Group;
                if ( parentGroup != null )
                {
                    qry = qry.Where( a => a.IsAncestorOfGroup( parentGroup.Id ) );
                }
            }

            if ( sortProperty != null )
            {
                gGroups.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroups.DataSource = qry.OrderBy( p => p.Name ).ToList();
            }

            gGroups.DataBind();
        }

        #endregion
    }
}