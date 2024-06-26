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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A representation of a saved check-in configuration on the server.
    /// </summary>
    public class SavedCheckInConfigurationBag
    {
        /// <summary>
        /// Gets or sets the identifier of the configuration.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of this saved configuration.
        /// </summary>
        /// <value>The configuration name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text that describes this saved configuration.
        /// </summary>
        /// <value>The description text.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the campuses that this saved configuration can be
        /// used with.
        /// </summary>
        /// <value>The campuses.</value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the settings for this saved configuration.
        /// </summary>
        /// <value>The configuration settings.</value>
        public SavedCheckInConfigurationSettingsBag Settings { get; set; }
    }
}
