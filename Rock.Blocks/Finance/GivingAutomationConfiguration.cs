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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility.Settings.Giving;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.GivingAutomationConfiguration;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Configures the giving automation system: classification thresholds, alerting durations,
    /// and the alert types that drive giving alerts.
    /// </summary>

    [DisplayName( "Giving Automation Configuration" )]
    [Category( "Finance" )]
    [Description( "Block used to view and create new alert types for the giving automation system." )]
    [IconCssClass( "ti ti-filter-dollar" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "FCC5A3ED-C8CB-4F5A-86C4-B69DBB8F8DC5" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3C1D2E62-6223-4FCA-BC6B-4841D6C3A42B" )]
    [Rock.SystemGuid.BlockTypeGuid( "A91ACA78-68FD-41FC-B652-17A37789EA32" )]
    public class GivingAutomationConfiguration : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The account selection-mode values.
        /// </summary>
        private static class AccountTypeKey
        {
            public const string AllTaxDeductible = "AllTaxDeductible";
            public const string Custom = "Custom";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GivingAutomationConfigurationBag, GivingAutomationConfigurationOptionsBag>
            {
                Bag = GetConfigurationBag(),
                Options = GetBoxOptions()
            };

            return box;
        }

        /// <summary>
        /// Builds the bag of saved settings (plus the alert grid data) used to populate the form.
        /// </summary>
        /// <returns>The populated configuration bag.</returns>
        private GivingAutomationConfigurationBag GetConfigurationBag()
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();

            // Only surface saved transaction types that still exist and are active.
            var activeTransactionTypeGuids = GetActiveTransactionTypes().Select( dv => dv.Guid ).ToList();
            var savedTransactionTypeGuids = settings.TransactionTypeGuids
                ?? new List<Guid> { Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() };
            var selectedTransactionTypeGuids = savedTransactionTypeGuids
                .Where( g => activeTransactionTypeGuids.Contains( g ) )
                .Select( g => g.ToString() )
                .ToList();

            // When custom accounts are saved, resolve them to picker items.
            var savedAccountGuids = settings.FinancialAccountGuids ?? new List<Guid>();
            var isCustomAccounts = savedAccountGuids.Any();
            var selectedAccounts = isCustomAccounts
                ? new FinancialAccountService( RockContext ).Queryable()
                    .Where( a => savedAccountGuids.Contains( a.Guid ) )
                    .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                    .ToList()
                : new List<ListItemBag>();

            var journey = settings.GivingJourneySettings;
            var alerting = settings.GivingAlertingSettings;

            return new GivingAutomationConfigurationBag
            {
                IsGivingAutomationEnabled = settings.GivingAutomationJobSettings.IsEnabled,
                DaysToUpdateClassifications = ToDayNumberList( settings.GivingClassificationSettings.RunDays ),
                SelectedTransactionTypeGuids = selectedTransactionTypeGuids,
                AccountType = isCustomAccounts ? AccountTypeKey.Custom : AccountTypeKey.AllTaxDeductible,
                SelectedAccounts = selectedAccounts,
                IsIncludeChildAccounts = settings.AreChildAccountsIncluded ?? false,

                DaysToUpdateGivingJourneys = ToDayNumberList( journey.DaysToUpdateGivingJourneys ),
                NewGiverContributionCountMinimum = journey.NewGiverContributionCountBetweenMinimum,
                NewGiverContributionCountMaximum = journey.NewGiverContributionCountBetweenMaximum,
                NewGiverFirstGaveDays = journey.NewGiverFirstGaveDays,
                ConsistentGiverLastGaveDays = journey.ConsistentGiverLastGaveDays,
                ConsistentGiverMeanFrequency = journey.ConsistentGiverMeanFrequency,
                OccasionalGiverLastGaveDays = journey.OccasionalGiverLastGaveDays,
                OccasionalGiverMeanFrequency = journey.OccasionalGiverMeanFrequency,
                LapsedGiverNoGiftDays = journey.LapsedGiverNoGiftDays,
                LapsedGiverMeanFrequency = journey.LapsedGiverMeanFrequency,

                GlobalRepeatPreventionDurationDays = alerting.GlobalRepeatPreventionDurationDays,
                GratitudeRepeatPreventionDurationDays = alerting.GratitudeRepeatPreventionDurationDays,
                FollowupRepeatPreventionDurationDays = alerting.FollowupRepeatPreventionDurationDays,

                AlertTypes = GetAlertGridData()
            };
        }

        /// <summary>
        /// Builds the read-only options (dropdown data and display flags) for the block.
        /// </summary>
        /// <returns>The populated options bag.</returns>
        private GivingAutomationConfigurationOptionsBag GetBoxOptions()
        {
            var transactionTypes = GetActiveTransactionTypes()
                .Select( dv => new ListItemBag { Value = dv.Guid.ToString(), Text = dv.Value } )
                .ToList();

            var connectionTypes = ConnectionTypeCache.All()
                .OrderBy( t => t.Name )
                .ToListItemBagList();

            // The opportunity's Category holds its parent connection type GUID so the client can
            // filter opportunities by the selected connection type without a server round trip.
            var connectionOpportunities = new ConnectionOpportunityService( RockContext ).Queryable()
                .OrderBy( o => o.Name )
                .Select( o => new ListItemBag
                {
                    Value = o.Guid.ToString(),
                    Text = o.Name,
                    Category = o.ConnectionType.Guid.ToString()
                } )
                .ToList();

            var systemCommunications = new SystemCommunicationService( RockContext ).Queryable()
                .OrderBy( c => c.Title )
                .Select( c => new ListItemBag { Value = c.Guid.ToString(), Text = c.Title } )
                .ToList();

            return new GivingAutomationConfigurationOptionsBag
            {
                TransactionTypes = transactionTypes,
                AccountTypeOptions = new List<ListItemBag>
                {
                    new ListItemBag { Value = AccountTypeKey.AllTaxDeductible, Text = "All Tax Deductible Accounts" },
                    new ListItemBag { Value = AccountTypeKey.Custom, Text = "Custom" }
                },
                ConnectionTypes = connectionTypes,
                ConnectionOpportunities = connectionOpportunities,
                SystemCommunications = systemCommunications,
                AmountSensitivityDescriptions = GetSensitivityDescriptions( isAmount: true ),
                FrequencySensitivityDescriptions = GetSensitivityDescriptions( isAmount: false ),
                IsCampusColumnVisible = CampusCache.All().Count > 1,
                ParentPageUrl = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Gets the active financial transaction types from cache.
        /// </summary>
        /// <returns>The active transaction-type defined values.</returns>
        private static List<DefinedValueCache> GetActiveTransactionTypes()
        {
            return DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )
                .DefinedValues
                .Where( dv => dv.IsActive )
                .ToList();
        }

        /// <summary>
        /// Builds the sensitivity help text for every alert type, keyed by the numeric alert-type value.
        /// </summary>
        /// <param name="isAmount">When <c>true</c> the amount-sensitivity text is returned; otherwise the frequency text.</param>
        /// <returns>A dictionary of alert-type value to help text.</returns>
        private static Dictionary<string, string> GetSensitivityDescriptions( bool isAmount )
        {
            var descriptions = new Dictionary<string, string>();

            foreach ( AlertType alertType in Enum.GetValues( typeof( AlertType ) ) )
            {
                var key = ( ( int ) alertType ).ToString();
                descriptions[key] = isAmount
                    ? FinancialTransactionAlertType.GetAmountSensitivityDescription( alertType )
                    : FinancialTransactionAlertType.GetFrequencySensitivityDescription( alertType );
            }

            return descriptions;
        }

        #endregion Methods

        #region Grid

        /// <summary>
        /// Builds the grid definition for the alert types grid.
        /// </summary>
        /// <returns>The grid builder.</returns>
        private GridBuilder<FinancialTransactionAlertType> GetAlertGridBuilder()
        {
            return new GridBuilder<FinancialTransactionAlertType>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "order", a => a.Order )
                .AddTextField( "name", a => a.Name )
                .AddField( "alertType", a => ( int ) a.AlertType )
                .AddTextField( "campus", a => a.CampusId.HasValue ? CampusCache.Get( a.CampusId.Value )?.Name : null )
                .AddField( "minimumGiftAmount", a => a.MinimumGiftAmount )
                .AddField( "maximumGiftAmount", a => a.MaximumGiftAmount )
                .AddField( "isContinueIfMatched", a => a.ContinueIfMatched )
                .AddField( "hasWorkflow", a => a.WorkflowTypeId.HasValue )
                .AddField( "hasCommunication", a => a.SystemCommunicationId.HasValue )
                .AddField( "hasConnection", a => a.ConnectionOpportunityId.HasValue )
                .AddField( "hasBusEvent", a => a.SendBusEvent )
                .AddField( "hasAccountParticipant", a => a.AccountParticipantSystemCommunicationId.HasValue );
        }

        /// <summary>
        /// Gets the grid data for the configured alert types.
        /// </summary>
        /// <returns>The alert-type grid data.</returns>
        private GridDataBag GetAlertGridData()
        {
            return GetAlertGridBuilder().Build( GetOrderedAlertTypes() );
        }

        /// <summary>
        /// Gets the alert types ordered the same way the grid presents them.
        /// </summary>
        /// <returns>The ordered list of alert types.</returns>
        private List<FinancialTransactionAlertType> GetOrderedAlertTypes()
        {
            return new FinancialTransactionAlertTypeService( RockContext )
                .Queryable()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        #endregion Grid

        #region Block Actions

        /// <summary>
        /// Saves the giving-automation settings.
        /// </summary>
        /// <param name="bag">The settings to save.</param>
        /// <param name="isFilterChangeConfirmed">
        /// Whether the user has confirmed the full attribute recomputation triggered by a transaction-filter change.
        /// </param>
        /// <returns>A result indicating success or that filter-change confirmation is required.</returns>
        [BlockAction]
        public BlockActionResult SaveConfiguration( GivingAutomationConfigurationBag bag, bool isFilterChangeConfirmed )
        {
            var hasRequiredJourneyValues = bag != null
                && bag.NewGiverContributionCountMinimum.HasValue
                && bag.NewGiverContributionCountMaximum.HasValue
                && bag.NewGiverFirstGaveDays.HasValue
                && bag.ConsistentGiverLastGaveDays.HasValue
                && bag.ConsistentGiverMeanFrequency.HasValue
                && bag.OccasionalGiverLastGaveDays.HasValue
                && bag.OccasionalGiverMeanFrequency.HasValue
                && bag.LapsedGiverNoGiftDays.HasValue
                && bag.LapsedGiverMeanFrequency.HasValue;

            if ( !hasRequiredJourneyValues )
            {
                return ActionBadRequest( "All Giving Journey settings are required." );
            }

            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();

            var isCustomAccounts = bag.AccountType == AccountTypeKey.Custom;

            // Validate the selected accounts against the database and reduce them to their GUIDs.
            var selectedAccountGuids = new List<Guid>();
            if ( isCustomAccounts && bag.SelectedAccounts?.Any() == true )
            {
                var requestedGuids = bag.SelectedAccounts
                    .Select( a => a.Value.AsGuid() )
                    .Where( g => g != Guid.Empty )
                    .ToList();

                selectedAccountGuids = new FinancialAccountService( RockContext ).Queryable()
                    .Where( a => requestedGuids.Contains( a.Guid ) )
                    .Select( a => a.Guid )
                    .ToList();
            }

            /*
                Changing the transaction filters (Transaction Types, Accounts, Include Child Accounts)
                invalidates every previously-computed giving attribute, so the Giving Automation job must
                recompute them for all giving units. We surface a confirmation before committing because
                that recomputation is expensive and overwrites system-wide computed values.

                Reason: A filter change forces a full attribute recomputation, so it requires confirmation.
            */
            var originalTransactionTypeGuids = ( settings.TransactionTypeGuids ?? new List<Guid>() ).OrderBy( g => g ).ToList();
            var originalAccountGuids = ( settings.FinancialAccountGuids ?? new List<Guid>() ).OrderBy( g => g ).ToList();
            var originalIncludeChildren = settings.AreChildAccountsIncluded == true;

            var newTransactionTypeGuids = ( bag.SelectedTransactionTypeGuids ?? new List<string>() )
                .Select( g => g.AsGuid() )
                .OrderBy( g => g )
                .ToList();
            var newAccountGuids = selectedAccountGuids.OrderBy( g => g ).ToList();
            var newIncludeChildren = isCustomAccounts && bag.IsIncludeChildAccounts;

            var filtersChanged = !originalTransactionTypeGuids.SequenceEqual( newTransactionTypeGuids )
                || !originalAccountGuids.SequenceEqual( newAccountGuids )
                || originalIncludeChildren != newIncludeChildren;

            if ( filtersChanged && !isFilterChangeConfirmed )
            {
                return ActionOk( new SaveConfigurationResponseBag { IsFilterChangeConfirmationRequired = true } );
            }

            // Transaction filters.
            settings.TransactionTypeGuids = newTransactionTypeGuids;
            settings.FinancialAccountGuids = isCustomAccounts ? selectedAccountGuids : null;
            settings.AreChildAccountsIncluded = isCustomAccounts ? bag.IsIncludeChildAccounts : ( bool? ) null;

            if ( filtersChanged )
            {
                settings.GivingClassificationSettings.FiltersChanged = true;
            }

            // General settings.
            settings.GivingAutomationJobSettings.IsEnabled = bag.IsGivingAutomationEnabled;
            settings.GivingClassificationSettings.RunDays = ToDayArray( bag.DaysToUpdateClassifications );

            // Giving journey settings.
            var journey = settings.GivingJourneySettings;
            journey.DaysToUpdateGivingJourneys = ToDayArray( bag.DaysToUpdateGivingJourneys );
            journey.NewGiverContributionCountBetweenMinimum = bag.NewGiverContributionCountMinimum;
            journey.NewGiverContributionCountBetweenMaximum = bag.NewGiverContributionCountMaximum;
            journey.NewGiverFirstGaveDays = bag.NewGiverFirstGaveDays;
            journey.ConsistentGiverLastGaveDays = bag.ConsistentGiverLastGaveDays;
            journey.ConsistentGiverMeanFrequency = bag.ConsistentGiverMeanFrequency;
            journey.OccasionalGiverLastGaveDays = bag.OccasionalGiverLastGaveDays;
            journey.OccasionalGiverMeanFrequency = bag.OccasionalGiverMeanFrequency;
            journey.LapsedGiverNoGiftDays = bag.LapsedGiverNoGiftDays;
            journey.LapsedGiverMeanFrequency = bag.LapsedGiverMeanFrequency;

            // Alerting settings.
            var alerting = settings.GivingAlertingSettings;
            alerting.GlobalRepeatPreventionDurationDays = bag.GlobalRepeatPreventionDurationDays;
            alerting.GratitudeRepeatPreventionDurationDays = bag.GratitudeRepeatPreventionDurationDays;
            alerting.FollowupRepeatPreventionDurationDays = bag.FollowupRepeatPreventionDurationDays;

            GivingAutomationSettings.SaveGivingAutomationSettings( settings );

            return ActionOk( new SaveConfigurationResponseBag() );
        }

        /// <summary>
        /// Gets the editable detail for a single alert type, or defaults for a new one.
        /// </summary>
        /// <param name="key">The alert type identifier, or empty/null for a new alert type.</param>
        /// <returns>The alert type bag.</returns>
        [BlockAction]
        public BlockActionResult GetAlertType( string key )
        {
            var bag = key.IsNullOrWhiteSpace()
                ? GetNewAlertTypeBag()
                : GetExistingAlertTypeBag( key );

            if ( bag == null )
            {
                return ActionNotFound( "The selected alert type was not found." );
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Creates or updates a single alert type.
        /// </summary>
        /// <param name="bag">The alert type to save.</param>
        /// <returns>The refreshed alert-type grid data.</returns>
        [BlockAction]
        public BlockActionResult SaveAlertType( FinancialTransactionAlertTypeBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No alert type was provided." );
            }

            var service = new FinancialTransactionAlertTypeService( RockContext );
            FinancialTransactionAlertType alertType = null;

            if ( bag.IdKey.IsNotNullOrWhiteSpace() )
            {
                alertType = service.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( alertType == null )
                {
                    return ActionNotFound( "The selected alert type was not found." );
                }
            }

            if ( alertType == null )
            {
                alertType = new FinancialTransactionAlertType();
                service.Add( alertType );
            }

            alertType.Name = bag.Name;
            alertType.CampusId = GetEntityId( CampusCache.GetId, bag.Campus );
            alertType.FinancialAccountId = GetEntityId( FinancialAccountCache.GetId, bag.Account );
            alertType.IncludeChildFinancialAccounts = bag.IsIncludeChildAccounts;
            alertType.AlertType = bag.AlertType;
            alertType.ContinueIfMatched = bag.IsContinueIfMatched;
            alertType.RepeatPreventionDuration = bag.RepeatPreventionDuration;
            alertType.AmountSensitivityScale = bag.AmountSensitivityScale;
            alertType.FrequencySensitivityScale = bag.FrequencySensitivityScale;
            alertType.MinimumGiftAmount = bag.MinimumGiftAmount;
            alertType.MaximumGiftAmount = bag.MaximumGiftAmount;
            alertType.MinimumMedianGiftAmount = bag.MinimumMedianGiftAmount;
            alertType.MaximumMedianGiftAmount = bag.MaximumMedianGiftAmount;
            alertType.MaximumDaysSinceLastGift = bag.MaximumDaysSinceLastGift;
            alertType.DataViewId = GetEntityId( DataViewCache.GetId, bag.PersonDataView );
            alertType.SendBusEvent = bag.IsSendBusEvent;
            alertType.ConnectionOpportunityId = GetEntityId( new ConnectionOpportunityService( RockContext ), bag.ConnectionOpportunityGuid );
            alertType.SystemCommunicationId = GetEntityId( new SystemCommunicationService( RockContext ), bag.DonorSystemCommunicationGuid );
            alertType.AccountParticipantSystemCommunicationId = GetEntityId( new SystemCommunicationService( RockContext ), bag.AccountParticipantSystemCommunicationGuid );

            alertType.WorkflowTypeId = GetEntityId( WorkflowTypeCache.GetId, bag.WorkflowType );
            alertType.AlertSummaryNotificationGroupId = GetEntityId( GroupCache.GetId, bag.NotificationGroup );
            alertType.RunDaysOfWeek = ( bag.RunDays ?? new List<string>() )
                .Select( d => ( DayOfWeek ) d.AsInteger() )
                .AsFlags();

            RockContext.SaveChanges();

            return ActionOk( GetAlertGridData() );
        }

        /// <summary>
        /// Deletes a single alert type.
        /// </summary>
        /// <param name="key">The alert type identifier.</param>
        /// <returns>The refreshed alert-type grid data.</returns>
        [BlockAction]
        public BlockActionResult DeleteAlertType( string key )
        {
            var service = new FinancialTransactionAlertTypeService( RockContext );
            var alertType = service.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( alertType == null )
            {
                return ActionBadRequest( "The selected alert type was not found." );
            }

            if ( !service.CanDelete( alertType, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            service.Delete( alertType );
            RockContext.SaveChanges();

            return ActionOk( GetAlertGridData() );
        }

        /// <summary>
        /// Changes the ordered position of a single alert type.
        /// </summary>
        /// <param name="key">The identifier of the alert type being moved.</param>
        /// <param name="beforeKey">The identifier of the alert type it will be placed before, or null to move to the end.</param>
        /// <returns>An empty result indicating success.</returns>
        [BlockAction]
        public BlockActionResult ReorderAlertType( string key, string beforeKey )
        {
            var alertTypes = GetOrderedAlertTypes();

            if ( !alertTypes.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the current alert-type grid data.
        /// </summary>
        /// <returns>The alert-type grid data.</returns>
        [BlockAction]
        public BlockActionResult GetAlertTypes()
        {
            return ActionOk( GetAlertGridData() );
        }

        #endregion Block Actions

        #region Alert Type Mapping

        /// <summary>
        /// Builds a bag for a new alert type using the default values shown when adding.
        /// </summary>
        /// <returns>A new alert type bag.</returns>
        private FinancialTransactionAlertTypeBag GetNewAlertTypeBag()
        {
            return new FinancialTransactionAlertTypeBag
            {
                IdKey = string.Empty,
                AlertType = AlertType.Gratitude,
                RunDays = ToDayNumberList( DaysOfWeekFlags.All.AsDayOfWeekList() )
            };
        }

        /// <summary>
        /// Builds a bag for an existing alert type, resolving related entities without lazy loading.
        /// </summary>
        /// <param name="key">The alert type identifier.</param>
        /// <returns>The populated bag, or null if the alert type was not found.</returns>
        private FinancialTransactionAlertTypeBag GetExistingAlertTypeBag( string key )
        {
            var alertType = new FinancialTransactionAlertTypeService( RockContext )
                .Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( alertType == null )
            {
                return null;
            }

            // Resolve the connection opportunity and its parent connection type up front.
            string connectionTypeGuid = null;
            string connectionOpportunityGuid = null;
            if ( alertType.ConnectionOpportunityId.HasValue )
            {
                var opportunity = new ConnectionOpportunityService( RockContext ).Queryable()
                    .Where( o => o.Id == alertType.ConnectionOpportunityId.Value )
                    .Select( o => new { o.Guid, ConnectionTypeGuid = o.ConnectionType.Guid } )
                    .FirstOrDefault();

                if ( opportunity != null )
                {
                    connectionOpportunityGuid = opportunity.Guid.ToString();
                    connectionTypeGuid = opportunity.ConnectionTypeGuid.ToString();
                }
            }

            return new FinancialTransactionAlertTypeBag
            {
                IdKey = alertType.IdKey,
                Name = alertType.Name,
                Campus = alertType.CampusId.HasValue ? CampusCache.Get( alertType.CampusId.Value )?.ToListItemBag() : null,
                Account = GetListItem( new FinancialAccountService( RockContext ), alertType.FinancialAccountId, a => a.Name ),
                IsIncludeChildAccounts = alertType.IncludeChildFinancialAccounts,
                AlertType = alertType.AlertType,
                IsContinueIfMatched = alertType.ContinueIfMatched,
                RunDays = ToDayNumberList( ( alertType.RunDaysOfWeek ?? DaysOfWeekFlags.All ).AsDayOfWeekList() ),
                RepeatPreventionDuration = alertType.RepeatPreventionDuration,
                AmountSensitivityScale = alertType.AmountSensitivityScale,
                FrequencySensitivityScale = alertType.FrequencySensitivityScale,
                MinimumGiftAmount = alertType.MinimumGiftAmount,
                MaximumGiftAmount = alertType.MaximumGiftAmount,
                MinimumMedianGiftAmount = alertType.MinimumMedianGiftAmount,
                MaximumMedianGiftAmount = alertType.MaximumMedianGiftAmount,
                MaximumDaysSinceLastGift = alertType.MaximumDaysSinceLastGift,
                PersonDataView = GetListItem( new DataViewService( RockContext ), alertType.DataViewId, d => d.Name ),
                WorkflowType = alertType.WorkflowTypeId.HasValue ? WorkflowTypeCache.Get( alertType.WorkflowTypeId.Value )?.ToListItemBag() : null,
                ConnectionTypeGuid = connectionTypeGuid,
                ConnectionOpportunityGuid = connectionOpportunityGuid,
                DonorSystemCommunicationGuid = GetGuidString( new SystemCommunicationService( RockContext ), alertType.SystemCommunicationId ),
                AccountParticipantSystemCommunicationGuid = GetGuidString( new SystemCommunicationService( RockContext ), alertType.AccountParticipantSystemCommunicationId ),
                IsSendBusEvent = alertType.SendBusEvent,
                NotificationGroup = GetListItem( new GroupService( RockContext ), alertType.AlertSummaryNotificationGroupId )
            };
        }

        #endregion Alert Type Mapping

        #region Private Helpers

        /// <summary>
        /// Converts a collection of days of the week to a list of their numeric string values.
        /// </summary>
        /// <param name="days">The days of the week.</param>
        /// <returns>The numeric day-of-week strings.</returns>
        private static List<string> ToDayNumberList( IEnumerable<DayOfWeek> days )
        {
            return days?.Select( d => ( ( int ) d ).ToString() ).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Converts a list of numeric day-of-week strings to an array of <see cref="DayOfWeek"/>.
        /// </summary>
        /// <param name="dayNumbers">The numeric day-of-week strings.</param>
        /// <returns>The day-of-week array.</returns>
        private static DayOfWeek[] ToDayArray( List<string> dayNumbers )
        {
            return ( dayNumbers ?? new List<string>() )
                .Select( d => ( DayOfWeek ) d.AsInteger() )
                .ToArray();
        }

        /// <summary>
        /// Resolves the integer identifier for the entity referenced by a picker's selected item.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The service used to resolve the identifier.</param>
        /// <param name="bag">The selected list item whose value is the entity GUID.</param>
        /// <returns>The entity identifier, or null when nothing is selected.</returns>
        private static int? GetEntityId<T>( Service<T> service, ListItemBag bag )
            where T : Rock.Data.Entity<T>, new()
        {
            return GetEntityId( service, bag?.Value );
        }

        /// <summary>
        /// Resolves the integer identifier for the entity referenced by a GUID string.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The service used to resolve the identifier.</param>
        /// <param name="guidString">The entity GUID as a string.</param>
        /// <returns>The entity identifier, or null when the GUID is missing or invalid.</returns>
        private static int? GetEntityId<T>( Service<T> service, string guidString )
            where T : Rock.Data.Entity<T>, new()
        {
            var guid = guidString.AsGuidOrNull();
            return guid.HasValue ? service.GetId( guid.Value ) : null;
        }

        /// <summary>
        /// Resolves the integer identifier for the entity referenced by a picker's selected item
        /// using a cache lookup (e.g. <see cref="CampusCache.GetId(Guid)"/>) instead of a database query.
        /// </summary>
        /// <param name="getCachedId">The cache <c>GetId</c> method that resolves a GUID to an identifier.</param>
        /// <param name="bag">The selected list item whose value is the entity GUID.</param>
        /// <returns>The entity identifier, or null when nothing is selected.</returns>
        private static int? GetEntityId( Func<Guid, int?> getCachedId, ListItemBag bag )
        {
            return GetEntityId( getCachedId, bag?.Value );
        }

        /// <summary>
        /// Resolves the integer identifier for the entity referenced by a GUID string
        /// using a cache lookup instead of a database query.
        /// </summary>
        /// <param name="getCachedId">The cache <c>GetId</c> method that resolves a GUID to an identifier.</param>
        /// <param name="guidString">The entity GUID as a string.</param>
        /// <returns>The entity identifier, or null when the GUID is missing or invalid.</returns>
        private static int? GetEntityId( Func<Guid, int?> getCachedId, string guidString )
        {
            var guid = guidString.AsGuidOrNull();
            return guid.HasValue ? getCachedId( guid.Value ) : null;
        }

        /// <summary>
        /// Builds a list item (value = GUID, text = friendly name) for the entity with the given identifier.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The service used to load the entity.</param>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The list item, or null when no identifier is provided or the entity is missing.</returns>
        private static ListItemBag GetListItem<T>( Service<T> service, int? id )
            where T : Rock.Data.Entity<T>, new()
        {
            if ( !id.HasValue )
            {
                return null;
            }

            var entity = service.Get( id.Value );
            return entity?.ToListItemBag();
        }

        /// <summary>
        /// Builds a list item (value = GUID, text = the supplied text) for the entity with the given identifier.
        /// Use this for entities whose <see cref="object.ToString"/> does not return a friendly name.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The service used to load the entity.</param>
        /// <param name="id">The entity identifier.</param>
        /// <param name="getText">A function that returns the display text for the entity.</param>
        /// <returns>The list item, or null when no identifier is provided or the entity is missing.</returns>
        private static ListItemBag GetListItem<T>( Service<T> service, int? id, Func<T, string> getText )
            where T : Rock.Data.Entity<T>, new()
        {
            if ( !id.HasValue )
            {
                return null;
            }

            var entity = service.Get( id.Value );
            return entity != null ? entity.ToListItemBag( getText( entity ) ) : null;
        }

        /// <summary>
        /// Gets the GUID string for the entity with the given identifier.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="service">The service used to load the entity.</param>
        /// <param name="id">The entity identifier.</param>
        /// <returns>The GUID string, or null when no identifier is provided or the entity is missing.</returns>
        private static string GetGuidString<T>( Service<T> service, int? id )
            where T : Rock.Data.Entity<T>, new()
        {
            if ( !id.HasValue )
            {
                return null;
            }

            var entity = service.Get( id.Value );
            return entity?.Guid.ToString();
        }

        #endregion Private Helpers
    }
}
