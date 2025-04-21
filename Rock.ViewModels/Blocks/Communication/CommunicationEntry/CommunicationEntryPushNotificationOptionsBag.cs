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
using System;
using System.Collections.Generic;

using Rock.ViewModels.Rest.Controls;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the push notification options for the Communication Entry block.
    /// </summary>
    public class CommunicationEntryPushNotificationOptionsBag
    {
        /// <summary>
        /// Gets or sets the mobile page.
        /// </summary>
        /// <value>
        /// The mobile page.
        /// </value>
        public PageRouteValueBag MobilePage { get; set; }

        /// <summary>
        /// Gets or sets the mobile page query string.
        /// </summary>
        /// <value>
        /// The mobile page query string.
        /// </value>
        public Dictionary<string, string> MobilePageQueryString { get; set; }

        /// <summary>
        /// Gets or sets the mobile application unique identifier.
        /// </summary>
        /// <value>
        /// The mobile application unique identifier.
        /// </value>
        public Guid? MobileApplicationGuid { get; set; }

        /// <summary>
        /// Gets or sets the link-to-page URL.
        /// </summary>
        /// <value>
        /// The link-to-page URL.
        /// </value>
        public string LinkToPageUrl { get; set; }
    }
}
