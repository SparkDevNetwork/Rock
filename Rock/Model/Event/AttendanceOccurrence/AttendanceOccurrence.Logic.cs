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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    public partial class AttendanceOccurrence
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether attendance was entered (based on presence of any attendee records).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance entered]; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        [NotMapped]
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
        public virtual double AttendanceRate
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

        #endregion Properties

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
    }
}
