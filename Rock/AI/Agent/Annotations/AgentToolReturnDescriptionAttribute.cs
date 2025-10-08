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
    /// Specifies a description for what the expected return value of the tool.
    /// This can help the language model know when to use the tool to obtain
    /// specific information.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Method, Inherited = false, AllowMultiple = false )]
    internal class AgentToolReturnDescriptionAttribute : System.Attribute
    {
        /// <summary>
        /// The description of the return value.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentToolReturnDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description of the return value.</param>
        public AgentToolReturnDescriptionAttribute( string description )
        {
            Description = description;
        }
    }
}
