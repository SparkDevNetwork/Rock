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
    /// Adds a value to a defined type or updates an existing value.
    /// </summary>
    /// <remarks>
    /// A value's defined type is fixed at creation. It is required when adding and
    /// cannot be changed on update, because moving a value between types would
    /// invalidate every attribute qualified by the old type. Attribute values set
    /// here are the value's own attributes, which are qualified by the parent
    /// defined type, so the type must be resolved before they are applied.
    /// </remarks>
    [Description( "Adds a value to a defined type or updates an existing defined value." )]
    [AgentUsage( "definedTypeIdKey and value are required when adding. Supplying definedValueIdKey updates that value and leaves any parameter you omit unchanged. A value cannot be moved to a different defined type." )]
    [AgentToolPrerequisite( "Call ListDefinedTypes to determine the definedTypeIdKey." )]
    [AgentToolGuid( "0132DCB9-8DEE-4B84-A9D5-6E88088851CC" )]
    public AgentToolResult AddOrUpdateDefinedValue(
        string definedValueIdKey = null,
        [Description( "The defined type the value belongs to. Required when adding; a value cannot be moved between types afterwards." )]
        string definedTypeIdKey = null,
        string value = null,
        SetOrClear<string> description = null,
        [Description( "The category the value is filed under. Only meaningful when the parent defined type has categorized values enabled." )]
        SetOrClear<string> categoryIdKey = null,
        bool? isActive = null,
        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var definedValueService = new DefinedValueService( rockContext );

        Rock.Model.DefinedValue definedValue;
        var isNew = definedValueIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            definedValue = helper.GetRequiredEntity<Rock.Model.DefinedValue>( definedValueIdKey );

            if ( definedValue == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListDefinedValues )} function to determine the available defined values." );
            }

            // The parent type is fixed. A supplied type that disagrees is a mistake
            // worth naming rather than silently ignoring.
            if ( definedTypeIdKey.IsNotNullOrWhiteSpace() )
            {
                var suppliedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

                if ( suppliedType == null )
                {
                    return helper.ErrorResult
                        .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
                }

                if ( suppliedType.Id != definedValue.DefinedTypeId )
                {
                    return Error( "A defined value cannot be moved to a different defined type." );
                }
            }
        }
        else
        {
            if ( definedTypeIdKey.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( definedTypeIdKey )} is required when adding a defined value." )
                    .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
            }

            if ( value.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( value )} is required when adding a defined value." );
            }

            var definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

            if ( definedType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            definedValue = rockContext.Set<Rock.Model.DefinedValue>().Create();

            definedValue.DefinedTypeId = definedType.Id;
            definedValue.IsActive = true;

            // File the new value after its existing siblings. Nothing supplies an
            // order, and a run of new values all at zero would sort arbitrarily.
            var maxOrder = definedValueService.Queryable()
                .Where( dv => dv.DefinedTypeId == definedType.Id )
                .Select( dv => ( int? ) dv.Order )
                .Max();

            definedValue.Order = ( maxOrder ?? -1 ) + 1;

            definedValueService.Add( definedValue );
        }

        helper.UpdateProperty( definedValue, dv => dv.Value, value );
        helper.UpdateProperty( definedValue, dv => dv.Description, description );
        helper.UpdateProperty( definedValue, dv => dv.IsActive, isActive );
        helper.UpdateNavigationProperty( definedValue, dv => dv.Category, categoryIdKey );

        // Attribute values are qualified by the parent defined type, which is set
        // above before this runs, so LoadAttributes resolves the right set.
        helper.SetAttributeValues( definedValue, attributeValues );

        // The caller must be able to edit the defined value, which resolves
        // through its parent defined type (or the default at the root of the chain).
        if ( !definedValue.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to save that defined value." );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var parentType = DefinedTypeCache.Get( definedValue.DefinedTypeId, rockContext );

        var category = definedValue.CategoryId.HasValue
            ? CategoryCache.Get( definedValue.CategoryId.Value, rockContext )
            : null;

        definedValue.LoadAttributes( rockContext );

        var result = new DefinedValueDetailResult
        {
            Id = definedValue.Id,
            Guid = definedValue.Guid,
            Value = definedValue.Value,
            Description = definedValue.Description,
            Order = definedValue.Order,
            IsActive = definedValue.IsActive,
            DefinedType = KeyNameResult.FromCache( parentType ),
            Category = KeyNameResult.FromCache( category ),
            AttributeValues = definedValue.GetAttributeValueResults( AgentRequestContext ).ToList()
        };

        result.Sanitize( AgentRequestContext );

        return Success( result )
            .WithInstructions( isNew
                ? "The defined value has been created."
                : "The defined value has been updated." )
            .WithHistoryContent( new KeyNameResult( definedValue.Id, definedValue.Guid, definedValue.Value ) );
    }

    #endregion
}
