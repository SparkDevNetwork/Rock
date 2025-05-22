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
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Core.Automation
{
    /// <summary>
    /// Base class for all automation trigger components. This is used to
    /// provide UI and logic for a specific trigger type in the automation
    /// system.
    /// </summary>
    internal abstract class AutomationTriggerComponent
    {
        #region Properties

        /// <summary>
        /// The trigger identifier that this component instance is handling.
        /// This may be <c>null</c> in some cases so you should always check to
        /// make sure it has a value before accessing it.
        /// </summary>
        public int? AutomationTriggerId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an operational trigger from the configuration values. The
        /// returned object will be disposed of when the trigger is no longer
        /// needed and should be shutdown.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the <see cref="AutomationTrigger"/> that should be used when firing events for this trigger.</param>
        /// <param name="privateConfiguration">The private configuration values for the trigger.</param>
        /// <param name="rockContext">The context to use if access to the database is required while initializing the trigger. This will be disposed after the method returns.</param>
        /// <returns>The object that will be disposed when the trigger should shutdown and stop monitoring.</returns>
        public abstract IDisposable CreateTriggerMonitor( int automationTriggerId, Dictionary<string, string> privateConfiguration, RockContext rockContext );

        /// <summary>
        /// Gets the value types that are available to events that are fired
        /// by this trigger. This is used to provide a list of keys and their
        /// expected types to the event UI components. This helps them display
        /// information to the individual to assist in configuring the event.
        /// </summary>
        /// <param name="privateConfiguration">The private configuration values for the trigger.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A collection of <see cref="AutomationValueDefinition"/> objects that describe the values available.</returns>
        public virtual IEnumerable<AutomationValueDefinition> GetValueTypes( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return Array.Empty<AutomationValueDefinition>();
        }

        /// <summary>
        /// Gets the configuration details that describe the any configuration
        /// options that should be displayed on the Trigger Detail page. Each
        /// item will be displayed as a field in the UI with the Value property
        /// being the field title and the Text property being the text
        /// displayed for the field. The Text value may be Markdown to provide
        /// simple formatting.
        /// </summary>
        /// <param name="privateConfiguration">The private configuration values for the trigger.</param>
        /// <param name="rockContext">The context to use if access to the database is required.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that describe the fields to display.</returns>
        public virtual IEnumerable<ListItemBag> GetConfigurationDetails( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            return Array.Empty<ListItemBag>();
        }

        /// <summary>
        /// Gets the definition of the Obsidian component that will be used to
        /// render the UI for editing the trigger instance.
        /// </summary>
        /// <param name="privateConfiguration">The current configuration values that will be displayed on initial load.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        /// <returns>An instance of <see cref="DynamicComponentDefinitionBag"/> that describes how to render the UI.</returns>
        public abstract DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext );

        /// <summary>
        /// Executes a request that is sent from the UI component to the server
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
