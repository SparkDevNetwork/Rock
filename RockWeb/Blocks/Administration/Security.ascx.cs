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

using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Security : Rock.Web.UI.Block
    {
        #region Fields

        private Rock.CMS.AuthService authService = new Rock.CMS.AuthService();
        private Rock.Security.ISecured iSecured;

        protected string CurrentAction
        {
            get
            {
                object currentAction = ViewState["CurrentAction"];
                return currentAction != null ? currentAction.ToString() : "View";
            }
            set
            {
                ViewState["CurrentAction"] = value;
            }
        }

        #endregion

        #region Overridden Methods

        protected override void OnInit( EventArgs e )
        {
            // Read parameter values
            string entityName = Rock.Security.Authorization.DecodeEntityTypeName(PageParameter( "EntityType" ));
            int entityId = Convert.ToInt32( PageParameter( "EntityId" ) );

            // Get object type
            Type entityType = Type.GetType( entityName );

            // Instantiate object
            iSecured = entityType.InvokeMember( "Read", System.Reflection.BindingFlags.InvokeMethod, null, entityType, new object[] { entityId } ) as Rock.Security.ISecured;

            if ( iSecured.Authorized( "Configure", CurrentUser ) )
            {
                rptActions.DataSource = iSecured.SupportedActions;
                rptActions.DataBind();
                ShowActionNote();

                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
                rGrid.ShowHeaderWhenEmpty = false;
                rGrid.EmptyDataText = string.Empty;
                rGrid.ShowActionRow = false;


                ddlRoles.DataSource = Rock.Security.Role.AllRoles();
                ddlRoles.DataValueField = "Guid";
                ddlRoles.DataTextField = "Name";
                ddlRoles.DataBind();

                string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('#{0} td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this role/user?');
            }});
        $('#modal-popup div.modal-header h3 small', window.parent.document).html('{1}');
    }});
