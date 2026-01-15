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
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Helper class for the Giving Automation job.
    /// </summary>
    internal static class GivingAutomationHelper
    {
        #region Stored Procedures

        /// <summary>
        /// Processes the giving journeys and updates the associated attribute values.
        /// </summary>
        /// <returns>The number of journey stages changed.</returns>
        internal static int UpdateJourneyStages( GivingAutomationSettings settings )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = 180;

                var journeySettings = settings.GivingJourneySettings;
                var filters = new FinancialTransactionService( rockContext ).GetGivingAutomationFilterIds();

                var now = new SqlParameter( "@CurrentRockDateTime", ( object ) RockDateTime.Now );
                var transactionTypeIds = filters.TransactionTypeIds.ConvertToIdListParameter( "@TransactionTypeIds" );
                var financialAccountIds = filters.FinancialAccountIds.ConvertToIdListParameter( "@FinancialAccountIds" );

                var newGiverFirstGaveDays = new SqlParameter( "@NewGiverFirstGaveDays", journeySettings.NewGiverFirstGaveDays );
                var newGiverContributionCountBetweenMinimum = new SqlParameter( "@NewGiverContributionCountBetweenMinimum", journeySettings.NewGiverContributionCountBetweenMinimum );
                var newGiverContributionCountBetweenMaximum = new SqlParameter( "@NewGiverContributionCountBetweenMaximum", journeySettings.NewGiverContributionCountBetweenMaximum );
                var consistentGiverLastGaveDays = new SqlParameter( "@ConsistentGiverLastGaveDays", journeySettings.ConsistentGiverLastGaveDays );
                var consistentGiverMeanFrequency = new SqlParameter( "@ConsistentGiverMeanFrequency", journeySettings.ConsistentGiverMeanFrequency );
                var occasionalGiverLastGaveDays = new SqlParameter( "@OccasionalGiverLastGaveDays", journeySettings.OccasionalGiverLastGaveDays );
                var occasionalGiverMeanFrequency = new SqlParameter( "@OccasionalGiverMeanFrequency", journeySettings.OccasionalGiverMeanFrequency );
                var lapsedGiverNoGiftDays = new SqlParameter( "@LapsedGiverNoGiftDays", journeySettings.LapsedGiverNoGiftDays );
                var lapsedGiverMeanFrequency = new SqlParameter( "@LapsedGiverMeanFrequency", journeySettings.LapsedGiverMeanFrequency );

                return rockContext.Database.SqlQuery<int>(
                    @"EXEC [dbo].[spGivingAutomation_UpdateGivingJourneyStages] 
                        @CurrentRockDateTime, 
                        @TransactionTypeIds, 
                        @FinancialAccountIds, 
                        @NewGiverFirstGaveDays, 
                        @NewGiverContributionCountBetweenMinimum,
                        @NewGiverContributionCountBetweenMaximum,
                        @ConsistentGiverLastGaveDays, 
                        @ConsistentGiverMeanFrequency, 
                        @OccasionalGiverLastGaveDays, 
                        @OccasionalGiverMeanFrequency, 
                        @LapsedGiverNoGiftDays,
                        @LapsedGiverMeanFrequency",
                    now,
                    transactionTypeIds,
                    financialAccountIds,
                    newGiverFirstGaveDays,
                    newGiverContributionCountBetweenMinimum,
                    newGiverContributionCountBetweenMaximum,
                    consistentGiverLastGaveDays,
                    consistentGiverMeanFrequency,
                    occasionalGiverLastGaveDays,
                    occasionalGiverMeanFrequency,
                    lapsedGiverNoGiftDays,
                    lapsedGiverMeanFrequency )
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Updates giving bins and percentiles (stored as attributes) via a stored procedure.
        /// </summary>
        internal static void UpdateGivingBinsAndPercentiles()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = 180;

                var filters = new FinancialTransactionService( rockContext ).GetGivingAutomationFilterIds();

                var now = new SqlParameter( "@CurrentRockDateTime", ( object ) RockDateTime.Now );
                var transactionTypeIds = filters.TransactionTypeIds.ConvertToIdListParameter( "@TransactionTypeIds" );
                var financialAccountIds = filters.FinancialAccountIds.ConvertToIdListParameter( "@FinancialAccountIds" );

                rockContext.Database.ExecuteSqlCommand(
                    @"EXEC [dbo].[spGivingAutomation_UpdateGivingBinsAndPercentiles]
                        @CurrentRockDateTime,
                        @TransactionTypeIds, 
                        @FinancialAccountIds",
                    now,
                    transactionTypeIds,
                    financialAccountIds );
            }
        }

        #endregion Stored Procedures

        #region Alerts

        /// <summary>
        /// Gets the alert type account ids as a set.
        /// Returns null if no FinancialAccountId is specified on the alert type (meaning "all accounts").
        /// </summary>
        internal static HashSet<int> GetAlertTypeAccountIds( FinancialTransactionAlertType alertType )
        {
            if ( alertType?.FinancialAccountId.HasValue != true )
            {
                return null;
            }

            var alertTypeAccountIds = new HashSet<int> { alertType.FinancialAccountId.Value };

            if ( alertType.IncludeChildFinancialAccounts )
            {
                var childAccountIds = alertType.FinancialAccount?.ChildAccounts?.Select( a => a.Id );
                if ( childAccountIds != null )
                {
                    foreach ( var id in childAccountIds )
                    {
                        alertTypeAccountIds.Add( id );
                    }
                }
            }

            return alertTypeAccountIds;
        }

        /// <summary>
        /// Determines whether follow-up alerts are allowed based on recent alerts and repeat-prevention settings.
        /// </summary>
        /// <param name="recentAlerts">The recent alerts for a giving unit (last 12 months).</param>
        /// <param name="now">The current time.</param>
        /// <param name="globalRepeatPreventionDays">The global repeat-prevention duration, in days.</param>
        /// <param name="followUpRepeatPreventionDays">The follow-up repeat-prevention duration, in days.</param>
        internal static bool AllowFollowUpAlerts( List<AlertView> recentAlerts, DateTime now, int? globalRepeatPreventionDays, int? followUpRepeatPreventionDays )
        {
            if ( recentAlerts == null || !recentAlerts.Any() )
            {
                return true;
            }

            if ( globalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Max( x => x.AlertDateTime );
                var daysSinceLastAlert = ( now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= globalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastFollowUpAlerts = recentAlerts.Where( a => a.AlertType == AlertType.FollowUp );
            if ( lastFollowUpAlerts?.Any() == true )
            {
                var lastFollowUpAlertDate = lastFollowUpAlerts.Max( x => ( DateTime? ) x.AlertDateTime );
                if ( followUpRepeatPreventionDays.HasValue && lastFollowUpAlertDate.HasValue )
                {
                    var daysSinceLastFollowUpAlert = ( now - lastFollowUpAlertDate.Value ).TotalDays;

                    if ( daysSinceLastFollowUpAlert <= followUpRepeatPreventionDays.Value )
                    {
                        // This group has follow-up alerts within the repeat duration. Don't create any new follow-up alerts.
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether gratitude alerts are allowed based on recent alerts and repeat-prevention settings.
        /// </summary>
        /// <param name="recentAlerts">The recent alerts for a giving unit (last 12 months).</param>
        /// <param name="now">The current time.</param>
        /// <param name="globalRepeatPreventionDays">The global repeat-prevention duration, in days.</param>
        /// <param name="gratitudeRepeatPreventionDays">The gratitude repeat-prevention duration, in days.</param>
        internal static bool AllowGratitudeAlerts( List<AlertView> recentAlerts, DateTime now, int? globalRepeatPreventionDays, int? gratitudeRepeatPreventionDays )
        {
            if ( !recentAlerts.Any() )
            {
                return true;
            }

            if ( globalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Max( x => x.AlertDateTime );
                var daysSinceLastAlert = ( now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= globalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastGratitudeAlerts = recentAlerts.Where( a => a.AlertType == AlertType.Gratitude );

            if ( lastGratitudeAlerts?.Any() == true )
            {
                var lastGratitudeAlertDate = lastGratitudeAlerts.Max( a => ( DateTime? ) a.AlertDateTime );
                if ( gratitudeRepeatPreventionDays.HasValue && lastGratitudeAlertDate.HasValue )
                {
                    var daysSinceLastGratitudeAlert = ( now - lastGratitudeAlertDate.Value ).TotalDays;

                    if ( daysSinceLastGratitudeAlert <= gratitudeRepeatPreventionDays.Value )
                    {
                        // This group has gratitude alerts within the repeat duration. Don't create any new gratitude alerts.
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Computes per-alert-type metrics (quartiles, frequency stats, history counts) based on
        /// ordered 12-month transaction history for a single GivingId.
        /// </summary>
        /// <param name="alertTypes">The alert types that are configured to 'run today'.</param>
        /// <param name="orderedTransactions">The giving unit's 12-month transactions, ordered by TransactionDateTime.</param>
        /// <param name="transactionWindowDurationHours">The duration of the window within which transactions are considered a single giving event.</param>
        internal static Dictionary<int, AlertTypeComputedMetrics> ComputeMetricsForAlertTypes(
            List<FinancialTransactionAlertType> alertTypes,
            List<FinancialTransactionView> orderedTransactions,
            int transactionWindowDurationHours )
        {
            var computedMetrics = new Dictionary<int, AlertTypeComputedMetrics>();

            if ( alertTypes == null || orderedTransactions == null || orderedTransactions.Count == 0 )
            {
                return computedMetrics;
            }

            foreach ( var alertType in alertTypes )
            {
                if ( alertType == null )
                {
                    continue;
                }

                computedMetrics[alertType.Id] = ComputeMetricsForAlertType( alertType, orderedTransactions, transactionWindowDurationHours );
            }

            return computedMetrics;
        }

        /// <summary>
        /// Computes per-alert-type metrics (quartiles, frequency stats, history counts) for a single alert type.
        /// This is the core implementation used by <see cref="ComputeMetricsForAlertTypes"/>.
        /// </summary>
        internal static AlertTypeComputedMetrics ComputeMetricsForAlertType(
            FinancialTransactionAlertType alertType,
            List<FinancialTransactionView> orderedTransactions,
            int transactionWindowDurationHours )
        {
            var alertTypeAccountIds = GetAlertTypeAccountIds( alertType );

            var transactionsForAlertType = alertTypeAccountIds == null
                ? ( orderedTransactions ?? new List<FinancialTransactionView>() )
                : ( orderedTransactions ?? new List<FinancialTransactionView>() )
                    .Where( t => t.GetTransactionDetails().Any( ftd => alertTypeAccountIds.Contains( ftd.AccountId ) ) )
                    .ToList();

            var transactionDateTimesForAlertType = transactionsForAlertType
                .Select( t => t.TransactionDateTime )
                .ToList();

            var transactionAmountsForAlertType = alertTypeAccountIds == null
                ? transactionsForAlertType
                    .SelectMany( t => t.GetTransactionDetails() )
                    .Select( ftd => ftd.Amount )
                    .ToList()
                : transactionsForAlertType
                    .SelectMany( t => t.GetTransactionDetails() )
                    .Where( ftd => alertTypeAccountIds.Contains( ftd.AccountId ) )
                    .Select( ftd => ftd.Amount )
                    .ToList();

            return new AlertTypeComputedMetrics
            {
                AlertTypeAccountIds = alertTypeAccountIds,
                QuartileRanges = GetQuartileRanges( transactionAmountsForAlertType ),
                FrequencyStats = GetFrequencyStats( transactionDateTimesForAlertType, transactionWindowDurationHours ),
                MostRecentTransaction = transactionsForAlertType.LastOrDefault(),
                TransactionCount = transactionDateTimesForAlertType.Count
            };
        }

        #endregion Alerts

        #region Math

        /// <summary>
        /// Calculates giving-frequency statistics from ordered transaction date/times, applying transaction-window grouping.
        /// </summary>
        /// <param name="orderedTransactionDateTimes">The ordered transaction date/times to analyze.</param>
        /// <param name="transactionWindowDurationHours">The duration in hours within which multiple transactions are treated as a single giving event.</param>
        internal static FrequencyCalculationResult GetFrequencyStats( List<DateTime> orderedTransactionDateTimes, int transactionWindowDurationHours )
        {
            var dayGaps = new List<decimal>();
            DateTime? lastKeptDate = null;

            foreach ( var dt in orderedTransactionDateTimes )
            {
                if ( !lastKeptDate.HasValue )
                {
                    lastKeptDate = dt;
                    continue;
                }

                if ( ( dt - lastKeptDate.Value ).TotalHours < transactionWindowDurationHours )
                {
                    continue; // too recent to previous event, so ignore it. 
                }

                dayGaps.Add( ( decimal ) ( dt - lastKeptDate.Value ).TotalDays );
                lastKeptDate = dt;
            }

            decimal meanDays = dayGaps.Any() ? dayGaps.Average() : 0;
            double mean = ( double ) meanDays;

            double variance = dayGaps.Any() ? dayGaps.Average( d => Math.Pow( ( double ) d - mean, 2 ) ) : 0;

            decimal stdDevDays = ( decimal ) Math.Sqrt( variance );
            double stdDev = ( double ) stdDevDays;

            var nextExpectedGiftDate = lastKeptDate.HasValue ? lastKeptDate.Value.AddDays( ( double ) mean ) : ( DateTime? ) null;

            var label = FinancialGivingAnalyticsFrequencyLabel.Undetermined;

            if ( mean >= 4.5 && mean <= 8.5 && stdDev < 7 )
            {
                label = FinancialGivingAnalyticsFrequencyLabel.Weekly;
            }
            else if ( mean >= 9 && mean <= 17 && stdDev < 10 )
            {
                label = FinancialGivingAnalyticsFrequencyLabel.BiWeekly;
            }
            else if ( mean >= 25 && mean <= 35 && stdDev < 10 )
            {
                label = FinancialGivingAnalyticsFrequencyLabel.Monthly;
            }
            else if ( mean >= 80 && mean <= 110 && stdDev < 15 )
            {
                label = FinancialGivingAnalyticsFrequencyLabel.Quarterly;
            }
            else if ( mean / 2 < stdDev )
            {
                label = FinancialGivingAnalyticsFrequencyLabel.Erratic;
            }

            return new FrequencyCalculationResult
            {
                MeanDays = meanDays,
                StdDevDays = stdDevDays,
                FrequencyLabel = label,
                NextExpectedGiftDate = nextExpectedGiftDate,
                LastTransactionDateTime = lastKeptDate
            };
        }

        /// <summary>
        /// Splits the total amount of each transactions into Interquartile Ranges.
        /// Ex: 1,2,3,4,5,6,7,8,9,10 =&gt; (1,2,3,4), (5,6), (7,8,9,10)
        /// Ex: 1,2,3,4,5,6,7,8,9,10,11 =&gt; (1,2,3,4, 5), (6), (7,8,9,10, 11)
        /// </summary>
        /// <param name="transactionAmounts">The transaction amounts.</param>
        /// <returns>QuartileRanges.</returns>
        internal static QuartileRanges GetQuartileRanges( IEnumerable<decimal> transactionAmounts )
        {
            var orderedValues = transactionAmounts.OrderBy( a => a ).ToList();

            var count = orderedValues.Count;

            if ( count <= 2 )
            {
                return new QuartileRanges
                {
                    Q1MedianRange = new List<decimal>(),
                    Q2MedianRange = orderedValues,
                    Q3MedianRange = new List<decimal>(),
                };
            }

            var lastMidIndex = count / 2;
            var isSingleMidIndex = count % 2 != 0;
            var firstMidIndex = isSingleMidIndex ? lastMidIndex : lastMidIndex - 1;

            var medianValues = isSingleMidIndex ?
                orderedValues.GetRange( firstMidIndex, 1 ) :
                orderedValues.GetRange( firstMidIndex, 2 );

            var q1 = orderedValues.GetRange( 0, firstMidIndex );
            var q3 = orderedValues.GetRange( lastMidIndex + 1, count - lastMidIndex - 1 );

            return new QuartileRanges
            {
                Q1MedianRange = q1,
                Q2MedianRange = medianValues,
                Q3MedianRange = q3,
            };
        }

        /// <summary>
        /// Calculates the percent of transactions that are scheduled.
        /// </summary>
        /// <param name="transactions">The transactions to evaluate.</param>
        internal static int GetPercentScheduled( List<FinancialTransactionView> transactions )
        {
            var txCount = transactions.Count;
            var scheduledCount = transactions.Count( t => t.IsScheduled );

            var asDecimal = ( decimal ) scheduledCount / txCount;
            return ( int ) decimal.Round( asDecimal * 100 );
        }

        /// <summary>
        /// Gets the preferred source defined value Guid from a set of transactions (tie-breaker: most recent transaction).
        /// </summary>
        /// <param name="transactions">The transactions to evaluate.</param>
        internal static Guid? GetPreferredSourceGuid( List<FinancialTransactionView> transactions )
        {
            var sourceGroups = transactions
                .Where( t => t.SourceTypeValueId.HasValue )
                .GroupBy( t => t.SourceTypeValueId )
                .OrderByDescending( g => g.Count() );

            var maxSourceCount = sourceGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredSourceTx = sourceGroups
                .Where( g => g.Count() == maxSourceCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime )
                .FirstOrDefault();

            if ( preferredSourceTx?.SourceTypeValueId.HasValue == true )
            {
                return DefinedValueCache.Get( preferredSourceTx.SourceTypeValueId.Value )?.Guid;
            }

            return null;
        }

        /// <summary>
        /// Gets the preferred currency defined value Guid from a set of transactions (tie-breaker: most recent transaction).
        /// </summary>
        /// <param name="transactions">The transactions to evaluate.</param>
        internal static Guid? GetPreferredCurrencyGuid( List<FinancialTransactionView> transactions )
        {
            var currencyGroups = transactions
                .Where( t => t.CurrencyTypeValueId.HasValue )
                .GroupBy( t => t.CurrencyTypeValueId )
                .OrderByDescending( g => g.Count() );

            var maxCurrencyCount = currencyGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredCurrencyTx = currencyGroups
                .Where( g => g.Count() == maxCurrencyCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime )
                .FirstOrDefault();

            if ( preferredCurrencyTx?.CurrencyTypeValueId.HasValue == true )
            {
                return DefinedValueCache.Get( preferredCurrencyTx.CurrencyTypeValueId.Value )?.Guid;
            }

            return null;
        }

        /// <summary>
        /// Gets the amount IQR count (Measure of how much of an outlier the amount is).
        /// IQR is kind of similar to Std Dev, but excludes outliers.
        /// https://www.statology.org/interquartile-range-vs-standard-deviation/
        /// This would be how much the specified amount compares with the normal deviation.
        /// <para>Positive values indicate a larger amount than usual.</para>
        /// <para>
        /// Example:
        /// </para>
        /// For example, if they give between $400 and $600 (Average: $500). The normal deviation is  +/-$100.
        /// <br />
        /// If the specified amount is $1250, that is $750 more than the average.
        /// That is $750 is 7.5x bigger than than their normal deviation (-/+ 100).
        /// <br />
        /// Therefore, IQR Count would be 7.5.
        /// </summary>
        /// <param name="quartileRanges">The quartile ranges.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>System.Decimal.</returns>
        internal static decimal GetAmountIqrCount( QuartileRanges quartileRanges, decimal amount )
        {
            // For the purpose of having a high number that is reasonable and also does not overflow any c# type, I am choosing
            // a constant to represent infinity other than max value of any particular type
            var infinity = 1000;
            var negativeInfinity = 0 - infinity;

            // Check the number of IQRs that the amount varies
            var medianGiftAmount = quartileRanges.MedianAmount;
            var amountIqr = quartileRanges.IQRAmount;
            var amountDeviation = amount - medianGiftAmount;
            decimal numberOfAmountIqrs;

            if ( amountDeviation == 0 )
            {
                numberOfAmountIqrs = 0;
            }
            else if ( amountIqr != 0 )
            {
                numberOfAmountIqrs = amountDeviation / amountIqr;
            }
            else
            {
                // If the amount IQR is 0, then this giving group gives the same amount every time and even a $1 increase would be an infinite
                // number of IQRs since the formula is dividing by zero. Since we don't want alerts for scenarios like an increase of $1, we use
                // a fallback formula for IQR.
                // Use 15% of the median amount or $100 if the median amount is somehow $0.
                amountIqr = 0.15m * medianGiftAmount;

                if ( amountIqr == 0 )
                {
                    // Shouldn't happen. They somehow have given $0.00 every time, multiple times. Also, we only query for amount > $0.00, so it really should not happen.
                    amountIqr = 100m;
                }

                numberOfAmountIqrs = amountDeviation / amountIqr;
            }

            // Make sure the calculation doesn't exceed "infinity"
            if ( numberOfAmountIqrs > infinity )
            {
                numberOfAmountIqrs = infinity;
            }
            else if ( numberOfAmountIqrs < negativeInfinity )
            {
                numberOfAmountIqrs = negativeInfinity;
            }

            return numberOfAmountIqrs;
        }

        /// <summary>
        /// Gets the frequency deviation count.
        /// <para>Positive values indicate <i>earlier</i> than usual.</para>
        /// </summary>
        /// <param name="frequencyStdDev">The frequency standard dev.</param>
        /// <param name="frequencyMean">The frequency mean.</param>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <returns>System.Decimal.</returns>
        internal static decimal GetFrequencyDeviationCount( decimal frequencyStdDev, decimal frequencyMean, decimal daysSinceLastTransaction )
        {
            // For the purpose of having a high number that is reasonable and also does not overflow any c# type, I am choosing
            // a constant to represent infinity other than max value of any particular type
            const int FrequencyStdDevsInfinity = 1000;
            const int FrequencyStdDevsNegativeInfinity = 0 - FrequencyStdDevsInfinity;

            var frequencyDeviation = frequencyMean - daysSinceLastTransaction;

            decimal numberOfFrequencyStdDevs;

            if ( frequencyDeviation == 0 )
            {
                numberOfFrequencyStdDevs = 0;
            }
            else if ( frequencyStdDev >= 1 )
            {
                numberOfFrequencyStdDevs = frequencyDeviation / frequencyStdDev;
            }
            else
            {
                // If the frequency std dev is less than 1, then this giving group gives the same interval and even a 1.1 day change would be a large
                // number of std devs since the formula is dividing by zero. Since we don't want alerts for scenarios like being 1 day early, we use
                // a fallback formula for std dev.
                frequencyStdDev = 0.15M * frequencyMean;

                if ( frequencyStdDev < 3 )
                {

                    frequencyStdDev = 3;
                }

                numberOfFrequencyStdDevs = frequencyDeviation / frequencyStdDev;
            }

            // Make sure the calculation doesn't exceed "infinity"
            if ( numberOfFrequencyStdDevs > FrequencyStdDevsInfinity )
            {
                numberOfFrequencyStdDevs = FrequencyStdDevsInfinity;
            }
            else if ( numberOfFrequencyStdDevs < FrequencyStdDevsNegativeInfinity )
            {
                numberOfFrequencyStdDevs = FrequencyStdDevsNegativeInfinity;
            }

            return numberOfFrequencyStdDevs;
        }

        #endregion Math
    }

    #region DTOs

    /// <summary>
    /// Represents computed giving-frequency statistics for a transaction series.
    /// </summary>
    internal sealed class FrequencyCalculationResult
    {
        /// <summary>
        /// Gets or sets the mean number of days between giving events.
        /// </summary>
        public decimal MeanDays { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation (in days) between giving events.
        /// </summary>
        public decimal StdDevDays { get; set; }

        /// <summary>
        /// Gets or sets the computed giving frequency label.
        /// </summary>
        public FinancialGivingAnalyticsFrequencyLabel FrequencyLabel { get; set; }

        /// <summary>
        /// Gets or sets the next expected gift date based on the computed mean.
        /// </summary>
        public DateTime? NextExpectedGiftDate { get; set; }

        /// <summary>
        /// The last transaction date that was kept after applying the transaction-window grouping logic.
        /// </summary>
        public DateTime? LastTransactionDateTime { get; set; }
    }

    /// <summary>
    /// Represents interquartile ranges (Q1/Q2/Q3) and derived quartile metrics for a set of transaction amounts.
    /// </summary>
    internal sealed class QuartileRanges
    {
        /// <summary>
        /// Gets or sets the lower-half values used to compute Q1.
        /// </summary>
        public List<decimal> Q1MedianRange { get; set; }

        /// <summary>
        /// Gets the Q1 median amount.
        /// </summary>
        public decimal Q1MedianAmount => GetQuartileRanges( Q1MedianRange ).MedianAmount;

        /// <summary>
        /// Gets or sets the middle values used to compute the overall median (Q2).
        /// </summary>
        public List<decimal> Q2MedianRange { get; set; }

        /// <summary>
        /// Gets the median amount (Q2).
        /// </summary>
        public decimal MedianAmount => Q2MedianAmount;

        /// <summary>
        /// Gets the Q2 median amount.
        /// </summary>
        public decimal Q2MedianAmount => Q2MedianRange.Any() ? Q2MedianRange.Average() : 0.00M;

        /// <summary>
        /// Gets or sets the upper-half values used to compute Q3.
        /// </summary>
        public List<decimal> Q3MedianRange { get; set; }

        /// <summary>
        /// Gets the Q3 median amount.
        /// </summary>
        public decimal Q3MedianAmount => GetQuartileRanges( Q3MedianRange ).MedianAmount;

        /// <summary>
        /// Gets the interquartile range amount (Q3 - Q1).
        /// </summary>
        public decimal IQRAmount => Q3MedianAmount - Q1MedianAmount;

        /// <summary>
        /// Creates quartile ranges from a set of transaction amounts using the same implementation as <see cref="GivingAutomationHelper.GetQuartileRanges"/>.
        /// </summary>
        private static QuartileRanges GetQuartileRanges( IEnumerable<decimal> transactionAmounts )
        {
            return GivingAutomationHelper.GetQuartileRanges( transactionAmounts );
        }
    }

    /// <summary>
    /// Minimal projection for an individual transaction from the FinancialTransaction table.
    /// </summary>
    internal sealed class FinancialTransactionView
    {
        /// <summary>
        /// Gets or sets the FinancialTransaction Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the AuthorizedPersonAliasId for the transaction.
        /// </summary>
        public int AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the giving id for the authorized person (giving unit id).
        /// </summary>
        public string AuthorizedPersonGivingId { get; set; }

        /// <summary>
        /// Gets or sets the campus id for the authorized person.
        /// </summary>
        public int? AuthorizedPersonCampusId { get; set; }

        /// <summary>
        /// Gets or sets the transaction date/time.
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the currency type defined value id.
        /// </summary>
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the source type defined value id.
        /// </summary>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this transaction was scheduled.
        /// </summary>
        public bool IsScheduled { get; set; }

        /// <summary>
        /// Gets or sets the transaction details (raw, pre-aggregation).
        /// </summary>
        public List<FinancialTransactionDetailView> TransactionDetails { get; set; } = new List<FinancialTransactionDetailView>();

        /// <summary>
        /// Gets or sets the refund details (raw, pre-aggregation).
        /// </summary>
        public List<FinancialTransactionDetailView> RefundDetails { get; set; } = new List<FinancialTransactionDetailView>();

        /// <summary>
        /// Gets the total amount for the transaction after applying refunds.
        /// </summary>
        public decimal TotalAmount => GetTransactionDetails().Sum( ftd => ftd.Amount );

        /// <summary>
        /// Gets the transaction details aggregated by account id and adjusted for refunds.
        /// </summary>
        internal List<FinancialTransactionDetailView> GetTransactionDetails()
        {
            var details = new Dictionary<int, decimal>();

            if ( TransactionDetails != null )
            {
                foreach ( var detail in TransactionDetails )
                {
                    if ( !details.ContainsKey( detail.AccountId ) )
                    {
                        details[detail.AccountId] = 0;
                    }
                    details[detail.AccountId] += detail.Amount;
                }
            }

            if ( RefundDetails != null )
            {
                foreach ( var refund in RefundDetails )
                {
                    if ( !details.ContainsKey( refund.AccountId ) )
                    {
                        details[refund.AccountId] = 0;
                    }
                    details[refund.AccountId] += refund.Amount;
                }
            }

            return details.Select( kvp => new FinancialTransactionDetailView
            {
                AccountId = kvp.Key,
                Amount = kvp.Value
            } ).Where( d => d.Amount != 0 ).ToList();
        }
    }

    /// <summary>
    /// Minimal projection for a record from the FinancialTransactionDetail table.
    /// </summary>
    internal sealed class FinancialTransactionDetailView
    {
        /// <summary>
        /// Gets or sets the FinancialAccount Id.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the amount for the account detail line.
        /// </summary>
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Minimal projection for a record from the FinancialTransactionAlert table.
    /// </summary>
    internal sealed class AlertView
    {
        /// <summary>
        /// Gets or sets the alert type identifier.
        /// </summary>
        /// <value>
        /// The alert type identifier.
        /// </value>
        public int AlertTypeId { get; set; }

        /// <summary>
        /// Gets or sets the alert date time.
        /// </summary>
        /// <value>
        /// The alert date time.
        /// </value>
        public DateTime AlertDateTime { get; set; }

        /// <summary>
        /// Gets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public int? TransactionId { get; set; }
    }

    /// <summary>
    /// Represents precomputed metrics for evaluating an alert type against a giving unit's history.
    /// </summary>
    internal sealed class AlertTypeComputedMetrics
    {
        /// <summary>
        /// Gets or sets the account ids relevant to the alert type (null means "all accounts").
        /// </summary>
        public HashSet<int> AlertTypeAccountIds { get; set; }

        /// <summary>
        /// Gets or sets quartile ranges computed from the relevant transaction amounts.
        /// </summary>
        public QuartileRanges QuartileRanges { get; set; }

        /// <summary>
        /// Gets or sets giving-frequency statistics computed from the relevant transaction dates.
        /// </summary>
        public FrequencyCalculationResult FrequencyStats { get; set; }

        /// <summary>
        /// Gets or sets the most recent qualifying transaction (per alert-type filter).
        /// </summary>
        public FinancialTransactionView MostRecentTransaction { get; set; }

        /// <summary>
        /// Gets or sets the number of qualifying transactions used for computed metrics.
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Enum GivingJourneyStage
    /// </summary>
    public enum GivingJourneyStage
    {
        /// <summary>
        /// Non-Giver
        /// </summary>
        None = 0,

        /// <summary>
        /// New giver.
        /// </summary>
        New = 1,

        /// <summary>
        /// Consistent giver
        /// </summary>
        Consistent = 2,

        /// <summary>
        /// Occasional giver
        /// </summary>
        Occasional = 3,

        /// <summary>
        /// Lapsed giver
        /// </summary>
        Lapsed = 4,

        /// <summary>
        /// Former giver
        /// </summary>
        Former = 5
    }

    #endregion DTOs
}
