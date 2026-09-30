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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.PlatformMobileAppDetail
{
    /// <summary>
    /// The church owned settings of the platform mobile application, edited together in
    /// the Platform Mobile App Detail block and saved with one builder run.
    /// </summary>
    public class PlatformMobileAppSettingsBag
    {
        /// <summary>
        /// Gets or sets the Guid of the content collection the app's content page shows,
        /// or null if the church has none.
        /// </summary>
        public Guid? ContentCollectionGuid { get; set; }

        /// <summary>
        /// Gets or sets the in-app logo shown over a light background.
        /// </summary>
        public ListItemBag LightLogo { get; set; }

        /// <summary>
        /// Gets or sets the in-app logo shown over a dark background.
        /// </summary>
        public ListItemBag DarkLogo { get; set; }

        /// <summary>
        /// Gets or sets the strong shade of the church's color, used for buttons and links.
        /// Dark mode swaps it with the soft shade.
        /// </summary>
        public string ColorStrong { get; set; }

        /// <summary>
        /// Gets or sets the soft shade of the church's color.
        /// </summary>
        public string ColorSoft { get; set; }
    }
}
