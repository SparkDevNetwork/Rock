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

namespace Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects
{
    /// <summary>
    /// The church directory's answer to a campus publish (Argus spec, section 5.3). The
    /// counts are only for the campus job's status message.
    /// </summary>
    internal class MobileAppCampusesResponse
    {
        /// <summary>
        /// The number of campuses the directory now holds for the church.
        /// </summary>
        public int CampusCount { get; set; }

        /// <summary>
        /// The number of those campuses that have coordinates, and so can be found by a
        /// near-me search.
        /// </summary>
        public int GeocodedCount { get; set; }
    }
}
