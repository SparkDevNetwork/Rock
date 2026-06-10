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
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Blocks.Store.PromoList;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Lists Rock Store promotions as a card list or rotator.
    /// </summary>

    [DisplayName( "Promo List" )]
    [Category( "Store" )]
    [Description( "Lists Rock Store promotions." )]
    [IconCssClass( "fa fa-star" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [CustomRadioListField( "Display Style",
        Description = "Determines how the promotions are rendered: a card list or a rotator.",
        ListSource = "Card List,Rotator",
        IsRequired = true,
        DefaultValue = "Card List",
        Key = AttributeKey.DisplayStyle,
        Order = 0 )]

    [CustomRadioListField( "Promo Type",
        Description = "Display the promos of the specified type",
        ListSource = "All, Top Paid, Top Free, Featured",
        IsRequired = true,
        DefaultValue = "Featured",
        Order = 1 )]

    [TextField( "Category Id",
        Description = "Filters promos for a specific category id. If none is provided it will show promos with no category.",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage( "Detail Page",
        Description = "Page reference to use for the detail page.",
        IsRequired = false,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "085D1D51-CC02-4E07-A90D-4E10638B7B46" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "511C6C14-4D98-42CF-B89D-82F4D63C1E49" )]
    [Rock.SystemGuid.BlockTypeGuid( "B8F1B648-8C5F-4529-8F8B-B564C2A19061" )]
    public class PromoList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PromoType = "PromoType";
            public const string CategoryId = "CategoryId";
            public const string DisplayStyle = "DisplayStyle";
            public const string DetailPage = "DetailPage";
        }

        private static class DisplayStyleValue
        {
            public const string CardList = "Card List";
            public const string Rotator = "Rotator";
        }

        private static class PageParameterKey
        {
            public const string PackageId = "PackageId";
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
        /// and the promo data the Obsidian component needs to render.
        /// </summary>
        /// <returns>The initialization box for the block.</returns>
        private PromoListInitializationBox GetBox()
        {
            var box = new PromoListInitializationBox
            {
                DisplayStyle = GetAttributeValue( AttributeKey.DisplayStyle ),
                PanelTitle = GetPanelTitle()
            };

            box.NavigationUrls = GetBoxNavigationUrls();

            var promos = GetPromoListBags( out var storeErrorMessage );

            if ( storeErrorMessage.IsNotNullOrWhiteSpace() )
            {
                box.StoreErrorMessage = storeErrorMessage;
                return box;
            }

            box.Promos = promos;

            return box;
        }

        /// <summary>
        /// Returns the panel heading text for the card list display style based on the
        /// current PromoType block setting. Returns an empty string for the "All" type,
        /// which matches the original Lava templates that had no dedicated heading for that case.
        /// </summary>
        /// <returns>The panel title string, or an empty string when no heading is needed.</returns>
        private string GetPanelTitle()
        {
            return GetAttributeValue( AttributeKey.PromoType ) switch
            {
                "Featured" => "Featured Items",
                "Top Paid" => "Sponsored Apps",
                "Top Free" => "Top Free",
                _ => "All Items"
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "PackageId", "((Key))" )
            };
        }

        /// <summary>
        /// Fetches the promos from the Rock Store API and maps them to bags
        /// for the Obsidian component.
        /// </summary>
        /// <param name="errorMessage">
        /// When the store API call fails, contains the error message returned by the API.
        /// An empty string indicates success.
        /// </param>
        /// <returns>The list of promo bags. Empty when the store could not be reached.</returns>
        private List<PromoBag> GetPromoListBags( out string errorMessage )
        {
            var promoService = new PromoService();

            var categoryParam = GetAttributeValue( AttributeKey.CategoryId );

            int? categoryId = categoryParam.AsIntegerOrNull();

            var promoType = GetAttributeValue( AttributeKey.PromoType );

            bool isTopPaid = promoType == "Top Paid";
            bool isTopFree = promoType == "Top Free";
            bool isFeatured = promoType == "Featured";

            var promos = promoService.GetPromos( categoryId, out errorMessage, isTopFree, isFeatured, isTopPaid );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return new List<PromoBag>();
            }

            var detailPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.PackageId, "{0}" );

            return promos.Select( p => new PromoBag
            {
                PackageId = p.PackageId,
                PackageName = p.PackageName,
                PackageVendor = p.PackageVendor,
                PackageRating = p.PackageRating,
                PackagePrice = p.PackagePrice,
                PackageIconUrl = p.PackageIconBinaryFile?.ImageUrl is string iconUrl
                    ? $"{iconUrl}&h=140&w=280&mode=crop&scale=both"
                    : null,
                ImageLargeUrl = p.ImageLarge?.ImageUrl,
                DetailPageUrl = string.Format( detailPageUrl, p.PackageId )
            } ).ToList();
        }

        #endregion Methods
    }
}
