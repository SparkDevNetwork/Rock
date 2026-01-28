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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.ConnectionSkill;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

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

        private readonly ILogger<ConnectionSkill> _logger;

        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Connection Skill.
        /// </summary>
        /// <param name="rockContextFactory">Factory to create rock contexts.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public ConnectionSkill( IRockContextFactory rockContextFactory, ILogger<ConnectionSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Methods

        private ConnectionRequestResult GetFullConnectionRequestResult( ConnectionRequest connectionRequest )
        {
            return new ConnectionRequestResult
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
                AttributeValues = connectionRequest.ConnectionRequestAttributeValues.GetAttributeValueResults( AgentRequestContext ).ToList(),
            };
        }

        #endregion
    }
}
