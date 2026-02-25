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
using System.Configuration;
using System.Linq;
using System.Web;

using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.Security;
using Rock.Store;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Store.PackageDetail;
using Rock.Web.Cache;

using Package = Rock.Store.Package;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Displays the details of a particular package.
    /// </summary>

    [DisplayName( "Package Detail" )]
    [Category( "Store" )]
    [Description( "Displays the details of a particular package." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [LinkedPage( "Install Page", "Page reference to use for the install / update page.", false, "", "", 1 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "33509fe0-f134-4985-aa7b-fb4dfcfd0775" )]
    //Was [Rock.SystemGuid.BlockTypeGuid( "3304b23b-76f4-4c2c-82ac-0238a014cec1" )]
    [Rock.SystemGuid.BlockTypeGuid( "69A7D88E-5CD8-4993-A88A-4DA15BAD3CB3" )]
    public class PackageDetail : RockBlockType
    {
        private static string _rockVersion = Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber();

        #region Keys

        private static class AttributeKey
        {
            public const string InstallPage = "InstallPage";
        }

        private static class PageParameterKey
        {
            public const string PackageId = "PackageId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string InstallPage = "InstallPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<PackageBag, PackageDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PackageDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new PackageDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PackageBag, PackageDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Person.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Person.FriendlyTypeName );
                }
            }
        }

        /// <inheritdoc/>
        protected PackageBag GetEntityBagForView( Package entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var storeKey = StoreService.GetOrganizationKey();

            var latestCompatibleVersion = GetLatestCompatiblePackageVersion( entity );

            // Check if a newer package version exists but requires a rock update first
            var newestVersion = entity.Versions.OrderByDescending( v => v.Id ).FirstOrDefault();
            var requiresRockUpdate = latestCompatibleVersion != null && newestVersion != null && newestVersion.Id > latestCompatibleVersion.Id;
            var rockUpdateRequirement = requiresRockUpdate ? FormatRequiredRockVersion( newestVersion.RequiredRockSemanticVersion ) : string.Empty;

            var installedStatusMessage = GetInstalledPackageStatus( entity, latestCompatibleVersion );

            var bag = new PackageBag
            {
                IdKey = entity.Id.AsIdKey(),
                Name = entity.Name,
                Description = entity.Description,
                SupportUrl = entity.SupportUrl,
                IsFree = entity.IsFree,
                Price = entity.Price,
                IsPurchased = entity.IsPurchased,
                PurchasedDate = entity.IsPurchased ? entity.PurchasedDate.ToShortDateString() : "",
                VendorName = entity.Vendor.Name,
                VendorUrl = entity.Vendor.Url,
                PackageIconImageUrl = entity.PackageIconBinaryFile.ImageUrl,
                StoreKey = StoreService.GetOrganizationKey(),
                Rating = (int?)entity.Rating,
                LatestVersion = latestCompatibleVersion,
                LatestVersionRatings = GetRatingsForVersion( latestCompatibleVersion.Id ),
                InstalledStatusMessage = installedStatusMessage,
                RatePostBackUrl = GetRockPostbackUrl(storeKey, entity.Id),
                RequiresRockUpdate = requiresRockUpdate,
                RockUpdateRequirement = rockUpdateRequirement,
                Versions = latestCompatibleVersion != null ? GetAdditionalPackageVersions( entity, latestCompatibleVersion.Id ) : new List<PackageVersionBag>()
            };

            return bag;
        }

        private Package GetInitialEntity()
        {
            var packageId = ( PageParameter( PageParameterKey.PackageId ) ).AsIntegerOrNull();

            if( !packageId.HasValue )
            {
                return null;
            }

            var packageService = new PackageService();

            var package = packageService.GetPackage( packageId.Value, out _ );

            return package;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.InstallPage] = this.GetLinkedPageUrl( AttributeKey.InstallPage, "PackageId", PageParameter( PageParameterKey.PackageId ) )
            };
        }

        private PackageVersionBag GetLatestCompatiblePackageVersion(Package package)
        {
            var rockVersion = RockSemanticVersion.Parse( _rockVersion );
            var latestVersion = package.Versions.Where( v => v.RequiredRockSemanticVersion <= rockVersion ).OrderByDescending( v => v.Id ).FirstOrDefault();

            if( latestVersion == null )
            {
                return null;
            }
            var requiredRockVersion = FormatRequiredRockVersion( latestVersion.RequiredRockSemanticVersion );

            return new PackageVersionBag
            {
                Id = latestVersion.Id,
                VersionLabel = latestVersion.VersionLabel,
                DisplayDate = latestVersion.AddedDate.ToString( "MMMM d, yyyy" ),
                Date = latestVersion.AddedDate.ToShortDateString(),
                Description = latestVersion.Description,
                DocumentationUrl = latestVersion.DocumentationUrl,
                RequiredRockVersionDisplay = requiredRockVersion,
                ScreenshotURLs = latestVersion.Screenshots.Select( s => s.ImageUrl ).ToList()
            };
        }

        private List<PackageVersionBag> GetAdditionalPackageVersions( Package package, int latestVersionId )
        {
            var packageRatingSerivce = new PackageVersionRatingService();
            var additionalVersionBags = new List<PackageVersionBag>();
            var additionalVersions = package.Versions.Where( v => v.Id < latestVersionId ).OrderByDescending( v => v.AddedDate ).ToList();

            if ( additionalVersions == null || !additionalVersions.Any() )
            {
                return additionalVersionBags;
            }

            foreach ( var v in additionalVersions )
            {
                var ratingAverage = 0;
                var reviews = packageRatingSerivce.GetPackageVersionRatings( v.Id );

                if ( reviews != null && reviews.Any() )
                {
                    ratingAverage = ( int )reviews.Average( r => r.Rating );
                }
                additionalVersionBags.Add( new PackageVersionBag
                {
                    Id = v.Id,
                    VersionLabel = v.VersionLabel,
                    DisplayDate = v.AddedDate.ToString( "MMMM d, yyyy" ),
                    Description = v.Description,
                    Rating = ratingAverage
                } );
            }
            return additionalVersionBags;
        }


        private List<PackageVersionRatingBag> GetRatingsForVersion( int packageVersionId )
        {
            var ratings = new List<PackageVersionRatingBag>();
            var reviews = new PackageVersionRatingService().GetPackageVersionRatings( packageVersionId );
            if( reviews == null )
            {
                return ratings;
            }

            foreach( var review in reviews )
            {
                var personPhoto = PersonPhotoUrl( review.PersonAlias?.Person?.PhotoUrl );

                ratings.Add( new PackageVersionRatingBag
                {
                    Id = review.Id,
                    Rating = review.Rating,
                    ReviewerName = review.PersonAlias != null && review.PersonAlias.Person != null ? review.PersonAlias.Person.FullName : "Anonymous",
                    Comment = review.Review,
                    CreatedDate = review.AddedDate,
                    ReviewerPhoto = personPhoto ?? ""
                } );
            }

            return ratings;
        }

        private string GetInstalledPackageStatus( Package package, PackageVersionBag latestVersion )
        {
            var installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

            // If not installed
            if ( installedPackage == null )
            {
                if ( package.IsFree || package.IsPurchased )
                {
                    return "Install";
                }
                else
                {
                    return "Buy";
                }
            }
            // If installed, but not the latest version
            else if ( installedPackage.VersionId != latestVersion.Id )
            {
                return "Update";
            }
            //if Installed
            else
            {
                return "Installed";
            }

        }

        private string PersonPhotoUrl( string relativeUrl )
        {
            var url = relativeUrl;

            var localPath = VirtualPathUtility.ToAbsolute( "~" );
            if ( !localPath.EndsWith( "/" ) )
            {
                localPath += "/";
            }

            if ( relativeUrl.StartsWith( localPath, StringComparison.OrdinalIgnoreCase ) )
            {
                url = relativeUrl.Substring( localPath.Length );
            }

            return "https://www.rockrms.com/" + url;
        }

        private string GetRockPostbackUrl( string storeKey, int packageId)
        {
            var installedPackage = InstalledPackageService.InstalledPackageVersion( packageId );

            var baseUrl = ConfigurationManager.AppSettings["RockStoreUrl"].EnsureTrailingForwardslash();

            if ( installedPackage == null )
            {
                return $"{baseUrl}Store/Rate?OrganizationKey={storeKey}&PackageId={packageId}";
            }
            return $"{baseUrl}Store/Rate?OrganizationKey={storeKey}&PackageId={packageId}&InstalledVersionId={installedPackage.VersionId}";
        }

        /// <summary>
        /// Helper method used to format the rock version to correctly display
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static string FormatRequiredRockVersion( RockSemanticVersion version )
        {
            if ( version == null )
            {
                throw new ArgumentNullException( nameof( version ) );
            }

            return version.Major <= 1
                ? $"v{version.Minor}.{version.Patch}"
                : $"v{version.Major}.{version.Minor}";
        }

        /// <inheritdoc/>

        #endregion

        #region Block Actions
        #endregion
    }
}
