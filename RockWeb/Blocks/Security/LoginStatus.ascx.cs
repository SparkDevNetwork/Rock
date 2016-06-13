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

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Displays currently logged in user's name along with options to Login, Logout, or manage account.
    /// </summary>
    [DisplayName( "Login Status" )]
    [Category( "Security" )]
    [Description( "Displays the currently logged in user's name along with options to Login, Logout, or manage account." )]

    [LinkedPage( "My Account Page", "Page for user to manage their account (if blank will use 'MyAccount' page route)", false )]
    [LinkedPage( "My Profile Page", "Page for user to view their person profile (if blank option will not be displayed)", false )]
    [LinkedPage( "My Settings Page", "Page for user to view their settings (if blank option will not be displayed)", false )]
    [KeyValueListField( "Logged In Page List", "List of pages to show in the dropdown when the user is logged in. The link field takes Lava with the CurrentPerson merge fields. Place the text 'divider' in the title field to add a divider.", false, "", "Title", "Link" )]
    
    public partial class LoginStatus : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var myAccountUrl = LinkedPageUrl( "MyAccountPage" );
            
            if ( !string.IsNullOrWhiteSpace( myAccountUrl ) )
            {
                hlMyAccount.NavigateUrl = myAccountUrl;
            }
            else
            {
                phMyAccount.Visible = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var currentPerson = CurrentPerson;
            if ( currentPerson != null )
            {
                phHello.Visible = true;
                lHello.Text = string.Format( "<span>Hello {0}</span>", currentPerson.NickName );

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

                lbLoginLogout.Text = "Logout";
                
                divProfilePhoto.Attributes.Add( "style", String.Format( "background-image: url('{0}'); background-size: cover; background-repeat: no-repeat; background-position: center top;", Rock.Model.Person.GetPersonPhotoUrl( currentPerson, 200, 200 )));

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
                lbLoginLogout.Text = "Login";

                liDropdown.Visible = false;
                liLogin.Visible = true;
            }

            hfActionType.Value = lbLoginLogout.Text;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbLoginLogout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLoginLogout_Click( object sender, EventArgs e )
        {
            string action = hfActionType.Value;
            if ( action == "Login" )
            {
                var site = RockPage.Layout.Site;
                if ( site.LoginPageId.HasValue )
                {
                    site.RedirectToLoginPage( true );
                }
                else
                {
                    FormsAuthentication.RedirectToLoginPage();
                }
            }
            else
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
        }

        #endregion
    }
}