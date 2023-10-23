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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetEntityTags API action of
    /// the EntityTagList control.
    /// </summary>
    public class EntityTagListGetEntityTagsOptionsBag
    {
        /// <summary>
        /// Gets or sets the entity type unique identifier.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity key used with <see cref="EntityTypeGuid"/>
        /// to locate the entity.
        /// </summary>
        /// <value>The entity key.</value>
        public string EntityKey { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier column.
        /// </summary>
        /// <value>
        /// The entity qualifier column.
        /// </value>
        public string EntityQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier value.
        /// </summary>
        /// <value>
        /// The entity qualifier value.
        /// </value>
        public string EntityQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the category unique identifier to limit tags to. If a
        /// value is specified then any tags must match the category.
        /// </summary>
        /// <value>The category unique identifier to limit tags to.</value>
        public Guid? CategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets whether to include inactive tags in the results.
        /// </summary>
        /// <value>Whether to include inactive tags</value>
        public bool ShowInactiveTags { get; set; } = false;

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
