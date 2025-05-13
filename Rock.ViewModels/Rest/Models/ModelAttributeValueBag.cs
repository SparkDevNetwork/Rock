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
        /// The raw attribute value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The formatted text value.
        /// </summary>
        public string TextValue { get; set; }

        /// <summary>
        /// The formatted HTML value.
        /// </summary>
        public string HtmlValue { get; set; }

        /// <summary>
        /// The formatted condensed text value.
        /// </summary>
        public string CondensedTextValue { get; set; }

        /// <summary>
        /// The formatted condensed HTML value.
        /// </summary>
        public string CondensedHtmlValue { get; set; }
    }
}
