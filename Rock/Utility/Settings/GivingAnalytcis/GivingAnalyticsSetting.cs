﻿// <copyright>
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

namespace Rock.Utility.Settings.GivingAnalytics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GivingAnalyticsSetting"/> class.
    /// </summary>
    public class GivingAnalyticsSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivingAnalyticsSetting"/> class.
        /// </summary>
        public GivingAnalyticsSetting()
        {
            this.GivingAnalytics = new GivingAnalytics();
            this.Alerting = new Alerting();
        }

        /// <summary>
        /// Gets or sets the giving analytics.
        /// </summary>
        /// <value>
        /// The giving analytics.
        /// </value>
        public GivingAnalytics GivingAnalytics { get; set; }

        /// <summary>
        /// Gets or sets the alerting.
        /// </summary>
        /// <value>
        /// The alerting.
        /// </value>
        public Alerting Alerting { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GivingAnalyticsSetting"/> class.
    /// </summary>
    public class GivingAnalytics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivingAnalytics"/> class.
        /// </summary>
        public GivingAnalytics()
        {
            // Default to 7 days a week if there is not already a setting
            this.GiverAnalyticsRunDays = new List<DayOfWeek>
            {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };

            this.GiverBins = new List<GiverBin>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the days of the week to run the analytics for each giver.
        /// </summary>
        /// <value>
        /// The days of the week to run the analytics for each giver.
        /// </value>
        public List<DayOfWeek> GiverAnalyticsRunDays { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the giving analytics Job last completed successfully.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time of the last time that the giving analytics Job completed successfully
        /// </value>
        public DateTime? GivingAnalyticsLastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets the giver bins.
        /// </summary>
        /// <value>
        /// The giver bins.
        /// </value>
        public List<GiverBin> GiverBins { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Alerting"/> class.
    /// </summary>
    public class Alerting
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
        /// Gets or sets the followup repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The followup repeat prevention duration.
        /// </value>
        public int? FollowupRepeatPreventionDurationDays { get; set; }
    }

    /// <summary>
    /// Information About the Giver Bin
    /// </summary>
    public class GiverBin {
        /// <summary>
        /// Gets or sets the lower range.
        /// </summary>
        /// <value>
        /// The lower range.
        /// </value>
        public decimal? LowerLimit { get; set; }
    }
}
