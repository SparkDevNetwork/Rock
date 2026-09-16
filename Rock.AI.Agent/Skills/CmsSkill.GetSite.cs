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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    [Description( "Gets the details of a single site, including its theme, default page, and login page." )]
    [AgentToolGuid( "16C84C00-62DC-4AE9-9A85-F7CDE7D20FC8" )]
    public AgentToolResult GetSite( string siteIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // Match LookupSites: inactive sites are not visible to external audiences.
        if ( !site.IsActive && AgentRequestContext.AudienceType != AudienceType.Internal )
        {
            return NoData();
        }

        var defaultPage = site.DefaultPageId.HasValue
            ? PageCache.Get( site.DefaultPageId.Value, rockContext )
            : null;
        var loginPage = site.LoginPageId.HasValue
            ? PageCache.Get( site.LoginPageId.Value, rockContext )
            : null;

        return Success( new SiteResult
        {
            Id = site.Id,
            Guid = site.Guid,
            Name = site.Name,
            Description = site.Description,
            SiteType = site.SiteType.ConvertToString( true ),
            ExternalUrl = site.ExternalUrl,
            IsActive = site.IsActive,
            Theme = site.Theme,
            DefaultPage = KeyNameResult.FromCache( defaultPage ),
            LoginPage = KeyNameResult.FromCache( loginPage ),
            AttributeValues = site.GetAttributeValueResults( AgentRequestContext ).ToList()
        } )
            .WithHistoryContent( new KeyNameResult
            {
                Id = site.Id,
                Name = site.Name
            } );
    }

    #endregion
}
