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
//
using System;
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class StepSkill
{
    #region Tool(s)

    /// <summary>
    /// "Deletes a step from the system.
    /// </summary>
    /// <param name="stepIdKey">The encoded identifier of the step to delete.</param>
    /// <returns>The tool result.</returns>
    [Description( "Deletes a step from the system." )]
    [AgentToolGuid( "d62573f6-03da-4a1f-b550-cb0e1ec6a211" )]
    [AgentGuardrail( "This action will permanently delete the specified step. Ensure that this action is intentional and that you have the correct step identifier before proceeding." )]
    public IAgentToolResult DeleteStep( string stepIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var stepService = new StepService( rockContext );

        var existingStep = helper.GetRequiredEntity<Step>( stepIdKey, checkSecurity: true );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        stepService.Delete( existingStep );

        try
        {
            rockContext.SaveChanges();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "An error occurred while deleting a step." );

            return Error( "An error occurred while deleting the step." );
        }

        return Success( "The step has been deleted." );
    }

    #endregion
}
