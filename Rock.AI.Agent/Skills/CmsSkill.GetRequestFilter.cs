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
    /// Gets a single request filter in full detail.
    /// </summary>
    /// <remarks>
    /// The matching criteria are reported as present or absent rather than
    /// returned; they are a filter tree edited through Rock's personalization
    /// screens.
    /// </remarks>
    [Description( "Gets a single request filter in full detail." )]
    [AgentPurpose( "Retrieves how a request filter is configured." )]
    [AgentToolPrerequisite( "Call LookupRequestFilters to determine the requestFilterIdKey." )]
    [AgentToolGuid( "D3BED3BD-C19A-4F40-86FE-FF6F81916920" )]
    public AgentToolResult GetRequestFilter( string requestFilterIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var requestFilter = helper.GetRequiredEntity<Model.RequestFilter>( requestFilterIdKey, checkSecurity: false );

        if ( requestFilter == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupRequestFilters function to determine the available request filters." );
        }

        var site = requestFilter.SiteId.HasValue
            ? SiteCache.Get( requestFilter.SiteId.Value, rockContext )
            : null;

        var result = new RequestFilterDetailResult
        {
            Id = requestFilter.Id,
            Guid = requestFilter.Guid,
            Name = requestFilter.Name,
            RequestFilterKey = requestFilter.RequestFilterKey,
            Site = KeyNameResult.FromCache( site ),
            IsActive = requestFilter.IsActive,
            HasFilterCriteria = requestFilter.FilterJson.IsNotNullOrWhiteSpace()
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( requestFilter.Id, requestFilter.Guid, requestFilter.Name ) );
    }

    #endregion
}
