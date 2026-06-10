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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationList
{
    /// <summary>
    /// The box that contains all the initialization information for the Check-in Configuration List block.
    /// </summary>
    public class CheckInConfigurationListInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the list of "sort by" items the individual may select.
        /// </summary>
        public List<ListItemBag> SortByItems { get; set; }

        /// <summary>
        /// Gets or sets the currently effective "sort by" value (the individual's stored preference, or the block
        /// default when none is stored).
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets whether to show the add check-in configuration button.
        /// </summary>
        public bool ShowAddCheckInConfigurationButton { get; set; }

        /// <summary>
        /// Gets or sets whether to show the page link under related settings that allows the configuration of classic labels.
        /// </summary>
        public bool ShowClassicLabelSettings { get; set; }

        /// <summary>
        /// Gets or sets the list of check-in configurations.
        /// </summary>
        public List<CheckInConfigurationBag> CheckInConfigurations { get; set; }
    }
}
