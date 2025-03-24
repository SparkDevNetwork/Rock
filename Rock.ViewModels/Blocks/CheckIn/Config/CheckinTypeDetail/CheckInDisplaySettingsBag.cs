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
    /// The item details for the Check-In Type Detail block Display Settings.
    /// </summary>
    public class CheckInDisplaySettingsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether [hide photos].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide photos]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePhotos { get; set; }

        /// <summary>
        /// Gets or sets the start template.
        /// </summary>
        /// <value>
        /// The start template.
        /// </value>
        public string StartTemplate { get; set; }

        /// <summary>
        /// Gets or sets the family select template.
        /// </summary>
        /// <value>
        /// The family select template.
        /// </value>
        public string FamilySelectTemplate { get; set; }

        /// <summary>
        /// Gets or sets the person select template.
        /// </summary>
        /// <value>
        /// The person select template.
        /// </value>
        public string PersonSelectTemplate { get; set; }

        /// <summary>
        /// Gets or sets the success template override display mode.
        /// </summary>
        /// <value>
        /// The success template override display mode.
        /// </value>
        public string SuccessTemplateOverrideDisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the success template.
        /// </summary>
        /// <value>
        /// The success template.
        /// </value>
        public string SuccessTemplate { get; set; }
    }
}
