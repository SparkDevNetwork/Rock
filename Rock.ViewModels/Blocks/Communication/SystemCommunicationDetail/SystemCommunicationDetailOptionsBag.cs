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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SystemCommunicationDetail
{
    /// <summary>
    /// Represents the configuration options for the System Communication Detail block.
    /// </summary>
    public class SystemCommunicationDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this is a new system communication.
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in read-only mode
        /// because the current user lacks edit authorization.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Rock Mobile Push transport is configured.
        /// When true, "Link to Mobile Page" and "Show Details" open actions are available.
        /// When false, only "No Action" and "Link to URL" are available.
        /// </summary>
        public bool IsRockMobilePushTransportConfigured { get; set; }

        /// <summary>
        /// Gets or sets the list of system phone numbers available for SMS sending.
        /// </summary>
        public List<ListItemBag> SmsFromSystemPhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the list of mobile applications available for push notifications.
        /// </summary>
        public List<ListItemBag> PushMobileApplications { get; set; }
    }
}
