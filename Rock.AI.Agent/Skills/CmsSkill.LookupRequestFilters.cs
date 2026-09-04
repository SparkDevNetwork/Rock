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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Looks up the request filters configured in Rock.
    /// </summary>
    /// <remarks>
    /// A Lookup because the set is a bounded configuration surface returned whole.
    /// A request filter matches an incoming request (by device, query string, and
    /// so on) to drive personalization.
    /// </remarks>
    [Description( "Looks up the request filters configured in Rock. A request filter matches incoming requests (device, query string, and so on) for personalization." )]
    [AgentPurpose( "Finds a request filter so its configuration can be retrieved." )]
    [AgentToolGuid( "B1488B38-BCEC-4707-A4BC-B76E4ADF3A5E" )]
    public AgentToolResult LookupRequestFilters()
    {
        var rockContext = AgentRequestContext.RockContext;

        var results = RequestFilterCache.All( rockContext )
            .OrderBy( f => f.Name )
            .Select( f =>
            {
                var site = f.SiteId.HasValue ? SiteCache.Get( f.SiteId.Value, rockContext ) : null;

                return new RequestFilterResult
                {
                    Id = f.Id,
                    Guid = f.Guid,
                    Name = f.Name,
                    RequestFilterKey = f.RequestFilterKey,
                    Site = KeyNameResult.FromCache( site )
                };
            } )
            .ToList();

        if ( !results.Any() )
        {
            return NoData()
                .WithInstructions( "No request filters are configured." );
        }

        return Success( results )
            .WithHistoryKey( "request-filters" );
    }

    #endregion
}
