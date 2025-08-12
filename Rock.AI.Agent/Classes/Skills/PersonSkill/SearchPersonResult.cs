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

using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents a single result item returned from a person search.
    /// Includes basic identity fields, family relationships, campus information, and record statuses.
    /// </summary>
    public class SearchPersonResult
    {
        /// <summary>
        /// Opaque person identifier (Id Key). Use this value with other APIs/functions instead of the numeric Id.
        /// </summary>
        public string PersonKey { get; set; }

        /// <summary>
        /// Given/first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Preferred name/nickname. May match <see cref="FirstName"/>.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Family/surname (last name).
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Name suffix (e.g., Jr., III). Empty if none.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Human-friendly age classification (e.g., Child, Adult).
        /// </summary>
        public string AgeClassification { get; set; }

        /// <summary>
        /// Spouse's full name, if applicable; otherwise empty or null.
        /// </summary>
        public string SpouseName { get; set; }

        /// <summary>
        /// Child dependents for this person, if available.
        /// </summary>
        public List<PersonResult> Children { get; set; }

        /// <summary>
        /// Parent/guardian records for this person, if available.
        /// </summary>
        public List<PersonResult> Parents { get; set; }

        /// <summary>
        /// Display name of the associated campus.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// Rock connection status (display text).
        /// </summary>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Rock record status (display text).
        /// </summary>
        public string RecordStatus { get; set; }
    }
}
