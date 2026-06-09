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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

using Rock.Attribute;
using Rock.Model;
using Rock.Tasks;
using Rock.ViewModels.Blocks.Security.LoginStatus;
using Rock.Web.Cache;

using Authorization = Rock.Security.Authorization;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays the currently logged in user's name along with options to
    /// log in, log out, or manage account.
    /// </summary>
    [DisplayName( "Login Status" )]
    [Category( "Security" )]
    [Description( "Displays the currently logged in user's name along with options to log in, log out, or manage account." )]

    #region Block Attributes

    [KeyValueListField( "Logged In Page List",
        Description = "List of pages to show in the dropdown when the user is logged in. The link field takes Lava with the CurrentPerson merge fields. Place the text 'divider' in the title field to add a divider.",
        IsRequired = false,
        KeyPrompt = "Title",
        ValuePrompt = "Link",
        Order = 0 )]

    [LinkedPage( "My Account Page",
        Description = "Page for user to manage their account (if blank will use 'MyAccount' page route).",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "My Profile Page",
        Description = "Page for user to view their person profile (if blank option will not be displayed).",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "My Settings Page",
        Description = "Page for user to view their settings (if blank option will not be displayed).",
        IsRequired = false,
        Order = 3 )]

    [EnumField( "Mode",
        Description = "The functionality mode to use when rendering the block. Minimal will display just profile photo.",
        EnumSourceType = typeof( LoginStatusMode ),
        DefaultEnumValue = ( int ) LoginStatusMode.Full,
        Key = AttributeKey.Mode,
        Order = 4 )]

    #endregion

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "24E72D19-B443-4113-A66E-84C67FAD25FD" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "80BAB443-D4D4-4DDC-8411-5655F9795100" )]
    [Rock.SystemGuid.BlockTypeGuid( "04712F3D-9667-4901-A49D-4507573EF7AD" )]
    public class LoginStatus : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string MyAccountPage = "MyAccountPage";
            public const string MyProfilePage = "MyProfilePage";
            public const string MySettingsPage = "MySettingsPage";
            public const string LoggedInPageList = "LoggedInPageList";
            public const string Mode = "Mode";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// This is a private variable used by <see cref="GetInitializationBox"/>
        /// to return a cached version of the box during startup.
        /// </summary>
        private LoginStatusInitializationBox _initBox;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Gets the initialization box data, caching the result so it can
        /// be shared between <see cref="GetObsidianBlockInitialization"/>
        /// and <see cref="GetInitialHtmlContent"/>.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private LoginStatusInitializationBox GetInitializationBox()
        {
            if ( _initBox != null )
            {
                return _initBox;
            }

            var box = new LoginStatusInitializationBox();
            var currentPerson = GetCurrentPerson();

            if ( currentPerson != null )
            {
                box.IsLoggedIn = true;
                box.NickName = currentPerson.NickName;
                box.PhotoUrl = Person.GetPersonPhotoUrl( currentPerson, 400 );

                if ( RequestContext.CurrentUser != null )
                {
                    var myAccountUrl = this.GetLinkedPageUrl( AttributeKey.MyAccountPage );
                    if ( myAccountUrl.IsNotNullOrWhiteSpace() )
                    {
                        box.MyAccountUrl = myAccountUrl;
                    }

                    var mySettingsUrl = this.GetLinkedPageUrl( AttributeKey.MySettingsPage );
                    if ( mySettingsUrl.IsNotNullOrWhiteSpace() )
                    {
                        box.MySettingsUrl = mySettingsUrl;
                    }
                }

                var myProfileUrl = this.GetLinkedPageUrl( AttributeKey.MyProfilePage, new Dictionary<string, string>
                {
                    ["PersonId"] = currentPerson.IdKey
                } );

                if ( myProfileUrl.IsNotNullOrWhiteSpace() )
                {
                    box.MyProfileUrl = myProfileUrl;
                }

                box.CustomNavPages = GetCustomNavPages( currentPerson );
            }
            else
            {
                box.LoginPageUrl = this.GetLoginPageUrl( this.GetCurrentPageUrl() );
            }

            box.IsMinimalMode = GetAttributeValue( AttributeKey.Mode ).ConvertToEnumOrNull<LoginStatusMode>() == LoginStatusMode.Minimal;

            _initBox = box;

            return _initBox;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            /*
                4/14/2026 - MSE

                Added server-rendered HTML so the login status UI is visible
                before the Vue component loads. Because this block sits in the
                header zone of every page, the default loading animation would
                flash on every single page navigation.

                Reason: Prevent loading animation flicker on every page navigation.
            */
            var box = GetInitializationBox();
            var isMinimal = box.IsMinimalMode;
            var containerClass = isMinimal
                ? "nav navbar-nav loginstatus loginstatus-minimal"
                : "nav navbar-nav loginstatus";

            var sb = new StringBuilder();
            sb.Append( $"<ul class=\"{containerClass}\">" );

            if ( box.IsLoggedIn )
            {
                sb.Append( "<li class=\"dropdown\">" );
                sb.Append( "<a class=\"dropdown-toggle navbar-link\" href=\"#\" data-toggle=\"dropdown\">" );
                sb.Append( "<div class=\"loginstatus-wrapper\">" );

                sb.Append( $"<div class=\"profile-photo\" style=\"background-image: url('{WebUtility.HtmlEncode( box.PhotoUrl )}');\"></div>" );

                if ( !isMinimal )
                {
                    sb.Append( $"<span>{WebUtility.HtmlEncode( box.NickName )}</span>" );
                }

                sb.Append( "</div>" );

                if ( !isMinimal )
                {
                    sb.Append( "<b class=\"ti ti-caret-down-filled\"></b>" );
                }

                sb.Append( "</a>" );

                // Dropdown menu.
                sb.Append( "<ul class=\"dropdown-menu\">" );

                if ( box.MyAccountUrl.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( $"<li><a href=\"{WebUtility.HtmlEncode( box.MyAccountUrl )}\">My Account</a></li>" );
                }

                if ( box.MySettingsUrl.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( $"<li><a href=\"{WebUtility.HtmlEncode( box.MySettingsUrl )}\">My Settings</a></li>" );
                }

                if ( box.MyProfileUrl.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( $"<li><a href=\"{WebUtility.HtmlEncode( box.MyProfileUrl )}\">My Profile</a></li>" );
                }

                if ( box.CustomNavPages != null )
                {
                    foreach ( var page in box.CustomNavPages )
                    {
                        if ( page.IsDivider )
                        {
                            sb.Append( "<li class=\"divider\"></li>" );
                        }
                        else
                        {
                            var href = page.Url.IsNotNullOrWhiteSpace() ? WebUtility.HtmlEncode( page.Url ) : "#";
                            sb.Append( $"<li><a href=\"{href}\">{WebUtility.HtmlEncode( page.Title )}</a></li>" );
                        }
                    }
                }

                // Divider before Log Out, only if there are items above it.
                var hasDropdownItems = box.MyAccountUrl.IsNotNullOrWhiteSpace()
                    || box.MyProfileUrl.IsNotNullOrWhiteSpace()
                    || box.MySettingsUrl.IsNotNullOrWhiteSpace()
                    || ( box.CustomNavPages != null && box.CustomNavPages.Any() );

                if ( hasDropdownItems )
                {
                    sb.Append( "<li class=\"divider\"></li>" );
                }

                sb.Append( "<li><a href=\"#\">Log Out</a></li>" );
                sb.Append( "</ul>" );
                sb.Append( "</li>" );
            }
            else
            {
                sb.Append( "<li><a href=\"#\">Log In</a></li>" );
            }

            sb.Append( "</ul>" );

            return sb.ToString();
        }

        /// <summary>
        /// Parses the Logged In Page List block setting and resolves any
        /// Lava merge fields in the link values.
        /// </summary>
        /// <param name="currentPerson">The current person for Lava merge fields.</param>
        /// <returns>A list of custom nav page bags.</returns>
        private List<LoginStatusNavPageBag> GetCustomNavPages( Person currentPerson )
        {
            var navPagesString = GetAttributeValue( AttributeKey.LoggedInPageList );

            if ( navPagesString.IsNullOrWhiteSpace() )
            {
                return new List<LoginStatusNavPageBag>();
            }

            var mergeFields = new Dictionary<string, object>
            {
                { "CurrentPerson", currentPerson }
            };

            return navPagesString
                .TrimEnd( '|' )
                .Split( '|' )
                .Select( s => s.Split( '^' ) )
                .Where( p => p.Length >= 2 )
                .Select( p =>
                {
                    var title = p[0].Trim();
                    if ( title.Equals( "divider", System.StringComparison.OrdinalIgnoreCase ) )
                    {
                        return new LoginStatusNavPageBag { IsDivider = true };
                    }

                    return new LoginStatusNavPageBag
                    {
                        Title = title,
                        Url = RequestContext.ResolveRockUrl( p[1].ResolveMergeFields( mergeFields ) )
                    };
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Logs the current user out, updates their last activity, and returns
        /// the URL to redirect to after logout.
        /// </summary>
        /// <returns>A redirect URL string.</returns>
        [BlockAction]
        public BlockActionResult Logout()
        {
            if ( RequestContext.CurrentUser != null )
            {
#pragma warning disable 618 // UpdateUserLastActivity is obsolete; the writer is retained during the dual-reader window. See Phase 15 of the PersonSession spec.
                var updateUserLastActivityMsg = new UpdateUserLastActivity.Message
                {
                    UserId = RequestContext.CurrentUser.Id,
                    LastActivityDate = RockDateTime.Now,
                    IsOnline = false
                };
                updateUserLastActivityMsg.Send();
#pragma warning restore 618
            }

            // Check if the current page is viewable by anonymous users.
            // If so, redirect back to the same page; otherwise redirect
            // to the site root.
            string redirectUrl;

            if ( PageCache.IsAuthorized( Authorization.VIEW, null ) )
            {
                redirectUrl = this.GetCurrentPageUrl();
            }
            else
            {
                redirectUrl = "/";
            }

            Authorization.SignOut();

            return ActionOk( redirectUrl );
        }

        #endregion Block Actions
    }
}
