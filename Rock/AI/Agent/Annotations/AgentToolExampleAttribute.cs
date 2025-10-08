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
    /// Provides an example to the language model of how to call the tool.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
    internal class AgentToolExampleAttribute : System.Attribute
    {
        /// <summary>
        /// The example of how to use the tool.
        /// </summary>
        public string Example { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentToolExampleAttribute"/> class.
        /// </summary>
        /// <param name="example">The example of how to use the tool.</param>
        public AgentToolExampleAttribute( string example )
        {
            Example = example;
        }
    }
}
