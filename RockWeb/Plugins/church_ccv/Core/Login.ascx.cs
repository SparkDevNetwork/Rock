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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using RestSharp;
using System.Net;

namespace RockWeb.Plugins.church_ccv.Core
{
    /// <summary>
    /// Prompts user for login credentials.
    /// </summary>
    [DisplayName( "Login" )]
    [Category( "CCV > Core" )]
    [Description( "Prompts user for login credentials." )]

    [LinkedPage( "New Account Page", "Page to navigate to when user selects 'Create New Account' (if blank will use 'NewAccountPage' page route)", false, "", "", 0 )]
    [LinkedPage( "Help Page", "Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)", false, "", "", 1 )]
    [CodeEditorField( "Confirm Caption", "The text (HTML) to display when a user's account needs to be confirmed.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"
Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.
", "", 2 )]
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", false, "", "", 3 )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    [CodeEditorField( "Locked Out Caption", "The text (HTML) to display when a user's account has been locked.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"
Sorry, your account has been locked.  Please contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email {{ 'Global' | Attribute:'OrganizationEmail' }} to resolve this.  Thank-you. 
", "", 5 )]
    
    [LinkedPage( "Redirect Page", "Page to redirect user to upon successful login. The 'returnurl' query string will always override this setting for database authenticated logins. Redirect Page Setting will override third-party authentication 'returnurl'.", false, "", "", 10 )]
    public partial class Login : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            
            tbUserName.FormGroupCssClass = "login-form-label";
            tbPassword.FormGroupCssClass = "login-form-label";
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
                tbUserName.Focus();
            }
            
            pnlMessage.Visible = false;
        }
        #endregion

        #region Events

        protected void btnTempLoginTrigger_Click( object sender, EventArgs e )
        {
        }


        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// NOTE: This is the btnLogin for Internal Auth
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            // assume an error, and let any of the success conditions toggle it off
            bool displayLoginError = true;

            if ( Page.IsValid )
            {
                // first get the appropriate login service
                var rockContext = new RockContext();
                var userLoginService = new UserLoginService(rockContext);
                var userLogin = userLoginService.GetByUserName( tbUserName.Text );
                if ( userLogin != null && userLogin.EntityType != null)
                {
                    var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                    if (component != null && component.IsActive && !component.RequiresRemoteAuthentication)
                    {
                        // see if the credentials are valid
                        if ( component.Authenticate( userLogin, tbPassword.Text ) )
                        {
                            // if the account isn't locked or needing confirmation
                            if ( ( userLogin.IsConfirmed ?? true ) && !(userLogin.IsLockedOut ?? false ) )
                            {
                                // then proceed to the final step, validating them with PMG2's site
                                if ( TryPMG2Login( tbUserName.Text, tbPassword.Text ) )
                                {
                                    string returnUrl = Request.QueryString["returnurl"];
                                    LoginUser( tbUserName.Text, returnUrl, cbRememberMe.Checked );

                                    // no need for an error
                                    displayLoginError = false;
                                }
                            }
                            else
                            {
                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                                if ( userLogin.IsLockedOut ?? false )
                                {
                                    lLockedOutCaption.Text = GetAttributeValue( "LockedOutCaption" ).ResolveMergeFields( mergeFields );
                                    
                                    pnlLockedOut.Visible = true;

                                    // no need for an error
                                    displayLoginError = false;
                                }
                                else
                                {
                                    SendConfirmation( userLogin );

                                    lConfirmCaption.Text = GetAttributeValue( "ConfirmCaption" ).ResolveMergeFields( mergeFields );
                                    
                                    pnlConfirmation.Visible = true;

                                    // no need for an error
                                    displayLoginError = false;
                                }
                            }
                        }
                    }
                }
            }
            
            if ( displayLoginError )
            {
                DisplayError( "Sorry, we couldn't find an account matching that username/password." );
            }
        }

        protected bool TryPMG2Login( string username, string password )
        {
            // contact PMG2's site and attempt to login with the same credentials
            var restClient = new RestClient(
                string.Format( "https://apistaging.ccv.church/auth?user[username]={0}&user[password]={1}", username, password ) );

            var restRequest = new RestRequest( Method.POST );
            var restResponse = restClient.Execute( restRequest );

            if ( restResponse.StatusCode == HttpStatusCode.OK )
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNewAccount_Click( object sender, EventArgs e )
        {
            string returnUrl = Request.QueryString["returnurl"];

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "NewAccountPage" ) ) )
            {
                var parms = new Dictionary<string, string>();
                
                if ( !string.IsNullOrWhiteSpace( returnUrl ) )
                {
                    parms.Add( "returnurl", returnUrl );
                }

                NavigateToLinkedPage( "NewAccountPage", parms );
            }
            else
            {
                string url = "~/NewAccount";

                if ( !string.IsNullOrWhiteSpace( returnUrl ) )
                {
                    url += "?returnurl=" + returnUrl;
                } 

                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnHelp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnHelp_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "HelpPage" ) ) )
            {
                NavigateToLinkedPage( "HelpPage" );
            }
            else
            {
                Response.Redirect( "~/ForgotUserName", false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
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

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                string redirectUrl = Server.UrlDecode( returnUrl );
                Response.Redirect( redirectUrl );
                Context.ApplicationInstance.CompleteRequest();
            }
            else if (!string.IsNullOrWhiteSpace(redirectUrlSetting))
            {
                Response.Redirect(redirectUrlSetting);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }

        /// <summary>
        /// Sends the confirmation.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        private void SendConfirmation( UserLogin userLogin )
        {
            string url = LinkedPageUrl( "ConfirmationPage" );
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = ResolveRockUrl( "~/ConfirmAccount" );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );

            var personDictionary = userLogin.Person.ToLiquid() as Dictionary<string, object>;
            mergeFields.Add( "Person", personDictionary );
            mergeFields.Add( "User", userLogin );

            var recipients = new List<RecipientData>();
            recipients.Add( new RecipientData( userLogin.Person.Email, mergeFields ) );

            Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ), false );
        }

        #endregion
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}
