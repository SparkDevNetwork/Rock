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

namespace Rock.Enums.Communication
{
    /// <summary>
    /// Condition for when a recipient no longer receives messages from a communication flow instance.
    /// </summary>
    public enum ExitConditionType
    {
        /// <summary>
        /// Recipient exits the communication flow instance after the last message is sent.
        /// </summary>
        LastMessageSent = 0,

        /// <summary>
        /// Recipient exits the communication flow instance if any email is opened.
        /// </summary>
        AnyEmailOpened = 1,

        /// <summary>
        /// Recipient exits the communication flow instance if any email is clicked through.
        /// </summary>
        AnyEmailClickedThrough = 2,

        /// <summary>
        /// Recipient exits the communication flow instance if conversion is achieved.
        /// </summary>
        ConversionAchieved = 3
    }
}
