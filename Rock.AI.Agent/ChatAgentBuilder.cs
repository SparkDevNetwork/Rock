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
using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Web.Cache;

namespace Rock.AI.Agent
{
    internal class ChatAgentBuilder : IChatAgentBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly RockContext _rockContext;

        public ChatAgentBuilder( IServiceProvider serviceProvider, RockContext rockContext )
        {
            _serviceProvider = serviceProvider;
            _rockContext = rockContext;
        }

        public IChatAgent Build( int agentId )
        {
            var factories = ( ConcurrentDictionary<int, ChatAgentFactory> ) RockCache.GetOrAddExisting( "Rock.AI.Agent.ChatAgentBuilder.Factories",
                () => new ConcurrentDictionary<int, ChatAgentFactory>() );

            var factory = factories.GetOrAdd( agentId, ( id, ctx ) => new ChatAgentFactory( id, ctx, _serviceProvider.GetService<ILoggerFactory>() ), _rockContext );

            return factory.Build( _serviceProvider );
        }

        internal void FlushCache()
        {
            RockCache.Remove( "Rock.AI.Agent.ChatAgentBuilder.Factories" );
        }
    }

    internal class AiSkill
    {
        /// <summary>
        /// A short, descriptive name for this AI Skill. This name helps identify the purpose of the skill 
        /// (e.g., "Guest Follow-Up Generator", "Giving Insights Summary"). It will appear in the UI, logs, and 
        /// will also be used to help determine how and when the skill is selected for inclusion into the AI 
        /// kernel. Choose a name that clearly conveys the skill’s function.
        /// Should be short, descriptive, and use PascalCase.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The key name derived from the name.
        /// </summary>
        public string Key
        {
            get
            {
                return Name.Replace( " ", "" );
            }
        }

        /// <summary>
        /// The type of skill that reflects the source of the logic. Code for native code skills or Extended for virtual
        /// skills stored in the database.
        /// </summary>
        public SkillType SkillType { get; set; } = SkillType.Code;

        /// <summary>
        /// A brief description or prompt that explains how this skill is intended to be used. This provides context to 
        /// users and automation systems when deciding whether to activate this skill (e.g., "Use after a guest attends 
        /// for the first time", or "Run weekly to summarize giving trends"). This hint helps guide appropriate usage 
        /// during AI-driven task selection.
        /// Leave blank when the name provides enough context.
        /// </summary>
        public string UsageHint { get; set; } = string.Empty;

        /// <summary>
        /// Listing of semantic functions for the skill.
        /// </summary>
        public List<AgentFunction> AiPromptFunctions { get; set; } = new List<AgentFunction>();

        /// <summary>
        /// Listing of code functions for the skill.
        /// </summary>
        public KernelPluginCollection CodeFunctions { get; set; } = new KernelPluginCollection();

        /// <summary>
        /// Listing of Lava functions for the skill.
        /// </summary>
        public List<AgentFunction> LavaFunctions { get; set; } = new List<AgentFunction>();
    }
}
