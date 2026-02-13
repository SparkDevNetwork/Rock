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

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// The options that can be used when building a new chat agent.
    /// </summary>
    [RockInternal( "18.0" )]
    internal class ChatAgentOptions
    {
        /// <summary>
        /// Enables additional debug information to be made available.
        /// </summary>
        public bool IsDebugEnabled { get; set; }

        /// <summary>
        /// When creating a new chat agent, this option can be used to specify
        /// that security should be checked on the current person for the
        /// active request. This only filters out skills and tools. It will
        /// not check permissions for the agent itself.
        /// </summary>
        public bool IsSecurityEnabled { get; set; }
    }
}
