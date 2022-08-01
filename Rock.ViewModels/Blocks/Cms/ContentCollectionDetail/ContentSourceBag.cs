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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionDetail
{
    /// <summary>
    /// Identifies a single content source that will be displayed in the list
    /// of collection sources.
    /// </summary>
    public class ContentSourceBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this content collection source
        /// record. This is not the unique identifier of the target entity.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of this content collection source.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity type this source
        /// references.
        /// </summary>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity this source references.
        /// </summary>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Gets or sets the CSS icon class to use when displaying this
        /// content collection source.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS color to use when displaying this content
        /// collection source. This will be in the format #rrggbb.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the attributes that are enabled for this content
        /// collection source.
        /// </summary>
        public List<ListItemBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the number of occurrences to include when indexing
        /// this source.
        /// </summary>
        public int OccurrencesToShow { get; set; }

        /// <summary>
        /// Gets or sets the number of items that exist on this content collection source.
        /// </summary>
        public int ItemCount { get; set; }
    }
}
