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
using System.Linq;

using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;

using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Retrieves all configured websites in Rock." )]
    [AgentToolGuid( "6234BB68-99B8-4B7C-884D-0D760B1F081C" )]
    public AgentToolResult LookupSites()
    {
        var sites = SiteCache.All( AgentRequestContext.RockContext )
            .Where( s => s.IsActive || AgentRequestContext.AudienceType == AudienceType.Internal );

        var siteList = sites.Select( s => new SiteResult
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            SiteType = s.SiteType.ConvertToString( true ),
            ExternalUrl = s.ExternalUrl
        } ).ToList();

        if ( !siteList.Any() )
        {
            return NoData();
        }

        // Store only essential properties in session context to keep it lean.
        var trimmedForHistory = siteList.Select( site => new
        {
            site.IdKey,
            site.Name,
            site.SiteType,
        } );

        return Success( siteList )
            .WithHistoryContent( trimmedForHistory, "site-list" );
    }

    #endregion
}
