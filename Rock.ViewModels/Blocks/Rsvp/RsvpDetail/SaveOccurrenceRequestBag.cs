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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Rsvp.RsvpDetail
{
    /// <summary>
    /// Payload sent to the SaveOccurrence block action.
    /// </summary>
    public class SaveOccurrenceRequestBag
    {
        /// <summary>
        /// Gets or sets the optional name describing the occurrence.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the occurrence date.
        /// </summary>
        public DateTime? OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the schedule selection (Value = Schedule.Guid).
        /// </summary>
        public ListItemBag Schedule { get; set; }

        /// <summary>
        /// Gets or sets the location selection (Value = Location.Guid).
        /// </summary>
        public ListItemBag Location { get; set; }

        /// <summary>
        /// Gets or sets the custom Accept message.
        /// </summary>
        public string AcceptMessage { get; set; }

        /// <summary>
        /// Gets or sets the custom Decline message.
        /// </summary>
        public string DeclineMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether decline reasons should be collected.
        /// </summary>
        public bool ShowDeclineReasons { get; set; }

        /// <summary>
        /// Gets or sets the decline reasons selected as available for this occurrence.
        /// Each value is a DefinedValue Guid.
        /// </summary>
        public List<string> AvailableDeclineReasonGuids { get; set; }
    }
}
