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
    /// The church directory's answer to an enroll or update (Argus spec,
    /// section 5.1). Rock keeps <see cref="ChurchCode"/> and <see cref="Link"/>.
    /// </summary>
    internal class MobileAppConfigurationResponse
    {
        /// <summary>
        /// The church's code, minted on its first enrollment.
        /// </summary>
        public string ChurchCode { get; set; }

        /// <summary>
        /// The URL to print on the church's QR poster.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Whether the directory now lists the church.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The name the directory stored.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The location line the directory renders from the campuses, or
        /// <c>null</c> when no campus has a city and state.
        /// </summary>
        public string Location { get; set; }
    }
}
