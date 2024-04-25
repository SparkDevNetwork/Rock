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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Scheduled event in Rock.  Several places where this has been used includes Check-in scheduling and Kiosk scheduling.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Schedule" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.SCHEDULE )]
    public partial class Schedule : Model<Schedule>, ICategorized, IHasActiveFlag, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the Schedule. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Schedule.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [IncludeForReporting]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined Description of the Schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Description of the Schedule.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes prior to the Schedule's start time  that Check-in should be active. 0 represents that Check-in 
        /// will not be available to the beginning of the event.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing how many minutes prior the Schedule's start time that Check-in should be active. 
        /// 0 means that Check-in will not be available to the Schedule's start time. This schedule will not be available if this value is <c>Null</c>.
        /// </value>
        [DataMember]
        public int? CheckInStartOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes following schedule start that Check-in should be active. 0 represents that Check-in will only be available
        /// until the Schedule's start time.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing how many minutes following the Schedule's end time that Check-in should be active. 0 represents that Check-in
        /// will only be available until the Schedule's start time.
        /// </value>
        [DataMember]
        public int? CheckInEndOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the Date that the Schedule becomes effective/active. This property is inclusive, and the schedule will be inactive before this date. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the date that this Schedule becomes active.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets that date that this Schedule expires and becomes inactive. This value is inclusive and the schedule will be inactive after this date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the date that this Schedule ends and becomes inactive.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Gets or sets the weekly day of week.
        /// </summary>
        /// <value>
        /// The weekly day of week.
        /// </value>
        [DataMember]
        public DayOfWeek? WeeklyDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the weekly time of day.
        /// </summary>
        /// <value>
        /// The weekly time of day.
        /// </value>
        [DataMember]
        [CodeGenExclude( CodeGenFeature.ViewModelFile )]
        public TimeSpan? WeeklyTimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this Schedule belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that this Schedule belongs to. This property will be null
        /// if the Schedule does not belong to a Category.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto inactivate when complete].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto inactivate when complete]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AutoInactivateWhenComplete { get; set; } = false;

        /// <summary>
        /// Gets or sets a flag indicating if this is an active schedule. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this schedule is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the order.
        /// Use <see cref="ExtensionMethods.OrderByOrderAndNextScheduledDateTime(List{Schedule})"/>
        /// to get the schedules in the desired order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this Schedule is public.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if this Schedule is public, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsPublic { get; set; } = true;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets a value indicating whether Check-in is enabled for this Schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is check in enabled; otherwise, <c>false</c>.
        /// <remarks>
        /// The <c>CheckInStartOffsetMinutes</c> is used to determine if Check-in is enabled. If the value is <c>null</c>, it is determined that Check-in is not 
        /// enabled for this Schedule.
        /// </remarks>
        /// </value>
        public virtual bool IsCheckInEnabled
        {
            get
            {
                return CheckInStartOffsetMinutes.HasValue && IsActive;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Schedule belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this Schedule belongs to.  If it does not belong to a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        #endregion

        #region Constants

        /// <summary>
        /// The "nth" names for DayName of Month (First, Second, Third, Forth, Last)
        /// </summary>
        public static readonly Dictionary<int, string> NthNames = new Dictionary<int, string> {
            { 1, "First" },
            { 2, "Second" },
            { 3, "Third" },
            { 4, "Fourth" },
            { -1, "Last" }
        };

        /// <summary>
        /// The abbreviated "nth" names for DayName of Month (1st, 2nd, 3rd, 4th, last)
        /// </summary>
        private static readonly Dictionary<int, string> NthNamesAbbreviated = new Dictionary<int, string> {
            { 1, "1st" },
            { 2, "2nd" },
            { 3, "3rd" },
            { 4, "4th" },
            { -1, "last" }
        };

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class ScheduleConfiguration : EntityTypeConfiguration<Schedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleConfiguration"/> class.
        /// </summary>
        public ScheduleConfiguration()
        {
            this.HasOptional( s => s.Category ).WithMany().HasForeignKey( s => s.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
