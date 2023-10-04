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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using rocks.pillars.PCOSync;
using rocks.pillars.PCOSync.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.rocks_pillars.PCOSync
{
    /// <summary>
    /// Displays the details of the given PCO Account.
    /// </summary>
    [DisplayName( "Account Detail" )]
    [Category( "Pillars > PCO Sync" )]
    [Description( "Displays the details of the given PCO Account." )]

    [LinkedPage( "Group Detail Page" )]
    public partial class AccountDetail : RockBlock
    {
        const string EDIT_MODE = "EDIT";
        const string VIEW_MODE = "VIEW";
        const string ADD_MODE = "ADD";

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSettings );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", PCOAccount.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
			//Set server timeout to 60 minutes
			Server.ScriptTimeout = 3600; Server.ScriptTimeout = 3600;
			ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 3600;

			base.OnLoad( e );

            nbAdd.Visible = false;

            if ( !Page.IsPostBack )
            {
                int? itemId = PageParameter( "accountId" ).AsIntegerOrNull();
                if ( itemId.HasValue )
                {
                    ShowDetail( itemId.Value );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                if ( pnlAdd.Visible )
                {
                    BuildAddControls();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int? itemId = PageParameter( "accountId" ).AsIntegerOrNull();
            if ( itemId.HasValue )
            {
                ShowDetail( itemId.Value );
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            PCOAccount account = null;
            PCOAccountService accountService = new PCOAccountService( rockContext );

            int accountId = hfAccountId.ValueAsInt();
            if ( accountId > 0 )
            {
                account = accountService.Get( accountId );
            }

            if ( account == null )
            {
                account = new PCOAccount();
                accountService.Add( account );
            }

            account.Name = tbName.Text;
            account.ApplicationId = tbApplicationId.Text;
            account.Secret = tbSecret.Text;

            ListItem li = ddlWelcomeEmailTemplate.SelectedItem;
            account.WelcomeEmailTemplateId = li != null ? li.Value.AsIntegerOrNull() : (int?)null;
            account.WelcomeEmailTemplate = li != null ? li.Text : "";

            account.AllowPermissionDowngrade = cbPermissionDowngrade.Checked;

            account.MapFirstAndNickName = rblNameMapping.SelectedValue.AsBooleanOrNull() ?? false;

            var syncFields = cblAddlFields.SelectedValues;
            account.SyncMiddleName = syncFields.Contains( "middle_name" );
            account.SyncGender = syncFields.Contains( "gender" );
            account.SyncBirthDate = syncFields.Contains( "birthdate" );
            account.SyncAnniversary = syncFields.Contains( "anniversary" );
            account.SyncHomeAddress = syncFields.Contains( "home_address" );
            account.SyncHomePhone = syncFields.Contains( "home_phone" );
            account.SyncWorkPhone = syncFields.Contains( "work_phone" );
            account.SyncMobilePhone = syncFields.Contains( "mobile_phone" );
            account.SyncPhoto = syncFields.Contains( "photo_date" );

            if ( !account.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            // Refresh Tags
            account = accountService.Get( account.Id );
            hfAccountId.Value = account.Id.ToString();

            var api = new PCOApi( account, false );
            api.SyncTags();

            ShowReadonlyDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            PCOAccountService accountService = new PCOAccountService( new RockContext() );
            PCOAccount account = accountService.Get( hfAccountId.ValueAsInt() );
            ShowEditDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            PCOAccountService accountService = new PCOAccountService( rockContext );
            PCOAccount account = accountService.Get( int.Parse( hfAccountId.Value ) );

            if ( account != null )
            {
                if ( !UserCanEdit ) 
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this Account.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !accountService.CanDelete( account, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( account );

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfAccountId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                PCOAccountService accountService = new PCOAccountService(new RockContext());
                PCOAccount account = accountService.Get( hfAccountId.ValueAsInt() );
                ShowReadonlyDetails( account );
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the tbToken control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tbToken_TextChanged( object sender, EventArgs e )
        {
            RefreshEmailTemplateList();
        }


        /// <summary>
        /// Handles the Click event of the btnImportPCO control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportPCO_Click( object sender, EventArgs e )
        {
            BuildAddControls();
            SetViewMode( ADD_MODE );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveGroups_Click( object sender, EventArgs e )
        {
            SaveGroups();

            PCOAccountService accountService = new PCOAccountService( new RockContext() );
            PCOAccount account = accountService.Get( hfAccountId.ValueAsInt() );
            ShowReadonlyDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelImport_Click( object sender, EventArgs e )
        {
            PCOAccountService accountService = new PCOAccountService( new RockContext() );
            PCOAccount account = accountService.Get( hfAccountId.ValueAsInt() );
            ShowReadonlyDetails( account );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowReadonlyDetails( PCOAccount account )
        {
            SetViewMode( VIEW_MODE );

            lTitle.Text = account.Name.FormatAsHtmlTitle();

            var details = new DescriptionList();
            details.Add( "Welcome Email", account.WelcomeEmailTemplate );

            using ( var rockContext = new RockContext() )
            {
                var groupPermissions = account.GetSyncedGroups( rockContext );
                if ( groupPermissions != null )
                {
                    var groupList = new Dictionary<string, string>();

                    var permissionsType = DefinedTypeCache.Get( rocks.pillars.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid(), rockContext );

                    var groupService = new GroupService( rockContext );

                    foreach ( var keyVal in groupPermissions )
                    {
                        var groupPath = groupService.GroupAncestorPathName( keyVal.Key );
                        if ( !string.IsNullOrWhiteSpace( groupPath ) )
                        {
                            var qryParams = new Dictionary<string, string> { { "GroupId", keyVal.Key.ToString() } };
                            var url = LinkedPageUrl( "GroupDetailPage", qryParams );
                            groupPath = string.Format( "<a href='{0}'>{1}</a>", url, groupPath );
                            groupList.Add( groupPath, GetSecurityLabel( permissionsType.DefinedValues.FirstOrDefault( v => v.Guid == keyVal.Value ) ) );
                        }
                    }

                    details.Add( "Groups Syncing to this Account", 
                        groupList
                            .OrderBy( l => l.Key )
                            .Select( l => string.Format( "<small>{0} {1}</small>", l.Key, l.Value ) )
                            .ToList()
                            .AsDelimited( "<br/>") );
                }
            }

            lblDetails.Text = details.Html;
        }

        private string GetSecurityLabel( DefinedValueCache permission )
        {
            if ( permission != null )
            {
                string labelType = permission.GetAttributeValue( "LabelType" );
                return string.Format( "<span class='label label-{0}'>{1}</span>",
                    !string.IsNullOrWhiteSpace( labelType ) ? labelType : "default", permission.Value );
            }
            return string.Empty;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowEditDetails( PCOAccount account )
        {
            if ( account.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( PCOAccount.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( PCOAccount.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetViewMode( EDIT_MODE );

            tbName.Text = account.Name;
            cbPermissionDowngrade.Checked = account.AllowPermissionDowngrade;
            rblNameMapping.SetValue( account.MapFirstAndNickName.ToString() );

            var syncFields = new List<string>();
            if ( account.SyncMiddleName ) { syncFields.Add( "middle_name" ); }
            if ( account.SyncGender ) { syncFields.Add( "gender" ); }
            if ( account.SyncBirthDate ) { syncFields.Add( "birthdate" ); }
            if ( account.SyncAnniversary ) { syncFields.Add( "anniversary" ); }
            if ( account.SyncHomeAddress ) { syncFields.Add( "home_address" ); }
            if ( account.SyncHomePhone ) { syncFields.Add( "home_phone" ); }
            if ( account.SyncWorkPhone ) { syncFields.Add( "work_phone" ); }
            if ( account.SyncMobilePhone ) { syncFields.Add( "mobile_phone" ); }
            if ( account.SyncPhoto ) { syncFields.Add( "photo_date" ); }
            cblAddlFields.SetValues( syncFields );

            tbApplicationId.Text = account.ApplicationId;
            tbSecret.Text = account.Secret;

            RefreshEmailTemplateList();
            ddlWelcomeEmailTemplate.SetValue( account.WelcomeEmailTemplateId );
        }

        /// <summary>
        /// Sets the view mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        private void SetViewMode( string mode )
        {
            pnlEditDetails.Visible = mode == EDIT_MODE;
            vsDetails.Enabled = mode == EDIT_MODE;
            fieldsetViewDetails.Visible = mode == VIEW_MODE;
            pnlAdd.Visible = mode == ADD_MODE;

            this.HideSecondaryBlocks( mode != VIEW_MODE );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="accountId">The defined type identifier.</param>
        public void ShowDetail( int accountId )
        {
            pnlDetails.Visible = true;
            PCOAccount account = null;

            if ( !accountId.Equals( 0 ) )
            {
                account = new PCOAccountService( new RockContext() ).Get( accountId );
            }

            if ( account == null )
            {
                account = new PCOAccount { Id = 0 };
            }

            hfAccountId.SetValue( account.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !UserCanEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PCOAccount.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( account );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                if ( account.Id > 0 )
                {
                    ShowReadonlyDetails( account );
                }
                else
                {
                    ShowEditDetails( account );
                }
            }
        }

        /// <summary>
        /// Refreshes the email template list.
        /// </summary>
        private void RefreshEmailTemplateList()
        {
            int? currentSelection = ddlWelcomeEmailTemplate.SelectedValueAsInt();
            ddlWelcomeEmailTemplate.SelectedIndex = -1;
            ddlWelcomeEmailTemplate.Items.Clear();

            string appId = tbApplicationId.Text;
            string secret = tbSecret.Text;

            if ( !string.IsNullOrWhiteSpace( appId ) && !string.IsNullOrWhiteSpace( secret ) )
            {
                var account = new PCOAccount { ApplicationId = tbApplicationId.Text, Secret = tbSecret.Text };
                var api = new PCOApi( account, false );
                ddlWelcomeEmailTemplate.DataSource = api.GetEmailTemplates();
                ddlWelcomeEmailTemplate.DataBind();

                ddlWelcomeEmailTemplate.Items.Insert( 0, new ListItem() );

                ddlWelcomeEmailTemplate.SetValue( currentSelection );
            }
        }

        /// <summary>
        /// Builds the add controls.
        /// </summary>
        private void BuildAddControls()
        {
            phAddGroups.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var accountService = new PCOAccountService( rockContext );
                var account = accountService.Get( hfAccountId.ValueAsInt() );
                var permissionDefinedType = DefinedTypeCache.Get( rocks.pillars.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );

                if ( account != null && permissionDefinedType != null )
                {
                    var attrValueService = new AttributeValueService( rockContext );
                    var groupService = new GroupService( rockContext );

                    // Get the group ids with a PCO account value equal to this account
                    var attributeValue = account.Guid.ToString();
                    var accountGroupIds = attrValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute.Key == "PCOAccount" &&
                            v.Value == attributeValue )
                        .Select( v => v.EntityId )
                        .ToList();

                    // Get all the permission level attributes for those groups
                    var permissionLevelValues = attrValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute.Key == "PCOPermissionLevel" &&
                            accountGroupIds.Contains( v.EntityId ) )
                        .ToList();

                    foreach ( var dv in permissionDefinedType.DefinedValues )
                    {
                        var div = new System.Web.UI.HtmlControls.HtmlGenericControl( "div" );
                        div.AddCssClass( "col-sm-6" );
                        phAddGroups.Controls.Add( div );

                        var ddlGroup = new RockDropDownList();
                        ddlGroup.ID = string.Format( "ddlGroup_{0}", dv.Id );
                        ddlGroup.Label = dv.Value + " Group";
                        div.Controls.Add( ddlGroup );

                        var permissionGroupIds = permissionLevelValues
                            .Where( v => v.Value.ToLower() == dv.Guid.ToString().ToLower() )
                            .Select( v => v.EntityId )
                            .ToList();

                        ddlGroup.Items.Clear();
                        ddlGroup.Items.Add( new ListItem() );

                        var groups = new Dictionary<int, string>();
                        foreach ( var group in groupService
                            .Queryable().AsNoTracking()
                            .Where( g =>
                                permissionGroupIds.Contains( g.Id ) &&
                                g.IsActive )
                            .OrderBy( g => g.Name ) )
                        {
                            groups.Add( group.Id, groupService.GroupAncestorPathName( group.Id ) );
                        }

                        foreach ( var group in groups.OrderBy( g => g.Value ) )
                        {
                            var li = new ListItem( group.Value, group.Key.ToString() );
                            li.Selected = account.DefaultGroups.Any( g => g.PermissionLevelValueId == dv.Id && g.GroupId == group.Key );
                            ddlGroup.Items.Add( li );
                        }
                    }
                }

            }
        }

        private void SaveGroups()
        {
            using ( var rockContext = new RockContext() )
            {
                var accountService = new PCOAccountService( rockContext );
                var account = accountService.Get( hfAccountId.ValueAsInt() );
                var permissionDefinedType = DefinedTypeCache.Get( rocks.pillars.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );

                if ( account != null && permissionDefinedType != null )
                {
                    var selectedPermissionGroups = new Dictionary<int, int?>();

                    var attrValueService = new AttributeValueService( rockContext );
                    var groupService = new GroupService( rockContext );

                    foreach ( var dv in permissionDefinedType.DefinedValues )
                    {
                        var ddlGroup = phAddGroups.FindControl( string.Format( "ddlGroup_{0}", dv.Id ) ) as RockDropDownList;
                        if ( ddlGroup != null )
                        {
                            selectedPermissionGroups.Add( dv.Id, ddlGroup.SelectedValueAsInt() );
                        }
                    }

                    if ( selectedPermissionGroups.Any() )
                    {
                        var permissionIds = selectedPermissionGroups.Keys;
                        foreach ( var group in account.DefaultGroups.Where( g => !permissionIds.Contains( g.PermissionLevelValueId ) ) )
                        {
                            account.DefaultGroups.Remove( group );
                        }

                        foreach ( var selectedPermissionGroup in selectedPermissionGroups )
                        {
                            var permissionGroup = account.DefaultGroups.FirstOrDefault( g => g.PermissionLevelValueId == selectedPermissionGroup.Key );
                            if ( permissionGroup == null )
                            {
                                permissionGroup = new PCOAccountGroup();
                                permissionGroup.PermissionLevelValueId = selectedPermissionGroup.Key;
                                account.DefaultGroups.Add( permissionGroup );
                            }
                            permissionGroup.GroupId = selectedPermissionGroup.Value;
                        }

                        rockContext.SaveChanges();

                    }
                }
            }
        }

        #endregion

    }
}