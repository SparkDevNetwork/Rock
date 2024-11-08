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

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Attendance
    {
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
                // If the Campus is assigned, trust that over the CampusId value.
                int? campusId = Campus?.Id ?? CampusId;

                return CalculateIsCurrentlyCheckedIn( StartDateTime, EndDateTime, campusId, this.Occurrence?.Schedule );
            }
        }

        /// <summary>
        /// Calculates if an attendance would be considered checked-in based on specified parameters
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        public static bool CalculateIsCurrentlyCheckedIn( DateTime? startDateTime, DateTime? endDateTime, int? campusId, Schedule schedule )
        {
            if ( schedule == null )
            {
                return false;
            }

            // If person has checked-out, they are obviously not still checked in.
            if ( endDateTime.HasValue )
            {
                return false;
            }

            // We'll check start time against timezone next, but don't even bother if start date was more than 2 days ago.
            if ( startDateTime < RockDateTime.Now.AddDays( -2 ) )
            {
                return false;
            }

            // Get the current time (and adjust for a campus timezone).
            var currentDateTime = RockDateTime.Now;
            if ( campusId.HasValue )
            {
                var campus = CampusCache.Get( campusId.Value );
                if ( campus != null )
                {
                    currentDateTime = campus.CurrentDateTime;
                }
            }

            // Now that we know the correct time, make sure that the attendance is for today and previous to current time.
            if ( startDateTime < currentDateTime.Date || startDateTime > currentDateTime )
            {
                return false;
            }

            // Person is currently checked in, if the schedule for this attendance is still active.
            return schedule.WasScheduleOrCheckInActive( currentDateTime );
        }

        /// <summary>
        /// Calculates if an attendance would be considered checked-in based on specified parameters
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="campus">The campus the attendance record is for.</param>
        /// <param name="schedule">The schedule the attendance record is for.</param>
        /// <returns><c>true</c> if the attendance record should be considered as currently checked in; <c>false</c> otherwise.</returns>
        public static bool CalculateIsCurrentlyCheckedIn( DateTime? startDateTime, DateTime? endDateTime, CampusCache campus, NamedScheduleCache schedule )
        {
            if ( schedule == null )
            {
                return false;
            }

            // If person has checked-out, they are obviously not still checked in.
            if ( endDateTime.HasValue )
            {
                return false;
            }

            // We'll check start time against timezone next, but don't even bother if start date was more than 2 days ago.
            if ( startDateTime < RockDateTime.Now.AddDays( -2 ) )
            {
                return false;
            }

            // Get the current time (and adjust for a campus timezone).
            var currentDateTime = campus?.CurrentDateTime ?? RockDateTime.Now;

            // Now that we know the correct time, make sure that the attendance is for today and previous to current time.
            if ( startDateTime < currentDateTime.Date || startDateTime > currentDateTime )
            {
                return false;
            }

            // Person is currently checked in, if the schedule for this attendance is still active.
            return schedule.WasScheduleOrCheckInActive( currentDateTime );
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
                if ( this.ScheduledToAttend == true )
                {
                    verb = "is scheduled to attend";
                }
                else if ( this.DeclineReasonValueId.HasValue )
                {
                    verb = "has declined to attend";
                }
                else if ( this.RequestedToAttend == true )
                {
                    verb = "has been requested to attend";
                }
                else
                {
                    verb = "did not attend";
                }
            }

            sb.Append( $"{verb} " );
            sb.Append( Occurrence?.Group?.ToStringSafe() );

            if ( DidAttend.Value )
            {
                sb.AppendFormat( " on {0} at {1}", StartDateTime.ToShortDateString(), StartDateTime.ToShortTimeString() );

                var end = EndDateTime;
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
    }
}
