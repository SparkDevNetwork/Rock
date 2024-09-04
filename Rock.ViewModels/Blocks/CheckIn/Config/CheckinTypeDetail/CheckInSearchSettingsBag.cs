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

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The item details for the Check-In Type Detail block Search Settings.
    /// </summary>
    public class CheckInSearchSettingsBag
    {
        /// <summary>
        /// Gets or sets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public string SearchType { get; set; }

        /// <summary>
        /// Gets or sets the maximum results.
        /// </summary>
        /// <value>
        /// The maximum results.
        /// </value>
        public int MaxResults { get; set; }

        /// <summary>
        /// Gets or sets the minimum length of the phone.
        /// </summary>
        /// <value>
        /// The minimum length of the phone.
        /// </value>
        public int MinPhoneLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the phone.
        /// </summary>
        /// <value>
        /// The maximum length of the phone.
        /// </value>
        public int MaxPhoneLength { get; set; }

        /// <summary>
        /// Gets or sets the type of the phone search.
        /// </summary>
        /// <value>
        /// The type of the phone search.
        /// </value>
        public string PhoneSearchType { get; set; }
    }
}
