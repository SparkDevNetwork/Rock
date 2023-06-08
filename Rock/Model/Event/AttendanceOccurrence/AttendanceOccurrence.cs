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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance where a <see cref="Rock.Model.Person"/> attended (or was scheduled to attend) a group/location/schedule.
    /// This can be used for attendee/volunteer check-in, group attendance, etc.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "AttendanceOccurrence" )]
    [DataContract]
    [Analytics( false, false )]
    [Rock.SystemGuid.EntityTypeGuid( "0F6FD7F1-7AF5-4135-843F-E34948D4EA28")]
    public partial class AttendanceOccurrence : Model<AttendanceOccurrence>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that was checked in to.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that the individual attended/checked in to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> that was checked in to.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Schedule"/> that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the schedule that was checked in to.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [DataMember]
        [IgnoreCanDelete]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the date of the Attendance. Only the date is used.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date and time/check in date and time.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        [Index( "IX_OccurrenceDate" )]
        public DateTime OccurrenceDate
        {
            get
            {
                return _occurrenceDate.Date;
            }

            set
            {
                _occurrenceDate = value.Date;
            }
        }

        private DateTime _occurrenceDate;

        /// <summary>
        /// Gets or sets the did not occur.
        /// </summary>
        /// <value>
        /// The did not occur.
        /// </value>
        [DataMember]
        public bool? DidNotOccur { get; set; }

        /// <summary>
        /// Gets Sunday date.
        /// </summary>
        /// <value>
        /// The Sunday date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        [Index( "IX_SundayDate" )]
        public DateTime SundayDate
        {
            get
            {
                // NOTE: This is the In-Memory get, LinqToSql will get the value from the database.
                // Also, on an Insert/Update this will be the value saved to the database
                return OccurrenceDate.SundayDate();
            }

            set
            {
                // don't do anything here since EF uses this for loading, and we also want to ignore if somebody other than EF tries to set this 
            }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        [DataMember]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the number anonymous attendance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number anonymous attendance.
        /// </value>
        [DataMember]
        public int? AnonymousAttendanceCount { get; set; }

        /// <summary>
        /// Gets or sets the Accept Confirmation Message (for RSVP).
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string AcceptConfirmationMessage { get; set; }

        /// <summary>
        /// Gets or sets the Decline Confirmation Message (for RSVP).
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string DeclineConfirmationMessage { get; set; }

        /// <summary>
        /// Indicates whether or not to show the Decline Confirmation Message.
        /// </summary>
        [DataMember]
        public bool ShowDeclineReasons { get; set; }

        /// <summary>
        /// A comma-separated list of integer ID values representing the Decline Reasons selected by the attendee.
        /// </summary>
        /// <value>
        /// The integer IDs.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string DeclineReasonValueIds { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StepType"/> to which this occurrence is associated.
        /// </summary>
        [DataMember]
        public int? StepTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets the occurrence date key.
        /// </summary>
        /// <value>
        /// The occurrence date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int OccurrenceDateKey
        {
            get => OccurrenceDate.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        /// <summary>
        /// Gets or sets the attendance type value identifier.
        /// </summary>
        /// <value>
        /// The attendance type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES )]
        public int? AttendanceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the attendance reminder last sent date time.
        /// </summary>
        /// <value>The attendance reminder last sent date time.</value>
        [DataMember]
        public DateTime? AttendanceReminderLastSentDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that was attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that was attended.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> where the Person attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [LavaVisible]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/>.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaVisible]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the attendees.
        /// </summary>
        /// <value>
        /// The attendees.
        /// </value>
        [DataMember]
        public virtual ICollection<Attendance> Attendees { get; set; } = new Collection<Attendance>();

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StepType"/>.
        /// </summary>
        [DataMember]
        public virtual StepType StepType { get; set; }

        /// <summary>
        /// Gets or sets the occurrence source date.
        /// </summary>
        /// <value>
        /// The occurrence source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate OccurrenceSourceDate { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class AttendanceOccurrenceConfiguration : EntityTypeConfiguration<AttendanceOccurrence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceOccurrenceConfiguration"/> class.
        /// </summary>
        public AttendanceOccurrenceConfiguration()
        {
            this.HasOptional( a => a.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( true );

            // A Migration will manually add a ON DELETE SET NULL for ScheduleId.
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.StepType ).WithMany().HasForeignKey( p => p.StepTypeId ).WillCascadeOnDelete( true );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( r => r.OccurrenceSourceDate ).WithMany().HasForeignKey( r => r.OccurrenceDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
