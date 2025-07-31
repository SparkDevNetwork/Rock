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

namespace Rock.Enums.Core.Automation.Triggers
{
    /// <summary>
    /// The type of rule used to determine if a chat message should trigger an automation event.
    /// </summary>
    public enum ChatMessageCriteriaRuleType
    {
        /// <summary>
        /// The message was sent within a channel of a specified type.
        /// </summary>
        ChannelType = 0,

        /// <summary>
        /// The message was sent within a specified channel.
        /// </summary>
        Channel = 1,

        /// <summary>
        /// The message contains a simple, case-insensitive string value.
        /// </summary>
        MessageContains = 2,

        /// <summary>
        /// The message matches a Regex pattern.
        /// </summary>
        MessagePattern = 3
    }
}
