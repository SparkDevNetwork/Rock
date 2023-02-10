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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SystemPhoneNumberDetail
{
    /// <summary>
    /// Class SystemPhoneNumberBag
    /// </summary>
    public class SystemPhoneNumberBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the person alias who should receive responses to the SMS
        /// number. This person must have a phone number with SMS enabled
        /// or no response will be sent.
        /// </summary>
        public ListItemBag AssignedToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this phone number is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance support SMS.
        /// </summary>
        public bool IsSmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this phone number will
        /// forward incoming messages to Rock.Model.SystemPhoneNumber.AssignedToPersonAliasId.
        /// </summary>
        public bool IsSmsForwardingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the phone number.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the phone number. This should be in E.123 format,
        /// such as +16235553324.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the mobile application site. This is used
        /// when determining what devices to send push notifications to.
        /// </summary>
        public ListItemBag MobileApplicationSite { get; set; }

        /// <summary>
        /// Gets or sets the SMS notification group. Active members of this
        /// group will be notified when a new SMS message comes in to this
        /// phone number.
        /// </summary>
        public ListItemBag SmsNotificationGroup { get; set; }

        /// <summary>
        /// Gets or sets the workflow type that will be launched when an
        /// SMS message is received on this number.
        /// </summary>
        public ListItemBag SmsReceivedWorkflowType { get; set; }
    }
}
