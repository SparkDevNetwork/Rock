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

namespace Rock.Web.UI
{
    /// <summary>
    /// A Block that can be used in an <see cref="Rock.Web.UI.Controls.ItemFromBlockPicker"></see>
    /// </summary>
    public interface IPickerBlock
    {
        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        string SelectedValue { get; set; }

        /// <summary>
        /// Gets the text representing the selected item
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        string GetSelectedText( string selectedValue );

        /// <summary>
        /// Any Picker Settings that be configured
        /// </summary>
        /// <value>
        /// The picker settings.
        /// </value>
        Dictionary<string, string> PickerSettings { get; set; }

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        event EventHandler SelectItem;
    }
}