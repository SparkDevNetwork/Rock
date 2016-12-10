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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsFactAttendance is SQL View based on AnalyticsSourceAttendance
    /// </summary>
    [Table( "AnalyticsFactAttendance" )]
    [DataContract]
    public class AnalyticsFactAttendance : AnalyticsBaseAttendance<AnalyticsFactAttendance>
    {
        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the attendance type name (which is a GroupType)
        /// The intention of this is to do the same thing that Attendance Analytics has in "Attendance Type" drop down list which comes from
        /// SELECT Name
        ///     FROM GroupType
        /// WHERE GroupTypePurposeValueId IN (
        ///     SELECT Id
        ///     FROM DefinedValue
        ///     WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01'-- GroupTypePurpose Checkin
        /// )
        /// </summary>
        /// <value>
        /// The attendance type name
        /// </value>
        [DataMember]
        public string AttendanceTypeName { get; set; }

        /// <summary>
        /// Gets or sets the person key (reference to AnalyticsDimPerson)
        /// </summary>
        /// <value>
        /// The person key.
        /// </value>
        [DataMember]
        public int PersonKey { get; set; }

        #endregion

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        [DataMember]
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus.
        /// </summary>
        /// <value>
        /// The name of the campus.
        /// </value>
        [DataMember]
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        /// <value>
        /// The name of the schedule.
        /// </value>
        [DataMember]
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        [DataMember]
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type.
        /// </summary>
        /// <value>
        /// The name of the group type.
        /// </value>
        [DataMember]
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the device.
        /// </summary>
        /// <value>
        /// The name of the device.
        /// </value>
        [DataMember]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the search type.
        /// </summary>
        /// <value>
        /// The name of the search type.
        /// </value>
        [DataMember]
        public string SearchTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the search result group.
        /// </summary>
        /// <value>
        /// The name of the search result group.
        /// </value>
        [DataMember]
        public string SearchResultGroupName { get; set; }

        /// <summary>
        /// Gets or sets the attendance code.
        /// </summary>
        /// <value>
        /// The attendance code.
        /// </value>
        [DataMember]
        public string AttendanceCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier.
        /// </summary>
        /// <value>
        /// The name of the qualifier.
        /// </value>
        [DataMember]
        public string QualifierName { get; set; }

        /// <summary>
        /// Gets or sets the RSVP status.
        /// </summary>
        /// <value>
        /// The RSVP status.
        /// </value>
        [DataMember]
        public string RSVPStatus { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AnalyticsFactAttendanceConfiguration : EntityTypeConfiguration<AnalyticsFactAttendance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsFactAttendanceConfiguration"/> class.
        /// </summary>
        public AnalyticsFactAttendanceConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier TransactionDates that aren't in the AnalyticsDimDate table
            // and so that the AnalyticsDimDate can be rebuilt from scratch as needed
            this.HasRequired( t => t.AttendanceDate ).WithMany().HasForeignKey( t => t.AttendanceDateKey ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for any of these since they are views
            this.HasOptional( t => t.AttendanceType ).WithMany().HasForeignKey( t => t.AttendanceTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Campus ).WithMany().HasForeignKey( t => t.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Device ).WithMany().HasForeignKey( t => t.DeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Group ).WithMany().HasForeignKey( t => t.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Location ).WithMany().HasForeignKey( t => t.LocationId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Schedule ).WithMany().HasForeignKey( t => t.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SearchType ).WithMany().HasForeignKey( t => t.SearchTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}