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

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// POCO result for campus team member information.
    /// </summary>
    internal class CampusTeamMemberResult
    {
        /// <summary>
        /// The role of the team member.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The team member.
        /// </summary>
        public PersonResult TeamMember { get; set; }
    }
}