", rGrid.ClientID, iSecured.ToString() );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( iSecured.Authorized( "Configure", CurrentUser ) )
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

        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            List<Rock.CMS.Auth> rules = authService.GetAuths( iSecured.AuthEntity, iSecured.Id, CurrentAction ).ToList();
            authService.Reorder( rules, e.OldIndex, e.NewIndex, CurrentPersonId );

            Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, CurrentAction );

            BindGrid();
        }

        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Rock.Security.AuthRule authRule = ( Rock.Security.AuthRule )e.Row.DataItem;
                RadioButtonList rbl = ( RadioButtonList )e.Row.FindControl( "rblAllowDeny" );
                rbl.SelectedValue = authRule.AllowOrDeny;
            }
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.Auth auth = authService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( auth != null )
            {
                authService.Delete( auth, CurrentPersonId );
                authService.Save( auth, CurrentPersonId );

                Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, CurrentAction );
            }

            BindGrid();
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        protected void lbAction_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                CurrentAction = lb.Text;

                rptActions.DataSource = iSecured.SupportedActions;
                rptActions.DataBind();

                ShowActionNote();
                SetRoleActions();
            }

            BindGrid();
        }

        protected void rblAllowDeny_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rblAllowDeny = ( RadioButtonList )sender;
            GridViewRow selectedRow = rblAllowDeny.NamingContainer as GridViewRow;
            if ( selectedRow != null )
            {
                int id = ( int )rGrid.DataKeys[selectedRow.RowIndex]["id"];

                Rock.CMS.Auth auth = authService.Get( id );
                if ( auth != null )
                {
                    auth.AllowOrDeny = rblAllowDeny.SelectedValue;
                    authService.Save( auth, CurrentPersonId );

                    Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, CurrentAction );
                }
            }

            BindGrid();
        }

        protected void lbShowRole_Click( object sender, EventArgs e )
        {
            SetRoleActions();
            pnlActions.Visible = false;
            pnlAddRole.Visible = true;
        }

        protected void lbShowUser_Click( object sender, EventArgs e )
        {
            pnlActions.Visible = false;
            pnlAddUser.Visible = true;
        }

        protected void lbCancelAdd_Click( object sender, EventArgs e )
        {
            pnlAddRole.Visible = false;
            pnlAddUser.Visible = false;
            pnlActions.Visible = true;
        }

        protected void lbAddAllUsers_Click( object sender, EventArgs e )
        {
            List<Rock.Security.AuthRule> existingAuths =
                Rock.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, CurrentAction );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : 0;

            bool actionUpdated = false;

            bool alreadyExists = false;

            foreach ( Rock.Security.AuthRule auth in existingAuths )
                if ( auth.UserOrRole == "U" && auth.UserOrRoleName == "*" )
                {
                    alreadyExists = true;
                    break;
                }

            if ( !alreadyExists )
            {
                Rock.CMS.Auth auth = new Rock.CMS.Auth();
                auth.EntityType = iSecured.AuthEntity;
                auth.EntityId = iSecured.Id;
                auth.Action = CurrentAction;
                auth.AllowOrDeny = "A";
                auth.UserOrRole = "U";
                auth.UserOrRoleName = "*";
                auth.Order = ++maxOrder;
                authService.Add( auth, CurrentPersonId );
                authService.Save( auth, CurrentPersonId );

                actionUpdated = true;
            }

            if ( actionUpdated )
                Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, CurrentAction );

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

                    foreach ( Rock.Security.AuthRule rule in
                        Rock.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, li.Text ) )
                    {
                        if ( rule.UserOrRole == "R" && rule.UserOrRoleName == ddlRoles.SelectedValue )
                            alreadyExists = true;
                        if (rule.Order > maxOrder)
                            maxOrder = rule.Order;
                    }

                    if ( !alreadyExists )
                    {
                        Rock.CMS.Auth auth = new Rock.CMS.Auth();
                        auth.EntityType = iSecured.AuthEntity;
                        auth.EntityId = iSecured.Id;
                        auth.Action = li.Text;
                        auth.AllowOrDeny = "A";
                        auth.UserOrRole = "R";
                        auth.UserOrRoleName = ddlRoles.SelectedValue;
                        auth.Order = ++maxOrder;
                        authService.Add( auth, CurrentPersonId );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                    if ( actionUpdated )
                        Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, li.Text );
                }
            }

            pnlAddRole.Visible = false;
            pnlActions.Visible = true;

            BindGrid();
        }

        protected void lbUserSearch_Click( object sender, EventArgs e )
        {
            cbUsers.DataTextField = "FullName";
            cbUsers.DataValueField = "Guid";
            cbUsers.DataSource = new Rock.CRM.PersonService().GetByFullName( tbUser.Text ).ToList();
            cbUsers.DataBind();
        }

        protected void lbAddUser_Click( object sender, EventArgs e )
        {
            List<Rock.Security.AuthRule> existingAuths =
                Rock.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, CurrentAction );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : 0;

            bool actionUpdated = false;

            foreach ( ListItem li in cbUsers.Items )
            {
                if ( li.Selected )
                {
                    bool alreadyExists = false;

                    foreach ( Rock.Security.AuthRule auth in existingAuths )
                        if ( auth.UserOrRole == "U" && auth.UserOrRoleName == li.Value )
                        {
                            alreadyExists = true;
                            break;
                        }

                    if ( !alreadyExists )
                    {
                        Rock.CMS.Auth auth = new Rock.CMS.Auth();
                        auth.EntityType = iSecured.AuthEntity;
                        auth.EntityId = iSecured.Id;
                        auth.Action = CurrentAction;
                        auth.AllowOrDeny = "A";
                        auth.UserOrRole = "U";
                        auth.UserOrRoleName = li.Value;
                        auth.Order = ++maxOrder;
                        authService.Add( auth, CurrentPersonId );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                }
            }

            if ( actionUpdated )
                Rock.Security.Authorization.ReloadAction( iSecured.AuthEntity, iSecured.Id, CurrentAction );

            pnlAddUser.Visible = false;
            pnlActions.Visible = true;

            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                rGrid.DataSource = Rock.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, CurrentAction );
                rGrid.DataBind();
            }
        }

        protected string GetTabClass( object action )
        {
            if ( action.ToString() == CurrentAction )
                return "active";
            else
                return "";
        }

        private void ShowActionNote()
        {
            bool defaultAction = iSecured.DefaultAuthorization( CurrentAction );
            lActionNote.Text = string.Format( "By default, all users <span class='label {0}'>{1}</span> be authorized to {2}.",
                ( defaultAction ? "success" : "important" ), ( defaultAction ? "WILL" : "WILL NOT" ), CurrentAction );
            
        }

        private void SetRoleActions()
        {
            cblRoleActionList.Items.Clear();

            foreach ( string action in iSecured.SupportedActions )
            {
                if ( action == CurrentAction )
                {
                    ListItem roleItem = new ListItem( action );
                    roleItem.Selected = true;
                    cblRoleActionList.Items.Add( roleItem );
                }
                else
                {
                    bool alreadyAdded = false;
                    foreach ( Rock.Security.AuthRule rule in
                        Rock.Security.Authorization.AuthRules( iSecured.AuthEntity, iSecured.Id, action ) )
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