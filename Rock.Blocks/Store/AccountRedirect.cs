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

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Store;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Redirects the client to the organization's account page on the Rock
    /// website, or to the link-organization page when the store has not yet
    /// been configured.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Account Redirect" )]
    [Category( "Store" )]
    [Description( "Redirects client to the organization's account page on the Rock website." )]
    [IconCssClass( "ti ti-external-link" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Link Organization Page",
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.LinkOrganizationPage )]

    #endregion

    // In order for the redirect to take affect, a full page reload is required.
    [ConfigurationChangedReload( Enums.Cms.BlockReloadMode.Page )]

    [Rock.SystemGuid.EntityTypeGuid( "0D2F5E87-6633-46F5-8A46-FF80D255A84C" )]
    //[Rock.SystemGuid.BlockTypeGuid( "79593A20-99E3-4570-80FC-E52AC497EF2A" )]
     [Rock.SystemGuid.BlockTypeGuid( "DBDB7C26-8271-4B1A-9791-71DEC99A7A2F" )]
    public class AccountRedirect : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The base URL of the organization's account page on the Rock website.
        /// The organization's store key is appended to this value.
        /// </summary>
        private const string OrganizationAccountUrlBase = "https://www.rockrms.com/Rock/Organization/";

        #endregion Constants

        #region Properties

        /// <inheritdoc/>
        public override string ObsidianFileUrl => null; // Server-side redirect; no Obsidian component.

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            var redirectUrl = GetRedirectUrl();

            if ( redirectUrl.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            RequestContext.Response.RedirectToUrl( redirectUrl );

            return string.Empty;
        }

        /// <summary>
        /// Resolves the URL the client should be redirected to. When the store
        /// is configured the user is sent to the organization's account page on
        /// the Rock website; otherwise the user is sent to the link-organization
        /// page so the store can be configured.
        /// </summary>
        /// <returns>The resolved redirect URL, or an empty string when no destination could be determined.</returns>
        private string GetRedirectUrl()
        {
            if ( StoreService.OrganizationIsConfigured() )
            {
                var storeKey = StoreService.GetOrganizationKey();

                return OrganizationAccountUrlBase + storeKey;
            }

            // The store has not been configured yet, so send the user to the
            // link-organization page and pass along the current URL so they can
            // be returned here once configuration is complete.
            var queryParams = new Dictionary<string, string>
            {
                ["ReturnUrl"] = RequestContext?.RequestUri?.PathAndQuery
            };

            return this.GetLinkedPageUrl( AttributeKey.LinkOrganizationPage, queryParams );
        }

        #endregion Methods
    }
}
