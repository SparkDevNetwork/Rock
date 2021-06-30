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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Jobs;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.GivingAnalytics;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Jobs
{
    [TestClass]
    public class GivingAnalyticsJobTests
    {
        #region SplitQuartileRanges

        [TestMethod]
        public void SplitQuartileRanges_EvenCount()
        {
            var orderedValues = new List<decimal> { 1.11m, 2.22m, 3.33m, 4.44m, 5.55m, 6.66m, 7.77m, 8.88m };
            var ranges = Rock.Jobs.GivingAnalytics.SplitQuartileRanges( orderedValues );
            var q1 = ranges.Item1;
            var q2 = ranges.Item2;
            var q3 = ranges.Item3;

            // The first range should be the values before the median values.
            Assert.AreEqual( 1.11m, q1[0] );
            Assert.AreEqual( 2.22m, q1[1] );
            Assert.AreEqual( 3.33m, q1[2] );

            // The middle range should be the middle 2 values since there is an even count. These would be used to get the median.
            Assert.AreEqual( 4.44m, q2[0] );
            Assert.AreEqual( 5.55m, q2[1] );

            // The third range should be the values after the median values.
            Assert.AreEqual( 6.66m, q3[0] );
            Assert.AreEqual( 7.77m, q3[1] );
            Assert.AreEqual( 8.88m, q3[2] );
        }

        [TestMethod]
        public void SplitQuartileRanges_OddCount()
        {
            var orderedValues = new List<decimal> { 1.11m, 2.22m, 3.33m, 4.44m, 5.55m, 6.66m, 7.77m, 8.88m, 9.99m };
            var ranges = Rock.Jobs.GivingAnalytics.SplitQuartileRanges( orderedValues );
            var q1 = ranges.Item1;
            var q2 = ranges.Item2;
            var q3 = ranges.Item3;

            // The first range should be the values before the median values.
            Assert.AreEqual( 1.11m, q1[0] );
            Assert.AreEqual( 2.22m, q1[1] );
            Assert.AreEqual( 3.33m, q1[2] );
            Assert.AreEqual( 4.44m, q1[3] );

            // The middle range should be the middle value since there is an odd count. This is the median.
            Assert.AreEqual( 5.55m, q2[0] );

            // The third range should be the values after the median values.
            Assert.AreEqual( 6.66m, q3[0] );
            Assert.AreEqual( 7.77m, q3[1] );
            Assert.AreEqual( 8.88m, q3[2] );
            Assert.AreEqual( 9.99m, q3[3] );
        }

        #endregion SplitQuartileRanges

        #region GetAmountIqrCount

        /// <summary>
        /// Tests that normal amount deviation count calculates correctly.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 30.0, 0.0 )]
        [DataRow( 32.5, 1.0 )]
        [DataRow( 35.0, 2.0 )]
        [DataRow( 27.5, -1.0 )]
        [DataRow( 25.0, -2.0 )]
        [DataRow( 31.0, 0.4 )]
        [DataRow( 29.0, -0.4 )]
        [DataRow( -9999999.9, -1000.0 )]
        [DataRow( 9999999.9, 1000.0 )]
        public void GetAmountIqrCount_CalculatesCorrectly( double amount, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 30m;
            var amountIqr = 2.5m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
            }

            var amountIqrCount = Rock.Jobs.GivingAnalytics.GetAmountIqrCount( context, people, Convert.ToDecimal( amount ) );
            Assert.AreEqual( Convert.ToDecimal( expected ), amountIqrCount );
        }

        /// <summary>
        /// Tests that amount deviation count calculates correctly when standard deviation is 0.  There is a fallback of 15%.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 30.0, 0.0 )]
        [DataRow( 34.5, 1.0 )]
        [DataRow( 39.0, 2.0 )]
        [DataRow( 25.5, -1.0 )]
        [DataRow( 21.0, -2.0 )]
        [DataRow( 30.45, 0.1 )]
        [DataRow( 29.55, -0.1 )]
        [DataRow( -9999999.9, -1000.0 )]
        [DataRow( 9999999.9, 1000.0 )]
        public void GetAmountIqrCount_CalculatesCorrectlyWhenLowMedian( double amount, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 30m;
            var amountIqr = 0.0m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
            }

            var amountIqrCount = Rock.Jobs.GivingAnalytics.GetAmountIqrCount( context, people, Convert.ToDecimal( amount ) );
            Assert.AreEqual( Convert.ToDecimal( expected ), amountIqrCount );
        }

        /// <summary>
        /// Tests that amount deviation count calculates correctly when standard deviation is 0 and the median is also 0.
        /// In this case the fallback is 100.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 100.0, 1.0 )]
        [DataRow( 9.0, 0.09 )]
        [DataRow( -9999999.9, -1000.0 )]
        [DataRow( 9999999.9, 1000.0 )]
        public void GetAmountIqrCount_CalculatesCorrectlyWhenLowIqrAndMedian( double amount, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 0m;
            var amountIqr = 0m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
            }

            var amountIqrCount = Rock.Jobs.GivingAnalytics.GetAmountIqrCount( context, people, Convert.ToDecimal( amount ) );
            Assert.AreEqual( Convert.ToDecimal( expected ), amountIqrCount );
        }

        #endregion GetAmountIqrCount

        #region GetFrequencyDeviationCount

        /// <summary>
        /// Tests that normal frequency deviation count calculates correctly.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 30.0, 0.0 )]
        [DataRow( 32.5, -1.0 )]
        [DataRow( 35.0, -2.0 )]
        [DataRow( 27.5, 1.0 )]
        [DataRow( 25.0, 2.0 )]
        [DataRow( 31.0, -0.4 )]
        [DataRow( 29.0, 0.4 )]
        [DataRow( -9999999.9, 1000.0 )]
        [DataRow( 9999999.9, -1000.0 )]
        public void GetFrequencyDeviationCount_CalculatesCorrectly( double daysSinceLastTransaction, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var frequencyMean = 30m;
            var frequencyStdDev = 2.5m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var frequencyDeviationCount = Rock.Jobs.GivingAnalytics.GetFrequencyDeviationCount( context, people, daysSinceLastTransaction );
            Assert.AreEqual( Convert.ToDecimal( expected ), frequencyDeviationCount );
        }

        /// <summary>
        /// Tests that frequency deviation count calculates correctly when standard deviation is below 1.  There is a fallback of 15%
        /// for the standard deviation when the actual number is less than 1.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 30.0, 0.0 )]
        [DataRow( 34.5, -1.0 )]
        [DataRow( 39.0, -2.0 )]
        [DataRow( 25.5, 1.0 )]
        [DataRow( 21.0, 2.0 )]
        [DataRow( 30.45, -0.1 )]
        [DataRow( 29.55, 0.1 )]
        [DataRow( -9999999.9, 1000.0 )]
        [DataRow( 9999999.9, -1000.0 )]
        public void GetFrequencyDeviationCount_CalculatesCorrectlyWhenLowStdDev( double daysSinceLastTransaction, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var frequencyMean = 30m;
            var frequencyStdDev = 0.5m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var frequencyDeviationCount = Rock.Jobs.GivingAnalytics.GetFrequencyDeviationCount( context, people, daysSinceLastTransaction );
            Assert.AreEqual( Convert.ToDecimal( expected ), frequencyDeviationCount );
        }

        /// <summary>
        /// Tests that frequency deviation count calculates correctly when standard deviation is below 1 and mean is also low.
        /// There is a fallback of 3 for the standard deviation when this occurs.
        /// </summary>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <param name="expected">The expected.</param>
        [TestMethod]
        [DataRow( 6.0, 0.0 )]
        [DataRow( 9.0, -1.0 )]
        [DataRow( 12.0, -2.0 )]
        [DataRow( 3.0, 1.0 )]
        [DataRow( 0.0, 2.0 )]
        [DataRow( 6.3, -0.1 )]
        [DataRow( 5.7, 0.1 )]
        [DataRow( -9999999.9, 1000.0 )]
        [DataRow( 9999999.9, -1000.0 )]
        public void GetFrequencyDeviationCount_CalculatesCorrectlyWhenLowStdDevAndMean( double daysSinceLastTransaction, double expected )
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext );

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var frequencyMean = 6m;
            var frequencyStdDev = 0.5m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var frequencyDeviationCount = Rock.Jobs.GivingAnalytics.GetFrequencyDeviationCount( context, people, daysSinceLastTransaction );
            Assert.AreEqual( Convert.ToDecimal( expected ), frequencyDeviationCount );
        }

        #endregion GetFrequencyDeviationCount

        #region CreateAlertsForLateTransaction

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_CreatesAlertForMissingGift()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType> {
                new FinancialTransactionAlertType {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 3,
                    MaximumDaysSinceLastGift = 35,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. Skips the
        /// first alert type because of alert type.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsAlertTypeBecauseOfDataview()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType>
            {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    DataViewId = 1
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    DataViewId = 2
                },
            };

            var people = new List<Person>
            {
                new Person { Id = 1 },
                new Person { Id = 2 }
            };

            context.DataViewPersonQueries[1] = new List<int> { 3, 4, 5 }.AsQueryable();
            context.DataViewPersonQueries[2] = new List<int> { 2, 3, 4 }.AsQueryable();

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. The first
        /// alert type alerted and has a repeat prevention that should block a new alert of
        /// that type.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsAlertTypeRecentlyAlerted()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType> {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    RepeatPreventionDuration = 5
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<AlertView>
            {
                new AlertView
                {
                    AlertDateTime = context.Now.AddDays( -4 ),
                    AlertType = AlertType.FollowUp,
                    AlertTypeId = 1
                }
            };
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days.
        /// that type.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsAlertTypeBecauseOfMedianAmount()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType> {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp,
                    MinimumMedianGiftAmount = 500.01m
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp,
                    MaximumMedianGiftAmount = 499.99m
                },
                new FinancialTransactionAlertType
                {
                    Id = 3,
                    Order = 3,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<AlertView>
            {
                new AlertView
                {
                    AlertDateTime = context.Now.AddDays( -4 ),
                    AlertType = AlertType.FollowUp,
                    AlertTypeId = 1
                }
            };
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 3, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. The first
        /// alert type alerted and has a repeat prevention that should block a new alert of
        /// that type.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsAlertBecauseOfCampus()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType> {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    CampusId = 1
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    CampusId = 2
                },
            };

            var family = new Group { Campus = new Campus { Id = 2 } };

            var people = new List<Person>
            {
                new Person { PrimaryFamily = family },
                new Person { PrimaryFamily = family }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<AlertView>
            {
                new AlertView
                {
                    AlertDateTime = context.Now.AddDays( -4 ),
                    AlertType = AlertType.FollowUp,
                    AlertTypeId = 1
                }
            };
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. The first alert type
        /// is below sensitivity, but the second is not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsUnmetSensitivityAlertType()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType>
            {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 4,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 3,
                    Order = 3,
                    FrequencySensitivityScale = 2,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp
                },
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.IsNull( alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.IsNull( alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.IsNull( alert.FrequencyDifferenceFromMean );
            Assert.IsNull( alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. The first alert type
        /// is below sensitivity, but the second and third are not. The second continues and allows the third
        /// to be created.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_CreatesMultipleAlerts()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
            };

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType>
            {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    FrequencySensitivityScale = 4,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 3,
                    Order = 3,
                    FrequencySensitivityScale = 2,
                    ContinueIfMatched = false,
                    AlertType = AlertType.FollowUp
                },
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGave.ToISO8601DateString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastTransactionAuthorizedAliasId = 999;

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForLateTransaction( null, lateGiftAlertTypes, lastTransactionAuthorizedAliasId, people, recentAlerts, context );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 2, alerts.Count );

            Assert.AreEqual( 2, alerts[0].AlertTypeId );
            Assert.AreEqual( 3, alerts[1].AlertTypeId );

            foreach ( var alert in alerts )
            {
                Assert.AreEqual( context.Now, alert.AlertDateTime );
                Assert.IsNull( alert.TransactionId );

                Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
                Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
                Assert.IsNull( alert.AmountIqrMultiplier );

                Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
                Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
                Assert.IsNull( alert.FrequencyDifferenceFromMean );
                Assert.IsNull( alert.FrequencyZScore );

                var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
                Assert.IsNotNull( reasons );
                Assert.AreEqual( 1, reasons.Count );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
            }
        }

        #endregion CreateAlertsForLateTransaction

        #region CreateAlertsForTransaction

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertForLargeGift()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 33,
                        Order = 3,
                        AmountSensitivityScale = 3,
                        MaximumDaysSinceLastGift = 10,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 4,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 5,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 6,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 3, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There is an alert type that has no sensitivity settings. It should be matched because of the other filters.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertWithNoSensitivity()
        {
            var jobExecutionContext = new TestJobContext();
            var now = new DateTime( 2020, 3, 1 );
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                Now = now,
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 4,
                        MinimumGiftAmount = 999,
                        MaximumGiftAmount = 1001,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 5,
                        MinimumGiftAmount = 1001,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 6,
                        Order = 6,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 7,
                        Order = 7,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = now.AddDays( -36 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = now.AddDays( -5 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 4, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 0, reasons.Count );
        }

        /// <summary>
        /// Tests an example transaction that is small
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is smaller in amount at $100.
        /// This also tests that two alert types can both make alerts for a single transaction.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertForSmallGift()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 4,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 5,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 6,
                        Order = 6,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 100m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 2, alerts.Count );

            foreach ( var alert in alerts )
            {
                Assert.AreEqual( context.Now, alert.AlertDateTime );
                Assert.AreEqual( transaction.Id, alert.TransactionId );

                Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
                Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
                Assert.AreEqual( -4m, alert.AmountIqrMultiplier );

                Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
                Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
                Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
                Assert.AreEqual( -0.5m, alert.FrequencyZScore );

                var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
                Assert.IsNotNull( reasons );
                Assert.AreEqual( 1, reasons.Count );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
            }

            var alert1 = alerts.First();
            Assert.AreEqual( 2, alert1.AlertTypeId );

            var alert2 = alerts.Last();
            Assert.AreEqual( 6, alert2.AlertTypeId );
        }

        /// <summary>
        /// Tests an example transaction that is early
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is about half a month early.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertForEarlyGift()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 4,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 5,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 1, 15 ),
                TotalAmount = 650m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 4, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 1.5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( 16m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( 8m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There are two rules that could be triggered, but one has a minimum amount that is not met. Also tests continue
        /// if matched being false.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsAlertTypeBecauseOfMinAmount()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 1000.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MinimumGiftAmount = 1000m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MinimumGiftAmount = 0m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There are two rules that could be triggered, but one has a minimum amount that is not met. Also tests continue
        /// if matched being false.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsAlertTypeBecauseOfMaxAmount()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MaximumGiftAmount = 999.99m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MaximumGiftAmount = 1000m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MaximumGiftAmount = 1000.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There are two rules that could be triggered, but one has a minimum median amount that is not met. Also tests continue
        /// if matched being false.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsAlertTypeBecauseOfMinMedianAmount()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumMedianGiftAmount = 500.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MinimumMedianGiftAmount = 500.00m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MinimumMedianGiftAmount = 499.99m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There are two rules that could be triggered, but one has a max median amount that is not met. Also tests continue
        /// if matched being false.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsAlertTypeBecauseOfMaxMedianAmount()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MaximumMedianGiftAmount = 499.99m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MaximumMedianGiftAmount = 500.00m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MaximumMedianGiftAmount = 500.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// One of the rules has a dataview constraint that matches, and one does not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsBecauseDataview()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        DataViewId = 1,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        DataViewId = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person { Id = 1 },
                new Person { Id = 2 }
            };

            context.DataViewPersonQueries[1] = new List<int> { 3, 4, 5 }.AsQueryable();
            context.DataViewPersonQueries[2] = new List<int> { 2, 3, 4 }.AsQueryable();

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// One of the rules has a campus constraint that matches, and one does not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsBecauseCampus()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        CampusId = 1,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        CampusId = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var family = new Group { Campus = new Campus { Id = 2 } };

            var people = new List<Person>
            {
                new Person { PrimaryFamily = family },
                new Person { PrimaryFamily = family }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var recentAlerts = new List<Rock.Jobs.AlertView>();
            var lastGiftDate = new DateTime( 2000, 1, 1 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = new DateTime( 2000, 2, 1 ),
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( -1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( -0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// One of the rules has a repeat constraint that matches, and one does not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_SkipsRepeatPrevention()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        RepeatPreventionDuration = 30,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        RepeatPreventionDuration = 10,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var now = RockDateTime.Now;
            var recentAlerts = new List<Rock.Jobs.AlertView> {
                new AlertView { AlertTypeId = 1, AlertDateTime = now.AddDays(-20) },
                new AlertView { AlertTypeId = 2, AlertDateTime = now.AddDays(-20) }
            };
            var lastGiftDate = now.AddDays( -29 );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = now,
                TotalAmount = 1000m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 1, alerts.Count );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            Assert.AreEqual( amountMedian, alert.AmountCurrentMedian );
            Assert.AreEqual( amountIqr, alert.AmountCurrentIqr );
            Assert.AreEqual( 5m, alert.AmountIqrMultiplier );

            Assert.AreEqual( frequencyMean, alert.FrequencyCurrentMean );
            Assert.AreEqual( frequencyStdDev, alert.FrequencyCurrentStandardDeviation );
            Assert.AreEqual( 1m, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( 0.5m, alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.AreEqual( 1, reasons.Count );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family always gives monthly exactly $500. This gift is larger in amount at $501. Make sure
        /// an alert is not triggered for a small increase that technically is infinite on the sensitivity scale
        /// because ($1 / 0 => infinite).
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_AccountsForZeroStdDev()
        {
            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        AmountSensitivityScale = 1,
                        FrequencySensitivityScale = 1,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 1,
                        FrequencySensitivityScale = 1,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var people = new List<Person>
            {
                new Person(),
                new Person()
            };

            var amountMedian = 500m;
            var amountIqr = 0m;
            var frequencyMean = 30m;
            var frequencyStdDev = 0m;

            foreach ( var person in people )
            {
                person.LoadAttributes();
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, amountMedian.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, amountIqr.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, frequencyMean.ToString() );
                SetAttributeValue( person, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDev.ToString() );
            }

            var now = context.Now;
            var recentAlerts = new List<Rock.Jobs.AlertView> { };
            var lastGiftDate = now.AddDays( 0 - ( int ) frequencyMean );

            var transaction = new Rock.Jobs.TransactionView
            {
                Id = 888,
                TransactionDateTime = now.AddDays( -1 ),
                TotalAmount = 501m
            };

            var alerts = Rock.Jobs.GivingAnalytics.CreateAlertsForTransaction( null, people, recentAlerts, transaction, lastGiftDate, context, true, true );

            Assert.IsNotNull( alerts );
            Assert.AreEqual( 0, alerts.Count );
        }

        #endregion CreateAlertsForTransaction

        #region UpdateGivingUnitClassifications

        /// <summary>
        /// Tests an example giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesMonthlyCorrectly()
        {
            SetGivingAnalyticsSetting( 20000, 10000, 1000, 0 );

            var givingGroupId = 800;
            var givingId = $"G{givingGroupId}";

            var firstCurrencyTypeValueGuid = Guid.NewGuid();
            var secondCurrencyTypeValueGuid = Guid.NewGuid();

            var firstSourceTypeValueGuid = Guid.NewGuid();
            var secondSourceTypeValueGuid = Guid.NewGuid();

            var mostRecentOldTransactionDate = new DateTime( 2019, 12, 29 );
            var minDate = new DateTime( 2020, 1, 1 );

            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                PercentileLowerRange = new List<decimal>()
            };

            for ( var i = 0; i < 100; i++ )
            {
                context.PercentileLowerRange.Add( 200 * i );
            }

            var people = new List<Person>
            {
                new Person
                {
                    GivingGroupId = givingGroupId
                },
                new Person
                {
                    GivingGroupId = givingGroupId
                }
            };

            people.ForEach( p => p.LoadAttributes() );

            var transactions = new List<TransactionView>();
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 28 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 750.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 11 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 26 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 11 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 11 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 11 ),
                CurrencyTypeValueGuid = firstCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = firstSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = secondSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = secondSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = secondSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 11 ),
                CurrencyTypeValueGuid = secondCurrencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = secondSourceTypeValueGuid,
                TotalAmount = 1200.0000000000m,
            } );

            Rock.Jobs.GivingAnalytics.UpdateGivingUnitClassifications( givingId, people, transactions, mostRecentOldTransactionDate, context, minDate );

            Assert.AreEqual( 0, context.Errors.Count );

            // Preferred Currency - Defined Type
            var firstPerson = people.First();
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY );
            var preferredCurrency = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY ).AsGuidOrNull();
            Assert.AreEqual( secondCurrencyTypeValueGuid, preferredCurrency );

            // Preferred Source - Defined Type
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE );
            var preferredSource = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE ).AsGuidOrNull();
            Assert.AreEqual( firstSourceTypeValueGuid, preferredSource );

            // Frequency Label - Single Select (1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            var frequencyLabel = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL ).AsIntegerOrNull();
            Assert.AreEqual( 3, frequencyLabel );

            // Percent of Gifts Scheduled - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED );
            var percentScheduled = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED ).AsIntegerOrNull();
            Assert.AreEqual( 92, percentScheduled );

            // Gift Amount: Median - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN );
            var medianGivingAmount = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimalOrNull();
            Assert.AreEqual( 1200.00m, decimal.Round( medianGivingAmount ?? 0, 2 ) );

            // Gift Amount: IQR - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR );
            var iqr = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimalOrNull();
            Assert.AreEqual( 50.00m, decimal.Round( iqr ?? 0, 2 ) );

            // Gift Frequency Days: Mean - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS );
            var meanFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 26.50m, decimal.Round( meanFrequencyDays ?? 0, 2 ) );

            // Gift Frequency Days: Standard Deviation - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS );
            var stdDevFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 7.04m, decimal.Round( stdDevFrequencyDays ?? 0, 2 ) );

            // Giving Bin - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_BIN );
            var bin = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_BIN ).AsIntegerOrNull();
            Assert.AreEqual( 2, bin );

            // Giving Percentile - Number - This will be rounded to the nearest percent and stored as a whole number (15 vs .15)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );
            var percentile = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE ).AsIntegerOrNull();
            Assert.AreEqual( 66, percentile );

            // Last Gift Date - Exists, but link to the ‘Giving Analytics’ category
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            var lastGave = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE ).AsDateTime();
            Assert.AreEqual( new DateTime( 2020, 11, 11 ), lastGave );

            // Next Expected Gift Date - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE );
            var nextExpected = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE ).AsDateTime();
            Assert.AreEqual( lastGave.Value.AddDays( ( double ) meanFrequencyDays.Value ), nextExpected );

            // Last Classified - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE );
            var lastClassified = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime();
            Assert.AreEqual( context.Now, lastClassified );
        }

        /// <summary>
        /// Tests an example a giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesWeeklyCorrectly()
        {
            SetGivingAnalyticsSetting( 20000, 10000, 1000, 0 );

            var givingGroupId = 900;
            var givingId = $"G{givingGroupId}";

            var currencyTypeValueGuid = Guid.NewGuid();
            var sourceTypeValueGuid = Guid.NewGuid();

            var mostRecentOldTransactionDate = new DateTime( 2019, 12, 28 );
            var minDate = new DateTime( 2020, 1, 1 );

            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                PercentileLowerRange = new List<decimal>(),
                Now = new DateTime( 2020, 12, 1 )
            };

            for ( var i = 0; i < 100; i++ )
            {
                context.PercentileLowerRange.Add( 200 * i );
            }

            var people = new List<Person>
            {
                new Person
                {
                    GivingGroupId = givingGroupId
                },
                new Person
                {
                    GivingGroupId = givingGroupId
                },
                new Person
                {
                    GivingGroupId = givingGroupId
                },
                new Person
                {
                    GivingGroupId = givingGroupId
                }
            };

            people.ForEach( p => p.LoadAttributes() );

            var transactions = new List<TransactionView>();
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 11 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 7 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 14 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 21 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 11 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 2 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 9 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 23 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 6 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 20 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 27 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 11 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 5 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 12 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 100000.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 19 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 26 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 10 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 17 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 31 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 7 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 14 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = true,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 500.0000000000m,
            } );

            Rock.Jobs.GivingAnalytics.UpdateGivingUnitClassifications( givingId, people, transactions, mostRecentOldTransactionDate, context, minDate );

            Assert.AreEqual( 0, context.Errors.Count );

            // Preferred Currency - Defined Type
            var firstPerson = people.First();
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY );
            var preferredCurrency = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY ).AsGuidOrNull();
            Assert.AreEqual( currencyTypeValueGuid, preferredCurrency );

            // Preferred Source - Defined Type
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE );
            var preferredSource = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE ).AsGuidOrNull();
            Assert.AreEqual( sourceTypeValueGuid, preferredSource );

            // Frequency Label - Single Select (1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            var frequencyLabel = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL ).AsIntegerOrNull();
            Assert.AreEqual( 1, frequencyLabel );

            // Percent of Gifts Scheduled - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED );
            var percentScheduled = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED ).AsIntegerOrNull();
            Assert.AreEqual( 98, percentScheduled );

            // Gift Amount: Median - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN );
            var medianGivingAmount = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimalOrNull();
            Assert.AreEqual( 500.00m, decimal.Round( medianGivingAmount ?? 0, 2 ) );

            // Gift Amount: IQR - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR );
            var iqr = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimalOrNull();
            Assert.AreEqual( 0.00m, decimal.Round( iqr ?? 0, 2 ) );

            // Gift Frequency Days: Mean - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS );
            var meanFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 6.85m, decimal.Round( meanFrequencyDays ?? 0, 2 ) );

            // Gift Frequency Days: Standard Deviation - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS );
            var stdDevFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 0.87m, decimal.Round( stdDevFrequencyDays ?? 0, 2 ) );

            // Giving Bin - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_BIN );
            var bin = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_BIN ).AsIntegerOrNull();
            Assert.AreEqual( 1, bin );

            // Giving Percentile - Number - This will be rounded to the nearest percent and stored as a whole number (15 vs .15)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );
            var percentile = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE ).AsIntegerOrNull();
            Assert.AreEqual( 99, percentile );

            // Last Gift Date - Exists, but link to the ‘Giving Analytics’ category
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            var lastGave = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE ).AsDateTime();
            Assert.AreEqual( new DateTime( 2020, 11, 14 ), lastGave );

            // Next Expected Gift Date - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE );
            var nextExpected = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE ).AsDateTime();
            Assert.AreEqual( lastGave.Value.AddDays( ( double ) meanFrequencyDays.Value ), nextExpected );

            // Last Classified - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE );
            var lastClassified = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime();
            Assert.AreEqual( context.Now, lastClassified );

            // 90 day count
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_90_DAYS_COUNT );
            var ninetyDayCount = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_90_DAYS_COUNT ).AsIntegerOrNull();
            Assert.AreEqual( 12, ninetyDayCount );

            // Annual day count
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS_COUNT );
            var annualCount = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS_COUNT ).AsIntegerOrNull();
            Assert.AreEqual( 47, annualCount );

            // Annual sum
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS );
            var annualSum = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS ).AsDecimalOrNull();
            Assert.AreEqual( 46 * 500m + 100000m, annualSum );

            // 90 day sum
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_90_DAYS );
            var ninetyDaySum = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_90_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 11 * 500m + 100000m, ninetyDaySum );

            // Prior 90 day sum
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PRIOR_90_DAYS );
            var priorNinetyDaySum = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PRIOR_90_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 13 * 500m, priorNinetyDaySum );
        }

        /// <summary>
        /// Tests an example a giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesErraticCorrectly()
        {
            SetGivingAnalyticsSetting( 20000, 10000, 1000, 0 );

            var personId = 1111;
            var givingId = $"P{personId}";

            var currencyTypeValueGuid = Guid.NewGuid();
            var sourceTypeValueGuid = Guid.NewGuid();

            var mostRecentOldTransactionDate = new DateTime( 2019, 12, 27 );
            var minDate = new DateTime( 2020, 1, 1 );

            var jobExecutionContext = new TestJobContext();
            var context = new GivingAnalyticsContext( jobExecutionContext )
            {
                PercentileLowerRange = new List<decimal>()
            };

            for ( var i = 0; i < 100; i++ )
            {
                context.PercentileLowerRange.Add( 200 * i );
            }

            var people = new List<Person>
            {
                new Person
                {
                    Id = personId,
                    GivingGroupId = null
                },
            };

            people.ForEach( p => p.LoadAttributes() );

            var transactions = new List<TransactionView>();
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 50.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 180.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 82.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 45.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 1, 31 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 155.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 85.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 140.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 30.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 115.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 2, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 82.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 6 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 66.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 20 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 20 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 97.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 90.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 140.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 3, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 63.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 17.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 10 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 81.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 10 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 19.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 120.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 15 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 120.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 17 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 12.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 17 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 98.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 112.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 18.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 4, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 200.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 110.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 7 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 7 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 25.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 17.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 14 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 25.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 120.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 20.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 19 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 35.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 21 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 21 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 135.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 12.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 108.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 5, 29 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 110.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 75.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 140.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 22.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 98.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 116.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 14.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 20.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 140.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 19 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 18.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 19 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 132.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 80.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 26 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 6, 26 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 105.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 100.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 107.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 13.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 5 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 35.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 10 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 85.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 12 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 100.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 12 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 20.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 17 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 135.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 20 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 110.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 70.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 50.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 25 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 80.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 7, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 1 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 75.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 8 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 80.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 120.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 75.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 13.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 77.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 14 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 73.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 8, 28 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 17.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 18.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 4 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 72.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 10 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 130.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 96.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 14.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 18 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 66.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 24 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 175.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 26 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 20.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 9, 26 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 110.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 3 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 95.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 9 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 125.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 9 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 136.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 9 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 14.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 95.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 82.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 16 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 18.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 22 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 125.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 23 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 12.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 23 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 70.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 150.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 110.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 70.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 10, 30 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 6 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 190.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 6 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 10.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 9 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 175.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 85.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 13 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 15.0000000000m,
            } );
            transactions.Add( new TransactionView
            {
                TransactionDateTime = new DateTime( 2020, 11, 14 ),
                CurrencyTypeValueGuid = currencyTypeValueGuid,
                IsScheduled = false,
                SourceTypeValueGuid = sourceTypeValueGuid,
                TotalAmount = 140.0000000000m,
            } );

            Rock.Jobs.GivingAnalytics.UpdateGivingUnitClassifications( givingId, people, transactions, mostRecentOldTransactionDate, context, minDate );

            Assert.AreEqual( 0, context.Errors.Count );

            // Preferred Currency - Defined Type
            var firstPerson = people.First();
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY );
            var preferredCurrency = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY ).AsGuidOrNull();
            Assert.AreEqual( currencyTypeValueGuid, preferredCurrency );

            // Preferred Source - Defined Type
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE );
            var preferredSource = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE ).AsGuidOrNull();
            Assert.AreEqual( sourceTypeValueGuid, preferredSource );

            // Frequency Label - Single Select (1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            var frequencyLabel = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL ).AsIntegerOrNull();
            Assert.AreEqual( 5, frequencyLabel );

            // Percent of Gifts Scheduled - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED );
            var percentScheduled = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED ).AsIntegerOrNull();
            Assert.AreEqual( 0, percentScheduled );

            // Gift Amount: Median - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN );
            var medianGivingAmount = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimalOrNull();
            Assert.AreEqual( 81.00m, decimal.Round( medianGivingAmount ?? 0, 2 ) );

            // Gift Amount: IQR - Currency
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR );
            var iqr = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimalOrNull();
            Assert.AreEqual( 101.50m, decimal.Round( iqr ?? 0, 2 ) );

            // Gift Frequency Days: Mean - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS );
            var meanFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 2.76m, decimal.Round( meanFrequencyDays ?? 0, 2 ) );

            // Gift Frequency Days: Standard Deviation - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS );
            var stdDevFrequencyDays = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimalOrNull();
            Assert.AreEqual( 3.17m, decimal.Round( stdDevFrequencyDays ?? 0, 2 ) );

            // Giving Bin - Number
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_BIN );
            var bin = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_BIN ).AsIntegerOrNull();
            Assert.AreEqual( 3, bin );

            // Giving Percentile - Number - This will be rounded to the nearest percent and stored as a whole number (15 vs .15)
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );
            var percentile = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE ).AsIntegerOrNull();
            Assert.AreEqual( 45, percentile );

            // Last Gift Date - Exists, but link to the ‘Giving Analytics’ category
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            var lastGave = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE ).AsDateTime();
            Assert.AreEqual( new DateTime( 2020, 11, 14 ), lastGave );

            // Next Expected Gift Date - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE );
            var nextExpected = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE ).AsDateTime();
            Assert.AreEqual( lastGave.Value.AddDays( ( double ) meanFrequencyDays.Value ), nextExpected );

            // Last Classified - Date
            AssertPeopleHaveSameAttributeValue( people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE );
            var lastClassified = GetAttributeValue( firstPerson, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime();
            Assert.AreEqual( context.Now, lastClassified );
        }

        #endregion UpdateGivingUnitClassifications

        #region Helpers

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        private static string GetAttributeKey( string guidString )
        {
            var key = AttributeCache.Get( guidString )?.Key;

            if ( key.IsNullOrWhiteSpace() )
            {
                return "%$$$ KEY DOES NOT EXIST $$$%";
            }

            return key;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        private static string GetAttributeValue( Person person, string guidString )
        {
            var key = GetAttributeKey( guidString );
            return person.GetAttributeValue( key );
        }

        /// <summary>
        /// Sets the attribute value.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static void SetAttributeValue( Person person, string guidString, string value )
        {
            var key = GetAttributeKey( guidString );
            person.SetAttributeValue( key, value );
        }

        /// <summary>
        /// Asserts the people have same attribute value.
        /// </summary>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        private static void AssertPeopleHaveSameAttributeValue( List<Person> people, string guidString )
        {
            var value = GetAttributeValue( people[0], guidString );

            for ( var i = 1; i < people.Count; i++ )
            {
                var otherValue = GetAttributeValue( people[i], guidString );
                Assert.AreEqual( value, otherValue );
            }
        }

        /// <summary>
        /// Creates the giving analytics setting.
        /// </summary>
        /// <param name="bin1">The bin1.</param>
        /// <param name="bin2">The bin2.</param>
        /// <param name="bin3">The bin3.</param>
        /// <param name="bin4">The bin4.</param>
        /// <returns></returns>
        private static void SetGivingAnalyticsSetting( decimal bin1, decimal bin2, decimal bin3, decimal bin4 )
        {
            var settings = new GivingAnalyticsSetting();
            var givingAnalytics = settings.GivingAnalytics ?? new Rock.Utility.Settings.GivingAnalytics.GivingAnalytics();
            settings.GivingAnalytics = givingAnalytics;

            settings.GivingAnalytics.GiverBins = new List<GiverBin> {
                new GiverBin { LowerLimit = bin1 },
                new GiverBin { LowerLimit = bin2 },
                new GiverBin { LowerLimit = bin3 },
                new GiverBin { LowerLimit = bin4 }
            };

            Rock.Web.SystemSettings.SetValue( SystemSetting.GIVING_ANALYTICS_CONFIGURATION, settings.ToJson() );
        }

        #endregion Helpers
    }
}
