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

using System.Text.Json;

namespace Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects
{
    /// <summary>
    /// The church's details sent to the church directory when it enrolls or
    /// updates its listing in the shared mobile application. Shaped exactly as
    /// the directory's enroll request (Argus spec, section 5.1).
    /// </summary>
    internal class MobileAppConfigurationRequest
    {
        /// <summary>
        /// The name shown for the church in the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The directory color and logo.
        /// </summary>
        public MobileAppBranding Branding { get; set; }

        /// <summary>
        /// The connection the shared app uses to reach this Rock.
        /// </summary>
        public MobileAppConnection Connection { get; set; }

        /// <summary>
        /// The campuses array, already serialized by
        /// <see cref="Rock.Mobile.PlatformMobileAppCampusPayload"/>. It is
        /// carried as a parsed JSON element so the fields and values sent are
        /// exactly those the campus hash was taken over. The writer may escape
        /// some characters differently (an apostrophe in a campus name, for
        /// example), which the directory decodes to the same text.
        /// </summary>
        public JsonElement? Campuses { get; set; }
    }

    /// <summary>
    /// The directory's branding for one church.
    /// </summary>
    internal class MobileAppBranding
    {
        /// <summary>
        /// The directory color, as <c>#RRGGBB</c>.
        /// </summary>
        public string BrandColor { get; set; }

        /// <summary>
        /// The absolute URL of the directory logo, or <c>null</c> to remove it.
        /// </summary>
        public string LogoUrl { get; set; }
    }

    /// <summary>
    /// The connection the shared app is handed for one church.
    /// </summary>
    internal class MobileAppConnection
    {
        /// <summary>
        /// The church's public API root, <c>{PublicApplicationRoot}api</c>.
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// The platform Site's low-privilege bootstrap key.
        /// </summary>
        public string ApiKey { get; set; }
    }
}
