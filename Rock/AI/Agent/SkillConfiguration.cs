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
    internal class SkillConfiguration
    {
        public string Name { get; }

        public string Key => Name.Replace( " ", string.Empty );

        public string UsageHint { get; }

        public List<AgentFunction> Functions { get; } = new List<AgentFunction>();

        public Type NativeType { get; }

        public List<Guid> DisabledFunctions { get; } = new List<Guid>();

        public Dictionary<string, string> ConfigurationValues { get; } = new Dictionary<string, string>();

        public SkillConfiguration( string name, string usageHint, List<AgentFunction> functions )
        {
            Name = name;
            UsageHint = usageHint;
            Functions = functions ?? new List<AgentFunction>();
        }

        public SkillConfiguration( string name, string usageHint, Type nativeType, AgentSkillSettings agentSkillSettings )
        {
            Name = name;
            UsageHint = usageHint;
            NativeType = nativeType;
            DisabledFunctions = agentSkillSettings.DisabledFunctions ?? new List<Guid>();
            ConfigurationValues = agentSkillSettings.ConfigurationValues ?? new Dictionary<string, string>();
        }
    }
}
