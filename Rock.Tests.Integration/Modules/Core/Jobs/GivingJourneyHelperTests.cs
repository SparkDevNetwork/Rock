using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Financial;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Settings.Giving;

namespace Rock.Tests.Integration.Modules.Core.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class GivingJourneyHelperTests : DatabaseTestsBase
    {
        /// <summary>
        /// </summary>
        /// <param name="expectedGivingJourneyStage">The expected giving journey stage.</param>
        /// <param name="daysAgoList">The days ago list.</param>
        [TestMethod]
        [DataRow( GivingJourneyStage.Former, 376, 383, 390, 397 )]
        [DataRow( GivingJourneyStage.Former, 390, 420, 450 )]
        [DataRow( GivingJourneyStage.Former, 400, 414, 428 )]

        [DataRow( GivingJourneyStage.Lapsed, 151, 200, 260 )]
        [DataRow( GivingJourneyStage.Lapsed, 161, 170, 190 )]

        [DataRow( GivingJourneyStage.New, 7 )]
        [DataRow( GivingJourneyStage.New, 149, 14, 1 )]

        // gives consistently, but first transaction was less than 150 days ago
        [DataRow( GivingJourneyStage.New, 7, 14, 21, 28 )]

        [DataRow( GivingJourneyStage.Occasional, 40, 120, 190 )]
        [DataRow( GivingJourneyStage.Occasional, 40, 120, ( 120 + 93 ) )]

        // Started recently, but gave more than 5 times, and has been giving consistently
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42 )]

        // Has been giving consistently starting more than 150 days ago, and gave three times, and hasn't lapsed more than 149 days
        [DataRow( GivingJourneyStage.Consistent, 151, 144, 137 )]

        // Has been giving consistently every 2 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 14, 28, 42, 56, 70, 84, 98, 112, 126, 140, 154, 168, 182, 196, 210, 224, 238, 252, 266, 280, 294, 308, 322, 336, 350, 364, 378, 392, 406, 420, 434, 448, 462 )]

        // Has been giving consistently every 1 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196, 203, 210, 217, 224, 231, 238, 245, 252, 259, 266, 273, 280, 287, 294, 301, 308, 315, 322, 329, 336, 343, 350, 357, 364, 371, 378, 385, 392, 399, 406 )]

        // Has been giving, but doesn't meet any of the Giving Journey criteria
        [DataRow( null, 40, 200, 207, 500 )]

        public void GivingJourneyStage_CalculatesGivingJourneyStageCorrectly( GivingJourneyStage? expectedGivingJourneyStage, params int[] daysAgoList )
        {
            GivingJourneySettings givingJourneySettings = new GivingJourneySettings();
            givingJourneySettings.FormerGiverNoContributionInTheLastDays = 375;
            givingJourneySettings.FormerGiverMedianFrequencyLessThanDays = 320;

            givingJourneySettings.LapsedGiverNoContributionInTheLastDays = 150;
            givingJourneySettings.LapsedGiverMedianFrequencyLessThanDays = 100;

            givingJourneySettings.NewGiverContributionCountBetweenMinimum = 1;
            givingJourneySettings.NewGiverContributionCountBetweenMaximum = 5;
            givingJourneySettings.NewGiverFirstGiftInTheLastDays = 150;

            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum = 33;
            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum = 94;

            givingJourneySettings.ConsistentGiverMedianLessThanDays = 32;

            var currentDate = RockDateTime.Now;

            var transactionDates = new List<DateTime>();
            foreach ( var daysAgo in daysAgoList )
            {
                transactionDates.Add( currentDate.AddDays( -daysAgo ) );
            }

            var givingJourneyStage = GivingJourneyHelper.GetGivingJourneyStage( givingJourneySettings, currentDate, transactionDates );

            Assert.AreEqual( expectedGivingJourneyStage, givingJourneyStage );
        }

        /// <summary>
        /// </summary>
        /// <param name="unExpectedGivingJourneyStage">The unexpected giving journey stage.</param>
        /// <param name="daysAgoList">The days ago list.</param>
        [TestMethod]
        [DataRow( GivingJourneyStage.Former, 376, 383, 390, 397 )]
        [DataRow( GivingJourneyStage.Former, 390, 420, 450 )]
        [DataRow( GivingJourneyStage.Former, 400, 414, 428 )]

        [DataRow( GivingJourneyStage.Lapsed, 151, 200, 260 )]
        [DataRow( GivingJourneyStage.Lapsed, 161, 170, 190 )]

        [DataRow( GivingJourneyStage.New, 7 )]
        [DataRow( GivingJourneyStage.New, 149, 14, 1 )]

        // gives consistently, but first transaction was less than 150 days ago
        [DataRow( GivingJourneyStage.New, 7, 14, 21, 28 )]

        [DataRow( GivingJourneyStage.Occasional, 40, 120, 190 )]
        [DataRow( GivingJourneyStage.Occasional, 40, 120, ( 120 + 93 ) )]

        // Started recently, but gave more than 5 times, and has been giving consistently
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42 )]

        // Has been giving consistently starting more than 150 days ago, and gave three times, and hasn't lapsed more than 149 days
        [DataRow( GivingJourneyStage.Consistent, 151, 144, 137 )]

        // Has been giving consistently every 2 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 14, 28, 42, 56, 70, 84, 98, 112, 126, 140, 154, 168, 182, 196, 210, 224, 238, 252, 266, 280, 294, 308, 322, 336, 350, 364, 378, 392, 406, 420, 434, 448, 462 )]

        // Has been giving consistently every 1 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196, 203, 210, 217, 224, 231, 238, 245, 252, 259, 266, 273, 280, 287, 294, 301, 308, 315, 322, 329, 336, 343, 350, 357, 364, 371, 378, 385, 392, 399, 406 )]
        public void GivingJourneyStage_CalculatesGivingJourneyStageCorrectlyWithNullSettings( GivingJourneyStage? unExpectedGivingJourneyStage, params int[] daysAgoList )
        {
            GivingJourneySettings givingJourneySettings = new GivingJourneySettings();
            givingJourneySettings.FormerGiverNoContributionInTheLastDays = null;
            givingJourneySettings.FormerGiverMedianFrequencyLessThanDays = null;

            givingJourneySettings.LapsedGiverNoContributionInTheLastDays = null;
            givingJourneySettings.LapsedGiverMedianFrequencyLessThanDays = null;

            givingJourneySettings.NewGiverContributionCountBetweenMinimum = null;
            givingJourneySettings.NewGiverContributionCountBetweenMaximum = null;
            givingJourneySettings.NewGiverFirstGiftInTheLastDays = null;

            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum = null;
            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum = null;

            givingJourneySettings.ConsistentGiverMedianLessThanDays = null;

            var currentDate = RockDateTime.Now;

            var transactionDates = new List<DateTime>();
            foreach ( var daysAgo in daysAgoList )
            {
                transactionDates.Add( currentDate.AddDays( -daysAgo ) );
            }

            var givingJourneyStage = GivingJourneyHelper.GetGivingJourneyStage( givingJourneySettings, currentDate, transactionDates );

            // with NULL giving JourneySettings, all the results should be null and not what they would be if not null
            Assert.AreNotEqual( unExpectedGivingJourneyStage, givingJourneyStage );
            Assert.AreEqual( null, givingJourneyStage );
        }

        /// <summary>
        /// </summary>
        /// <param name="expectedGivingJourneyStage">The expected giving journey stage.</param>
        /// <param name="daysAgoList">The days ago list.</param>
        [TestMethod]
        [DataRow( GivingJourneyStage.Former, 376, 383, 390, 397 )]
        [DataRow( GivingJourneyStage.Former, 390, 420, 450 )]
        [DataRow( GivingJourneyStage.Former, 400, 414, 428 )]

        [DataRow( GivingJourneyStage.Lapsed, 151, 200, 260 )]
        [DataRow( GivingJourneyStage.Lapsed, 161, 170, 190 )]

        // they look like a new giver since their first gift was 7 days ago, but NewFirstGiftInTheLastDays is null and they only gave once, so they wouldn't fall into a stage
        [DataRow( null, 7 )]

        // they look like a new giver since their first gift was less than 150 days ago and gave 3 times, but NewFirstGiftInTheLastDays is null, they would fall into Occasional instead of FirstTime giver
        [DataRow( GivingJourneyStage.Occasional, 149, 14, 1 )]

        // Just started giving, and gives consistently, but less than 4 times. So they might look like a new giver. However, if NewFirstGiftInTheLastDays is null, 
        // they would fall into Consistent giver instead of FirstTime giver since they are giving consistently every week
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28 )]

        // Just started giving, and started giving consistently, but only 2 times. So they might look like a new giver. However, if NewFirstGiftInTheLastDays is null,
        // They would fall into Consistent Giver instead of FirstTime giver.
        // This is a situation where they really shouldn't be called a consistent giver, because they only gave 2 times.  But since New isn't configured, they end up here.
        [DataRow( GivingJourneyStage.Consistent, 7, 14)]

        [DataRow( GivingJourneyStage.Occasional, 40, 120, 190 )]
        [DataRow( GivingJourneyStage.Occasional, 40, 120, ( 120 + 93 ) )]

        // Started recently, but gave more than 5 times, and has been giving consistently
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42 )]

        // Has been giving consistently starting more than 150 days ago, and gave three times, and hasn't lapsed more than 149 days
        [DataRow( GivingJourneyStage.Consistent, 151, 144, 137 )]

        // Has been giving consistently every 2 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 14, 28, 42, 56, 70, 84, 98, 112, 126, 140, 154, 168, 182, 196, 210, 224, 238, 252, 266, 280, 294, 308, 322, 336, 350, 364, 378, 392, 406, 420, 434, 448, 462 )]

        // Has been giving consistently every 1 weeks for over a year
        [DataRow( GivingJourneyStage.Consistent, 7, 14, 21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98, 105, 112, 119, 126, 133, 140, 147, 154, 161, 168, 175, 182, 189, 196, 203, 210, 217, 224, 231, 238, 245, 252, 259, 266, 273, 280, 287, 294, 301, 308, 315, 322, 329, 336, 343, 350, 357, 364, 371, 378, 385, 392, 399, 406 )]

        public void GivingJourneyStage_CalculatesGivingJourneyStageCorrectlyWithNullOptionalSettings( GivingJourneyStage? expectedGivingJourneyStage, params int[] daysAgoList )
        {
            GivingJourneySettings givingJourneySettings = new GivingJourneySettings();
            givingJourneySettings.FormerGiverNoContributionInTheLastDays = 375;
            givingJourneySettings.FormerGiverMedianFrequencyLessThanDays = null;

            givingJourneySettings.LapsedGiverNoContributionInTheLastDays = 150;
            givingJourneySettings.LapsedGiverMedianFrequencyLessThanDays = null;

            givingJourneySettings.NewGiverContributionCountBetweenMinimum = 1;
            givingJourneySettings.NewGiverContributionCountBetweenMaximum = 5;
            givingJourneySettings.NewGiverFirstGiftInTheLastDays = null;

            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMinimum = 33;
            givingJourneySettings.OccasionalGiverMedianFrequencyDaysMaximum = 94;

            givingJourneySettings.ConsistentGiverMedianLessThanDays = 32;

            var currentDate = RockDateTime.Now;

            var transactionDates = new List<DateTime>();
            foreach ( var daysAgo in daysAgoList )
            {
                transactionDates.Add( currentDate.AddDays( -daysAgo ) );
            }

            var givingJourneyStage = GivingJourneyHelper.GetGivingJourneyStage( givingJourneySettings, currentDate, transactionDates );

            Assert.AreEqual( expectedGivingJourneyStage, givingJourneyStage );
        }
    }
}
