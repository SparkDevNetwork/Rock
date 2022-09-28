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

using Rock.Enums.Cms;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionDetail
{
    /// <summary>
    /// Represents a single attribute filter for a content collection.
    /// </summary>
    public class AttributeFilterBag
    {
        /// <summary>
        /// Gets or sets the friendly name of the attribute.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the internal identification key of the attribute.
        /// </summary>
        public string AttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the enabled state of this attribute filter.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the friendly field type name used by this filter.
        /// </summary>
        public string FieldTypeName { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier used by this filter.
        /// </summary>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the names of the sources that make up this filter.
        /// </summary>
        public List<string> SourceNames { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if this filter is in an
        /// inconsistent state. If <c>true</c> then editing should not be
        /// allowed.
        /// </summary>
        public bool IsInconsistent { get; set; }

        /// <summary>
        /// Gets or sets the friendly label to use when displaying this filter.
        /// </summary>
        public string FilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the type of control to use when displaying this filter.
        /// </summary>
        public ContentCollectionFilterControl FilterControl { get; set; }

        /// <summary>
        /// Gets or sets if multiple selections are allowed.
        /// </summary>
        public bool IsMultipleSelection { get; set; }
    }
}
