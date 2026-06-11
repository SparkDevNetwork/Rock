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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Scheduled event in Rock.  Several places where this has been used includes Check-in scheduling and Kiosk scheduling.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "ScheduleDate" )]
    [Rock.SystemGuid.EntityTypeGuid( "eabc4b1c-8203-497b-a28c-93cac051de1d" )]
    public class ScheduleDate
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the schedule this date belongs to.
        /// </summary>
        [DataMember]
        [Key, Column( Order = 0 )]
        public int ScheduleId { get; set; }

        /// <summary>
        /// The date and time that this schedule instance starts on.
        /// </summary>
        [DataMember]
        [Key, Column( Order = 1 )]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The date and time that this schedule instance ends at.
        /// </summary>
        [DataMember]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// The date key for when this instance occurs. This can be used to join
        /// to the <see cref="AnalyticsSourceDate"/> table to allow for easier
        /// querying of schedule dates by date parts like month, day, year, etc.
        /// </summary>
        [DataMember]
        public int StartDateKey { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The <see cref="Rock.Model.Schedule"/> that date belongs to.
        /// </summary>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// The <see cref="Rock.Model.AnalyticsSourceDate"/> that provides
        /// additional query capabilities for this date.
        /// </summary>
        [DataMember]
        public virtual AnalyticsSourceDate AnalyticsSourceDate { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Schedule Date Configuration class.
    /// </summary>
    public class ScheduleDateConfiguration : EntityTypeConfiguration<ScheduleDate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleDateConfiguration"/> class.
        /// </summary>
        public ScheduleDateConfiguration()
        {
            this.HasRequired( sd => sd.Schedule )
                .WithMany( s => s.ScheduleDates )
                .HasForeignKey( s => s.ScheduleId )
                .WillCascadeOnDelete( true );

            // NOTE: When creating a migration for this, don't create the actual
            // FK's in the database for this just in case there are outlier dates
            // that aren't in the AnalyticsSourceDate table and so that the
            // AnalyticsSourceDate can be rebuilt from scratch as needed.
            this.HasRequired( r => r.AnalyticsSourceDate )
                .WithMany()
                .HasForeignKey( r => r.StartDateKey )
                .WillCascadeOnDelete( false );
        }
    }

    #endregion
}
