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

namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// The church's enrollment in the shared mobile application, as the
    /// Connected Services card shows it.
    /// </summary>
    public class MobileAppConfigurationBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the platform mobile app has
        /// been built and deployed. Enable stays off until it has.
        /// </summary>
        public bool IsPlatformSiteBuilt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the church is listed in the
        /// shared mobile application right now.
        /// </summary>
        public bool IsEnrolled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enrollment goes through a fake
        /// gateway on this server instead of Spark. Only true on a development
        /// machine.
        /// </summary>
        public bool IsFakeGateway { get; set; }

        /// <summary>
        /// Gets or sets the church name shown in the directory. Prefilled with
        /// the organization name before the first enable.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the directory color, <c>#RRGGBB</c>.
        /// </summary>
        public string BrandColor { get; set; }

        /// <summary>
        /// Gets or sets the directory logo.
        /// </summary>
        public ListItemBag Logo { get; set; }

        /// <summary>
        /// Gets or sets the API URL the shared app is handed, from the Public
        /// Application Root global attribute. Read only on the card.
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the church code, once the directory has minted one. Kept
        /// after a disable.
        /// </summary>
        public string ChurchCode { get; set; }

        /// <summary>
        /// Gets or sets the URL to print on the church's QR poster. Kept after a
        /// disable.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the issue Spark's manifest reports for the service, if
        /// any. Enable stays off while there is one.
        /// </summary>
        public string ManifestIssue { get; set; }
    }
}
