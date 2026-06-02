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

namespace Rock.ViewModels.Blocks.Rsvp.RsvpList
{
    /// <summary>
    /// 
    /// </summary>
    public class RsvpListBag
    {
        /// <summary>
        /// Unique identifier for the key field of uncreated occurrences. Occurrences are not added
        /// to the database if part of a schedule and no interactions have occurred.
        /// </summary>
        public string KeyField { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of RSVP.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Date of RSVP Event.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Id for the location used to create the occurrence if it doesn't exist.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Name of the Location Event is being held at.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// ID used for the schedule used to create the occurrence if it doesn't exist.
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Name of the Schedule associated with the RSVP.
        /// </summary>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Number of people invited to the event.
        /// </summary>
        public int InvitedCount { get; set; }

        /// <summary>
        /// Number of people that have accepted the invitation to the event.
        /// </summary>
        public int AcceptedCount { get; set; }

        /// <summary>
        /// Number of people that have declined the invitation to the event.
        /// </summary>
        public int DeclinedCount { get; set; }

        /// <summary>
        /// Number of people invited but who have not responded to the invitation to the event.
        /// </summary>
        public int NoResponseCount { get; set; }

        /// <summary>
        /// Gets the percentage of people that accepted the invitation to the event. Rounded to the nearest whole number.
        /// </summary>
        public int AcceptedPercentage
        {
            get
            {
                if ( InvitedCount == 0 )
                {
                    return 0;
                }
                return ( int ) ( Math.Round( ( decimal ) AcceptedCount / InvitedCount, 2 ) * 100 );
            }
        }

        /// <summary>
        /// Gets the percentage of people that declined the invitation to the event. Rounded to the nearest whole number.
        /// </summary>
        public int DeclinedPercentage
        {
            get
            {
                if ( InvitedCount == 0 )
                {
                    return 0;
                }
                return ( int ) ( Math.Round( ( decimal ) DeclinedCount / InvitedCount, 2 ) * 100 );
            }
        }
    }
}
