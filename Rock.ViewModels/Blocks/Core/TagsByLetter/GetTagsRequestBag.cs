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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Core.TagsByLetter
{
    /// <summary>
    /// The request data sent from the client when fetching tags
    /// with the current filter state.
    /// </summary>
    public class GetTagsRequestBag
    {
        /// <summary>
        /// Gets or sets the ownership filter for the tag query.
        /// </summary>
        public TagOwnershipType OwnerType { get; set; }

        /// <summary>
        /// Gets or sets the optional entity type unique identifier to filter tags by.
        /// When <c>null</c>, tags for all entity types are returned.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive tags should be included.
        /// </summary>
        public bool IncludeInactive { get; set; }
    }
}
