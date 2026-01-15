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

using Rock.SystemKey;
using Rock.Enums.Core;

namespace Rock.Utility.Settings.Giving
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GivingAutomationSettings"/> class.
    /// </summary>
    public class GivingAutomationSettings
    {
        /// <summary>
        /// Load the giving automation settings from system settings
        /// </summary>
        /// <returns></returns>
        public static GivingAutomationSettings LoadGivingAutomationSettings()
        {
            var settings = Rock.Web.SystemSettings
                .GetValue( SystemSetting.GIVING_AUTOMATION_CONFIGURATION )
                .FromJsonOrNull<GivingAutomationSettings>() ?? new GivingAutomationSettings();

            settings.TransactionTypeGuids = settings.TransactionTypeGuids ?? new List<Guid>();
            settings.FinancialAccountGuids = settings.FinancialAccountGuids ?? new List<Guid>();

            if ( !settings.TransactionTypeGuids.Any() )
            {
                settings.TransactionTypeGuids.Add( SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            }

            settings.GivingAutomationJobSettings = settings.GivingAutomationJobSettings ?? new GivingAutomationJobSettings();
            settings.GivingJourneySettings = settings.GivingJourneySettings ?? new GivingJourneySettings();
            settings.GivingJourneySettings.DaysToUpdateGivingJourneys = settings.GivingJourneySettings.DaysToUpdateGivingJourneys ?? new DayOfWeek[1] { DayOfWeek.Tuesday };
            settings.GivingAlertingSettings = settings.GivingAlertingSettings ?? new GivingAlertingSettings();
            settings.GivingClassificationSettings = settings.GivingClassificationSettings ?? new GivingClassificationSettings();
            settings.GivingClassificationSettings.RunDays = settings.GivingClassificationSettings.RunDays ?? DaysOfWeekFlags.All.AsDayOfWeekList().ToArray();

            return settings;
        }

        /// <summary>
        /// Saves the giving automation settings to system settings
        /// </summary>
        /// <param name="givingAutomationSettings">The giving automation setting.</param>
        public static void SaveGivingAutomationSettings( GivingAutomationSettings givingAutomationSettings )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.GIVING_AUTOMATION_CONFIGURATION, givingAutomationSettings.ToJson() );
        }

        /*
        * 2021-06-30 BJW
        * There used to be a constructor here that would set GivingAutomation and Alerting to new instances so
        * that null values wouldn't be an issue. This was causing problems with the JSON serialization and
        * deserialization. Particularly, lists of values were appended rather than being replaced when
        * deserializing the value from the attribute value. So it was possible to get a list of days of the
        * week that contained 10+ values for instance.
        * 
        * Updated 2021-08-28 MDP
        * For example. Duplicates to build up if declaring a default value like this:
        * 
        * public List<DayOfWeek> RunDays { get; set; } = DayOfWeekFlag.All.AsDayOfWeekList()
        * 
        */

        /// <summary>
        /// This is the Settings for the <see cref="Rock.Jobs.GivingAutomation"/> Job.
        /// </summary>
        /// <value>
        /// The giving automation job settings.
        /// </value>
        public GivingAutomationJobSettings GivingAutomationJobSettings { get; set; }

        /// <summary>
        /// Gets or sets the giving journey settings.
        /// </summary>
        /// <value>The giving journey settings.</value>
        public GivingJourneySettings GivingJourneySettings { get; set; }

        /// <summary>
        /// Gets or sets the giving alerting settings.
        /// </summary>
        /// <value>The giving alerting settings.</value>
        public GivingAlertingSettings GivingAlertingSettings { get; set; }

        /// <summary>
        /// Gets or sets the giving classification settings.
        /// </summary>
        /// <value>The giving classification settings.</value>
        public GivingClassificationSettings GivingClassificationSettings { get; set; }

        /// <summary>
        /// Gets or sets the transaction type guids (defined value guids) that will be included in
        /// classification, journeys and alerting.
        /// </summary>
        /// <value>
        /// The transaction type guids.
        /// </value>
        public List<Guid> TransactionTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the financial account guids. If empty, then we assume all tax deductible accounts are used.
        /// </summary>
        /// <value>
        /// The financial account guids.
        /// </value>
        public List<Guid> FinancialAccountGuids { get; set; }

        /// <summary>
        /// Gets or sets if child accounts (all descendants) are included. This defaults to false.
        /// </summary>
        /// <value>
        /// The are child accounts included.
        /// </value>
        public bool? AreChildAccountsIncluded { get; set; }
    }

    /// <summary>
    /// This is the Settings for the <see cref="Rock.Jobs.GivingAutomation">Rock.Jobs.GivingAutomation</see> Job.
    /// </summary>
    public class GivingAutomationJobSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Giving Classificatio nSettings.
    /// </summary>
    public class GivingClassificationSettings
    {
        /// <summary>
        /// Gets or sets the run days of the Classification updates in the  <see cref="Rock.Jobs.GivingAutomation"/> job.
        /// </summary>
        /// <value>The run days.</value>
        public DayOfWeek[] RunDays { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the <see cref="Rock.Jobs.GivingAutomation"/> job last completed successfully.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time of the last time that the giving automation Job completed successfully
        /// </value>
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Giving Automation filters have changed since the last time
        /// the job was run.
        /// </summary>
        /// <remarks>
        /// This is set when the effective filter inputs (transaction types + accounts, including child account expansion)
        /// change. When true, jobs should reprocess all giving units and then clear this flag.
        /// </remarks>
        public bool FiltersChanged { get; set; }
    }

    /// <summary>
    /// Giving Journey Settings
    /// </summary>
    public class GivingJourneySettings
    {
        /// <summary>
        /// Gets or sets the days to update giving journeys.
        /// </summary>
        /// <value>The days to update giving journeys.</value>
        public DayOfWeek[] DaysToUpdateGivingJourneys { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of days since a giving unit's first qualifying gift to be classified as a New Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@NewGiverFirstGaveDays</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? NewGiverFirstGaveDays { get; set; } = 150;

        /// <summary>
        /// Gets or sets the minimum number of qualifying gifts (within the 24-month analysis window) required to be classified as a New Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@NewGiverContributionCountBetweenMinimum</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? NewGiverContributionCountBetweenMinimum { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of qualifying gifts (within the 24-month analysis window) allowed to be classified as a New Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@NewGiverContributionCountBetweenMaximum</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? NewGiverContributionCountBetweenMaximum { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum number of days since the most recent qualifying gift (within the 24-month analysis window)
        /// for a giving unit to be considered "active" for Consistent Giver classification.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@ConsistentGiverLastGaveDays</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? ConsistentGiverLastGaveDays { get; set; } = 32;

        /// <summary>
        /// Gets or sets the maximum mean frequency (average days between gifts) for a giving unit to be classified as a Consistent Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@ConsistentGiverMeanFrequency</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? ConsistentGiverMeanFrequency { get; set; } = 32;

        /// <summary>
        /// Gets or sets the maximum number of days since the most recent qualifying gift (within the 24-month analysis window)
        /// for a giving unit to be considered "active" for Occasional Giver classification.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@OccasionalGiverLastGaveDays</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? OccasionalGiverLastGaveDays { get; set; } = 150;

        /// <summary>
        /// Gets or sets the maximum mean frequency (average days between gifts) for a giving unit to be classified as an Occasional Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@OccasionalGiverMeanFrequency</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? OccasionalGiverMeanFrequency { get; set; } = 94;

        /// <summary>
        /// Gets or sets the number of days of inactivity after which a giving unit becomes a Lapsed Giver.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@LapsedGiverNoGiftDays</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? LapsedGiverNoGiftDays { get; set; } = 150;

        /// <summary>
        /// Gets or sets the mean frequency (average days between gifts) threshold for a giving unit to be classified as Lapsed.
        /// </summary>
        /// <remarks>
        /// This corresponds to <c>@LapsedGiverMeanFrequency</c> in <c>spGivingAutomation_UpdateGivingJourneyStages</c>.
        /// </remarks>
        public int? LapsedGiverMeanFrequency { get; set; } = 100;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GivingAlertingSettings"/> class.
    /// </summary>
    public class GivingAlertingSettings
    {
        /// <summary>
        /// Gets or sets the global repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The global repeat prevention duration.
        /// </value>
        public int? GlobalRepeatPreventionDurationDays { get; set; }

        /// <summary>
        /// Gets or sets the gratitude repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The gratitude repeat prevention duration.
        /// </value>
        public int? GratitudeRepeatPreventionDurationDays { get; set; }

        /// <summary>
        /// Gets or sets the follow-up repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The follow-up repeat prevention duration.
        /// </value>
        public int? FollowupRepeatPreventionDurationDays { get; set; }
    }

}
