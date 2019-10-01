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

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Prompts user for login credentials.
    /// </summary>
    [DisplayName( "Login" )]
    [Category( "Security" )]
    [Description( "Prompts user for login credentials." )]

    [LinkedPage( "New Account Page", "Page to navigate to when user selects 'Create New Account' (if blank will use 'NewAccountPage' page route)", false, "", "", 0 )]
    [LinkedPage( "Help Page", "Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)", false, "", "", 1 )]
    [CodeEditorField( "Confirm Caption", "The text (HTML) to display when a user's account needs to be confirmed.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"
Thank you for logging in, however, we need to confirm the email associated with this account belongs to you. We’ve sent you an email that contains a link for confirming.  Please click the link in your email to continue.
", "", 2 )]
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", false, "", "", 3 )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    [CodeEditorField( "Locked Out Caption", "The text (HTML) to display when a user's account has been locked.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"
{%- assign phone = Global' | Attribute:'OrganizationPhone' | Trim -%} Sorry, your account has been locked.  Please {% if phone != '' %}contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email{% else %}email us at{% endif %} <a href='mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}'>{{ 'Global' | Attribute:'OrganizationEmail' }}</a> for help. Thank you.
", "", 5 )]
    [BooleanField( "Hide New Account Option", "Should 'New Account' option be hidden?  For sites that require user to be in a role (Internal Rock Site for example), users shouldn't be able to create their own account.", false, "", 6, "HideNewAccount" )]
    [TextField( "New Account Text", "The text to show on the New Account button.", false, "Register", "", 7, "NewAccountButtonText" )]
    [CodeEditorField( "No Account Text", "The text to show when no account exists. <span class='tip tip-lava'></span>.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"We couldn’t find an account with that username and password combination. Can we help you recover your <a href='{{HelpPage}}'>account information</a>?", "", 8, "NoAccountText" )]

    [CodeEditorField( "Remote Authorization Prompt Message", "Optional text (HTML) to display above remote authorization options.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, defaultValue: "Login with social account", order: 9 )]
    [RemoteAuthsField( "Remote Authorization Types", "Which of the active remote authorization types should be displayed as an option for user to use for authentication.", false, "", "", 10 )]
    [BooleanField( "Show Internal Login", "Show the default (non-remote) login", defaultValue: true, order: 11 )]
    [BooleanField( "Redirect to Single External Auth Provider", "Redirect straight to the external authentication provider if only one is configured and the internal login is disabled.", defaultValue: false, order: 12 )]

    [CodeEditorField( "Prompt Message", "Optional text (HTML) to display above username and password fields.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"", "", 13 )]
    [LinkedPage( "Redirect Page", "Page to redirect user to upon successful login. The 'returnurl' query string will always override this setting for database authenticated logins. Redirect Page Setting will override third-party authentication 'returnurl'.", false, "", "", 14 )]

    [CodeEditorField( "Invalid PersonToken Text", "The text to show when a person is logged out due to an invalid persontoken. <span class='tip tip-lava'></span>.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, @"<div class='alert alert-warning'>The login token you provided is no longer valid. Please login below.</div>", "", 15 )]
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

            this.BlockUpdated += Login_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ApplyBlockSettings();
        }

        /// <summary>
        /// Applies the block settings.
        /// </summary>
        private void ApplyBlockSettings()
        {
            btnNewAccount.Visible = !GetAttributeValue( "HideNewAccount" ).AsBoolean();
            btnNewAccount.Text = this.GetAttributeValue( "NewAccountButtonText" ) ?? "Register";

            phExternalLogins.Controls.Clear();

            List<AuthenticationComponent> activeAuthProviders = new List<AuthenticationComponent>();

            var selectedGuids = new List<Guid>();
            GetAttributeValue( "RemoteAuthorizationTypes" ).SplitDelimitedValues()
                .ToList()
                .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

            lRemoteAuthLoginsHeadingText.Text = this.GetAttributeValue( "RemoteAuthorizationPromptMessage" );

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
                        if ( component.Authenticate( Request, out userName, out returnUrl ) )
                        {
                            if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
                            {
                                CheckUser( userName, redirectUrlSetting, true );
                                break;
                            }
                            else
                            {
                                CheckUser( userName, returnUrl, true );
                                break;
                            }
                        }
                    }

                    activeAuthProviders.Add( component );

                    LinkButton lbLogin = new LinkButton();
                    phExternalLogins.Controls.Add( lbLogin );
                    lbLogin.AddCssClass( "btn btn-authentication " + component.LoginButtonCssClass );
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

                    lbLogin.Text = component.LoginButtonText;
                }
            }

            // adjust the page layout based on the RemoteAuth and InternalLogin options
            pnlRemoteAuthLogins.Visible = activeAuthProviders.Any();
            bool showInternalLogin = this.GetAttributeValue( "ShowInternalLogin" ).AsBooleanOrNull() ?? true;
            pnlInternalAuthLogin.Visible = showInternalLogin;

            if ( activeAuthProviders.Count() == 1 && !showInternalLogin )
            {
                var singleAuthProvider = activeAuthProviders[0];
                bool redirecttoSingleExternalAuthProvider = this.GetAttributeValue( "RedirecttoSingleExternalAuthProvider" ).AsBoolean();

                if ( redirecttoSingleExternalAuthProvider )
                {
                    Uri remoteAuthLoginUri = singleAuthProvider.GenerateLoginUrl( this.Request );
                    if ( remoteAuthLoginUri != null )
                    {
                        if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                        {
                            nbAdminRedirectPrompt.Text = string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to the <a href='{0}'>{1}</a> url.", remoteAuthLoginUri.AbsoluteUri, singleAuthProvider.LoginButtonText );
                            nbAdminRedirectPrompt.Visible = true;
                        }
                        else
                        {
                            Response.Redirect( remoteAuthLoginUri.AbsoluteUri, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }
                }
            }

            if ( pnlInternalAuthLogin.Visible && pnlRemoteAuthLogins.Visible )
            {
                // if they are both visible, show in 2 equal columns
                pnlRemoteAuthLogins.CssClass = "col-sm-6 margin-b-lg";
                pnlInternalAuthLogin.CssClass = "col-sm-6";
            }
            else
            {
                // if only one (or none) is visible, show in one column
                pnlRemoteAuthLogins.CssClass = "col-sm-12 margin-b-lg";
                pnlInternalAuthLogin.CssClass = "col-sm-12";
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Login control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Login_BlockUpdated( object sender, EventArgs e )
        {
            ApplyBlockSettings();
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
                lPromptMessage.Text = GetAttributeValue( "PromptMessage" );

                if ( (bool?)Session["InvalidPersonToken"] == true )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    lInvalidPersonTokenText.Text = GetAttributeValue( "InvalidPersonTokenText" ).ResolveMergeFields( mergeFields );
                    Session.Remove( "InvalidPersonToken" );
                }
            }

            pnlMessage.Visible = false;
            tbUserName.Focus();
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
                        var isSuccess = component.AuthenticateAndTrack( userLogin, tbPassword.Text );
                        rockContext.SaveChanges();

                        if ( isSuccess )
                        {
                            CheckUser( userLogin, Request.QueryString["returnurl"], cbRememberMe.Checked );
                            return;
                        }
                    }
                }
            }

            string helpUrl = string.Empty;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "HelpPage" ) ) )
            {
                helpUrl = LinkedPageUrl( "HelpPage" );
            }
            else
            {
                helpUrl = ResolveRockUrl( "~/ForgotUserName" );
            }

            var mergeFieldsNoAccount = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFieldsNoAccount.Add( "HelpPage", helpUrl );
            DisplayError( GetAttributeValue( "NoAccountText" ).ResolveMergeFields( mergeFieldsNoAccount ) );
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
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

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
                LinkButton lb = (LinkButton)sender;

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

            Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                string redirectUrl = ExtensionMethods.ScrubEncodedStringForXSSObjects(returnUrl);
                redirectUrl =  Server.UrlDecode( redirectUrl );
                Response.Redirect( redirectUrl );
                Context.ApplicationInstance.CompleteRequest();
            }
            else if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
            {
                Response.Redirect( redirectUrlSetting, false );
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

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( '/' ) );
            mergeFields.Add( "Person", userLogin.Person );
            mergeFields.Add( "User", userLogin );

            var recipients = new List<RecipientData>();
            recipients.Add( new RecipientData( userLogin.Person.Email, mergeFields ) );

            var message = new RockEmailMessage( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid() );
            message.SetRecipients( recipients );
            message.AppRoot = ResolveRockUrl( "~/" );
            message.ThemeRoot = ResolveRockUrl( "~~/" );
            message.CreateCommunicationRecord = false;
            message.Send();
        }

        #endregion
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}
