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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.ConnectionSkill
{
    internal class ConnectionOpportunityResult : EntityResultBase
    {
        /// <summary>
        /// The name of the connection type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the connection type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A short summary of the connection opportunity.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The public name of the connection opportunity.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// The photo identifier of the connection opportunity.
        /// </summary>
        public int? PhotoId { get; set; }

        /// <summary>
        /// List of campuses that the connection opportunity is associated with.
        /// </summary>
        public List<CampusResult> Campuses { get; set; }

        /// <summary>
        /// The connection type that the opportunity belongs to.
        /// </summary>
        public ConnectionTypeResult ConnectionType { get; set; }
    }
}
