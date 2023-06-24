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

using Rock.ViewModels.Utility;
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestBoard
{
    /// <summary>
    /// A bag that contains connection request information for the connection request board.
    /// </summary>
    public class ConnectionRequestBoardConnectionRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the "requester" person.
        /// </summary>
        public ListItemBag Requester { get; set; }

        /// <summary>
        /// Gets or sets the "connector" person alias identifier.
        /// </summary>
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the connection state identifier.
        /// </summary>
        public int ConnectionStateId { get; set; }

        /// <summary>
        /// Gets or sets the connection status identifier.
        /// </summary>
        public int ConnectionStatusId { get; set; }

        /// <summary>
        /// Gets or sets the placement group identifier.
        /// </summary>
        public int? PlacementGroupId { get; set; }

        /// <summary>
        /// Gets or sets the placement group member role identifier.
        /// </summary>
        public int? PlacementGroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the placement group member status identifier.
        /// </summary>
        public int? PlacementGroupMemberStatusId { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the follow-up date.
        /// </summary>
        public DateTimeOffset? FollowUpDate { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
