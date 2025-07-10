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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Data;
using Rock.Model;
using Rock.Net;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Prompts user for attribute values in a pre-defined layout.
    /// </summary>
    [ActionCategory( "HideFromUser" )]
    [Description( "Prompts user for attribute values in a pre-defined layout." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Form Builder" )]

    [Rock.SystemGuid.EntityTypeGuid( "B2A91AD5-3B41-45A6-A670-EBBF3FF626F9" )]
    public class FormBuilder : ActionComponent, IInteractiveAction
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            return false;
        }

        #region IInteractiveAction

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.StartAction( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            return UserEntryForm.StartActionInternal( action, rockContext, requestContext );
        }

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.UpdateAction( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            return UserEntryForm.UpdateActionInternal( action, componentData, rockContext, requestContext );
        }

        #endregion
    }
}
