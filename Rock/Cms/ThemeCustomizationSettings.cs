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

using Rock.Enums.Cms;

namespace Rock.Cms
{
    /// <summary>
    /// The customization settings for a next-generation theme. This needs to
    /// contain all the settings that relate to customizing a theme. If any data
    /// is stored only on disk then it will not work correctly across other
    /// servers in a web farm.
    /// </summary>
    internal class ThemeCustomizationSettings
    {
        /// <summary>
        /// Any custom CSS overrides that have been manually typed in by the
        /// user.
        /// </summary>
        public string CustomOverrides { get; set; }

        /// <summary>
        /// Specifies which icon sets are enabled for the theme. If this value
        /// is <c>null</c> then the value from <see cref="ThemeDefinition.AvailableIconSets"/>
        /// should be used instead.
        /// </summary>
        public ThemeIconSet? EnabledIconSets { get; set; }

        /// <summary>
        /// The default FontAwesome icon weight to include in the theme. This
        /// will allow uses such as <c>fa fa-star</c>.
        /// </summary>
        public ThemeFontAwesomeWeight DefaultFontAwesomeWeight { get; set; }

        /// <summary>
        /// Any additional weights to be included in the theme. This will allow
        /// weight specific icons such as <c>fas fa-star</c> or <c>fal fa-star</c>.
        /// </summary>
        public List<ThemeFontAwesomeWeight> AdditionalFontAwesomeWeights { get; set; }

        /// <summary>
        /// The raw values for the various field variables. These will be used
        /// to generate the actual CSS variables file after being formatted
        /// by the field logic.
        /// </summary>
        public Dictionary<string, string> VariableValues { get; set; }
    }
}
