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
using System.Text;

using Humanizer;

using Rock;
using Rock.Attribute;
using Rock.Financial;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GivingOverview;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block used to view the giving overview of a person.
    /// </summary>

    [DisplayName( "Giving Overview" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Block used to view the giving." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Inactive Giver Cutoff (days)",
        Key = AttributeKey.InactiveGiverCutoff,
        Description = "The number of days after which a person is considered an inactive giver.",
        IsRequired = true,
        DefaultIntegerValue = 365,
        Order = 0 )]

    [LinkedPage(
        "Alert List Page",
        Description = "The page to see a list of alerts for the person.",
        Order = 1,
        Key = AttributeKey.AlertListPage,
        DefaultValue = Rock.SystemGuid.Page.GIVING_ALERTS )]

    #endregion

    [ContextAware( typeof( Person ) )]
    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Primary )]
    [Rock.SystemGuid.EntityTypeGuid( "9235199C-8A58-4754-8A7A-4976BB15E466" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "1ECDE465-BE19-4001-BD9B-1E833B4A8F75" )]
    [Rock.SystemGuid.BlockTypeGuid( "896D807D-2110-4007-AFD1-4D953B83375B" )]
    public class GivingOverview : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The HTML escaped "plus or minus" symbol to use for markup strings.
        /// </summary>
        private const string PlusOrMinus = "&#177;";

        #endregion Constants

        #region Keys

        private static class AttributeKey
        {
            public const string InactiveGiverCutoff = "InactiveGiverCutoff";
            public const string AlertListPage = "AlertListPage";
        }

        private static class NavigationUrlKey
        {
            public const string AlertListPage = "AlertListPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string PersonGuid = "PersonGuid";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GivingOverviewBag, GivingOverviewOptionsBag>();
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                box.Bag = new GivingOverviewBag();
                return box;
            }

            box.Bag = GetGivingOverviewBag( person );
            box.NavigationUrls = GetBoxNavigationUrls( person );

            return box;
        }

        /// <summary>
        /// Gets the person to display giving information for, either from the
        /// block context or the page parameter.
        /// </summary>
        /// <returns>The resolved person or <c>null</c>.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Builds the initialization bag with all of the giving overview data
        /// for the person.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <returns>The populated bag.</returns>
        private GivingOverviewBag GetGivingOverviewBag( Person person )
        {
            var bag = new GivingOverviewBag();

            var contributionType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );

            if ( contributionType == null )
            {
                return bag;
            }

            bag.IsVisible = true;

            // Get the past 3 years of monthly giving history.
            var threeYearsAgo = RockDateTime.Now.AddMonths( -35 ).StartOfMonth();
            var threeYearsOfMonthlyAccountGiving = GetMonthlyGivingHistory( person.GivingId, threeYearsAgo );

            if ( !threeYearsOfMonthlyAccountGiving.Any() )
            {
                return bag;
            }

            bag.HasGivingData = true;

            person.LoadAttributes( RockContext );

            var inactiveGiverCutOffDate = RockDateTime.Now.AddDays( -GetAttributeValue( AttributeKey.InactiveGiverCutoff ).AsInteger() ).Date;

            /*
                7/8/26 - MSE

                A gift only counts toward active giving when its month falls on
                or after the cutoff month. Comparing the year with >= would
                treat every gift in the cutoff calendar year as recent, even
                gifts from months before the cutoff, hiding the inactive giver
                warning for people who stopped giving up to a year earlier than
                the configured cutoff.

                Reason: Inactive giver detection must respect the configured cutoff.
            */
            var hasGiftsAfterCutoff = threeYearsOfMonthlyAccountGiving
                .Any( h =>
                    h.Amount > 0 && (
                        h.Year > inactiveGiverCutOffDate.Year ||
                        ( h.Year == inactiveGiverCutOffDate.Year && h.Month >= inactiveGiverCutOffDate.Month )
                    ) );

            if ( !hasGiftsAfterCutoff )
            {
                // The monthly giving history is ordered by most recent month
                // first, so the first record with a positive amount is the
                // month of the last gift.
                var lastGaveMonth = threeYearsOfMonthlyAccountGiving.FirstOrDefault( h => h.Amount > 0 );

                bag.IsInactiveGiver = true;
                bag.InactiveLastGiftText = lastGaveMonth != null
                    ? new DateTime( lastGaveMonth.Year, lastGaveMonth.Month, 1 ).ToString( "MMM yyyy" )
                    : string.Empty;
            }

            bag.GivingByMonth = GetGivingByMonth( threeYearsOfMonthlyAccountGiving );
            bag.GivingStatsKpiHtml = GetGivingStatsKpiHtml( person );
            bag.GivingCharacteristicsKpiHtml = GetGivingCharacteristicsKpiHtml( person );
            bag.StaleWarningText = GetStaleWarningText( person );

            SetCommunityViewData( person, bag );
            SetGivingAlertCounts( person, bag );

            // The collapsed yearly summary only includes the current and previous year.
            bag.YearlySummary = GetYearlySummaries( threeYearsOfMonthlyAccountGiving, RockDateTime.Now.Year - 1 );

            var eraFirstGave = person.GetAttributeValue( "core_EraFirstGave" ).AsDateTime();
            bag.FirstGiftText = $"First Gift: {eraFirstGave.ToElapsedString()}";
            bag.FirstGiftTooltip = eraFirstGave.ToShortDateString();

            var eraLastGave = person.GetAttributeValue( "core_EraLastGave" ).AsDateTime();
            bag.LastGiftText = $"Last Gift: {eraLastGave.ToElapsedString()}";
            bag.LastGiftTooltip = eraLastGave.ToShortDateString();

            return bag;
        }

        /// <summary>
        /// Gets the monthly giving history for the giving group.
        /// </summary>
        /// <param name="givingId">The giving identifier of the person.</param>
        /// <param name="startDate">The optional earliest date to include.</param>
        /// <returns>The monthly giving history ordered by most recent month first.</returns>
        private List<MonthlyAccountGivingHistory> GetMonthlyGivingHistory( string givingId, DateTime? startDate )
        {
            var financialTransactionService = new FinancialTransactionService( RockContext );

            return financialTransactionService.GetGivingAutomationMonthlyAccountGivingHistory( givingId, startDate, includeNegativeTransactions: true );
        }

        /// <summary>
        /// Builds the trailing 36 months of giving amounts for the giving by
        /// month chart, in ascending month order.
        /// </summary>
        /// <param name="givingHistories">The monthly giving history.</param>
        /// <returns>The chart data bags.</returns>
        private List<MonthlyGivingBag> GetGivingByMonth( List<MonthlyAccountGivingHistory> givingHistories )
        {
            var givingByMonth = new List<MonthlyGivingBag>();

            for ( var i = 35; i >= 0; i-- )
            {
                var month = RockDateTime.Now.StartOfMonth().AddMonths( -i );
                var total = givingHistories
                    .Where( h => h.Year == month.Year && h.Month == month.Month )
                    .Sum( h => h.Amount );

                givingByMonth.Add( new MonthlyGivingBag
                {
                    Label = month.ToString( "MMM yyyy" ),
                    Amount = total,
                    FormattedAmount = total.FormatAsCurrency()
                } );
            }

            return givingByMonth;
        }

        /// <summary>
        /// Builds the rendered HTML for the giving statistics KPI section
        /// covering the last twelve months.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <returns>The rendered KPI HTML.</returns>
        private string GetGivingStatsKpiHtml( Person person )
        {
            var financialTransactionService = new FinancialTransactionService( RockContext );
            var oneYearAgo = RockDateTime.Now.AddMonths( -12 );

            var twelveMonthsTransactionsQry = financialTransactionService
                .GetGivingAutomationSourceTransactionQueryByGivingId( person.GivingId, true )
                .Where( t => t.TransactionDateTime >= oneYearAgo );

            var twelveMonthTransactions = twelveMonthsTransactionsQry
                .Select( a => new
                {
                    a.TransactionDateTime,
                    TotalAmountBeforeRefund = a.TransactionDetails
                        .Select( d => d.Amount )
                        .DefaultIfEmpty( 0.0M )
                        .Sum(),

                    // For each Refund (there could be more than one) get the refund amount for each of the refund's Detail records for the Account.
                    // Then sum that up for the total refund amount for the account.
                    TotalRefundAmount = a.Refunds
                        .Select( r => r.FinancialTransaction.TransactionDetails
                            .Sum( d => ( decimal? ) d.Amount ) )
                        .Sum() ?? 0.0M
                } )
                .ToList();

            var last12MonthTotal = twelveMonthTransactions.Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );
            var last12MonthCount = twelveMonthTransactions.Count;

            // Last 12 Months KPI.
            var last12MonthCountText = $"{last12MonthCount} {"gift".PluralizeIf( last12MonthCount != 1 )}";
            var kpiLast12Months = GetKpiShortCode(
                "Last 12 Months",
                $"<span class=\"currency-span\">{FormatAsCurrency( last12MonthTotal )}</span>",
                subValue: $"<div class=\"small\">{last12MonthCountText}</div>" );

            // Last 90 Days KPI, with the growth percent compared to the prior 90 days.
            var oneHundredEightyDaysAgo = RockDateTime.Now.AddDays( -180 );
            var ninetyDaysAgo = RockDateTime.Now.AddDays( -90 );
            var baseGrowthContribution = twelveMonthTransactions
                .Where( t => t.TransactionDateTime >= oneHundredEightyDaysAgo && t.TransactionDateTime < ninetyDaysAgo )
                .Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );
            var last90DaysContribution = twelveMonthTransactions
                .Where( t => t.TransactionDateTime >= ninetyDaysAgo )
                .Sum( t => t.TotalAmountBeforeRefund + t.TotalRefundAmount );

            decimal growthPercent;

            if ( last90DaysContribution == 0 )
            {
                growthPercent = 0;
            }
            else if ( baseGrowthContribution == 0 )
            {
                growthPercent = 100;
            }
            else
            {
                growthPercent = ( last90DaysContribution - baseGrowthContribution ) / baseGrowthContribution * 100;
            }

            var isGrowthPositive = growthPercent >= 0;
            var growthPercentText = Math.Abs( growthPercent ).ToString( "N1" ) + "%";

            // Show HIGH or LOW instead of the growth percent when it is beyond 1000%.
            string growthPercentDisplay;

            if ( growthPercent > 1000 )
            {
                growthPercentDisplay = "HIGH";
            }
            else if ( growthPercent < -1000 )
            {
                growthPercentDisplay = "LOW";
            }
            else
            {
                growthPercentDisplay = growthPercentText;
            }

            var last90DayCount = twelveMonthTransactions.Count( t => t.TransactionDateTime >= ninetyDaysAgo );
            var last90DayCountText = $"{last90DayCount} {"gift".PluralizeIf( last90DayCount != 1 )}";

            string growthPercentClass;
            string growthPercentIcon;

            if ( last90DaysContribution == 0 )
            {
                growthPercentClass = "default";
                growthPercentIcon = "ti-minus";
            }
            else if ( isGrowthPositive )
            {
                growthPercentClass = "success";
                growthPercentIcon = "ti-arrow-up";
            }
            else
            {
                growthPercentClass = "danger";
                growthPercentIcon = "ti-arrow-down";
            }

            var last90DaysSubValue =
