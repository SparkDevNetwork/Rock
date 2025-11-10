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
    /// Specifies a prerequisite for the tool. This can help the language model
    /// understand the order of operations that must be performed
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
    internal class AgentToolPrerequisiteAttribute : System.Attribute
    {
        /// <summary>
        /// The prerequisite information for the tool.
        /// </summary>
        public string Prerequisite { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentToolPrerequisiteAttribute"/> class.
        /// </summary>
        /// <param name="prerequisite">The prerequisite information for the tool.</param>
        public AgentToolPrerequisiteAttribute( string prerequisite )
        {
            Prerequisite = prerequisite;
        }
    }
}
