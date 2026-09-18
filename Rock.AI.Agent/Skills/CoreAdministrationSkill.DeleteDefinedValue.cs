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
    /// Deletes a defined value.
    /// </summary>
    /// <remarks>
    /// System defined values, and values still referenced by other records, cannot
    /// be deleted.
    /// </remarks>
    [AgentGuardrail( "This permanently deletes the defined value. Confirm the exact value with the user before proceeding." )]
    [Description( "Deletes a defined value. A value that is referenced by other records cannot be deleted." )]
    [AgentToolPreamble( "Deleting the defined value." )]
    [AgentToolPrerequisite( "Call ListDefinedValues to determine the definedValueIdKey." )]
    [AgentToolGuid( "10F3C6B8-C07D-4990-B2AA-F66A923E87A3" )]
    public AgentToolResult DeleteDefinedValue( string definedValueIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var definedValueService = new DefinedValueService( rockContext );

        var definedValue = helper.GetRequiredEntity<Rock.Model.DefinedValue>( definedValueIdKey );

        if ( definedValue == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListDefinedValues )} function to determine the available defined values." );
        }

        // A defined value inherits its security from its parent defined type.
        if ( !definedValue.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete that defined value." );
        }

        if ( definedValue.IsSystem )
        {
            return Error( "That defined value is part of Rock's core configuration and cannot be deleted." );
        }

        if ( !definedValueService.CanDelete( definedValue, out var errorMessage ) )
        {
            return Error( errorMessage );
        }

        var value = definedValue.Value;

        definedValueService.Delete( definedValue );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( $"The '{value}' defined value has been deleted." );
    }

    #endregion
}
