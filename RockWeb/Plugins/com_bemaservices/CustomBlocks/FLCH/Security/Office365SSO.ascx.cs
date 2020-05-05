using System;
using System.ComponentModel;
using System.Web;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using System.Net;

namespace RockWeb.Plugins.com_bemaservices.Security
{
    [DisplayName( "O365 Single Sign On" )]
    [Category( "BEMA Services > Security" )]
    [Description( "Automatically Authenticates Person against Office 365." )]
    [TextField( "Allowed Network", "The network to compare to in the format of '192.168.0.0/24'. See http://www.ipaddressguide.com/cidr for assistance in calculating CIDR addresses.", false)]
    [LinkedPage( "Successful Redirect Page", "Page to redirect user to upon successful login.", true, Rock.SystemGuid.Page.INTERNAL_HOMEPAGE )]
    [LinkedPage( "Alternative Login Page", "If SSO fails, you will be redirected to this page", true, Rock.SystemGuid.Page.LOGIN_SECURITY )]

    public partial class Office365SSO : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            if ( !Request.IsAuthenticated )
            {
                // Getting Authentication provider
                var authenticationProvider = AuthenticationContainer.GetComponent( "com.bemaservices.Security.Authentication.Office365.Office365" );

                // Checking if authentication provider was found and enabled
                if (authenticationProvider != null )
                {
                    var authorizationLink = authenticationProvider.GenerateLoginUrl( Request );

                    if ( !IsPostBack && authenticationProvider.IsReturningFromAuthentication( Request ) )
                    {
                        var userName = string.Empty;
                        var returnUrl = string.Empty;
                        string redirectUrlSetting = LinkedPageUrl( "SuccessfulRedirectPage" );
                        if ( authenticationProvider.Authenticate( Request, out userName, out returnUrl ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
                            {
                                CheckUser( userName, redirectUrlSetting, true );
                            }
                            else
                            {
                                CheckUser( userName, returnUrl, true );
                            }
                        }
                        else
                        {
                            // Logging exception to make troubleshooting easier
                            var excceptionText = string.Format( "Office 365 SSO - failed to login user: {0}", userName );
                            ExceptionLogService.LogException( new Exception( excceptionText ) );

                            // User is not authorized
                            string alternateRedirectUrlSetting = LinkedPageUrl( "AlternativeLoginPage" );
                            Response.Redirect( alternateRedirectUrlSetting );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                    else
                    {
                        if ( !IsPostBack && !string.IsNullOrEmpty( GetAttributeValue( "AllowedNetwork" ) ) )
                        {
                            // User has configured Allowed Network

                            var userIPAddress = HttpContext.Current.Request.UserHostAddress;
                            if ( userIPAddress == "::1" )
                            {
                                userIPAddress = "127.0.0.1";
                            }

                            if ( IsInRange( userIPAddress, GetAttributeValue( "AllowedNetwork" ) ) )
                            {
                                // User is in range
                                Response.Redirect( authorizationLink.AbsoluteUri );
                                Context.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                // Logging exception to make troubleshooting easier
                                var excceptionText = string.Format( "Office 365 SSO - IP Address not in CIDR Network: {0}", userIPAddress );
                                ExceptionLogService.LogException( new Exception( excceptionText ) );

                                // User is not in range
                                string redirectUrlSetting = LinkedPageUrl( "AlternativeLoginPage" );
                                Response.Redirect( redirectUrlSetting );
                                Context.ApplicationInstance.CompleteRequest();
                            }
                        }
                        else if ( !IsPostBack && string.IsNullOrEmpty( GetAttributeValue( "AllowedNetwork" ) ) )
                        {
                            // Sending to Office 365 to get authorization
                            Response.Redirect( authorizationLink.AbsoluteUri );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                    }
                }
                else
                {
                    nbAlert.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbAlert.Text = "This action could not be completed.";
                }
                
            }
            else
            {
                string redirectUrlSetting = LinkedPageUrl( "SuccessfulRedirectPage" );

                if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                {
                    nbAlert.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbAlert.Text = string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{0}'>{0}</a>.", redirectUrlSetting );
                }
                else
                {
                    Response.Redirect( redirectUrlSetting, false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }

        /// <summary>
        /// Checks if a username is locked out or needs confirmation, and handles those events
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        private void CheckUser( string userName, string returnUrl, bool rememberMe )
        {
            var userLogin = new UserLoginService( new RockContext() ).GetByUserName( userName );
            CheckUser( userLogin, returnUrl, rememberMe );
        }

        /// <summary>
        /// Checks if a userLogin is locked out or needs confirmation, and handles those events
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="returnUrl">Where to redirect next</param>
        /// <param name="rememberMe">True for external auth, the checkbox for internal auth</param>
        private void CheckUser( UserLogin userLogin, string returnUrl, bool rememberMe )
        {
            if ( userLogin != null )
            {
                if ( ( userLogin.IsConfirmed ?? true ) && !( userLogin.IsLockedOut ?? false ) )
                {
                    LoginUser( userLogin.UserName, returnUrl, rememberMe );
                }
                else
                {
                    string redirectUrlSetting = LinkedPageUrl( "AlternativeLoginPage" );
                    Response.Redirect( redirectUrlSetting );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        /// <summary>
        /// Logs in the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        private void LoginUser( string userName, string returnUrl, bool rememberMe )
        {
            string redirectUrlSetting = LinkedPageUrl( "RedirectPage" );

            UserLoginService.UpdateLastLogin( userName );

            Rock.Security.Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                string redirectUrl = Server.UrlDecode( returnUrl );
                Response.Redirect( redirectUrl );
                Context.ApplicationInstance.CompleteRequest();
            }
            else if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
            {
                Response.Redirect( redirectUrlSetting );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }

        /// <summary>
        /// Checks to see if IP Address is inside CIDR range
        /// </summary>
        /// <param name="ipAddress">IP Address</param>
        /// <param name="CIDRmask">The CIDR we are comparing the IP against</param>
        private bool IsInRange( string ipAddress, string CIDRmask )
        {
            string[] parts = CIDRmask.Split( '/' );

            int IP_addr = BitConverter.ToInt32( IPAddress.Parse( parts[0] ).GetAddressBytes(), 0 );
            int CIDR_addr = BitConverter.ToInt32( IPAddress.Parse( ipAddress ).GetAddressBytes(), 0 );
            int CIDR_mask = IPAddress.HostToNetworkOrder( -1 << ( 32 - int.Parse( parts[1] ) ) );

            return ( ( IP_addr & CIDR_mask ) == ( CIDR_addr & CIDR_mask ) );
        }
    }
}
