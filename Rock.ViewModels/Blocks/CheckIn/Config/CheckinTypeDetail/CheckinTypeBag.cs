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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The item details for the Check-In Type Detail block.
    /// </summary>
    public class CheckinTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Description of the GroupType.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupType. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the scheduled times.
        /// </summary>
        /// <value>
        /// The scheduled times.
        /// </value>
        public string ScheduledTimes { get; set; }

        /// <summary>
        /// Gets or sets the type of the check in.
        /// </summary>
        /// <value>
        /// The type of the check in.
        /// </value>
        public string CheckInType { get; set; }

        /// <summary>
        /// Gets or sets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public string SearchType { get; set; }

        /// <summary>
        /// Gets or sets the phone number compare.
        /// </summary>
        /// <value>
        /// The phone number compare.
        /// </value>
        public string PhoneNumberCompare { get; set; }

        /// <summary>
        /// Gets or sets the general settings.
        /// </summary>
        /// <value>
        /// The general settings.
        /// </value>
        public CheckInGeneralSettingsBag GeneralSettings { get; set; }

        /// <summary>
        /// Gets or sets the barcode settings.
        /// </summary>
        /// <value>
        /// The barcode settings.
        /// </value>
        public CheckInBarcodeSettingsBag BarcodeSettings { get; set; }

        /// <summary>
        /// Gets or sets the search settings.
        /// </summary>
        /// <value>
        /// The search settings.
        /// </value>
        public CheckInSearchSettingsBag SearchSettings { get; set; }

        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
        public CheckInHeaderTextBag HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the display settings.
        /// </summary>
        /// <value>
        /// The display settings.
        /// </value>
        public CheckInDisplaySettingsBag DisplaySettings { get; set; }

        /// <summary>
        /// Gets or sets the registration settings.
        /// </summary>
        /// <value>
        /// The registration settings.
        /// </value>
        public CheckInRegistrationSettingsBag RegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the advanced settings.
        /// </summary>
        /// <value>
        /// The advanced settings.
        /// </value>
        public CheckInAdvancedSettingsBag AdvancedSettings { get; set; }
    }
}
