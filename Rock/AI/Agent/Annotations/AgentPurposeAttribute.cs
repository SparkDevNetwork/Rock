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
    /// Specifies a purpose for the tool. A tool can have multiple purposes
    /// so this attribute can be applied multiple times to a single method.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
    internal class AgentPurposeAttribute : System.Attribute
    {
        /// <summary>
        /// The purpose of the tool.
        /// </summary>
        public string Purpose { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentPurposeAttribute"/> class.
        /// </summary>
        /// <param name="purpose">The purpose of the tool.</param>
        public AgentPurposeAttribute( string purpose )
        {
            Purpose = purpose;
        }
    }
}
