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

namespace Rock.Crm.ConnectionStatusChangeReport
{
    public class ConnectionStatusChangeEventInfo
    {
        /// <summary>
        /// The system-assigned unique identifier for the event.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The date and time on which the event occurred.
        /// </summary>
        public DateTime EventDate { get; set; }

        // <summary>
        // The system identifier of the person's current primary campus.
        // </summary>
        public int? CampusId { get; set; }

        // <summary>
        // The name of the person's current primary campus.
        // </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// The identifier of the person affected by this event.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// The person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Indicates if the person is deceased.
        /// </summary>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// The campus of which the person was a member at the end of the reporting period.
        /// </summary>
        public int NewConnectionStatusId { get; set; }

        /// <summary>
        /// The campus of which the person was a member at the end of the reporting period.
        /// </summary>
        public string NewConnectionStatusName { get; set; }

        /// <summary>
        /// The campus of which the person was a member at the start of the reporting period.
        /// </summary>
        public int? OldConnectionStatusId { get; set; }

        /// <summary>
        /// The campus of which the person was a member at the start of the reporting period.
        /// </summary>
        public string OldConnectionStatusName { get; set; }

        /// <summary>
        /// The name of the person who recorded this event.
        /// </summary>
        public string CreatedBy { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} [{OldConnectionStatusName} --> {NewConnectionStatusName}]";
        }
    }
}