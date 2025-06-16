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
    /// <summary>
    /// <para>
    /// Base class for all code-based agent skills. A skill that is code-based
    /// uses C# methods to provide the functionality for the skill. These are
    /// not editable by the individual.
    /// </para>
    /// <para>
    /// Each individual skill must be decorated with <see cref="Microsoft.SemanticKernel.KernelFunctionAttribute"/>,
    /// <see cref="System.ComponentModel.DescriptionAttribute"/>, and
    /// <see cref="SystemGuid.AgentFunctionGuidAttribute"/> attributes.
    /// </para>
    /// </summary>
    internal abstract class AgentSkill
    {
        /// <summary>
        /// Gets the semantic functions that should be registered with this
        /// skill. A semantic function does not execute code. Instead it
        /// provides a prompt that can be used by an AI model to feed back
        /// into itself and generate a response based on the prompt.
        /// </summary>
        /// <returns>A collection of <see cref="AgentFunction"/> objects that represent the semantic functions.</returns>
        public virtual IReadOnlyCollection<AgentFunction> GetSemanticFunctions() => Array.Empty<AgentFunction>();
    }
}
