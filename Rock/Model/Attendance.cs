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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;

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
        /// Gets or sets the Id of the <see cref="Rock.Model.OccurrenceAttendance"/> that the attendance is for. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.OccurrenceAttendance"/> that the attendance is for. 
        /// </value>
        [DataMember]
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
        [Obsolete( "Use Occurrence.GroupId instead", false )]
        public int? GroupId => Occurrence?.GroupId;

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.Location"/> that the individual attended/checked in to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [Obsolete( "Use Occurrence.GroupId instead", false )]
        public int? LocationId => Occurrence?.LocationId;

        /// <summary>
        /// Gets the Id of the schedule that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the schedule that was checked in to.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [Obsolete( "Use Occurrence.ScheduleId instead", false )]

        public int? ScheduleId => Occurrence?.ScheduleId;

        /// <summary>
        /// Gets or sets the did not occur.
        /// </summary>
        /// <value>
        /// The did not occur.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [Obsolete( "Use Occurrence.DidNotOccur instead", false )]
        public bool? DidNotOccur => Occurrence?.DidNotOccur;

        /// <summary>
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [LavaInclude]
        [NotMapped]
        [Obsolete( "Use Occurrence.SundayDate instead", false )]
        public DateTime SundayDate => Occurrence.SundayDate;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that was attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that was attended.
        /// </value>
        [LavaInclude]
        [Obsolete( "Use Occurrence.Group instead", false )]
        public virtual Group Group => Occurrence?.Group;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [LavaInclude]
        [Obsolete( "Use Occurrence.Location instead", false )]
        public virtual Location Location => Occurrence?.Location;

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaInclude]
        [Obsolete( "Use Occurrence.Schedule instead", false )]
        public virtual Schedule Schedule => Occurrence?.Schedule;

        #endregion

        #region Public Methods

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var transaction = new Rock.Transactions.GroupAttendedTransaction( entry );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!DidAttend.HasValue) return string.Empty;

            var sb = new StringBuilder();
            sb.Append( ( PersonAlias?.Person != null ) ? PersonAlias.Person.ToStringSafe() + " " : "" );
            sb.Append( DidAttend.Value ? "attended " : "did not attend " );
            sb.Append( Occurrence?.Group?.ToStringSafe() );
            if (DidAttend.Value)
            {
                sb.AppendFormat("on {0} at {1} ", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString());

                var end = EndDateTime ?? Occurrence?.OccurrenceDate;
                if (end.HasValue)
                {
                    sb.AppendFormat("until {0} at {1} ", end.Value.ToShortDateString(), end.Value.ToShortTimeString());
                }
            }

            if (Occurrence?.Location != null)
            {
                sb.Append("in " + Occurrence.Location.ToStringSafe());
            }

            return sb.ToString().Trim();
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
