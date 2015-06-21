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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Security" )]
    [Category( "Administration" )]
    [Description( "Displays security settings for a specific entity." )]
    public partial class Security : RockBlock
    {
        #region Fields

        private ISecured iSecured;

        /// <summary>
        /// Gets or sets the current action.
        /// </summary>
        /// <value>
        /// The current action.
        /// </value>
        protected string CurrentAction
        {
            get
            {
                object currentAction = ViewState["CurrentAction"];
                return currentAction != null ? currentAction.ToString() : Authorization.VIEW;
            }

            set
            {
                ViewState["CurrentAction"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            int? entityTypeId = PageParameter( "EntityTypeId" ).AsIntegerOrNull();
            string entityTypeName = string.Empty;
            Type type = null;

            // Get Entity Type
            if ( entityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( entityTypeId.Value );
                if ( entityType != null )
                {
                    entityTypeName = entityType.FriendlyName;
                    type = entityType.GetEntityType();
                }
            }

            // Get Entity Id
            int entityId = PageParameter( "EntityId" ).AsIntegerOrNull() ?? 0;

            // Get object type
            if ( type != null )
            {
                if ( entityId == 0 )
                {
                    iSecured = (ISecured)Activator.CreateInstance( type );
                }
                else
                {
                    // Get the context type since this may be for a non-rock core object
                    Type contextType = null;
                    var contexts = Rock.Reflection.SearchAssembly( type.Assembly, typeof( Rock.Data.DbContext ) );
                    if ( contexts.Any() )
                    {
                        contextType = contexts.First().Value;
                    }
                    else
                    {
                        contextType = typeof( RockContext );
                    }

                    Type serviceType = typeof( Rock.Data.Service<> );
                    Type[] modelType = { type };
                    Type service = serviceType.MakeGenericType( modelType );
                    var getMethod = service.GetMethod( "Get", new Type[] { typeof( int ) } );

                    var context = Activator.CreateInstance( contextType );
                    var serviceInstance = Activator.CreateInstance( service, new object[] { context } );
                    iSecured = getMethod.Invoke( serviceInstance, new object[] { entityId } ) as ISecured;
                }

                var block = iSecured as Rock.Model.Block;
                if ( block != null )
                {
                    // If the entity is a block, get any actions that were updated or added by the block type using
                    // one or more SecurityActionAttributes.
                    foreach ( var action in BlockCache.Read( block.Id ).BlockType.SecurityActions )
                    {
                        if ( block.SupportedActions.ContainsKey( action.Key ) )
                        {
                            block.SupportedActions[action.Key] = action.Value;
                        }
                        else
                        {
                            block.SupportedActions.Add( action.Key, action.Value );
                        }
                    }

                    iSecured = block;
                }

                if ( iSecured != null )
                {
                    if ( iSecured.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                    {
                        if ( iSecured.SupportedActions.Any() )
                        {
                            lActionDescription.Text = iSecured.SupportedActions.FirstOrDefault().Value;
                        }

                        rptActions.DataSource = iSecured.SupportedActions;
                        rptActions.DataBind();

                        rGrid.DataKeyNames = new string[] { "Id" };
                        rGrid.GridReorder += new GridReorderEventHandler( rGrid_GridReorder );
                        rGrid.GridRebind += new GridRebindEventHandler( rGrid_GridRebind );
                        rGrid.RowDataBound += new GridViewRowEventHandler( rGrid_RowDataBound );
                        rGrid.ShowHeaderWhenEmpty = false;
                        rGrid.EmptyDataText = string.Empty;
                        rGrid.ShowActionRow = false;

                        rGridParentRules.DataKeyNames = new string[] { "Id" };
                        rGridParentRules.ShowHeaderWhenEmpty = false;
                        rGridParentRules.EmptyDataText = string.Empty;
                        rGridParentRules.ShowActionRow = false;

                        BindRoles();

                        string scriptFormat = @"
                    Sys.Application.add_load(function () {{
                        $('#modal-popup div.modal-header h3 small', window.parent.document).html('{0}');
                    }});
                ";
                        string script = string.Format( scriptFormat, HttpUtility.JavaScriptStringEncode( iSecured.ToString() ) );

                        this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "set-html-{0}", this.ClientID ), script, true );
                    }
                    else
                    {
                        nbMessage.Text = "Unfortunately, you are not able to edit security because you do not belong to a role that has been configured to allow administration of this item.";
                    }
                }
                else
                {
                    nbMessage.Text = "The item you are trying to secure does not exist or does not implement ISecured.";
                }
            }
            else
            {
                nbMessage.Text = string.Format( "The requested entity type ('{0}') could not be loaded to determine security attributes.", entityTypeName );
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( iSecured != null && iSecured.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                }
            }
            else
            {
                divActions.Visible = false;
                divContent.Visible = false;
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        #region Grid Events

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            int entityTypeId = iSecured.TypeId;

            var rockContext = new RockContext();
            var authService = new Rock.Model.AuthService( rockContext );
            List<Rock.Model.Auth> rules = authService.GetAuths( iSecured.TypeId, iSecured.Id, CurrentAction ).ToList();
            authService.Reorder( rules, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                AuthRule authRule = (AuthRule)e.Row.DataItem;
                RadioButtonList rbl = (RadioButtonList)e.Row.FindControl( "rblAllowDeny" );
                rbl.SelectedValue = authRule.AllowOrDeny;
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var authService = new Rock.Model.AuthService( rockContext );
            Rock.Model.Auth auth = authService.Get( e.RowKeyId );
            if ( auth != null )
            {
                authService.Delete( auth );
                rockContext.SaveChanges();

                Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the lbAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblAllowDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblAllowDeny_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rblAllowDeny = (RadioButtonList)sender;
            GridViewRow selectedRow = rblAllowDeny.NamingContainer as GridViewRow;
            if ( selectedRow != null )
            {
                int id = (int)rGrid.DataKeys[selectedRow.RowIndex]["Id"];

                var rockContext = new RockContext();
                var authService = new Rock.Model.AuthService( rockContext );
                Rock.Model.Auth auth = authService.Get( id );
                if ( auth != null )
                {
                    auth.AllowOrDeny = rblAllowDeny.SelectedValue;
                    rockContext.SaveChanges();

                    Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbShowRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowRole_Click( object sender, EventArgs e )
        {
            SetRoleActions();
            phList.Visible = false;
            pnlAddRole.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbShowUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowUser_Click( object sender, EventArgs e )
        {
            phList.Visible = false;
            pnlAddUser.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbCancelAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelAdd_Click( object sender, EventArgs e )
        {
            pnlAddRole.Visible = false;
            pnlAddUser.Visible = false;
            phList.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRoles_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetRoleActions();
        }

        /// <summary>
        /// Handles the Click event of the lbAddRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                    int? groupId = ddlRoles.SelectedValue.AsIntegerOrNull();

                    switch ( groupId )
                    {
                        case -1: specialRole = Rock.Model.SpecialRole.AllUsers;
                            break;
                        case -2: specialRole = Rock.Model.SpecialRole.AllAuthenticatedUsers;
                            break;
                        case -3: specialRole = Rock.Model.SpecialRole.AllUnAuthenticatedUsers;
                            break;
                        default: specialRole = Rock.Model.SpecialRole.None;
                            break;
                    }

                    if ( groupId < 0 )
                    {
                        groupId = null;
                    }

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
                        var rockContext = new RockContext();
                        var authService = new Rock.Model.AuthService( rockContext );

                        Rock.Model.Auth auth = new Rock.Model.Auth();
                        auth.EntityTypeId = iSecured.TypeId;
                        auth.EntityId = iSecured.Id;
                        auth.Action = li.Text;
                        auth.AllowOrDeny = "A";
                        auth.SpecialRole = specialRole;
                        auth.GroupId = groupId;
                        auth.Order = ++maxOrder;
                        authService.Add( auth );

                        rockContext.SaveChanges();

                        actionUpdated = true;
                    }

                    if ( actionUpdated )
                    {
                        Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, li.Text );
                    }
                }
            }

            pnlAddRole.Visible = false;
            phList.Visible = true;

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbAddUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddUser_Click( object sender, EventArgs e )
        {
            List<AuthRule> existingAuths =
                Authorization.AuthRules( iSecured.TypeId, iSecured.Id, CurrentAction );

            int maxOrder = existingAuths.Count > 0 ? existingAuths.Last().Order : -1;

            bool actionUpdated = false;

            if ( ppUser.PersonId.HasValue )
            {
                var rockContext = new RockContext();

                int? personAliasId = ppUser.PersonAliasId;
                if ( personAliasId.HasValue )
                {
                    bool alreadyExists = false;

                    foreach ( AuthRule auth in existingAuths )
                    {
                        if ( auth.PersonAliasId.HasValue && auth.PersonAliasId.Equals( personAliasId.Value ) )
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if ( !alreadyExists )
                    {
                        var authService = new Rock.Model.AuthService( rockContext );

                        Rock.Model.Auth auth = new Rock.Model.Auth();
                        auth.EntityTypeId = iSecured.TypeId;
                        auth.EntityId = iSecured.Id;
                        auth.Action = CurrentAction;
                        auth.AllowOrDeny = "A";
                        auth.SpecialRole = Rock.Model.SpecialRole.None;
                        auth.PersonAliasId = personAliasId;
                        auth.Order = ++maxOrder;
                        authService.Add( auth );

                        rockContext.SaveChanges();

                        actionUpdated = true;
                    }
                }
            }

            if ( actionUpdated )
            {
                Authorization.ReloadAction( iSecured.TypeId, iSecured.Id, CurrentAction );
            }

            pnlAddUser.Visible = false;
            phList.Visible = true;

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var itemRules = Authorization.AuthRules( iSecured.TypeId, iSecured.Id, CurrentAction );
            rGrid.DataSource = itemRules;
            rGrid.DataBind();

            var parentRules = new List<MyAuthRule>();
            AddParentRules( itemRules, parentRules, iSecured.ParentAuthorityPre, CurrentAction );
            AddParentRules( itemRules, parentRules, iSecured.ParentAuthority, CurrentAction );
            rGridParentRules.DataSource = parentRules;
            rGridParentRules.DataBind();
        }

        /// <summary>
        /// Adds the parent rules.
        /// </summary>
        /// <param name="itemRules">The item rules.</param>
        /// <param name="parentRules">The parent rules.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="action">The action.</param>
        private void AddParentRules( List<AuthRule> itemRules, List<MyAuthRule> parentRules, ISecured parent, string action )
        {
            if ( parent != null )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Read( parent.TypeId );
                foreach ( AuthRule rule in Authorization.AuthRules( parent.TypeId, parent.Id, action ) )
                {
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
                        myRule.EntityTitle = string.Format( "{0} <small>({1})</small>", parent.ToString(), entityType.FriendlyName ?? entityType.Name ).TrimStart();
                        parentRules.Add( myRule );
                    }
                }

                AddParentRules( itemRules, parentRules, parent.ParentAuthority, action );
            }
        }

        /// <summary>
        /// Binds the roles.
        /// </summary>
        private void BindRoles()
        {
            ddlRoles.Items.Clear();

            ddlRoles.Items.Add( new ListItem( "[All Users]", "-1" ) );
            ddlRoles.Items.Add( new ListItem( "[All Authenticated Users]", "-2" ) );
            ddlRoles.Items.Add( new ListItem( "[All Un-Authenticated Users]", "-3" ) );

            var securityRoleType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );
            
            foreach ( var role in Role.AllRoles() )
            {
                string name = role.IsSecurityTypeGroup ? role.Name : "GROUP - " + role.Name;
                ddlRoles.Items.Add( new ListItem( name, role.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Gets the tab class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        protected string GetTabClass( object action )
        {
            if ( action.ToString() == CurrentAction )
            {
                return "active";
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets the role actions.
        /// </summary>
        private void SetRoleActions()
        {
            cblRoleActionList.Items.Clear();

            foreach ( var action in iSecured.SupportedActions )
            {
                if ( action.Key == CurrentAction )
                {
                    lActionDescription.Text = action.Value;

                    ListItem roleItem = new ListItem( action.Key );
                    roleItem.Selected = true;
                    cblRoleActionList.Items.Add( roleItem );
                }
                else
                {
                    bool alreadyAdded = false;

                    Rock.Model.SpecialRole specialRole = Rock.Model.SpecialRole.None;
                    int? groupId = ddlRoles.SelectedValue.AsIntegerOrNull();

                    switch ( groupId )
                    {
                        case -1: specialRole = Rock.Model.SpecialRole.AllUsers;
                            break;
                        case -2: specialRole = Rock.Model.SpecialRole.AllAuthenticatedUsers;
                            break;
                        case -3: specialRole = Rock.Model.SpecialRole.AllUnAuthenticatedUsers;
                            break;
                        default: specialRole = Rock.Model.SpecialRole.None;
                            break;
                    }

                    if ( groupId < 0 )
                    {
                        groupId = null;
                    }

                    foreach ( AuthRule rule in Authorization.AuthRules( iSecured.TypeId, iSecured.Id, action.Key ) )
                    {
                        if ( rule.SpecialRole == specialRole && rule.GroupId == groupId )
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if ( !alreadyAdded )
                    {
                        cblRoleActionList.Items.Add( new ListItem( action.Key ) );
                    }
                }
            }
        }

        #endregion
    }

    #region MyAuthRule class

    /// <summary>
    /// 
    /// </summary>
    public class MyAuthRule : AuthRule
    {
        /// <summary>
        /// Gets or sets the entity title.
        /// </summary>
        /// <value>
        /// The entity title.
        /// </value>
        public string EntityTitle { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAuthRule"/> class.
        /// </summary>
        /// <param name="rule">The rule.</param>
        public MyAuthRule( AuthRule rule )
            : base( rule.Id, rule.EntityId, rule.AllowOrDeny, rule.SpecialRole, rule.PersonId, rule.PersonAliasId, rule.GroupId, rule.Order )
        {
        }
    }

    #endregion
}