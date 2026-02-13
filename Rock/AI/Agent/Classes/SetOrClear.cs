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

namespace Rock.AI.Agent.Classes
{
    /// <summary>
    /// Wrapper indicating an intent to either set a value or clear an existing value.
    /// </summary>
    /// <typeparam name="T">The value type. Usually nullable for clearing semantics.</typeparam>
    /// <remarks>
    /// Null instance ⇒ no change. <see cref="ClearValue"/> takes precedence over <see cref="Value"/>.
    /// </remarks>
    internal class SetOrClear<T>
    {
        /// <summary>
        /// The value to set when <see cref="ClearValue"/> is false. Ignored when clearing.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// When true, clear the target field regardless of <see cref="Value"/>.
        /// </summary>
        public bool ClearValue { get; set; }
    }
}
