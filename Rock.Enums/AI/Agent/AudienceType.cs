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

namespace Rock.Enums.AI.Agent
{
    /// <summary>
    /// The type of audience the agent is configured to serve. This gives a hint
    /// to skills and tools about how to behave, such as disabling certain
    /// functionality that is not appropriate for the audience.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    public enum AudienceType
    {
        /// <summary>
        /// The agent is configured to be used internally by Rock staff.
        /// </summary>
        Internal = 0,

        /// <summary>
        /// The agent is configured to be used by the general public.
        /// </summary>
        Public = 1,
    }
}
