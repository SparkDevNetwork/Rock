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
using Rock.Model;
using Rock.ViewModels.Blocks.Administration.SparkConnectedServices;
using Rock.ViewModels.Utility;

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
            var initializationBag = new InitializationBag();

            if ( !provider.IsOrganizationLinked() )
            {
                initializationBag.IsOrganizationInvalid = true;
                initializationBag.IsUpgradePossible = provider.IsLegacyOrganizationLinked();

                return initializationBag;
            }

            try
            {
                await EnsureManifestAsync( provider );

                initializationBag.OrganizationIdentifier = provider.GetLegacyOrganizationIdentifier();
                initializationBag.CreditCardSummary = await GetCreditCardSummaryBagAsync( provider );
                initializationBag.RockIntelligence = await GetRockIntelligenceConfigurationAsync( provider );
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

        #endregion
    }
}
