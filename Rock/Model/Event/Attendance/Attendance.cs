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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Event;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance where a <see cref="Rock.Model.Person"/> who attended or was scheduled to attend a group or event.
    /// This can be used for attendee/volunteer check-in, group attendance, etc.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "Attendance" )]
    [DataContract]
    [Analytics( false, false )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ATTENDANCE )]
    public partial class Attendance : Model<Attendance>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.AttendanceOccurrence"/> that the attendance is for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the Id of the AttendanceOccurrence that the attendance is for.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that attended/checked in to the <see cref="Rock.Model.Group"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who attended/checked in.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that the individual attended/checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that was checked in to.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Device"/> that was used (the device where the person checked in from).
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Device"/> that was used.
        /// </value>
        [DataMember]
        public int? DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Check-in Search Type <see cref="Rock.Model.DefinedValue"/> that was used to search for the person/family.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Check-in Search Type <see cref="Rock.Model.DefinedValue"/> that was used to search for the person/family.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE )]
        public int? SearchTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the value that was entered when searching for family during check-in.
        /// </summary>
        /// <value>
        /// The search value entered.
        /// </value>
        [DataMember]
        public string SearchValue { get; set; }

        /// <summary>
        /// Gets or sets the person who was identified as the person doing the check-in.
        /// </summary>
        /// <value>
        /// The person alias identifier of person doing check-in.
        /// </value>
        [DataMember]
        public int? CheckedInByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> (family) that was selected after searching.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> (family) that was selected.
        /// </value>
        [DataMember]
        public int? SearchResultGroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.AttendanceCode"/> that is associated with this <see cref="Rock.Model.Attendance"/> entity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.AttendanceCode"/> that is associated with this <see cref="Rock.Model.Attendance"/> entity.
        /// </value>
        [DataMember]
        public int? AttendanceCodeId { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value id.  Qualifier can be used to
        /// "qualify" attendance records.  There are not any system values
        /// for this particular defined type
        /// </summary>
        /// <value>
        /// The qualifier value id.
        /// </value>
        [DataMember]
        public int? QualifierValueId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that person checked in
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that person checked in
        /// </value>
        [DataMember]
        [Index( "IX_StartDateTime" )]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that person checked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that person checked out.
        /// </value>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the RSVP.
        /// </summary>
        /// <value>
        /// The RSVP.
        /// </value>
        [DataMember]
        public RSVP RSVP { get; set; } = RSVP.Unknown;

        /// <summary>
        /// Gets or sets a flag indicating if the person attended.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> indicating if the person attended. This value will be <c>true</c> if they did attend, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? DidAttend { get; set; } = true;

        /// <summary>
        /// Gets or sets the processed.
        /// </summary>
        /// <value>
        /// The processed.
        /// </value>
        [DataMember]
        public bool? Processed { get; set; }

        /// <summary>
        /// Gets or sets if this first time that this person has ever checked into anything
        /// </summary>
        /// <value>
        /// If this attendance is the first time the person has attended anything
        /// </value>
        [DataMember]
        public bool? IsFirstTime { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="PersonAlias"/> person is scheduled (confirmed) to attend.
        /// </summary>
        /// <value>
        /// The scheduled to attend.
        /// </value>
        [DataMember]
        public bool? ScheduledToAttend { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="PersonAlias"/> person has been requested to attend.
        /// </summary>
        /// <value>
        /// The requested to attend.
        /// </value>
        [DataMember]
        public bool? RequestedToAttend { get; set; }

        /// <summary>
        /// Gets or sets if a schedule confirmation has been sent to the <see cref="PersonAlias"/> person
        /// </summary>
        /// <value>
        /// The schedule confirmation sent.
        /// </value>
        [DataMember]
        public bool? ScheduleConfirmationSent { get; set; }

        /// <summary>
        /// Gets or sets if a schedule reminder has been sent to the <see cref="PersonAlias"/> person
        /// </summary>
        /// <value>
        /// The schedule reminder sent.
        /// </value>
        [DataMember]
        public bool? ScheduleReminderSent { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RSVP"/> date time.
        /// </summary>
        /// <value>
        /// The RSVP date time.
        /// </value>
        [DataMember]
        public DateTime? RSVPDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Reason that the <see cref="PersonAlias"/> person declined to attend
        /// </summary>
        /// <value>
        /// The decline reason value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.GROUP_SCHEDULE_DECLINE_REASON )]
        public int? DeclineReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the person that scheduled the <see cref="PersonAlias"/> person to attend
        /// </summary>
        /// <value>
        /// The scheduled by person alias identifier.
        /// </value>
        [DataMember]
        public int? ScheduledByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceCheckInSession"/> identifier.
        /// </summary>
        /// <value>
        /// The attendance check in session identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? AttendanceCheckInSessionId { get; set; }

        /// <summary>
        /// Gets or sets the present date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the present date and time.
        /// </value>
        [DataMember]
        public DateTime? PresentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person that presented the <see cref="Rock.Model.PersonAlias"/> person attended.
        /// </summary>
        /// <value>
        /// The person that presented the <see cref="PersonAlias"/> person attended.
        /// </value>
        [DataMember]
        public int? PresentByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the person that checked-out the <see cref="Rock.Model.PersonAlias"/> person attended.
        /// </summary>
        /// <value>
        /// The person that checked-out the <see cref="PersonAlias"/> person attended.
        /// </value>
        [DataMember]
        public int? CheckedOutByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the check in status of this attendance record.
        /// </summary>
        /// <value>The check in status.</value>
        [DataMember]
        public CheckInStatus CheckInStatus { get; set; } = CheckInStatus.Unknown;

        /// <summary>
        /// Gets or sets the Id of the Attendance Source <see cref="DefinedValue"/>
        /// that is considered the source of this attendance record.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Check-in Search Type <see cref="Rock.Model.DefinedValue"/> that was used to search for the person/family.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE )]
        public int? SourceValueId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceCheckInSession"/>.
        /// </summary>
        /// <value>
        /// The attendance check in session.
        /// </value>
        public virtual AttendanceCheckInSession AttendanceCheckInSession { get; set; }

        /// <summary>
        /// Gets or sets additional data associated with the Attendance, including LabelData
        /// </summary>
        /// <value>
        /// The label data.
        /// </value>
        [LavaVisible]
        public virtual AttendanceData AttendanceData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceOccurrence"/> for the attendance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceOccurrence"/> for the attendance
        /// </value>
        [LavaVisible]
        public virtual AttendanceOccurrence Occurrence { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Device"/> that was used to check in
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Device"/> that was used to check in
        /// </value>
        [DataMember]
        public virtual Device Device { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue "/> representing the type of search used during check-in
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/>  representing the search type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SearchTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> (family) that was selected after searching during check-in.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> (family) that was selected during check-in.
        /// </value>
        [LavaVisible]
        public virtual Group SearchResultGroup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceCode"/> associated with this Attendance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceCode"/> associated with this Attendance.
        /// </value>
        [DataMember]
        public virtual AttendanceCode AttendanceCode { get; set; }

        /// <summary>
        /// Gets or sets the qualifier.
        /// </summary>
        /// <value>
        /// The qualifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue Qualifier { get; set; }

        /// <summary>
        /// Gets or sets the decline reason value.
        /// </summary>
        /// <value>
        /// The decline reason value.
        /// </value>
        [DataMember]
        public virtual DefinedValue DeclineReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the scheduled by <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The scheduled by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ScheduledByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the presented by <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The presented by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PresentByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the checked-out by <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The checked-out by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias CheckedOutByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DefinedValue "/> representing the source
        /// of this attendance record.
        /// </summary>
        /// <value>
        /// The <see cref="DefinedValue"/> representing the source of this
        /// attendance record.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceValue { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class AttendanceConfiguration : EntityTypeConfiguration<Attendance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttendanceConfiguration"/> class.
        /// </summary>
        public AttendanceConfiguration()
        {
            this.HasRequired( a => a.Occurrence ).WithMany( o => o.Attendees ).HasForeignKey( p => p.OccurrenceId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Device ).WithMany().HasForeignKey( d => d.DeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SearchTypeValue ).WithMany().HasForeignKey( v => v.SearchTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SearchResultGroup ).WithMany().HasForeignKey( p => p.SearchResultGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Qualifier ).WithMany().HasForeignKey( p => p.QualifierValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AttendanceCode ).WithMany( c => c.Attendances ).HasForeignKey( a => a.AttendanceCodeId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.DeclineReasonValue ).WithMany().HasForeignKey( a => a.DeclineReasonValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ScheduledByPersonAlias ).WithMany().HasForeignKey( p => p.ScheduledByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.CheckedOutByPersonAlias ).WithMany().HasForeignKey( p => p.CheckedOutByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.PresentByPersonAlias ).WithMany().HasForeignKey( p => p.PresentByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SourceValue ).WithMany().HasForeignKey( a => a.SourceValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AttendanceData ).WithRequired().WillCascadeOnDelete();
        }
    }

    #endregion Entity Configuration
}