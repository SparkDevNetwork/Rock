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
using Rock.Extension;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;

namespace Rock.AI.Agent
{
    /// <summary>
    /// <para>
    /// Base class for all code-based agent skills. A skill that is code-based
    /// uses C# methods to provide the functionality for the skill. These are
    /// not editable by the individual.
    /// </para>
    /// <para>
    /// Each individual skill must be decorated with <see cref="Microsoft.SemanticKernel.KernelFunctionAttribute"/>,
    /// <see cref="System.ComponentModel.DescriptionAttribute"/>, and
    /// <see cref="SystemGuid.AgentFunctionGuidAttribute"/> attributes.
    /// </para>
    /// </summary>
    internal abstract class AgentSkillComponent : LightComponent
    {
        #region Properties

        /// <summary>
        /// The context for this chat agent request. This will be null except
        /// when the skill is being executed as part of a chat. Meaning any
        /// method that is not a kernel function will not have a context.
        /// </summary>
        protected AgentRequestContext AgentRequestContext { get; private set; }

        /// <summary>
        /// The configuration values that were configured for this skill when it
        /// was added to the agent. These represent the private values. They are
        /// not valid for use inside the configuration methods.
        /// </summary>
        protected IReadOnlyDictionary<string, string> ConfigurationValues { get; private set; } = new Dictionary<string, string>();

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the component for use with a chat agent.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="agentRequestContext">The context for this chat agent request.</param>
        internal void Initialize( IReadOnlyDictionary<string, string> configurationValues, AgentRequestContext agentRequestContext )
        {
            ConfigurationValues = configurationValues;
            AgentRequestContext = agentRequestContext;
        }

        /// <summary>
        /// Gets the semantic functions that should be registered with this
        /// skill. A semantic function does not execute code. Instead it
        /// provides a prompt that can be used by an AI model to feed back
        /// into itself and generate a response based on the prompt.
        /// </summary>
        /// <returns>A collection of <see cref="AgentFunction"/> objects that represent the semantic functions.</returns>
        public virtual IReadOnlyCollection<AgentFunction> GetSemanticFunctions() => Array.Empty<AgentFunction>();

        #endregion

        #region Configuration

        /// <summary>
        /// Gets the definition of the Obsidian component that will be used to
        /// render the UI for editing configuration.
        /// </summary>
        /// <param name="privateConfiguration">The current configuration values that will be displayed on initial load.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        /// <param name="requestContext">The context describing the current request.</param>
        /// <returns>An instance of <see cref="DynamicComponentDefinitionBag"/> that describes how to render the UI.</returns>
        public virtual DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

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
