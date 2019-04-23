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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for an Analytic Fact Attendance record in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceAttendance" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsSourceAttendance : AnalyticsBaseAttendance<AnalyticsSourceAttendance>
    {
        // intentionally blank
    }

    /// <summary>
    /// AnalyticSourceAttendance is a real table, and AnalyticsFactAttendance is a VIEW off of AnalyticSourceAttendance, so they share lots of columns
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Rock.Data.Entity{T}" />
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsBaseAttendance<T> : Entity<T>
        where T : AnalyticsBaseAttendance<T>, new()
    {
        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the attendance identifier from the original Attendance.Id value
        /// </summary>
        /// <value>
        /// The attendance identifier.
        /// </value>
        [DataMember]
        [Index( IsUnique = true )]
        public int AttendanceId { get; set; }

        /// <summary>
        /// Gets or sets the attendance date key which is the form YYYYMMDD, and is based off of Attendance.StartDateTime
        /// </summary>
        /// <value>
        /// The transaction date key.
        /// </value>
        [DataMember]
        public int AttendanceDateKey { get; set; }

        /// <summary>
        /// Gets or sets the attendance type identifier (which is a GroupType)
        /// The intention of this is to do the same thing that Attendance Analytics has in "Attendance Type" drop down list which comes from
        /// SELECT Id
        ///     FROM GroupType
        /// WHERE GroupTypePurposeValueId IN (
        ///     SELECT Id
        ///     FROM DefinedValue
        ///     WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01'-- GroupTypePurpose Checkin
        /// )
        /// </summary>
        /// <value>
        /// The attendance type identifier.
        /// </value>
        [DataMember]
        public int? AttendanceTypeId { get; set; }

        /// <summary>
        /// Number of Days since the last time this giving unit did a TransactionType that is the same as this TransactionType (and didattend = true)
        /// If IsFirstTransactionOfType is TRUE, DaysSinceLastTransactionOfType will be null 
        /// </summary>
        /// <value>
        /// The type of the days since last transaction of.
        /// </value>
        [DataMember]
        public int? DaysSinceLastAttendanceOfType { get; set; }

        /// <summary>
        /// This is true if this is the first time this giving unit did a transaction with this TransactionType (and didattend = true)
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is first transaction of type; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsFirstAttendanceOfType { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: This will either be 1 or 0 based on DidAttend. It is stored in the table as [Count] because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the person key which is the AnalyticsDimPersonHistorical record for the person at the time of the Transaction
        /// </summary>
        /// <value>
        /// The person key.
        /// </value>
        [DataMember]
        public int? PersonKey { get; set; }

        /// <summary>
        /// Gets or sets the person key which is the current AnalyticsDimPersonHistorical record for the person 
        /// </summary>
        /// <value>
        /// The current person key.
        /// </value>
        [DataMember]
        public int? CurrentPersonKey { get; set; }

        #endregion

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
        /// Gets or sets the Id of the Check-in Search Type Name <see cref="Rock.Model.DefinedValue"/> that was used to search for the person/family.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Name of the Check-in Search Type <see cref="Rock.Model.DefinedValue"/> that was used to search for the person/family.
        /// </value>
        [DataMember]
        public string SearchTypeName { get; set; }

        /// <summary>
        /// Gets or sets the start date and time/check in time
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date and time/check in date and time.
        /// </value>
        [DataMember]
        [Index( "IX_StartDateTime" )]
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
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime SundayDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the attendance date.
        /// </summary>
        /// <value>
        /// The attendance date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate AttendanceDate { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public virtual AnalyticsDimAttendanceLocation Location { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AnalyticsSourceAttendanceConfiguration : EntityTypeConfiguration<AnalyticsSourceAttendance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourceAttendanceConfiguration"/> class.
        /// </summary>
        public AnalyticsSourceAttendanceConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier TransactionDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasRequired( t => t.AttendanceDate ).WithMany().HasForeignKey( t => t.AttendanceDateKey ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for any of these since they are views
            this.HasOptional( t => t.Location ).WithMany().HasForeignKey( t => t.LocationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}