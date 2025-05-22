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
using Rock.Security;
using Rock.ViewModels.Controls;

namespace Rock.Core.Automation
{
    /// <summary>
    /// Base class for all automation event components. This is used to
    /// provide UI and logic for a specific event type in the automation
    /// system.
    /// </summary>
    internal abstract class AutomationEventComponent
    {
        #region Properties

        /// <summary>
        /// The event identifier that this component instance is handling.
        /// This may be <c>null</c> in some cases so you should always check to
        /// make sure it has a value before accessing it.
        /// </summary>
        public int? AutomationEventId { get; set; }

        /// <summary>
        /// The CSS class that will be used to display the icon for this
        /// event type in the UI.
        /// </summary>
        public abstract string IconCssClass { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the name of the event that will be displayed in the UI.
        /// </summary>
        /// <param name="privateConfiguration">The configuration values that describe the event settings.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <returns>A short string that describes the event to the individual.</returns>
        public abstract string GetEventName( Dictionary<string, string> privateConfiguration, RockContext rockContext );

        /// <summary>
        /// Gets the description of the event that will be displayed in the UI.
        /// </summary>
        /// <param name="privateConfiguration">The configuration values that describe the event settings.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <returns>A long string that describes the purpose of the event and what it accomplishes to the individual.</returns>
        public abstract string GetEventDescription( Dictionary<string, string> privateConfiguration, RockContext rockContext );

        /// <summary>
        /// Creates an instance object that will handle executing the event for the
        /// specified configuration. This allows the configuration to be parsed
        /// and stored on the executor for improved performance.
        /// </summary>
        /// <param name="automationEventId">The identifier of the <see cref="AutomationEvent"/> that represents this executor.</param>
        /// <param name="privateConfiguration">The private configuration values for the event.</param>
        /// <param name="rockContext">The context to use if access to the database is required while initializing the trigger. This will be disposed after the method returns.</param>
        /// <returns>An instance of <see cref="AutomationEventExecutor"/> that will be called repeatedly to execute requests.</returns>
        public abstract AutomationEventExecutor CreateExecutor( int automationEventId, Dictionary<string, string> privateConfiguration, RockContext rockContext );

        /// <summary>
        /// Gets the definition of the Obsidian component that will be used to
        /// render the UI for editing the event instance.
        /// </summary>
        /// <param name="privateConfiguration">The current configuration values that will be displayed on initial load.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        /// <returns>An instance of <see cref="DynamicComponentDefinitionBag"/> that describes how to render the UI.</returns>
        public abstract DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext );

        /// <summary>
        /// Execute a request that is sent from the UI component to the server
        /// component. This is used to handle any dynamic updates that are
        /// required by the UI in order to operate correctly.
        /// </summary>
        /// <param name="request">The request object from the UI component.</param>
        /// <param name="securityGrant">The security grant that is providing additional authorization to this request.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the current network request being processed.</param>
        /// <returns>A dictionary of values that will be returned to the UI component.</returns>
        public virtual Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        /// <summary>
        /// Transforms the private configuration values into ones that will be
        /// sent down to the UI component. This is used to translate the data
        /// into a format that is more easily consumed by the UI component.
        /// </summary>
        /// <param name="privateConfiguration">The current configuration values from the database.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the current network request being processed.</param>
        /// <returns>A dictionary of values that will be returned to the UI component.</returns>
        public virtual Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return privateConfiguration;
        }

        /// <summary>
        /// Transforms the public configuration values into ones that will be
        /// stored in the database. This is used to translate the data
        /// into a format that is cleaner to store.
        /// </summary>
        /// <param name="publicConfiguration">The current configuration values from the database.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the current network request being processed.</param>
        /// <returns>A dictionary of values that will be returned to the UI component.</returns>
        public virtual Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return publicConfiguration;
        }

        #endregion
    }
}
