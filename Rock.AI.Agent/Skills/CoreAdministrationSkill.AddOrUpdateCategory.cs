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
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds a category or updates an existing one.
    /// </summary>
    /// <remarks>
    /// A category's entity type is fixed at creation. It is required when adding
    /// and cannot be changed on update, because the category and everything filed
    /// under it belong to that entity type. A parent category must share the same
    /// entity type as the category being filed under it.
    /// </remarks>
    [Description( "Adds a new category or updates an existing one. A category is scoped to a single entity type, such as WorkflowType or DefinedValue." )]
    [AgentUsage( "entityTypeIdKey and name are required when adding. Supplying categoryIdKey updates that category and leaves any parameter you omit unchanged. A category cannot be moved to a different entity type." )]
    [AgentToolPrerequisite( "Call ListEntityTypes to determine the entityTypeIdKey, and ListCategories to determine the parentCategoryIdKey." )]
    [AgentToolGuid( "E5CD789E-183D-4D6D-A476-262E86BDD3C0" )]
    public AgentToolResult AddOrUpdateCategory(
        string categoryIdKey = null,
        [Description( "The entity type the category applies to. Required when adding; a category cannot be moved between entity types afterwards." )]
        string entityTypeIdKey = null,
        string name = null,
        SetOrClear<string> description = null,
        [Description( "The parent category to file this one under. Must share the same entity type. Omit for a root-level category." )]
        SetOrClear<string> parentCategoryIdKey = null,
        [Description( "The CSS class for the category's icon, such as 'ti ti-folder'." )]
        SetOrClear<string> iconCssClass = null,
        [Description( "The highlight color for the category, as a hex value such as '#4e9a06'." )]
        SetOrClear<string> highlightColor = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var categoryService = new CategoryService( rockContext );

        Rock.Model.Category category;
        var isNew = categoryIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            category = helper.GetRequiredEntity<Rock.Model.Category>( categoryIdKey );

            if ( category == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListCategories )} function to determine the available categories." );
            }

            // The entity type is fixed. A supplied type that disagrees is a mistake
            // worth naming rather than silently ignoring.
            if ( entityTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var suppliedEntityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey );

                if ( suppliedEntityType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
                }

                if ( suppliedEntityType.Id != category.EntityTypeId )
                {
                    return Error( "A category cannot be moved to a different entity type." );
                }
            }
        }
        else
        {
            if ( entityTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( entityTypeIdKey )} is required when adding a category." )
                    .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding a category." );
            }

            var entityType = helper.GetRequiredEntity<Rock.Model.EntityType>( entityTypeIdKey );

            if ( entityType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListEntityTypes )} function to determine the available entity types." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            category = rockContext.Set<Rock.Model.Category>().Create();

            category.EntityTypeId = entityType.Id;

            // File the new category after its existing siblings. Nothing supplies an
            // order, and a run of new categories all at zero would sort arbitrarily.
            var maxOrder = categoryService.Queryable()
                .Where( c => c.EntityTypeId == entityType.Id && c.ParentCategoryId == null )
                .Select( c => ( int? ) c.Order )
                .Max();

            category.Order = ( maxOrder ?? -1 ) + 1;

            categoryService.Add( category );
        }

        helper.UpdateProperty( category, c => c.Name, name );
        helper.UpdateProperty( category, c => c.Description, description );
        helper.UpdateProperty( category, c => c.IconCssClass, iconCssClass );
        helper.UpdateProperty( category, c => c.HighlightColor, highlightColor );
        helper.UpdateNavigationProperty( category, c => c.ParentCategory, parentCategoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCategories )} function to determine the available parent categories." );
        }

        // A child category must belong to the same entity type as its parent.
        if ( category.ParentCategoryId.HasValue )
        {
            var parentCategory = categoryService.Get( category.ParentCategoryId.Value );

            if ( parentCategory != null && parentCategory.EntityTypeId != category.EntityTypeId )
            {
                return Error( "The parent category belongs to a different entity type." )
                    .WithInstructions( $"Call the {nameof( ListCategories )} function with this category's entity type to determine a valid parent." );
            }
        }

        // The caller must be able to edit the category, which for a new one
        // resolves through its parent (or the default at the root of the chain).
        if ( !category.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to save that category." );
        }

        // Saving is enough to refresh the cache. Category is ICacheable, and the
        // context updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var categoryCache = CategoryCache.Get( category.Id, rockContext );
        var resultEntityType = EntityTypeCache.Get( category.EntityTypeId, rockContext );
        var resultParent = category.ParentCategoryId.HasValue
            ? CategoryCache.Get( category.ParentCategoryId.Value, rockContext )
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
            EntityType = KeyNameResult.FromCache( resultEntityType ),
            ParentCategory = KeyNameResult.FromCache( resultParent ),
            ChildCategoryCount = categoryCache?.Categories?.Count ?? 0
        };

        return Success( result )
            .WithInstructions( isNew
                ? "The category has been created."
                : "The category has been updated." )
            .WithHistoryContent( new KeyNameResult( category.Id, category.Guid, category.Name ) );
    }

    #endregion
}
