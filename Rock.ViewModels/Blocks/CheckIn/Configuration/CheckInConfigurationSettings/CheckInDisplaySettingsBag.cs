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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The display settings for a check-in configuration. Controls what kind of content is displayed during
    /// check-in.
    /// </summary>
    public class CheckInDisplaySettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the content channel used to display promotions on the kiosk
        /// welcome screen. Only supported by next-gen check-in.
        /// </summary>
        public Guid? PromotionsContentChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets whether person photos are hidden when selecting people from the family who are
        /// checking in.
        /// </summary>
        public bool HidePhotos { get; set; }

        /// <summary>
        /// Gets or sets whether the room location options include a count of how many people are currently
        /// checked into that location.
        /// </summary>
        public bool DisplayLocationCount { get; set; }

        /// <summary>
        /// Gets or sets the achievement types to recognize during check-in celebrations.
        /// </summary>
        public List<string> AchievementTypes { get; set; }
    }
}
