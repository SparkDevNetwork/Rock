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

using Rock.Financial;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

using static Rock.Jobs.GivingAutomation;

namespace Rock.Tests.Integration.Core.Jobs
{
    [TestClass]
    public class GivingAutomationClassificationTests : DatabaseTestsBase
    {
        #region SplitQuartileRanges

        [TestMethod]
        public void SplitQuartileRanges_EvenCount()
        {
            var orderedValues = new List<decimal> { 1.11m, 2.22m, 3.33m, 4.44m, 5.55m, 6.66m, 7.77m, 8.88m };
            var ranges = GivingAutomationHelper.GetQuartileRanges( orderedValues );
            var q1 = ranges.Q1MedianRange;
            var q2 = ranges.Q2MedianRange;
            var q3 = ranges.Q3MedianRange;

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
            var ranges = GivingAutomationHelper.GetQuartileRanges( orderedValues );
            var q1 = ranges.Q1MedianRange;
            var q2 = ranges.Q2MedianRange;
            var q3 = ranges.Q3MedianRange;


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
        /// Tests that normal amount deviation count calculates correctly when median is $30
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
            var now = new GivingAutomation.GivingAutomationContext().Now;

            var amountMedian = 30m;
            var amountIqr = 2.5m;

            var testTransactions = GenerateTestTransactions( amountMedian, amountIqr, 30, 0, now );
            var transactionAmounts = testTransactions.Select( a => a.TotalAmount ).ToList();

            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactionAmounts );

            var amountIqrCount = GivingAutomationHelper.GetAmountIqrCount( quartileRanges, Convert.ToDecimal( amount ) );

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
            var now = new GivingAutomation.GivingAutomationContext().Now;

            var amountMedian = 30m;
            var amountIqr = 0m;

            var testTransactions = GenerateTestTransactions( amountMedian, amountIqr, 30, 0, now );
            var transactionAmounts = testTransactions.Select( a => a.TotalAmount ).ToList();

            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactionAmounts );

            var amountIqrCount = GivingAutomationHelper.GetAmountIqrCount( quartileRanges, Convert.ToDecimal( amount ) );
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
            var now = new GivingAutomation.GivingAutomationContext().Now;

            var amountMedian = 0m;
            var amountIqr = 0m;

