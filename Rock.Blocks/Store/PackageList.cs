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

using Rock.Attribute;
using Rock.Model;
using Rock.Store;
using Rock.ViewModels.Blocks.Store.PackageList;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Lists Rock Store packages as a card grid.
    /// </summary>

    [DisplayName( "Package List" )]
    [Category( "Store" )]
    [Description( "Lists Rock Store packages." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Set Page Title",
        Description = "Determines if the block should set the page title with the category name (category name must be provided via the query string as &CategoryName=.)",
        DefaultBooleanValue = false,
        Key = AttributeKey.SetPageTitle,
        Order = 0 )]

    [TextField( "Category Id",
        Description = "Filters packages for a specific category id. If none is provided it will show all packages.",
        IsRequired = false,
        Key = AttributeKey.CategoryId,
        Order = 1 )]

    [LinkedPage( "Detail Page",
        Description = "Page reference to use for the detail page.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "9ACBB158-888B-459D-8084-FD07CCAC7E05" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "31B0A0D7-60E6-41A1-BDD4-353F17CBC615" )]
    [Rock.SystemGuid.BlockTypeGuid( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261" )]
    public class PackageList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CategoryId = "CategoryId";
            public const string DetailPage = "DetailPage";
            public const string SetPageTitle = "SetPageTitle";
        }

        private static class PageParameterKey
        {
            public const string PackageId = "PackageId";
            public const string CategoryId = "CategoryId";
            public const string CategoryName = "CategoryName";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetBox();
        }

        /// <summary>
        /// Builds the initialization box for the block, including block settings
        /// and the package data the Obsidian component needs to render.
        /// </summary>
        /// <returns>The initialization box for the block.</returns>
        private PackageListInitializationBox GetBox()
        {
            var box = new PackageListInitializationBox
            {
                NavigationUrls = GetBoxNavigationUrls()
            };

            SetPageTitleIfConfigured();

            var packages = GetPackageListBags( out var storeErrorMessage );

            if ( storeErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.StoreErrorMessage = storeErrorMessage;
                return box;
            }

            box.Packages = packages;

            return box;
        }

        /// <summary>
        /// Sets the page and browser title from the CategoryName query string parameter
        /// when the "Set Page Title" block setting is enabled. Mirrors the behavior of the
        /// legacy WebForms block.
        /// </summary>
        private void SetPageTitleIfConfigured()
        {
            if ( !GetAttributeValue( AttributeKey.SetPageTitle ).AsBoolean() )
            {
                return;
            }

            var categoryName = PageParameter( PageParameterKey.CategoryName );

            if ( categoryName.IsNullOrWhiteSpace() )
            {
                return;
            }

            var pageTitle = $"Items for {categoryName}";
            var siteName = PageCache?.Layout?.Site?.Name;

            RequestContext.Response.SetPageTitle( pageTitle );
            RequestContext.Response.SetBrowserTitle( siteName.IsNotNullOrWhiteSpace() ? $"{pageTitle} | {siteName}" : pageTitle );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.PackageId, "((Key))" )
            };
        }

        /// <summary>
        /// Fetches the packages from the Rock Store API and maps them to bags
        /// for the Obsidian component.
        /// </summary>
        /// <param name="errorMessage">
        /// When the store API call fails, contains the error message returned by the API.
        /// An empty string indicates success.
        /// </param>
        /// <returns>The list of package bags. Empty when the store could not be reached.</returns>
        private List<PackageListItemBag> GetPackageListBags( out string errorMessage )
        {
            // The category can come from the block setting or, when that is blank, the
            // CategoryId query string parameter. This matches the legacy WebForms block.
            var categoryId = GetAttributeValue( AttributeKey.CategoryId ).AsIntegerOrNull()
                ?? PageParameter( PageParameterKey.CategoryId ).AsIntegerOrNull();

            var packageService = new PackageService();
            var packages = packageService.GetAllPackages( categoryId, out errorMessage );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return new List<PackageListItemBag>();
            }

            return packages.Select( p => new PackageListItemBag
            {
                PackageId = p.Id,
                Name = p.Name,
                Vendor = p.Vendor?.Name,
                Rating = p.Rating,
                Price = p.Price,
                IsFree = p.IsFree,
                IconUrl = p.PackageIconBinaryFile?.ImageUrl is string iconUrl
                    ? $"{iconUrl}&h=140&w=280&mode=crop&scale=both"
                    : null,
                DetailPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.PackageId, p.Id.ToString() )
            } ).ToList();
        }

        #endregion Methods
    }
}
