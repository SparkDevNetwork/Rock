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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// POCO result for a note.
    /// </summary>
    internal class PrayerRequestResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the category of the prayer request.
        /// </summary>
        public KeyNameResult Category { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the date time the note was entered.
        /// </summary>
        public DateTime EnteredDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this prayer request is urgent.
        /// </summary>
        public bool? IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this prayer request has been approved.
        /// </summary>
        public bool? IsApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is prayer request is public.
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this prayer request is active.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the number of times this prayer request has been prayed for.
        /// </summary>
        public int? PrayerCount { get; set; }

        /// <summary>
        /// Gets or sets the author of the note.
        /// </summary>
        public PersonResult RequestedByPerson { get; set; }
    }
}
