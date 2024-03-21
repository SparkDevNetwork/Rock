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

namespace Rock.Enums.Controls
{
    /// <summary>
    /// The way to display the items for the item value pickers.
    /// </summary>
    public enum UniversalItemValuePickerDisplayStyle
    {
        /// <summary>
        /// Let the system decide the best way to display the list of options.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Display the list of options as a long list of items. For example,
        /// as a list of checkboxes or radio buttons.
        /// </summary>
        List = 1,

        /// <summary>
        /// Display the list of options in a condensed format. For example,
        /// as a drop down list.
        /// </summary>
        Condensed = 2
    }
}
