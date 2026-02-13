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
    /// Specifies the preamble of the tool in an AI skill. This is displayed in
    /// the agent UI when the tool is being called.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Method, Inherited = false, AllowMultiple = false )]
    internal class AgentToolPreambleAttribute : System.Attribute
    {
        /// <summary>
        /// The preamble of the tool.
        /// </summary>
        public string Preamble { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentToolPreambleAttribute"/> class.
        /// </summary>
        /// <param name="preamble">The preamble of the tool.</param>
        public AgentToolPreambleAttribute( string preamble )
        {
            Preamble = preamble;
        }
    }
}
