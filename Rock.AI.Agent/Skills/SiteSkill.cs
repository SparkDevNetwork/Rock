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

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.SiteSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>

    [Description( "This skill provides an overview of site details and engagement across websites, mobile apps, and TV apps." )]
    [AgentSkillGuid( "613D7110-6453-4BAB-892B-064222F8397C" )]
    [EntityTypeGuid( "7A63570D-6FC3-4573-BDF2-89CFF605D5AB" )]
    internal sealed class SiteSkill : AgentSkillComponent
    {
        #region Fields

        private readonly ILogger<SiteSkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public SiteSkill( ILogger<SiteSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Skill Tools

        /// <summary>
        /// Retrieves all websites (sites) configured in Rock.
        /// Also persists a trimmed list (Id, Name, SiteType) into the agent’s session context
        /// under the key <c>"site-list"</c> so that subsequent calls can reference it.
        /// </summary>
        [Description( "Retrieves all configured websites in Rock." )]
        [AgentToolGuid( "6234BB68-99B8-4B7C-884D-0D760B1F081C" )]
        public RockToolResult LookupSites()
        {
            var sites = SiteCache.All( AgentRequestContext.RockContext );

            if ( !sites.Any() )
            {
                return RockToolResult.NoData();
            }

            // If the agent is running in an internal context (e.g., staff user), include inactive sites.
            var isInternal = AgentRequestContext.AudienceType == Enums.AI.Agent.AudienceType.Internal;
            sites = sites.Where( s => isInternal || s.IsActive ).ToList();

            var siteList = sites.Select( s => new SiteResult
            {
                IdKey = s.IdKey,
                Name = s.Name,
                Description = s.Description,
                SiteType = s.SiteType.ConvertToString( true ),
                ExternalUrl = s.ExternalUrl
            } ).ToList();

            // Store only essential properties in session context to keep it lean.
            var trimmedForHistory = siteList.Select( site => new
            {
                site.IdKey,
                site.Name,
                site.SiteType,
            } );

            return RockToolResult.Success( siteList )
                .WithHistoryContent( trimmedForHistory, "site-list" );
        }

        #endregion
    }
}