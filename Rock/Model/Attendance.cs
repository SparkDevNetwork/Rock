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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

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
    public partial class Attendance : Model<Attendance>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the AttendanceOccurrence that the attendance is for.
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
        /// Gets or sets the attendance check in session identifier.
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
        /// Gets or sets the person that presented the <see cref="PersonAlias"/> person attended.
        /// </summary>
        /// <value>
        /// The person that presented the <see cref="PersonAlias"/> person attended.
        /// </value>
        [DataMember]
        public int? PresentByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the person that checked-out the <see cref="PersonAlias"/> person attended.
        /// </summary>
        /// <value>
        /// The person that checked-out the <see cref="PersonAlias"/> person attended.
        /// </value>
        [DataMember]
        public int? CheckedOutByPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the attendance check in session.
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
        [LavaInclude]
        public virtual AttendanceData AttendanceData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceOccurrence"/> for the attendance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceOccurrence"/> for the attendance
        /// </value>
        [LavaInclude]
        public virtual AttendanceOccurrence Occurrence { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [LavaInclude]
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
        [LavaInclude]
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
        /// Gets or sets the scheduled by person alias.
        /// </summary>
        /// <value>
        /// The scheduled by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ScheduledByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the presented by person alias.
        /// </summary>
        /// <value>
        /// The presented by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PresentByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the checked-out by person alias.
        /// </summary>
        /// <value>
        /// The checked-out by person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias CheckedOutByPersonAlias { get; set; }

        /// <summary>
        /// Gets a value indicating whether this attendance is currently checked in.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is currently checked in; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public bool IsCurrentlyCheckedIn
        {
            get
            {
                // If the attendance does not have an occurrence schedule, then there's nothing to check.
                if ( Occurrence == null || Occurrence.Schedule == null )
                {
                    return false;
                }

                // If person has checked-out, they are obviously not still checked in
                if ( EndDateTime.HasValue )
                {
                    return false;
                }

                // We'll check start time against timezone next, but don't even bother with that, if start date was more than 2 days ago
                if ( StartDateTime < RockDateTime.Now.AddDays( -2 ) )
                {
                    return false;
                }

                // Get the current time (and adjust for a campus timezone)
                var currentDateTime = RockDateTime.Now;
                if ( Campus != null )
                {
                    currentDateTime = Campus.CurrentDateTime;
                }
                else if ( CampusId.HasValue )
                {
                    var campus = CampusCache.Get( CampusId.Value );
                    if ( campus != null )
                    {
                        currentDateTime = campus.CurrentDateTime;
                    }
                }

                // Now that we now the correct time, make sure that the attendance is for today and previous to current time
                if ( StartDateTime < currentDateTime.Date || StartDateTime > currentDateTime )
                {
                    return false;
                }

                // Person is currently checked in, if the schedule for this attendance is still active
                return Occurrence.Schedule.WasScheduleOrCheckInActive( currentDateTime );
            }
        }

        #endregion

        #region Obsolete Properties

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.Group"/> that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.GroupId instead", true )]
        public int? GroupId
        {
            get
            {
                return null;
            }

            set
            {

            }
        }

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.Location"/> that the individual attended/checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.LocationId instead", true )]
        public int? LocationId
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets the Id of the schedule that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the schedule that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.ScheduleId instead", true )]
        public int? ScheduleId
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the did not occur.
        /// </summary>
        /// <value>
        /// The did not occur.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.DidNotOccur instead", true )]
        public bool? DidNotOccur
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.SundayDate instead", true )]
        public DateTime SundayDate
        {
            get
            {
                return Occurrence?.SundayDate ?? DateTime.MinValue;
            }

            set
            {
                if ( Occurrence != null )
                {
                    Occurrence.SundayDate = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that was attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that was attended.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.Group instead", true )]
        public virtual Group Group
        {
            get
            {
                return Occurrence?.Group;
            }

            set
            {
                this.GroupId = value?.Id;
                if ( Occurrence != null )
                {
                    Occurrence.Group = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.Location instead", true )]
        public virtual Location Location
        {
            get
            {
                return Occurrence?.Location;
            }

            set
            {
                this.LocationId = value?.Id;
                if ( Occurrence != null )
                {
                    Occurrence.Location = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.Schedule instead", true )]
        public virtual Schedule Schedule
        {
            get
            {
                return Occurrence?.Schedule;
            }

            set
            {
                this.ScheduleId = value?.Id;
                if ( Occurrence != null )
                {
                    Occurrence.Schedule = value;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            _isDeleted = entry.State == EntityState.Deleted;
            bool previousDidAttendValue;

            bool previouslyDeclined;

            if ( entry.State == EntityState.Added )
            {
                previousDidAttendValue = false;
                previouslyDeclined = false;
            }
            else
            {
                previousDidAttendValue = entry.Property( "DidAttend" )?.OriginalValue as bool? ?? false;

                // get original value of RSVP so we can detect whether the value changed
                previouslyDeclined = ( entry.Property( "RSVP" )?.OriginalValue as RSVP? ) == RSVP.No;
            }

            // if the record was changed to Declined, queue a GroupScheduleCancellationTransaction in PostSaveChanges
            _declinedScheduledAttendance = ( previouslyDeclined == false ) && this.IsScheduledPersonDeclined();

            if ( previousDidAttendValue == false && this.DidAttend == true )
            {
                new Rock.Transactions.GroupAttendedTransaction( entry ).Enqueue();
            }

            base.PreSaveChanges( dbContext, entry );
        }

        private bool _declinedScheduledAttendance = false;
        private bool _isDeleted = false;

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( _declinedScheduledAttendance )
            {
                new GroupScheduleCancellationTransaction( this ).Enqueue();
            }

            if ( !_isDeleted )
            {
                // The data context save operation doesn't need to wait for this to complete
                Task.Run( () => StreakTypeService.HandleAttendanceRecord( this ) );
            }

            base.PostSaveChanges( dbContext );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( !DidAttend.HasValue )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append( ( PersonAlias?.Person != null ) ? PersonAlias.Person.ToStringSafe() + " " : string.Empty );

            string verb = "attended";
            if ( this.DidAttend != true )
            {
                verb = "did not attend";

                if ( this.ScheduledToAttend == true )
                {
                    verb = "is scheduled to attend";
                }
                else if ( this.RequestedToAttend == true )
                {
                    verb = "has been requested to attend";
                }
                else if ( this.DeclineReasonValueId.HasValue )
                {
                    verb = "has declined to attend";
                }
                else
                {
                    verb = "did not attend";
                }
            }
            else
            {
                verb = "attended";
            }

            sb.Append( $"{verb} " );

            sb.Append( Occurrence?.Group?.ToStringSafe() );
            if ( DidAttend.Value )
            {
                sb.AppendFormat( " on {0} at {1}", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString() );

                var end = EndDateTime ?? Occurrence?.OccurrenceDate;
                if ( end.HasValue )
                {
                    sb.AppendFormat( " until {0} at {1}", end.Value.ToShortDateString(), end.Value.ToShortTimeString() );
                }
            }

            if ( Occurrence?.Location != null )
            {
                sb.Append( " in " + Occurrence.Location.ToStringSafe() );
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// A <see cref="T:System.Boolean" /> that is <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                return base.IsValid;
            }
        }

        /// <summary>
        /// Determines whether [is scheduled person confirmed].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is scheduled person confirmed]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsScheduledPersonConfirmed()
        {
            return this.ScheduledToAttend == true && this.RSVP == RSVP.Yes;
        }

        /// <summary>
        /// Determines whether [is scheduled person declined].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is scheduled person declined]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsScheduledPersonDeclined()
        {
            return this.RSVP == RSVP.No;
        }

        /// <summary>
        /// Gets the scheduled attendance item status.
        /// </summary>
        /// <param name="rsvp">The RSVP.</param>
        /// <param name="scheduledToAttend">The scheduled to attend.</param>
        /// <returns></returns>
        public static ScheduledAttendanceItemStatus GetScheduledAttendanceItemStatus( RSVP rsvp, bool? scheduledToAttend )
        {
            var status = ScheduledAttendanceItemStatus.Pending;
            if ( rsvp == RSVP.No )
            {
                status = ScheduledAttendanceItemStatus.Declined;
            }
            else if ( scheduledToAttend == true )
            {
                status = ScheduledAttendanceItemStatus.Confirmed;
            }

            return status;
        }

        #endregion
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
            this.HasOptional( a => a.AttendanceData ).WithRequired().WillCascadeOnDelete();
        }
    }

    #endregion


    #region Enumerations

    /// <summary>
    /// RSVP Response
    /// </summary>
    public enum RSVP
    {
        /// <summary>
        /// No
        /// </summary>
        No = 0,

        /// <summary>
        /// Yes
        /// </summary>
        Yes = 1,

        /// <summary>
        /// Here's my number, call me Maybe.
        /// Not used by Group Scheduler.
        /// </summary>
        Maybe = 2,

        /// <summary>
        /// RVSP not answered yet (or doesn't apply)
        /// </summary>
        Unknown = 3
    }

    #endregion
}