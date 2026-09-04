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
using Rock.Store;
using Rock.ViewModels.Blocks.Store.StoreHeader;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Shows the organization information used by the Rock Shop.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Store Header" )]
    [Category( "Store" )]
    [Description( "Shows the Organization information used by the Rock Shop." )]
    [IconCssClass( "fa fa-store" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Link Organization Page",
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.LinkOrganizationPage )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "D6D49B49-DAFE-4131-B94C-AA91188F2742" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "E7FD9794-F7A3-4460-95DF-855F3AE49CE6" )]
    [Rock.SystemGuid.BlockTypeGuid( "91355804-4B64-434F-949B-6180E5CC31D9" )]
    public class StoreHeader : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        private static class NavigationUrlKey
        {
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBox();
        }

        /// <summary>
        /// Builds the initialization box for the block, including the configuration
        /// state and the organization data the Obsidian component needs to render.
        /// </summary>
        /// <returns>The initialization box for the block.</returns>
        private StoreHeaderInitializationBox GetBox()
        {
            var storeOrganizationKey = StoreService.GetOrganizationKey();

            var box = new StoreHeaderInitializationBox
            {
                NavigationUrls = GetBoxNavigationUrls(),
                IsConfigured = false
            };

            if ( storeOrganizationKey.IsNullOrWhiteSpace() )
            {
                return box;
            }

            var organization = new OrganizationService().GetOrganization( storeOrganizationKey ).Result;

            if ( organization == null || organization.AverageWeeklyAttendance == 0 )
            {
                return box;
            }

            box.IsConfigured = true;
            box.Organization = GetOrganizationBag( organization );

            return box;
        }

        /// <summary>
        /// Maps a Rock Store organization to the bag sent to the Obsidian component.
        /// </summary>
        /// <param name="organization">The store organization to map.</param>
        /// <returns>The organization bag, or null when no organization was supplied.</returns>
        private StoreOrganizationBag GetOrganizationBag( Organization organization )
        {
            return new StoreOrganizationBag
            {
                Name = organization.Name,
                LogoUrl = organization.LogoUrl,
                City = organization.City,
                State = organization.State,
                AverageWeeklyAttendance = organization.AverageWeeklyAttendance
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.LinkOrganizationPage] = this.GetLinkedPageUrl( AttributeKey.LinkOrganizationPage )
            };
        }

        #endregion Methods
    }
}
