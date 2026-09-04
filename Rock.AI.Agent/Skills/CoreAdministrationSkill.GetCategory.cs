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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single category in full detail.
    /// </summary>
    /// <remarks>
    /// This exists so the list tool can stay to identity only. Description,
    /// order, icon, color, and the child count are all paid for once here rather
    /// than once per row of a list.
    /// </remarks>
    [Description( "Gets a single category in full detail." )]
    [AgentPurpose( "Retrieves the settings of one category." )]
    [AgentToolPrerequisite( "Call ListCategories to determine the categoryIdKey." )]
    [AgentToolGuid( "9E3E5A2C-6D67-4F3B-B0AC-11A02C43B0E1" )]
    public AgentToolResult GetCategory( string categoryIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var category = helper.GetRequiredEntity<Rock.Model.Category>( categoryIdKey );

        if ( category == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCategories )} function to determine the available categories." );
        }

        var categoryCache = CategoryCache.Get( category.Id, AgentRequestContext.RockContext );

        var entityType = category.EntityTypeId > 0
            ? EntityTypeCache.Get( category.EntityTypeId, AgentRequestContext.RockContext )
            : null;

        var parentCategory = category.ParentCategoryId.HasValue
            ? CategoryCache.Get( category.ParentCategoryId.Value, AgentRequestContext.RockContext )
            : null;

        var result = new CategoryDetailResult
        {
            Id = category.Id,
            Guid = category.Guid,
            Name = category.Name,
            Description = category.Description,
            Order = category.Order,
            IconCssClass = category.IconCssClass,
            HighlightColor = category.HighlightColor,
            EntityType = KeyNameResult.FromCache( entityType ),
            ParentCategory = KeyNameResult.FromCache( parentCategory ),

            // Tells a caller whether descending further is worthwhile without
            // spending a second call to find out.
            ChildCategoryCount = categoryCache?.Categories?.Count ?? 0
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this category." );
        }

        // A reference rather than the whole result. The detail is answered into the
        // current turn; carrying it in history repeats the payload on every later
        // message for a record the caller can read again by key.
        return Success( result )
            .WithHistoryContent( new KeyNameResult( category.Id, category.Guid, category.Name ) );
    }

    #endregion
}
