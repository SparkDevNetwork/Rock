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
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Field;
using Rock.Field.Types;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    /// <summary>
    /// <para>
    /// Base class for all code-based agent skills. A skill that is code-based
    /// uses C# methods to provide the functionality for the skill. These are
    /// not editable by the individual.
    /// </para>
    /// <para>
    /// Each individual tool must be decorated with <see cref="SystemGuid.AgentToolGuidAttribute"/>.
    /// </para>
    /// </summary>
    internal abstract class AgentSkillComponent : LightComponent
    {
        #region Constants

        internal static readonly IReadOnlyCollection<Guid> SystemSkillGuids = new HashSet<Guid>
        {
            new Guid( "3406D2DC-6718-45A2-99D3-1DAA32BF2EFD" ), // CoreUtility
        };

        #endregion

        #region Properties

        /// <summary>
        /// The context for this chat agent request. This will be null except
        /// when the skill is being executed as part of a chat. Meaning any
        /// method that is not a tool will not have a context.
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
        /// Gets the dynamic tools that should be registered with this skill.
        /// Only <see cref="Enums.AI.Agent.ToolType.AIPrompt"/> tools are
        /// supported.
        /// </summary>
        /// <returns>A collection of <see cref="AgentTool"/> objects that represent the dynamic tools.</returns>
        public virtual IReadOnlyCollection<AgentTool> GetDymanicTools() => Array.Empty<AgentTool>();

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
            var fieldTypeAttributes = GetConfigurationAttributes();
            int order = 0;
            var attributeBags = new List<PublicAttributeBag>();

            // Build a list of all the field type attributes defined on this
            // instance. These are transformed into a fake attribute so that
            // the client can present them with standard logic.
            foreach ( var fieldTypeAttribute in fieldTypeAttributes )
            {
                var fieldTypeCache = FieldTypeCache.All().FirstOrDefault( c => c.Class == fieldTypeAttribute.FieldTypeClass );
                if ( fieldTypeCache == null || fieldTypeCache.Field == null )
                {
                    continue;
                }

                var configurationValues = fieldTypeAttribute.FieldConfigurationValues
                    .ToDictionary( k => k.Key, k => k.Value.Value );

                var bag = new PublicAttributeBag
                {
                    FieldTypeGuid = fieldTypeCache.ControlFieldTypeGuid,
                    AttributeGuid = Guid.NewGuid(),
                    Name = fieldTypeAttribute.Name,
                    Order = order++,
                    Key = fieldTypeAttribute.Key,
                    IsRequired = fieldTypeAttribute.IsRequired,
                    Description = fieldTypeAttribute.Description,
                    ConfigurationValues = fieldTypeCache.Field.GetPublicConfigurationValues( configurationValues, ConfigurationValueUsage.Edit, null ),
                };

                attributeBags.Add( bag );
            }

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/AI/Skills/skillConfiguration.obs" ),
                Options = new Dictionary<string, string>
                {
                    ["attributes"] = attributeBags.ToCamelCaseJson( false, true )
                }
            };
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
            var fieldTypeAttributes = GetConfigurationAttributes();
            var publicConfiguration = new Dictionary<string, string>();

            // Convert the private configuration values into public ones for
            // each of the field type attributes defined on this instance.
            foreach ( var fieldTypeAttribute in fieldTypeAttributes )
            {
                var fieldTypeCache = FieldTypeCache.All().FirstOrDefault( c => c.Class == fieldTypeAttribute.FieldTypeClass );
                if ( fieldTypeCache == null || fieldTypeCache.Field == null )
                {
                    continue;
                }

                if ( !privateConfiguration.TryGetValue( fieldTypeAttribute.Key, out var privateValue ) )
                {
                    privateValue = string.Empty;
                }

                var configurationValues = fieldTypeAttribute.FieldConfigurationValues
                    .ToDictionary( k => k.Key, k => k.Value.Value );

                publicConfiguration[fieldTypeAttribute.Key] = fieldTypeCache.Field.GetPublicEditValue( privateValue, configurationValues );
            }

            return publicConfiguration;
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
            var fieldTypeAttributes = GetConfigurationAttributes();
            var privateConfiguration = new Dictionary<string, string>();

            // Convert the public configuration values into private ones for
            // each of the field type attributes defined on this instance.
            foreach ( var fieldTypeAttribute in fieldTypeAttributes )
            {
                var fieldTypeCache = FieldTypeCache.All().FirstOrDefault( c => c.Class == fieldTypeAttribute.FieldTypeClass );
                if ( fieldTypeCache == null || fieldTypeCache.Field == null )
                {
                    continue;
                }

                if ( !publicConfiguration.TryGetValue( fieldTypeAttribute.Key, out var publicValue ) )
                {
                    publicValue = string.Empty;
                }

                var configurationValues = fieldTypeAttribute.FieldConfigurationValues
                    .ToDictionary( k => k.Key, k => k.Value.Value );

                privateConfiguration[fieldTypeAttribute.Key] = fieldTypeCache.Field.GetPrivateEditValue( publicValue, configurationValues );
            }

            return privateConfiguration;
        }

        /// <summary>
        /// Gets the field attributes that define the configuration for this
        /// skill.
        /// </summary>
        /// <returns>A list of <see cref="FieldAttribute"/> instances.</returns>
        private List<FieldAttribute> GetConfigurationAttributes()
        {
            return GetType()
                .GetCustomAttributes( true )
                .Where( a => typeof( FieldAttribute ).IsAssignableFrom( a.GetType() ) )
                .Cast<FieldAttribute>()
                .Where( fa => fa.FieldTypeClass != typeof( FileFieldType ).FullName
                    && fa.FieldTypeClass != typeof( ImageFieldType ).FullName
                    && fa.FieldTypeClass != typeof( BackgroundCheckFieldType ).FullName
                    && fa.FieldTypeClass != typeof( StructureContentEditorFieldType ).FullName )
                .OrderBy( a => a.Order )
                .ToList();
        }

        #endregion
    }
}
