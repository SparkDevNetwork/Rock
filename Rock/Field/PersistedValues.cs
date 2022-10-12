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

namespace Rock.Field
{
    /// <summary>
    /// All the persisted values from <see cref="IFieldType.GetPersistedValues(string, Dictionary{string, string})"/>.
    /// </summary>
    public class PersistedValues
    {
        /// <summary>
        /// Gets or sets the persisted text value.
        /// </summary>
        /// <value>The persisted text value.</value>
        public string TextValue { get; set; }

        /// <summary>
        /// Gets or sets the persisted HTML value.
        /// </summary>
        /// <value>The persisted HTML value.</value>
        public string HtmlValue { get; set; }

        /// <summary>
        /// Gets or sets the persisted condensed text value.
        /// </summary>
        /// <value>The persisted condensed text value.</value>
        public string CondensedTextValue { get; set; }

        /// <summary>
        /// Gets or sets the persisted condensed HTML value.
        /// </summary>
        /// <value>The persisted condensed HTML value.</value>
        public string CondensedHtmlValue { get; set; }
    }
}
