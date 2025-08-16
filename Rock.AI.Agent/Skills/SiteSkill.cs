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
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.SiteSkill;
using Rock.Data;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>

    [AgentSkillGuid( "613D7110-6453-4BAB-892B-064222F8397C" )]
    [EntityTypeGuid( "7A63570D-6FC3-4573-BDF2-89CFF605D5AB" )]
    internal sealed class SiteSkill : AgentSkillComponent
    {
        #region Fields

        private readonly RockContext _rockContext;
        private readonly ILogger<SiteSkill> _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteSkill"/> class.
        /// </summary>
        /// <param name="rockContext">Rock data context used for database access.</param>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public SiteSkill( RockContext rockContext, ILogger<SiteSkill> logger )
        {
            _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Agent Functions

        /// <summary>
        /// Retrieves all websites (sites) configured in Rock.
        /// Also persists a trimmed list (Id, Name, SiteType) into the agent’s session context
        /// under the key <c>"site-list"</c> so that subsequent calls can reference it.
        /// </summary>
        [KernelFunction( "LookupSites" )]
        [UserDescription( "Retrieves all configured websites in Rock." )]
        [AgentFunctionGuid( "6234BB68-99B8-4B7C-884D-0D760B1F081C" )]
        public async Task<RockFunctionResult> LookupSites()
        {
            var sites = SiteCache.All();

            if ( !sites.Any() )
            {
                return RockFunctionResult.NoData();
            }

            var siteList = sites.Select( s => new SiteResult
            {
                Key = s.IdKey,
                Name = s.Name,
                Description = s.Description,
                SiteType = s.SiteType.ConvertToString( true ),
                ExternalUrl = s.ExternalUrl
            } ).ToList();

            // Store only essential properties in session context to keep it lean.
            var trimmedForContext = siteList.Select( site => new
            {
                site.Key,
                site.Name,
                site.SiteType,
                site
            } );

            await AgentRequestContext.ChatAgent.AddSessionContextAsync( "site-list", trimmedForContext.ToJson() );

            return RockFunctionResult.Success( siteList );
        }

        #endregion
    }
}