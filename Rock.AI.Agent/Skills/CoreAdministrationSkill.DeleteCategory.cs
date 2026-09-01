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
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <remarks>
    /// A category with child categories is not deleted. The children must be
    /// removed first, one at a time, so nothing is deleted implicitly. System
    /// categories, and categories still referenced by other records, cannot be
    /// deleted at all.
    /// </remarks>
    [AgentGuardrail( "This permanently deletes the category. Confirm the exact category with the user before proceeding." )]
    [Description( "Deletes a category. A category that still contains child categories or is referenced by other records cannot be deleted." )]
    [AgentToolPreamble( "Deleting the category." )]
    [AgentToolPrerequisite( "Call ListCategories to determine the categoryIdKey." )]
    [AgentToolGuid( "B4A4C58D-A4F5-4AB5-BC30-FDFD150BECBB" )]
    public AgentToolResult DeleteCategory( string categoryIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var categoryService = new CategoryService( rockContext );

        var category = helper.GetRequiredEntity<Rock.Model.Category>( categoryIdKey );

        if ( category == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCategories )} function to determine the available categories." );
        }

        if ( !category.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete that category." );
        }

        if ( category.IsSystem )
        {
            return Error( "That category is part of Rock's core configuration and cannot be deleted." );
        }

        // A category with children is not deleted implicitly; the caller removes
        // each child first so every deletion is explicit.
        var hasChildCategories = categoryService.Queryable().Any( c => c.ParentCategoryId == category.Id );

        if ( hasChildCategories )
        {
            return Error( "That category still has child categories and cannot be deleted until they are removed." )
                .WithInstructions( $"Call the {nameof( ListCategories )} function with this categoryIdKey as the parentCategoryIdKey to enumerate the children, delete each one with {nameof( DeleteCategory )}, then delete this category." );
        }

        if ( !categoryService.CanDelete( category, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var name = category.Name;

        categoryService.Delete( category );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( $"The '{name}' category has been deleted." );
    }

    #endregion
}
