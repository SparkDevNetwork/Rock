// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Navigation Tree for accounts
    /// </summary>
    [DisplayName( "Account Tree View" )]
    [Category( "Finance" )]
    [Description( "Creates a navigation tree for accounts" )]

    [TextField( "Treeview Title", "Account Tree View", false, order: 1 )]
    [BooleanField( "Show Settings Panel", defaultValue: true, key: "ShowFilterOption", order: 2 )]
    [CustomDropdownListField( "Initial Active Setting", "Select whether to initially show all or just active accounts in the treeview", "0^All,1^Active", false, "1", "", 3 )]
    [LinkedPage( "Detail Page", order: 4 )]
    [LinkedPage( "Order Top-Level Page", key: "OrderTopLevelPage", order: 5 )]
    [BooleanField( "Use Public Name", "Determines if the public name to be displayed for accounts.", defaultValue: false, order: 6 )]
    [Rock.SystemGuid.BlockTypeGuid( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6" )]
    public partial class AccountTreeView : RockBlock
    {
        #region Fields

        private string _accountId = string.Empty;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _accountId = PageParameter( "AccountId" );

            var detailPageReference = new Rock.Web.PageReference( GetAttributeValue( "DetailPage" ) );

            // NOTE: if the detail page is the current page, use the current route instead of route specified in the DetailPage (to preserve old behavior)
            if ( detailPageReference == null || detailPageReference.PageId == this.RockPage.PageId )
            {
                hfPageRouteTemplate.Value = ( this.RockPage.RouteData.Route as System.Web.Routing.Route ).Url;
                hfDetailPageUrl.Value = new Rock.Web.PageReference( this.RockPage.PageId ).BuildUrl();
            }
            else
            {
                hfPageRouteTemplate.Value = string.Empty;
                var pageCache = PageCache.Get( detailPageReference.PageId );
                if ( pageCache != null )
                {
                    var route = pageCache.PageRoutes.FirstOrDefault( a => a.Id == detailPageReference.RouteId );
                    if ( route != null )
                    {
                        hfPageRouteTemplate.Value = route.Route;
                    }
                }

                hfDetailPageUrl.Value = detailPageReference.BuildUrl();
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upAccountType );

            pnlConfigPanel.Visible = this.GetAttributeValue( "ShowFilterOption" ).AsBooleanOrNull() ?? false;
            pnlRolloverConfig.Visible = this.GetAttributeValue( "ShowFilterOption" ).AsBooleanOrNull() ?? false;
            hfUsePublicName.Value = this.GetAttributeValue( "UsePublicName" ).AsBoolean( false ).ToTrueFalse();

            if ( pnlConfigPanel.Visible )
            {
                var typePreferences = GetBlockTypePersonPreferences();
                var hideInactiveAccounts = typePreferences.GetValue( "hide-inactive-accounts" ).AsBooleanOrNull();
                if ( !hideInactiveAccounts.HasValue )
                {
                    hideInactiveAccounts = this.GetAttributeValue( "InitialActiveSetting" ) == "1";
                }

                tglHideInactiveAccounts.Checked = hideInactiveAccounts ?? true;
            }
            else
            {
                // if the filter is hidden, don't show inactive accounts
                tglHideInactiveAccounts.Checked = true;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( string.IsNullOrWhiteSpace( _accountId ) )
                {
                    // If no account was selected, try to find the first account and redirect
                    // back to current page with that account selected
                    var account = FindFirstAccount();
                    {
                        if ( account != null )
                        {
                            _accountId = account.Id.ToString();
                            string redirectUrl = string.Empty;

                            // redirect so that the account treeview has the first node selected right away and account detail shows the account
                            if ( hfPageRouteTemplate.Value.IndexOf( "{accountId}", StringComparison.OrdinalIgnoreCase ) >= 0 )
                            {
                                redirectUrl = "~/" + hfPageRouteTemplate.Value.ReplaceCaseInsensitive( "{accountId}", _accountId.ToString() );
                            }
                            else
                            {
                                var queryStringSeperator = this.Request.QueryString.Count == 0 ? "?" : "&";
                                redirectUrl = $"{this.Request.UrlProxySafe()}{queryStringSeperator}AccountId={_accountId}";
                            }

                            this.Response.Redirect( redirectUrl, false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
            }

            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            bool canAddChildAccount = false;

            if ( !string.IsNullOrWhiteSpace( _accountId ) )
            {
                string key = string.Format( "Account:{0}", _accountId );
                var rockContext = new RockContext();
                FinancialAccount selectedAccount = RockPage.GetSharedItem( key ) as FinancialAccount;
                if ( selectedAccount == null )
                {
                    int id = _accountId.AsInteger();
                    selectedAccount = new FinancialAccountService( rockContext ).Get( id );
                    RockPage.SaveSharedItem( key, selectedAccount );
                }

                // get the parents of the selected item so we can tell the treeview to expand those
                List<string> parentIdList = new List<string>();


                // also get any additional expanded nodes that were sent in the Post
                string postedExpandedIds = this.Request.Params["ExpandedIds"];
                if ( !string.IsNullOrWhiteSpace( postedExpandedIds ) )
                {
                    var postedExpandedIdList = postedExpandedIds.Split( ',' ).ToList();
                    foreach ( var id in postedExpandedIdList )
                    {
                        if ( !parentIdList.Contains( id ) )
                        {
                            parentIdList.Add( id );
                        }
                    }
                }

                if ( selectedAccount != null )
                {
                    hfInitialAccountId.Value = selectedAccount.Id.ToString();
                    hfSelectedAccountId.Value = selectedAccount.Id.ToString();

                    canAddChildAccount = canEditBlock;

                    if ( !canAddChildAccount )
                    {
                        canAddChildAccount = selectedAccount.IsAuthorized( Authorization.EDIT, CurrentPerson );
                        if ( !canAddChildAccount )
                        {
                            foreach ( var childAccount in selectedAccount.ChildAccounts )
                            {
                                if ( childAccount != null && childAccount.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                                {
                                    canAddChildAccount = true;
                                    break;
                                }
                            }
                        }
                    }

                }

                hfInitialAccountParentIds.Value = parentIdList.AsDelimited( "," );
            }
            else
            {
                // let the Add button be visible if there is nothing selected (if authorized)
                lbAddAccountChild.Enabled = canEditBlock;
            }

            divTreeviewActions.Visible = canEditBlock || canAddChildAccount;
            lbAddAccountRoot.Enabled = canEditBlock;
            lbAddAccountChild.Enabled = canAddChildAccount;

            // disable add child account if no account is selected
            if ( hfSelectedAccountId.ValueAsInt() == 0 )
            {
                lbAddAccountChild.Enabled = false;
            }

            hfexcludeInactiveGroups.Value = ( tglHideInactiveAccounts.Checked ).ToTrueFalse();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddAccountRoot_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "AccountId", 0.ToString() );
            qryParams.Add( "ParentAccountId", 0.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialAccountParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddAccountChild_Click( object sender, EventArgs e )
        {
            int accountId = hfSelectedAccountId.ValueAsInt();

            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "AccountId", 0.ToString() );
            qryParams.Add( "ParentAccountId", accountId.ToString() );
            qryParams.Add( "ExpandedIds", hfInitialAccountParentIds.Value );

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbOrderTopLevelAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOrderTopLevelAccounts_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "TopLevel", "True" );
            NavigateToLinkedPage( "OrderTopLevelPage", queryParams );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the CheckedChanged event of the tglHideInactiveAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglHideInactiveAccounts_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockTypePersonPreferences();

            preferences.SetValue( "hide-inactive-accounts", tglHideInactiveAccounts.Checked.ToTrueFalse() );
            preferences.Save();

            // reload the whole page
            NavigateToPage( this.RockPage.Guid, new Dictionary<string, string>() );
        }

        /// <summary>
        /// Finds the first account.
        /// </summary>
        /// <returns></returns>
        private FinancialAccount FindFirstAccount()
        {
            var accounts = new FinancialAccountService( new RockContext() );

            var qry = accounts.GetChildren( 0, !tglHideInactiveAccounts.Checked );

            foreach ( var account in qry.OrderBy( g => g.Order ).ThenBy( g => g.Name ) )
            {
                // return first account they are authorized to view
                if ( account.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    return account;
                }
            }

            return null;
        }

        #endregion
    }
}