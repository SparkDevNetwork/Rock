// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an event item schedule.
    /// </summary>
    [Table( "EventItemSchedule" )]
    [DataContract]
    public partial class EventItemSchedule : Model<EventItemSchedule>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventItem"/> that this EventItemSchedule is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventItem"/> that the EventItemSchedule is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EventItemCampusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Schedule"/> that this EventItemSchedule is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/> that the EventItemSchedule is associated with.
        /// </value>
        [Required]
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the EventItemSchedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the EventItemSchedule.
        /// </value>
        [Required]
        [DataMember]
        [MaxLength( 100 )]
        public string ScheduleName { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this EventItemSchedule is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this EventItemSchedule is a member of.
        /// </value>
        public virtual EventItemCampus EventItemCampus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> that this EventItemSchedule is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Schedule"/> that this EventItemSchedule is a member of.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <value>
        /// The next start date time.
        /// </value>
        [NotMapped]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                if ( Schedule != null )
                {
                    return Schedule.NextStartDateTime;
                }

                return null;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Gets the start times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetStartTimes ( DateTime beginDateTime, DateTime endDateTime )
        {
            if ( Schedule != null )
            {
                return Schedule.GetScheduledStartTimes( beginDateTime, endDateTime );
            }
            else
            {
                return new List<DateTime>();
            }
        }

        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetFirstStartDateTime()
        {
            if ( Schedule != null )
            {
                return Schedule.GetFirstStartDateTime();
            }
            else
            {
                return null;
            }
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// EventItemSchedule Configuration class.
    /// </summary>
    public partial class EventItemScheduleConfiguration : EntityTypeConfiguration<EventItemSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemScheduleConfiguration" /> class.
        /// </summary>
        public EventItemScheduleConfiguration()
        {
            this.HasRequired( p => p.EventItemCampus ).WithMany( p => p.EventItemSchedules ).HasForeignKey( p => p.EventItemCampusId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}