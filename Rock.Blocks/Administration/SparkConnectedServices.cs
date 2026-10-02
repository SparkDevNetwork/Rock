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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Mobile;
using Rock.Model;
using Rock.ViewModels.Blocks.Administration.SparkConnectedServices;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Configures the connected services provided by Spark for use in Rock.
    /// </summary>
    [DisplayName( "Spark Connected Services" )]
    [Category( "Administration" )]
    [Description( "Configures the connected services provided by Spark for use in Rock." )]
    [IconCssClass( "ti ti-affiliate" )]

    [SystemGuid.EntityTypeGuid( "af86a425-26ab-4254-b525-46d007d4b97e" )]
    [SystemGuid.BlockTypeGuid( "8f5f7c7d-cabc-4dca-963e-70b788cd262f" )]
    internal class SparkConnectedServices : RockBlockType
    {
        public override async Task<object> GetObsidianBlockInitializationAsync()
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
            var mobileAppGateway = MobileAppGatewayFactory.Create( provider );
            var initializationBag = new InitializationBag();

            if ( !provider.IsOrganizationLinked() )
            {
                initializationBag.IsOrganizationInvalid = true;
                initializationBag.IsUpgradePossible = provider.IsLegacyOrganizationLinked();

                // ARGUS-LIVE: The fake gateway needs no linked organization, so a development
                // machine can try the card without linking. Remove this line with the fake.
                if ( mobileAppGateway.IsFake )
                {
                    initializationBag.MobileApp = GetMobileAppConfiguration( provider, mobileAppGateway );
                }

                return initializationBag;
            }

            try
            {
                await EnsureManifestAsync( provider );

                initializationBag.OrganizationIdentifier = provider.GetLegacyOrganizationIdentifier();
                initializationBag.CreditCardSummary = await GetCreditCardSummaryBagAsync( provider );
                initializationBag.RockIntelligence = await GetRockIntelligenceConfigurationAsync( provider );
                initializationBag.MobileApp = GetMobileAppConfiguration( provider, mobileAppGateway );
                initializationBag.ManifestLastRefreshedDateTime = GetManifestLastRefreshedDateTime( provider );
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                initializationBag.ErrorTitle = "Configuration Error";
                initializationBag.ErrorDescription = $"There was an error getting the current configuration information: {ex.Message}";

                return initializationBag;
            }

            return initializationBag;
        }

        private async Task EnsureManifestAsync( ConnectedServicesProvider provider )
        {
            var manifest = provider.GetManifest();

            if ( manifest == null )
            {
                await provider.UpdateManifestAsync( CancellationToken.None );
            }
        }

        /// <summary>
        /// Gets the credit card summary information from the connected
        /// services provider
        /// </summary>
        /// <param name="provider">The connected services provider.</param>
        /// <returns>The credit card summary information.</returns>
        private async Task<CreditCardSummaryBag> GetCreditCardSummaryBagAsync( ConnectedServicesProvider provider )
        {
            var summary = await provider.GetCreditCardSummaryAsync( CancellationToken.None );

            return new CreditCardSummaryBag
            {
                CardType = summary.CardType,
                ExpirationMonth = summary.ExpirationMonth,
                ExpirationYear = summary.ExpirationYear,
                IsCardExpired = summary.IsCardExpired,
                IsCardExpiringSoon = summary.IsCardExpiringSoon,
                IsCardOnFile = summary.IsCardOnFile,
                LastFourDigits = summary.LastFourDigits
            };
        }

        /// <summary>
        /// Gets the connected services manifest's last-refreshed timestamp
        /// as a DateTimeOffset in the Rock organization time zone.
        /// </summary>
        /// <returns>The last-refreshed timestamp as a DateTimeOffset, or <c>null</c> if the manifest has never been loaded.</returns>
        private static DateTimeOffset? GetManifestLastRefreshedDateTime( ConnectedServicesProvider provider )
        {
            return provider.GetManifest()
                ?.CreatedDateTime
                .ToOrganizationDateTime()
                .ToRockDateTimeOffset();
        }

        /// <summary>
        /// Builds the ordered list of Rock Intelligence bundles from the
        /// currently cached manifest for use as a drop-down source.
        /// </summary>
        /// <returns>The ordered list of Rock Intelligence bundles as ListItemBag objects.</returns>
        private static List<ListItemBag> GetRockIntelligenceBundleList( List<ServiceBundle> bundles )
        {
            return bundles
                ?.OrderBy( b => b.Order )
                .ThenBy( b => b.Name )
                .Select( b => new ListItemBag
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                } )
                .ToList()
                ?? new List<ListItemBag>();
        }

        private async Task<RockIntelligenceConfigurationBag> GetRockIntelligenceConfigurationAsync( ConnectedServicesProvider provider )
        {
            var bundle = provider.GetConfiguration()?.RockIntelligence?.Bundle;

            if ( bundle == null )
            {
                return null;
            }

            var bag = new RockIntelligenceConfigurationBag
            {
                BundleIdentifier = bundle.Id,
                BundleName = bundle.Name,
            };

            try
            {
                var usage = await provider.GetRockIntelligenceUsageAsync( CancellationToken.None );

                bag.MonthlyUsage = usage.CurrentMonthSpending;
                bag.BalanceRemaining = usage.BalanceRemaining;
                bag.MonthlySpendingLimit = usage.MonthlySpendLimit;
            }
            catch ( Exception ex )
            {
                bag.UsageError = ex.Message;
            }

            return bag;
        }

        /// <summary>
        /// Gets the church's enrollment in the shared mobile application for the card.
        /// </summary>
        /// <param name="provider">The connected services provider.</param>
        /// <param name="gateway">The gateway the card enrolls through.</param>
        /// <returns>The bag, or <c>null</c> when Spark's manifest does not offer the service.</returns>
        private MobileAppConfigurationBag GetMobileAppConfiguration( ConnectedServicesProvider provider, IMobileAppGateway gateway )
        {
            var serviceEntry = provider.GetMobileAppServiceEntry();

            // ARGUS-LIVE: The fake shows the card with no manifest entry. Once Spark lists the
            // service, the manifest is the only gate (card impl spec, section 11).
            if ( serviceEntry == null && !gateway.IsFake )
            {
                return null;
            }

            var stored = provider.GetConfiguration()?.MobileApp;
            var manifestIssue = serviceEntry?.Status == ServiceStatus.Error
                ? serviceEntry.Issue.IfEmpty( "The service is not available right now." )
                : null;

            return new MobileAppConfigurationBag
            {
                IsPlatformSiteBuilt = MultitenantEnrollment.GetPlatformSite( RockContext ) != null,
                IsEnrolled = stored?.IsEnrolled == true,
                IsFakeGateway = gateway.IsFake,
                Name = stored?.Name ?? GlobalAttributesCache.Value( "OrganizationName" ),
                BrandColor = stored?.BrandColor,
                Logo = GetBinaryFileListItemBag( stored?.LogoBinaryFileGuid ),
                ApiUrl = MultitenantEnrollment.GetApiUrl(),
                ChurchCode = stored?.ChurchCode,
                Link = stored?.Link,
                ManifestIssue = manifestIssue
            };
        }

        /// <summary>
        /// Builds the list item an image uploader shows for a binary file.
        /// </summary>
        /// <param name="binaryFileGuid">The binary file Guid, or <c>null</c>.</param>
        /// <returns>The list item, or <c>null</c> if there is no file.</returns>
        private ListItemBag GetBinaryFileListItemBag( Guid? binaryFileGuid )
        {
            if ( !binaryFileGuid.HasValue )
            {
                return null;
            }

            var fileName = new BinaryFileService( RockContext ).GetSelect( binaryFileGuid.Value, b => b.FileName );

            return fileName == null
                ? null
                : new ListItemBag { Value = binaryFileGuid.Value.ToString(), Text = fileName };
        }

        /// <summary>
        /// Builds the response sent after an enrollment change.
        /// </summary>
        /// <param name="provider">The connected services provider.</param>
        /// <param name="gateway">The gateway the card enrolls through.</param>
        /// <returns>The response bag.</returns>
        private SaveMobileAppResponseBag GetSaveMobileAppResponse( ConnectedServicesProvider provider, IMobileAppGateway gateway )
        {
            return new SaveMobileAppResponseBag
            {
                Configuration = GetMobileAppConfiguration( provider, gateway )
            };
        }

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> GetRockIntelligenceConfigurationOptions()
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
            var bundleData = await provider.GetRockIntelligenceBundlesAsync( CancellationToken.None );

            var options = new RockIntelligenceOptionsBag
            {
                Bundles = GetRockIntelligenceBundleList( bundleData.Bundles ),
                SelectedBundleId = bundleData.SelectedBundleId,
                SpendingLimit = ( await provider.GetRockIntelligenceMonthlySpendLimitAsync( CancellationToken.None ) ).Data,
            };

            return ActionOk( options );
        }

        [BlockAction]
        public async Task<BlockActionResult> RefreshManifest()
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();

            try
            {
                await provider.UpdateManifestAsync( CancellationToken.None );
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                return ActionBadRequest( $"There was an error refreshing the manifest: {ex.Message}" );
            }

            return ActionOk( new RefreshManifestResponseBag
            {
                ManifestLastRefreshedDateTime = GetManifestLastRefreshedDateTime( provider ),
            } );
        }

        [BlockAction]
        public async Task<BlockActionResult> SaveRockIntelligence( Guid? bundleIdentifier, decimal? monthlySpendLimit, decimal? oneTimeBoost )
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();

            if ( !bundleIdentifier.HasValue && ( monthlySpendLimit.HasValue || oneTimeBoost.HasValue ) )
            {
                return ActionBadRequest( "You may not disable Rock Intelligence while also attempting to set a monthly spend limit or apply a one-time boost." );
            }

            if ( monthlySpendLimit.HasValue && oneTimeBoost.HasValue )
            {
                return ActionBadRequest( "You may not set a monthly spend limit and apply a one-time boost in the same request." );
            }

            var enabledResult = await provider.SetRockIntelligenceEnabledAsync( bundleIdentifier.HasValue, CancellationToken.None );

            if ( !enabledResult.IsSuccess )
            {
                return ActionBadRequest( enabledResult.ErrorMessage );
            }

            if ( !bundleIdentifier.HasValue )
            {
                return ActionOk( new SaveRockIntelligenceResponseBag
                {
                    Configuration = await GetRockIntelligenceConfigurationAsync( provider ),
                } );
            }

            if ( bundleIdentifier.HasValue )
            {
                var bundleResult = await provider.SetRockIntelligenceBundleAsync( bundleIdentifier.Value, CancellationToken.None );

                if ( !bundleResult.IsSuccess )
                {
                    return ActionBadRequest( bundleResult.ErrorMessage );
                }
            }

            if ( monthlySpendLimit.HasValue )
            {
                var spendLimitResult = await provider.SetRockIntelligenceMonthlySpendLimitAsync( monthlySpendLimit.Value, CancellationToken.None );

                if ( !spendLimitResult.IsSuccess )
                {
                    return ActionBadRequest( spendLimitResult.ErrorMessage );
                }
            }

            // If this is newly provisioned, also charge the credit card for
            // the initial monthly spend limit.
            if ( enabledResult.Data?.NewlyProvisioned == true && monthlySpendLimit.HasValue )
            {
                oneTimeBoost = monthlySpendLimit.Value;
            }

            OneTimeBoostResult boostResult = null;

            if ( oneTimeBoost.HasValue )
            {
                boostResult = await provider.ApplyRockIntelligenceOneTimeBoostAsync( oneTimeBoost.Value, CancellationToken.None );
            }

            return ActionOk( new SaveRockIntelligenceResponseBag
            {
                Configuration = await GetRockIntelligenceConfigurationAsync( provider ),
                BoostStatus = boostResult?.Status.ConvertToInt() ?? 0,
                BoostMessage = boostResult?.Message
            } );
        }

        /// <summary>
        /// Enables the shared mobile application for the church, or updates its
        /// directory listing when it is already enabled.
        /// </summary>
        /// <param name="name">The church name shown in the directory.</param>
        /// <param name="brandColor">The directory color, <c>#RRGGBB</c>.</param>
        /// <param name="logo">The directory logo, or <c>null</c> for none.</param>
        /// <returns>The refreshed enrollment.</returns>
        [BlockAction]
        public async Task<BlockActionResult> SaveMobileApp( string name, string brandColor, ListItemBag logo )
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
            var gateway = MobileAppGatewayFactory.Create( provider );

            if ( !gateway.IsFake && provider.GetMobileAppServiceEntry()?.Status == ServiceStatus.Error )
            {
                return ActionBadRequest( "Spark reports an issue with this service, so it cannot be enabled right now." );
            }

            var options = new MultitenantEnrollmentOptions
            {
                Name = name,
                BrandColor = brandColor,
                LogoBinaryFileGuid = logo?.Value.AsGuidOrNull()
            };

            MultitenantEnrollmentResult result;

            try
            {
                result = await MultitenantEnrollment.SaveAsync( options, gateway, provider, RockContext, CancellationToken.None );
            }
            catch ( InvalidOperationException ex )
            {
                // The provider throws when the organization is not linked or in the demo environment.
                return ActionBadRequest( ex.Message );
            }

            if ( !result.IsSuccess )
            {
                return ActionBadRequest( result.ErrorMessage );
            }

            return ActionOk( GetSaveMobileAppResponse( provider, gateway ) );
        }

        /// <summary>
        /// Takes the church out of the shared mobile application. Installed copies
        /// stop at their next launch and the church leaves the directory; the church
        /// code and its posters come back if it enables again.
        /// </summary>
        /// <returns>The refreshed enrollment.</returns>
        [BlockAction]
        public async Task<BlockActionResult> DisableMobileApp()
        {
            var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
            var gateway = MobileAppGatewayFactory.Create( provider );

            MultitenantEnrollmentResult result;

            try
            {
                result = await MultitenantEnrollment.DisableAsync( gateway, provider, RockContext, CancellationToken.None );
            }
            catch ( InvalidOperationException ex )
            {
                return ActionBadRequest( ex.Message );
            }

            if ( !result.IsSuccess )
            {
                return ActionBadRequest( result.ErrorMessage );
            }

            return ActionOk( GetSaveMobileAppResponse( provider, gateway ) );
        }

        #endregion
    }
}
