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

using com.minecartstudio.PCOSync;
using com.minecartstudio.PCOSync.Model;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_mineCartStudio.PCOSync
{
    /// <summary>
    /// Displays the details of the given PCO Account.
    /// </summary>
    [DisplayName( "Account Detail" )]
    [Category( "Mine Cart Studio > PCO Sync" )]
    [Description( "Displays the details of the given PCO Account." )]

    [LinkedPage( "Group Detail Page" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status",
        "The connection status to use when people are added to Rock from PCO.", true, false,
        Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE, "", 2, "ConnectionStatus" )]
    public partial class AccountDetail : RockBlock, IDetailBlock
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

            //Set server timeout to 10 minutes
            Server.ScriptTimeout = 600; Server.ScriptTimeout = 600;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 600; ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 600;
            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
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

            if ( !account.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            // Refresh Tags
            account = accountService.Get( account.Id );
            var api = new PCOApi( account );
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
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            int newPeople = 0;
            int newAccountRecords = 0; 

            using ( var rockContext = new RockContext() )
            {
                var accountService = new PCOAccountService( rockContext );
                var accountPersonService = new PCOAccountPersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var personService = new PersonService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                var account = accountService.Get( hfAccountId.ValueAsInt() );
                var defaultConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                var permissionDefinedType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );

                if ( account != null && defaultConnectionStatus != null && permissionDefinedType != null )
                {
                    var permissionLevels = permissionDefinedType.DefinedValues.OrderByDescending( v => v.Order ).ToList();
                    var permissionGroups = new Dictionary<int, Group>();
                        
                    var syncedPCOIds = account.People
                        .Select( p => p.PCOId )
                        .ToList();

                    var api = new PCOApi( account );
                    var pcoPeople = api.GetPeople();

                    foreach ( var pcoPerson in pcoPeople
                        .Where( p =>
                            p.permissions != null &&
                            !syncedPCOIds.Contains( p.id ) )
                        .ToList() )
                    {
                        var permissionLevel = permissionLevels.Where( p => p.Value.ToLower() == pcoPerson.permissions.ToLower() ).FirstOrDefault();
                        if ( permissionLevel != null )
                        {
                            // Get the target Group
                            Group group = null;
                            if ( permissionGroups.ContainsKey( permissionLevel.Id ) )
                            {
                                group = permissionGroups[permissionLevel.Id];
                            }
                            else
                            {
                                if ( permissionLevel != null )
                                {
                                    var ddlGroup = phAddGroups.FindControl( string.Format( "ddlGroup_{0}", permissionLevel.Id ) ) as RockDropDownList;
                                    if ( ddlGroup != null )
                                    {
                                        int? groupId = ddlGroup.SelectedValueAsInt();
                                        if ( groupId.HasValue )
                                        {
                                            group = groupService.Get( groupId.Value );
                                        }
                                    }
                                    permissionGroups.Add( permissionLevel.Id, group );
                                }
                            }

                            // If a group was configured
                            if ( group != null && group.GroupType != null && group.GroupType.DefaultGroupRoleId.HasValue )
                            {
                                Person person = GetRockPerson( rockContext, personService, pcoPerson, defaultConnectionStatus );
                                if ( person != null )
                                {
                                    newAccountRecords++;

                                    if ( person.SystemNote == "JustAdded" )
                                    {
                                        newPeople++;
                                    }

                                    var pcoAccountPerson = new PCOAccountPerson();
                                    pcoAccountPerson.AccountId = account.Id;
                                    pcoAccountPerson.PersonAliasId = person.PrimaryAliasId.Value;
                                    pcoAccountPerson.PCOId = pcoPerson.id;
                                    pcoAccountPerson.PermissionLevelValueId = permissionLevel.Id;
                                    accountPersonService.Add( pcoAccountPerson );

                                    var groupMember = groupMemberService.GetByGroupIdAndPersonId( group.Id, person.Id ).FirstOrDefault();
                                    if ( groupMember == null )
                                    {
                                        groupMember = new GroupMember();
                                        groupMember.GroupId = group.Id;
                                        groupMember.PersonId = person.Id;
                                        groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                                        groupMemberService.Add( groupMember );
                                    }

                                    rockContext.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }

            nbAdd.Title = "Import Complete";
            nbAdd.Text = string.Format( "{0:N0} sync records added. {1:N0} new people created.", newAccountRecords, newPeople );
            nbAdd.Visible = true;
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

                    var permissionsType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid(), rockContext );

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
                var api = new PCOApi( account );
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

            var accountService = new PCOAccountService( new RockContext() );
            var account = accountService.Get( hfAccountId.ValueAsInt() );
            var permissionDefinedType = DefinedTypeCache.Read( com.minecartstudio.PCOSync.SystemGuid.DefinedType.PCO_PERMISSION_LEVELS.AsGuid() );

            if ( account != null && permissionDefinedType != null )
            {
                using ( var rockContext = new RockContext() )
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
                        foreach( var group in groupService
                            .Queryable().AsNoTracking()
                            .Where( g =>
                                permissionGroupIds.Contains( g.Id ) &&
                                g.IsActive )
                            .OrderBy( g => g.Name ) )
                        {
                            groups.Add( group.Id, groupService.GroupAncestorPathName( group.Id ) );
                        }
                        bool selected = true;
                        foreach( var group in groups.OrderBy( g => g.Value ))
                        {
                            var li = new ListItem( group.Value, group.Key.ToString() );
                            li.Selected = selected;
                            ddlGroup.Items.Add( li );
                            selected = false;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Gets the rock person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="personService">The person service.</param>
        /// <param name="pcoPerson">The pco person.</param>
        /// <param name="defaultConnectionStatus">The default connection status.</param>
        /// <returns></returns>
        private Person GetRockPerson( RockContext rockContext, PersonService personService, PCOPerson pcoPerson, DefinedValueCache defaultConnectionStatus )
        {
            string homeEmail = string.Empty;
            PCOEmailAddress email = pcoPerson.contact_data.email_addresses.Where( p => p.location == "Home" ).FirstOrDefault();
            if ( email != null )
            {
                homeEmail = email.address;
            }

            // If person had an email, get the first person with the same name and email address.
            if ( !string.IsNullOrWhiteSpace( homeEmail ) )
            {
                Person person = null;
                var people = personService.GetByMatch( pcoPerson.first_name, pcoPerson.last_name, homeEmail );
                if ( people.Count() == 1 )
                {
                    person = people.First();
                }

                if ( person == null )
                {
                    var personRecordTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusActive = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                    rockContext.WrapTransaction( () =>
                    {
                        person = new Person();
                        person.IsSystem = false;
                        person.RecordTypeValueId = personRecordTypeId;
                        person.RecordStatusValueId = personStatusActive;
                        person.ConnectionStatusValueId = defaultConnectionStatus != null ? defaultConnectionStatus.Id : (int?)null;
                        person.FirstName = pcoPerson.first_name;
                        person.LastName = pcoPerson.last_name;
                        person.Email = homeEmail;
                        person.IsEmailActive = true;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.SystemNote = "Added by PCO Sync";

                        PersonService.SaveNewPerson( person, rockContext, null, false );
                    } );

                    person = personService.Get( person.Id );
                    person.SystemNote = "JustAdded";
                }

                return person;
            }

            return null;
        }

        #endregion

    }
}