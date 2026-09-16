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

namespace Rock.ViewModels.Blocks.Store.StoreHeader
{
    /// <summary>
    /// Contains the organization data sent to the Store Header block for rendering
    /// the organization card. Mapped server-side from the Rock Store organization model.
    /// </summary>
    public class StoreOrganizationBag
    {
        /// <summary>
        /// Gets or sets the name of the organization.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the organization's logo image.
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Gets or sets the city the organization is located in.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state the organization is located in.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the organization's average weekly attendance.
        /// </summary>
        public int AverageWeeklyAttendance { get; set; }
    }
}
