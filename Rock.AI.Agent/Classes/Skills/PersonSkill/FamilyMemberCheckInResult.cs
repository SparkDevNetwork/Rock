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
//
using System;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents the check-in details of a family member.
    /// </summary>
    /// <remarks>
    /// This class contains information about the check-in, including the date, location, schedule, and associated person.
    /// </remarks>
    internal class FamilyMemberCheckInResult
    {
        /// <summary>
        /// Gets or sets the date of the check-in.
        /// </summary>
        /// <value>
        /// The date of the check-in.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the campus associated with the check-in.
        /// </summary>
        /// <value>
        /// The campus associated with the check-in.
        /// </value>
        public KeyNameResult Campus { get; set; }

        /// <summary>
        /// Gets or sets the location of the check-in.
        /// </summary>
        /// <value>
        /// The location of the check-in.
        /// </value>
        public KeyNameResult Location { get; set; }

        /// <summary>
        /// Gets or sets the schedule of the check-in.
        /// </summary>
        /// <value>
        /// The schedule of the check-in.
        /// </value>
        public KeyNameResult Schedule { get; set; }

        /// <summary>
        /// Gets or sets the group associated with the check-in.
        /// </summary>
        /// <value>
        /// The group associated with the check-in.
        /// </value>
        public KeyNameResult Group { get; set; }

        /// <summary>
        /// Gets or sets the area associated with the check-in.
        /// </summary>
        /// <value>
        /// The area associated with the check-in.
        /// </value>
        public KeyNameResult Area { get; set; }

        /// <summary>
        /// Gets or sets the person associated with the check-in.
        /// </summary>
        /// <value>
        /// The person associated with the check-in.
        /// </value>
        public KeyNameResult Person { get; set; }

        /// <summary>
        /// Gets or sets the root-level group associated with the check-in.
        /// </summary>
        /// <value>
        /// The root-level group associated with the check-in.
        /// </value>
        public KeyNameResult RootLevelGroup { get; set; }
    }
}