$@"<span title=""{growthPercentText}"" class=""small text-{growthPercentClass}"">
    <i class=""ti {growthPercentIcon}""></i>
    {growthPercentDisplay}
</span>
<div class=""small"">{last90DayCountText}</div>";

            var kpiLast90Days = GetKpiShortCode(
                "Last 90 Days",
                $"<span class=\"currency-span\">{FormatAsCurrency( last90DaysContribution )}</span>",
                subValue: last90DaysSubValue );

            // Gives as family / individual KPI.
            var givesAs = person.GivingGroupId.HasValue ? "Family" : "Individual";
            var givesAsIcon = person.GivingGroupId.HasValue ? "ti-users" : "ti-user";
            var kpiGivesAs = GetKpiShortCode( "Gives As", givesAs, icon: givesAsIcon );

            // Giving journey KPI.
            var journeyStage = ( GivingJourneyStage ) person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_CURRENT_GIVING_JOURNEY_STAGE.AsGuid() ).AsInteger();
            var journeyStageName = journeyStage.GetDisplayName();
            var kpiGivingJourney = GetKpiShortCode( "Giving Journey", journeyStageName, icon: "fa fa-hiking" );

            var kpi = kpiLast12Months + kpiLast90Days + kpiGivesAs + kpiGivingJourney;

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            return $"{{[kpis style:'edgeless' iconbackground:'false' columnmin:'180px' columncount:'4' columncountmd:'4' columncountsm:'2']}}{kpi}{{[endkpis]}}".ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Builds the rendered HTML for the giving characteristics KPI section.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <returns>The rendered KPI HTML.</returns>
        private string GetGivingCharacteristicsKpiHtml( Person person )
        {
            var stringBuilder = new StringBuilder();

            // Typical gift KPI.
            var giftAmountMedian = person.GetAttributeValue( "GiftAmountMedian" ).AsDecimal();
            var giftAmountIqr = person.GetAttributeValue( "GiftAmountIQR" ).AsDecimal();

            var typicalGiftKpi = GetKpiShortCode(
                "Typical Gift",
                $"<span class=\"currency-span\">{FormatAsCurrency( giftAmountMedian )}</span>",
                $"{giftAmountIqr}",
                "fa-fw fa-money-bill",
                "left",
                $"A typical gift amount has a median value of ${giftAmountMedian} with a variability of ${giftAmountIqr}." );

            stringBuilder.Append( typicalGiftKpi );

            // Typical frequency KPI for the average days between gifts and the
            // standard deviation of days between gifts.
            var giftFrequencyDaysMean = person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() ).AsDecimal().ToString( "N0" );
            var giftFrequencyDaysMeanUnits = giftFrequencyDaysMean == "1" ? "day" : "days";
            var giftFrequencyDaysStdDev = person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() ).AsDecimal().ToString( "N1" );
            var giftFrequencyDaysStdDevUnits = giftFrequencyDaysStdDev == "1.0" ? "day" : "days";

            var typicalFrequencyKpi = GetKpiShortCode(
                "Typical Frequency",
                giftFrequencyDaysMean + "d",
                $"{PlusOrMinus}{giftFrequencyDaysStdDev}d",
                "fa-fw fa-clock",
                description: $"A typical gift frequency has a mean value of {giftFrequencyDaysMean} {giftFrequencyDaysMeanUnits} with a variability of {giftFrequencyDaysStdDev} {giftFrequencyDaysStdDevUnits}." );

            stringBuilder.Append( typicalFrequencyKpi );

            // Percent of gifts that are scheduled KPI.
            stringBuilder.Append( GetKpiShortCode( "Percent Scheduled", person.GetAttributeValue( "PercentofGiftsScheduled" ).AsInteger() + "%", icon: "fa-fw fa-percentage" ) );

            // Frequency label KPI.
            var frequencyLabelAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );

            if ( frequencyLabelAttribute != null )
            {
                var frequencyLabel = frequencyLabelAttribute.FieldType.Field.GetTextValue( person.GetAttributeValue( "FrequencyLabel" ), frequencyLabelAttribute.ConfigurationValues );
                stringBuilder.Append( GetKpiShortCode( "Frequency", frequencyLabel, icon: "fa-fw fa-calendar-alt", textAlign: "left" ) );
            }

            // Preferred currency KPI.
            var currencyTypeIconCssClassAttr = AttributeCache.Get( Rock.SystemGuid.Attribute.DEFINED_TYPE_CURRENCY_TYPE_ICONCSSCLASS );

            if ( currencyTypeIconCssClassAttr != null )
            {
                var iconCssClass = currencyTypeIconCssClassAttr.DefaultValue;
                var preferredCurrencyGuidValue = person.GetAttributeValue( "PreferredCurrency" ).AsGuidOrNull();
                var preferredCurrencyValue = preferredCurrencyGuidValue.HasValue
                    ? DefinedValueCache.Get( preferredCurrencyGuidValue.Value )
                    : null;

                if ( preferredCurrencyValue != null )
                {
                    if ( preferredCurrencyValue.GetAttributeValue( "IconCssClass" ).IsNotNullOrWhiteSpace() )
                    {
                        iconCssClass = preferredCurrencyValue.GetAttributeValue( "IconCssClass" );
                    }

                    stringBuilder.Append( GetKpiShortCode( "Preferred Currency", preferredCurrencyValue.Value, icon: iconCssClass ) );
                }
                else
                {
                    stringBuilder.Append( GetKpiShortCode( "Preferred Currency", string.Empty, icon: iconCssClass ) );
                }
            }

            // Preferred source KPI.
            var transactionSourceIconCssClassAttr = AttributeCache.Get( Rock.SystemGuid.Attribute.DEFINED_TYPE_TRANSACTION_SOURCE_ICONCSSCLASS );

            if ( transactionSourceIconCssClassAttr != null )
            {
                var iconCssClass = transactionSourceIconCssClassAttr.DefaultValue;
                var preferredSourceGuidValue = person.GetAttributeValue( "PreferredSource" ).AsGuidOrNull();
                var preferredSourceValue = preferredSourceGuidValue.HasValue
                    ? DefinedValueCache.Get( preferredSourceGuidValue.Value )
                    : null;

                if ( preferredSourceValue != null )
                {
                    if ( preferredSourceValue.GetAttributeValue( "IconCssClass" ).IsNotNullOrWhiteSpace() )
                    {
                        iconCssClass = preferredSourceValue.GetAttributeValue( "IconCssClass" );
                    }

                    stringBuilder.Append( GetKpiShortCode( "Preferred Source", preferredSourceValue.Value, icon: iconCssClass ) );
                }
                else
                {
                    stringBuilder.Append( GetKpiShortCode( "Preferred Source", string.Empty, icon: iconCssClass ) );
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );

            return $"{{[kpis columnmin:'200px' style:'edgeless' iconbackground:'false' columncount:'3' columncountmd:'2' columncountsm:'2']}}{stringBuilder}{{[endkpis]}}".ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the warning text shown when the giving characteristics are stale.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <returns>The warning text, or <c>null</c> when the characteristics are current.</returns>
        private string GetStaleWarningText( Person person )
        {
            /* 2021-09-30 MDP

             Rules for when giving characteristics are considered stale

            Show the 'stale' message when the last gift was over { TypicalFrequency + 2* Frequency Standard Deviation } days.

            Message should be worded as:

            The giving characteristics below were generated (stale time) ago at the time of the last gift.
            Information on bin, percentile and typical gift patterns represent values from that time period.

            */

            var lastTransaction = new FinancialTransactionService( RockContext )
                .GetGivingAutomationSourceTransactionQueryByGivingId( person.GivingId )
                .Max( a => ( DateTime? ) a.TransactionDateTime );
            var frequencyMeanDays = person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS.AsGuid() ).AsDecimalOrNull();
            var frequencyStandardDeviationDays = person.GetAttributeValue( Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS.AsGuid() ).AsDecimalOrNull();

            if ( !lastTransaction.HasValue || !frequencyMeanDays.HasValue || !frequencyStandardDeviationDays.HasValue )
            {
                return null;
            }

            var consideredStaleAfterDays = frequencyMeanDays.Value + ( frequencyStandardDeviationDays.Value * 2 );
            var timeSpanSinceLastUpdated = RockDateTime.Now - lastTransaction.Value;

            if ( ( decimal ) timeSpanSinceLastUpdated.TotalDays <= consideredStaleAfterDays )
            {
                return null;
            }

            return $"The giving characteristics below were generated {lastTransaction.ToElapsedString().ToLower()} at the time of the last gift. Information on bin, percentile and typical gift patterns represent values from that time period.";
        }

        /// <summary>
        /// Sets the community view percentile and bin values on the bag.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <param name="bag">The bag to populate.</param>
        private void SetCommunityViewData( Person person, GivingOverviewBag bag )
        {
            var givingPercentileAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );

            if ( givingPercentileAttribute != null )
            {
                var givingPercentile = person.GetAttributeValue( givingPercentileAttribute.Key ).AsInteger();
                var percentileStage = 10 - ( givingPercentile / 10 );

                if ( givingPercentile % 10 == 0 && givingPercentile != 0 )
                {
                    percentileStage += 1;
                }

                bag.GivingPercentile = givingPercentile;
                bag.PercentileStage = percentileStage;
            }

            var givingBinAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_BIN );

            if ( givingBinAttribute != null )
            {
                bag.GivingBin = person.GetAttributeValue( givingBinAttribute.Key ).AsInteger();
            }

            if ( givingPercentileAttribute != null && givingBinAttribute != null )
            {
                bag.CommunityViewHelpText = $"{person.NickName.ToPossessive()} giving is in the {bag.GivingPercentile.Ordinalize()} percentile, this is classified as Bin {bag.GivingBin}.";
            }
        }

        /// <summary>
        /// Sets the gratitude and follow-up alert counts on the bag.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <param name="bag">The bag to populate.</param>
        private void SetGivingAlertCounts( Person person, GivingOverviewBag bag )
        {
            var givingId = person.GivingId;
            var givingIdPersonAliasIdQuery = new PersonAliasService( RockContext )
                .Queryable()
                .Where( a => a.Person.GivingId == givingId )
                .Select( a => a.Id );

            var alertCountsByType = new FinancialTransactionAlertService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => givingIdPersonAliasIdQuery.Contains( a.PersonAliasId ) )
                .GroupBy( a => a.FinancialTransactionAlertType.AlertType )
                .Select( g => new { AlertType = g.Key, Count = g.Count() } )
                .ToList();

            bag.GratitudeAlertCount = alertCountsByType.FirstOrDefault( a => a.AlertType == AlertType.Gratitude )?.Count ?? 0;
            bag.FollowUpAlertCount = alertCountsByType.FirstOrDefault( a => a.AlertType == AlertType.FollowUp )?.Count ?? 0;
        }

        /// <summary>
        /// Builds the yearly contribution summaries, broken down by account,
        /// ordered by most recent year first.
        /// </summary>
        /// <param name="givingHistories">The monthly giving history to summarize.</param>
        /// <param name="minimumYear">The optional earliest year to include.</param>
        /// <returns>The yearly summary bags.</returns>
        private List<ContributionYearSummaryBag> GetYearlySummaries( List<MonthlyAccountGivingHistory> givingHistories, int? minimumYear )
        {
            var monthlyHistories = minimumYear.HasValue
                ? givingHistories.Where( h => h.Year >= minimumYear.Value )
                : givingHistories;

            return monthlyHistories
                .GroupBy( h => h.Year )
                .OrderByDescending( g => g.Key )
                .Select( yearGroup => new ContributionYearSummaryBag
                {
                    Year = yearGroup.Key,
                    Accounts = yearGroup
                        .GroupBy( h => h.AccountId )
                        .Select( accountGroup => new
                        {
                            Account = FinancialAccountCache.Get( accountGroup.Key ),
                            TotalAmount = accountGroup.Sum( h => h.Amount )
                        } )
                        .OrderBy( a => a.Account?.Order ?? 0 )
                        .ThenBy( a => a.Account?.Name ?? string.Empty )
                        .Select( a => new AccountContributionSummaryBag
                        {
                            AccountName = a.Account?.Name ?? string.Empty,
                            FormattedAmount = a.TotalAmount.FormatAsCurrency()
                        } )
                        .ToList(),
                    FormattedTotalAmount = yearGroup.Sum( h => h.Amount ).FormatAsCurrency()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the box navigation URLs.
        /// </summary>
        /// <param name="person">The person whose giving is displayed.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( Person person )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.AlertListPage] = this.GetLinkedPageUrl( AttributeKey.AlertListPage, new Dictionary<string, string>
                {
                    [PageParameterKey.PersonGuid] = person.Guid.ToString()
                } )
            };
        }

        /// <summary>
        /// Gets the kpi shortcode markup for a single KPI.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        /// <param name="subValue">The sub value.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="textAlign">The text align.</param>
        /// <param name="description">The description.</param>
        /// <returns>The kpi shortcode markup.</returns>
        private string GetKpiShortCode( string label, string value, string subValue = "", string icon = "", string textAlign = "", string description = "" )
        {
            if ( subValue.IsNotNullOrWhiteSpace() )
            {
                subValue = $"subvalue:'{subValue}'";
            }

            if ( icon.IsNotNullOrWhiteSpace() )
            {
                icon = $"icon:'{icon}'";
            }

            if ( textAlign.IsNotNullOrWhiteSpace() )
            {
                textAlign = $"textalign:'{textAlign}'";
            }

            return $"[[ kpi {icon} labellocation:'top' value:'{value}' {subValue} label:'{label}' {textAlign} description:'{description}' ]][[ endkpi ]]";
        }

        /// <summary>
        /// Formats the value as a whole-dollar currency string with the first
        /// character (the currency symbol) wrapped in a span for styling.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted currency markup.</returns>
        private string FormatAsCurrency( decimal value )
        {
            var formattedValue = value.FormatAsCurrencyWithDecimalPlaces( 0 );

            return $"<span>{formattedValue.Substring( 0, 1 )}</span>{formattedValue.Substring( 1 )}";
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the yearly contribution summaries for all years of giving history.
        /// </summary>
        /// <returns>The yearly summary bags ordered by most recent year first.</returns>
        [BlockAction]
        public BlockActionResult GetYearlySummary()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "Person not found." );
            }

            var givingHistories = GetMonthlyGivingHistory( person.GivingId, null );

            return ActionOk( GetYearlySummaries( givingHistories, null ) );
        }

        #endregion Block Actions
    }
}
