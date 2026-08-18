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
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets the attribute definitions shared by every value of a defined type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Definitions are per type, not per value, so passing either the type or any
    /// one of its values resolves to the same answer.
    /// </para>
    /// <para>
    /// This exists as a dedicated tool rather than a call to
    /// <see cref="GetEntityAvailableAttributes"/> because defined value
    /// attributes are qualified by DefinedTypeId, and the generic tool has no
    /// qualifier support by design.
    /// </para>
    /// </remarks>
    [Description( "Gets the available attributes that can be set on the values of a defined type. Supply either the defined type or any one of its values." )]
    [AgentPurpose( "Provides the attribute definitions that apply to a defined type's values, along with any value format instructions." )]
    [AgentToolGuid( "542ED067-19EA-4DEE-B8DA-47FBB47C467D" )]
    public AgentToolResult GetDefinedValueAvailableAttributes( string definedTypeIdKey = null, string definedValueIdKey = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        int definedTypeId;

        if ( definedTypeIdKey.IsNotNullOrWhiteSpace() )
        {
            var definedType = helper.GetRequiredEntity<Rock.Model.DefinedType>( definedTypeIdKey );

            if ( definedType == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
            }

            definedTypeId = definedType.Id;
        }
        else if ( definedValueIdKey.IsNotNullOrWhiteSpace() )
        {
            var definedValue = helper.GetRequiredEntity<Rock.Model.DefinedValue>( definedValueIdKey );

            if ( definedValue == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListDefinedValues )} function to determine the available defined values." );
            }

            definedTypeId = definedValue.DefinedTypeId;
        }
        else
        {
            return Error( "Either definedTypeIdKey or definedValueIdKey must be provided." )
                .WithInstructions( $"Call the {nameof( ListDefinedTypes )} function to determine the available defined types." );
        }

        // A stub carrying only the qualifier is enough for LoadAttributes to
        // resolve the definitions, and it keeps this on the standard helper so no
        // bespoke result shape is needed.
        var stubDefinedValue = new Rock.Model.DefinedValue
        {
            DefinedTypeId = definedTypeId
        };

        stubDefinedValue.LoadAttributes( AgentRequestContext.RockContext );

        return Success( helper.GetAvailableAttributes( stubDefinedValue ) );
    }

    #endregion
}
