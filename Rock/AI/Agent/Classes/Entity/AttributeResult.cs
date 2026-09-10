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

using Rock.AI.Agent.Classes.Common;
using Rock.ViewModels.Utility;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a single attribute to the agent.
    /// </summary>
    public class AttributeResult
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The key that identifies this specific attribute in requests and responses.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The unique identifier of the attribute. Required when a value must
        /// reference this attribute, which is how workflow action settings and
        /// criteria store attribute references.
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// The description of the attribute. This is the help text an
        /// administrator would see, and is often the only explanation of what a
        /// value is expected to mean.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Describes the format of the values for this attribute.
        /// </summary>
        public string ValueFormat { get; set; }

        /// <summary>
        /// The field type that determines how values for this attribute are
        /// stored and validated. Use its key with the field type tools to find
        /// the configuration qualifiers this attribute supports.
        /// </summary>
        public KeyNameResult FieldType { get; set; }

        /// <summary>
        /// The values this attribute is known to accept, when the field type can
        /// enumerate them. Null when the field type does not describe its values,
        /// which is not the same as the attribute having no valid values.
        /// </summary>
        public List<ListItemBag> Values { get; set; }

        /// <summary>
        /// Indicates that <see cref="Values"/> is the complete set of accepted
        /// values rather than a sample of common ones. Null when
        /// <see cref="Values"/> is null.
        /// </summary>
        public bool? IsCompleteList { get; set; }

        /// <summary>
        /// How to obtain the accepted values when they cannot be read from
        /// <see cref="Values"/> alone, either because the list is too large to
        /// enumerate or because it lives on another record. Names the record and
        /// the identifier needed to reach it. Null when the values are already
        /// fully described.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// The order of the attribute relative to the other attributes on the
        /// same entity.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Indicates if this attribute is required.
        /// </summary>
        public bool? IsRequired { get; set; }

        /// <summary>
        /// Indicates that this attribute's values are read-only and can't be changed.
        /// </summary>
        public bool? IsReadOnly { get; set; }
    }
}
