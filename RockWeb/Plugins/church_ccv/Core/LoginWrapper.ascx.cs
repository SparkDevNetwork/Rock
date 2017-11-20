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
                
                hlNewAccount.Visible = true;
                liDropdown.Visible = false;
                liLogin.Visible = true;
            }
        }

        public int LoginWrapper_GetPageId( )
        {
            return RockPage.PageId;
        }

        public string LoginWrapper_GetDefaultPage( )
        {
            Rock.Web.Cache.PageCache.PageRouteInfo routeInfo = RockPage.Layout.Site.DefaultPage.PageRoutes.FirstOrDefault( );
            if ( routeInfo != null )
            {
                return "/" + routeInfo.Route;
            }
            return "/Page/" + RockPage.Layout.Site.DefaultPageId.ToString( );
        }
        #endregion
    }
}
