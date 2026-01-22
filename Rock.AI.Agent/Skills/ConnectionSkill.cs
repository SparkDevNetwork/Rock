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

        private ConnectionRequestResult GetResult( ConnectionRequest connectionRequest )
        {
            return GetResultExpression().Compile().Invoke( connectionRequest );
        }

        private Expression<Func<ConnectionRequest, ConnectionRequestResult>> GetResultExpression()
        {
            var isInternal = AgentRequestContext.AudienceType == AudienceType.Internal;

            return cr => new ConnectionRequestResult
            {
                Id = cr.Id,
                Requester = new PersonResult
                {
                    Id = cr.PersonAlias.Person.Id,
                    FirstName = cr.PersonAlias.Person.FirstName,
                    LastName = cr.PersonAlias.Person.LastName,
                    NickName = cr.PersonAlias.Person.NickName,
                    PhotoId = cr.PersonAlias.Person.PhotoId
                },
                Comments = cr.Comments,
                ConnectionState = new KeyNameResult { Id = ( int ) cr.ConnectionState, Name = cr.ConnectionState.ToString() },
                ConnectionStatus = new KeyNameResult { Id = cr.ConnectionStatus.Id, Name = cr.ConnectionStatus.Name },
                ConnectionOpportunity = new ConnectionOpportunityResult
                {
                    Id = cr.ConnectionOpportunity.Id,
                    Name = cr.ConnectionOpportunity.Name,
                    ConnectionType = new ConnectionTypeResult { Id = cr.ConnectionOpportunity.ConnectionType.Id, Name = cr.ConnectionOpportunity.ConnectionType.Name }
                },
                CreatedDateTime = cr.CreatedDateTime,
                ModifiedDateTime = cr.ModifiedDateTime,
                FollowupDate = cr.FollowupDate,
                Campus = cr.Campus != null ? new CampusResult { Id = cr.Campus.Id, Name = cr.Campus.Name } : null,
                AssignedGroup = cr.AssignedGroup != null ? new GroupResult { Id = cr.AssignedGroup.Id, Name = cr.AssignedGroup.Name } : null,
                Connector = cr.ConnectorPersonAlias != null ? new PersonResult
                {
                    Id = cr.ConnectorPersonAlias.Person.Id,
                    FirstName = cr.ConnectorPersonAlias.Person.FirstName,
                    LastName = cr.ConnectorPersonAlias.Person.LastName,
                    NickName = cr.ConnectorPersonAlias.Person.NickName,
                    PhotoId = cr.ConnectorPersonAlias.Person.PhotoId
                } : null,
                Activities = cr.ConnectionRequestActivities.Select( a => new ConnectionRequestActivityResult
                {
                    Id = a.Id,
                    ActivityType = new KeyNameResult { Id = a.ConnectionActivityTypeId, Name = a.ConnectionActivityType.Name },
                    Note = a.Note,
                    CreatedDateTime = a.CreatedDateTime,
                    Connector = a.ConnectorPersonAlias != null ? new PersonResult
                    {
                        Id = a.CreatedByPersonAlias.Person.Id,
                        FirstName = a.CreatedByPersonAlias.Person.FirstName,
                        LastName = a.CreatedByPersonAlias.Person.LastName,
                        NickName = a.CreatedByPersonAlias.Person.NickName,
                        PhotoId = a.CreatedByPersonAlias.Person.PhotoId
                    } : null
                } ).ToList(),
                Attributes = cr.ConnectionRequestAttributeValues
                        .Where( a => isInternal || a.IsPublic )
                        .Select( a => new AttributeResult
                        {
                            Id = a.AttributeId,
                            Value = a.PersistedTextValue,
                            Name = a.Name
                        } )
                        .ToList()
            };
        }

        #endregion
    }
}