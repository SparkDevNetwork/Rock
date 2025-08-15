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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.AI.Agent;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache.Entities
{
    /// <inheritdoc cref="AIAgent"/>
    [Serializable]
    [DataContract]
    public class AIAgentCache : ModelCache<AIAgentCache, AIAgent>, IHasReadOnlyAdditionalSettings
    {
        #region Entity Properties

        /// <inheritdoc cref="AIAgent.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="AIAgent.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="AIAgent.AvatarBinaryFileId"/>
        [DataMember]
        public int? AvatarBinaryFileId { get; private set; }

        /// <inheritdoc cref="AIAgent.Instructions"/>
        [DataMember]
        public string Instructions { get; private set; }

        /// <inheritdoc cref="AIAgent.AgentType"/>
        [DataMember]
        public AgentType AgentType { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        #endregion

        #region Navigation Properties

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is AIAgent agent ) )
            {
                return;
            }

            Name = agent.Name;
            Description = agent.Description;
            AvatarBinaryFileId = agent.AvatarBinaryFileId;
            Instructions = agent.Instructions;
            AgentType = agent.AgentType;
            AdditionalSettingsJson = agent.AdditionalSettingsJson;
        }

        /// <summary>
        /// Determines if this agent is excluding system skills.
        /// </summary>
        /// <returns><c>true</c> if system skills should be excluded; otherwise <c>false</c>.</returns>
        private bool IsExcludingSystemSkills()
        {
            if ( AgentType == AgentType.Chat )
            {
                var settings = this.GetAdditionalSettingsOrNull<ChatAgentSettings>();
                return settings?.IsExcludingSystemSkills ?? false;
            }
            else if ( AgentType == AgentType.Mcp )
            {
                var settings = this.GetAdditionalSettingsOrNull<McpAgentSettings>();
                return settings?.IsExcludingSystemSkills ?? false;
            }

            return false;
        }

        /// <summary>
        /// Adds the system skills to the provided list of skill configurations.
        /// </summary>
        /// <param name="skillConfigurations">The list of configuration skills to update.</param>
        /// <param name="currentPerson">The person that will be accessing the agent, used for filtering skills and functions by security.</param>
        /// <param name="isSecurityEnabled">Indicates if security should be checked on skills and functions.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private static void AddSystemSkills( List<SkillConfiguration> skillConfigurations, Person currentPerson, bool isSecurityEnabled, RockContext rockContext )
        {
            foreach ( var systemSkillGuid in AgentSkillComponent.SystemSkillGuids )
            {
                var systemSkill = AISkillCache.Get( systemSkillGuid, rockContext );

                if ( systemSkill == null || !systemSkill.CodeEntityTypeId.HasValue )
                {
                    continue;
                }

                if ( isSecurityEnabled && !systemSkill.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                var entityType = EntityTypeCache.Get( systemSkill.CodeEntityTypeId.Value, rockContext );
                var type = entityType?.GetEntityType();

                if ( type != null )
                {
                    var agentSkillSettings = new AgentSkillSettings();
                    var deniedFunctions = new AISkillFunctionService( rockContext )
                        .Queryable()
                        .Where( f => f.AISkillId == systemSkill.Id )
                        .ToList()
                        .Where( f => isSecurityEnabled && !f.IsAuthorized( Authorization.VIEW, currentPerson ) );

                    // Add the denied functions to the disabled list so that
                    // they are not made available to the agent.
                    agentSkillSettings.DisabledFunctions.AddRange( deniedFunctions.Select( f => f.Guid ) );

                    skillConfigurations.Add( new SkillConfiguration( systemSkill.Name, systemSkill.Instructions, type, agentSkillSettings ) );
                }
            }
        }

        /// <summary>
        /// Gets the skill configurations for this agent.
        /// </summary>
        /// <param name="currentPerson">The person that will be accessing the agent, used for filtering skills and functions by security.</param>
        /// <param name="isSecurityEnabled">Indicates if security should be checked on skills and functions.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of skill configuration objects.</returns>
        internal List<SkillConfiguration> GetSkillConfigurations( Person currentPerson, bool isSecurityEnabled, RockContext rockContext )
        {
            var agentSkills = new AIAgentSkillService( rockContext )
                .Queryable()
                .Include( aa => aa.AISkill )
                .Where( aa => aa.AIAgentId == Id )
                .ToList()
                .Where( aa => !isSecurityEnabled || aa.AISkill.IsAuthorized( Authorization.VIEW, currentPerson ) );

            var skillConfigurations = new List<SkillConfiguration>();

            if ( !IsExcludingSystemSkills() )
            {
                AddSystemSkills( skillConfigurations, currentPerson, isSecurityEnabled, rockContext );
            }

            foreach ( var agentSkill in agentSkills )
            {
                var agentSkillSettings = agentSkill.GetAdditionalSettings<AgentSkillSettings>();

                if ( agentSkill.AISkill.CodeEntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( agentSkill.AISkill.CodeEntityTypeId.Value, rockContext );
                    var type = entityType?.GetEntityType();

                    if ( type != null )
                    {
                        var deniedFunctions = new AISkillFunctionService( rockContext )
                            .Queryable()
                            .Where( f => f.AISkillId == agentSkill.AISkillId )
                            .ToList()
                            .Where( f => isSecurityEnabled && !f.IsAuthorized( Authorization.VIEW, currentPerson ) );

                        // Add the denied functions to the disabled list so that
                        // they are not made available to the agent.
                        agentSkillSettings.DisabledFunctions.AddRange( deniedFunctions.Select( f => f.Guid ) );

                        skillConfigurations.Add( new SkillConfiguration( agentSkill.AISkill.Name, agentSkill.AISkill.Instructions, type, agentSkillSettings ) );
                    }
                }
                else
                {
                    var functions = new AISkillFunctionService( rockContext )
                        .Queryable()
                        .Where( f => f.AISkillId == agentSkill.AISkillId )
                        .ToList()
                        .Where( f => !isSecurityEnabled || f.IsAuthorized( Authorization.VIEW, currentPerson ) );

                    var skillFunctions = new List<AgentFunction>();

                    foreach ( var function in functions )
                    {
                        if ( agentSkillSettings.DisabledFunctions?.Contains( function.Guid ) == true )
                        {
                            continue;
                        }

                        var prompt = function.GetAdditionalSettings<PromptInformationSettings>();

                        var agentFunction = new AgentFunction
                        {
                            Guid = function.Guid,
                            Name = function.Name,
                            Description = function.Description,
                            Instructions = function.Instructions,
                            Role = ModelServiceRole.Default, // TODO: Fix this
                            FunctionType = function.FunctionType,
                            Prompt = prompt.Prompt ?? string.Empty,
                            EnableLavaPreRendering = prompt.PreRenderLava,
                            Parameters = prompt.PromptParameters,
                            Temperature = ( double? ) prompt.Temperature,
                            MaxTokens = prompt.MaxTokens,
                        };

                        skillFunctions.Add( agentFunction );
                    }

                    if ( skillFunctions.Count > 0 )
                    {
                        skillConfigurations.Add( new SkillConfiguration( agentSkill.AISkill.Name, agentSkill.AISkill.Instructions, skillFunctions ) );
                    }
                }
            }

            return skillConfigurations;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
