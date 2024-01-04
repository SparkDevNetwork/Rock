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
    /// The item details for the Check-In Type Detail block Advanced Settings.
    /// </summary>
    public class CheckInAdvancedSettingsBag
    {
        /// <summary>
        /// Gets or sets the search regex.
        /// </summary>
        /// <value>
        /// The search regex.
        /// </value>
        public string SearchRegex { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval.
        /// </summary>
        /// <value>
        /// The refresh interval.
        /// </value>
        public int? RefreshInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [age required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [age required]; otherwise, <c>false</c>.
        /// </value>
        public bool AgeRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [grade required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [grade required]; otherwise, <c>false</c>.
        /// </value>
        public bool GradeRequired { get; set; }

        /// <summary>
        /// Gets or sets the ability level determination.
        /// </summary>
        /// <value>
        /// The ability level determination.
        /// </value>
        public string AbilityLevelDetermination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display loc count].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display loc count]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayLocCount { get; set; }
    }
}
