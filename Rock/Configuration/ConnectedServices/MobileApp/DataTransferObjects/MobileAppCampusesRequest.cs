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
    /// The church's campus set, sent to the church directory by the daily campus job.
    /// Shaped exactly as the directory's campus publish request (Argus spec, section 5.3).
    /// It is a full replace: whatever is sent becomes the church's campus set.
    /// </summary>
    internal class MobileAppCampusesRequest
    {
        /// <summary>
        /// The campuses array, already serialized by
        /// <see cref="Rock.Mobile.PlatformMobileAppCampusPayload"/>. Carried as a parsed
        /// JSON element for the same reason as
        /// <see cref="MobileAppConfigurationRequest.Campuses"/>: the fields and values sent
        /// are exactly those the campus hash was taken over.
        /// </summary>
        public JsonElement Campuses { get; set; }
    }
}
