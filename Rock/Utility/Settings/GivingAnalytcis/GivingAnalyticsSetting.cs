using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.GiverAnalyticsRunDays = new List<DayOfWeek>();
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
        public int? GlobalRepeatPreventionDuration { get; set; }

        /// <summary>
        /// Gets or sets the gratitude repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The gratitude repeat prevention duration.
        /// </value>
        public int? GratitudeRepeatPreventionDuration { get; set; }

        /// <summary>
        /// Gets or sets the followup repeat prevention duration in days.
        /// </summary>
        /// <value>
        /// The followup repeat prevention duration.
        /// </value>
        public int? FollowupRepeatPreventionDuration { get; set; }
    }
}
