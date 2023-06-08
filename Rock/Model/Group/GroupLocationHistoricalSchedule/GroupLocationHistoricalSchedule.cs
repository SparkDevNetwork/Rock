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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Child table for GroupLocationHistorical to store the 0 or more Schedules associated with that GroupLocation at that point in history
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupLocationHistoricalSchedule" )]
    [DataContract]
    [Obsolete( "Group Location Historical Schedule is not used and is not reflected in the UI.  Consider using 'History' entity instead." )]
    [RockObsolete( "1.16" )]
    [Rock.SystemGuid.EntityTypeGuid( "3BC646E4-CA5E-47D6-BC6D-4BBFAAEDAD8B" )]
    public class GroupLocationHistoricalSchedule : Entity<GroupLocationHistoricalSchedule>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group location historical identifier.
        /// </summary>
        /// <value>
        /// The group location historical identifier.
        /// </value>
        [DataMember]
        public int GroupLocationHistoricalId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> id of this group's location's schedule[n] at this point in history (Group.GroupLocation.Schedules[n].Id)
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the schedule name of this group's location's schedule at this point in history  (Group.GroupLocation.Schedules[n].ToString())
        /// </summary>
        /// <value>
        /// The location name.
        /// </value>
        [DataMember]
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the Schedule's ModifiedDateTime. This is used internally to detect if the group's schedule has changed
        /// </summary>
        /// <value>
        /// The schedule's iCalendarContent
        /// </value>
        [DataMember]
        public DateTime? ScheduleModifiedDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupLocationHistorical"/> for this group historical schedule record
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual GroupLocationHistorical GroupLocationHistorical { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> for this group historical schedule record
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    [Obsolete( "Group Location Historical Schedule is not used and is not reflected in the UI.  Consider using 'History' entity instead." )]
    [RockObsolete( "1.16" )]
    public partial class GroupLocationHistoricalScheduleConfiguration : EntityTypeConfiguration<GroupLocationHistoricalSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationHistoricalScheduleConfiguration"/> class.
        /// </summary>
        public GroupLocationHistoricalScheduleConfiguration()
        {
            this.HasRequired( p => p.GroupLocationHistorical ).WithMany( t => t.GroupLocationHistoricalSchedules ).HasForeignKey( p => p.GroupLocationHistoricalId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
