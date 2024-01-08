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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The item details for the Check-In Type Detail block General Settings.
    /// </summary>
    public class CheckInGeneralSettingsBag
    {
        /// <summary>
        /// Gets or sets the type of the check in.
        /// </summary>
        /// <value>
        /// The type of the check in.
        /// </value>
        public string CheckInType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow checkout at kiosk].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow checkout at kiosk]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCheckoutAtKiosk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow checkout in manager].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow checkout in manager]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCheckoutInManager { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable presence].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable presence]; otherwise, <c>false</c>.
        /// </value>
        public bool EnablePresence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable manager].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable manager]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableManager { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable override].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable override]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableOverride { get; set; }

        /// <summary>
        /// Gets or sets the achievement types.
        /// </summary>
        /// <value>
        /// The achievement types.
        /// </value>
        public List<string> AchievementTypes { get; set; }

        /// <summary>
        /// Gets or sets the automatic select days back.
        /// </summary>
        /// <value>
        /// The automatic select days back.
        /// </value>
        public int AutoSelectDaysBack { get; set; }

        /// <summary>
        /// Gets or sets the automatic select options.
        /// </summary>
        /// <value>
        /// The automatic select options.
        /// </value>
        public string AutoSelectOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use same options].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use same options]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSameOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [prevent inactive people].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prevent inactive people]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventInactivePeople { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [prevent duplicate checkin].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prevent duplicate checkin]; otherwise, <c>false</c>.
        /// </value>
        public bool PreventDuplicateCheckin { get; set; }
    }
}
