//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using com.pushpay.RockRMS;
using com.pushpay.RockRMS.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_pushPay.RockRMS
{
    /// <summary>
    /// Block used to edit the Pushpay accounts.
    /// </summary>
    [DisplayName( "Account List" )]
    [Category( "Pushpay" )]
    [Description( "Block used to edit Pushpay accounts." )]

    [LinkedPage( "Merchant List Page", "Page used to display the Pushpay Merchant Listings for a given account.", true, "", "", 0 )]
    public partial class AccountList : Rock.Web.UI.RockBlock
    {
        private const string API_REQUEST_URL = "mailto:care@echurchgiving.com?subject=Requesting API Client Id and Secret for Rock-Pushpay plugin&body=We have a Pushpay account and we are installing the Pushpay plugin for Rock. The setup wizard is asking us for API Client Id and Secret. The Return Url to our Rock installation is {0}.";
        private const string GET_STARTED_URL = "http://echurchgiving.com/go/integration-rock";
        private const string PROMOTION_IMAGE_URL = "http://storage.rockrms.com/pushpay/splash-banner-inapp.png";

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAccounts.DataKeyNames = new string[] { "Id" };
            gAccounts.Actions.ShowAdd = true;
            gAccounts.Actions.AddClick += gAccounts_Add;
            gAccounts.GridRebind += gAccounts_GridRebind;
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
                // Check if returning from Pushpay authorization
                if ( !String.IsNullOrWhiteSpace( Request.QueryString["code"] ) &&
                    !String.IsNullOrWhiteSpace( Request.QueryString["state"] ) )
                {
                    int? accountId = Request.QueryString["State"].AsIntegerOrNull();
                    if ( accountId.HasValue )
                    {
                        PushpayApi.Authenticate( accountId.Value, Request, Request.QueryString["code"] );

                        // Redirect back to self without the code and state parameters so that breadcrumb navigation does not re-authorize
                        var pageRef = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, CurrentPageReference.Parameters );
                        NavigateToPage( pageRef );
                    }
                }
                else
                {
                    BindGrid();
                }
            }
        }

        #endregion

        #region Events

        protected void lbSaveNew_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var accountService = new AccountService( rockContext );
                Account account = new Account();
                accountService.Add( account );

                account.Name = GlobalAttributesCache.Value( "OrganizationName" );
                account.AuthorizationUrl = "https://auth.pushpay.com/pushpay/oauth/";
                account.ApiUrl = "https://api.pushpay.com";
                account.ClientId = tbNewClientId.Text.Trim();
                account.ClientSecretEncrypted = Encryption.EncryptString( tbNewClientSecret.Text.Trim() );

                Uri uri = new Uri( Request.Url.ToString() );
                account.AuthorizationRedirectUrl = "https://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );

                account.IsActive = true;

                rockContext.SaveChanges();

                uri = PushpayApi.GetAuthorizationUri( account, Request );
                if ( uri != null )
                {
                    Response.Redirect( uri.AbsoluteUri, false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAccounts_Add( object sender, EventArgs e )
        {
            hfAccountId.Value = string.Empty;
            tbName.Text = GlobalAttributesCache.Value("OrganizationName");
            tbAuthorizationUrl.Text = "https://auth.pushpay.com/pushpay/oauth/";
            tbApiUrl.Text = "https://api.pushpay.com";
            tbClientId.Text = string.Empty;
            tbClientSecret.Text = string.Empty;

            Uri uri = new Uri( Request.Url.ToString() );
            tbAuthorizationRedirectUrl.Text = "https://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );

            cbActive.Checked = true;

            liSettings.AddCssClass( "active" );
            liAdvancedSettings.RemoveCssClass( "active" );
            divSettings.AddCssClass( "active" );
            divAdvancedSettings.RemoveCssClass( "active" );

            mdEditAccountSettings.Show();
        }

        /// <summary>
        /// Handles the RowSelected event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccounts_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "MerchantListPage", new Dictionary<string, string> { { "AccountId", e.RowKeyId.ToString() } } );
        }

        /// <summary>
        /// Handles the Edit event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccounts_Edit( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var account = new AccountService( rockContext ).Get( e.RowKeyId );
                if ( account != null )
                {
                    hfAccountId.Value = account.Id.ToString();
                    tbName.Text = account.Name;
                    tbAuthorizationUrl.Text = account.AuthorizationUrl;
                    tbApiUrl.Text = account.ApiUrl;
                    tbClientId.Text = account.ClientId;
                    tbClientSecret.Text = Encryption.DecryptString( account.ClientSecretEncrypted );

                    if ( !string.IsNullOrWhiteSpace( account.AuthorizationRedirectUrl ))
                    {
                        tbAuthorizationRedirectUrl.Text = account.AuthorizationRedirectUrl;
                    }
                    else
                    {
                        Uri uri = new Uri( Request.Url.ToString() );
                        tbAuthorizationRedirectUrl.Text = "https://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );
                    }

                    cbActive.Checked = account.IsActive;

                    liSettings.AddCssClass( "active" );
                    liAdvancedSettings.RemoveCssClass( "active" );
                    divSettings.AddCssClass( "active" );
                    divAdvancedSettings.RemoveCssClass( "active" );

                    mdEditAccountSettings.Show();
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccounts_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var accountService = new AccountService( rockContext );
                var account = accountService.Get( e.RowKeyId );
                if ( account != null )
                {
                    accountService.Delete( account );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAccounts_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void mdEditAccountSettings_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var accountService = new AccountService( rockContext );
                Account account = null;

                int? accountId = hfAccountId.Value.AsIntegerOrNull();
                if ( accountId.HasValue )
                {
                    account = accountService.Get( accountId.Value );
                }

                if ( account == null )
                {
                    account = new Account();
                    accountService.Add( account );
                }

                account.Name = tbName.Text;
                account.AuthorizationUrl = tbAuthorizationUrl.Text.Trim();
                account.ApiUrl = tbApiUrl.Text.Trim();
                account.ClientId = tbClientId.Text.Trim();
                account.ClientSecretEncrypted = Encryption.EncryptString( tbClientSecret.Text.Trim() );
                account.AuthorizationRedirectUrl = tbAuthorizationRedirectUrl.Text.Trim();
                account.IsActive = cbActive.Checked;

                rockContext.SaveChanges();

                mdEditAccountSettings.Hide();

                // If the account is active, re-authorize when saved
                if ( account.IsActive )
                {
                    var uri = PushpayApi.GetAuthorizationUri( account, Request );
                    if ( uri != null )
                    {
                        Response.Redirect( uri.AbsoluteUri, false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
                else
                {
                    BindGrid();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new AccountService( rockContext );
                var qry = service.Queryable().AsNoTracking();

                var sortProperty = gAccounts.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderBy( c => c.Name );
                }

                var accountList = qry.ToList();

                if ( accountList.Any() )
                {
                    pnlNew.Visible = false;
                    pnlAccounts.Visible = true;
                    gAccounts.DataSource = qry.ToList();
                    gAccounts.DataBind();
                }
                else
                {
                    Uri uri = new Uri( Request.Url.ToString() );
                    string apiReturnUrl = "https://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );

                    imgPromotion.ImageUrl = PROMOTION_IMAGE_URL;
                    hlRequestAPI.NavigateUrl = string.Format( API_REQUEST_URL, apiReturnUrl );
                    hlGetStarted.NavigateUrl = GET_STARTED_URL;

                    pnlNew.Visible = true;
                    pnlAccounts.Visible = false;
                }
            }
        }

        #endregion



}
}