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

using Rock.Enums.Core;

namespace Rock.ViewModels.Core
{
    /// <summary>
    /// Type of audit done to an entity
    /// </summary>
    public class NotificationMessageActionBag
    {
        /// <summary>
        /// Gets or sets the type of action to be performed.
        /// </summary>
        /// <value>The type of action to be performed.</value>
        public NotificationMessageActionType Type { get; set; }

        /// <summary>
        /// Gets or sets the message to be displayed. Only valid if <see cref="Type"/>
        /// is the value <see cref="NotificationMessageActionType.ShowMessage"/>.
        /// </summary>
        /// <value>The message to be displayed.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page to link to. Only valid if
        /// <see cref="Type"/> is the value <see cref="NotificationMessageActionType.LinkToPage"/>.
        /// </summary>
        /// <value>The URL of the page to link to.</value>
        public string Url { get; set; }
    }
}
