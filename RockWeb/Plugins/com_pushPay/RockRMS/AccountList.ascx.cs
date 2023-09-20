//
// Copyright (C) Spark Development Network - All Rights Reserved
//
using com.pushpay.RockRMS;
using com.pushpay.RockRMS.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

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
        private const string GET_STARTED_URL = "https://echurch.com/partners/rockrms/";
        private const string PROMOTION_IMAGE_URL = "https://storage.rockrms.com/pushpay/splash-banner-inapp.png";

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

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                // Check if returning from Pushpay authorization
                if ( !String.IsNullOrWhiteSpace( Request.QueryString["code"] ) &&
                    !String.IsNullOrWhiteSpace( Request.QueryString["state"] ) )
                {
                    int? accountId = Request.QueryString["State"].AsIntegerOrNull();
                    if ( accountId.HasValue )
                    {
                        string errorMessage = string.Empty;
                        if ( PushpayApi.Authenticate( accountId.Value, Request, Request.QueryString["code"], out errorMessage ) )
                        {
                            // Redirect back to self without the code and state parameters so that breadcrumb navigation does not re-authorize
                            var pageRef = new Rock.Web.PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId, CurrentPageReference.Parameters );
                            NavigateToPage( pageRef );
                        }
                        else
                        {
                            ShowMessage( "Error Authenticating", errorMessage, NotificationBoxType.Danger );
                            BindGrid();
                        }
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
                account.AuthorizationRedirectUrl = uri.Scheme + "://" + uri.GetComponents( UriComponents.Host, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );

                account.IsActive = true;
                account.ActiveDate = RockDateTime.Today;
                account.DownloadSettledTransactionsOnly = true;

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
            tbName.Text = GlobalAttributesCache.Value( "OrganizationName" );
            tbAuthorizationUrl.Text = "https://auth.pushpay.com/pushpay/oauth/";
            tbApiUrl.Text = "https://api.pushpay.com";
            tbClientId.Text = string.Empty;
            tbClientSecret.Text = string.Empty;
            tbEventRegistrationRedirectToken.Text = string.Empty;

            Uri uri = new Uri( Request.Url.ToString() );
            tbAuthorizationRedirectUrl.Text = uri.Scheme + "://" + uri.GetComponents( UriComponents.Host, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );

            cbActive.Checked = true;
            dpActiveDate.SelectedDate = RockDateTime.Today;
            cbDownloadSettledOnly.Checked = false;

            ShowBatchSettings( new BatchSettings() );

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
        /// Handles the Refresh event of the gAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAccounts_Refresh( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var account = new AccountService( rockContext ).Get( e.RowKeyId );
                if ( account != null )
                {
                    if ( Account.RefreshMerchants( account.Id ) )
                    {
                        BindGrid();
                        ShowMessage( "Settings Updated",
                            string.Format( "The merchant listings and funds have been refreshed for the <strong>{0}</strong> Pushpay account.", account.Name ),
                            NotificationBoxType.Success );
                    }
                    else
                    {
                        ShowMessage( "Settings Could Not Be Updated",
                            string.Format( "The merchant listings could not be refreshed for the <strong>{0}</strong> Pushpay account. If there was an error that occurred, it would be listed in the exception log", account.Name ),
                            NotificationBoxType.Danger );
                    }
                }
            }
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
                    tbEventRegistrationRedirectToken.Text = account.EventRegistrationRedirectToken;

                    if ( !string.IsNullOrWhiteSpace( account.AuthorizationRedirectUrl ))
                    {
                        tbAuthorizationRedirectUrl.Text = account.AuthorizationRedirectUrl;
                    }
                    else
                    {
                        Uri uri = new Uri( Request.Url.ToString() );
                        tbAuthorizationRedirectUrl.Text = uri.Scheme + "://" + uri.GetComponents( UriComponents.Host, UriFormat.UriEscaped ) + ResolveRockUrl( "~/pushpayredirect" );
                    }

                    cbActive.Checked = account.IsActive;
                    dpActiveDate.SelectedDate = account.ActiveDate;
                    cbDownloadSettledOnly.Checked = account.DownloadSettledTransactionsOnly ?? true;
                    ShowBatchSettings( account.BatchSettings );

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

                bool reAuthRequired = false;

                if ( account == null )
                {
                    account = new Account();
                    accountService.Add( account );
                    reAuthRequired = true;
                }
                else
                {
                    reAuthRequired =
                        !account.AuthorizationUrl.Equals( tbAuthorizationUrl.Text.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                        !account.ApiUrl.Equals( tbApiUrl.Text.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                        !account.ClientId.Equals( tbClientId.Text.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                        !Encryption.DecryptString( account.ClientSecretEncrypted ).Equals( tbClientSecret.Text.Trim() ) ||
                        !account.AuthorizationRedirectUrl.Equals( tbAuthorizationRedirectUrl.Text.Trim(), StringComparison.OrdinalIgnoreCase );
                }
                account.Name = tbName.Text;
                account.AuthorizationUrl = tbAuthorizationUrl.Text.Trim();
                account.ApiUrl = tbApiUrl.Text.Trim();
                account.ClientId = tbClientId.Text.Trim();
                account.ClientSecretEncrypted = Encryption.EncryptString( tbClientSecret.Text.Trim() );
                account.EventRegistrationRedirectToken = tbEventRegistrationRedirectToken.Text.Trim();
                account.AuthorizationRedirectUrl = tbAuthorizationRedirectUrl.Text.Trim();
                account.IsActive = cbActive.Checked;
                account.ActiveDate = dpActiveDate.SelectedDate;
                account.DownloadSettledTransactionsOnly = cbDownloadSettledOnly.Checked;
                account.BatchSettings = GetBatchSettings();

                rockContext.SaveChanges();

                mdEditAccountSettings.Hide();

                // If the account is active and any api settings changed, re-authorize when saved
                if ( account.IsActive && reAuthRequired )
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
        /// Get the URL Scheme, respecting forward headers for reverse proxies.
        /// </summary>
        /// <returns></returns>
        private string GetScheme()
        {
            Uri uri = new Uri( Request.Url.ToString() );
            var scheme = uri.Scheme.ToLower();
            foreach ( var key in Request.Headers.AllKeys )
            {
                var headerKey = key.ToLower();
                if ( headerKey == "forwarded" )
                {
                    // If RFC 7239 header is present, use that.  https://datatracker.ietf.org/doc/html/rfc7239
                    var headerParameters = Request.Headers[headerKey].Split( ';' );
                    foreach ( var headerParameter in headerParameters )
                    {
                        var paramName = headerParameter.Trim().Split( '=' )[0].ToLower();
                        if ( paramName.ToLower() == "proto" && headerParameter.Contains( '=' ) )
                        {
                            scheme = headerParameter.Trim().Split('=')[1].ToLower();
                            break; // break here, becase this header should be given priority if it exists.
                        }
                    }
                }
                else if ( headerKey == "x-forwarded-proto" || headerKey == "x-forwarded-protocol" )
                {
                    scheme = Request.Headers[headerKey].ToLower();
                }
            }

            return scheme;
        }

        /// <summary>
        /// Get the URL Host and Port, respecting forward headers for reverse proxies.
        /// </summary>
        /// <returns></returns>
        private string GetRequestHostAndPort()
        {
            Uri uri = new Uri( Request.Url.ToString() );
            var hostAndPort = uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped );
            var host = uri.GetComponents( UriComponents.Host, UriFormat.UriEscaped );
            var port = uri.GetComponents( UriComponents.Port, UriFormat.UriEscaped );

            foreach ( var key in Request.Headers.AllKeys )
            {
                var headerKey = key.ToLower();
                bool isPortSet = false;
                if ( headerKey == "forwarded" )
                {
                    // If RFC 7239 header is present, use that.  https://datatracker.ietf.org/doc/html/rfc7239
                    var headerParameters = Request.Headers[headerKey].Split( ';' );
                    bool isHostSet = false;
                    foreach ( var headerParameter in headerParameters )
                    {
                        var paramName = headerParameter.Trim().Split( '=' )[0].ToLower();
                        if ( paramName == "for" && headerParameter.Contains( '=' ) )
                        {
                            var forParameters = headerParameter.Split( '=' )[1].Split( ',' );
                            // if there is more than one "for" listed, then the first value
                            // should be the original client (browser) and the second value
                            // should be the first reverse proxy server, which is where we
                            // want to direct any requests.
                            if ( forParameters.Length > 1 )
                            {
                                isPortSet = true;
                                var forValues = forParameters[1].Split( ':' );
                                if ( !isHostSet )
                                {
                                    // do not override host if there's an explicit value provided.
                                    host = forValues[0].Trim();
                                }

                                if ( forValues.Length > 1 )
                                {
                                    port = forValues[1];
                                }
                            }

                        }
                        else if ( !isPortSet && paramName == "by" && headerParameter.Contains( '=' ) )
                        {
                            // we're only interested in the "by" parameter if we have not
                            // already obtained a value from the "for" parameter (other than
                            // the original client's address).
                            var byValues = headerParameter.Split( '=' )[1].Split( ':' );

                            if ( !isHostSet )
                            {
                                // do not override host if there's an explicit value provided.
                                host = byValues[0];
                            }
                            
                            if ( byValues.Length > 1 )
                            {
                                port = byValues[1];
                            }

                        }
                        else if ( paramName == "host" && headerParameter.Contains( '=' ) )
                        {
                            // if a "host" parameter has been provided, we should assume that
                            // is the correct host value and not use the ip address of "for"
                            // or "by" parameters.
                            isHostSet = true;
                            host = headerParameter.Split( '=' )[1];
                        }
                    }

                    hostAndPort = host + ":" + port;
                    break; // stop processing other headers.
                }
                else if ( headerKey == "x-forwarded-host" || headerKey == "x-original-host" )
                {
                    host = Request.Headers[headerKey];
                    if ( !isPortSet )
                    {
                        port = "443"; // assume SSL, since it's required for OAuth.
                    }
                    hostAndPort = host + ":" + port;
                }
                else if ( headerKey == "x-forwarded-port" )
                {
                    isPortSet = true;
                    port = Request.Headers[headerKey];
                    hostAndPort = host + ":" + port;
                }
            }

            if ( port == "443" )
            {
                // don't need to include the port in the URL if it's the standard SSL port.
                return host;
            }

            return hostAndPort;
        }
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
                    var scheme = GetScheme();
                    if ( scheme == "https" )
                    {
                        Uri uri = new Uri( Request.Url.ToString() );
                        string apiReturnUrl = scheme + "://"
                            + GetRequestHostAndPort()
                            + ResolveRockUrl( "~/pushpayredirect" );

                        imgPromotion.ImageUrl = PROMOTION_IMAGE_URL;
                        hlRequestAPI.NavigateUrl = string.Format( API_REQUEST_URL, apiReturnUrl );
                        hlGetStarted.NavigateUrl = GET_STARTED_URL;

                        pnlNew.Visible = true;
                        pnlAccounts.Visible = false;
                    }
                    else
                    {
                        pnlAccounts.Visible = false;
                        pnlNew.Visible = false;
                        ShowMessage( "Invalid Configuration",
                            "Your Rock instance must be configured for SSL encryption before you can authorize access to your Pushpay account.  Please access this page with SSL enabled to proceed.",
                            NotificationBoxType.Danger );
                    }
                }
            }
        }

        private void ShowBatchSettings( BatchSettings batchSettings )
        {
            cbUseTransactionDate.Checked = batchSettings.UseTransactionDate;
            cbUseCampus.Checked = batchSettings.UseCampus;
            cbIncludeCurrencyType.Checked = batchSettings.IncludeCurrencyType;
            cbMoveUpdatedTxns.Checked = batchSettings.MoveUpdatedTxns;
        }

        private BatchSettings GetBatchSettings()
        {
            var batchSettings = new BatchSettings();

            batchSettings.UseTransactionDate = cbUseTransactionDate.Checked;
            batchSettings.UseCampus = cbUseCampus.Checked;
            batchSettings.IncludeCurrencyType =cbIncludeCurrencyType.Checked;
            batchSettings.MoveUpdatedTxns =cbMoveUpdatedTxns.Checked;

            return batchSettings;
        }

        private void ShowMessage( string title, string message, NotificationBoxType messageType )
        {
            nbMessage.Title = title;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = messageType;
            nbMessage.Visible = true;
        }

        #endregion

    }
}