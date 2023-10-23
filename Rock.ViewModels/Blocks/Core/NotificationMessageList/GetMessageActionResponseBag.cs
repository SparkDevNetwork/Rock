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

using Rock.ViewModels.Core;

namespace Rock.ViewModels.Blocks.Core.NotificationMessageList
{
    /// <summary>
    /// Describes the response data sent back from the GetMessageAction
    /// block action.
    /// </summary>
    public class GetMessageActionResponseBag
    {
        /// <summary>
        /// Gets or sets the action to be performed for the message.
        /// </summary>
        /// <value>The action to be performed for the message.</value>
        public NotificationMessageActionBag Action { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was deleted.
        /// </summary>
        /// <value><c>true</c> if the message was deleted; otherwise, <c>false</c>.</value>
        public bool IsDeleted { get; set; }
    }
}
