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

namespace Rock.Model.Core.Category.Options
{
    /// <summary>
    /// Options that define the behavior when retrieving child categories.
    /// </summary>
    public class ChildCategoryQueryOptions
    {
        /// <summary>
        /// Gets or sets the parent unique identifier whose children are to
        /// be retrieved. If null then the root categories are being requested.
        /// </summary>
        /// <value>The parent unique identifier.</value>
        public Guid? ParentGuid { get; set; }

        /// <summary>
        /// Gets or sets the entity type unique identifier to limit the
        /// results to.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the category unique identifiers to be included in the
        /// results. If this contains any values then any categories that do
        /// not match one of these values will be excluded. If this value is set
        /// then <see cref="ExcludeCategoryGuids"/> is ignored.
        /// </summary>
        /// <value>The unique identifiers to limit category results to.</value>
        public List<Guid> IncludeCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the unique category identifiers to be excluded from
        /// the results. If this contains any values then any categories that
        /// match one of these values will be excluded. Only takes effect if
        /// <see cref="IncludeCategoryGuids"/> is null or empty.
        /// </summary>
        /// <value>The unique category identifiers to exclude from the results.</value>
        public List<Guid> ExcludeCategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier that is used to filter category
        /// results. If not blank, then the category EntityTypeQualifierColumn
        /// property must match this value.
        /// </summary>
        /// <value>The entity qualifier used to filter results.</value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity qualifier value that is used to filter
        /// category results. If both this and <see cref="EntityTypeQualifierColumn"/>
        /// are not blank, then the category EntityTypeQualifierValue property
        /// must match this value.
        /// </summary>
        /// <value>The entity qualifier value used to filter results.</value>
        public string EntityTypeQualifierValue { get; set; }
    }
}
