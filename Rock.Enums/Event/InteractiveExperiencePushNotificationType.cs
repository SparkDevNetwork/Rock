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

namespace Rock.Enums.Event
{
    /// <summary>
    /// Represents the type of push notification to use for the Interactive
    /// Experience system when posting questions.
    /// </summary>
    public enum InteractiveExperiencePushNotificationType
    {
        /// <summary>
        /// A push notification is never sent when a question is posted.
        /// </summary>
        Never = 0,

        /// <summary>
        /// A push notification is always sent when a question is posted.
        /// </summary>
        EveryAction = 1,

        /// <summary>
        /// The moderator decides if the push notification should be sent
        /// when they post a question.
        /// </summary>
        SpecificActions = 2
    }
}
