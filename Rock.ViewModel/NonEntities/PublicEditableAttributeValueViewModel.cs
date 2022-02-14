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

namespace Rock.ViewModel.NonEntities
{
    /// <summary>
    /// Extends <see cref="PublicAttributeValueViewModel"/> to include additional
    /// properties required to make edits to the value.
    /// </summary>
    /// <seealso cref="PublicAttributeValueViewModel" />
    public class PublicEditableAttributeValueViewModel : PublicAttributeValueViewModel
    {
        /// <summary>
        /// Gets or sets the key that identifies the attribute on the entity.
        /// </summary>
        /// <value>
        /// The key that identifies the attribute on the entity.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute value is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this attribute value is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the configuration values.
        /// </summary>
        /// <value>
        /// The configuration values.
        /// </value>
        public Dictionary<string, string> ConfigurationValues { get; set; }
    }
}
