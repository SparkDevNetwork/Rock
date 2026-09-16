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

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// A single attribute key/value pair for a transaction detail line item,
    /// carrying both the raw value and a formatted display value.
    /// </summary>
    public class AttributeValueBag
    {
        /// <summary>
        /// Gets or sets the attribute key used to identify this attribute.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the human-readable attribute name shown as a label in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the raw stored value of the attribute.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the formatted display value of the attribute (e.g. a resolved defined value label).
        /// </summary>
        public string FormattedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit this attribute value.
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute value is required.
        /// </summary>
        public bool IsRequired { get; set; }
    }
}
