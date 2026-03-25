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

using System;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>

    [Description( "This skill provides an overview of connection features." )]
    [AgentSkillGuid( "02214EF2-B1AB-52A4-42FE-C722262925EE" )]
    [EntityTypeGuid( "FE485F5E-7422-78BB-4973-692975860393" )]
    internal sealed partial class ConnectionSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Connection Skill.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public ConnectionSkill( ILogger<ConnectionSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a result object to represent the full details of a
        /// connection request. This will not include every property, but will
        /// include the ones that would make sense to the language model.
        /// </summary>
        /// <param name="connectionRequest">The connection request to build the result from.</param>
        /// <returns>A result object that represents <paramref name="connectionRequest"/>.</returns>
        private ConnectionRequestResult GetFullConnectionRequestResult( ConnectionRequest connectionRequest )
        {
            var result = new ConnectionRequestResult
            {
                Id = connectionRequest.Id,
                Requester = PersonResult.Basic( connectionRequest.PersonAlias ),
                Comments = connectionRequest.Comments,
                ConnectionState = connectionRequest.ConnectionState,
                ConnectionStatus = new KeyNameResult { Id = connectionRequest.ConnectionStatus.Id, Name = connectionRequest.ConnectionStatus.Name },
                ConnectionOpportunity = new ConnectionOpportunityResult
                {
                    Id = connectionRequest.ConnectionOpportunity.Id,
                    Name = connectionRequest.ConnectionOpportunity.Name,
                    ConnectionType = new ConnectionTypeResult
                    {
                        Id = connectionRequest.ConnectionOpportunity.ConnectionType.Id,
                        Name = connectionRequest.ConnectionOpportunity.ConnectionType.Name
                    }
                },
                CreatedDateTime = connectionRequest.CreatedDateTime,
                ModifiedDateTime = connectionRequest.ModifiedDateTime,
                FollowupDate = connectionRequest.FollowupDate,
                Campus = connectionRequest.Campus != null ? new CampusResult { Id = connectionRequest.Campus.Id, Name = connectionRequest.Campus.Name } : null,
                AssignedGroup = connectionRequest.AssignedGroup != null ? new GroupResult { Id = connectionRequest.AssignedGroup.Id, Name = connectionRequest.AssignedGroup.Name } : null,
                Connector = PersonResult.Basic( connectionRequest.ConnectorPersonAlias ),
                Activities = connectionRequest.ConnectionRequestActivities
                    .OrderBy( a => a.CreatedDateTime )
                    .Select( a => new ConnectionRequestActivityResult
                    {
                        Id = a.Id,
                        ActivityType = new KeyNameResult { Id = a.ConnectionActivityType.Id, Name = a.ConnectionActivityType.Name },
                        Connector = PersonResult.Basic( a.ConnectorPersonAlias ),
                        CreatedDateTime = a.CreatedDateTime,
                        Note = a.Note,
                    } )
                    .ToList(),
                AttributeValues = connectionRequest.GetAttributeValueResults( AgentRequestContext ).ToList(),
            };

            result.Sanitize( AgentRequestContext );

            return result;
        }

        #endregion
    }
}
