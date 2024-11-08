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

namespace Rock.ViewModels.Blocks.Cms.ThemeDetail
{
    /// <summary>
    /// A single UI field that will be displayed in the editor.
    /// </summary>
    public class ThemeFieldBag
    {
        /// <summary>
        /// The type of field represented by this instance.
        /// </summary>
        public ThemeFieldType Type { get; set; }

        /// <summary>
        /// The name of the field. When type is a variable then this contains
        /// the user friendly name of the variable. Otherwise it is the title
        /// of the heading or panel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of this field. For variables this is the help text.
        /// Other fields use this as additional text to display in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// The default value of the variable.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Determines if the panel should default to expanded. Only used by
        /// panel fields.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Determines if the text field should be displayed as a multiline
        /// input box.
        /// </summary>
        public bool IsMultiline { get; set; }

        /// <summary>
        /// The child fields of this field. Only used by panel fields.
        /// </summary>
        public List<ThemeFieldBag> Fields { get; set; }

        /// <summary>
        /// Specifies the input width of the text input box.
        /// </summary>
        public string InputWidth { get; set; }
    }
}
