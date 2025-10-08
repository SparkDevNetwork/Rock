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

using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Extension methods for <see cref="IChatAgent"/>. These provide additional
    /// convenience methods that build on the core methods provided by the
    /// interface.
    /// </summary>
    /// <remarks>
    /// This allows unit tests to provide mocked <see cref="IChatAgent"/>
    /// implementations without having to implement all the various overloads.
    /// </remarks>
    [RockInternal( "18.0" )]
    internal static class IChatAgentExtensions
    {
        /// <summary>
        /// Starts a new session in the database without associating it with
        /// a specific entity.
        /// </summary>
        /// <param name="chatAgent">The chat agent instance.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        public static Task StartNewSessionAsync( this IChatAgent chatAgent )
        {
            return chatAgent.StartNewSessionAsync( null, null );
        }
    }
}
