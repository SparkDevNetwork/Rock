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
    /// Lists the Lava shortcodes configured in Rock.
    /// </summary>
    /// <remarks>
    /// A List rather than a Lookup because churches create shortcodes freely, so
    /// the set grows with data. It reads from the cache, so it pages by number: the
    /// collection is materialized already and there is nothing to round-trip.
    /// </remarks>
    [Description( "Lists the Lava shortcodes configured in Rock, such as accordion or chart. A shortcode is a reusable Lava tag invoked with {[ tagName ]}." )]
    [AgentPurpose( "Finds a shortcode and the tag used to invoke it." )]
    [AgentToolGuid( "A31839EC-F2AA-4A6A-AE7C-24EA0B311141" )]
    public AgentToolResult ListShortcodes( string partialName = null, string categoryIdKey = null, bool includeInactive = false, int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var shortcodes = LavaShortcodeCache.All( AgentRequestContext.RockContext ).AsEnumerable();

        if ( !includeInactive )
        {
            shortcodes = shortcodes.Where( s => s.IsActive );
        }

        if ( partialName.IsNotNullOrWhiteSpace() )
        {
            shortcodes = shortcodes.Where( s => s.Name.ContainsIgnoreCase( partialName ) || s.TagName.ContainsIgnoreCase( partialName ) );
        }

        if ( categoryIdKey.IsNotNullOrWhiteSpace() )
        {
            var category = helper.GetOptionalEntity<Model.Category>( categoryIdKey );

            if ( helper.HasErrors )
            {
                return helper.ErrorResult;
            }

            if ( category != null )
            {
                shortcodes = shortcodes.Where( s => s.CategoryIds != null && s.CategoryIds.Contains( category.Id ) );
            }
        }

        var ordered = shortcodes
            .OrderBy( s => s.Name )
            .ThenBy( s => s.Id )
            .Select( s => new ShortcodeResult
            {
                Id = s.Id,
                Guid = s.Guid,
                Name = s.Name,
                TagName = s.TagName,
                TagType = s.TagType.ConvertToString(),
                Description = s.Description.IsNullOrWhiteSpace() ? null : s.Description,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem
            } )
            .AsQueryable();

        var page = helper.GetPaginatedItems( ordered, pageNumber );

        var historyPage = page.WithItems( page.Items
            .Select( s => new KeyNameResult { Id = s.Id, Guid = s.Guid, Name = s.Name } ) );

        return helper.GetPaginatedResult( page, historyPage );
    }

    #endregion
}
