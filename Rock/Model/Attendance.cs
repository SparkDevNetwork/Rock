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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Text;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance where a <see cref="Rock.Model.Person"/> who attended or was scheduled to attend a group or event.
    /// This can be used for attendee/volunteer check-in, group attendance, etc.
    /// </summary>
    [Table( "Attendance" )]
    [DataContract]
    public partial class Attendance : Model<Attendance>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that the individual attended/checked in to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> that was checked in to.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that the individual attended/checked in to. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that was checked in to.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the schedule that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the schedule that was checked in to.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that the <see cref="Rock.Model.Person"/> checked in to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that was checked in to.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that attended/checked in to the <see cref="Rock.Model.Group"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who attended/checked in.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

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
        /// Gets or sets the start date and time/check in time
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date and time/check in date and time.
        /// </value>
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time/check out date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the end date and time/check out time.
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
        /// Gets or sets the did not occur.
        /// </summary>
        /// <value>
        /// The did not occur.
        /// </value>
        [DataMember]
        public bool? DidNotOccur { get; set; }

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
        /// Gets or sets the <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that was attended.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that was attended.
        /// </value>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

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

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();

            if ( DidAttend.HasValue )
            {
                sb.Append( ( PersonAlias != null && PersonAlias.Person != null ) ? PersonAlias.Person.ToStringSafe() + " " : "" );
                sb.Append( DidAttend.Value ? "attended " : "did not attend " );
                sb.Append( Group != null ? Group.ToStringSafe() + " " : "" );
                if ( DidAttend.HasValue && DidAttend.Value )
                {
                    sb.AppendFormat( "on {0} at {1} ", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString() );
                    if ( EndDateTime.HasValue )
                    {
                        sb.AppendFormat( "until {0} at {1} ", EndDateTime.Value.ToShortDateString(), EndDateTime.Value.ToShortTimeString() );
                    }
                }

                sb.Append( Location != null ? "in " + Location.ToStringSafe() : "" );
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
            this.HasOptional( a => a.Location ).WithMany().HasForeignKey( p => p.LocationId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Device ).WithMany().HasForeignKey( d => d.DeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SearchTypeValue ).WithMany().HasForeignKey( v => v.SearchTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Qualifier ).WithMany().HasForeignKey( p => p.QualifierValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AttendanceCode ).WithMany( c => c.Attendances ).HasForeignKey( a => a.AttendanceCodeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// For Attendance Reporting, summarize counts by Week, Month, or Year
    /// </summary>
    public enum AttendanceGroupBy
    {
        /// <summary>
        /// Week (using RockDateTime.FirstDayOfWeek to determine week)
        /// </summary>
        Week = 0,

        /// <summary>
        /// Month
        /// </summary>
        Month = 1,

        /// <summary>
        /// Year
        /// </summary>
        Year = 2
    }

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
        /// Each campus (from Attendance.CampusId) is it's own series
        /// </summary>
        Campus = 2,

        /// <summary>
        /// Each schedule (from Attendance.ScheduleId) is it's own series
        /// </summary>
        Schedule = 3
    }

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
