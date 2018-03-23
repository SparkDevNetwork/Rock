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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using church.ccv.Authentication;

namespace RockWeb.Plugins.church_ccv.ChurchOnlinePlatform
{
    /// <summary>
    /// Prompts user for login credentials.
    /// </summary>
    [DisplayName( "Login" )]
    [Category( "CCV > Church Online Platform" )]
    [Description( "SSO login for Church Online Platform. Add this block to an external facing page and point Church Online Platform at the external page." )]

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
    [BooleanField( "Hide New Account Option", "Should 'New Account' option be hidden?  For site's that require user to be in a role (Internal Rock Site for example), users shouldn't be able to create their own account.", false, "", 6, "HideNewAccount" )]
    [TextField( "New Account Text", "The text to show on the New Account button.", false, "Register", "", 7, "NewAccountButtonText" )]
    [CodeEditorField( "Prompt Message", "Optional text (HTML) to display above username and password fields.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"", "", 8 )]
    [RemoteAuthsField( "Remote Authorization Types", "Which of the active remote authorization types should be displayed as an option for user to use for authentication.", false, "", "", 9 )]
    [TextField( "Church Online Platform Sso Key", "You can find the Sso key on the Tools > Sso page in Church Online Platform.", false, "", "", 10, "ChOPSSOKey" )]
    [TextField( "Church Online Platform Url", "Your Church Online Platform Url", false, "", "", 11, "ChOPUrl" )]

    public partial class ChOPLogin : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Apply block settings
            btnNewAccount.Visible = !GetAttributeValue( "HideNewAccount" ).AsBoolean();
            btnNewAccount.Text = this.GetAttributeValue( "NewAccountButtonText" ) ?? "Register";

            // Add third-party providers if enabled in block settings
            phExternalLogins.Controls.Clear();

            int activeAuthProviders = 0;

            var selectedGuids = new List<Guid>();
            GetAttributeValue( "RemoteAuthorizationTypes" ).SplitDelimitedValues()
                .ToList()
                .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

            // Look for active external authentication providers
            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.IsActive &&
                    component.RequiresRemoteAuthentication &&
                    selectedGuids.Contains( component.EntityType.Guid ) )
                {
                    string loginTypeName = component.GetType().Name;

                    // Check if returning from third-party authentication
                    if ( !IsPostBack && component.IsReturningFromAuthentication( Request ) )
                    {
                        string userName = string.Empty;
                        string returnUrl = string.Empty;
                        string redirectUrlSetting = LinkedPageUrl( "RedirectPage" );
                        
                        // Authenticate user to third-party
                        if ( component.Authenticate( Request, out userName, out returnUrl ) )
                        {
                            // Get UserLogin from username returned by third-party authentication
                            var rockContext = new RockContext();
                            var userLoginService = new UserLoginService( rockContext );
                            var userLogin = userLoginService.GetByUserName( userName );

                            // Login User and redirect to ChOp
                            LoginUser( userLogin, false );
                        }
                    }

                    activeAuthProviders++;

                    // Build and Add authentication control
                    LinkButton lbLogin = new LinkButton();
                    phExternalLogins.Controls.Add( lbLogin );
                    lbLogin.AddCssClass( "btn btn-authentication " + loginTypeName.ToLower() );
                    lbLogin.ID = "lb" + loginTypeName + "Login";
                    lbLogin.Click += lbLogin_Click;
                    lbLogin.CausesValidation = false;

                    if ( !string.IsNullOrWhiteSpace( component.ImageUrl() ) )
                    {
                        HtmlImage img = new HtmlImage();
                        lbLogin.Controls.Add( img );
                        img.Attributes.Add( "style", "border:none" );
                        img.Src = Page.ResolveUrl( component.ImageUrl() );
                    }
                    else
                    {
                        lbLogin.Text = loginTypeName;
                    }
                }
            }

            // adjust the page if there are no social auth providers
            if ( activeAuthProviders == 0 )
            {
                divSocialLogin.Visible = false;
                divOrgLogin.RemoveCssClass( "col-sm-6" );
                divOrgLogin.AddCssClass( "col-sm-12" );
            }

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
                // Check if ChOP block settings are set
                if ( ( GetAttributeValue( "ChOPSSOKey" ).IsNullOrWhiteSpace() ) || ( GetAttributeValue( "ChOPUrl" ).IsNullOrWhiteSpace() ) )
                {
                    DisplayError( "Missing Church Online Platform Block Settings" );
                    pnlLoginForm.Visible = false;
                    return;
                }
                
