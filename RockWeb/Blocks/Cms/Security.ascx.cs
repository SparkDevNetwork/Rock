using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Controls;

namespace RockWeb.Blocks.Cms
{
    public partial class Security : Rock.Cms.CmsBlock
    {
        #region Fields

        private Rock.Services.Cms.AuthService authService = new Rock.Services.Cms.AuthService();
        private Rock.Cms.Security.ISecured iSecured;

        #endregion

        #region Overridden Methods

        protected override void OnInit( EventArgs e )
        {
            // Read parameter values
            string entityName = Rock.Cms.Security.Authorization.DecodeEntityTypeName(PageParameter( "EntityType" ));
            int entityId = Convert.ToInt32( PageParameter( "EntityId" ) );

            // Get object type
            Type entityType = Type.GetType( entityName );

            // Instantiate object
            iSecured = entityType.InvokeMember( "Read", System.Reflection.BindingFlags.InvokeMethod, null, entityType, new object[] { entityId } ) as Rock.Cms.Security.ISecured;

            lTitle.Text = iSecured.ToString();

            MembershipUser user = Membership.GetUser();
            if ( iSecured.Authorized( "Configure", Membership.GetUser() ) )
            {
                ddlAction.DataSource = iSecured.SupportedActions;
                ddlAction.DataBind();
                ShowActionNote();

                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );

                ddlRoles.DataSource = Rock.Cms.Security.Role.AllRoles();
                ddlRoles.DataValueField = "Guid";
                ddlRoles.DataTextField = "Name";
                ddlRoles.DataBind();

                string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0} td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this role/user?');
            }});
    }});
