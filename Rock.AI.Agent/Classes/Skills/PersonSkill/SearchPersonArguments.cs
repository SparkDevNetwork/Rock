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

using System.ComponentModel;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Arguments for searching for a person by name.
    /// </summary>
    internal class SearchPersonArguments
    {
        /// <summary>
        /// Required. The full name to search for. This should be in the format of first name last name with an optional suffix.
        /// </summary>
        [Description( "Required. The full name to search for. This should be in the format of first name last name with an optional suffix." )]
        public string FullName { get; set; }

        /// <summary>
        /// The maximum number of results to return. Defaults to 10.
        /// </summary>
        public int MaxResults { get; set; } = 10;

        /// <summary>
        /// Optional. The campus key to filter the search results by.
        /// </summary>
        public string CampusKey { get; set; } = null;
    }
}