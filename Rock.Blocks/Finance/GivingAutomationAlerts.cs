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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.GivingAutomationAlerts;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of giving automation alerts based on the current filters.
    /// </summary>

    [DisplayName( "Giving Automation Alerts" )]
    [Category( "Finance" )]
    [Description( "Lists current alerts based on current filters." )]
    [IconCssClass( "ti ti-message" )]
    [SupportedSiteTypes( Rock.Model.SiteType.Web )]

    [LinkedPage( "Transaction Detail Page",
        Description = "The page used to view the details of the transaction that triggered an alert.",
        DefaultValue = Rock.SystemGuid.Page.TRANSACTION_DETAIL_TRANSACTIONS,
        Order = 0,
        Key = AttributeKey.TransactionPage )]

    [LinkedPage( "Automation Configuration Page",
        Description = "The page to configure what criteria should be used to generate alerts.",
        Order = 1,
        Key = AttributeKey.ConfigPage )]

    [Rock.SystemGuid.EntityTypeGuid( "B59ECB97-0B91-4D40-AA90-FA6237BFB4F5" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "14C2AE82-80E2-41FA-A3B9-30623A37170A" )]
    [Rock.SystemGuid.BlockTypeGuid( "0A813EC3-EC36-499B-9EBD-C3388DC7F49D" )]
    [CustomizedGrid]
    public class GivingAutomationAlerts : RockEntityListBlockType<FinancialTransactionAlert>
    {
        #region Fields

        /// <summary>
        /// The parsed alert reason keys, keyed by alert Id. Populated once per grid build in
        /// <see cref="GetListItems"/> so the "isAmountAlert" and "isFrequencyAlert" columns can be
        /// evaluated without re-parsing each alert's <see cref="FinancialTransactionAlert.ReasonsKey"/> JSON.
        /// </summary>
        private Dictionary<int, List<string>> _alertReasons = new Dictionary<int, List<string>>();

        #endregion Fields

        #region Keys

        private static class AttributeKey
        {
            public const string TransactionPage = "TransactionPage";
            public const string ConfigPage = "ConfigPage";
        }

        private static class PageParameterKey
        {
            public const string PersonGuid = "PersonGuid";
            public const string PersonId = "PersonId";
            public const string CampusId = "CampusId";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string AlertTypeId = "AlertTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string TransactionDetailPage = "TransactionDetailPage";
            public const string ConfigurationPage = "ConfigurationPage";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterAlertCategory = "filter-alert-category";
            public const string FilterPerson = "filter-person";
            public const string FilterTransactionAmountLower = "filter-transaction-amount-lower";
            public const string FilterTransactionAmountUpper = "filter-transaction-amount-upper";
            public const string FilterCampus = "filter-campus";
            public const string FilterAlertTypes = "filter-alert-types";
        }

        #endregion Keys

        #region Properties

        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the date range by which to filter the alerts.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the alert categories (<see cref="AlertType"/>) by which to filter the alerts.
        /// </summary>
        private List<AlertType> FilterAlertCategories => ( BlockPersonPreferences
            .GetValue( PreferenceKey.FilterAlertCategory )
            .FromJsonOrNull<List<string>>() ?? new List<string>() )
            .Select( v => v.ConvertToEnumOrNull<AlertType>() )
            .Where( v => v.HasValue )
            .Select( v => v.Value )
            .ToList();

        /// <summary>
        /// Gets the <see cref="PersonAlias"/> unique identifier of the person by whom to filter the alerts.
        /// </summary>
        private Guid? FilterPersonAliasGuid => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterPerson )
            .FromJsonOrNull<ListItemBag>()?.Value.AsGuidOrNull();

        /// <summary>
        /// Gets the lower bound of the transaction amount by which to filter the alerts.
        /// </summary>
        private decimal? FilterTransactionAmountLower => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterTransactionAmountLower )
            .AsDecimalOrNull();

        /// <summary>
        /// Gets the upper bound of the transaction amount by which to filter the alerts.
        /// </summary>
        private decimal? FilterTransactionAmountUpper => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterTransactionAmountUpper )
            .AsDecimalOrNull();

        /// <summary>
        /// Gets the unique identifier of the <see cref="Campus"/> by which to filter the alerts.
        /// </summary>
        private Guid? FilterCampusGuid => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>()?.Value.AsGuidOrNull();

        /// <summary>
        /// Gets the unique identifiers of the alert types by which to filter the alerts.
        /// </summary>
        private List<Guid> FilterAlertTypeGuids => ( BlockPersonPreferences
            .GetValue( PreferenceKey.FilterAlertTypes )
            .FromJsonOrNull<List<string>>() ?? new List<string>() )
            .Select( v => v.AsGuidOrNull() )
            .Where( v => v.HasValue )
            .Select( v => v.Value )
            .ToList();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GivingAutomationAlertsOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private GivingAutomationAlertsOptionsBag GetBoxOptions()
        {
            var currencyInfo = new RockCurrencyCodeInfo();

            return new GivingAutomationAlertsOptionsBag
            {
                AlertTypeItems = GetAlertTypeItems(),
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                },
                IsPersonContext = GetContextPersonId().HasValue,
                IsCampusContext = GetContextCampusId().HasValue,
                IsAlertTypeContext = GetContextAlertTypeId().HasValue,
                IsDateRangeContext = PageParameter( PageParameterKey.StartDate ).AsDateTime().HasValue
                    || PageParameter( PageParameterKey.EndDate ).AsDateTime().HasValue,
                HasMultipleCampuses = CampusCache.All( false ).Count > 1
            };
        }

        /// <summary>
        /// Gets the alert types used to populate the "Alert Types" filter.
        /// </summary>
        /// <returns>A list of alert type items, keyed by their unique identifier.</returns>
        private List<ListItemBag> GetAlertTypeItems()
        {
            return new FinancialTransactionAlertTypeService( RockContext )
                .Queryable()
                .AsNoTracking()
                .OrderBy( at => at.Name )
                .Select( at => new { at.Guid, at.Name } )
                .ToList()
                .Select( at => new ListItemBag { Text = at.Name, Value = at.Guid.ToString() } )
                .ToList();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.TransactionDetailPage] = this.GetLinkedPageUrl( AttributeKey.TransactionPage, "TransactionId", "((Key))" ),
                [NavigationUrlKey.ConfigurationPage] = this.GetLinkedPageUrl( AttributeKey.ConfigPage )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransactionAlert> GetListQueryable( RockContext rockContext )
        {
            var qry = base.GetListQueryable( rockContext )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.FinancialTransactionAlertType );

            // Filter by date range. A start/end date supplied through the page parameters
            // takes precedence over the interactive date range filter.
            var dateRange = GetEffectiveDateRange();
            var startDateTime = dateRange.StartDate;
            var endDateTime = dateRange.EndDate;

            if ( startDateTime.HasValue )
            {
                qry = qry.Where( a => a.AlertDateTime >= startDateTime.Value );
            }

            if ( endDateTime.HasValue )
            {
                qry = qry.Where( a => a.AlertDateTime <= endDateTime.Value );
            }

            // Filter by alert type. A single alert type supplied through the page parameters
            // takes precedence over the interactive alert type and alert category filters.
            var contextAlertTypeId = GetContextAlertTypeId();
            if ( contextAlertTypeId.HasValue )
            {
                qry = qry.Where( a => a.AlertTypeId == contextAlertTypeId.Value );
            }
            else
            {
                var alertTypeIds = GetSelectedAlertTypeIds( rockContext );
                if ( alertTypeIds.Any() )
                {
                    qry = qry.Where( a => alertTypeIds.Contains( a.AlertTypeId ) );
                }

                var alertCategories = FilterAlertCategories;
                if ( alertCategories.Any() )
                {
                    qry = qry.Where( a => alertCategories.Contains( a.FinancialTransactionAlertType.AlertType ) );
                }
            }

            // Filter by the person's giving id.
            var givingId = GetEffectiveGivingId();
            if ( givingId.IsNotNullOrWhiteSpace() )
            {
                qry = qry.Where( a => a.GivingId == givingId );
            }

            // Filter by the transaction amount.
            if ( FilterTransactionAmountLower.HasValue )
            {
                var lowerAmount = FilterTransactionAmountLower.Value;
                qry = qry.Where( a => a.Amount >= lowerAmount );
            }

            if ( FilterTransactionAmountUpper.HasValue )
            {
                var upperAmount = FilterTransactionAmountUpper.Value;
                qry = qry.Where( a => a.Amount <= upperAmount );
            }

            // Filter by campus. A campus supplied through the page parameters takes precedence
            // over the interactive campus filter. Campus-agnostic alert types (those without a
            // campus) apply to every campus and are therefore always included.
            var campusId = GetEffectiveCampusId();
            if ( campusId.HasValue )
            {
                qry = qry.Where( a => !a.FinancialTransactionAlertType.CampusId.HasValue
                    || a.FinancialTransactionAlertType.CampusId == campusId.Value );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransactionAlert> GetOrderedListQueryable( IQueryable<FinancialTransactionAlert> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.AlertDateTime );
        }

        /// <inheritdoc/>
        protected override List<FinancialTransactionAlert> GetListItems( IQueryable<FinancialTransactionAlert> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            // Parse each alert's reason keys a single time so the "isAmountAlert" and
            // "isFrequencyAlert" grid columns can be evaluated without re-parsing the JSON per column.
            _alertReasons = items.ToDictionary(
                a => a.Id,
                a => a.ReasonsKey.FromJsonOrNull<List<string>>() ?? new List<string>() );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialTransactionAlert> GetGridBuilder()
        {
            return new GridBuilder<FinancialTransactionAlert>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "alertCategory", a => a.FinancialTransactionAlertType.AlertType )
                .AddDateTimeField( "alertDateTime", a => a.AlertDateTime )
                .AddPersonField( "person", a => a.PersonAlias?.Person )
                .AddTextField( "campus", a => GetCampusName( a ) )
                .AddTextField( "alertName", a => a.FinancialTransactionAlertType.Name )
                .AddField( "amount", a => a.Amount )
                .AddTextField( "transactionIdKey", a => a.TransactionId.HasValue ? IdHasher.Instance.GetHash( a.TransactionId.Value ) : null )
                .AddField( "amountMedianDifference", a => a.Amount - a.AmountCurrentMedian )
                .AddField( "amountCurrentMedian", a => a.AmountCurrentMedian )
                .AddField( "amountCurrentIqr", a => a.AmountCurrentIqr )
                .AddField( "frequencyDifferenceFromMean", a => a.FrequencyDifferenceFromMean )
                .AddField( "frequencyCurrentMean", a => a.FrequencyCurrentMean )
                .AddField( "frequencyCurrentStandardDeviation", a => a.FrequencyCurrentStandardDeviation )
                .AddField( "isAmountAlert", a => HasReason( a, nameof( FinancialTransactionAlertType.AmountSensitivityScale ) ) )
                .AddField( "isFrequencyAlert", a => HasReason( a, nameof( FinancialTransactionAlertType.FrequencySensitivityScale ) ) );
        }

        #endregion Methods

        #region Private Helpers

        /// <summary>
        /// Gets the name of the campus associated with the alert's alert type, or <c>null</c> when the
        /// alert type is not scoped to a campus.
        /// </summary>
        /// <param name="alert">The alert whose alert type campus name is requested.</param>
        /// <returns>The campus name, or <c>null</c>.</returns>
        private static string GetCampusName( FinancialTransactionAlert alert )
        {
            var campusId = alert.FinancialTransactionAlertType?.CampusId;
            return campusId.HasValue ? CampusCache.Get( campusId.Value )?.Name : null;
        }

        /// <summary>
        /// Determines whether the alert was triggered for the given reason, using the reason keys
        /// parsed once per grid build in <see cref="GetListItems"/>. The reasons are stored on the
        /// alert as a JSON-serialized array of <see cref="FinancialTransactionAlertType"/> property names.
        /// </summary>
        /// <param name="alert">The alert to inspect.</param>
        /// <param name="reasonKey">The reason property name to look for.</param>
        /// <returns><c>true</c> if the alert was triggered for the given reason; otherwise <c>false</c>.</returns>
        private bool HasReason( FinancialTransactionAlert alert, string reasonKey )
        {
            return _alertReasons.TryGetValue( alert.Id, out var reasons ) && reasons.Contains( reasonKey );
        }

        /// <summary>
        /// Resolves the effective date range, giving precedence to a start/end date supplied through
        /// the page parameters over the interactive date range filter.
        /// </summary>
        /// <returns>The effective start and end dates, either of which may be <c>null</c>.</returns>
        private (DateTime? StartDate, DateTime? EndDate) GetEffectiveDateRange()
        {
            var startParam = PageParameter( PageParameterKey.StartDate ).AsDateTime();
            var endParam = PageParameter( PageParameterKey.EndDate ).AsDateTime();

            if ( startParam.HasValue || endParam.HasValue )
            {
                var endOfDay = endParam.HasValue ? ( DateTime? ) endParam.Value.Date.AddDays( 1 ).AddTicks( -1 ) : null;
                return (startParam?.Date, endOfDay);
            }

            var dateRange = FilterDateRange?.ToActualDateRange();
            return (dateRange?.Start, dateRange?.End);
        }

        /// <summary>
        /// Gets the giving id of the person to filter by, giving precedence to a person supplied
        /// through the page parameters over the interactive person filter.
        /// </summary>
        /// <returns>The person's giving id, or <c>null</c> when no person is selected.</returns>
        private string GetEffectiveGivingId()
        {
            var personId = GetContextPersonId();

            if ( !personId.HasValue && FilterPersonAliasGuid.HasValue )
            {
                personId = new PersonAliasService( RockContext ).GetPersonId( FilterPersonAliasGuid.Value );
            }

            if ( !personId.HasValue )
            {
                return null;
            }

            return new PersonService( RockContext ).GetSelect( personId.Value, p => p.GivingId );
        }

        /// <summary>
        /// Gets the identifier of the person supplied through the page parameters, resolving either
        /// the <c>PersonId</c> (Id, IdKey, or Guid) or the legacy <c>PersonGuid</c> parameter.
        /// </summary>
        /// <returns>The person identifier, or <c>null</c> when no person is in context.</returns>
        private int? GetContextPersonId()
        {
            var personIdParam = PageParameter( PageParameterKey.PersonId );
            if ( personIdParam.IsNotNullOrWhiteSpace() )
            {
                var personId = new PersonService( RockContext ).Get( personIdParam, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
                if ( personId.HasValue )
                {
                    return personId;
                }
            }

            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            if ( personGuid.HasValue )
            {
                return new PersonService( RockContext ).GetId( personGuid.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the identifier of the campus supplied through the page parameters (Id, IdKey, or Guid).
        /// </summary>
        /// <returns>The campus identifier, or <c>null</c> when no campus is in context.</returns>
        private int? GetContextCampusId()
        {
            var campusParam = PageParameter( PageParameterKey.CampusId );
            if ( campusParam.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return CampusCache.Get( campusParam, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
        }

        /// <summary>
        /// Gets the identifier of the alert type supplied through the page parameters (Id, IdKey, or Guid).
        /// </summary>
        /// <returns>The alert type identifier, or <c>null</c> when no alert type is in context.</returns>
        private int? GetContextAlertTypeId()
        {
            var alertTypeParam = PageParameter( PageParameterKey.AlertTypeId );
            if ( alertTypeParam.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new FinancialTransactionAlertTypeService( RockContext )
                .Get( alertTypeParam, !PageCache.Layout.Site.DisablePredictableIds )?.Id;
        }

        /// <summary>
        /// Resolves the effective campus identifier, giving precedence to a campus supplied through
        /// the page parameters over the interactive campus filter.
        /// </summary>
        /// <returns>The campus identifier, or <c>null</c> when no campus is selected.</returns>
        private int? GetEffectiveCampusId()
        {
            var campusId = GetContextCampusId();
            if ( campusId.HasValue )
            {
                return campusId;
            }

            if ( FilterCampusGuid.HasValue )
            {
                return CampusCache.Get( FilterCampusGuid.Value )?.Id;
            }

            return null;
        }

        /// <summary>
        /// Resolves the selected alert type filter values (unique identifiers) to their integer identifiers.
        /// </summary>
        /// <param name="rockContext">The data context to query against.</param>
        /// <returns>The selected alert type identifiers.</returns>
        private List<int> GetSelectedAlertTypeIds( RockContext rockContext )
        {
            var alertTypeGuids = FilterAlertTypeGuids;
            if ( !alertTypeGuids.Any() )
            {
                return new List<int>();
            }

            return new FinancialTransactionAlertTypeService( rockContext )
                .Queryable()
                .Where( at => alertTypeGuids.Contains( at.Guid ) )
                .Select( at => at.Id )
                .ToList();
        }

        #endregion Private Helpers
    }
}
