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

using Rock.Data;
using Rock.Model;
using Rock.Net;

namespace Rock.Workflow
{
    /// <summary>
    /// The interface the interactive workflow actions must implement in order
    /// to present 
    /// </summary>
    internal interface IInteractiveAction
    {
        /// <summary>
        /// Starts processing the action in the context of an interactive session
        /// where information can be displayed to the individual.
        /// </summary>
        /// <param name="action">The action that is being executed.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The current request that is active, must never be <c>null</c>.</param>
        /// <returns>An instance of <see cref="InteractiveActionResult"/> that describes how the workflow entry block should proceed.</returns>
        InteractiveActionResult StartAction( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext );

        /// <summary>
        /// Updates the workflow action with the data from a component that was
        /// displayed. This can return various results such as displaying the
        /// same component again (probably with custom error messages) or
        /// marking the action as completed and dislpaying a message.
        /// </summary>
        /// <param name="action">The action that will be represented by the compponent.</param>
        /// <param name="componentData">The data from the UI component.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <param name="requestContext">The current request that is active, must never be <c>null</c>.</param>
        /// <returns>An instance of <see cref="InteractiveActionResult"/> that describes how the workflow entry block should proceed.</returns>
        InteractiveActionResult UpdateAction( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext );
    }
}
