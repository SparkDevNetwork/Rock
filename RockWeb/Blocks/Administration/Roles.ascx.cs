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
using Rock.Core;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Roles : Rock.Web.UI.Block
    {
        private bool _canConfigure = false;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _canConfigure = PageInstance.IsAuthorized( "Configure", CurrentUser );

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this role?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );
                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }
        }

        protected void tbNameFilter_TextChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            base.OnLoad( e );
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var authService = new Rock.CMS.AuthService();
                var groupService = new Rock.Groups.GroupService();

                Rock.Groups.Group group = groupService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
                if ( group != null )
                {
                    foreach(var auth in authService.Queryable().Where( a => a.GroupId == group.Id).ToList())
                    {
                        authService.Delete(auth, CurrentPersonId);
                        authService.Save(auth, CurrentPersonId);
                    }

                    groupService.Delete( group, CurrentPersonId );
                    groupService.Save( group, CurrentPersonId );

                    Rock.Security.Authorization.Flush();
                    Rock.Security.Role.Flush( group.Id );
                }
            }

            BindGrid();
        }

        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var groupService = new Rock.Groups.GroupService();

                Rock.Groups.Group group;

                int groupId = 0;
                if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out groupId ) )
                    groupId = 0;

                if ( groupId == 0 )
                {
                    group = new Rock.Groups.Group();
                    group.IsSystem = false;
                    group.IsSecurityRole = true;
                    // TODO: Need to figure out how to set/configure the "Role" group type
                    group.GroupTypeId = 1;
                    groupService.Add( group, CurrentPersonId );
                }
                else
                {
                    Rock.Security.Role.Flush( groupId );
                    group = groupService.Get( groupId );
                }

                group.Name = tbName.Text;
                group.Description = tbDescription.Text;
                groupService.Save( group, CurrentPersonId );

            }

            Rock.Security.Authorization.Flush();

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        private void BindGrid()
        {
            var queryable = new Rock.Groups.GroupService().Queryable().
                Where( r => r.IsSecurityRole == true);

            if ( !string.IsNullOrWhiteSpace( tbNameFilter.Text ) )
                queryable = queryable.Where( r => r.Name.StartsWith( tbNameFilter.Text.Trim() ) );

            SortProperty sortProperty = rGrid.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.
                    Sort( sortProperty );
            else
                queryable = queryable.
                    OrderBy( r => r.Name );

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int groupId )
        {
            var groupModel = new Rock.Groups.GroupService().Get( groupId );

            if ( groupModel != null )
            {
                lAction.Text = "Edit";
                hfId.Value = groupModel.Id.ToString();

                tbName.Text = groupModel.Name;
                tbDescription.Text = groupModel.Description;
            }
            else
            {
                lAction.Text = "Add";
                hfId.Value = string.Empty;

                tbName.Text = string.Empty;
                tbDescription.Text = string.Empty;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

    }
}