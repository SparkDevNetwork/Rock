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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// The configuration details of a single saved check-in configuration.
    /// </summary>
    public class SavedCheckInConfigurationSettingsBag
    {
        /// <summary>
        /// Gets or sets the check-in template identifier used for general
        /// kiosk configuration options.
        /// </summary>
        /// <value>The check-in template identifier.</value>
        public string TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the area identifiers that are valid for this
        /// configuration.
        /// </summary>
        /// <value>The area identifiers.</value>
        public List<string> AreaIds { get; set; }

        /// <summary>
        /// Gets or sets the name of the theme that will be used with this configuration.
        /// </summary>
        /// <value>The name of the theme.</value>
        public string ThemeName { get; set; }
    }
}
