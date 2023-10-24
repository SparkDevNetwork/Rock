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

namespace Rock.ViewModels.Rest.Models
{
    /// <summary>
    /// The object that contains a single attribute value of a model.
    /// </summary>
    public class ModelAttributeValueBag
    {
        /// <summary>
        /// Gets or sets the raw value.
        /// </summary>
        /// <value>The raw value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        /// <value>The text value.</value>
        public string TextValue { get; set; }

        /// <summary>
        /// Gets or sets the HTML value.
        /// </summary>
        /// <value>The HTML value.</value>
        public string HtmlValue { get; set; }

        /// <summary>
        /// Gets or sets the condensed text value.
        /// </summary>
        /// <value>The condensed text value.</value>
        public string CondensedTextValue { get; set; }

        /// <summary>
        /// Gets or sets the condensed HTML value.
        /// </summary>
        /// <value>The condensed HTML value.</value>
        public string CondensedHtmlValue { get; set; }
    }
}
