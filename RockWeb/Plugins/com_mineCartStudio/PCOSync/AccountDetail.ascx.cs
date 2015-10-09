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
using System.Web.UI.WebControls;

using com.minecartstudio.PCOSync;
using com.minecartstudio.PCOSync.Model;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
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

    [LinkedPage( "Application Group Detail Page")]
    public partial class AccountDetail : RockBlock, IDetailBlock
    {

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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Account.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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

            Account account = null;
            AccountService accountService = new AccountService( rockContext );

            int accountId = hfAccountId.ValueAsInt();
            if ( accountId > 0 )
            {
                account = accountService.Get( accountId );
            }

            if ( account == null )
            {
                account = new Account();
                accountService.Add( account );
            }

            account.Name = tbName.Text;
            account.ApplicationId = tbApplicationId.Text;
            account.Secret = tbSecret.Text;
            account.AllowPermissionDowngrade = cbPermissionDowngrade.Checked;
            account.AdministratorGroupId = ddlAdministrators.SelectedValueAsInt();
            account.EditorGroupId = ddlEditors.SelectedValueAsInt();
            account.SchedulerGroupId = ddlSchedulers.SelectedValueAsInt();
            account.ViewerGroupId = ddlViewers.SelectedValueAsInt();
            account.ScheduledViewerGroupId = ddlScheduledViewers.SelectedValueAsInt();

            if ( !account.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            ShowReadonlyDetails( account );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            AccountService accountService = new AccountService( new RockContext() );
            Account account = accountService.Get( hfAccountId.ValueAsInt() );
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
            AccountService accountService = new AccountService( rockContext );
            Account account = accountService.Get( int.Parse( hfAccountId.Value ) );

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
                AccountService accountService = new AccountService(new RockContext());
                Account account = accountService.Get( hfAccountId.ValueAsInt() );
                ShowReadonlyDetails( account );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowReadonlyDetails( Account account )
        {
            SetEditMode( false );

            lTitle.Text = account.Name.FormatAsHtmlTitle();

            var leftDetails = new DescriptionList();
            leftDetails.Add( "Administrators", ApplicationGroupAnchorTag( account.AdministratorGroup ) );
            leftDetails.Add( "Editors", ApplicationGroupAnchorTag( account.EditorGroup ) );
            leftDetails.Add( "Schedulers", ApplicationGroupAnchorTag( account.SchedulerGroup ) );
            leftDetails.Add( "Viewers", ApplicationGroupAnchorTag( account.ViewerGroup ) );
            leftDetails.Add( "Scheduled Viewers", ApplicationGroupAnchorTag( account.ScheduledViewerGroup ) );
            lblLeftDetails.Text = leftDetails.Html;

            var rightDetails = new DescriptionList();
            lblRightDetails.Text = rightDetails.Html;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="account">Type of the defined.</param>
        private void ShowEditDetails( Account account )
        {

            // Load dropdowns
            ddlAdministrators.Items.Clear();
            ddlEditors.Items.Clear();
            ddlSchedulers.Items.Clear();
            ddlViewers.Items.Clear();
            ddlScheduledViewers.Items.Clear();

            ddlAdministrators.Items.Add( new ListItem() );
            ddlEditors.Items.Add( new ListItem() );
            ddlSchedulers.Items.Add( new ListItem() );
            ddlViewers.Items.Add( new ListItem() );
            ddlScheduledViewers.Items.Add( new ListItem() );

            Guid groupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP.AsGuid();
            var applicationGroups = new GroupService( new RockContext() )
                .Queryable()
                .Where( g => g.GroupType.Guid.Equals( groupTypeGuid ) )
                .OrderBy( t => t.Name );

            if ( applicationGroups.Any() )
            {
                foreach ( var group in applicationGroups )
                {
                    ddlAdministrators.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    ddlEditors.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    ddlSchedulers.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    ddlViewers.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                    ddlScheduledViewers.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
                }
            }

            if ( account.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( Account.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( Account.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = account.Name;
            tbApplicationId.Text = account.ApplicationId;
            tbSecret.Text = account.Secret;
            cbPermissionDowngrade.Checked = account.AllowPermissionDowngrade;
            ddlAdministrators.SetValue( account.AdministratorGroupId );
            ddlEditors.SetValue( account.EditorGroupId );
            ddlSchedulers.SetValue( account.SchedulerGroupId );
            ddlViewers.SetValue( account.ViewerGroupId );
            ddlScheduledViewers.SetValue( account.ScheduledViewerGroupId );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="accountId">The defined type identifier.</param>
        public void ShowDetail( int accountId )
        {
            pnlDetails.Visible = true;
            Account account = null;

            if ( !accountId.Equals( 0 ) )
            {
                account = new AccountService( new RockContext() ).Get( accountId );
            }

            if ( account == null )
            {
                account = new Account { Id = 0 };
            }

            hfAccountId.SetValue( account.Id );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !UserCanEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Account.FriendlyTypeName );
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
                btnDelete.Visible = false;
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
                
        private string ApplicationGroupAnchorTag( Group group )
        {
            if ( group == null )
            {
                return string.Empty;
            }

            var qryParams = new Dictionary<string, string> { { "GroupId", group.Id.ToString() }};
            var pageRef = new PageReference( GetAttributeValue( "ApplicationGroupDetailPage" ), qryParams );
            string url = pageRef.BuildUrl();
            return string.Format( "<a href='{0}'>{1}</a>", url, group.Name );
        }

        #endregion

    }
}