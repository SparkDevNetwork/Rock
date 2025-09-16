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
using System.Diagnostics.CodeAnalysis;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.ConnectionSkill
{
    /// <summary>
    /// POCO result for a note.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConnectionRequestResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the connection opportunity that the connection request is associated with.
        /// </summary>
        public ConnectionOpportunityResult ConnectionOpportunity { get; set; }

        /// <summary>
        /// Gets or sets the comments associated with the connection request.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the status of the connection request.
        /// </summary>
        public KeyNameResult ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the state of the connection request.
        /// </summary>
        public KeyNameResult ConnectionState { get; set; }

        /// <summary>
        /// Gets or sets the date that the connection request is scheduled for a follow-up.
        /// </summary>
        public DateTime? FollowupDate { get; set; }

        /// <summary>
        /// Gets or sets the campus that the connection request is associated with.
        /// </summary>
        public CampusResult Campus { get; set; }

        /// <summary>
        /// Gets or sets the group that the connection request is assigned to.
        /// </summary>
        public GroupResult AssignedGroup { get; set; }

        /// <summary>
        /// Gets or sets the connector of the connection request.
        /// </summary>
        public PersonResult Connector { get; set; }

        /// <summary>
        /// Gets or sets the list of activities that have been performed on this connection request.
        /// </summary>
        public List<ConnectionRequestActivityResult> Activities { get; set; }

        /// <summary>
        /// Gets or sets the author of the note.
        /// </summary>
        public PersonResult Requester { get; set; }

        /// <summary>
        /// The URL to the request.
        /// </summary>
        public string Url => $"/connectionrequest/{Id}";
    }
}
