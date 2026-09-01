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
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Updates an existing request filter's metadata.
    /// </summary>
    /// <remarks>
    /// This updates the request filter's name, site scope, and active state only.
    /// It does not create request filters, and it does not change the matching
    /// criteria, which are a filter tree edited through Rock's personalization
    /// screens.
    /// </remarks>
    [Description( "Updates an existing request filter's name, site scope, and active state. It does not create request filters or change the matching criteria." )]
    [AgentToolPreamble( "Saving the request filter." )]
    [AgentUsage( "Pass only the properties to change. Request filters cannot be created here, and their matching criteria are not editable through this tool." )]
    [AgentToolPrerequisite( "Call LookupRequestFilters to determine the requestFilterIdKey, and LookupSites for the siteIdKey." )]
    [AgentToolGuid( "AA86E787-74E1-4CB6-BB58-F1AB4261DC02" )]
    public AgentToolResult UpdateRequestFilter(
        string requestFilterIdKey,
        string name = null,
        [Description( "The site to scope the filter to. Clear to apply to all sites." )]
        SetOrClear<string> siteIdKey = null,
        bool? isActive = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var requestFilter = helper.GetRequiredEntity<Model.RequestFilter>( requestFilterIdKey, checkSecurity: false );

        if ( requestFilter == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupRequestFilters function to determine the available request filters." );
        }

        helper.UpdateProperty( requestFilter, f => f.Name, name );
        helper.UpdateProperty( requestFilter, f => f.IsActive, isActive );
        helper.UpdateNavigationProperty( requestFilter, f => f.Site, siteIdKey, checkSecurity: false );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupSites function to determine a valid siteIdKey." );
        }

        if ( !requestFilter.IsValid )
        {
            helper.AddError( requestFilter.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The request filter could not be saved." );

            return helper.ErrorResult;
        }

        // Saving is enough to refresh the cache. RequestFilter is ICacheable, and
        // the context updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
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
            .WithInstructions( "The request filter has been updated." )
            .WithHistoryContent( new KeyNameResult( requestFilter.Id, requestFilter.Guid, requestFilter.Name ) );
    }

    #endregion
}
