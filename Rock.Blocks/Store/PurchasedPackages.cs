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
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Store;
using Rock.Utility;
using Rock.ViewModels.Blocks.Store.PurchasedPackages;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Purchased Packages" )]
    [Category( "Store" )]
    [Description( "Lists packages that have been purchased in the Rock Store." )]
    [IconCssClass( "ti ti-gift" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page reference to use for the detail page.",
        IsRequired = false,
        Order = 0 )]

    [LinkedPage( "Install Page",
        Key = AttributeKey.InstallPage,
        Description = "Page reference to use for the install / update page.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage( "Link Organization Page",
        Key = AttributeKey.LinkOrganizationPage,
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "781304D9-3782-45B0-8895-65E4B57E064F" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "FFC53C49-972A-4B63-883E-21DDA7652AF6" )]
    [Rock.SystemGuid.BlockTypeGuid( "C0332D98-7CD0-43C2-9810-60F7DF86FBB6" )]
    public class PurchasedPackages : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string InstallPage = "InstallPage";
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        private static class PageParameterKey
        {
            public const string PackageId = "PackageId";
        }

        #endregion Keys

        #region Enums

        /// <summary>
        /// The install state of a purchased package, derived per package by
        /// comparing the locally installed version against the latest version
        /// compatible with this Rock version. Serialized to the bag by name and
        /// mirrored by the client-side enum in <c>types.partial.ts</c>.
        /// </summary>
        private enum InstallState
        {
            /// <summary>
            /// No version of the package is compatible with this Rock version.
            /// </summary>
            NotAvailable,

            /// <summary>
            /// The package is not currently installed.
            /// </summary>
            Install,

            /// <summary>
            /// The package is installed but a newer compatible version exists.
            /// </summary>
            Update,

            /// <summary>
            /// The package is installed and current.
            /// </summary>
            Installed
        }

        #endregion Enums

        #region Fields

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Builds the initialization box describing the purchased packages and
        /// the current store state for the component to render.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private PurchasedPackagesInitializationBox GetInitializationBox()
        {
            // When the store has not been linked to an organization, the
            // component redirects to the Link Organization page
            if ( !StoreService.OrganizationIsConfigured() )
            {
                return new PurchasedPackagesInitializationBox
                {
                    IsStoreConfigured = false,
                    LinkOrganizationPageUrl = GetLinkOrganizationPageUrl()
                };
            }

            var box = new PurchasedPackagesInitializationBox
            {
                IsStoreConfigured = true
            };

            var packages = new PackageService().GetPurchasedPackages( out var errorResponse ) ?? new List<Package>();

            if ( errorResponse.IsNotNullOrWhiteSpace() )
            {
                box.StoreErrorMessage = errorResponse ?? "An unknown error occurred loading purchased packages.";
                return box;
            }

               box.PurchasedPackages = packages.Select( GetPurchasedPackageBag ).ToList();

            return box;
        }

        /// <summary>
        /// Maps a store package to its bag, including the computed install state
        /// and version labels. Ports the per-item logic from the legacy block's
        /// <c>rptPurchasedProducts_ItemDataBound</c> handler.
        /// </summary>
        /// <param name="package">The purchased package.</param>
        /// <returns>The package bag for the component.</returns>
        private PurchasedPackageBag GetPurchasedPackageBag( Package package )
        {
            var rockVersion = RockSemanticVersion.Parse( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );
            var installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );
            var latestVersion = GetLatestCompatibleVersion( package, rockVersion );
            var installState = GetInstallState( installedPackage, latestVersion );

            var sanitizedDescription = package.Description.SanitizeHtml( strict: false );

            var bag = new PurchasedPackageBag
            {
                PackageId = package.Id,
                Name = package.Name,
                Description = sanitizedDescription,
                ShortDescription = GetShortDescription( sanitizedDescription ),
                IconUrl = package.PackageIconBinaryFile?.ImageUrl is string iconUrl
                    ? $"{iconUrl}&h=140&w=280&zoom=2&mode=crop"
                    : null,
                PurchasedDate = package.PurchasedDate != default( DateTime ) ? ( DateTimeOffset? ) package.PurchasedDate : null,
                Purchaser = package.Purchaser,
                InstallState = installState.ToString(),
                DetailPageUrl = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.PackageId, package.Id.ToString() ),
                InstallPageUrl = this.GetLinkedPageUrl( AttributeKey.InstallPage, PageParameterKey.PackageId, package.Id.ToString() )
            };

            // Populate version labels based on install state, mirroring the
            // legacy ItemDataBound text shown in the version notes literal.
            switch ( installState )
            {
                case InstallState.Install:
                    bag.LatestVersionLabel = latestVersion?.VersionLabel;
                    break;

                case InstallState.Update:
                    bag.InstalledVersionLabel = installedPackage?.VersionLabel;
                    bag.LatestVersionLabel = latestVersion?.VersionLabel;
                    break;

                case InstallState.Installed:
                    bag.InstalledVersionLabel = installedPackage?.VersionLabel;
                    break;
            }

            return bag;
        }

        /// <summary>
        /// Strips HTML tags from the description and truncates to
        /// <paramref name="maxLength"/> characters at a word boundary.
        /// Returns <c>null</c> when the plain text fits within the limit so the
        /// component knows no "Show more" button is needed.
        /// </summary>
        /// <param name="html">The raw HTML description from the store API.</param>
        /// <param name="maxLength">Maximum plain-text character count before truncation.</param>
        /// <returns>A truncated plain-text string ending in "…", or <c>null</c>.</returns>
        private static string GetShortDescription( string html, int maxLength = 400 )
        {
            if ( html.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var plainText = Regex.Replace( html, @"<(style|script)[^>]*>[\s\S]*?<\/\1>", " ", RegexOptions.IgnoreCase );

            // Preserve heading text as bold so the short description retains
            // visual hierarchy from the original HTML.
            plainText = Regex.Replace( plainText, @"<h[1-6][^>]*>([\s\S]*?)<\/h[1-6]>", "<strong>$1</strong>", RegexOptions.IgnoreCase );

            plainText = Regex.Replace( plainText, @"<(?!\/?strong\b)[^>]+>", " ", RegexOptions.IgnoreCase );
            plainText = Regex.Replace( plainText, @"\s+", " " ).Trim();

            if ( plainText.Length <= maxLength )
            {
                return null;
            }

            var truncated = plainText.Substring( 0, maxLength );
            var lastSpace = truncated.LastIndexOf( ' ' );

            if ( lastSpace > 0 )
            {
                truncated = truncated.Substring( 0, lastSpace );
            }

            return truncated + "…";
        }

        /// <summary>
        /// Returns the most recent package version whose required Rock version
        /// does not exceed the current Rock version.
        /// </summary>
        /// <param name="package">The package whose versions are inspected.</param>
        /// <param name="rockVersion">The current Rock semantic version.</param>
        /// <returns>The latest compatible version, or <c>null</c> if none exists.</returns>
        private static PackageVersion GetLatestCompatibleVersion( Package package, RockSemanticVersion rockVersion )
        {
            if ( rockVersion == null || package?.Versions == null )
            {
                return null;
            }

            return package.Versions
                .Where( v => v.RequiredRockSemanticVersion <= rockVersion )
                .OrderByDescending( v => v.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines the install state of a package by comparing the installed
        /// version against the latest compatible version.
        /// </summary>
        /// <param name="installedPackage">The locally installed package record, or <c>null</c> if not installed.</param>
        /// <param name="latestVersion">The latest compatible version from the store, or <c>null</c> if none is compatible.</param>
        /// <returns>The install state for the package.</returns>
        private static InstallState GetInstallState( InstalledPackage installedPackage, PackageVersion latestVersion )
        {
            if ( latestVersion == null )
            {
                return InstallState.NotAvailable;
            }

            if ( installedPackage == null )
            {
                return InstallState.Install;
            }

            if ( installedPackage.VersionId != latestVersion.Id )
            {
                return InstallState.Update;
            }

            return InstallState.Installed;
        }

        /// <summary>
        /// Resolves the Link Organization page URL, carrying a ReturnUrl back to
        /// this page so the user returns here after configuring the store.
        /// </summary>
        /// <returns>The Link Organization page URL.</returns>
        private string GetLinkOrganizationPageUrl()
        {
            // Use the page reference rather than RequestUri, which points at the BlockActions API endpoint during a block reload rather than this page.
            var queryParams = new Dictionary<string, string>
            {
                { "ReturnUrl", RequestContext.PageReference?.BuildUrl() }
            };

            return this.GetLinkedPageUrl( AttributeKey.LinkOrganizationPage, queryParams );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Revokes the organization's store key. The component reloads after a
        /// successful revoke, which re-runs initialization and surfaces the
        /// unconfigured-store state.
        /// </summary>
        /// <returns>An empty success result.</returns>
        [BlockAction]
        public BlockActionResult RevokeStoreKey()
        {
            try
            {
                StoreService.RevokeOrganizationKey();
                return ActionOk();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( "Unable to revoke the store key. Please try again." );
            }
        }

        #endregion Block Actions
    }
}
