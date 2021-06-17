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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
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

        #endregion

        #region Virtual Properties

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
        /// Gets a value indicating whether attendance was entered (based on presence of any attendee records).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance entered]; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        public virtual bool AttendanceEntered
        {
            get
            {
                return Attendees.Any();
            }
        }

        /// <summary>
        /// Gets the number of attendees who attended.
        /// </summary>
        /// <value>
        /// The did attend count.
        /// </value>
        public virtual int DidAttendCount
        {
            get
            {
                return Attendees.Where( a => a.DidAttend.HasValue && a.DidAttend.Value ).Count();
            }
        }

        /// <summary>
        /// Gets the attendance rate.
        /// </summary>
        /// <value>
        /// The attendance rate which is the number of attendance records marked as did attend
        /// divided by the total number of attendance records for this occurrence.
        /// </value>
        public double AttendanceRate
        {
            get
            {
                var totalCount = Attendees.Count();
                if ( totalCount > 0 )
                {
                    return ( double ) ( DidAttendCount ) / ( double ) totalCount;
                }
                else
                {
                    return 0.0d;
                }
            }
        }

        /// <summary>
        /// Gets the percent members attended.
        /// </summary>
        /// <value>
        /// The percent members attended is the number of attendance records marked as did attend
        /// divided by the total number of members in the group.
        /// </value>
        [RockObsolete( "1.10" )]
        [System.Obsolete( "Use Attendance Rate instead." )]
        public double PercentMembersAttended
        {
            get
            {
                var groupMemberCount = Group.Members
                                    .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                    .Where( m => !m.IsArchived )
                                    .Count();

                if ( groupMemberCount > 0 )
                {
                    return ( double ) ( DidAttendCount ) / ( double ) groupMemberCount;
                }
                else
                {
                    return 0.0d;
                }
            }
        }
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
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( !result )
                    return result;

                using ( var rockContext = new RockContext() )
                {
                    // validate cases where the group type requires that a location/schedule is required
                    if ( GroupId == null )
                        return result;

                    var group = Group ?? new GroupService( rockContext ).Queryable( "GroupType" ).FirstOrDefault( g => g.Id == GroupId );
                    if ( group == null )
                        return result;

                    if ( group.GroupType.GroupAttendanceRequiresLocation && LocationId == null )
                    {
                        var locationErrorMessage = $"{group.GroupType.Name.Pluralize()} requires attendance records to have a location.";
                        ValidationResults.Add( new ValidationResult( locationErrorMessage ) );
                        result = false;
                    }

                    if ( group.GroupType.GroupAttendanceRequiresSchedule && ScheduleId == null )
                    {
                        var scheduleErrorMessage = $"{group.GroupType.Name.Pluralize()} requires attendance records to have a schedule.";
                        ValidationResults.Add( new ValidationResult( scheduleErrorMessage ) );
                        result = false;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Occurrence for {Schedule} {Group} at {Location} on { OccurrenceDate.ToShortDateString() }";
        }

        #endregion

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

    #region Enumerations

    /// <summary>
    /// For Attendance Reporting, graph into series partitioned by Total, Group, Campus, or Schedule
    /// </summary>
    public enum AttendanceGraphBy
    {
        /// <summary>
        /// Total (one series)
        /// </summary>
        Total = 0,

        /// <summary>
        /// Each selected Check-in Group (which is actually a [Group] under the covers) is a series
        /// </summary>
        Group = 1,

        /// <summary>
        /// Each campus (from Attendance.CampusId) is its own series
        /// </summary>
        Campus = 2,

        /// <summary>
        /// Each schedule (from Attendance.ScheduleId) is its own series
        /// </summary>
        Schedule = 3,

        /// <summary>
        /// Each Location (from Attendance.LocationId) is its own series
        /// </summary>
        Location = 4
    }

    #endregion

}
