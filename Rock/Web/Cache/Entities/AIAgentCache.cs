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
using Rock.Model;

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

        /// <inheritdoc cref="AIAgent.Persona"/>
        [DataMember]
        public string Persona { get; private set; }

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
            Persona = agent.Persona;
            AdditionalSettingsJson = agent.AdditionalSettingsJson;
        }

        internal List<SkillConfiguration> GetSkillConfigurations( RockContext rockContext )
        {
            var agentSkills = new AIAgentSkillService( rockContext )
                .Queryable()
                .Include( aa => aa.AISkill )
                .Where( aa => aa.AIAgentId == Id )
                .ToList();

            var skillConfigurations = new List<SkillConfiguration>();

            foreach ( var agentSkill in agentSkills )
            {
                var agentSkillSettings = agentSkill.GetAdditionalSettings<AgentSkillSettings>();

                if ( agentSkill.AISkill.CodeEntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( agentSkill.AISkill.CodeEntityTypeId.Value, rockContext );
                    var type = entityType?.GetEntityType();

                    if ( type != null )
                    {
                        skillConfigurations.Add( new SkillConfiguration( agentSkill.AISkill.Name, agentSkill.AISkill.UsageHint, type, agentSkillSettings ) );
                    }
                }
                else
                {
                    var functions = new AISkillFunctionService( rockContext )
                        .Queryable()
                        .Where( f => f.AISkillId == agentSkill.AISkillId )
                        .ToList();

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
                            UsageHint = function.UsageHint,
                            Role = Enums.Core.AI.Agent.ModelServiceRole.Default, // TODO: Fix this
                            FunctionType = function.FunctionType,
                            Prompt = prompt.Prompt ?? string.Empty,
                            EnableLavaPreRendering = prompt.PreRenderLava,
                            InputSchema = prompt.PromptParametersSchema,
                            Temperature = ( double? ) prompt.Temperature,
                            MaxTokens = prompt.MaxTokens,
                        };

                        skillFunctions.Add( agentFunction );
                    }

                    skillConfigurations.Add( new SkillConfiguration( agentSkill.AISkill.Name, agentSkill.AISkill.UsageHint, skillFunctions ) );
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
