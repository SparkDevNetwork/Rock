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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options sent when requesting the child items for a universal item
    /// tree field type.
    /// </summary>
    public class UniversalItemTreePickerOptionsBag
    {
        /// <summary>
        /// Gets or sets the general purpose context provided by the field
        /// type control.
        /// </summary>
        /// <value>The general purpose context.</value>
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets the values that need to be expanded to. This is used
        /// when opening the tree view with an already selected value. Each
        /// selected value is included in this property. When getting the list
        /// of root items, you should automatically expand your results until
        /// each of these values is reached.
        /// </summary>
        /// <value>The values that should be automatically expanded to.</value>
        public List<string> ExpandToValues { get; set; }

        /// <summary>
        /// Gets or sets the parent value.
        /// </summary>
        /// <value>The parent value or <c>null</c> to indicate the root items.</value>
        public string ParentValue { get; set; }

        /// <summary>
        /// Gets or sets the security grant token.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
