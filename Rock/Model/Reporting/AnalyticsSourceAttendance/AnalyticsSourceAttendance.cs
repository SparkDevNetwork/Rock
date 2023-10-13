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
using Rock.Data;
using Rock.Utility;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for an Analytic Fact Attendance record in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourceAttendance" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "BCE52831-6FEF-4521-9E4A-AE5C29F20E2F")]
    public class AnalyticsSourceAttendance : AnalyticsBaseAttendance<AnalyticsSourceAttendance>
    {
        // intentionally blank.  See AnalyticsBaseAttendance.
    }

    #region Entity Configuration

    /// <summary>
    /// AnalyticsSourceAttendance Configuration Class.
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

    #endregion Entity Configuration
}