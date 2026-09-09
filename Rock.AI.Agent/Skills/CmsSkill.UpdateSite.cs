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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.CmsSkill;
using Rock.Configuration;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CmsSkill
{
    #region Tool(s)

    /// <summary>
    /// Updates an existing site. Sites cannot be created through the agent.
    /// </summary>
    /// <remarks>
    /// Creating a site provisions layouts, pages, and routes and is deliberately
    /// out of scope; this tool only edits an existing site. It can be renamed to
    /// AddOrUpdate in the future without breaking anything if creation is ever
    /// supported.
    /// </remarks>
    [Description( "Updates an existing site's settings. A new site cannot be created through the agent." )]
    [AgentToolPreamble( "Saving the site." )]
    [AgentUsage( "Pass only the properties to change. Sites cannot be created here." )]
    [AgentToolPrerequisite( "Call LookupSites to determine the siteIdKey, and GetSiteAvailableAttributes to determine which attributeValues the site accepts." )]
    [AgentToolGuid( "1F474109-57D9-4504-BFE0-EFFD30B57E93" )]
    public AgentToolResult UpdateSite(
        string siteIdKey,
        SetOrClear<string> name = null,
        SetOrClear<string> description = null,
        bool? isActive = null,
        [Description( "The name of the theme the site renders with." )]
        SetOrClear<string> theme = null,
        [Description( "The external URL the site is reached at." )]
        SetOrClear<string> externalUrl = null,
        [Description( "The page shown when someone visits the site without specifying one." )]
        SetOrClear<string> defaultPageIdKey = null,
        [Description( "The page people are sent to in order to log in." )]
        SetOrClear<string> loginPageIdKey = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var site = helper.GetRequiredEntity<Model.Site>( siteIdKey, checkSecurity: false );

        if ( site == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the LookupSites function to determine the available sites." );
        }

        if ( name?.ClearValue == true )
        {
            return Error( "The name of a site cannot be cleared." );
        }

        // Authorization is checked through the cache so inherited security participates.
        var siteCache = SiteCache.Get( site.Id, rockContext );

        if ( siteCache == null || !siteCache.IsAuthorized( Authorization.ADMINISTRATE, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to edit that site." );
        }

        helper.UpdateProperty( site, s => s.Name, name );
        helper.UpdateProperty( site, s => s.Description, description );
        helper.UpdateProperty( site, s => s.IsActive, isActive );
        helper.UpdateProperty( site, s => s.Theme, theme );
        helper.UpdateProperty( site, s => s.ExternalUrl, externalUrl );
        helper.UpdateNavigationProperty( site, s => s.DefaultPage, defaultPageIdKey );
        helper.UpdateNavigationProperty( site, s => s.LoginPage, loginPageIdKey );
        helper.SetAttributeValues( site, attributeValues );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        if ( !site.IsValid )
        {
            helper.AddError( site.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The site could not be saved." );

            return helper.ErrorResult;
        }

        // Saving is enough to refresh the cache. Site is ICacheable, and the context
        // updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
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
            .WithHistoryContent( new KeyNameResult { Id = site.Id, Name = site.Name } )
            .WithInstructions( "The site has been updated." );
    }

    #endregion
}
