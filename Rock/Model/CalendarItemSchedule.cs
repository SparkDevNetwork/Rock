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
    /// Represents an CalendarItemSchedule.
    /// </summary>
    [Table( "CalendarItemSchedule" )]
    [DataContract]
    public partial class CalendarItemSchedule : Model<CalendarItemSchedule>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventItem"/> that this CalendarItemSchedule is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventItem"/> that the CalendarItemSchedule is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EventItemCampusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Schedule"/> that this CalendarItemSchedule is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/> that the CalendarItemSchedule is associated with.
        /// </value>
        [Required]
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the CalendarItemSchedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the CalendarItemSchedule.
        /// </value>
        [Required]
        [DataMember]
        public string ScheduleName { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItem"/> that this CalendarItemSchedule is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EventItem"/> that this CalendarItemSchedule is a member of.
        /// </value>
        [DataMember]
        public virtual EventItemCampus EventItemCampus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> that this CalendarItemSchedule is a member of.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Schedule"/> that this CalendarItemSchedule is a member of.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// CalendarItemSchedule Configuration class.
    /// </summary>
    public partial class CalendarItemScheduleConfiguration : EntityTypeConfiguration<CalendarItemSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarItemScheduleConfiguration" /> class.
        /// </summary>
        public CalendarItemScheduleConfiguration()
        {
            this.HasRequired( p => p.EventItemCampus ).WithMany( p => p.CalendarItemSchedules ).HasForeignKey( p => p.EventItemCampusId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}