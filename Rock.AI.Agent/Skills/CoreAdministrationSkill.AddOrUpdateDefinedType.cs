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
    /// Adds a defined type or updates an existing one.
    /// </summary>
    /// <remarks>
    /// This creates the type shell only. Its values are added separately by
    /// <see cref="AddOrUpdateDefinedValue"/>, so building a populated type takes
    /// several calls. The field type used to edit a defined type's values is not
    /// exposed here: Rock's own defined type screen does not set it either, and a
    /// class name is never a parameter in this skill.
    /// </remarks>
    [Description( "Adds a new defined type or updates an existing one. This creates the type only; its values are added separately." )]
    [AgentUsage( "name is required when adding. Supplying definedTypeIdKey updates that defined type and leaves any parameter you omit unchanged." )]
    [AgentToolPrerequisite( "Call ListCategories with the DefinedType entity type to determine the categoryIdKey." )]
    [AgentToolGuid( "0AFC25AE-4C15-480D-8D4B-DFB5E970CB7D" )]
    public AgentToolResult AddOrUpdateDefinedType(
        string definedTypeIdKey = null,
        string name = null,
        SetOrClear<string> description = null,
        SetOrClear<string> categoryIdKey = null,
        SetOrClear<string> helpText = null,
        bool? isActive = null,
        bool? categorizedValuesEnabled = null,
        bool? enableSecurityOnValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var definedTypeService = new DefinedTypeService( rockContext );

        Rock.Model.DefinedType definedType;
        var isNew = definedTypeIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

            if ( definedType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
            }
        }
        else
        {
            if ( name.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( name )} is required when adding a defined type." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            definedType = rockContext.Set<Rock.Model.DefinedType>().Create();

            definedType.IsActive = true;

            /*
                8/27/26 - CLAUDE

                Seed FieldTypeId to the Text field type. The column is nullable and the
                Obsidian DefinedTypeDetail block never sets it (there is no UI for it),
                but MigrationHelper.AddDefinedType hardcodes the Text field type on every
                seeded defined type, so every row in a real database has FieldTypeId set
                to Text. Leaving it null would make an agent-created type the only row
                that differs.

                Reason: Match the value every existing defined type already carries.
            */
            definedType.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() )?.Id;

            definedTypeService.Add( definedType );
        }

        helper.UpdateProperty( definedType, dt => dt.Name, name );
        helper.UpdateProperty( definedType, dt => dt.Description, description );
        helper.UpdateProperty( definedType, dt => dt.HelpText, helpText );
        helper.UpdateProperty( definedType, dt => dt.IsActive, isActive );
        helper.UpdateProperty( definedType, dt => dt.CategorizedValuesEnabled, categorizedValuesEnabled );
        helper.UpdateProperty( definedType, dt => dt.EnableSecurityOnValues, enableSecurityOnValues );
        helper.UpdateNavigationProperty( definedType, dt => dt.Category, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCategories )} function with the DefinedType entity type to determine the available categories." );
        }

        // The caller must be able to edit the defined type, which for a new one
        // resolves to the default security at the root of the chain.
        if ( !definedType.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to save that defined type." );
        }

        // Saving is enough to refresh the cache. DefinedType is ICacheable, and the
        // context updates those entries as part of the save.
        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var category = definedType.CategoryId.HasValue
            ? CategoryCache.Get( definedType.CategoryId.Value, rockContext )
            : null;

        var result = new DefinedTypeDetailResult
        {
            Id = definedType.Id,
            Guid = definedType.Guid,
            Name = definedType.Name,
            Description = definedType.Description,
            HelpText = definedType.HelpText,
            Category = KeyNameResult.FromCache( category ),
            IsSystem = definedType.IsSystem,
            IsActive = definedType.IsActive,
            CategorizedValuesEnabled = definedType.CategorizedValuesEnabled ?? false,
            EnableSecurityOnValues = definedType.EnableSecurityOnValues,
            ValueCount = new DefinedValueService( rockContext )
                .Queryable()
                .Count( dv => dv.DefinedTypeId == definedType.Id )
        };

        return Success( result )
            .WithInstructions( isNew
                ? $"The defined type has been created. Add its values with {nameof( AddOrUpdateDefinedValue )}."
                : "The defined type has been updated." )
            .WithHistoryContent( new KeyNameResult( definedType.Id, definedType.Guid, definedType.Name ) );
    }

    #endregion
}