                // If person is logged in, redirect back to Church Online Platform using current person
                // If person is administrator dont redirect so they can edit block settings
                Person currentPerson = CurrentPerson;
                if ( currentPerson != null )
                {
                    // Generate ChOP SSO Login Url
                    string chOPSSOLoginUrl = LoginChOP( currentPerson );

                    if ( chOPSSOLoginUrl != null )
                    {
                        // If current person is administrator, dont redirect, hide login form, display message with ChOp url
                        if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                        {
                            pnlLoginForm.Visible = false;
                            DisplayError( string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{0}'>ChOP SSO Url</a>.", chOPSSOLoginUrl ) );
                            return;
                        }
                        else
                        {
                            // Redirect current person
                            Response.Redirect( chOPSSOLoginUrl, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }
                    else
                    {
                        // Something went wrong with generating URL, display error
                        DisplayError( string.Format( "Authentication to Church Online Platform failed" ) );
                    }
                }

                lPromptMessage.Text = GetAttributeValue( "PromptMessage" );
                tbUserName.Focus();
            }

            pnlMessage.Visible = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// NOTE: This is the btnLogin for Internal Auth
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( tbUserName.Text );
                if ( userLogin != null && userLogin.EntityType != null )
                {
                    var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );
                    if ( component != null && component.IsActive && !component.RequiresRemoteAuthentication )
                    {
                        if ( component.Authenticate( userLogin, tbPassword.Text ) )
                        {
                            if ( ( userLogin.IsConfirmed ?? true ) && !( userLogin.IsLockedOut ?? false ) )
                            {
                                // If authentication is successful, user is confirmed, and user is not locked out
                                // Login the user
                                LoginUser( userLogin, cbRememberMe.Checked );
                            }
                            else
                            {
                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                                if ( userLogin.IsLockedOut ?? false )
                                {
                                    lLockedOutCaption.Text = GetAttributeValue( "LockedOutCaption" ).ResolveMergeFields( mergeFields );

                                    pnlLogin.Visible = false;
                                    pnlLockedOut.Visible = true;
                                }
                                else
                                {
                                    SendConfirmation( userLogin );

                                    lConfirmCaption.Text = GetAttributeValue( "ConfirmCaption" ).ResolveMergeFields( mergeFields );

                                    pnlLogin.Visible = false;
                                    pnlConfirmation.Visible = true;
                                }
                            }                                
                            return;
                        }
                    }
                }
            }

            // Login failed...display error with forgot account link
            string helpUrl = string.Empty;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "HelpPage" ) ) )
            {
                helpUrl = LinkedPageUrl( "HelpPage" );
            }
            else
            {
                helpUrl = ResolveRockUrl( "~/ForgotUserName" );
            }

            DisplayError( string.Format( "Sorry, we couldn't find an account matching that username/password. Can we help you <a href='{0}'>recover your account information</a>?", helpUrl ) );
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

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// NOTE: This is the lbLogin for External/Remote logins
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void lbLogin_Click( object sender, EventArgs e )
        {
            if ( sender is LinkButton )
            {
                LinkButton lb = ( LinkButton ) sender;

                foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    if ( component.IsActive && component.RequiresRemoteAuthentication )
                    {
                        string loginTypeName = component.GetType().Name;
                        if ( lb.ID == "lb" + loginTypeName + "Login" )
                        {
                            Uri uri = component.GenerateLoginUrl( Request );
                            if ( uri != null )
                            {
                                Response.Redirect( uri.AbsoluteUri, false );
                                Context.ApplicationInstance.CompleteRequest();
                                return;
                            }
                            else
                            {
                                DisplayError( string.Format( "ERROR: {0} does not have a remote login URL", loginTypeName ) );
                            }
                        }
                    }
                }
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

        /// <summary>
        /// Logs in the user.
        /// </summary>
        /// <param name="userLogin">UserLogin object</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        private void LoginUser( UserLogin userLogin, bool rememberMe )
        {
            // Update Last Login
            UserLoginService.UpdateLastLogin( userLogin.UserName );

            // Set Auth Cookie
            //bool rememberMe = cbRememberMe.Checked;
            Rock.Security.Authorization.SetAuthCookie( userLogin.UserName, rememberMe, false );

            // Generate Church Online Platform login Url
            string chOPUrl = LoginChOP( userLogin.Person );

            if ( !chOPUrl.IsNullOrWhiteSpace() )
            {
                // Redirect to return URL and end processing
                Response.Redirect( chOPUrl, false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                DisplayError( string.Format( "Authentication to Church Online Platform failed" ) );
            }
        }

        /// <summary>
        /// Generates Church Online Platform SSO login URL
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        private string LoginChOP( Person person )
        {
            // Generate and return sso login URL
            return church.ccv.Authentication.ChurchOnlinePlatform.CreateSSOUrlChOP( person, GetAttributeValue( "ChOPSSOKey" ), GetAttributeValue( "ChOPUrl" ) );
        }

        #endregion
    }
}