", rGrid.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( iSecured.Authorized( "Configure", Membership.GetUser() ) )
            {
                if (!Page.IsPostBack)
                    BindGrid();
            }
            else
            {
                rGrid.Visible = false;
                nbMessage.Text = "You are not authorized to edit security for this entity";
                nbMessage.Visible = true;
            }

            
            base.OnLoad( e );
        }

        #endregion

        #region Events

        #region Grid Events

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            List<Rock.Models.Cms.Auth> rules = authService.GetAuths( iSecured.AuthEntity, iSecured.Id, ddlAction.Text ).ToList();
            authService.Reorder( rules, e.OldIndex, e.NewIndex, CurrentPersonId );

            Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );

            BindGrid();
        }

        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Rock.Cms.Security.AuthRule authRule = ( Rock.Cms.Security.AuthRule )e.Row.DataItem;
                RadioButtonList rbl = ( RadioButtonList )e.Row.FindControl( "rblAllowDeny" );
                rbl.SelectedValue = authRule.AllowOrDeny;
            }
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Models.Cms.Auth auth = authService.GetAuth( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( auth != null )
            {
                authService.DeleteAuth( auth );
                authService.Save( auth, CurrentPersonId );

                Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );
            }

            BindGrid();
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        protected void ddlAction_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowActionNote();
            SetRoleActions();
            BindGrid();
        }

        protected void rblAllowDeny_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rblAllowDeny = ( RadioButtonList )sender;
            GridViewRow selectedRow = rblAllowDeny.NamingContainer as GridViewRow;
            if ( selectedRow != null )
            {
                int id = ( int )rGrid.DataKeys[selectedRow.RowIndex]["id"];

                Rock.Models.Cms.Auth auth = authService.GetAuth( id );
                if ( auth != null )
                {
                    auth.AllowOrDeny = rblAllowDeny.SelectedValue;
                    authService.Save( auth, CurrentPersonId );

                    Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );
                }
            }

            BindGrid();
        }

        protected void lbShowRole_Click( object sender, EventArgs e )
        {
            pnlAddRole.Visible = true;
            pnlAddUser.Visible = false;
            SetRoleActions();
        }

        protected void lbShowUser_Click( object sender, EventArgs e )
        {
            pnlAddUser.Visible = true;
            pnlAddRole.Visible = false;
        }

        protected void lbAddAllUsers_Click( object sender, EventArgs e )
        {
            List<Rock.Cms.Security.AuthRule> existingAuths =
                Rock.Cms.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : 0;

            bool actionUpdated = false;

            bool alreadyExists = false;

            foreach ( Rock.Cms.Security.AuthRule auth in existingAuths )
                if ( auth.UserOrRole == "U" && auth.UserOrRoleName == "*" )
                {
                    alreadyExists = true;
                    break;
                }

            if ( !alreadyExists )
            {
                Rock.Models.Cms.Auth auth = new Rock.Models.Cms.Auth();
                auth.EntityType = iSecured.AuthEntity;
                auth.EntityId = iSecured.Id;
                auth.Action = ddlAction.Text;
                auth.AllowOrDeny = "A";
                auth.UserOrRole = "U";
                auth.UserOrRoleName = "*";
                auth.Order = ++maxOrder;
                authService.AddAuth( auth );
                authService.Save( auth, CurrentPersonId );

                actionUpdated = true;
            }

            if ( actionUpdated )
                Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );

            pnlAddUser.Visible = false;
            BindGrid();
        }

        protected void ddlRoles_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetRoleActions();
        }

        protected void lbAddRole_Click( object sender, EventArgs e )
        {
            foreach ( ListItem li in cblRoleActionList.Items )
            {
                if (li.Selected)
                {
                    bool actionUpdated = false;
                    bool alreadyExists = false;
                    int maxOrder = 0;

                    foreach ( Rock.Cms.Security.AuthRule rule in
                        Rock.Cms.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, li.Text ) )
                    {
                        if ( rule.UserOrRole == "R" && rule.UserOrRoleName == ddlRoles.SelectedValue )
                            alreadyExists = true;
                        if (rule.Order > maxOrder)
                            maxOrder = rule.Order;
                    }

                    if ( !alreadyExists )
                    {
                        Rock.Models.Cms.Auth auth = new Rock.Models.Cms.Auth();
                        auth.EntityType = iSecured.AuthEntity;
                        auth.EntityId = iSecured.Id;
                        auth.Action = li.Text;
                        auth.AllowOrDeny = "A";
                        auth.UserOrRole = "R";
                        auth.UserOrRoleName = ddlRoles.SelectedValue;
                        auth.Order = ++maxOrder;
                        authService.AddAuth( auth );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                    if ( actionUpdated )
                        Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, li.Text );
                }
            }

            pnlAddRole.Visible = false;
            BindGrid();
        }

        protected void lbUserSearch_Click( object sender, EventArgs e )
        {
            cbUsers.DataTextField = "FullName";
            cbUsers.DataValueField = "Guid";
            cbUsers.DataSource = new Rock.Services.Crm.PersonService().GetByFullName( tbUser.Text ).ToList();
            cbUsers.DataBind();
        }

        protected void lbAddUser_Click( object sender, EventArgs e )
        {
            List<Rock.Cms.Security.AuthRule> existingAuths =
                Rock.Cms.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : 0;

            bool actionUpdated = false;

            foreach ( ListItem li in cbUsers.Items )
            {
                if ( li.Selected )
                {
                    bool alreadyExists = false;

                    foreach ( Rock.Cms.Security.AuthRule auth in existingAuths )
                        if ( auth.UserOrRole == "U" && auth.UserOrRoleName == li.Value )
                        {
                            alreadyExists = true;
                            break;
                        }

                    if ( !alreadyExists )
                    {
                        Rock.Models.Cms.Auth auth = new Rock.Models.Cms.Auth();
                        auth.EntityType = iSecured.AuthEntity;
                        auth.EntityId = iSecured.Id;
                        auth.Action = ddlAction.Text;
                        auth.AllowOrDeny = "A";
                        auth.UserOrRole = "U";
                        auth.UserOrRoleName = li.Value;
                        auth.Order = ++maxOrder;
                        authService.AddAuth( auth );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                }
            }

            if ( actionUpdated )
                Rock.Cms.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );

            pnlAddUser.Visible = false;
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                rGrid.DataSource = Rock.Cms.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, ddlAction.Text );
                rGrid.DataBind();
            }
        }

        private void ShowActionNote()
        {
            bool defaultAction = iSecured.DefaultAuthorization( ddlAction.SelectedValue );
            lActionNote.Text = string.Format( "By default, all users {0} be authorized to {1}.",
                ( defaultAction ? "WILL" : "WILL NOT" ), ddlAction.SelectedValue );
        }

        private void SetRoleActions()
        {
            cblRoleActionList.Items.Clear();

            foreach ( string action in iSecured.SupportedActions )
            {
                if ( action == ddlAction.Text )
                {
                    ListItem roleItem = new ListItem( action );
                    roleItem.Selected = true;
                    cblRoleActionList.Items.Add( roleItem );
                }
                else
                {
                    bool alreadyAdded = false;
                    foreach ( Rock.Cms.Security.AuthRule rule in
                        Rock.Cms.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, action ) )
                    {
                        if ( rule.UserOrRole == "R" && rule.UserOrRoleName == ddlRoles.SelectedValue )
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if ( !alreadyAdded )
                        cblRoleActionList.Items.Add( new ListItem( action ) );
                }
            }
        }

        #endregion

    }
}