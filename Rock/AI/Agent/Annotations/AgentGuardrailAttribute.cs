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
    /// Specifies the guardrail guidance for the tool. This is used to provide
    /// protected guardrails to help prevent the tool from being used
    /// inappropriately. This can be specified multiple times.
    /// </summary>
    [System.AttributeUsage( System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
    internal class AgentGuardrailAttribute : System.Attribute
    {
        /// <summary>
        /// The guardrail guidance for the tool.
        /// </summary>
        public string Guardrail { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentGuardrailAttribute"/> class.
        /// </summary>
        /// <param name="guardrail">The guardrail guidance for the tool.</param>
        public AgentGuardrailAttribute( string guardrail )
        {
            Guardrail = guardrail;
        }
    }
}
