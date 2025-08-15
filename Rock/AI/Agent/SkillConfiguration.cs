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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents the configuration for a single agent skill, including its metadata,
    /// function definitions, and optional native type or settings.
    /// Used to register both semantic and native skills for an AI agent.
    /// </summary>
    internal class SkillConfiguration
    {
        /// <summary>
        /// Gets the display name of the skill.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the key identifier for this skill, derived from <see cref="Name"/> with spaces removed.
        /// </summary>
        public string Key => Name.Replace( " ", string.Empty );

        /// <summary>
        /// Gets the instructions describing the purpose or typical use of
        /// this skill. These will be passed to the language model.
        /// </summary>
        public string Instructions { get; }

        /// <summary>
        /// Gets the list of function definitions (semantic or native) associated with this skill.
        /// </summary>
        public List<AgentFunction> Functions { get; } = new List<AgentFunction>();

        /// <summary>
        /// Gets the native type implementing this skill, if it is a native (code-based) skill.
        /// </summary>
        public Type NativeType { get; }

        /// <summary>
        /// Gets the list of disabled function GUIDs for this skill.
        /// </summary>
        public List<Guid> DisabledFunctions { get; } = new List<Guid>();

        /// <summary>
        /// Gets a dictionary of configuration values for this skill.
        /// </summary>
        public Dictionary<string, string> ConfigurationValues { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new <see cref="SkillConfiguration"/> for a semantic skill with the specified metadata and functions.
        /// </summary>
        /// <param name="name">The display name of the skill.</param>
        /// <param name="instructions">The instructions describing the skill's purpose.</param>
        /// <param name="functions">The list of semantic or native functions defined for this skill.</param>
        public SkillConfiguration( string name, string instructions, List<AgentFunction> functions )
        {
            Name = name;
            Instructions = instructions;
            Functions = functions ?? new List<AgentFunction>();
        }

        /// <summary>
        /// Initializes a new <see cref="SkillConfiguration"/> for a native skill, specifying its type and settings.
        /// </summary>
        /// <param name="name">The display name of the skill.</param>
        /// <param name="instructions">The instructions describing the skill's purpose.</param>
        /// <param name="nativeType">The <see cref="Type"/> implementing the native skill.</param>
        /// <param name="agentSkillSettings">The settings used to configure the native skill, including disabled functions and configuration values.</param>
        public SkillConfiguration( string name, string instructions, Type nativeType, AgentSkillSettings agentSkillSettings )
        {
            Name = name;
            Instructions = instructions;
            NativeType = nativeType;
            DisabledFunctions = agentSkillSettings.DisabledFunctions ?? new List<Guid>();
            ConfigurationValues = agentSkillSettings.ConfigurationValues ?? new Dictionary<string, string>();
        }
    }
}
