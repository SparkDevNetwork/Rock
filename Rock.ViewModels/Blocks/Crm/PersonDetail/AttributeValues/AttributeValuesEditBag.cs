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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.AttributeValues
{
    /// <summary>
    /// The data required to render the edit view of the displayed attributes.
    /// Editable attributes are shown as editors while attributes the person
    /// may only view are shown as read-only formatted values, both interleaved
    /// in display order.
    /// </summary>
    public class AttributeValuesEditBag
    {
        /// <summary>
        /// Gets or sets the ordered groups of fields to render. A single group
        /// with no category name is used when the block is not grouping by
        /// category.
        /// </summary>
        public List<AttributeValuesFieldGroupBag> FieldGroups { get; set; }

        /// <summary>
        /// Gets or sets the public attribute metadata, keyed by attribute key.
        /// Editable attributes carry edit metadata; view-only attributes carry
        /// view metadata.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values, keyed by attribute key. Editable
        /// attributes carry the edit value; view-only attributes carry the
        /// formatted view value.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the security grant token used by field types that
        /// require additional security context while editing.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
