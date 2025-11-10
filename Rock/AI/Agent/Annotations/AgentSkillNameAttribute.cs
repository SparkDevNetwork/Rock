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
namespace Rock.AI.Agent.Annotations
{
    /// <summary>
    /// Specifies the name of the skill. If not specified then one will be
    /// generated based on the class name.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    internal class AgentSkillNameAttribute : System.Attribute
    {
        /// <summary>
        /// The name of the skill.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentSkillNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the skill.</param>
        public AgentSkillNameAttribute( string name )
        {
            Name = name;
        }
    }
}
