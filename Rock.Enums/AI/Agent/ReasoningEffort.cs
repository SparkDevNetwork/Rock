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

using System.ComponentModel;

namespace Rock.Enums.AI.Agent
{
    /// <summary>
    /// How much reasoning effort the language model should spend before
    /// answering. Higher values produce more thorough answers at the cost of
    /// latency and tokens.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    public enum ReasoningEffort
    {
        /// <summary>
        /// The smallest amount of reasoning the model supports.
        /// </summary>
        Minimal = 0,

        /// <summary>
        /// A small amount of reasoning suited to quick, conversational requests.
        /// </summary>
        Low = 1,

        /// <summary>
        /// A balanced amount of reasoning.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// A large amount of reasoning for complex, multi-step requests.
        /// </summary>
        High = 3,

        /// <summary>
        /// The most reasoning the model supports.
        /// </summary>
        [Description( "Extra High" )]
        XHigh = 4,
    }
}
