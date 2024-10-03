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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ThemeDetail
{
    /// <summary>
    /// The details about a theme being viewed or edited.
    /// </summary>
    public class ThemeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description of the theme, this cannot be edited.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Rock.Model.Theme is a part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the theme, this cannot be edited.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The purpose of the theme, this cannot be edited.
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Specifies which icon sets are supported by the theme. This cannot
        /// be edited.
        /// </summary>
        public ThemeIconSet AvailableIconSets { get; set; }

        /// <summary>
        /// Specifies which icon sets are enabled for the theme.
        /// </summary>
        public ThemeIconSet EnabledIconSets { get; set; }

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
        /// The fields that should be displayed when editing the theme.
        /// </summary>
        public List<ThemeFieldBag> Fields { get; set; }

        /// <summary>
        /// The CSS overrides that were manually entered by the person.
        /// </summary>
        public string CustomOverrides { get; set; }

        /// <summary>
        /// The values of the CSS variables.
        /// </summary>
        public Dictionary<string, string> VariableValues { get; set; }
    }
}
