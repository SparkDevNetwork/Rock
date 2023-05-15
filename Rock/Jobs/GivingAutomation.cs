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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility.Enums;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that serves three purposes:
    ///   1) Update Classification Attributes. This will be done no more than once a day and only on the days of week
    ///       configured in the automation settings.
    ///   2) Update Giving Journey Stage Attributes.    
    ///   3) Send Alerts - Sends alerts for gifts since the last run date and determines ‘Follow-up Alerts’ (alerts
    ///       triggered from gifts expected but not given) once a day.
    /// </summary>
    [DisplayName( "Giving Automation" )]
    [Description( "Job that updates giving classifications and journey stages, and send any giving alerts." )]

    [IntegerField( "Max Days Since Last Gift for Alerts",
        Description = "The maximum number of days since a giving group last gave where alerts can be made. If the last gift was earlier than this maximum, then alerts are not relevant.",
        DefaultIntegerValue = AttributeDefaultValue.MaxDaysSinceLastGift,
        Key = AttributeKey.MaxDaysSinceLastGift,
        Order = 1 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout,
        Category = "General",
        Order = 7 )]

    public class GivingAutomation : RockJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string MaxDaysSinceLastGift = "MaxDaysSinceLastGift";
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Default Values for Attributes
        /// </summary>
        private static class AttributeDefaultValue
        {
            public const int MaxDaysSinceLastGift = 548;
            public const int CommandTimeout = 180;
        }

        /// <summary>
        /// The lower percentile for the giver bin
        /// </summary>
        private static class GiverBinLowerPercentile
        {
            public const decimal First = 0.95m;
            public const decimal Second = 0.80m;
            public const decimal Third = 0.60m;
        }

        #endregion Keys

        #region Constructors

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GivingAutomation()
        {
        }

        #endregion Constructors

        #region Execute

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            if ( !settings.GivingAutomationJobSettings.IsEnabled )
            {
                this.UpdateLastStatusMessage( $"Giving Automation is not enabled." );
                return;
            }

            // Create a context object that will help transport state and helper information so as to not rely on the
            // job class itself being a single use instance
            var context = new GivingAutomationContext
            {
                SqlCommandTimeoutSeconds = GetAttributeValue( GivingAutomation.AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaultValue.CommandTimeout,
                MaxDaysSinceLastGift = GetAttributeValue( GivingAutomation.AttributeKey.MaxDaysSinceLastGift ).AsIntegerOrNull() ?? AttributeDefaultValue.MaxDaysSinceLastGift
            };

            // First determine the ranges for each of the 4 giving bins by looking at all contribution transactions in the last 12 months.
            // These ranges will be updated in the Giving Automation system settings.
            UpdateGiverBinRanges( context );

            // Load the alert types once since they will be needed for each giving id
            HydrateAlertTypes( context );

            // We only want to update classifications on people that have had new transactions since they last time they were classified
            DateTime? lastClassificationRunDateTime = GetLastClassificationsRunDateTime();

            var classificationStartDateTime = RockDateTime.Now;

            var classificationsDaysToRun = settings.GivingClassificationSettings.RunDays ?? DayOfWeekFlag.All.AsDayOfWeekList().ToArray();
            context.IsGivingClassificationRunDay = classificationsDaysToRun.Contains( context.Now.DayOfWeek );

            // Get a list of all giving units (distinct giver ids) that have given since the last classification
            HydrateGivingIdsToClassify( context, lastClassificationRunDateTime );

            // For each giving id, classify and run analysis and create alerts
            var totalStopWatch = new Stopwatch();
            var elapsedTimesMs = new ConcurrentBag<long>();

            if ( _debugModeEnabled )
            {
                totalStopWatch.Start();
            }

            long progressCount = 0;
            long totalCount = context.GivingIdsToClassify.Count();

            this.UpdateLastStatusMessage( $"Processing classifications and alerts for {totalCount} Giving Units..." );

            DateTime lastGivingClassificationProgressUpdate = DateTime.MinValue;
            var parallelOptions = new ParallelOptions
            {
                // Set MaxDegreeOfParallelism to 1 to make it easier to debug. 
                // Otherwise set it to half of Processor Count. Seems to be a sweet spot on best performance without overwhelming the machine and slowing down IIS.
                MaxDegreeOfParallelism = Environment.ProcessorCount > 4
                    ? Environment.ProcessorCount / 2
                    : 1
            };

            Parallel.ForEach(
                context.GivingIdsToClassify,
                parallelOptions,
                givingId =>
                {
                    var perStopWatch = new Stopwatch();

                    if ( _debugModeEnabled )
                    {
                        perStopWatch.Start();
                    }

                    ProcessGivingIdClassificationsAndAlerts( givingId, context );

                    perStopWatch.Stop();

                    if ( _debugModeEnabled )
                    {
                        elapsedTimesMs.Add( perStopWatch.ElapsedMilliseconds );
                    }

                    progressCount++;
                    if ( ( RockDateTime.Now - lastGivingClassificationProgressUpdate ).TotalSeconds >= 3 )
                    {
                        var ms = perStopWatch.ElapsedMilliseconds.ToString( "G" );

                        WriteToDebugOutput( $"Progress: {progressCount}/{totalCount}, progressCount/minute:{progressCount / totalStopWatch.Elapsed.TotalMinutes}" );

                        try
                        {
                            this.UpdateLastStatusMessage( $"Processing Giving Classifications and Alerts: {progressCount}/{totalCount}" );
                        }
                        catch ( Exception ex )
                        {
                            // ignore, but write to debug output
                            Debug.WriteLine( $"Error updating LastStatusMessage for ProcessGivingIdClassificationsAndAlerts loop: {ex}" );
                        }

                        perStopWatch.Reset();
                        lastGivingClassificationProgressUpdate = RockDateTime.Now;
                    }
                } );

            if ( context.IsGivingClassificationRunDay )
            {
                // If GivingClassifications where run and we've identified the GivingIds and are done processing them, set lastClassificationRunDateTime to when we starting hydrating. 
                SaveLastClassificationsRunDateTime( classificationStartDateTime );
            }

            if ( _debugModeEnabled && elapsedTimesMs.Any() )
            {
                totalStopWatch.Stop();
                var ms = totalStopWatch.ElapsedMilliseconds.ToString( "G" );
                WriteToDebugOutput( $"Finished {elapsedTimesMs.Count} giving ids in average of {elapsedTimesMs.Average()}ms per giving unit and total {ms}ms" );
            }

            this.UpdateLastStatusMessage( "Processing Late Alerts..." );

            // Create alerts for "late" gifts
            ProcessLateAlertTypes( context );

            // Process the Giving Journeys
            var givingJourneyHelper = new GivingJourneyHelper
            {
                SqlCommandTimeout = context.SqlCommandTimeoutSeconds
            };

            givingJourneyHelper.OnProgress += ( object sender, GivingJourneyHelper.ProgressEventArgs e ) =>
            {
                this.UpdateLastStatusMessage( e.ProgressMessage );
            };

            var daysToUpdateGivingJourneys = GivingAutomationSettings.LoadGivingAutomationSettings().GivingJourneySettings.DaysToUpdateGivingJourneys;

            bool updateGivingJourneyStages = daysToUpdateGivingJourneys?.Contains( context.Now.DayOfWeek ) == true;

            string journeyStageStatusMessage;
            if ( updateGivingJourneyStages )
            {
                givingJourneyHelper.UpdateGivingJourneyStages();
                if ( givingJourneyHelper.UpdatedJourneyStageCount == 1 )
                {
                    journeyStageStatusMessage = $"Updated {givingJourneyHelper.UpdatedJourneyStageCount} journey stage.";
                }
                else
                {
                    journeyStageStatusMessage = $"Updated {givingJourneyHelper.UpdatedJourneyStageCount} journey stages.";
                }
            }
            else
            {
                journeyStageStatusMessage = "Journey Stage updates not configured to run today.";
            }

            string classificationStatusMessage;
            if ( context.IsGivingClassificationRunDay )
            {
                classificationStatusMessage = $@"
Classified {context.GivingIdsSuccessful} giving {"group".PluralizeIf( context.GivingIdsSuccessful != 1 )}.
There were {context.GivingIdsFailed} {"failure".PluralizeIf( context.GivingIdsFailed != 1 )}.".Trim();
            }
            else
            {
                classificationStatusMessage = "Classification updates not configured to run today.";
            }

            // Format the result message
            this.Result = $@"
{classificationStatusMessage}
Created {context.AlertsCreated} {"alert".PluralizeIf( context.AlertsCreated != 1 )}.
{journeyStageStatusMessage}".Trim();

            if ( context.Errors.Any() )
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine( "Errors: " );

                foreach ( var error in context.Errors )
                {
                    sb.AppendLine( error );
                }

                var errorMessage = sb.ToString();
                this.Result += errorMessage;
            }
        }

        #endregion Execute

        #region Settings and Attribute Helpers

        /// <summary>
        /// Creates the person query for DataView.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="alertType">Type of the alert.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static IQueryable<Person> CreatePersonQueryForDataView( RockContext rockContext, FinancialTransactionAlertType alertType, GivingAutomationContext context )
        {
            if ( alertType.DataViewId is null )
            {
                return null;
            }

            var dataViewService = new DataViewService( rockContext );
            var dataview = dataViewService.Get( alertType.DataViewId.Value );

            if ( dataview == null )
            {
                context.Errors.Add( $"The dataview {alertType.DataViewId} for giving alert type {alertType.Id} did not resolve" );
                return null;
            }

            // We can use the dataview to get the person query
            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };

            IQueryable<Person> dataviewQuery;
            try
            {
                dataviewQuery = dataview.GetQuery( dataViewGetQueryArgs ) as IQueryable<Person>;
            }
            catch ( Exception ex )
            {
                context.Errors.Add( ex.Message );
                ExceptionLogService.LogException( ex );
                return null;
            }

            if ( dataviewQuery == null )
            {
                context.Errors.Add( $"Generating a query for dataview {alertType.DataViewId} for giving alert type {alertType.Id} was not successful" );
                return null;
            }

            return dataviewQuery;
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        private static string GetAttributeKey( GivingAutomationContext context, string guidString )
        {
            var key = AttributeCache.Get( guidString )?.Key;

            if ( key.IsNullOrWhiteSpace() )
            {
                context.Errors.Add( $"An attribute was expected using the guid '{guidString}', but failed to resolve" );
            }

            return key;
        }

        /// <summary>
        /// Gets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The guid string.</param>
        /// <returns></returns>
        private static string GetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString )
        {
            if ( !people.Any() )
            {
                return string.Empty;
            }

            var key = GetAttributeKey( context, guidString );

            if ( key.IsNullOrWhiteSpace() )
            {
                // GetAttributeKey logs an error in the context
                return string.Empty;
            }

            var unitValue = people.First().GetAttributeValue( key );

            for ( var i = 1; i < people.Count; i++ )
            {
                var person = people[i];
                var personValue = person.GetAttributeValue( key );

                if ( unitValue != personValue )
                {
                    // The people in this giving unit have different values for this. We don't know which is actually correct, so assume no value.
                    return string.Empty;
                }
            }

            return unitValue;
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, decimal? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToStringSafe(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, int? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToStringSafe(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, DateTime? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToISO8601DateString(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, string value, RockContext rockContext = null )
        {
            var key = GetAttributeKey( context, guidString );

            if ( key.IsNullOrWhiteSpace() )
            {
                // GetAttributeKey logs an error in the context
                return;
            }

            foreach ( var person in people )
            {
                person.SetAttributeValue( key, value );
            }
        }

        /// <summary>
        /// Gets the percent int. Ex: 50 and 200 => 25. This is safeguarded from 0 as a
        /// denominator by returning 0 in that case.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns></returns>
        private static int GetPercentInt( int numerator, int denominator )
        {
            if ( denominator == 0 )
            {
                return 0;
            }

            var asDecimal = decimal.Divide( numerator, denominator );
            return GetPercentInt( asDecimal );
        }

        /// <summary>
        /// Gets the percent int. Ex: .976 => 98
        /// </summary>
        /// <param name="percentDecimal">The percent decimal.</param>
        /// <returns></returns>
        private static int GetPercentInt( decimal percentDecimal )
        {
            return ( int ) decimal.Round( percentDecimal * 100 );
        }

        /// <summary>
        /// Gets the global repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? GlobalRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings.GivingAlertingSettings.GlobalRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the gratitude repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? GratitudeRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings?.GivingAlertingSettings?.GratitudeRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the follow-up repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? FollowUpRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings?.GivingAlertingSettings?.FollowupRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static DateTime? GetLastClassificationsRunDateTime()
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            return settings.GivingClassificationSettings.LastRunDateTime;
        }

        private static void SaveLastClassificationsRunDateTime( DateTime lastRunDateTime )
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            settings.GivingClassificationSettings.LastRunDateTime = lastRunDateTime;
            GivingAutomationSettings.SaveGivingAutomationSettings( settings );
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static decimal? GetGivingBinLowerLimit( int binIndex )
        {
            var classificationSettings = GivingAutomationSettings.LoadGivingAutomationSettings().GivingClassificationSettings;
            var giverBin = classificationSettings.GiverBins?.Count > binIndex
                    ? classificationSettings.GiverBins[binIndex]
                    : null;

            return giverBin?.LowerLimit;
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static void SetGivingBinLowerLimit( int binIndex, decimal? lowerLimit )
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();

            while ( settings.GivingClassificationSettings.GiverBins.Count <= binIndex )
            {
                settings.GivingClassificationSettings.GiverBins.Add( new GiverBin() );
            }

            var giverBin = settings.GivingClassificationSettings.GiverBins[binIndex];
            giverBin.LowerLimit = lowerLimit;
            GivingAutomationSettings.SaveGivingAutomationSettings( settings );
        }

        /// <summary>
        /// Gets the earliest last gift date time.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static DateTime GetEarliestLastGiftDateTime( GivingAutomationContext context )
        {
            var days = context.MaxDaysSinceLastGift;
            return context.Now.AddDays( 0 - days );
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
        /// A List of Giving Amounts broken into 3 Quartiles. See https://en.wikipedia.org/wiki/Interquartile_range
        /// These are sorted from smallest amount to largest amount.
        /// <para>
        /// Examples
        /// <list type="bullet">
        /// <item>1,2,3,4,5,6,7,8,9,10 => (1,2,3,4), (5,6), (7,8,9,10)</item>
        /// <item>1,2,3,4,5,6,7,8,9,10,11 => (1,2,3,4, 5), (6), (7,8,9,10,11)</item>
        /// </list>
        /// </para>
        /// </summary>
        internal class QuartileRanges
        {
            /// <summary>
            /// Gets or sets the Q1 median range.
            /// This is The list of amounts (in order) that are Less than the Median amount all all amounts.
            /// </summary>
            /// <value>The q1 median range.</value>
            public List<decimal> Q1MedianRange { get; set; }

            /// <summary>
            /// Gets the q1 median amount.
            /// </summary>
            /// <value>The q1 median amount.</value>
            public decimal Q1MedianAmount => GetQuartileRanges( Q1MedianRange ).MedianAmount;

            /// <summary>
            /// Gets or sets the Q2 median range.
            /// This the middle of all amounts. This would either be 1 or 2 amounts depending on if there are an odd or event number of amounts;
            /// </summary>
            /// <value>The q2 median range.</value>
            public List<decimal> Q2MedianRange { get; set; }

            /// <summary>
            /// Gets the median amount.
            /// </summary>
            /// <value>The median amount.</value>
            public decimal MedianAmount => Q2MedianAmount;

            /// <summary>
            /// Gets the q2 median amount (this is what is stored in PERSON_GIVING_AMOUNT_MEDIAN )
            /// Q2MedianRange is either 1 or 2 values, so we use Average just in case there are 2 to get the 'Median'
            /// </summary>
            /// <value>The q2 median amount.</value>
            public decimal Q2MedianAmount => Q2MedianRange.Any() ? Q2MedianRange.Average() : 0.00M;

            /// <summary>
            /// Gets or sets the Q3 median range.
            /// The List of amounts (in order) that are greater than the Median amount all all amounts
            /// </summary>
            /// <value>The q3 median rangle.</value>
            public List<decimal> Q3MedianRange { get; set; }

            /// <summary>
            /// Gets the q3 median amount.
            /// </summary>
            /// <value>The q3 median amount.</value>
            public decimal Q3MedianAmount => GetQuartileRanges( Q3MedianRange ).MedianAmount;

            /// <summary>
            /// IQR Amount is the difference between <see cref="Q3MedianAmount"/> and <seealso cref="Q1MedianAmount"/>
            /// </summary>
            /// <value><see cref="Q3MedianAmount"/> minus <seealso cref="Q1MedianAmount"/></value>
            public decimal IQRAmount => Q3MedianAmount - Q1MedianAmount;
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
                // Examples
                //
                // Exactly $20 every time
                //   - IQR = $3 (15%)
                //   - Sensitivity 3
                //   - Minimum Difference from Mean $9  (3 * 3)
                //   - Larger than usual would be $29 or more
                //
                // Exactly $100 every time
                //   - IQR = $15 (15%)
                //   - Sensitivity 3
                //   - Minimum Difference from Mean $45  (3 * 15)
                //   - Larger than usual would be $145 or more
                //
                // Exactly $500 every time
                //   - IQR = $75 (15% * $500)
                //   - Sensitivity 3
                //   - Minimum Difference from Mean $225  (3 * 75)
                //   - Larger than usual would be $725 or more
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

            /* Example
             Example: Family gives every 30 days, but skipped a month once, and gave early once

             Days since previous:
             29
             30  
             31
             30
             
             60 (they must have skipped a month)
             28
             30
             31
             28
             14
             stddev: 10.75

             Lets say it has been 45 days after than last gift ( around 15 days later than usual )

             frequencyStdDev = 10.75
               frequencyMean = 30
             - daysSince = 45
             = frequencyDeviation = 15  "15 days later than usual"
             numberOfFrequencyStdDevs =  1.39  ( 15 / 10.75 )

            They are only 1.39x late, so 15 days late isn't really that concerning. Don't need a 'late alert' quite yet.

            But lets say it has been over 65 days (35 days late).
            frequencyStdDev = 10.75
               frequencyMean = 30
             - daysSince = 65 (95 since they late gave)
             = frequencyDeviation = 35  "45 days later than usual"
             numberOfFrequencyStdDevs =  4.18  ( 45 / 10.75 )

            Now they are 4.18x late, so 45 days late might be worth a 'late alert'.
            
            */

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
                // For example:
                // Frequency mean: 3 days (consistently twice a week!): std Dev : 0.45 days! ( 0.15 * 3 ) 
                // Frequency mean: 7 day (once a week). Std Dev = 1.05 days ( 0.15 * 7 )
                // Frequency mean: 14 days (consistently every 2 weeks): std Dev : 2.1 days ( 0.15 * 14 )
                // Frequency mean: ~30 days (consistently every month): std Dev : 4.5 days ( 0.15 * 30 )
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

        /// <summary>
        /// Are follow-up alerts allowed given the recent alerts based on 
        /// <see cref="GivingAlertingSettings.FollowupRepeatPreventionDurationDays"/> and <see cref="GivingAlertingSettings.GlobalRepeatPreventionDurationDays"/>
        /// </summary>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool AllowFollowUpAlerts( List<AlertView> recentAlerts, GivingAutomationContext context )
        {
            if ( recentAlerts == null || !recentAlerts.Any() )
            {
                return true;
            }

            if ( GlobalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Last().AlertDateTime;
                var daysSinceLastAlert = ( context.Now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= GlobalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastFollowUpAlerts = recentAlerts.Where( a => a.AlertType == AlertType.FollowUp );
            if ( lastFollowUpAlerts?.Any() == true )
            {
                var lastFollowUpAlertDate = lastFollowUpAlerts.Max( x => ( DateTime? ) x.AlertDateTime );
                if ( FollowUpRepeatPreventionDays.HasValue && lastFollowUpAlertDate.HasValue )
                {
                    var daysSinceLastFollowUpAlert = ( context.Now - lastFollowUpAlertDate.Value ).TotalDays;

                    if ( daysSinceLastFollowUpAlert <= FollowUpRepeatPreventionDays.Value )
                    {
                        // This group has follow-up alerts within the repeat duration. Don't create any new follow-up alerts.
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Are gratitude alerts allowed given the recent alerts based on 
        /// <see cref="GivingAlertingSettings.GratitudeRepeatPreventionDurationDays"/> and <see cref="GivingAlertingSettings.GlobalRepeatPreventionDurationDays"/>
        /// </summary>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool AllowGratitudeAlerts( List<AlertView> recentAlerts, GivingAutomationContext context )
        {
            if ( !recentAlerts.Any() )
            {
                return true;
            }

            if ( GlobalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Last().AlertDateTime;
                var daysSinceLastAlert = ( context.Now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= GlobalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastGratitudeAlerts = recentAlerts.Where( a => a.AlertType == AlertType.Gratitude );

            if ( lastGratitudeAlerts?.Any() == true )
            {
                var lastGratitudeAlertDate = lastGratitudeAlerts.Max( a => ( DateTime? ) a.AlertDateTime );
                if ( GratitudeRepeatPreventionDays.HasValue && lastGratitudeAlertDate.HasValue )
                {
                    var daysSinceLastGratitudeAlert = ( context.Now - lastGratitudeAlertDate.Value ).TotalDays;

                    if ( daysSinceLastGratitudeAlert <= GratitudeRepeatPreventionDays.Value )
                    {
                        // This group has gratitude alerts within the repeat duration. Don't create any new gratitude alerts.
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion Settings and Attribute Helpers

        #region Execute Logic

        /// <summary>
        /// Hydrates the alert types that should be considered today (according to the alert type RunDays).
        /// </summary>
        /// <param name="context">The context.</param>
        private static void HydrateAlertTypes( GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;

                // Get the alert types
                var alertTypeService = new FinancialTransactionAlertTypeService( rockContext );
                var alertTypes = alertTypeService.Queryable()
                    .Include( a => a.FinancialAccount.ChildAccounts )
                    .AsNoTracking()
                    .OrderBy( at => at.Order )
                    .ToList();

                // Filter out alert types that are not supposed to run today
                var currentDayOfWeekFlag = context.Now.DayOfWeek.AsFlag();

                alertTypes = alertTypes
                    .Where( at =>
                        !at.RunDays.HasValue ||
                        ( at.RunDays.Value & currentDayOfWeekFlag ) == currentDayOfWeekFlag )
                    .ToList();

                context.AlertTypes = alertTypes;
            }
        }

        /// <summary>
        /// Hydrates the giving ids to classify.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="lastClassificationRunDateTime">The last classification run date time.</param>
        private static void HydrateGivingIdsToClassify( GivingAutomationContext context, DateTime? lastClassificationRunDateTime )
        {
            // Classification attributes need to be written for all adults with the same giver id in Rock. So Ted &
            // Cindy should have the same attribute values if they are set to contribute as a family even if Cindy
            // is always the one giving the gift.

            // We will reclassify anyone who has given since the last run of this job. This covers all alerts except
            // the "late gift" alert, which needs to find people based on the absence of a gift.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
                var financialTransactionService = new FinancialTransactionService( rockContext );

                // This is the people that have given since the last run date or the configured old gift date point.
                // Just in case transactions were modified since the last run date, also include givingids that have
                // transaction records that have been modified since the last run date.
                var minTransactionDate = lastClassificationRunDateTime ?? GetEarliestLastGiftDateTime( context );
                var givingIds = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        ( t.TransactionDateTime >= minTransactionDate )
                        || ( t.ModifiedDateTime.HasValue && t.ModifiedDateTime.Value >= minTransactionDate ) )
                    .Select( t => t.AuthorizedPersonAlias.Person.GivingId )
                    .Distinct()
                    .ToList();

                // This transforms the set of people to classify into distinct giving ids.
                context.GivingIdsToClassify = givingIds.OrderBy( a => a ).ToList();
            }
        }

        /// <summary>
        /// Updates the giver bins ranges.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void UpdateGiverBinRanges( GivingAutomationContext context )
        {
            // First determine the ranges for each of the 4 giving bins by looking at all contribution transactions in the last 12 months.
            // These ranges will be updated in the Giving Automation system settings.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
                var minDate = context.Now.AddMonths( -12 );

                var financialTransactionService = new FinancialTransactionService( rockContext );
                var givingGroups = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        t.TransactionDateTime.HasValue &&
                        t.TransactionDateTime > minDate &&
                        t.AuthorizedPersonAliasId.HasValue &&
                        t.AuthorizedPersonAlias.Person.GivingId != null &&
                        t.AuthorizedPersonAlias.Person.GivingId.Length > 0 )
                    .GroupBy( t => t.AuthorizedPersonAlias.Person.GivingId )
                    .Select( g => new GiverBinTotal
                    {
                        GivingId = g.Key,

                        // get the sum of each transaction for the last 12 months
                        Last12MonthsTotalGiftBeforeRefund = g.Sum( t => t.TransactionDetails.Sum( d => d.Amount ) ),
                        Last12MonthsTotalGiftRefunds = g.Sum( t =>
                            ( decimal? ) t.Refunds.Sum( rr =>
                                  rr.FinancialTransaction.TransactionDetails.Sum( rd => ( decimal? ) rd.Amount ) ?? 0.00M ) )

                    } )
                    .ToList();

                givingGroups = givingGroups.OrderBy( g => g.Last12MonthsTotalGift ).ToList();
                var givingGroupCount = givingGroups.Count;

                // Calculate the current giving percentile lower amounts. This means the lower range of the 48th percentile will
                // be at index 48 of the array.
                context.PercentileLowerRange = new List<decimal>();

                for ( var i = 0; i < 100; i++ )
                {
                    var percentDecimal = decimal.Divide( i, 100 );
                    var firstIndex = ( int ) decimal.Round( givingGroupCount * percentDecimal );

                    if ( firstIndex >= givingGroupCount )
                    {
                        firstIndex = givingGroupCount - 1;
                    }

                    if ( firstIndex >= 0 && firstIndex < givingGroups.Count )
                    {
                        context.PercentileLowerRange.Add( givingGroups[firstIndex].Last12MonthsTotalGift );
                    }
                    else
                    {
                        context.PercentileLowerRange.Add( 0m );
                    }
                }

                // These should be static, but just in case the count changes for some reason
                var percentileCount = context.PercentileLowerRange.Count;
                var firstBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.First );
                var secondBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.Second );
                var thirdBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.Third );

                SetGivingBinLowerLimit( 0, context.PercentileLowerRange[firstBinStartIndex] );
                SetGivingBinLowerLimit( 1, context.PercentileLowerRange[secondBinStartIndex] );
                SetGivingBinLowerLimit( 2, context.PercentileLowerRange[thirdBinStartIndex] );
                SetGivingBinLowerLimit( 3, context.PercentileLowerRange[0] );
            }
        }

        /// <summary>
        /// Processes the giving identifier.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="context">The context.</param>
        private static void ProcessGivingIdClassificationsAndAlerts( string givingId, GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;

                // Get the gifts from the past 12 months for the giving group
                var financialTransactionService = new FinancialTransactionService( rockContext );

                // Classifications for: % Scheduled, Gives As ___, Preferred Source, Preferred Currency will be based
                // off of all giving in the last 12 months.In the case of a tie in values( e.g. 50% credit card, 50%
                // cash ) use the most recent value as the tie breaker. This could be calculated with only one gift.
                var oneYearAgo = context.Now.AddMonths( -12 );
                var twelveMonthsTransactionsQry = financialTransactionService
                        .GetGivingAutomationSourceTransactionQueryByGivingId( givingId )
                        .Where( t => t.TransactionDateTime >= oneYearAgo );

                var twelveMonthsTransactions = twelveMonthsTransactionsQry
                    .Select( t => new TransactionView
                    {
                        Id = t.Id,
                        AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                        AuthorizedPersonCampusId = t.AuthorizedPersonAlias.Person.PrimaryCampusId,
                        AuthorizedPersonGivingId = givingId,
                        TransactionDateTime = t.TransactionDateTime.Value,
                        TransactionViewDetailsBeforeRefunds = t.TransactionDetails.Select( x => new TransactionViewDetail { AccountId = x.AccountId, Amount = x.Amount } ).ToList(),
                        RefundDetails = t.Refunds.SelectMany( r => r.FinancialTransaction.TransactionDetails ).Select( x => new TransactionViewDetail { AccountId = x.AccountId, Amount = x.Amount } ).ToList(),
                        CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                        SourceTypeValueId = t.SourceTypeValueId,
                        IsScheduled = t.ScheduledTransactionId.HasValue
                    } )
                    .ToList()
                    .OrderBy( t => t.TransactionDateTime )
                    .ToList();

                // If there are no transactions, then there is no basis to classify or alert
                if ( !twelveMonthsTransactions.Any() )
                {
                    return;
                }

                // Load the people that are in this giving group so their attribute values can be set
                var personService = new PersonService( rockContext );

                // Limit to only Business and Person type records.
                // Include deceased to cover transactions that could have occurred when they were not deceased
                // or transactions that are dated after they were marked deceased.
                var personQueryOptions = new PersonService.PersonQueryOptions
                {
                    IncludeDeceased = true,
                    IncludeBusinesses = true,
                    IncludePersons = true,
                    IncludeNameless = false,
                    IncludeRestUsers = false
                };

                var people = personService.Queryable( personQueryOptions )
                    .Include( p => p.Aliases )
                    .Where( p => p.GivingId == givingId )
                    .ToList();

                if ( !people.Any() )
                {
                    return;
                }

                people.LoadAttributes( rockContext );

                // If the group doesn't have FirstGiftDate attribute, set it by querying for the value
                var firstGiftDate = GetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE ).AsDateTime();
                var updatedFirstGiftDate = false;

                if ( !firstGiftDate.HasValue )
                {
                    var firstGiftDateQry = financialTransactionService.GetGivingAutomationSourceTransactionQueryByGivingId( givingId )
                        .Where( t => t.TransactionDateTime.HasValue );

                    firstGiftDate = firstGiftDateQry.Min( t => t.TransactionDateTime );

                    if ( firstGiftDate.HasValue )
                    {
                        SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE, firstGiftDate );
                        updatedFirstGiftDate = true;
                    }
                }

                // If the first gift date was less than 12 months ago and there are less than 5 gifts,
                // do not process the classification for the giving group because there is not
                // enough data to develop any meaningful insights.
                bool hasEnoughDataForClassification = !( ( !firstGiftDate.HasValue || firstGiftDate.Value > oneYearAgo ) && twelveMonthsTransactions.Count < context.MinimumTransactionCountForClassifications );

                // Determine if there are alert types that don't have either FrequencySensitivityScale or AmountSensitivityScale defined
                bool hasAlertsTypesWithoutSensitivity = context.AlertTypes.Any( a => !a.FrequencySensitivityScale.HasValue && !a.AmountSensitivityScale.HasValue );

                if ( hasEnoughDataForClassification == false )
                {
                    // We don't have enough data to classify.
                    // If we also don't have any Alert Types that need transaction history, we can move on to the next GivingID
                    if ( hasAlertsTypesWithoutSensitivity == false )
                    {
                        return;
                    }
                }

                // We need to know if this giving group has other transactions. If they do then we do not need to
                // extrapolate because we have the complete 12 month data picture.
                var mostRecentOldTransactionDateQuery = financialTransactionService
                        .GetGivingAutomationSourceTransactionQueryByGivingId( givingId )
                        .Where( t => t.TransactionDateTime < oneYearAgo );

                var mostRecentOldTransactionDate = mostRecentOldTransactionDateQuery.Select( t => t.TransactionDateTime ).Max();

                // Only run classifications on the days specified by the settings, and only if there is enough data to do classification for this GivingId
                if ( context.IsGivingClassificationRunDay && hasEnoughDataForClassification )
                {
                    // Update the attributes using the logic function
                    var classificationSuccess = UpdateGivingUnitClassifications( givingId, people, twelveMonthsTransactions, mostRecentOldTransactionDate, context, oneYearAgo );

                    if ( !classificationSuccess )
                    {
                        context.GivingIdsFailed++;

                        // Do not continue with alerts if classification failed
                        return;
                    }

                    // Save all the attribute value changes
                    people.ForEach( p => p.SaveAttributeValues( rockContext ) );
                    rockContext.SaveChanges();
                    context.GivingIdsSuccessful++;

                    // Fire the bus event to notify that these people have been classified
                    GivingUnitWasClassifiedMessage.Publish( people.Select( p => p.Id ) );
                }
                else if ( updatedFirstGiftDate )
                {
                    // Save attribute value change for first gift date
                    // First gift date isn't technically part of classification since this attribute predates giving automation
                    people.ForEach( p => p.SaveAttributeValues( rockContext ) );
                    rockContext.SaveChanges();
                }

                // Next we will generate alerts if there are any to process to today. The HydrateAlerts method already executed
                // and provided with alerts types that are scheduled to be run today
                if ( !context.AlertTypes.Any() )
                {
                    return;
                }

                // If this giving group has not been classified in the last week, then don't generate any alerts
                // because those alerts will be based on outdated statistics. One week was chosen since classifications
                // can occur at a minimum of 1 day per week since the control is a day of the week picker without being
                // all the way turned off.
                var lastClassifiedDate = GetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime() ?? DateTime.MinValue;
                var oneWeekAgo = context.Now.AddDays( -7 );
                var hasBeenClassifiedRecently = lastClassifiedDate >= oneWeekAgo;

                if ( hasBeenClassifiedRecently == false )
                {
                    // The person hasn't be classified recently.
                    // If we also don't have any Alert Types that need transaction history, we can move on to the next GivingID
                    if ( hasAlertsTypesWithoutSensitivity == false )
                    {
                        return;
                    }
                }

                // Alerts can be generated for transactions given in the last week. One week was chosen since alert types
                // can run at a minimum of 1 day per week since the control is a day of the week picker without being
                // all the way turned off.
                var transactionsToCheckAlerts = twelveMonthsTransactions.Where( t => t.TransactionDateTime >= oneWeekAgo ).ToList();

                if ( !transactionsToCheckAlerts.Any() )
                {
                    return;
                }

                // Get any recent alerts for these people. These will be used in the "repeat alert prevention" logic
                var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
                var twelveMonthsAlertsQry = financialTransactionAlertService.Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.AlertDateTime > oneYearAgo );

                var givingIdPersonAliasIdQuery = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == givingId ).Select( a => a.Id );
                twelveMonthsAlertsQry = twelveMonthsAlertsQry.Where( t => givingIdPersonAliasIdQuery.Contains( t.PersonAliasId ) );

                var twelveMonthsAlerts = twelveMonthsAlertsQry
                    .Select( a => new AlertView
                    {
                        AlertDateTime = a.AlertDateTime,
                        AlertTypeId = a.AlertTypeId,
                        AlertType = a.FinancialTransactionAlertType.AlertType,
                        TransactionId = a.TransactionId
                    } )
                    .OrderBy( a => a.AlertDateTime )
                    .ToList();

                // Check the repeat prevention durations. These prevent multiple alert types from being generated
                // for these people in a short time period (if configured)
                var allowFollowUp = AllowFollowUpAlerts( twelveMonthsAlerts, context );
                var allowGratitude = AllowGratitudeAlerts( twelveMonthsAlerts, context );
                var alertsToAddToDb = new List<FinancialTransactionAlert>();

                if ( allowFollowUp || allowGratitude )
                {
                    foreach ( var transaction in transactionsToCheckAlerts )
                    {
                        DateTime? previousTransactionDate;

                        var previousTransactions = twelveMonthsTransactions.Where( t => t.TransactionDateTime < transaction.TransactionDateTime ).ToList();

                        if ( previousTransactions.Any() )
                        {
                            previousTransactionDate = previousTransactions.Max( a => a.TransactionDateTime );
                        }
                        else
                        {
                            previousTransactionDate = transaction.TransactionDateTime;
                        }

                        List<FinancialTransactionAlert> alertsForTransaction =
                            CreateAlertsForTransaction(
                                twelveMonthsAlerts,
                                transaction,
                                twelveMonthsTransactions,
                                previousTransactionDate.Value,
                                mostRecentOldTransactionDate,
                                context,
                                allowGratitude,
                                allowFollowUp );

                        alertsToAddToDb.AddRange( alertsForTransaction );

                        if ( alertsForTransaction.Any() )
                        {
                            twelveMonthsAlerts.AddRange( alertsForTransaction.Select( a => new AlertView
                            {
                                AlertDateTime = a.AlertDateTime,
                                AlertType = context.AlertTypes.First( at => at.Id == a.AlertTypeId ).AlertType,
                                AlertTypeId = a.AlertTypeId,
                                TransactionId = a.TransactionId
                            } ) );

                            // Recheck if more alerts can be created now that there are more alerts created
                            allowFollowUp = AllowFollowUpAlerts( twelveMonthsAlerts, context );
                            allowGratitude = AllowGratitudeAlerts( twelveMonthsAlerts, context );

                            if ( !allowFollowUp && !allowGratitude )
                            {
                                // Break out of the for loop since no more alerts can be generated
                                break;
                            }
                        }
                    }
                }

                // Add any alerts to the database
                if ( alertsToAddToDb.Any() )
                {
                    var service = new FinancialTransactionAlertService( rockContext );
                    service.AddRange( alertsToAddToDb );
                }

                rockContext.SaveChanges();
                context.AlertsCreated += alertsToAddToDb.Count;
                HandlePostAlertsAddedLogic( alertsToAddToDb );
            }
        }

        /// <summary>
        /// Creates the alerts for transaction.
        /// </summary>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="twelveMonthsTransactions">The twelve months transactions.</param>
        /// <param name="lastGiftDate">The last gift date.</param>
        /// <param name="mostRecentOldTransactionDate">The most recent old transaction date for transactions older than 12 months ago</param>
        /// <param name="context">The context.</param>
        /// <param name="allowGratitude">if set to <c>true</c> [allow gratitude].</param>
        /// <param name="allowFollowUp">if set to <c>true</c> [allow follow up].</param>
        /// <returns>System.Collections.Generic.List&lt;Rock.Model.FinancialTransactionAlert&gt;.</returns>
        internal static List<FinancialTransactionAlert> CreateAlertsForTransaction(
            List<AlertView> recentAlerts,
            TransactionView transaction,
            List<TransactionView> twelveMonthsTransactions,
            DateTime lastGiftDate,
            DateTime? mostRecentOldTransactionDate,
            GivingAutomationContext context,
            bool allowGratitude,
            bool allowFollowUp )
        {
            var alerts = new List<FinancialTransactionAlert>();
            var daysSinceLastTransaction = ( transaction.TransactionDateTime - lastGiftDate ).TotalDays;

            // Go through the ordered alert types (ordered in the hydrate method)
            foreach ( var alertType in context.AlertTypes )
            {
                var newAlert = CreateAlertForRecentTransaction( alertType, recentAlerts, transaction, twelveMonthsTransactions, daysSinceLastTransaction, mostRecentOldTransactionDate, context, allowGratitude, allowFollowUp );
                if ( newAlert != null )
                {
                    alerts.Add( newAlert );
                    if ( !alertType.ContinueIfMatched )
                    {
                        break;
                    }
                }
            }

            return alerts;
        }

        /// <summary>
        /// Creates the alert.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="last12MonthsTransactionsAll">The last12 months transactions all.</param>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="mostRecentOldTransactionDate">The most recent old transaction date for transactions older than 12 months ago</param>
        /// <param name="context">The context.</param>
        /// <param name="allowGratitude">if set to <c>true</c> [allow gratitude].</param>
        /// <param name="allowFollowUp">if set to <c>true</c> [allow follow up].</param>
        /// <returns>FinancialTransactionAlert.</returns>
        private static FinancialTransactionAlert CreateAlertForRecentTransaction(
            FinancialTransactionAlertType alertType,
            List<AlertView> recentAlerts,
            TransactionView transaction,
            List<TransactionView> last12MonthsTransactionsAll,
            double daysSinceLastTransaction,
            DateTime? mostRecentOldTransactionDate,
            GivingAutomationContext context,
            bool allowGratitude,
            bool allowFollowUp )
        {
            // Make sure this transaction / alert type combo doesn't already exist
            var alreadyAlerted = recentAlerts.Any( a =>
                a.AlertTypeId == alertType.Id &&
                a.TransactionId == transaction.Id );

            if ( alreadyAlerted )
            {
                return null;
            }

            // Ensure that this alert type is allowed (might be disallowed because of global repeat prevention durations)
            if ( !allowFollowUp && alertType.AlertType == AlertType.FollowUp )
            {
                return null;
            }

            if ( !allowGratitude && alertType.AlertType == AlertType.Gratitude )
            {
                return null;
            }

            // Check the days since the last transaction are within allowed range
            if ( alertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > alertType.MaximumDaysSinceLastGift.Value )
            {
                return null;
            }

            // Check if this alert type has already been alerted too recently
            if ( alertType.RepeatPreventionDuration.HasValue && recentAlerts?.Any( a => a.AlertTypeId == alertType.Id ) == true )
            {
                var lastAlertOfTypeDate = recentAlerts.Last( a => a.AlertTypeId == alertType.Id ).AlertDateTime;
                var daysSinceLastAlert = ( context.Now - lastAlertOfTypeDate ).TotalDays;

                if ( daysSinceLastAlert <= alertType.RepeatPreventionDuration.Value )
                {
                    // Alert would be too soon after the last alert was generated
                    return null;
                }
            }

            // Check if the campus is a match
            if ( alertType.CampusId.HasValue )
            {
                var campusId = transaction.AuthorizedPersonCampusId;

                if ( alertType.CampusId != campusId )
                {
                    // Campus does not match
                    return null;
                }
            }

            List<decimal> transactionAmountsForAlert;
            List<DateTime> transactionDateTimesForAlert;
            List<int> alertAccountIds = GetAlertTypeAccountIds( alertType );
            decimal transactionAmount;

            if ( alertAccountIds != null )
            {
                if ( !transaction.GetTransactionViewDetails().Where( a => alertAccountIds.Contains( a.AccountId ) ).Any() )
                {
                    // transaction doesn't meet criteria for this alerts Accounts
                    return null;
                }

                transactionDateTimesForAlert = last12MonthsTransactionsAll.Where( a => a.GetTransactionViewDetails().Any( d => alertAccountIds.Contains( d.AccountId ) ) ).Select( a => a.TransactionDateTime ).ToList();
                transactionAmountsForAlert = last12MonthsTransactionsAll.SelectMany( m => m.GetTransactionViewDetails() ).Where( a => alertAccountIds.Contains( a.AccountId ) ).Select( a => a.Amount ).ToList();
                transactionAmount = transaction.GetTransactionViewDetails().Where( a => alertAccountIds.Contains( a.AccountId ) ).Sum( a => a.Amount );
            }
            else
            {
                transactionDateTimesForAlert = last12MonthsTransactionsAll.Select( a => a.TransactionDateTime ).ToList();
                transactionAmountsForAlert = last12MonthsTransactionsAll.SelectMany( m => m.GetTransactionViewDetails() ).Select( a => a.Amount ).ToList();
                transactionAmount = transaction.TotalTransactionAmount;
            }

            // Determine if the alert type has either FrequencySensitivityScale or AmountSensitivityScale defined
            if ( alertType.FrequencySensitivityScale.HasValue || alertType.AmountSensitivityScale.HasValue )
            {
                // If there is either FrequencySensitivity or AmountSensitivity rule, we need to have some transaction history
                if ( transactionDateTimesForAlert.Count() < context.MinimumTransactionCountForSensitivityAlertTypes )
                {
                    // after filtering to Accounts, there isn't enough transaction history for an alert type that has sensitivity rules
                    return null;
                }
            }

            // Check the min gift amount
            if ( alertType.MinimumGiftAmount.HasValue && transactionAmount < alertType.MinimumGiftAmount )
            {
                // Gift is less than this rule allows
                return null;
            }

            // Check the max gift amount
            if ( alertType.MaximumGiftAmount.HasValue && transactionAmount > alertType.MaximumGiftAmount )
            {
                // Gift is more than this rule allows
                return null;
            }

            var quartileRanges = GetQuartileRanges( transactionAmountsForAlert );

            // Check the median gift amount
            if ( alertType.MinimumMedianGiftAmount.HasValue && quartileRanges.MedianAmount < alertType.MinimumMedianGiftAmount )
            {
                // Median gift amount is too small for this rule
                return null;
            }

            if ( alertType.MaximumMedianGiftAmount.HasValue && quartileRanges.MedianAmount > alertType.MaximumMedianGiftAmount )
            {
                // Median gift amount is too large for this rule
                return null;
            }

            // Check the number of IQRs that the amount varies
            var numberOfAmountIqrs = GetAmountIqrCount( quartileRanges, transactionAmount );

            var transactionStats = GetFrequencyStats( transactionDateTimesForAlert,
                mostRecentOldTransactionDate,
                context.TransactionWindowDurationHours );
            var meanFrequencyDays = transactionStats.MeanFrequencyDays;

            // Store Frequency Std Dev
            var frequencyStdDevDays = transactionStats.FrequencyStdDevDays;

            var numberOfFrequencyStdDevs = GetFrequencyDeviationCount( frequencyStdDevDays, meanFrequencyDays, Convert.ToDecimal( daysSinceLastTransaction ) );

            // Detect which thing, amount or frequency, is exceeding the rule's sensitivity scale
            var reasons = new List<string>();

            /*
              11-18-2021 MDP

            The Sensitivity scale logic is dependant on whether Gratitude or Follow-up is selected:
              - Gratitude looks at 'better than usual' transactions (higher amount than usual, or earlier than usual)
              - Follow-up looks at 'worse than usual'  transactions (lower amount than usual, or later than usual)

            Example:
                - Normal Range $500 +/- $30
                - Gratitude (with sensitivity of 3), would alert if $590 or more


            */


            if ( alertType.AlertType == AlertType.Gratitude )
            {
                // For example, if Follow-up with a Sensitivity of 3
                // Normal is $500 +/- 50
                // Gratitude with look at values $650 or more
                if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs >= alertType.AmountSensitivityScale.Value )
                {
                    // Gift is larger amount than they usually give (Larger than Usual alert)
                    // Note that this is only for people have established a normal range of giving, so it would only
                    // people with some transaction history. ('Large Gift alert' is different than 'Larger than Usual alert')
                    reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                }

                if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs >= alertType.FrequencySensitivityScale )
                {
                    // Gift is earlier than when they usually give (Early Gift Alert)
                    reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                }
            }
            else if ( alertType.AlertType == AlertType.FollowUp )
            {
                // Follow up 'Flips the Sign' of what was specified.
                // For example, if Followup with a Sensitivity of 3
                // Normal is $500 +/- 50
                // Follow-up with look at values $350 or less
                if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs <= ( alertType.AmountSensitivityScale * -1 ) )
                {
                    // Gift is outside the amount sensitivity scale (Smaller Amount than Usual alert)
                    reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                }

                if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs <= ( alertType.FrequencySensitivityScale * -1 ) )
                {
                    // Gift is outside the frequency sensitivity scale (Later than Usual)
                    reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                }
            }

            bool hasSensitivityAlerts = reasons.Any();

            if ( !hasSensitivityAlerts )
            {
                bool hasSensitivityRules = alertType.AmountSensitivityScale.HasValue || alertType.FrequencySensitivityScale.HasValue;

                if ( hasSensitivityRules )
                {
                    // this alerts has Sensitivity rules, but neither triggered an alert, so we continue without generating alert 
                    return null;
                }
                else
                {
                    // If the case of no sensitivity rules and no sensitivity alerts,
                    // Check for a simple 'Transaction Amount Over $x' type of alert (no other criteria other than Minimum Amount)
                    if ( alertType.MinimumGiftAmount.HasValue && transactionAmount >= alertType.MinimumGiftAmount.Value )
                    {
                        // this is the 'Large Gift Amount' use case
                        reasons.Add( nameof( alertType.MinimumGiftAmount ) );
                    }
                    else
                    {
                        // No alert criteria is met, so continue without an alert
                        return null;
                    }
                }
            }

            bool personMeetsDataViewCriteria = PersonMeetsDataViewCriteria( alertType, transaction.AuthorizedPersonGivingId, context );
            if ( !personMeetsDataViewCriteria )
            {
                return null;
            }

            var frequencyDeviation = meanFrequencyDays - Convert.ToDecimal( daysSinceLastTransaction );

            // Create the alert because this gift is a match for this rule.
            var financialTransactionAlert = new FinancialTransactionAlert
            {
                TransactionId = transaction.Id,
                PersonAliasId = transaction.AuthorizedPersonAliasId,
                GivingId = transaction.AuthorizedPersonGivingId,
                AlertTypeId = alertType.Id,
                Amount = transactionAmount,
                AmountCurrentMedian = quartileRanges.MedianAmount,
                AmountCurrentIqr = quartileRanges.IQRAmount,
                AmountIqrMultiplier = numberOfAmountIqrs,
                FrequencyCurrentMean = meanFrequencyDays,
                FrequencyCurrentStandardDeviation = frequencyStdDevDays,
                FrequencyDifferenceFromMean = frequencyDeviation,
                FrequencyZScore = numberOfFrequencyStdDevs,
                ReasonsKey = reasons.ToJson(),
                AlertDateTime = context.Now,
                AlertDateKey = context.Now.ToDateKey()
            };

            return financialTransactionAlert;
        }

        /// <summary>
        /// Persons the meets data view criteria.
        /// </summary>
        /// <param name="alertType">Type of the alert.</param>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool PersonMeetsDataViewCriteria( FinancialTransactionAlertType alertType, string givingId, GivingAutomationContext context )
        {
            bool personMeetsDataViewCriteria = true;

            // Check if GivingUnitId is in the DataView
            // Since this would involve a database hit, check this only if all the criteria for an Alert has been met so far.
            if ( alertType.DataViewId.HasValue )
            {
                var personQuery = context.DataViewPersonQueries.GetValueOrNull( alertType.DataViewId.Value );

                // If the query hasn't already been created, generate the person id query for this dataview
                if ( personQuery is null )
                {
                    personQuery = CreatePersonQueryForDataView( context.DataViewPersonQueriesRockContext, alertType, context );

                    if ( personQuery is null )
                    {
                        // Errors are logged within the creation method so just return
                        personMeetsDataViewCriteria = false;
                    }

                    context.DataViewPersonQueries[alertType.DataViewId.Value] = personQuery;
                }

                // Check at least one of the people are within the dataview
                if ( !personQuery.Any( p => p.GivingId == givingId ) )
                {
                    // None of the people are in the dataview
                    personMeetsDataViewCriteria = false;
                }
            }

            return personMeetsDataViewCriteria;
        }

        /// <summary>
        /// Processes the giving identifier. This logic was isolated for automated testing.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="people">The people.</param>
        /// <param name="transactions">The transactions from the last 12 months</param>
        /// <param name="mostRecentOldTransactionDate">The most recent old transaction date for transactions older than 12 months ago</param>
        /// <param name="context">The context.</param>
        /// <param name="minDate">The minimum date that the transactions were queried with.</param>
        /// <returns>
        /// True if success
        /// </returns>
        internal static bool UpdateGivingUnitClassifications(
            string givingId,
            List<Person> people,
            List<TransactionView> transactions,
            DateTime? mostRecentOldTransactionDate,
            GivingAutomationContext context,
            DateTime minDate )
        {
            if ( transactions == null )
            {
                context.Errors.Add( $"the list of transactions was null for giving id {givingId}" );
                return false;
            }

            if ( people?.Any() != true )
            {
                context.Errors.Add( $"There were no people passed in the giving group {givingId}" );
                return false;
            }

            if ( people.Any( p => p.GivingId != givingId ) )
            {
                context.Errors.Add( $"The people (IDs: {people.Select( p => p.Id.ToString() ).JoinStringsWithCommaAnd()}) are not within the same giving group {givingId}" );
                return false;
            }

            if ( people.Any( p => p.Attributes == null ) )
            {
                context.Errors.Add( $"The people (IDs: {people.Select( p => p.Id.ToString() ).JoinStringsWithCommaAnd()}) did not have attributes loaded for giving group {givingId}" );
                return false;
            }

            if ( !transactions.Any() )
            {
                // shouldn't happen because we checked this earlier
                return false;
            }

            // Update the groups "last gift date" attribute
            var lastGiftDate = transactions.Max( a => a.TransactionDateTime );

            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGiftDate );

            var transactionTwelveMonthCount = transactions.Count;

            // Store percent scheduled
            var scheduledTransactionsCount = transactions.Count( t => t.IsScheduled );
            var percentScheduled = GetPercentInt( scheduledTransactionsCount, transactionTwelveMonthCount );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED, percentScheduled );

            // Store preferred source
            var sourceGroups = transactions.Where( a => a.SourceTypeValueId.HasValue ).GroupBy( t => t.SourceTypeValueId ).OrderByDescending( g => g.Count() );
            var maxSourceCount = sourceGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredSourceTransactions = sourceGroups
                .Where( g => g.Count() == maxSourceCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime );

            var preferredSourceTypeValueId = preferredSourceTransactions.FirstOrDefault()?.SourceTypeValueId;
            var preferredSourceGuid = preferredSourceTypeValueId.HasValue ? DefinedValueCache.Get( preferredSourceTypeValueId.Value )?.Guid : null;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE, preferredSourceGuid.ToStringSafe() );

            // Store preferred currency
            var currencyGroups = transactions.Where( a => a.CurrencyTypeValueId.HasValue ).GroupBy( t => t.CurrencyTypeValueId ).OrderByDescending( g => g.Count() );
            var maxCurrencyCount = currencyGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredCurrencyTransactions = currencyGroups
                .Where( g => g.Count() == maxCurrencyCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime );

            var preferredCurrencyValueId = preferredCurrencyTransactions.FirstOrDefault()?.CurrencyTypeValueId;
            var preferredCurrencyGuid = preferredCurrencyValueId.HasValue ? DefinedValueCache.Get( preferredCurrencyValueId.Value )?.Guid : null;

            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY, preferredCurrencyGuid.ToStringSafe() );

            // ii.) Classifications for: Bin, Percentile
            //      a.) If there is 12 months of giving use that.
            //      b.) If not then use the current number of days of gifts to extrapolate a full year. So if you have 60
            //          days of giving, multiply the giving amount by 6.08( 356 / 60 ). But there must be at least 3 gifts.
            var extrapolationFactor = 1m;
            var hasMoreTransactions = mostRecentOldTransactionDate.HasValue;

            if ( !hasMoreTransactions )
            {
                var oldestGiftDate = transactions.OrderByDescending( a => a.TransactionDateTime ).FirstOrDefault()?.TransactionDateTime;
                var daysSinceOldestGift = oldestGiftDate == null ? 0d : ( context.Now - oldestGiftDate.Value ).TotalDays;
                var daysSinceMinDate = ( context.Now - minDate ).TotalDays;
                extrapolationFactor = Convert.ToDecimal( daysSinceOldestGift > 0d ? ( daysSinceMinDate / daysSinceOldestGift ) : 0d );

                if ( extrapolationFactor > 1m )
                {
                    extrapolationFactor = 1m;
                }
            }

            // Store bin
            var yearGiftAmount = transactions.Sum( t => t.TotalTransactionAmount ) * extrapolationFactor;
            var binIndex = 3;

            while ( binIndex >= 1 )
            {
                var lowerLimitForNextBin = GetGivingBinLowerLimit( binIndex - 1 );

                if ( !lowerLimitForNextBin.HasValue || yearGiftAmount >= lowerLimitForNextBin )
                {
                    binIndex--;
                }
                else
                {
                    break;
                }
            }

            var bin = binIndex + 1;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_BIN, bin );

            // Store percentile
            var percentileInt = 0;

            while ( percentileInt < ( context.PercentileLowerRange.Count - 1 ) )
            {
                var nextPercentileInt = percentileInt + 1;
                var nextPercentileLowerRange = context.PercentileLowerRange[nextPercentileInt];

                if ( yearGiftAmount >= nextPercentileLowerRange )
                {
                    percentileInt++;
                }
                else
                {
                    break;
                }
            }

            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE, percentileInt );

            // Update the last classification run date to now
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE, context.Now );

            /*
            // iii.) Classification for: Median Amount, IQR Amount, Mean Frequency, Frequency Standard Deviation
            //      a.) If there is 12 months of giving use all of those
            //      b.) If not use the previous gifts that are within 12 months but there must be at least 5 gifts.
            //      c.) For Amount: we will calculate the median and interquartile range
            //      d.) For Frequency: we will calculate the trimmed mean and standard deviation. The trimmed mean will
            //          exclude the top 10 % largest and smallest gifts with in the dataset. If the number of gifts
            //          available is < 10 then we’ll remove the top largest and smallest gift.
            */

            if ( transactionTwelveMonthCount < context.MinimumTransactionCountForClassifications )
            {
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE, string.Empty );
                return true;
            }

            /*
            // Interquartile range deals with finding the median. Then we say the numbers before the median numbers
            // are q1 and the numbers after are q1.
            // Ex: 50, 100, 101, 103, 103, 5000
            // Q1, Median, and then Q3: (50, 100), (101, 103), (103, 5000)
            // IQR is the median(Q3) - median(Q1)
            */

            var quartileRanges = GetQuartileRanges( transactions.Select( a => a.TotalTransactionAmount ) );

            // Store median amount
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, quartileRanges.MedianAmount );

            // Store IQR amount
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, quartileRanges.IQRAmount );

            // Create a parallel array that stores the days since the last transaction for the transaction at that index
            var transactionStats = GetFrequencyStats( transactions.Select( a => a.TransactionDateTime ).ToList(),
                mostRecentOldTransactionDate,
                context.TransactionWindowDurationHours );

            // Store Mean Frequency
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, transactionStats.MeanFrequencyDays );

            // Store Frequency Std Dev
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, transactionStats.FrequencyStdDevDays );

            // Giving Frequency Label
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, ( int ) transactionStats.GivingFrequencyLabel );

            // Update the next expected gift date
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE, transactionStats.NextExpectedGiftDate );

            return true;
        }

        /// <summary>
        /// Gets Statistics about the dates of the specified transactions
        /// This uses Standard Deviation based on the dates of the transaction history.
        /// </summary>
        /// <param name="transactionDateTimes">The transaction date times.</param>
        /// <param name="mostRecentOldTransactionDate">The most recent old transaction date (last transaction that was over 12 months ago).</param>
        /// <param name="transactionWindowDurationHours">The duration of the window within which transactions are considered as a single giving event.</param>
        private static FrequencyStats GetFrequencyStats( List<DateTime> transactionDateTimes, DateTime? mostRecentOldTransactionDate, int? transactionWindowDurationHours = null )
        {
            // Calculate the days elapsed between transactions.
            var daysSinceLastTransactionList = new List<decimal?>();
            var lastTransactionDateTime = mostRecentOldTransactionDate;
            foreach ( var transactionDateTime in transactionDateTimes.OrderBy( a => a ) )
            {
                decimal? daysSince = null;
                if ( lastTransactionDateTime.HasValue )
                {
                    // If a transaction window is specified, consider transactions that occur within the same window as a single instance
                    // to avoid skewing the frequency calculations.
                    if ( transactionWindowDurationHours == null
                        || transactionWindowDurationHours <= ( transactionDateTime - lastTransactionDateTime.Value ).TotalHours )
                    {
                        daysSince = ( decimal ) ( transactionDateTime - lastTransactionDateTime.Value ).TotalDays;
                        lastTransactionDateTime = transactionDateTime;
                    }
                }
                else
                {
                    lastTransactionDateTime = transactionDateTime;
                }

                daysSinceLastTransactionList.Add( daysSince );
            }

            var daysSinceLastTransactionWithValue = daysSinceLastTransactionList
                .Where( d => d.HasValue )
                .Select( d => d.Value )
                .ToList();
            double meanFrequencyDays;
            double frequencyStdDevDays;
            if ( daysSinceLastTransactionWithValue.Any() )
            {
                meanFrequencyDays = Convert.ToDouble( daysSinceLastTransactionWithValue.Average() );
                frequencyStdDevDays = ( double ) Math.Sqrt( daysSinceLastTransactionWithValue.Average( d => Math.Pow( Convert.ToDouble( d ) - meanFrequencyDays, 2 ) ) );
            }
            else
            {
                meanFrequencyDays = 0;
                frequencyStdDevDays = 0;
            }

            var nextExpectedGiftDate = lastTransactionDateTime.HasValue ? lastTransactionDateTime.Value.AddDays( ( double ) meanFrequencyDays ) : ( DateTime? ) null;

            FinancialGivingAnalyticsFrequencyLabel givingFrequencyLabel;

            // Frequency Labels:
            //      Weekly = Avg days between 4.5 - 8.5; Std Dev< 7;
            //      2 Weeks = Avg days between 9 - 17; Std Dev< 10;
            //      Monthly = Avg days between 25 - 35; Std Dev< 10;
            //      Quarterly = Avg days between 80 - 110; Std Dev< 15;
            //      Erratic = Freq Avg / 2 < Std Dev;
            //      Undetermined = Everything else

            // Attribute value values: 1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined
            if ( meanFrequencyDays >= 4.5d && meanFrequencyDays <= 8.5d && frequencyStdDevDays < 7d )
            {
                // Weekly
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.Weekly;
            }
            else if ( meanFrequencyDays >= 9d && meanFrequencyDays <= 17d && frequencyStdDevDays < 10d )
            {
                // BiWeekly
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.BiWeekly;
            }
            else if ( meanFrequencyDays >= 25d && meanFrequencyDays <= 35d && frequencyStdDevDays < 10d )
            {
                // Monthly
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.Monthly;
            }
            else if ( meanFrequencyDays >= 80d && meanFrequencyDays <= 110d && frequencyStdDevDays < 15d )
            {
                // Quarterly
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.Quarterly;
            }
            else if ( ( meanFrequencyDays / 2 ) < frequencyStdDevDays )
            {
                // Erratic
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.Erratic;
            }
            else
            {
                // Undetermined
                givingFrequencyLabel = FinancialGivingAnalyticsFrequencyLabel.Undetermined;
            }

            return new FrequencyStats
            {
                LastTransactionDateTime = lastTransactionDateTime,
                MeanFrequencyDays = ( decimal ) meanFrequencyDays,
                FrequencyStdDevDays = ( decimal ) frequencyStdDevDays,
                NextExpectedGiftDate = nextExpectedGiftDate,
                GivingFrequencyLabel = givingFrequencyLabel
            };
        }

        /// <summary>
        /// Processes the late alerts. This method finds giving groups that have an expected gift date that has now passed
        /// </summary>
        /// <param name="context">The context.</param>
        private static void ProcessLateAlertTypes( GivingAutomationContext context )
        {
            // Find the late gift alert types. There are already filtered to the alert types that should run today
            var lateGiftAlertTypes = context.AlertTypes
                .Where( at =>
                     at.AlertType == AlertType.FollowUp &&
                     at.FrequencySensitivityScale.HasValue )
                .ToList();

            if ( !lateGiftAlertTypes.Any() )
            {
                return;
            }

            context.LateAlertsByGivingId = new Dictionary<string, List<(int AlertTypeId, bool ContinueIfMatched)>>();
            List<FinancialTransactionAlert> addedlateAlerts = new List<FinancialTransactionAlert>();

            foreach ( FinancialTransactionAlertType lateGiftAlertType in lateGiftAlertTypes.OrderBy( a => a.Order ) )
            {
                var lateAlertsForAlertType = ProcessLateAlertType( context, lateGiftAlertType );
                if ( lateAlertsForAlertType.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
                        new FinancialTransactionAlertService( rockContext ).AddRange( lateAlertsForAlertType );
                        context.AlertsCreated += lateAlertsForAlertType.Count;
                        rockContext.SaveChanges();

                        addedlateAlerts.AddRange( lateAlertsForAlertType );
                    }
                }
            }


            HandlePostAlertsAddedLogic( addedlateAlerts );
        }

        /// <summary>
        /// Processes the type of the late alert.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="lateGiftAlertType">Type of the late gift alert.</param>
        private static List<FinancialTransactionAlert> ProcessLateAlertType( GivingAutomationContext context, FinancialTransactionAlertType lateGiftAlertType )
        {
            List<int> alertAccountIds = GetAlertTypeAccountIds( lateGiftAlertType );

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
            var financialTransactionService = new FinancialTransactionService( rockContext );

            var givingAutomationSourceTransactionQueryForAlertType = financialTransactionService
                .GetGivingAutomationSourceTransactionQuery()
                .Where( a => a.TransactionDateTime.HasValue && a.AuthorizedPersonAliasId.HasValue );

            if ( alertAccountIds != null )
            {
                givingAutomationSourceTransactionQueryForAlertType = givingAutomationSourceTransactionQueryForAlertType.Where( a => a.TransactionDetails.Any( x => alertAccountIds.Contains( x.AccountId ) ) );
            }

            if ( lateGiftAlertType.DataViewId != null )
            {
                var personIdDataViewQuery = CreatePersonQueryForDataView( rockContext, lateGiftAlertType, context )?.Select( a => a.Id );
                if ( personIdDataViewQuery == null )
                {
                    return null;
                }

                var personAliasIdQueryFromDataView = new PersonAliasService( rockContext ).Queryable().Where( a => personIdDataViewQuery.Contains( a.PersonId ) ).Select( a => a.Id );

                givingAutomationSourceTransactionQueryForAlertType = givingAutomationSourceTransactionQueryForAlertType.Where( t => personAliasIdQueryFromDataView.Contains( t.AuthorizedPersonAliasId.Value ) );
            }

            var oneYearAgo = context.Now.AddMonths( -12 );

            /* 08/16/2022 MP
              
             Select the data from the database into a list before doing the GroupBy. This improves performance
             significantly in this case since we'll be doing the GroupBy and Max in memory instead of having SQL
             having to figure it out.
            
             */

            var mostRecentOldTransactionDateForAlertTypeByGivingId = givingAutomationSourceTransactionQueryForAlertType
                .Where( t => t.TransactionDateTime < oneYearAgo )
                .Select( a => new { a.AuthorizedPersonAlias.Person.GivingId, a.TransactionDateTime } )
                .ToList()
                .GroupBy( a => a.GivingId )
                .Select( a => new
                {
                    GivingId = a.Key,
                    MostRecentOldTransactionDateTime = a.Max( x => x.TransactionDateTime.Value )
                } ).ToDictionary(
                    k => k.GivingId,
                    v => v.MostRecentOldTransactionDateTime );


            /* 08/16/2022 MP
              
             Select the data from the database into a list before doing the GroupBy. This improves performance
             significantly in this case since we'll be doing the GroupBy in memory instead of having SQL
             figure it out. 
            
             */

            var twelveMonthsTransactionsForAlertTypeByGivingId = givingAutomationSourceTransactionQueryForAlertType
                .Where( t => t.TransactionDateTime >= oneYearAgo )
                .Select( t => new TransactionView
                {
                    Id = t.Id,
                    AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                    AuthorizedPersonGivingId = t.AuthorizedPersonAlias.Person.GivingId,
                    AuthorizedPersonCampusId = t.AuthorizedPersonAlias.Person.PrimaryCampusId,
                    TransactionDateTime = t.TransactionDateTime.Value,
                    TransactionViewDetailsBeforeRefunds = t.TransactionDetails.Select( x => new TransactionViewDetail { AccountId = x.AccountId, Amount = x.Amount } ).ToList(),
                    RefundDetails = t.Refunds.SelectMany( r => r.FinancialTransaction.TransactionDetails ).Select( x => new TransactionViewDetail { AccountId = x.AccountId, Amount = x.Amount } ).ToList(),
                    CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                    SourceTypeValueId = t.SourceTypeValueId,
                    IsScheduled = t.ScheduledTransactionId.HasValue
                } )
                .ToList()
                .GroupBy( a => a.AuthorizedPersonGivingId )
                .Select( a => new
                {
                    GivingId = a.Key,
                    Last12MonthsTransactions = a.ToList()
                } ).ToList();

            var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );

            // Get any recent alerts for these people
            var recentAlertsByGivingId = financialTransactionAlertService.Queryable()
                .AsNoTracking()
                .Where( a => a.AlertTypeId == lateGiftAlertType.Id && a.AlertDateTime > oneYearAgo && a.GivingId != null )
                .Select( a => new
                {
                    a.GivingId,
                    a.AlertDateTime,
                    a.FinancialTransactionAlertType.AlertType,
                    a.AlertTypeId
                } )
                .ToList()
                .GroupBy( a => a.GivingId )
                .Select( a => new
                {
                    GivingId = a.Key,
                    RecentAlerts = a.Select( x => new AlertView { AlertDateTime = x.AlertDateTime, AlertType = x.AlertType, AlertTypeId = x.AlertTypeId } ).OrderByDescending( x => x.AlertDateTime ).ToList()
                } )
                .ToDictionary( k => k.GivingId, v => v.RecentAlerts );

            var alertsForThisAlertType = new List<FinancialTransactionAlert>();

            foreach ( var givingIdTransactions in twelveMonthsTransactionsForAlertTypeByGivingId )
            {
                var givingId = givingIdTransactions.GivingId;

                var lateAlertForGivingIdArgs = new LateAlertForGivingIdArgs(
                    givingId,
                    givingIdTransactions.Last12MonthsTransactions,
                    recentAlertsByGivingId.GetValueOrNull( givingId ),
                    mostRecentOldTransactionDateForAlertTypeByGivingId.GetValueOrNull( givingId ) );

                var financialTransactionAlert = CreateAlertForLateTransaction( context, lateGiftAlertType, lateAlertForGivingIdArgs );

                if ( financialTransactionAlert != null )
                {
                    alertsForThisAlertType.Add( financialTransactionAlert );
                }
            }

            return alertsForThisAlertType;
        }

        private static List<int> GetAlertTypeAccountIds( FinancialTransactionAlertType lateGiftAlertType )
        {
            List<int> alertAccountIds;
            if ( lateGiftAlertType.FinancialAccountId.HasValue )
            {
                alertAccountIds = new List<int>();
                alertAccountIds.Add( lateGiftAlertType.FinancialAccountId.Value );
                if ( lateGiftAlertType.IncludeChildFinancialAccounts )
                {
                    var childAccountsIds = lateGiftAlertType.FinancialAccount?.ChildAccounts?.Select( a => a.Id ).ToList();
                    if ( childAccountsIds?.Any() == true )
                    {
                        alertAccountIds.AddRange( childAccountsIds );
                    }
                }
            }
            else
            {
                alertAccountIds = null;
            }

            return alertAccountIds;
        }

        /// <summary>
        /// Processes the late alert for giving identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="lateGiftAlertType">Type of the late gift alert.</param>
        /// <param name="lateAlertForGivingIdArgs">The late alert for giving identifier arguments.</param>
        /// <returns>FinancialTransactionAlert.</returns>
        internal static FinancialTransactionAlert CreateAlertForLateTransaction(
            GivingAutomationContext context,
            FinancialTransactionAlertType lateGiftAlertType,
            LateAlertForGivingIdArgs lateAlertForGivingIdArgs )
        {
            var givingId = lateAlertForGivingIdArgs.GivingId;
            context.LateAlertsByGivingId = context.LateAlertsByGivingId ?? new Dictionary<string, List<(int AlertTypeId, bool ContinueIfMatched)>>();
            var newAlertsForGivingId = context.LateAlertsByGivingId.GetValueOrNull( givingId );
            if ( newAlertsForGivingId != null )
            {
                // if an alert was already generated for this GivingId, don't add any more new ones if ContinueIfMatched is false;
                if ( newAlertsForGivingId.Any( a => a.ContinueIfMatched == false ) )
                {
                    return null;
                }
            }

            if ( newAlertsForGivingId == null )
            {
                newAlertsForGivingId = new List<(int AlertTypeId, bool ContinueIfMatched)>();
                context.LateAlertsByGivingId.Add( givingId, newAlertsForGivingId );
            }

            var alertTypeAccountIds = GetAlertTypeAccountIds( lateGiftAlertType );

            var allowFollowUp = AllowFollowUpAlerts( lateAlertForGivingIdArgs.RecentAlerts, context );

            List<decimal> transactionAmountsForAlert;
            List<DateTime> transactionDateTimesForAlert;
            TransactionView mostRecentTransaction;

            var last12MonthsTransactionsForAlertType = lateAlertForGivingIdArgs.Last12MonthsTransactions;

            // last12MonthsTransactionsForAlertType should have been filtered by account ids already, but just in case (and for integration) testing
            if ( alertTypeAccountIds != null )
            {
                transactionDateTimesForAlert = last12MonthsTransactionsForAlertType.Where( a => a.GetTransactionViewDetails().Any( d => alertTypeAccountIds.Contains( d.AccountId ) ) ).Select( a => a.TransactionDateTime ).ToList();
                transactionAmountsForAlert = last12MonthsTransactionsForAlertType.SelectMany( m => m.GetTransactionViewDetails() ).Where( a => alertTypeAccountIds.Contains( a.AccountId ) ).Select( a => a.Amount ).ToList();
                mostRecentTransaction = last12MonthsTransactionsForAlertType.Where( a => a.GetTransactionViewDetails().Any( d => alertTypeAccountIds.Contains( d.AccountId ) ) ).OrderByDescending( a => a.TransactionDateTime ).FirstOrDefault();
            }
            else
            {
                transactionDateTimesForAlert = last12MonthsTransactionsForAlertType.Select( a => a.TransactionDateTime ).ToList();
                transactionAmountsForAlert = last12MonthsTransactionsForAlertType.SelectMany( m => m.GetTransactionViewDetails() ).Select( a => a.Amount ).ToList();
                mostRecentTransaction = last12MonthsTransactionsForAlertType?.OrderByDescending( a => a.TransactionDateTime ).FirstOrDefault();
            }

            var mostRecentOldTransactionDateTime = lateAlertForGivingIdArgs.MostRecentOldTransactionDateTime;
            var mostRecentAlertOfThisTypeAlertDateTime = lateAlertForGivingIdArgs.GeMostRecentAlertOfThisTypeAlertDateTime( lateGiftAlertType.Id );

            if ( transactionDateTimesForAlert.Count < context.MinimumTransactionCountForSensitivityAlertTypes )
            {
                // A Late Transaction Alert requires Frequency Sensitivity Rules
                return null;
            }

            if ( mostRecentTransaction == null )
            {
                // no transaction in last 12 months for this AlertType's criteria
                return null;
            }

            var quartileRanges = GetQuartileRanges( transactionAmountsForAlert );

            var transactionStats = GetFrequencyStats( transactionDateTimesForAlert,
                mostRecentOldTransactionDateTime,
                context.TransactionWindowDurationHours );

            if ( !transactionStats.NextExpectedGiftDate.HasValue )
            {
                // Since there is NextExpectedGiftDate based on giving patterns, there wouldn't be a late alert
                return null;
            }

            if ( !transactionStats.LastTransactionDateTime.HasValue )
            {
                // No previous transaction
                return null;
            }

            if ( mostRecentAlertOfThisTypeAlertDateTime.HasValue )
            {
                if ( mostRecentAlertOfThisTypeAlertDateTime.Value > transactionStats.LastTransactionDateTime )
                {
                    // Don't generate late alerts more than once for a giving group since the last time they gave
                    return null;
                }

                // Check if this alert type has already been alerted too recently
                if ( lateGiftAlertType.RepeatPreventionDuration.HasValue )
                {
                    var lastAlertOfTypeDate = mostRecentAlertOfThisTypeAlertDateTime.Value;
                    var daysSinceLastAlert = ( context.Now - lastAlertOfTypeDate ).TotalDays;

                    if ( daysSinceLastAlert <= lateGiftAlertType.RepeatPreventionDuration.Value )
                    {
                        // Alert would be too soon after the last alert was generated
                        return null;
                    }
                }
            }

            var daysSinceLastTransaction = ( context.Now - transactionStats.LastTransactionDateTime.Value ).TotalDays;

            // Check the maximum days since the last alert
            if ( lateGiftAlertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > lateGiftAlertType.MaximumDaysSinceLastGift )
            {
                return null;
            }

            // Check if the campus is a match
            if ( lateGiftAlertType.CampusId.HasValue )
            {
                var mostRecentTransactionCampusId = mostRecentTransaction?.AuthorizedPersonCampusId;

                if ( lateGiftAlertType.CampusId != mostRecentTransactionCampusId )
                {
                    // Campus does not match
                    return null;
                }
            }

            // Check the median gift amount
            if ( lateGiftAlertType.MinimumMedianGiftAmount.HasValue && quartileRanges.MedianAmount < lateGiftAlertType.MinimumMedianGiftAmount )
            {
                // Median gift amount is too small for this rule
                return null;
            }

            if ( lateGiftAlertType.MaximumMedianGiftAmount.HasValue && quartileRanges.MedianAmount > lateGiftAlertType.MaximumMedianGiftAmount )
            {
                // Median gift amount is too large for this rule
                return null;
            }

            var numberOfFrequencyStdDevs = GetFrequencyDeviationCount( transactionStats.FrequencyStdDevDays, transactionStats.MeanFrequencyDays, ( decimal ) daysSinceLastTransaction );

            var reasons = new List<string>();

            if ( numberOfFrequencyStdDevs <= ( lateGiftAlertType.FrequencySensitivityScale * -1 ) )
            {
                // The current date is later the frequency sensitivity scale
                reasons.Add( nameof( lateGiftAlertType.FrequencySensitivityScale ) );
            }

            if ( !reasons.Any() )
            {
                // If the current date is earlier than the expected next transaction date, don't generate an alert
                return null;
            }

            bool personMeetsDataViewCriteria = PersonMeetsDataViewCriteria( lateGiftAlertType, givingId, context );
            if ( !personMeetsDataViewCriteria )
            {
                return null;
            }

            // The next expected gift date is earlier than today, and they haven't given since then, so create a late alert
            // Create the alert
            // Note that Amount, AmountIqrMultiplier, FrequencyDifferenceFromMean and FrequencyZScore will be set to null.
            // Those values are comparisons of the specific transaction that created the alert to the normal giving patterns.
            // In this case it is the lack of a transaction that caused the alert, so no transaction to compare to.
            var financialTransactionAlert = new FinancialTransactionAlert
            {
                TransactionId = null,
                PersonAliasId = mostRecentTransaction.AuthorizedPersonAliasId,
                GivingId = givingId,
                AlertTypeId = lateGiftAlertType.Id,
                Amount = null,
                AmountCurrentMedian = quartileRanges.MedianAmount,
                AmountCurrentIqr = quartileRanges.IQRAmount,
                AmountIqrMultiplier = null,
                FrequencyCurrentMean = transactionStats.MeanFrequencyDays,
                FrequencyCurrentStandardDeviation = transactionStats.FrequencyStdDevDays,
                FrequencyDifferenceFromMean = null,
                FrequencyZScore = null,
                ReasonsKey = reasons.ToJson(),
                AlertDateTime = context.Now,
                AlertDateKey = context.Now.ToDateKey()
            };

            newAlertsForGivingId.Add( (lateGiftAlertType.Id, lateGiftAlertType.ContinueIfMatched) );

            return financialTransactionAlert;
        }

        /// <summary>
        /// Handles the post alerts-added logic.
        /// </summary>
        /// <param name="alertsAdded">The alerts added.</param>
        private static void HandlePostAlertsAddedLogic( List<FinancialTransactionAlert> alertsAdded )
        {
            foreach ( var alert in alertsAdded )
            {
                if ( alert.Id == 0 )
                {
                    continue;
                }

                // This Task does all of the various Workflow, Bus Events, System Communications, etc
                new ProcessTransactionAlertActions.Message
                {
                    FinancialTransactionAlertId = alert.Id
                }.Send();
            }
        }

        #endregion Execute Logic

        #region Debug

        /// <summary>
        /// Log debug output?
        /// </summary>
        private static bool _debugModeEnabled = false;

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void WriteToDebugOutput( string message )
        {
            if ( _debugModeEnabled && System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( $"\tGiving Automation {RockDateTime.Now:mm.ss.f} {message}" );
            }
        }

        #endregion Debug

        #region Classes

        /// <summary>
        /// Giving Automation Context
        /// </summary>
        public sealed class GivingAutomationContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GivingAutomationContext" /> class.
            /// </summary>
            public GivingAutomationContext()
            {
                DataViewPersonQueries = new Dictionary<int, IQueryable<Person>>();
                SqlCommandTimeoutSeconds = AttributeDefaultValue.CommandTimeout;
                MaxDaysSinceLastGift = AttributeDefaultValue.MaxDaysSinceLastGift;
                DataViewPersonQueriesRockContext = new RockContext();
                DataViewPersonQueriesRockContext.Database.CommandTimeout = SqlCommandTimeoutSeconds;
                LateAlertsByGivingId = new Dictionary<string, List<(int AlertTypeId, bool ContinueIfMatched)>>();
            }

            /* 11-17-2021 MDP

            Talking with some local statistics nerds, 4 datapoints is the absolute bare minimum for an IQR,
            and 5 datapoints is the bare minimum for anything meaningful. It gets
            more meaningful as you approach 10. However, in the case of Church Giving,
            "at least 5 in last 12 months" is a reasonable rule (without making it more complex).

            Therefore here are our rules:
                -  Giving Classifications requires at least 5 in last 12 months
                -  Alert Types
                    - With sensitivity: Requires at least 5 in last 12 months
                    - Without sensitivity, doesn't require transaction history,
            */

            /// <summary>
            /// The minimum transaction count needed to get meaningful statistics
            /// for Alert Types that have either the Amount Sensitivity or Frequency Sensitivity defined.
            /// Need at least 5 in last 12 months (see above engineering note).
            /// </summary>
            public readonly int MinimumTransactionCountForSensitivityAlertTypes = 5;

            /// <summary>
            /// The minimum transaction count for classifications
            /// </summary>
            public readonly int MinimumTransactionCountForClassifications = 5;

            #region Fields

            /// <summary>
            /// Returns true based on <see cref="GivingClassificationSettings.RunDays"/>
            /// </summary>
            /// <value><c>true</c> if this instance is giving classification run day; otherwise, <c>false</c>.</value>
            public bool IsGivingClassificationRunDay { get; set; }

            /// <summary>
            /// Gets the SQL command timeout seconds.
            /// </summary>
            /// <value>The SQL command timeout seconds.</value>
            public int SqlCommandTimeoutSeconds { get; set; }

            /// <summary>
            /// Gets the maximum days since last gift.
            /// </summary>
            /// <value>The maximum days since last gift.</value>
            public int MaxDaysSinceLastGift { get; set; }

            /// <summary>
            /// Gets or sets the time period within which transactions will be considered as a single giving event,
            /// for the purposes of calculating giving frequency and consistency.
            /// See also <see cref="GivingJourneySettings.TransactionWindowDurationHours"/>
            /// </summary>
            /// <value>A period of time in hours.</value>
            public int? TransactionWindowDurationHours { get; set; } = null;

            #endregion Fields

            /// <summary>
            /// The date time to consider as current time. The time when this processing instance began
            /// </summary>
            public DateTime Now { get; set; } = RockDateTime.Now;

            /// <summary>
            /// The errors
            /// </summary>
            public readonly HashSet<string> Errors = new HashSet<string>();

            /// <summary>
            /// Gets or sets the giving ids to classify.
            /// </summary>
            /// <value>
            /// The giving ids to classify.
            /// </value>
            public List<string> GivingIdsToClassify { get; set; }

            /// <summary>
            /// Gets or sets the giving ids classified.
            /// </summary>
            /// <value>
            /// The giving ids classified.
            /// </value>
            public int GivingIdsSuccessful { get; set; }

            /// <summary>
            /// Gets or sets the alerts created.
            /// </summary>
            /// <value>
            /// The alerts created.
            /// </value>
            public int AlertsCreated { get; set; }

            /// <summary>
            /// Gets or sets the giving ids failed.
            /// </summary>
            /// <value>
            /// The giving ids failed.
            /// </value>
            public int GivingIdsFailed { get; set; }

            /// <summary>
            /// Gets or sets the percentile lower range.
            /// Ex. Index 50 holds the lower range for being in the 50th percentile of the givers within the church
            /// </summary>
            /// <value>
            /// The percentile lower range.
            /// </value>
            public List<decimal> PercentileLowerRange { get; set; }

            /// <summary>
            /// Gets or sets the alert types.
            /// </summary>
            /// <value>
            /// The alert types.
            /// </value>
            public List<FinancialTransactionAlertType> AlertTypes { get; set; }

            /// <summary>
            /// Gets or sets the data view person queries rock context.
            /// </summary>
            /// <value>The data view person queries rock context.</value>
            internal RockContext DataViewPersonQueriesRockContext { get; set; }

            /// <summary>
            /// Gets the data view person queries.
            /// </summary>
            /// <value>
            /// The data view person queries.
            /// </value>
            public Dictionary<int, IQueryable<Person>> DataViewPersonQueries { get; }

            /// <summary>
            /// Gets or sets the late alerts by giving identifier.
            /// </summary>
            /// <value>The late alerts by giving identifier.</value>
            internal Dictionary<string, List<(int AlertTypeId, bool ContinueIfMatched)>> LateAlertsByGivingId { get; set; }
        }

        #endregion

        /// <summary>
        /// Alert View
        /// </summary>
        public sealed class AlertView
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
        /// Transaction View
        /// </summary>
        public sealed class TransactionView
        {
            /// <summary>
            /// Gets or sets the transaction date time.
            /// </summary>
            /// <value>
            /// The transaction date time.
            /// </value>
            public DateTime TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the total amount (for all accounts)
            /// </summary>
            /// <value>
            /// The total amount.
            /// </value>
            public decimal TotalTransactionAmount
            {
                get
                {
                    return GetTransactionViewDetails()?.Sum( x => x.Amount ) ?? 0.0M;
                }
            }

            /// <summary>
            /// Gets or sets the currency type value identifier.
            /// </summary>
            /// <value>
            /// The currency type value identifier.
            /// </value>
            public int? CurrencyTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the source type value identifier.
            /// </summary>
            /// <value>
            /// The source type value identifier.
            /// </value>
            public int? SourceTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is scheduled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is scheduled; otherwise, <c>false</c>.
            /// </value>
            public bool IsScheduled { get; set; }

            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the authorized person alias identifier.
            /// </summary>
            /// <value>
            /// The authorized person alias identifier.
            /// </value>
            public int AuthorizedPersonAliasId { get; set; }

            /// <summary>
            /// Gets the authorized person giving identifier.
            /// </summary>
            /// <value>The authorized person giving identifier.</value>
            public string AuthorizedPersonGivingId { get; internal set; }

            /// <summary>
            /// Gets or sets the authorized person campus identifier.
            /// </summary>
            /// <value>The authorized person campus identifier.</value>
            public int? AuthorizedPersonCampusId { get; set; }

            /// <summary>
            /// Gets or sets the transaction view details.
            /// </summary>
            /// <value>The transaction view details.</value>
            public List<TransactionViewDetail> TransactionViewDetailsBeforeRefunds { get; set; }

            /// <summary>
            /// Gets the refund details.
            /// </summary>
            /// <value>The refund details.</value>
            public List<TransactionViewDetail> RefundDetails { get; internal set; }

            /// <summary>
            /// Gets the transaction view details factoring in refunds
            /// </summary>
            /// <returns>List&lt;TransactionViewDetail&gt;.</returns>
            public List<TransactionViewDetail> GetTransactionViewDetails()
            {
                if ( RefundDetails == null || !RefundDetails.Any() )
                {
                    return this.TransactionViewDetailsBeforeRefunds;
                }

                // make sure it is unique by AccountId since we'll have to adjust amount per account id
                var transactionViewDetailsBeforeRefundsByAccountId = this.TransactionViewDetailsBeforeRefunds
                    .GroupBy( a => a.AccountId ).Select( a => new TransactionViewDetail()
                    {
                        AccountId = a.Key,
                        Amount = a.Sum( x => x.Amount ),
                    } ).ToDictionary( k => k.AccountId, v => v );


                var refundDetailsByAccountId = this.RefundDetails
                    .GroupBy( a => a.AccountId ).Select( a => new TransactionViewDetail()
                    {
                        AccountId = a.Key,
                        Amount = a.Sum( x => x.Amount ),
                    } ).ToDictionary( k => k.AccountId, v => v );

                var transactionViewDetails = new List<TransactionViewDetail>();

                var accountIds = transactionViewDetailsBeforeRefundsByAccountId.Select( a => a.Value.AccountId ).ToList();

                foreach ( var accountId in accountIds )
                {
                    var beforeRefundForAccount = transactionViewDetailsBeforeRefundsByAccountId.GetValueOrNull( accountId );
                    var refundForAccount = refundDetailsByAccountId.GetValueOrNull( accountId );
                    var transactionViewDetail = new TransactionViewDetail
                    {
                        AccountId = accountId,
                        Amount = 0.00M
                    };

                    if ( beforeRefundForAccount != null )
                    {
                        transactionViewDetail.Amount += beforeRefundForAccount.Amount;
                    }

                    if ( refundForAccount != null )
                    {
                        // refund amounts are negative so we'll add to get adjusted amount
                        transactionViewDetail.Amount += refundForAccount.Amount;
                    }

                    if ( transactionViewDetail.Amount != 0.00M )
                    {

                        transactionViewDetails.Add( transactionViewDetail );
                    }
                }

                return transactionViewDetails;
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>string.</returns>
            public override string ToString()
            {
                return $"{TransactionDateTime}, GivingId: {AuthorizedPersonGivingId}, TotalAmount: {GetTransactionViewDetails().Sum( a => a.Amount )}";
            }
        }

        /// <summary>
        /// Class TransactionViewDetail. This class cannot be inherited.
        /// </summary>
        public sealed class TransactionViewDetail
        {
            /// <summary>
            /// Gets or sets the account identifier.
            /// </summary>
            /// <value>The account identifier.</value>
            public int AccountId { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            /// <value>The amount.</value>
            public decimal Amount { get; set; }
        }

        private sealed class FrequencyStats
        {
            public DateTime? LastTransactionDateTime { get; set; }

            public decimal MeanFrequencyDays { get; set; }

            public decimal FrequencyStdDevDays { get; set; }

            public DateTime? NextExpectedGiftDate { get; set; }

            public FinancialGivingAnalyticsFrequencyLabel GivingFrequencyLabel { get; set; }
        }

        /// <summary>
        /// Class LateAlertForGivingIdArgs. This class cannot be inherited.
        /// </summary>
        internal sealed class LateAlertForGivingIdArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LateAlertForGivingIdArgs" /> class.
            /// </summary>
            /// <param name="givingId">The giving identifier.</param>
            /// <param name="last12MonthsTransactions">The last12 months transactions.</param>
            /// <param name="recentAlerts">The recent alerts.</param>
            /// <param name="mostRecentOldTransactionDateTime">The most recent old transaction date time.</param>
            internal LateAlertForGivingIdArgs( string givingId, List<TransactionView> last12MonthsTransactions, List<AlertView> recentAlerts, DateTime? mostRecentOldTransactionDateTime )
            {
                GivingId = givingId;
                Last12MonthsTransactions = last12MonthsTransactions;
                RecentAlerts = recentAlerts ?? new List<AlertView>();
                MostRecentOldTransactionDateTime = mostRecentOldTransactionDateTime;
            }

            public readonly string GivingId;
            public readonly List<TransactionView> Last12MonthsTransactions;
            public readonly List<AlertView> RecentAlerts;

            public DateTime? GeMostRecentAlertOfThisTypeAlertDateTime( int alertTypeId )
            {
                if ( !RecentAlerts.Any() )
                {
                    return null;
                }

                return RecentAlerts.Where( a => a.AlertTypeId == alertTypeId ).Max( x => x.AlertDateTime );
            }

            public readonly DateTime? MostRecentOldTransactionDateTime;
        }

        private class GiverBinTotal
        {
            public string GivingId { get; set; }
            public decimal Last12MonthsTotalGiftBeforeRefund { get; set; }
            public decimal? Last12MonthsTotalGiftRefunds { get; set; }
            public decimal Last12MonthsTotalGift
            {
                get
                {
                    if ( Last12MonthsTotalGiftRefunds.HasValue )
                    {
                        return Last12MonthsTotalGiftBeforeRefund + Last12MonthsTotalGiftRefunds.Value;
                    }
                    else
                    {
                        return Last12MonthsTotalGiftBeforeRefund;
                    }
                }
            }
        }
    }
}