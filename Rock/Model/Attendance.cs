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

using Rock.Data;
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
        public RSVP RSVP { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the person attended.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> indicating if the person attended. This value will be <c>true</c> if they did attend, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? DidAttend { get; set; }

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

        #endregion

        #region Virtual Properties

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

        // Keep track if any of the obsolete properties that were moved to AttendanceOccurrence were updated, then we'll deal with that on PreSaveChanges
        private bool _updatedObsoleteGroupId = false;
        private int? _updatedObsoleteGroupIdValue = null;

        private bool _updatedObsoleteLocationId = false;
        private int? _updatedObsoleteLocationIdValue = null;

        private bool _updatedObsoleteScheduleId = false;
        private int? _updatedObsoleteScheduleIdValue = null;

        private bool _updatedObsoleteDidNotOccur = false;
        private bool? _updatedObsoleteDidNotOccurValue = null;

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.Group"/> that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Occurrence.GroupId instead", false )]
        public int? GroupId
        {
            get
            {
                return _updatedObsoleteGroupId ? _updatedObsoleteGroupIdValue : Occurrence?.GroupId;
            }

            set
            {
                _updatedObsoleteGroupId = true;
                _updatedObsoleteGroupIdValue = value;

                // Update ModifiedDateTime to ensure this record is Tracked in ChangeTracker
                ModifiedDateTime = RockDateTime.Now;
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
        [Obsolete( "Use Occurrence.LocationId instead", false )]
        public int? LocationId
        {
            get
            {
                return _updatedObsoleteLocationId ? _updatedObsoleteLocationIdValue : Occurrence?.LocationId;
            }

            set
            {
                _updatedObsoleteLocationId = true;
                _updatedObsoleteLocationIdValue = value;

                // Update ModifiedDateTime to ensure this record is Tracked in ChangeTracker
                ModifiedDateTime = RockDateTime.Now;
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
        [Obsolete( "Use Occurrence.ScheduleId instead", false )]
        public int? ScheduleId
        {
            get
            {
                return _updatedObsoleteScheduleId ? _updatedObsoleteScheduleIdValue : Occurrence?.ScheduleId;
            }

            set
            {
                _updatedObsoleteScheduleId = true;
                _updatedObsoleteScheduleIdValue = value;

                // Update ModifiedDateTime to ensure this record is Tracked in ChangeTracker
                ModifiedDateTime = RockDateTime.Now;
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
        [Obsolete( "Use Occurrence.DidNotOccur instead", false )]
        public bool? DidNotOccur
        {
            get
            {
                return _updatedObsoleteDidNotOccur ? _updatedObsoleteDidNotOccurValue : Occurrence?.DidNotOccur;
            }

            set
            {
                _updatedObsoleteDidNotOccur = true;
                _updatedObsoleteDidNotOccurValue = value;

                // Update ModifiedDateTime to ensure this record is Tracked in ChangeTracker
                ModifiedDateTime = RockDateTime.Now;
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
        [Obsolete( "Use Occurrence.SundayDate instead", false )]
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
        [Obsolete( "Use Occurrence.Group instead", false )]
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
        [Obsolete( "Use Occurrence.Location instead", false )]
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
        [Obsolete( "Use Occurrence.Schedule instead", false )]
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
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.GroupAttendedTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

#pragma warning disable 612, 618
            ProcessObsoleteOccurrenceFields( entry );
#pragma warning restore 612, 618

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Processes the obsolete occurrence fields.
        /// </summary>
        /// <param name="entry">The entry.</param>
        [RockObsolete( "1.8" )]
        [Obsolete]
        private void ProcessObsoleteOccurrenceFields( DbEntityEntry entry )
        {
            if ( entry.State == EntityState.Modified || entry.State == EntityState.Added )
            {
                // NOTE: If they only changed StartDateTime, don't change the Occurrence record. We want to support letting StartDateTime be a different Date than the OccurenceDate in that situation
                if ( _updatedObsoleteGroupId || _updatedObsoleteLocationId || _updatedObsoleteScheduleId || _updatedObsoleteDidNotOccur )
                {
                    if ( _updatedObsoleteGroupId || _updatedObsoleteLocationId || _updatedObsoleteScheduleId )
                    {
                        // if they changed or set stuff related to AttendanceOccurrence (not including DidNotOccur or StartDateTime) thru obsolete properties, find or create a Matching AttendanceOccurrence Record
                        using ( var attendanceOccurrenceRockContext = new RockContext() )
                        {
                            var attendanceOccurrenceService = new AttendanceOccurrenceService( attendanceOccurrenceRockContext );

                            // if GroupId,LocationId, or ScheduleId changed, use StartDateTime's Date as the OccurrenceDate to look up AttendanceOccurence since it is really a completely different Occurrence if Group,Location or Schedule changes
                            var occurrenceDate = this.StartDateTime.Date;

                            var attendanceOccurrence = attendanceOccurrenceService.Queryable().Where( a => a.GroupId == this.GroupId && a.LocationId == this.LocationId && a.ScheduleId == this.ScheduleId && a.OccurrenceDate == occurrenceDate ).FirstOrDefault();
                            if ( attendanceOccurrence != null )
                            {
                                // found a matching attendanceOccurrence, so use that
                                if ( _updatedObsoleteDidNotOccur && attendanceOccurrence.DidNotOccur != this.DidNotOccur )
                                {
                                    // If DidNotOccur also changed, update the DidNotOccur for the attendanceOccurrence
                                    // NOTE: This will update *all* Attendances' DidNotOccur for this AttendanceOccurrence. That is OK. That is what we want to happen.
                                    attendanceOccurrence.DidNotOccur = this.DidNotOccur;
                                    attendanceOccurrenceRockContext.SaveChanges();
                                }

                                if ( attendanceOccurrence.Id != this.OccurrenceId )
                                {
                                    this.OccurrenceId = attendanceOccurrence.Id;
                                }
                            }
                            else
                            {
                                // didn't find a matching attendanceOccurrence, so create and insert a new one
                                attendanceOccurrence = new AttendanceOccurrence
                                {
                                    GroupId = this.GroupId,
                                    LocationId = this.LocationId,
                                    ScheduleId = this.ScheduleId,
                                    DidNotOccur = this.DidNotOccur,
                                    OccurrenceDate = occurrenceDate
                                };

                                attendanceOccurrenceService.Add( attendanceOccurrence );
                                attendanceOccurrenceRockContext.SaveChanges();
                                this.OccurrenceId = attendanceOccurrence.Id;
                            }


                        }
                    }
                    else if ( _updatedObsoleteDidNotOccur )
                    {
                        // if they only changed DidNotOccur, but not any of the other obsolete attendanceoccurrence properties, just change the DidNotOccur on the existing AttendanceOccurrence record
                        if ( this.Occurrence != null )
                        {
                            this.Occurrence.DidNotOccur = _updatedObsoleteDidNotOccurValue;
                        }
                    }
                }
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
            if ( !DidAttend.HasValue )
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append( ( PersonAlias?.Person != null ) ? PersonAlias.Person.ToStringSafe() + " " : "" );
            sb.Append( DidAttend.Value ? "attended " : "did not attend " );
            sb.Append( Occurrence?.Group?.ToStringSafe() );
            if ( DidAttend.Value )
            {
                sb.AppendFormat( "on {0} at {1} ", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString() );

                var end = EndDateTime ?? Occurrence?.OccurrenceDate;
                if ( end.HasValue )
                {
                    sb.AppendFormat( "until {0} at {1} ", end.Value.ToShortDateString(), end.Value.ToShortTimeString() );
                }
            }

            if ( Occurrence?.Location != null )
            {
                sb.Append( "in " + Occurrence.Location.ToStringSafe() );
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
        /// Maybe
        /// </summary>
        Maybe = 2,

    }

    #endregion

}
