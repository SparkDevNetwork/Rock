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

namespace Rock.ViewModels.Blocks.CheckIn.Manager.Roster
{
    /// <summary>
    /// Represents an attendee on the attendance record.
    /// </summary>
    public class RosterAttendeeBag
    {
        /// <summary>
        /// The encoded identifier for the person.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// The unique identifier for the person.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The person's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// The URL that points to the person's photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// The name of the person's parents.
        /// </summary>
        public string Parents { get; set; }

        /// <summary>
        /// The collection of badges that should be displayed for the attendee.
        /// </summary>
        public List<RosterAttendeeBadgeBag> Badges { get; set; }
    }
}
