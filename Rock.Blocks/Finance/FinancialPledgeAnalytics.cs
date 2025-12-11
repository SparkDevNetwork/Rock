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
using System.Data;
using System.Globalization;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Blocks.Finance.FinancialPledgeAnalytics;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Used to look at pledges using various criteria.
    /// </summary>
    [DisplayName( "Pledge Analytics" )]
    [Category( "Finance" )]
    [Description( "Used to look at pledges using various criteria." )]
    [IconCssClass( "ti ti-report-money" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "BB1A8155-8747-40D9-B645-30384AF15F0E" )]
    [Rock.SystemGuid.BlockTypeGuid( "72B4BBC0-1E8A-46B7-B956-A399624F513C" )]
    public class FinancialPledgeAnalytics : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string PersonDetailPage = "PersonDetailPage";
        }

        private static class PreferenceKey
        {
            // NOTE: Some use legacy filter keys to preserve original stored filters from
            //       WebForms version of the block.
            public const string FilterAccounts = "apAccounts";
            public const string FilterPledgeDateRange = "drpDateRange"; 
            public const string FilterGivingDateRange = "FilterGivingDateRange";
            public const string FilterPledgeAmount = "nrePledgeAmount";
            public const string FilterPercentComplete = "nrePercentComplete";
            public const string FilterAmountGiven = "nreAmountGiven";
            public const string FilterInclude = "Include";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the account identifiers to use when filtering the pledges. Only
        /// pledges that have these accounts will be included.
        /// </summary>
        /// <value>
        /// The account identifiers to use when filtering the batches.
        /// </value>
        protected List<Guid> FilterAccounts => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterAccounts )
            .Split( ',' )
            .Select( s => s.AsGuid() )
            .ToList();
        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new FinancialPledgeAnalyticsBlockBox();

            box.Filters = GetFilterOptions();
            box.PledgeGridBox = GetPledgeGridBuilder().BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            var currencyInfo = new RockCurrencyCodeInfo();
            box.CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list. We prioritize getting the options from
        /// the query string first because if it's a shared/bookmarked URL, we want it to have the exact same filters
        /// as the previous viewing of the page.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FinancialPledgeAnalyticsFiltersBag GetFilterOptions()
        {
            var options = new FinancialPledgeAnalyticsFiltersBag();
            var personPreferences = GetBlockPersonPreferences();

            // Turn the stored FinancialAccount Guids into an array of ListItemBag items.
            FinancialAccountCache.GetByGuids( FilterAccounts ).ToList().ForEach( a =>
            {
                options.PledgeAccounts.Add( new ViewModels.Utility.ListItemBag() { Value = a.Guid.ToString(), Text = a.Name } );
            } );

            options.PledgeDateRangeDelimitedString = personPreferences.GetValue( PreferenceKey.FilterPledgeDateRange );
            options.GivingDateRangeDelimitedString = personPreferences.GetValue( PreferenceKey.FilterGivingDateRange );
            options.PledgeAmountRange = personPreferences.GetValue( PreferenceKey.FilterPledgeAmount );
            options.PercentCompleteRange = personPreferences.GetValue( PreferenceKey.FilterPercentComplete );
            options.GivenAmountRange = personPreferences.GetValue( PreferenceKey.FilterAmountGiven );
            options.IncludeOptions = personPreferences.GetValue( PreferenceKey.FilterInclude ).AsInteger().ToStringSafe();

            return options;
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
                [NavigationUrlKey.PersonDetailPage] = RequestContext.ResolveRockUrl( $"~/Person/((Key))/Contributions" )
            };
        }

        /// <summary>
        /// Gets the grid builder that will provide all the details and values
        /// of the grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        private GridBuilder<PledgeGridDataBag> GetPledgeGridBuilder()
        {
            return new GridBuilder<PledgeGridDataBag>()
                .WithBlock( this )
                .AddField( "personId", a => a.PersonId )
                .AddField( "personGuid", a => a.PersonGuid )
                .AddTextField( "personNickName", a => a.PersonNickName )
                .AddTextField( "personLastName", a => a.PersonLastName )
                .AddTextField( "personFullNameReversed", a => a.PersonFullNameReversed )
                .AddField( "personEmail", a => a.PersonEmail )
                .AddField( "pledgeTotal", a => a.PledgeTotal )
                .AddField( "totalGivingAmount", a => a.TotalGivingAmount )
                .AddField( "percentComplete", a => a.PercentComplete )
                .AddField( "givingCount", a => a.GivingCount );
        }

        /// <summary>
        /// Gets the grid data bag for the pledge grid that will be sent to the client.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="GridDataBag"/>.</returns>
        private GridDataBag GePledgeGridDataBag( RockContext rockContext )
        {
            var filters = new ComputedFilters( GetFilterOptions(), BlockCache );

            var accountIds = filters.AccountIds.ToArray();

            if ( accountIds.Length == 0 )
            {
                return null;
            }

            var pledgeStartDate = filters.PledgeStartDate.Value.Date;
            var pledgeEndDate = filters.PledgeEndDate.Value.Date;
            var givingStartDate = filters.GivingStartDate?.Date;
            var givingEndDate = filters.GivingEndDate?.Date;

            var minPledgeAmount = filters.PledgeAmountLowerValue;
            var maxPledgeAmount = filters.PledgeAmountUpperValue;
            var minComplete = filters.PercentCompleteLowerValue;
            var maxComplete = filters.PercentCompleteUpperValue;
            var minGiftAmount = filters.AmountGivenLowerValue;
            var maxGiftAmount = filters.AmountGivenUpperValue;

            int includeOption = filters.IncludeOption ?? 0;
            bool includePledges = includeOption != 1;
            bool includeGifts = includeOption != 0;

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = this.GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            DataSet ds = new FinancialPledgeService( rockContextAnalytics )
                .GetPledgeAnalyticsDataSet(
                    accountIds,
                    pledgeStartDate, pledgeEndDate,
                    givingStartDate, givingEndDate,
                    minPledgeAmount, maxPledgeAmount,
                    minComplete, maxComplete,
                    minGiftAmount, maxGiftAmount,
                    includePledges, includeGifts
                    );

            System.Data.DataView dv = ds.Tables[0].DefaultView;

            var gridData = dv.ToTable().AsEnumerable()
                .OrderBy( r => FieldOrDefault<string>( r, "LastName" ) )
                .ThenBy( r => FieldOrDefault<string>( r, "NickName" ) )
                .Select( r => new PledgeGridDataBag
                {
                    PersonId = FieldOrDefault<int>( r, "Id" ),
                    PersonGuid = FieldOrDefault<Guid>( r, "Guid" ),
                    PersonFullNameReversed = $"{FieldOrDefault<string>( r, "LastName" ) ?? string.Empty}, {FieldOrDefault<string>( r, "NickName" ) ?? string.Empty}",
                    PersonEmail = FieldOrDefault<string>( r, "Email" ) ?? string.Empty,
                    PledgeTotal = FieldOrDefault<decimal?>( r, "PledgeAmount" ) ?? 0m,
                    PledgeCount = FieldOrDefault<int?>( r, "PledgeCount" ) ?? 0,
                    TotalGivingAmount = FieldOrDefault<decimal?>( r, "GiftAmount" ) ?? 0m,
                    GivingCount = FieldOrDefault<int?>( r, "GiftCount" ) ?? 0,
                    PercentComplete = FieldOrDefault<decimal?>( r, "PercentComplete" ) ?? 0m
                } )
                .ToList();

            return GetPledgeGridBuilder().Build( gridData );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the bag that describes the grid data to be displayed in the block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public virtual BlockActionResult GetBlockData()
        {
            var rockContext = new RockContext();

            var bag = new FinancialPledgeAnalyticsBlockDataBag
            {
                PledgeGridData = GePledgeGridDataBag( rockContext )
            };

            return ActionOk( bag );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains the identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            if ( !IsAllowedToCreateEntitySet( entitySet ) )
            {
                return ActionForbidden( "You are not allowed to create entity sets." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Creates a communication for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the communication.</returns>
        [BlockAction]
        public BlockActionResult CreateGridCommunication( GridCommunicationBag communication )
        {
            if ( communication == null )
            {
                return ActionBadRequest( "No communication data was provided." );
            }

            if ( !IsAllowedToCreateCommunication( communication ) )
            {
                return ActionForbidden( "You are not allowed to create communications." );
            }

            var rockCommunication = GridHelper.CreateCommunication( communication, RequestContext );

            if ( rockCommunication == null )
            {
                return ActionBadRequest( "Grid has no recipients." );
            }

            return ActionOk( rockCommunication.Id.ToString() );
        }

        #endregion Block Actions

        #region Helper Methods

        private static T FieldOrDefault<T>( DataRow row, string column )
        {
            if ( !row.Table.Columns.Contains( column ) )
            {
                return default( T )!;
            }
            var val = row[column];
            if ( val == null || val == DBNull.Value )
            {
                return default( T )!;
            }
            if ( val is T typed )
            {
                return typed;
            }
            var target = Nullable.GetUnderlyingType( typeof( T ) ) ?? typeof( T );
            return ( T ) Convert.ChangeType( val, target, CultureInfo.InvariantCulture );
        }

        private static NumberRange CalculateNumberRangeFromDelimitedValues( string value )
        {
            var numberRange = new NumberRange();
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return numberRange;
            }
            var values = value.Split( ',' );
            if ( values.Length > 0 )
            {
                numberRange.LowerValue = values[0].AsIntegerOrNull();
            }
            if ( values.Length > 1 )
            {
                numberRange.UpperValue = values[1].AsIntegerOrNull();
            }
            return numberRange;
        }

        /*
            12/10/2025 - N.A.

            These helper methods mirror the RockListBlockType to maintain consistency in structure and behavior.

            Reason: Preserve a familiar pattern for easier maintenance and alignment with existing block logic.
        */

        /// <summary>
        /// Checks if the current person is allowed to create the specified
        /// entity set.
        /// </summary>
        /// <param name="entitySetBag">The entity set bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected bool IsAllowedToCreateEntitySet( GridEntitySetBag entitySetBag )
        {
            return true;
        }

        /// <summary>
        /// Checks if the current person is allowed to create the specified
        /// communication.
        /// </summary>
        /// <param name="communicationBag">The communication bag that will be created.</param>
        /// <returns><c>true</c> if the operation is allowed; otherwise, <c>false</c>.</returns>
        protected bool IsAllowedToCreateCommunication( GridCommunicationBag communicationBag )
        {
            return true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Data container for a single row in the pledge analytics grid, combining
        /// some person fields with pledge and giving aggregates for display.
        /// </summary>
        private class PledgeGridDataBag
        {
            /// <summary>
            /// Internal identifier of the person.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Stable unique identifier (GUID) of the person.
            /// </summary>
            public Guid PersonGuid { get; set; }

            /// <summary>
            /// Preferred first name or nickname of the person.
            /// </summary>
            public string PersonNickName { get; set; }

            /// <summary>
            /// Last (family) name of the person.
            /// </summary>
            public string PersonLastName { get; set; }

            /// <summary>
            /// Person’s name formatted as "Last, First".
            /// </summary>
            public string PersonFullNameReversed { get; set; }

            /// <summary>
            /// Primary email address of the person.
            /// </summary>
            public string PersonEmail { get; set; }

            /// <summary>
            /// The giving unit identifier associated with the person (e.g., individual or family giving ID).
            /// </summary>
            public string GivingId { get; set; }

            /// <summary>
            /// PersonId of the giving unit leader; null if not applicable.
            /// </summary>
            public int? GivingLeaderId { get; set; }

            /// <summary>
            /// Number of pledges on record for the current context; null if not calculated.
            /// </summary>
            public int? PledgeCount { get; set; }

            /// <summary>
            /// Sum of all pledge amounts for the current context/campaign.
            /// </summary>
            public decimal PledgeTotal { get; set; }

            /// <summary>
            /// Total amount given in the current context.
            /// </summary>
            public decimal TotalGivingAmount { get; set; }

            /// <summary>
            /// Percentage of the pledge(s) fulfilled (0–100).
            /// </summary>
            public decimal PercentComplete { get; set; }

            /// <summary>
            /// Count of individual gifts applied toward the pledge(s) in the current context.
            /// </summary>
            public int GivingCount { get; set; }
        }

        /// <summary>
        /// Aggregated, normalized filter values computed from the UI filter bag and cache,
        /// used to parameterize the pledge analytics query without additional transformation.
        /// </summary>
        private class ComputedFilters
        {
            /// <summary>
            /// Financial account identifiers (database Ids) resolved from selected account GUIDs; used by the server query.
            /// </summary>
            public List<int> AccountIds { get; set; }

            /// <summary>
            /// Inclusive lower bound of the pledge date range; null means unbounded.
            /// </summary>
            public DateTime? PledgeStartDate { get; set; }

            /// <summary>
            /// Inclusive upper bound of the pledge date range; null means unbounded.
            /// </summary>
            public DateTime? PledgeEndDate { get; set; }

            /// <summary>
            /// Inclusive lower bound of the giving (gift transaction) date range; null means unbounded.
            /// </summary>
            public DateTime? GivingStartDate { get; set; }

            /// <summary>
            /// Inclusive upper bound of the giving (gift transaction) date range; null means unbounded.
            /// </summary>
            public DateTime? GivingEndDate { get; set; }

            /// <summary>
            /// Minimum pledge amount (whole currency units); null means no minimum.
            /// </summary>
            public int? PledgeAmountLowerValue { get; set; }

            /// <summary>
            /// Maximum pledge amount (whole currency units); null means no maximum.
            /// </summary>
            public int? PledgeAmountUpperValue { get; set; }

            /// <summary>
            /// Minimum percent complete (usually 0–100); null means no minimum.
            /// </summary>
            public int? PercentCompleteLowerValue { get; set; }

            /// <summary>
            /// Maximum percent complete (usually 0–100); null means no maximum.
            /// </summary>
            public int? PercentCompleteUpperValue { get; set; }

            /// <summary>
            /// Minimum amount given (whole currency units); null means no minimum.
            /// </summary>
            public int? AmountGivenLowerValue { get; set; }

            /// <summary>
            /// Maximum amount given (whole currency units); null means no maximum.
            /// </summary>
            public int? AmountGivenUpperValue { get; set; }

            /// <summary>
            /// Selected include option from the UI (e.g., pledges, gifts, or both).
            /// </summary>
            public int? IncludeOption { get; set; }

            /// <summary>
            /// Create the computed filters from the data in the filter bag and the attributes found on the block cache.
            /// </summary>
            internal ComputedFilters( FinancialPledgeAnalyticsFiltersBag filterBag, BlockCache blockCache )
            {
                // Convert the guids to ids for the AccountIds needed for the server query later.
                var selectedAccountGuids = filterBag.PledgeAccounts?.ConvertAll( lb => lb.Value.AsGuid() );
                AccountIds = FinancialAccountCache.GetByGuids( selectedAccountGuids ).Select( a => a.Id ).ToList();

                PledgeStartDate = null;
                PledgeEndDate = null;
                GivingStartDate = null;
                GivingEndDate = null;

                // Generate a date range from the SlidingDateRangePicker's value
                var pledgeDateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( filterBag.PledgeDateRangeDelimitedString );

                PledgeStartDate = pledgeDateRange.Start ?? (DateTime? ) System.Data.SqlTypes.SqlDateTime.MinValue;
                PledgeEndDate = pledgeDateRange.End ?? ( DateTime? ) System.Data.SqlTypes.SqlDateTime.MaxValue;

                var givingDateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( filterBag.GivingDateRangeDelimitedString );
                GivingStartDate = givingDateRange.Start ?? (DateTime? ) System.Data.SqlTypes.SqlDateTime.MinValue;
                GivingEndDate = givingDateRange.End ?? ( DateTime? ) System.Data.SqlTypes.SqlDateTime.MaxValue;

                var pledgeRange = CalculateNumberRangeFromDelimitedValues( filterBag.PledgeAmountRange );
                PledgeAmountLowerValue = pledgeRange.LowerValue;
                PledgeAmountUpperValue = pledgeRange.UpperValue;

                var percentRange = CalculateNumberRangeFromDelimitedValues( filterBag.PercentCompleteRange );
                PercentCompleteLowerValue = percentRange.LowerValue;
                PercentCompleteUpperValue = percentRange.UpperValue;

                var givenAmountRange = CalculateNumberRangeFromDelimitedValues( filterBag.GivenAmountRange );
                AmountGivenLowerValue = givenAmountRange.LowerValue;
                AmountGivenUpperValue = givenAmountRange.UpperValue;

                IncludeOption = filterBag.IncludeOptions.ToIntSafe();
            }
        }

        /// <summary>
        /// Used to keep track of the lower and upper values for a number range.
        /// </summary>
        internal class NumberRange
        {
            public int? LowerValue { get; set; } = null;
            public int? UpperValue { get; set; } = null;
        }

        #endregion Helper Classes
    }
}
