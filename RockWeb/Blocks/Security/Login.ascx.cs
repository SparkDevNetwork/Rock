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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;
using Rock.Security;
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

    [LinkedPage( "New Account Page", "Page to navigate to when user selects 'Create New Account' (if blank will use 'NewAccountPage' page route)", true, "", "", 0 )]
    [LinkedPage( "Help Page", "Page to navigate to when user selects 'Help' option (if blank will use 'ForgotUserName' page route)", true, "", "", 1 )]
    [CodeEditorField( "Confirm Caption", "The text (HTML) to display when a user's account needs to be confirmed.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"
Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.
", "", 2 )]
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", true, "", "", 3 )]
    [EmailTemplateField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    [CodeEditorField( "Locked Out Caption", "The text (HTML) to display when a user's account has been locked.", CodeEditorMode.Html, CodeEditorTheme.Rock, 200, false, @"
Sorry, your account has been locked.  Please contact our office at {{ GlobalAttribute.OrganizationPhone }} or email {{ GlobalAttribute.OrganizationEmail }} to resolve this.  Thank-you. 
", "", 5 )]
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

            phExternalLogins.Controls.Clear();

            // Look for active external authentication providers
            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.IsActive && component.RequiresRemoteAuthentication )
                {
                    string loginTypeName = component.GetType().Name;

                    // Check if returning from third-party authentication
                    if ( !IsPostBack && component.IsReturningFromAuthentication( Request ) )
                    {
                        string userName = string.Empty;
                        string returnUrl = string.Empty;
                        if ( component.Authenticate( Request, out userName, out returnUrl ) )
                        {
                            LoginUser( userName, returnUrl, false );
                            break;
                        }
                    }

                    LinkButton lbLogin = new LinkButton();
                    phExternalLogins.Controls.Add( lbLogin );
                    lbLogin.AddCssClass( "btn btn-authenication " + loginTypeName.ToLower() );
                    lbLogin.ID = "lb" + loginTypeName + "Login";
                    lbLogin.Click += lbLogin_Click;
                    lbLogin.CausesValidation = false;

                    if ( !String.IsNullOrWhiteSpace( component.ImageUrl() ) )
                    {
                        HtmlImage img = new HtmlImage();
                        lbLogin.Controls.Add( img );
                        img.Attributes.Add( "style", "border:none" );
                        img.Src = Page.ResolveUrl( component.ImageUrl() );
                    }
                    else
                    {
                        lbLogin.Text = "Login Using " + loginTypeName;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlMessage.Visible = false;

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var userLoginService = new UserLoginService();
                var userLogin = userLoginService.GetByUserName( tbUserName.Text );
                if ( userLogin != null && userLogin.EntityType != null)
                {
                    var component = AuthenticationContainer.GetComponent(userLogin.EntityType.Name);
                    if (component.IsActive && 
                        component.ServiceType == AuthenticationServiceType.Internal &&
                        !component.RequiresRemoteAuthentication)
                    {
                        if ( component.Authenticate( userLogin, tbPassword.Text ) )
                        {
                            if ( ( userLogin.IsConfirmed ?? true ) && !(userLogin.IsLockedOut ?? false ) )
                            {
                                string returnUrl = Request.QueryString["returnurl"];
                                LoginUser( tbUserName.Text, returnUrl, cbRememberMe.Checked );
                            }
                            else
                            { 
                                var globalMergeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields(null);

                                if ( userLogin.IsLockedOut ?? false )
                                {
                                    lLockedOutCaption.Text = GetAttributeValue( "LockedOutCaption" ).ResolveMergeFields( globalMergeFields );

                                    pnlLogin.Visible = false;
                                    pnlLockedOut.Visible = true;
                                }
                                else
                                {
                                    SendConfirmation( userLogin );

                                    lConfirmCaption.Text = GetAttributeValue( "ConfirmCaption" ).ResolveMergeFields( globalMergeFields );

                                    pnlLogin.Visible = false;
                                    pnlConfirmation.Visible = true;

                                }
                            }
                            return;

                        }
                    }
                }
            }

            string helpUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(GetAttributeValue("HelpPage")))
            {
                helpUrl = LinkedPageUrl("HelpPage");
            }
            else
            {
                helpUrl = ResolveRockUrl("~/ForgotUserName");
            }
                
            DisplayError( String.Format("Sorry, we couldn't find an account matching that username/password. Can we help you <a href='{0}'>recover your account information</a>?", helpUrl) );
        }

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void lbLogin_Click( object sender, EventArgs e )
        {
            if ( sender is LinkButton )
            {
                LinkButton lb = (LinkButton)sender;

                foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    if (component.IsActive && component.RequiresRemoteAuthentication)
                    {
                        string loginTypeName = component.GetType().Name;
                        if ( lb.ID == "lb" + loginTypeName + "Login" )
                        {
                            Uri uri = component.GenerateLoginUrl( Request );
                            Response.Redirect( uri.AbsoluteUri, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
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
            new UserLoginService().UpdateLastLogin( userName );

            Rock.Security.Authorization.SetAuthCookie( userName, rememberMe, false );

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                Response.Redirect( Server.UrlDecode( returnUrl ), false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }

        private void SendConfirmation(UserLogin userLogin)
        {
            string url = LinkedPageUrl( "ConfirmationPage" );
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = ResolveRockUrl( "~/ConfirmAccount" );
            }
            var mergeObjects = new Dictionary<string, object>();
            mergeObjects.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );

            var personDictionary = userLogin.Person.ToDictionary();
            mergeObjects.Add( "Person", personDictionary );

            mergeObjects.Add( "User", userLogin.ToDictionary() );

            var recipients = new Dictionary<string, Dictionary<string, object>>();
            recipients.Add( userLogin.Person.Email, mergeObjects );

            Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ) );
        }

        #endregion
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}