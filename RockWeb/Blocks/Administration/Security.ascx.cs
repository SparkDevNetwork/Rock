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
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class Security : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Rock.Model.AuthService authService = new Rock.Model.AuthService();
        private ISecured iSecured;

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
            string entityParam = PageParameter( "EntityTypeId" );
            Type type = null;

            // Get Entity Type
            int entityTypeId = 0;
            if ( Int32.TryParse( entityParam, out entityTypeId ) )
            {
                var entityType = EntityTypeCache.Read( entityTypeId );
                if ( entityType != null )
                {
                    entityParam = entityType.FriendlyName;
                    type = entityType.GetEntityType();
                }
            }

            // Get Entity Id
            int entityId = 0;
            if ( !Int32.TryParse( PageParameter( "EntityId" ), out entityId ) )
            {
                entityId = 0;
            }

            // Get object type
            if ( type != null )
            {
                if ( entityId == 0 )
                {
                    iSecured = (ISecured)Activator.CreateInstance( type );
                }
                else
                {
                    Type serviceType = typeof( Rock.Data.Service<> );
                    Type[] modelType = { type };
                    Type service = serviceType.MakeGenericType( modelType );
                    var serviceInstance = Activator.CreateInstance( service );
                    var getMethod = service.GetMethod( "Get", new Type[] { typeof( int ) } );
                    iSecured = getMethod.Invoke( serviceInstance, new object[] { entityId } ) as ISecured;
                }

                var block = iSecured as Rock.Model.Block;
                if ( block != null )
                {
                    // If the entity is a block, get the cachedblock's supported action, as the RockPage may have
                    // added additional actions when the cache was created.
                    foreach ( var action in BlockCache.Read( block.Id, CurrentPage.SiteId ).SupportedActions )
                    {
                        if ( !block.SupportedActions.Contains( action ) )
                        {
                            block.SupportedActions.Add( action );
                        }
                    }

                    iSecured = block;
                }

                if ( iSecured != null && iSecured.IsAuthorized( "Administrate", CurrentPerson ) )
                {
                    rptActions.DataSource = iSecured.SupportedActions;
                    rptActions.DataBind();

                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                    rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
                    rGrid.RowDataBound += new GridViewRowEventHandler( rGrid_RowDataBound );
                    rGrid.ShowHeaderWhenEmpty = false;
                    rGrid.EmptyDataText = string.Empty;
                    rGrid.ShowActionRow = false;

                    rGridParentRules.DataKeyNames = new string[] { "id" };
                    rGridParentRules.ShowHeaderWhenEmpty = false;
                    rGridParentRules.EmptyDataText = string.Empty;
                    rGridParentRules.ShowActionRow = false;

                    BindRoles();

                    string script = string.Format( @"
                    Sys.Application.add_load(function () {{
                        $('#modal-popup div.modal-header h3 small', window.parent.document).html('{0}');
                    }});
                ", iSecured.ToString() );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "set-html-{0}", this.ClientID ), script, true );

                }
            }
            else
            {
                rGrid.Visible = false;
                rGridParentRules.Visible = false;
                nbMessage.Text = string.Format( "Could not load the requested entity type ('{0}') to determine security attributes", entityParam );
                nbMessage.Visible = true;
            }
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( iSecured.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                    BindGrid();
            }
            else
            {
                rGrid.Visible = false;
                rGridParentRules.Visible = false;
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
            int entityTypeId = iSecured.TypeId;

            List<Rock.Model.Auth> rules = authService.GetAuths( iSecured.TypeId, iSecured.Id, CurrentAction ).ToList();
            authService.Reorder( rules, e.OldIndex, e.NewIndex, CurrentPersonId );

            Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );

            BindGrid();
        }

        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                AuthRule authRule = (AuthRule)e.Row.DataItem;
                RadioButtonList rbl = (RadioButtonList)e.Row.FindControl( "rblAllowDeny" );
                rbl.SelectedValue = authRule.AllowOrDeny;
            }
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Model.Auth auth = authService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );
            if ( auth != null )
            {
                authService.Delete( auth, CurrentPersonId );
                authService.Save( auth, CurrentPersonId );

                Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );
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

                SetRoleActions();
            }

            BindGrid();
        }

        protected void rblAllowDeny_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rblAllowDeny = (RadioButtonList)sender;
            GridViewRow selectedRow = rblAllowDeny.NamingContainer as GridViewRow;
            if ( selectedRow != null )
            {
                int id = (int)rGrid.DataKeys[selectedRow.RowIndex]["id"];

                Rock.Model.Auth auth = authService.Get( id );
                if ( auth != null )
                {
                    auth.AllowOrDeny = rblAllowDeny.SelectedValue;
                    authService.Save( auth, CurrentPersonId );

                    Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );
                }
            }

            BindGrid();
        }

        protected void lbShowRole_Click( object sender, EventArgs e )
        {
            SetRoleActions();
            phList.Visible = false;
            pnlAddRole.Visible = true;
        }

        protected void lbShowUser_Click( object sender, EventArgs e )
        {
            phList.Visible = false;
            pnlAddUser.Visible = true;
        }

        protected void lbCancelAdd_Click( object sender, EventArgs e )
        {
            pnlAddRole.Visible = false;
            pnlAddUser.Visible = false;
            phList.Visible = true;
        }

        protected void ddlRoles_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetRoleActions();
        }

        protected void lbAddRole_Click( object sender, EventArgs e )
        {
            List<AuthRule> existingAuths =
                Authorization.AuthRules( iSecured.TypeId, iSecured.Id, CurrentAction );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : -1;

            foreach ( ListItem li in cblRoleActionList.Items )
            {
                if ( li.Selected )
                {
                    bool actionUpdated = false;
                    bool alreadyExists = false;

                    Rock.Model.SpecialRole specialRole = Rock.Model.SpecialRole.None;
                    int? groupId = Int32.Parse( ddlRoles.SelectedValue );

                    switch ( groupId )
                    {
                        case -1: specialRole = Rock.Model.SpecialRole.AllUsers; break;
                        case -2: specialRole = Rock.Model.SpecialRole.AllAuthenticatedUsers; break;
                        case -3: specialRole = Rock.Model.SpecialRole.AllUnAuthenticatedUsers; break;
                        default: specialRole = Rock.Model.SpecialRole.None; break;
                    }

                    if ( groupId < 0 )
                        groupId = null;

                    foreach ( AuthRule rule in
                        Authorization.AuthRules( iSecured.TypeId, iSecured.Id, li.Text ) )
                    {
                        if ( rule.SpecialRole == specialRole && rule.GroupId == groupId )
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if ( !alreadyExists )
                    {
                        Rock.Model.Auth auth = new Rock.Model.Auth();
                        auth.EntityTypeId = iSecured.TypeId;
                        auth.EntityId = iSecured.Id;
                        auth.Action = li.Text;
                        auth.AllowOrDeny = "A";
                        auth.SpecialRole = specialRole;
                        auth.GroupId = groupId;
                        auth.Order = ++maxOrder;
                        authService.Add( auth, CurrentPersonId );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                    if ( actionUpdated )
                        Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, li.Text );
                }
            }

            pnlAddRole.Visible = false;
            phList.Visible = true;

            BindGrid();
        }

        protected void lbUserSearch_Click( object sender, EventArgs e )
        {
            cbUsers.DataTextField = "FullName";
            cbUsers.DataValueField = "Id";
            cbUsers.DataSource = new Rock.Model.PersonService().GetByFullName( tbUser.Text ).ToList();
            cbUsers.DataBind();
        }

        protected void lbAddUser_Click( object sender, EventArgs e )
        {
            List<AuthRule> existingAuths =
                Authorization.AuthRules( iSecured.TypeId, iSecured.Id, CurrentAction );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : -1;

            bool actionUpdated = false;

            foreach ( ListItem li in cbUsers.Items )
            {
                if ( li.Selected )
                {
                    bool alreadyExists = false;

                    int personId = Int32.Parse( li.Value );

                    foreach ( AuthRule auth in existingAuths )
                        if ( auth.PersonId.HasValue && auth.PersonId.Value == personId )
                        {
                            alreadyExists = true;
                            break;
                        }

                    if ( !alreadyExists )
                    {
                        Rock.Model.Auth auth = new Rock.Model.Auth();
                        auth.EntityTypeId = iSecured.TypeId;
                        auth.EntityId = iSecured.Id;
                        auth.Action = CurrentAction;
                        auth.AllowOrDeny = "A";
                        auth.SpecialRole = Rock.Model.SpecialRole.None;
                        auth.PersonId = personId;
                        auth.Order = ++maxOrder;
                        authService.Add( auth, CurrentPersonId );
                        authService.Save( auth, CurrentPersonId );

                        actionUpdated = true;
                    }

                }
            }

            if ( actionUpdated )
                Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );

            pnlAddUser.Visible = false;
            phList.Visible = true;

            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            var itemRules = Authorization.AuthRules( iSecured.TypeId, iSecured.Id, CurrentAction );
            rGrid.DataSource = itemRules;
            rGrid.DataBind();

            var parentRules = new List<MyAuthRule>();
            AddParentRules( itemRules, parentRules, iSecured.ParentAuthority, CurrentAction );
            rGridParentRules.DataSource = parentRules;
            rGridParentRules.DataBind();
        }

        private void AddParentRules( List<AuthRule> itemRules, List<MyAuthRule> parentRules, ISecured parent, string action )
        {
            if ( parent != null )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Read( parent.TypeId );
                foreach ( AuthRule rule in Authorization.AuthRules( parent.TypeId, parent.Id, action ) )
                    if ( !itemRules.Exists( r =>
                            r.SpecialRole == rule.SpecialRole &&
                            r.PersonId == rule.PersonId &&
                            r.GroupId == rule.GroupId ) &&
                        !parentRules.Exists( r => 
                            r.SpecialRole == rule.SpecialRole &&
                            r.PersonId == rule.PersonId &&
                            r.GroupId == rule.GroupId ) )
                    {
                        var myRule = new MyAuthRule( rule );
                        myRule.EntityTitle = string.Format( "{0} ({1})", parent.ToString(), entityType.FriendlyName ?? entityType.Name ).TrimStart();
                        parentRules.Add( myRule );
                    }

                AddParentRules( itemRules, parentRules, parent.ParentAuthority, action );
            }
        }

        private void BindRoles()
        {
            ddlRoles.Items.Clear();

            ddlRoles.Items.Add( new ListItem( "[All Users]", "-1" ) );
            ddlRoles.Items.Add( new ListItem( "[All Authenticated Users]", "-2" ) );
            ddlRoles.Items.Add( new ListItem( "[All Un-Authenticated Users]", "-3" ) );

            foreach ( var role in Role.AllRoles() )
                ddlRoles.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
        }

        protected string GetTabClass( object action )
        {
            if ( action.ToString() == CurrentAction )
                return "active";
            else
                return "";
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

                    Rock.Model.SpecialRole specialRole = Rock.Model.SpecialRole.None;
                    int? groupId = Int32.Parse( ddlRoles.SelectedValue );

                    switch ( groupId )
                    {
                        case -1: specialRole = Rock.Model.SpecialRole.AllUsers; break;
                        case -2: specialRole = Rock.Model.SpecialRole.AllAuthenticatedUsers; break;
                        case -3: specialRole = Rock.Model.SpecialRole.AllUnAuthenticatedUsers; break;
                        default: specialRole = Rock.Model.SpecialRole.None; break;
                    }

                    if ( groupId < 0 )
                        groupId = null;

                    foreach ( AuthRule rule in Authorization.AuthRules( iSecured.TypeId, iSecured.Id, action ) )
                    {
                        if ( rule.SpecialRole == specialRole && rule.GroupId == groupId )
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

    class MyAuthRule : AuthRule
    {
        public string EntityTitle { get; set; }

        public MyAuthRule( AuthRule rule )
            : base( rule.Id, rule.EntityId, rule.AllowOrDeny, rule.SpecialRole, rule.PersonId, rule.GroupId, rule.Order )
        {
        }
    }
}