            var testTransactions = GenerateTestTransactions( amountMedian, amountIqr, 30, 0, now );
            var transactionAmounts = testTransactions.Select( a => a.TotalAmount ).ToList();

            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactionAmounts );

            var amountIqrCount = GivingAutomationHelper.GetAmountIqrCount( quartileRanges, Convert.ToDecimal( amount ) );

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
            var frequencyMean = 30m;
            var frequencyStdDev = 2.5m;

            var frequencyDeviationCount = GivingAutomationHelper.GetFrequencyDeviationCount( frequencyStdDev, frequencyMean, ( decimal ) daysSinceLastTransaction );
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
            var frequencyMean = 30m;
            var frequencyStdDev = 0.5m;

            var frequencyDeviationCount = GivingAutomationHelper.GetFrequencyDeviationCount( frequencyStdDev, frequencyMean, ( decimal ) daysSinceLastTransaction );
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
            var frequencyMean = 6m;
            var frequencyStdDev = 0.5m;

            var frequencyDeviationCount = GivingAutomationHelper.GetFrequencyDeviationCount( frequencyStdDev, frequencyMean, ( decimal ) daysSinceLastTransaction );
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
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

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
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );

            // Builder expects recent alerts for THIS alert type only (most recent first).
            var recentAlertsOfThisTypeByAlertTypeId = new Dictionary<int, List<AlertView>>
            {
                [1] = new List<AlertView>
                {
                    new AlertView { AlertDateTime = context.Now.AddDays( -4 ), AlertType = AlertType.FollowUp, AlertTypeId = 1, TransactionId = null }
                }
            };

            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions, recentAlertsOfThisTypeByAlertTypeId );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

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
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

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
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            last12MonthsTransactions.ForEach( t => t.AuthorizedPersonCampusId = 2 );

            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

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
            Assert.HasCount( 1, reasons );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// One of the rules has a dataview constraint that matches, and one does not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsAlertBecauseOfDataview()
        {
            var context = new GivingAutomation.GivingAutomationContext();

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType> {
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G200";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            last12MonthsTransactions.ForEach( t => t.AuthorizedPersonCampusId = 2 );

            // Simulate DataView filtering that now happens outside the builder.
            // Only DataViewId=2 is considered a match for this giving id.
            bool IsEligibleForAlertType( FinancialTransactionAlertType alertType )
            {
                return !alertType.DataViewId.HasValue || alertType.DataViewId.Value == 2;
            }

            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions, isEligibleForAlertType: IsEligibleForAlertType );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

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
            Assert.HasCount( 1, reasons );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }


        /// <summary>
        /// Tests an example missing transaction, but filtered by FinancialAccount criteria
        /// Scenario: Family typically gives monthly, but has not given in 40 days.
        /// </summary>
        [TestMethod]
        [DataRow( 66, new int[] { 2, 6 } )]
        [DataRow( 77, new int[] { 2, 7 } )]
        [DataRow( 88, new int[] { 2, 8 } )]

        [DataRow( 22, new int[] { 2 } )]

        [DataRow( 99, new int[] { 2 } )]
        public void CreateAlertsForLateTransaction_CalculatesCorrectlyBasedOnFinancialAccount( int previousTransactionsAccountId, int[] expectedAlertTypeIds )
        {
            var context = new GivingAutomation.GivingAutomationContext();

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
                new FinancialTransactionAlertType {
                    Id = 6,
                    Order = 7,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    FinancialAccountId = 66,
                },
                new FinancialTransactionAlertType {
                    Id = 7,
                    Order = 8,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    FinancialAccountId = 77
                },
                new FinancialTransactionAlertType {
                    Id = 8,
                    Order = 9,
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp,
                    FinancialAccountId = 88
                },
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            last12MonthsTransactions.ForEach( e =>
            {
                e.TransactionDetails[0].AccountId = previousTransactionsAccountId;
                if ( e.RefundDetails?.Count == 1 )
                {
                    e.RefundDetails[0].AccountId = previousTransactionsAccountId;
                }
            } );

            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );


            Assert.HasCount( expectedAlertTypeIds.Length, alerts );

            for ( int i = 0; i < expectedAlertTypeIds.Length; i++ )
            {
                var alert = alerts[i];
                var expectedAlertTypeId = expectedAlertTypeIds[i];

                Assert.AreEqual( expectedAlertTypeId, alert.AlertTypeId );
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
                Assert.HasCount( 1, reasons );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
            }
        }

        /// <summary>
        /// Tests an example missing transaction
        /// Scenario: Family typically gives monthly, but has not given in 40 days. The first alert type
        /// is below sensitivity, but the second is not.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForLateTransaction_SkipsUnmetSensitivityAlertType()
        {
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

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
            Assert.HasCount( 1, reasons );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
        }


        /// <summary>
        /// Tests an example missing transaction for various lateness for people that give consistently
        /// Sensitivity 3 normal, gets triggered more often
        /// Sensitivity 4 less aggressive than normal, should get triggered less often
        /// Sensitivity 5 even less aggressive than normal, should get triggered even less often
        /// </summary>
        [TestMethod]

        // Very consistent weekly
        [DataRow( 7, 8, 0 )]
        [DataRow( 7, 9, 0 )]
        [DataRow( 7, 10, 0 )]
        [DataRow( 7, 11, 0 )]
        [DataRow( 7, 12, 0 )]
        [DataRow( 7, 13, 0 )]
        [DataRow( 7, 14, 0 )]
        [DataRow( 7, 15, 0 )]
        [DataRow( 7, 16, 1 )] // Sensitivity 3
        [DataRow( 7, 17, 1 )]
        [DataRow( 7, 18, 1 )]
        [DataRow( 7, 19, 2 )] // Sensitivity 3 and 4
        [DataRow( 7, 20, 2 )]
        [DataRow( 7, 21, 2 )]
        [DataRow( 7, 22, 3 )] // Sensitivity 3, 4 and 5 
        [DataRow( 7, 23, 3 )]
        [DataRow( 7, 24, 3 )]
        [DataRow( 7, 25, 3 )]
        [DataRow( 7, 26, 3 )]
        [DataRow( 7, 27, 3 )]
        [DataRow( 7, 28, 3 )]
        [DataRow( 7, 29, 3 )]
        [DataRow( 7, 40, 3 )]
        [DataRow( 7, 80, 3 )]

        // Very consistent bi-weekly
        [DataRow( 14, 15, 0 )]
        [DataRow( 14, 16, 0 )]
        [DataRow( 14, 17, 0 )]
        [DataRow( 14, 18, 0 )]
        [DataRow( 14, 19, 0 )]
        [DataRow( 14, 20, 0 )]
        [DataRow( 14, 21, 0 )]
        [DataRow( 14, 22, 0 )]
        [DataRow( 14, 23, 1 )] // Sensitivity 3
        [DataRow( 14, 24, 1 )]
        [DataRow( 14, 25, 1 )]
        [DataRow( 14, 26, 2 )] // Sensitivity 3 and 4
        [DataRow( 14, 27, 2 )]
        [DataRow( 14, 28, 2 )]
        [DataRow( 14, 29, 3 )] // Sensitivity 3, 4 and 5
        [DataRow( 14, 30, 3 )]
        [DataRow( 14, 31, 3 )]
        [DataRow( 14, 32, 3 )]
        [DataRow( 14, 33, 3 )]

        // Somewhat consistent bi-weekly
        [DataRow( 17.8, 34, 0, 6.23 )]  // sometimes every 2 weeks, but sometimes every 4 weeks
        [DataRow( 17.8, 36, 0, 6.23 )]
        [DataRow( 17.8, 37, 1, 6.23 )]  // Sensitivity 3
        [DataRow( 17.8, 38, 1, 6.23 )]
        [DataRow( 17.8, 40, 1, 6.23 )]
        [DataRow( 17.8, 42, 1, 6.23 )]
        [DataRow( 17.8, 43, 2, 6.23 )]  // Sensitivity 3 and 4
        [DataRow( 17.8, 48, 2, 6.23 )]
        [DataRow( 17.8, 49, 3, 6.23 )]  // Sensitivity 3, 4 and 5
        [DataRow( 17.8, 50, 3, 6.23 )]
        [DataRow( 17.8, 60, 3, 6.23 )]
        [DataRow( 17.8, 80, 3, 6.23 )]
        [DataRow( 17.8, 90, 3, 6.23 )]

        // Consistent monthly (every 30-31 days, but average days in a month is 30.437)
        [DataRow( 30.437, 35, 0 )]
        [DataRow( 30.437, 36, 0 )]
        [DataRow( 30.437, 37, 0 )]
        [DataRow( 30.437, 38, 0 )]
        [DataRow( 30.437, 39, 0 )]
        [DataRow( 30.437, 40, 0 )]
        [DataRow( 30.437, 41, 0 )]
        [DataRow( 30.437, 42, 0 )]
        [DataRow( 30.437, 43, 0 )]
        [DataRow( 30.437, 44, 0 )]
        [DataRow( 30.437, 45, 1 )] // Sensitivity 3 
        [DataRow( 30.437, 46, 1 )]
        [DataRow( 30.437, 47, 1 )]
        [DataRow( 30.437, 48, 1 )]
        [DataRow( 30.437, 49, 2 )] // Sensitivity 3 and 4
        [DataRow( 30.437, 50, 2 )]
        [DataRow( 30.437, 51, 2 )]
        [DataRow( 30.437, 52, 2 )]
        [DataRow( 30.437, 53, 2 )]
        [DataRow( 30.437, 54, 3 )] // Sensitivity 3, 4 and 5  
        [DataRow( 30.437, 55, 3 )]
        [DataRow( 30.437, 56, 3 )]
        [DataRow( 30.437, 57, 3 )]
        public void CreateAlertsForLateTransaction_ConsistentGivers( double frequencyMean, double lastGaveDaysAgo, int alertCount, double frequencyStdDev = 0.0 )
        {
            var context = new GivingAutomation.GivingAutomationContext();

            var lateGiftAlertTypes = new List<FinancialTransactionAlertType>
            {
                new FinancialTransactionAlertType
                {
                    Id = 1,
                    Order = 1,
                    Name = "Normal Sensitivity",
                    FrequencySensitivityScale = 3,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 2,
                    Order = 2,
                    Name = "Less Sensitivity (more forgiving)",
                    FrequencySensitivityScale = 4,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
                new FinancialTransactionAlertType
                {
                    Id = 3,
                    Order = 3,
                    Name = "Even Less Sensitivity (even more forgiving)",
                    FrequencySensitivityScale = 5,
                    ContinueIfMatched = true,
                    AlertType = AlertType.FollowUp
                },
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var lastGave = context.Now.AddDays( -lastGaveDaysAgo );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, ( decimal ) frequencyMean, ( decimal ) frequencyStdDev, lastGave, givingId );
            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );
            Assert.HasCount( alertCount, alerts );
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
            var context = new GivingAutomation.GivingAutomationContext();

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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 3m;
            var lastGave = context.Now.AddDays( -40 );

            var givingId = "G1";
            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGave, givingId );
            var alerts = CreateLateAlertsForGivingId( context, lateGiftAlertTypes, givingId, last12MonthsTransactions );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 2, alerts );

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
                Assert.HasCount( 1, reasons );
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

        // $500 +/- $50. IQR range is $550 - $450
        // $1100.00 is $600 more than mean. 6x more than IQR
        [DataRow( 500.0, 100.0, 1100.00, 6.0 )]

        // Median Amount $110
        // Upper Median $120
        // Lower Median $95
        [DataRow( 110.0, 35.0, 1785.00, 47.86 )]
        public void CreateAlertsForTransaction_CreatesAlertForLargerThanUsualGift( double amountMedian, double amountIQR, double transactionAmount, double expectedAmountIqrMultiplier )
        {
            var context = new GivingAutomation.GivingAutomationContext()
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

            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = ( decimal ) transactionAmount, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( ( decimal ) amountMedian, ( decimal ) amountIQR, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );


            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 3, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 3 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );

            Assert.IsNotNull( alert.AmountIqrMultiplier );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 3 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests that Alerts are filtered by FinancialAccount as expected
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// </summary>
        [TestMethod]
        [DataRow( 66, 66, new int[] { 3, 6 } )]
        [DataRow( 77, 77, new int[] { 3, 7 } )]
        [DataRow( 88, 88, new int[] { 3, 8 } )]

        [DataRow( 22, 88, new int[] { 3 } )]
        [DataRow( 88, 22, new int[] { 3 } )]

        [DataRow( 99, 99, new int[] { 3 } )]
        public void CreateAlertsForTransaction_CalculatesCorrectlyBasedOnFinancialAccount( int newTransactionAccountId, int previousTransactionsAccountId, int[] expectedAlertTypeIds )
        {
            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    // Skip because MinimumGiftAmount
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    // Skip because Follow-up
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    // Skip because MaxDays is less than last gift
                    new FinancialTransactionAlertType {
                        Id = 33,
                        Order = 3,
                        AmountSensitivityScale = 3,
                        MaximumDaysSinceLastGift = 10,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    // Alert because AmountSensitivityScale, and no FinancialAccount specified 
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 4,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    // Skip because AmountSensitivityScale is not specified
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 5,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    // Skip because AmountSensitivityScale is not specified
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 6,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 6,
                        Order = 7,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude,
                        FinancialAccountId = 66,
                    },
                    new FinancialTransactionAlertType {
                        Id = 7,
                        Order = 8,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude,
                        FinancialAccountId = 77
                    },
                    new FinancialTransactionAlertType {
                        Id = 8,
                        Order = 9,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude,
                        FinancialAccountId = 88
                    },
                }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = newTransactionAccountId }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            last12MonthsTransactions.ForEach( e =>
             {
                 e.TransactionDetails[0].AccountId = previousTransactionsAccountId;
                 if ( e.RefundDetails?.Count == 1 )
                 {
                     e.RefundDetails[0].AccountId = previousTransactionsAccountId;
                 }
             } );

            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );

            Assert.HasCount( expectedAlertTypeIds.Length, alerts );

            for ( int i = 0; i < expectedAlertTypeIds.Length; i++ )
            {
                var alert = alerts[i];
                var expectedAlertTypeId = expectedAlertTypeIds[i];
                var alertType = context.AlertTypes.First( a => a.Id == expectedAlertTypeId );
                var expectedQuartiles = GetExpectedQuartileRangesForAlertType( alertType, last12MonthsTransactions, transaction );

                Assert.AreEqual( expectedAlertTypeId, alert.AlertTypeId );

                Assert.AreEqual( context.Now, alert.AlertDateTime );

                Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
                Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
                Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

                var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( alertType, last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
                Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
                Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

                var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
                Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
                Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

                var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
                Assert.IsNotNull( reasons );
                Assert.HasCount( 1, reasons );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
            }
        }

        /// <summary>
        /// Tests an example transaction that is large
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is larger in amount at $1000.
        /// There is an alert type that has no sensitivity settings. It should be matched because of the other filters.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertWithNoSensitivity()
        {
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );


            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 4, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 4 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 4 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
            var reason = reasons.Single();
            Assert.AreEqual( nameof( FinancialTransactionAlertType.MinimumGiftAmount ), reason );
        }

        /// <summary>
        /// Tests an example transaction that is small
        /// Scenario: Family typically gives monthly between $400 and $600. This gift is smaller in amount at $100.
        /// This also tests that two alert types can both make alerts for a single transaction.
        /// </summary>
        [TestMethod]
        public void CreateAlertsForTransaction_CreatesAlertForSmallGift()
        {
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 100M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 2, alerts );

            foreach ( var alert in alerts )
            {
                var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == alert.AlertTypeId ), last12MonthsTransactions, transaction );
                Assert.AreEqual( context.Now, alert.AlertDateTime );
                Assert.AreEqual( transaction.Id, alert.TransactionId );

                Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
                Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
                Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

                var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == alert.AlertTypeId ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
                Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
                Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

                var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
                Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
                Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

                var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
                Assert.IsNotNull( reasons );
                Assert.HasCount( 1, reasons );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
            }

            var alert1 = alerts.First();
            Assert.AreEqual( 2, alert1.AlertTypeId );

            var alert2 = alerts.Last();
            Assert.AreEqual( 6, alert2.AlertTypeId );
        }

        /// <summary>
        /// Tests an example transaction that is early/late depending on various test values
        /// </summary>
        [TestMethod]

        // range from 29-31 days
        [DataRow( 30.0, 4.0, new int[] { 4, 6 } )] // very early (26x their normal deviation)
        [DataRow( 30.0, 15.0, new int[] { 4, 6 } )] // early (15x their normal deviation), case 7 does get triggered because it is unusually early
        [DataRow( 30.0, 28.0, new int[] { 6, 7 } )] // a little early (only 2x normal deviation), but still pretty consistent
        [DataRow( 30.0, 32.0, new int[] { 6, 7 } )] // a little late (only 2x normal deviation), but still pretty consistent
        [DataRow( 30.0, 30.0, new int[] { 6, 7 } )] // consistent
        [DataRow( 30.0, 35.0, new int[] { 6, 7 } )] // kinda late, but still pretty consistent
        [DataRow( 30.0, 45.0, new int[] { 5, 7 } )] // late
        [DataRow( 30.0, 999.0, new int[] { 5, 7 } )] // really late

        // range from 5-9 days
        [DataRow( 7.0, 1.0, new int[] { 6, 7 } )] // Borderline case: "Early" (id 4) is just under the threshold for this fixture, but the negative-sensitivity rules (6/7) still trigger.
        [DataRow( 7.0, 3.0, new int[] { 6, 7 } )] // 4 days early, but they range from 5-9 days, (just 2x off their normal deviation) so pretty close to consistent
        [DataRow( 7.0, 6.0, new int[] { 6, 7 } )] // a little early, but still pretty consistent
        [DataRow( 7.0, 8.0, new int[] { 6, 7 } )] // a little late, but still pretty consistent
        [DataRow( 7.0, 7.0, new int[] { 6, 7 } )] // consistent
        [DataRow( 7.0, 11.0, new int[] { 6, 7 } )] // kinda late
        [DataRow( 7.0, 15.0, new int[] { 5, 7 } )] // late
        [DataRow( 7.0, 999.0, new int[] { 5, 7 } )] // really late
        public void CreateAlertsForTransaction_CreatesAlertForEarlyGift( double frequencyMean, double frequency, int[] expectedAlertTypeIds )
        {
            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        Name = "Large Gift over $100,000",
                        MinimumGiftAmount = 100000m,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        Name = "Smaller Than Usual Gift",
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        Name = "Larger Than Usual Gift",
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Name = "Gratitude - Alert if Early",
                        Order = 4,
                        FrequencySensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Name = "Follow up - Alert if Late",
                        Order = 5,
                        FrequencySensitivityScale = 3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 6,
                        Name = "Gratitude - (Negative) Alert if consistent or late!",
                        Order = 6,
                        FrequencySensitivityScale = -3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 7,
                        Name = "Follow up - (Negative) Alert if consistent  or late!",
                        Order = 7,
                        FrequencySensitivityScale = -3,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    }
                }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();

            // last gift was their 'normal every X days' gift which was about half that time ago
            var lastGiftDate = RockDateTime.Now.AddDays( -( frequencyMean / 2 ) );

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                // have the new gift be X days after the last normal giving date
                TransactionDateTime = lastGiftDate.AddDays( frequency ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 650M, AccountId = 123 }
                }
            };

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, ( decimal ) frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( expectedAlertTypeIds.Length, alerts );

            for ( int i = 0; i < expectedAlertTypeIds.Length; i++ )
            {
                var alert = alerts[i];
                var expectedAlertTypeId = expectedAlertTypeIds[i];
                var alertType = context.AlertTypes.Where( a => a.Id == expectedAlertTypeId ).First();

                Assert.AreEqual( expectedAlertTypeId, alert.AlertTypeId );
                Assert.AreEqual( context.Now, alert.AlertDateTime );
                Assert.AreEqual( transaction.Id, alert.TransactionId );

                var expectedQuartiles = GetExpectedQuartileRangesForAlertType( alertType, last12MonthsTransactions, transaction );
                Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
                Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
                Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

                var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( alertType, last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
                Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
                Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

                var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
                Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
                Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

                var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
                Assert.IsNotNull( reasons );
                Assert.HasCount( 1, reasons );
                Assert.AreEqual( nameof( FinancialTransactionAlertType.FrequencySensitivityScale ), reasons.Single() );
            }
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
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId2 = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId2,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );


            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MinimumMedianGiftAmount = 525.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MinimumMedianGiftAmount = 525.00m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MinimumMedianGiftAmount = 524.99m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId2 = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId2,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        MaximumMedianGiftAmount = 524.99m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        MaximumMedianGiftAmount = 525.00m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        MaximumMedianGiftAmount = 525.01m,
                        AmountSensitivityScale = 3,
                        ContinueIfMatched = false,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId3 = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId3,
                TransactionDateTime = context.Now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );


            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );


            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId = "G200";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                TransactionDateTime = context.Now.AddDays( -1 ),
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );

            // In the refactor, DataView eligibility is precomputed outside the builder and passed in.
            // Only DataViewId=2 should match this giving unit.
            var eligibleGivingIdsByDataViewId = new Dictionary<int, HashSet<string>>
            {
                [2] = new HashSet<string> { givingId }
            };

            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts, eligibleGivingIdsByDataViewId );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var recentAlerts = new List<AlertView>();
            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                TransactionDateTime = context.Now.AddDays( -1 ),
                AuthorizedPersonCampusId = 2,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var frequencyDifferenceFromMean = -1m;
            var lastGiftDate = transaction.TransactionDateTime.AddDays( ( double ) -( frequencyMean - frequencyDifferenceFromMean ) );


            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
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
            var context = new GivingAutomation.GivingAutomationContext()
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

            var amountMedian = 500m;
            var amountIqr = 100m;
            var frequencyMean = 30m;
            var frequencyStdDev = 2m;

            var now = RockDateTime.Now;
            context.Now = now;
            context.OneWeekAgo = now.AddDays( -7 );

            var recentAlerts = new List<AlertView> {
                new AlertView { AlertTypeId = 1, AlertDateTime = now.AddDays( -20 ) },
                new AlertView { AlertTypeId = 2, AlertDateTime = now.AddDays( -20 ) }
            };
            var lastGiftDate = now.AddDays( -29 );

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                TransactionDateTime = now,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = 1000M, AccountId = 123 }
                }
            };

            var last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( 1, alerts );

            var alert = alerts.Single();
            Assert.AreEqual( 2, alert.AlertTypeId );
            Assert.AreEqual( context.Now, alert.AlertDateTime );
            Assert.AreEqual( transaction.Id, alert.TransactionId );

            var expectedQuartiles = GetExpectedQuartileRangesForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedQuartiles.MedianAmount, alert.AmountCurrentMedian );
            Assert.AreEqual( expectedQuartiles.IQRAmount, alert.AmountCurrentIqr );
            Assert.AreEqual( GivingAutomationHelper.GetAmountIqrCount( expectedQuartiles, alert.Amount.Value ), alert.AmountIqrMultiplier );

            var expectedFrequencyStats = GetExpectedFrequencyStatsForAlertType( context.AlertTypes.First( a => a.Id == 2 ), last12MonthsTransactions, transaction, context.TransactionWindowDurationHours );
            Assert.AreEqual( expectedFrequencyStats.MeanDays, alert.FrequencyCurrentMean );
            Assert.AreEqual( expectedFrequencyStats.StdDevDays, alert.FrequencyCurrentStandardDeviation );

            var daysSincePrevious = GetDaysSincePreviousTransaction( last12MonthsTransactions, transaction );
            Assert.AreEqual( expectedFrequencyStats.MeanDays - daysSincePrevious, alert.FrequencyDifferenceFromMean );
            Assert.AreEqual( GivingAutomationHelper.GetFrequencyDeviationCount( expectedFrequencyStats.StdDevDays, expectedFrequencyStats.MeanDays, daysSincePrevious ), alert.FrequencyZScore );

            var reasons = alert.ReasonsKey.FromJsonOrNull<List<string>>();
            Assert.IsNotNull( reasons );
            Assert.HasCount( 1, reasons );
            Assert.AreEqual( nameof( FinancialTransactionAlertType.AmountSensitivityScale ), reasons.Single() );
        }

        /// <summary>
        /// Tests example transactions that might be $10,000 or larger
        /// when normal amount is $500.00
        /// Some scenerios are where this is their first transaction.
        /// If they have some transaction history, they might get the Larger Than Usual alert too
        /// Scenario: 
        /// </summary>
        [TestMethod]
        [DataRow( false, 9999.99, 0, 0 )] // Should create no alert
        [DataRow( false, 10000.00, 0, 3 )] // Should create 'Large Amount' alerts except for the AccountId 999 one, but not 'Larger than Usual' (they haven't given before)
        [DataRow( false, 10000.01, 0, 3 )]

        [DataRow( false, 10200.00, 201.00, 0 )] // partial refund takes it under $10000
        [DataRow( false, 10400.00, 201.00, 3 )] // partial refund but still over $10000
        [DataRow( false, 10200.00, 10200.00, 0 )] // full refund

        [DataRow( true, 9999.99, 0, 1 )] // Should create 'Larger than Usual', but not 'Large Amount' alerts
        [DataRow( true, 10000.00, 0, 4 )] // Should create 'Large Amount' alerts and 'Larger than Usual' (they normally give $500)
        [DataRow( true, 10000.01, 0, 4 )] // Should create 'Large Amount' alerts and 'Larger than Usual' (they normally give $500)

        [DataRow( true, 10200.00, 201.00, 1 )] // Should create 'Larger than Usual', but not the Large Amount alert since the amount is less than $10000 due to the partial refund
        [DataRow( true, 10400.00, 201.00, 4 )] // Should create 'Larger than Usual', and the Large Amount alert since the amount is still larger than $10000 after the partial refund
        public void CreateAlertsForTransaction_LargeAmount( bool generateTransactionHistory, double transactionAmount, double refundAmount, int expectedAlertCount )
        {
            var minimumGiftAmount = 10000.00M;
            int transactionAccountId = 123;

            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        Name = "Amount over $10,000 (No Sensitivity, any Account)",
                        AmountSensitivityScale = null,
                        FrequencySensitivityScale = null,
                        MinimumGiftAmount = minimumGiftAmount,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        Name = "Amount over $10,000 (No Sensitivity, AccountId 123)",
                        AmountSensitivityScale = null,
                        FrequencySensitivityScale = null,
                        MinimumGiftAmount = minimumGiftAmount,
                        FinancialAccountId = 123,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },

                    new FinancialTransactionAlertType {
                        Id = 3,
                        Order = 3,
                        Name = "Larger than Usual (With Sensitivity, no minimum)",
                        AmountSensitivityScale = 2,
                        FrequencySensitivityScale = 2,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 4,
                        Order = 4,
                        Name = "Amount over $10,000 (No Sensitivity, any Account) - Follow Up",
                        AmountSensitivityScale = null,
                        FrequencySensitivityScale = null,
                        MinimumGiftAmount = minimumGiftAmount,
                        ContinueIfMatched = true,
                        AlertType = AlertType.FollowUp
                    },
                    new FinancialTransactionAlertType {
                        Id = 5,
                        Order = 5,
                        Name = "Amount over $10,000 (No Sensitivity, different AccountId 999)",
                        AmountSensitivityScale = null,
                        FrequencySensitivityScale = null,
                        MinimumGiftAmount = minimumGiftAmount,
                        FinancialAccountId = 999,
                        ContinueIfMatched = true,
                        AlertType = AlertType.Gratitude
                    },
                }
            };

            var amountMedian = 500m;
            var amountIqr = 0m;
            var frequencyMean = 30m;
            var frequencyStdDev = 0m;

            var now = context.Now;
            var recentAlerts = new List<AlertView>();
            var lastGiftDate = now.AddDays( 0 - ( int ) frequencyMean );

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = ( decimal ) transactionAmount, AccountId = transactionAccountId }
                }
            };

            if ( refundAmount > 0.00 )
            {
                transaction.RefundDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = -( decimal ) refundAmount, AccountId = transactionAccountId }
                };
            }

            List<FinancialTransactionView> last12MonthsTransactions;

            if ( generateTransactionHistory )
            {
                last12MonthsTransactions = GenerateTestTransactions( amountMedian, amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            }
            else
            {
                last12MonthsTransactions = new List<FinancialTransactionView>();
            }

            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );


            Assert.HasCount( expectedAlertCount, alerts );
        }

        /// <summary>
        /// Tests various scenerios for a transaction that might be 'larger than usual'.
        /// Scenario: Family always gives monthly *exactly* same amount. Make sure alert isn't generated
        /// in cases where a family usually gives a consistent amount, and then gives a slightly larger amount.
        ///
        /// In cases of consistent amount, a small increase would technically be infinite on the sensitivity scale
        /// because ($1 / 0 => infinite). The logic should have a have a minimum deviation for those cases.
        /// </summary>
        [TestMethod]
        [DataRow( 0, 500, 510, 0 )]  // these larger amounts aren't really large enough to be considered for a Sensitivity 3
        [DataRow( 0, 500, 550, 0 )]
        [DataRow( 0, 500, 600, 0 )]
        [DataRow( 0, 500, 650, 0 )]
        [DataRow( 0, 500, 700, 0 )]
        [DataRow( 0, 500, 750, 2 )]  // Sensitivity 3 alert gets triggered, 50% larger than usual 
        [DataRow( 0, 500, 1000, 3 )] // Sensitivity 3 and 4 alerts get triggered, twice as larger 

        [DataRow( 0, 100, 110, 0 )]  // these larger amounts aren't really large enough to be considered for a Sensitivity 3
        [DataRow( 0, 100, 144, 0 )]
        [DataRow( 0, 100, 145, 2 )]  // Sensitivity 3 alert gets triggered
        [DataRow( 0, 100, 146, 2 )]
        [DataRow( 0, 100, 150, 2 )]
        [DataRow( 0, 100, 175, 3 )]  // Sensitivity 3 and 4 alert gets triggered
        [DataRow( 0, 100, 200, 3 )]

        [DataRow( 0, 20, 22, 0 )]
        [DataRow( 0, 20, 25, 0 )]
        [DataRow( 0, 20, 26, 0 )]
        [DataRow( 0, 20, 27, 0 )]
        [DataRow( 0, 20, 28, 0 )]
        [DataRow( 0, 20, 29, 2 )]
        [DataRow( 0, 20, 30, 2 )]
        [DataRow( 0, 20, 31, 2 )]
        [DataRow( 0, 20, 40, 3 )]
        [DataRow( 0, 20, 59, 3 )]
        [DataRow( 0, 20, 61, 3 )]

        [DataRow( 0, 5.00, 5.10, 0 )]
        [DataRow( 0, 5.00, 5.50, 0 )]
        [DataRow( 0, 5.00, 6.00, 0 )]
        [DataRow( 0, 5.00, 7.00, 0 )]
        [DataRow( 0, 5.00, 7.50, 1 )]
        [DataRow( 0, 5.00, 7.51, 1 )]
        [DataRow( 0, 5.00, 8.00, 1 )]

        public void CreateAlertsForTransaction_LargerAmountThanUsual( double amountIqr, double amountMedian, double transactionAmount, int expectedAlertCount )
        {
            var context = new GivingAutomation.GivingAutomationContext()
            {
                AlertTypes = new List<FinancialTransactionAlertType> {
                    new FinancialTransactionAlertType {
                        Id = 1,
                        Order = 1,
                        Name = "Minimum $20, Sensitivity 3",
                        AmountSensitivityScale = 3,
                        FrequencySensitivityScale = null,
                        ContinueIfMatched = true,
                        MinimumGiftAmount = 20.0M,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Order = 2,
                        Name = "Minimum $20, Sensitivity 4",
                        AmountSensitivityScale = 4,
                        FrequencySensitivityScale = null,
                        ContinueIfMatched = true,
                        MinimumGiftAmount = 20.0M,
                        AlertType = AlertType.Gratitude
                    },
                    new FinancialTransactionAlertType {
                        Id = 2,
                        Name = "No Minimum",
                        Order = 2,
                        AmountSensitivityScale = 3,
                        FrequencySensitivityScale = null,
                        ContinueIfMatched = true,
                        MinimumGiftAmount = 0.0M,
                        AlertType = AlertType.Gratitude
                    }
                }
            };

            var frequencyMean = 30m;
            var frequencyStdDev = 0m;

            var now = context.Now;
            var recentAlerts = new List<AlertView>();
            var lastGiftDate = now.AddDays( 0 - ( int ) frequencyMean );

            var givingId = "G1";
            var transaction = new FinancialTransactionView
            {
                Id = 888,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = now.AddDays( -1 ),
                TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { Amount = ( decimal ) transactionAmount, AccountId = 123 }
                }
            };

            var last12MonthsTransactions = GenerateTestTransactions( ( decimal ) amountMedian, ( decimal ) amountIqr, frequencyMean, frequencyStdDev, lastGiftDate );
            var alerts = CreateRecentTxnAlertsForTransaction( context, transaction, last12MonthsTransactions, recentAlerts );

            Assert.IsNotNull( alerts );
            Assert.HasCount( expectedAlertCount, alerts );
        }

        #endregion CreateAlertsForTransaction

        #region UpdateGivingUnitClassifications

        /// <summary>
        /// Tests an example giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesMonthlyCorrectly()
        {
            var firstCurrencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH );
            var secondCurrencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD );

            var firstSourceTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE );
            var secondSourceTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION );

            var context = new GivingAutomation.GivingAutomationContext();
            var givingId = "G800";

            var transactions = new List<FinancialTransactionView>
            {
                new FinancialTransactionView
                {
                    Id = 1,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 1, 28 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1300.00M } },
                    RefundDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = -550.00M } },
                },
                new FinancialTransactionView
                {
                    Id = 2,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 2, 11 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1150.00M } },
                },
                new FinancialTransactionView
                {
                    Id = 3,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 2, 26 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = false,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 500.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 4,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 3, 11 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 5,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 4, 11 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 6,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 5, 11 ),
                    CurrencyTypeValueId = firstCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 7,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 6, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 8,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 7, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = firstSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 9,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 8, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = secondSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 10,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 9, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = secondSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 11,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 10, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = secondSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                },
                new FinancialTransactionView
                {
                    Id = 12,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = new DateTime( 2020, 11, 11 ),
                    CurrencyTypeValueId = secondCurrencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = secondSourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 1200.0000000000m } }
                }
            };

            // Preferred Currency / Source
            Assert.AreEqual( secondCurrencyTypeValue.Guid, GivingAutomationHelper.GetPreferredCurrencyGuid( transactions ) );
            Assert.AreEqual( firstSourceTypeValue.Guid, GivingAutomationHelper.GetPreferredSourceGuid( transactions ) );

            // Percent scheduled
            Assert.AreEqual( 92, GivingAutomationHelper.GetPercentScheduled( transactions ) );

            // Amount stats
            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactions.Select( t => t.TotalAmount ) );
            Assert.AreEqual( 1200.00m, decimal.Round( quartileRanges.MedianAmount, 2 ) );
            Assert.AreEqual( 50.00m, decimal.Round( quartileRanges.IQRAmount, 2 ) );

            // Frequency stats / label
            var orderedDateTimes = transactions.Select( t => t.TransactionDateTime ).OrderBy( d => d ).ToList();
            var frequencyStats = GivingAutomationHelper.GetFrequencyStats( orderedDateTimes, context.TransactionWindowDurationHours );
            Assert.AreEqual( 3, ( int ) frequencyStats.FrequencyLabel );
            Assert.AreEqual( 26.18m, decimal.Round( frequencyStats.MeanDays, 2 ) );
            Assert.AreEqual( 7.27m, decimal.Round( frequencyStats.StdDevDays, 2 ) );

            // Last gave / next expected
            var lastGave = transactions.Max( t => t.TransactionDateTime );
            Assert.AreEqual( new DateTime( 2020, 11, 11 ), lastGave );
            Assert.AreEqual( lastGave.AddDays( ( double ) frequencyStats.MeanDays ), frequencyStats.NextExpectedGiftDate );
        }

        /// <summary>
        /// Tests an example a giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesWeeklyCorrectly()
        {
            var currencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH );
            var sourceTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE );

            var context = new GivingAutomation.GivingAutomationContext()
            {
                Now = new DateTime( 2020, 12, 1 )
            };
            var givingId = "G900";
            var transactions = new List<FinancialTransactionView>();
            int id = 1;

            var date = new DateTime( 2020, 1, 4 );
            var endDate = new DateTime( 2020, 11, 14 );
            while ( date <= endDate )
            {
                transactions.Add( new FinancialTransactionView
                {
                    Id = id++,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = date,
                    CurrencyTypeValueId = currencyTypeValue.Id,
                    IsScheduled = true,
                    SourceTypeValueId = sourceTypeValue.Id,
                    TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 500.0000000000m } },
                } );

                date = date.AddDays( 7 );
            }

            // One-off non-scheduled gift.
            transactions.Add( new FinancialTransactionView
            {
                Id = id++,
                AuthorizedPersonAliasId = 1,
                AuthorizedPersonGivingId = givingId,
                TransactionDateTime = new DateTime( 2020, 9, 13 ),
                CurrencyTypeValueId = currencyTypeValue.Id,
                IsScheduled = false,
                SourceTypeValueId = sourceTypeValue.Id,
                TransactionDetails = new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = 100000.0000000000m } },
            } );

            // Preferred Currency / Source
            Assert.AreEqual( currencyTypeValue.Guid, GivingAutomationHelper.GetPreferredCurrencyGuid( transactions ) );
            Assert.AreEqual( sourceTypeValue.Guid, GivingAutomationHelper.GetPreferredSourceGuid( transactions ) );

            // Percent scheduled
            Assert.AreEqual( 98, GivingAutomationHelper.GetPercentScheduled( transactions ) );

            // Amount stats
            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactions.Select( t => t.TotalAmount ) );
            Assert.AreEqual( 500.00m, decimal.Round( quartileRanges.MedianAmount, 2 ) );
            Assert.AreEqual( 0.00m, decimal.Round( quartileRanges.IQRAmount, 2 ) );

            // Frequency stats / label
            var orderedDateTimes = transactions.Select( t => t.TransactionDateTime ).OrderBy( d => d ).ToList();
            var frequencyStats = GivingAutomationHelper.GetFrequencyStats( orderedDateTimes, context.TransactionWindowDurationHours );
            Assert.AreEqual( 1, ( int ) frequencyStats.FrequencyLabel );
            Assert.AreEqual( 6.85m, decimal.Round( frequencyStats.MeanDays, 2 ) );
            Assert.AreEqual( 0.88m, decimal.Round( frequencyStats.StdDevDays, 2 ) );

            // Last gave / next expected
            var lastGave = transactions.Max( t => t.TransactionDateTime );
            Assert.AreEqual( new DateTime( 2020, 11, 14 ), lastGave );
            Assert.AreEqual( lastGave.AddDays( ( double ) frequencyStats.MeanDays ), frequencyStats.NextExpectedGiftDate );
        }



        /// <summary>
        /// Tests an example a giving family
        /// </summary>
        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesErraticCorrectly()
        {
            // The old `UpdateGivingUnitClassifications` pipeline was removed/refactored and bin/percentile
            // values are now handled by stored procedures. This test now asserts directly on the
            // helper computations using the same underlying transaction history.

            var personId = 1111;
            var givingId = $"P{personId}";

            var currencyTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH );
            var sourceTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE );

            var transactions = new List<FinancialTransactionView>();
            int id = 1;

            void AddTransaction( DateTime date, decimal amount, decimal refundAmount = 0m )
            {
                transactions.Add( new FinancialTransactionView
                {
                    Id = id++,
                    AuthorizedPersonAliasId = personId,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = date,
                    CurrencyTypeValueId = currencyTypeValue.Id,
                    SourceTypeValueId = sourceTypeValue.Id,
                    IsScheduled = false,
                    TransactionDetails = new List<FinancialTransactionDetailView>
                    {
                        new FinancialTransactionDetailView { AccountId = 123, Amount = amount }
                    },
                    RefundDetails = refundAmount != 0m
                        ? new List<FinancialTransactionDetailView> { new FinancialTransactionDetailView { AccountId = 123, Amount = refundAmount } }
                        : null
                } );
            }

            AddTransaction( new DateTime( 2020, 1, 3 ), 50.0000000000m, -10.0000000000m );
            AddTransaction( new DateTime( 2020, 1, 16 ), 180.0000000000m );
            AddTransaction( new DateTime( 2020, 1, 18 ), 82.0000000000m );
            AddTransaction( new DateTime( 2020, 1, 24 ), 45.0000000000m );
            AddTransaction( new DateTime( 2020, 1, 31 ), 155.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 8 ), 85.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 15 ), 140.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 15 ), 30.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 22 ), 115.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 29 ), 150.0000000000m );
            AddTransaction( new DateTime( 2020, 2, 29 ), 82.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 6 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 13 ), 66.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 13 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 13 ), 150.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 20 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 20 ), 97.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 28 ), 90.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 28 ), 140.0000000000m );
            AddTransaction( new DateTime( 2020, 3, 28 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 3 ), 63.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 3 ), 17.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 10 ), 81.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 10 ), 19.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 15 ), 120.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 15 ), 120.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 17 ), 12.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 17 ), 98.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 24 ), 112.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 24 ), 18.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 24 ), 150.0000000000m );
            AddTransaction( new DateTime( 2020, 4, 24 ), 200.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 1 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 1 ), 110.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 7 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 7 ), 25.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 8 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 8 ), 17.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 14 ), 25.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 16 ), 120.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 16 ), 20.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 19 ), 35.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 21 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 21 ), 135.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 22 ), 12.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 22 ), 108.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 29 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 5, 29 ), 110.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 3 ), 75.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 4 ), 140.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 8 ), 22.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 8 ), 98.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 13 ), 116.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 13 ), 14.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 16 ), 20.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 18 ), 140.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 19 ), 18.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 19 ), 132.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 25 ), 80.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 26 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 6, 26 ), 105.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 3 ), 100.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 4 ), 107.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 4 ), 13.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 5 ), 35.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 10 ), 85.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 12 ), 100.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 12 ), 20.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 17 ), 135.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 20 ), 110.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 24 ), 70.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 24 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 25 ), 50.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 25 ), 80.0000000000m );
            AddTransaction( new DateTime( 2020, 7, 30 ), 150.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 1 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 1 ), 75.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 8 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 8 ), 80.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 13 ), 120.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 13 ), 75.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 13 ), 13.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 13 ), 77.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 14 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 28 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 28 ), 73.0000000000m );
            AddTransaction( new DateTime( 2020, 8, 28 ), 17.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 4 ), 18.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 4 ), 72.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 10 ), 130.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 13 ), 96.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 13 ), 14.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 18 ), 66.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 24 ), 175.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 26 ), 20.0000000000m );
            AddTransaction( new DateTime( 2020, 9, 26 ), 110.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 3 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 3 ), 95.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 9 ), 125.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 9 ), 136.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 9 ), 14.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 16 ), 95.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 16 ), 82.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 16 ), 18.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 22 ), 125.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 23 ), 12.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 23 ), 70.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 30 ), 150.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 30 ), 110.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 30 ), 70.0000000000m );
            AddTransaction( new DateTime( 2020, 10, 30 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 6 ), 190.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 6 ), 10.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 9 ), 175.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 13 ), 85.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 13 ), 15.0000000000m );
            AddTransaction( new DateTime( 2020, 11, 14 ), 140.0000000000m );

            // Preferred Currency / Source
            Assert.AreEqual( currencyTypeValue.Guid, GivingAutomationHelper.GetPreferredCurrencyGuid( transactions ) );
            Assert.AreEqual( sourceTypeValue.Guid, GivingAutomationHelper.GetPreferredSourceGuid( transactions ) );

            // Percent scheduled
            Assert.AreEqual( 0, GivingAutomationHelper.GetPercentScheduled( transactions ) );

            // Amount stats
            var quartileRanges = GivingAutomationHelper.GetQuartileRanges( transactions.Select( t => t.TotalAmount ) );
            Assert.AreEqual( 81.00m, decimal.Round( quartileRanges.MedianAmount, 2 ) );
            Assert.AreEqual( 101.50m, decimal.Round( quartileRanges.IQRAmount, 2 ) );

            // Frequency stats / label
            var orderedDateTimes = transactions.Select( t => t.TransactionDateTime ).OrderBy( d => d ).ToList();
            var frequencyStats = GivingAutomationHelper.GetFrequencyStats( orderedDateTimes, transactionWindowDurationHours: 0 );
            Assert.AreEqual( 5, ( int ) frequencyStats.FrequencyLabel );
            Assert.AreEqual( 2.72m, decimal.Round( frequencyStats.MeanDays, 2 ) );
            Assert.AreEqual( 3.16m, decimal.Round( frequencyStats.StdDevDays, 2 ) );

            // Last gave / next expected
            var lastGave = transactions.Max( t => t.TransactionDateTime );
            Assert.AreEqual( new DateTime( 2020, 11, 14 ), lastGave );
            Assert.AreEqual( lastGave.AddDays( ( double ) frequencyStats.MeanDays ), frequencyStats.NextExpectedGiftDate );
        }

        [TestMethod]
        public void UpdateGivingUnitClassifications_ClassifiesTransactionsWithinWindowAsConsistent()
        {
            const int frequencyDefinedValueMonthlyId = 3;
            const int frequencyDefinedValueErraticId = 5;

            // Create a set of giving transactions spanning a year:
            // 1. Monthly on the 10th @ 20:00; and
            // 2. Monthly on the 11th @ 08:00.
            var currentDate = RockDateTime.Now;
            var gift1StartDate = RockDateTime.New( currentDate.AddMonths( -12 ).Year, currentDate.AddMonths( -1 ).Month, 10 ).Value.AddHours( 20 );
            var gift1Dates = GetDateTimeListWithMonthlyPattern( gift1StartDate, 12 );

            var gift2StartDate = gift1StartDate.AddHours( 12 );
            var gift2Dates = GetDateTimeListWithMonthlyPattern( gift2StartDate, 12 );

            var giftDates = new List<DateTime>();
            giftDates.AddRange( gift1Dates );
            giftDates.AddRange( gift2Dates );
            giftDates.Sort();

            // With no transaction window, the twice-monthly pattern is treated as Erratic.
            var statsNoWindow = GivingAutomationHelper.GetFrequencyStats( giftDates, transactionWindowDurationHours: 0 );
            Assert.AreEqual( frequencyDefinedValueErraticId, ( int ) statsNoWindow.FrequencyLabel );

            // With a 24-hour window, the paired gifts are treated as one (Monthly).
            var statsWithWindow = GivingAutomationHelper.GetFrequencyStats( giftDates, transactionWindowDurationHours: 24 );
            Assert.AreEqual( frequencyDefinedValueMonthlyId, ( int ) statsWithWindow.FrequencyLabel );
        }

        private List<DateTime> GetDateTimeListWithMonthlyPattern( DateTime startDateTime, int count )
        {
            var transactionDates = new List<DateTime>();
            for ( int month = 0; month < count; month++ )
            {
                var addDate = startDateTime.AddMonths( month );
                transactionDates.Add( addDate );
            }
            return transactionDates;
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

        private List<FinancialTransactionView> GenerateTestTransactions( decimal amountMedian, decimal amountIqr, decimal frequencyMean, decimal frequencyStdDev, DateTime lastGave, string givingId = "G1" )
        {
            var last12MonthsTransactions = new List<FinancialTransactionView>();

            // To similate the std dev, add/substract a little from every other day. This doesn't create outliers,
            // So this ends up working OK.
            var daysPlusMinus = ( double ) ( frequencyStdDev / 2.0M );

            var transactionDateTime = lastGave.AddDays( -daysPlusMinus );

            var oneYearAgo = RockDateTime.Now.AddYears( -1 );

            while ( transactionDateTime > oneYearAgo )
            {
                var transactionView = new FinancialTransactionView
                {
                    Id = last12MonthsTransactions.Count + 1,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = transactionDateTime
                };
                last12MonthsTransactions.Add( transactionView );
                transactionDateTime = transactionDateTime.AddDays( -( double ) frequencyMean );
            }

            // if there are an even number, add one more to make the reverse stddev math easier
            if ( last12MonthsTransactions.Count % 2 == 0 )
            {
                var transactionView = new FinancialTransactionView
                {
                    Id = last12MonthsTransactions.Count + 1,
                    AuthorizedPersonAliasId = 1,
                    AuthorizedPersonGivingId = givingId,
                    TransactionDateTime = transactionDateTime
                };
                last12MonthsTransactions.Add( transactionView );
            }

            var transactionCount = last12MonthsTransactions.Count;
            var middleTransactionPosition = transactionCount / 2.00;
            var currentPosition = 0;
            var refundAmount = 0.00m;
            bool didARefund = false;

            foreach ( var transactionView in last12MonthsTransactions.OrderByDescending( a => a.TransactionDateTime ) )
            {
                decimal testAmount;
                currentPosition++;
                transactionView.TransactionDateTime = transactionView.TransactionDateTime.AddDays( daysPlusMinus );
                daysPlusMinus = -daysPlusMinus;

                if ( currentPosition < middleTransactionPosition )
                {
                    testAmount = amountMedian - ( amountIqr / 2.0M );
                }
                else if ( Math.Abs( currentPosition - middleTransactionPosition ) < 1 )
                {
                    testAmount = amountMedian;
                }
                else
                {
                    testAmount = amountMedian + ( amountIqr / 2.0M );
                }

                // Partial refunds are rare, but let's throw one into our test transactions to help detect problems with the partial refund logic
                if ( !didARefund )
                {
                    refundAmount = Math.Round( testAmount * 0.25M, 2 );
                    didARefund = true;
                }
                else
                {
                    refundAmount = 0.00M;
                }

                transactionView.TransactionDetails = new List<FinancialTransactionDetailView>
                {
                    new FinancialTransactionDetailView { AccountId = 123, Amount = testAmount + refundAmount },
                };

                if ( refundAmount != 0.00M )
                {
                    transactionView.RefundDetails = new List<FinancialTransactionDetailView>
                    {
                        new FinancialTransactionDetailView { AccountId = 123, Amount = -refundAmount }
                    };
                }
            }

            return last12MonthsTransactions;
        }

        private static QuartileRanges GetExpectedQuartileRangesForAlertType(
            FinancialTransactionAlertType alertType,
            List<FinancialTransactionView> last12MonthsTransactions,
            FinancialTransactionView transaction )
        {
            var orderedTransactions = new List<FinancialTransactionView>();
            if ( last12MonthsTransactions?.Any() == true )
            {
                orderedTransactions.AddRange( last12MonthsTransactions );
            }

            if ( transaction != null )
            {
                orderedTransactions.Add( transaction );
            }

            IEnumerable<decimal> amounts;

            if ( alertType?.FinancialAccountId != null )
            {
                var accountId = alertType.FinancialAccountId.Value;
                amounts = orderedTransactions
                    .SelectMany( t => t.GetTransactionDetails() )
                    .Where( d => d.AccountId == accountId )
                    .Select( d => d.Amount );
            }
            else
            {
                amounts = orderedTransactions
                    .SelectMany( t => t.GetTransactionDetails() )
                    .Select( d => d.Amount );
            }

            return GivingAutomationHelper.GetQuartileRanges( amounts );
        }

        private static FrequencyCalculationResult GetExpectedFrequencyStatsForAlertType(
            FinancialTransactionAlertType alertType,
            List<FinancialTransactionView> last12MonthsTransactions,
            FinancialTransactionView transaction,
            int transactionWindowDurationHours )
        {
            var orderedTransactions = new List<FinancialTransactionView>();
            if ( last12MonthsTransactions?.Any() == true )
            {
                orderedTransactions.AddRange( last12MonthsTransactions );
            }

            if ( transaction != null )
            {
                orderedTransactions.Add( transaction );
            }

            List<DateTime> dateTimes;

            if ( alertType?.FinancialAccountId != null )
            {
                var accountId = alertType.FinancialAccountId.Value;
                dateTimes = orderedTransactions
                    .Where( t => t.GetTransactionDetails().Any( d => d.AccountId == accountId ) )
                    .Select( t => t.TransactionDateTime )
                    .OrderBy( d => d )
                    .ToList();
            }
            else
            {
                dateTimes = orderedTransactions
                    .Select( t => t.TransactionDateTime )
                    .OrderBy( d => d )
                    .ToList();
            }

            return GivingAutomationHelper.GetFrequencyStats( dateTimes, transactionWindowDurationHours );
        }

        private static decimal GetDaysSincePreviousTransaction( List<FinancialTransactionView> last12MonthsTransactions, FinancialTransactionView transaction )
        {
            if ( transaction == null )
            {
                return 0m;
            }

            var orderedTransactions = new List<FinancialTransactionView>();
            if ( last12MonthsTransactions?.Any() == true )
            {
                orderedTransactions.AddRange( last12MonthsTransactions );
            }

            orderedTransactions.Add( transaction );
            orderedTransactions = orderedTransactions.OrderBy( t => t.TransactionDateTime ).ToList();

            var index = orderedTransactions.FindIndex( t => t.Id == transaction.Id );
            if ( index <= 0 )
            {
                return 0m;
            }

            var previousDate = orderedTransactions[index - 1].TransactionDateTime;
            return Convert.ToDecimal( ( transaction.TransactionDateTime - previousDate ).TotalDays );
        }

        private static List<FinancialTransactionAlert> CreateLateAlertsForGivingId(
            GivingAutomation.GivingAutomationContext context,
            List<FinancialTransactionAlertType> lateGiftAlertTypes,
            string givingId,
            List<FinancialTransactionView> allTransactions,
            Dictionary<int, List<AlertView>> recentAlertsOfThisTypeByAlertTypeId = null,
            Func<FinancialTransactionAlertType, bool> isEligibleForAlertType = null )
        {
            var alerts = new List<FinancialTransactionAlert>();
            var givingIdsToExcludeFromSubsequentAlerts = new HashSet<string>();

            foreach ( var lateGiftAlertType in lateGiftAlertTypes.OrderBy( a => a.Order ) )
            {
                if ( isEligibleForAlertType != null && !isEligibleForAlertType( lateGiftAlertType ) )
                {
                    continue;
                }

                // DataView and FinancialAccount filtering happens before the builder is invoked.
                // Simulate that here by pre-filtering the transaction list passed to the builder.
                var orderedTransactionsForType = allTransactions;
                if ( lateGiftAlertType.FinancialAccountId.HasValue )
                {
                    var accountId = lateGiftAlertType.FinancialAccountId.Value;
                    orderedTransactionsForType = allTransactions
                        .Where( t => t.GetTransactionDetails().Any( d => d.AccountId == accountId ) )
                        .ToList();
                }

                if ( orderedTransactionsForType == null || orderedTransactionsForType.Count == 0 )
                {
                    continue;
                }

                orderedTransactionsForType = orderedTransactionsForType.OrderBy( t => t.TransactionDateTime ).ToList();

                List<AlertView> recentAlertsOfThisType = null;
                if ( recentAlertsOfThisTypeByAlertTypeId != null )
                {
                    recentAlertsOfThisTypeByAlertTypeId.TryGetValue( lateGiftAlertType.Id, out recentAlertsOfThisType );
                }

                var builder = new GivingAutomation.LateTxnAlertBuilder( context, lateGiftAlertType, givingIdsToExcludeFromSubsequentAlerts );
                var alert = builder.BuildAlert( givingId, orderedTransactionsForType, recentAlertsOfThisType );
                if ( alert == null )
                {
                    continue;
                }

                alerts.Add( alert );

                if ( !lateGiftAlertType.ContinueIfMatched && alert.GivingId.IsNotNullOrWhiteSpace() )
                {
                    givingIdsToExcludeFromSubsequentAlerts.Add( alert.GivingId );
                }
            }

            return alerts;
        }

        private static List<FinancialTransactionAlert> CreateRecentTxnAlertsForTransaction(
            GivingAutomation.GivingAutomationContext context,
            FinancialTransactionView transaction,
            List<FinancialTransactionView> twelveMonthsTransactions,
            List<AlertView> recentAlerts,
            Dictionary<int, HashSet<string>> eligibleGivingIdsByDataViewId = null,
            bool allowFollowUp = true,
            bool allowGratitude = true )
        {
            if ( context == null || transaction == null )
            {
                return new List<FinancialTransactionAlert>();
            }

            var givingId = transaction.AuthorizedPersonGivingId;

            var orderedTransactions = new List<FinancialTransactionView>();
            if ( twelveMonthsTransactions?.Any() == true )
            {
                orderedTransactions.AddRange( twelveMonthsTransactions );
            }

            orderedTransactions.Add( transaction );
            orderedTransactions = orderedTransactions
                .OrderBy( t => t.TransactionDateTime )
                .ToList();

            var computedMetricsByAlertTypeId = GivingAutomationHelper.ComputeMetricsForAlertTypes(
                context.AlertTypes,
                orderedTransactions,
                context.TransactionWindowDurationHours );

            var builder = new GivingAutomation.RecentTxnAlertBuilder(
                context,
                givingId,
                orderedTransactions,
                recentAlerts ?? new List<AlertView>(),
                eligibleGivingIdsByDataViewId,
                computedMetricsByAlertTypeId );

            return builder.BuildAlertsForTransaction( transaction, allowFollowUp, allowGratitude ) ?? new List<FinancialTransactionAlert>();
        }

        #endregion Helpers
    }
}
