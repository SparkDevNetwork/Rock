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
    /// Deletes a defined type.
    /// </summary>
    /// <remarks>
    /// A defined type that still has values is not deleted, so its values are never
    /// removed implicitly. The values must be deleted first, one at a time. System
    /// defined types, and types still referenced by other records, cannot be deleted
    /// at all.
    /// </remarks>
    [AgentGuardrail( "This permanently deletes the defined type. Confirm the exact defined type with the user before proceeding." )]
    [Description( "Deletes a defined type. A defined type that still has values or is referenced by other records cannot be deleted." )]
    [AgentToolPreamble( "Deleting the defined type." )]
    [AgentToolPrerequisite( "Call ListDefinedTypes to determine the definedTypeIdKey." )]
    [AgentToolGuid( "09EA4E73-C99D-412B-BBEF-C3011271691D" )]
    public AgentToolResult DeleteDefinedType( string definedTypeIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var definedTypeService = new DefinedTypeService( rockContext );

        var definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

        if ( definedType == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
        }

        if ( !definedType.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete that defined type." );
        }

        if ( definedType.IsSystem )
        {
            return Error( "That defined type is part of Rock's core configuration and cannot be deleted." );
        }

        // A defined type with values is not deleted implicitly; the caller removes
        // each value first so nothing is removed as a hidden side effect.
        var hasValues = new DefinedValueService( rockContext ).Queryable().Any( dv => dv.DefinedTypeId == definedType.Id );

        if ( hasValues )
        {
            return Error( "That defined type still has values and cannot be deleted until they are removed." )
                .WithInstructions( $"Call the {nameof( ListDefinedValues )} function with this definedTypeIdKey to enumerate the values, delete each one with {nameof( DeleteDefinedValue )}, then delete this defined type." );
        }

        if ( !definedTypeService.CanDelete( definedType, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var name = definedType.Name;

        definedTypeService.Delete( definedType );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( $"The '{name}' defined type has been deleted." );
    }

    #endregion
}
