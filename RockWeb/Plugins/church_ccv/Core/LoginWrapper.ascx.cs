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
using System.Web.Security;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Security;
using System.Text;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Core
{
    /// <summary>
    /// Displays currently logged in user's name along with options to Login, Logout, or manage account.
    /// </summary>
    [DisplayName( "Login Wrapper" )]
    [Category( "CCV > Core" )]
    [Description( "Manages the user's login status, account creation and forgot password functionality." )]
    [TextField( "Greeting", "Text that is displayed before the name of the logged in user", false )]
    [LinkedPage( "My Account Page", "Page for user to manage their account (if blank option will not be displayed)", false )]
    [LinkedPage( "My Profile Page", "Page for user to view their person profile (if blank option will not be displayed)", false )]
    [LinkedPage( "My Settings Page", "Page for user to view their settings (if blank option will not be displayed)", false )]
    [LinkedPage( "Register Account Page", "Page for user to create an account (if blank option will not be displayed)", false )]
    [KeyValueListField( "Logged In Page List", "List of pages to show in the dropdown when the user is logged in. The link field takes Lava with the CurrentPerson merge fields. Place the text 'divider' in the title field to add a divider.", false, "", "Title", "Link" )]
    


    [CodeEditorField( "Locked Out Caption", "The text (HTML) to display when a user's account has been locked.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, 
        @"Sorry, your account has been locked.  Please contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email {{ 'Global' | Attribute:'OrganizationEmail' }} to resolve this.  Thank-you.", "", 5 )]
    [CodeEditorField( "Confirm Caption", "The text (HTML) to display when a user's account needs to be confirmed.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, 
        @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "", 2 )]
    [CodeEditorField( "Forgot Password Caption", "The text (HTML) to display on the forgot password panel.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, 
        @"Enter the email address associated with your account. If you do not receive an email containing reset instructions, please call us at: {{ 'Global' | Attribute:'OrganizationPhone' }}", "", 5 )]
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", false, "", "", 3 )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    public partial class LoginWrapper : Rock.Web.UI.RockBlock
    {
        #region LoginWrapper
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            LoginStatus_OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoginStatus_OnLoad( e );
            LoginModal_OnLoad( e );
        }
        #endregion
        
        #region LoginStatus
        protected void LoginStatus_OnInit( EventArgs e )
        {
            var myAccountUrl = LinkedPageUrl( "MyAccountPage" );
            
            if ( !string.IsNullOrWhiteSpace( myAccountUrl ) )
            {
                hlMyAccount.NavigateUrl = myAccountUrl;
            }
            else
            {
                phMyAccount.Visible = false;
            }

            // Check if RegisterAccountPage is configured in the block and set new account page to the configured page
            // if not configured, hide
            var newAccountUrl = LinkedPageUrl( "RegisterAccountPage" );

            if ( !string.IsNullOrWhiteSpace( newAccountUrl ) )
            {
                hlNewAccount.NavigateUrl = newAccountUrl;
                phNewAccount.Visible = true;
            }
            else
            {
                hlNewAccount.Visible = false;
            }
        }

        protected void LoginStatus_OnLoad( EventArgs e )
        {
            var currentPerson = CurrentPerson;
            if ( currentPerson != null )
            {
                phHello.Visible = true;
                lHello.Text = this.GetAttributeValue( "Greeting" ) + currentPerson.NickName;

                var currentUser = CurrentUser;
                if ( currentUser == null || !currentUser.IsAuthenticated )
                {
                    phMyAccount.Visible = false;
                    phMySettings.Visible = false;
                }
                
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "PersonId", currentPerson.Id.ToString() );
                
                var myProfileUrl = LinkedPageUrl( "MyProfilePage", queryParams );
                if ( !string.IsNullOrWhiteSpace( myProfileUrl ) )
                {
                    hlMyProfile.NavigateUrl = myProfileUrl;
                }
                else
                {
                    phMyProfile.Visible = false;
                }

                var mySettingsUrl = LinkedPageUrl( "MySettingsPage", null );
                if ( !string.IsNullOrWhiteSpace( mySettingsUrl ) )
                {
                    hlMySettings.NavigateUrl = mySettingsUrl;
                }
                else
                {
                    phMySettings.Visible = false;
                }

                lbLogout.Text = "Logout";
                lbLogout.Visible = true;

                divProfilePhoto.Attributes.Add( "style", String.Format( "background-image: url('{0}');", Rock.Model.Person.GetPersonPhotoUrl( currentPerson, 200, 200 )));

                var navPagesString = GetAttributeValue( "LoggedInPageList" );

                if ( !string.IsNullOrWhiteSpace( navPagesString ) )
                {
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "CurrentPerson", CurrentPerson );

                    navPagesString = navPagesString.TrimEnd( '|' );
                    var navPages = navPagesString.Split( '|' )
                                    .Select( s => s.Split( '^' ) )
                                    .Select( p => new { Title = p[0], Link = p[1] } );

                    StringBuilder sbPageMarkup = new StringBuilder();
                    foreach ( var page in navPages )
                    {
                        if ( page.Title.Trim() == "divider" )
                        {
                            sbPageMarkup.Append( "<li class='divider'></li>" );
                        }
                        else
                        {
                            sbPageMarkup.Append( string.Format( "<li><a href='{0}'>{1}</a>", Page.ResolveUrl(page.Link.ResolveMergeFields(mergeFields)), page.Title ) );
                        }
                    }

                    lDropdownItems.Text = sbPageMarkup.ToString();
                }
            
            }
            else
            {
                phHello.Visible = false;
                phMyAccount.Visible = false;
                phMyProfile.Visible = false;
                phMySettings.Visible = false;
                lbLogout.Visible = false;

                hlNewAccount.Visible = true;
                liDropdown.Visible = false;
                liLogin.Visible = true;
            }
        }

        protected void LoginStatus_lbLogout_Click( object sender, EventArgs e )
        {
            if ( CurrentUser != null )
            {
                var transaction = new Rock.Transactions.UserLastActivityTransaction();
                transaction.UserId = CurrentUser.Id;
                transaction.LastActivityDate = RockDateTime.Now;
                transaction.IsOnLine = false;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            FormsAuthentication.SignOut();

            // After logging out check to see if an anonymous user is allowed to view the current page.  If so
            // redirect back to the current page, otherwise redirect to the site's default page
            var currentPage = Rock.Web.Cache.PageCache.Read( RockPage.PageId );
            if ( currentPage != null && currentPage.IsAuthorized(Authorization.VIEW, null))
            {
                Response.Redirect( CurrentPageReference.BuildUrl() );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }
        #endregion

        #region LoginModal
        protected void LoginModal_OnLoad( EventArgs e )
        {
            // see if the login class needs to handle this
            if ( Page.IsPostBack )
            {
                // parse the event args so we know what we need to handle
                string postbackArgs = Request.Params["__EVENTARGUMENT"];
                if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
                {
                    string[] splitArgs = postbackArgs.Split( new char[] { ':' } );

                    // the first should always be the action
                    switch ( splitArgs [ 0 ] )
                    {
                        case "__LOGIN_SUCCEEDED":
                        {
                            string returnUrl = Request.QueryString["returnurl"];
                            LoginModal_TryRedirectUser( returnUrl );

                            break;
                        }

                        // for now, do the same as when a login succeeds
                        case "__FORGOT_PASSWORD_SUCCEEDED":
                        case "__REGISTRATION_SUCCEEDED":
                        {
                            string returnUrl = Request.QueryString["returnurl"];
                            LoginModal_TryRedirectUser( returnUrl );

                            break;
                        }



                        default:
                        {
                            // ignore unknown actions
                            break;
                        }
                    }
                }
            }
        }

        protected void LoginModal_TryRedirectUser( string returnUrl )
        {
            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                string redirectUrl = Server.UrlDecode( returnUrl );
                Response.Redirect( redirectUrl );
                ApplicationInstance.CompleteRequest( );
            }
            else
            {
                // without a return URL, just reload the existing page.
                Response.Redirect(Request.RawUrl);
            }
        }
                
        public string LoginModal_GetThemeUrl( )
        {
            return ResolveRockUrl( "~~/" );
        }

        public string LoginModal_GetAppUrl( )
        {
            return ResolveRockUrl( "~/" );
        }

        public string LoginModal_GetConfirmAccountTemplate( )
        {
            return GetAttributeValue( "ConfirmAccountTemplate" );
        }

        public string LoginModal_GetConfirmationUrl( )
        {
            string url = LinkedPageUrl( "ConfirmationPage" );
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = ResolveRockUrl( "~/ConfirmAccount" );
            }

            return RootPath + url;
        }

        public string LoginModal_GetConfirmCaption( )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            return GetAttributeValue( "ConfirmCaption" ).ResolveMergeFields( mergeFields );
        }

        public string LoginModal_GetLockedOutCaption( )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            return GetAttributeValue( "LockedOutCaption" ).ResolveMergeFields( mergeFields );
        }

        public string LoginModal_GetForgotPasswordCaption( )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            return GetAttributeValue( "ForgotPasswordCaption" ).ResolveMergeFields( mergeFields );
        }
        #endregion
    }
}
