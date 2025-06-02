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
using System.ComponentModel;
using System.Reflection;

namespace Rock.AI.Agent
{
    internal class SkillConfiguration
    {
        public string Name { get; }

        public string Key => Name.Replace( " ", string.Empty );

        public string UsageHint { get; }

        public List<AgentFunction> Functions { get; set; }

        public Type NativeType { get; set; }

        public SkillConfiguration( string name, string usageHint, List<AgentFunction> functions )
        {
            Name = name;
            UsageHint = usageHint;
            Functions = functions ?? new List<AgentFunction>();
        }

        public SkillConfiguration( Type nativeType )
        {
            Name = nativeType.Name.SplitCase();
            UsageHint = nativeType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
            NativeType = nativeType;
        }
    }
}
