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

namespace Rock.ViewModels.Blocks.Group.GroupSimpleRegisterConfirm
{
    /// <summary>
    /// The bag that contains the result of the Group Simple Register Confirm block.
    /// </summary>
    public class GroupSimpleRegisterConfirmBag
    {
        /// <summary>
        /// Gets or sets the message to display.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the bold heading shown before the message (e.g. "Success" or "Sorry").
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// Gets or sets the NotificationBox alert type (e.g. "success" or "danger").
        /// </summary>
        public string AlertType { get; set; }
    }
}
