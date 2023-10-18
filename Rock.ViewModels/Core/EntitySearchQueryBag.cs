﻿// <copyright>
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

namespace Rock.ViewModels.Core
{
    /// <summary>
    /// Defines the details about an entity search request that will be
    /// executed.
    /// </summary>
    public class EntitySearchQueryBag
    {
        /// <summary>
        /// Gets or sets the expression that will be used to filter the query.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Where()</c> expression.</value>
        public string Where { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to group the results.
        /// This is processed after the <see cref="Where"/> expression.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>GroupBy()</c> expression.</value>
        public string GroupBy { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the the resulting items. This is processed after the <see cref="GroupBy"/>
        /// expression.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>Select()</c> expression.</value>
        public string Select { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to define the structure
        /// of the the resulting items. This is processed after the <see cref="GroupBy"/>
        /// expression and instead of the <see cref="Select"/> expression.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>SelectMany()</c> expression.</value>
        public string SelectMany { get; set; }

        /// <summary>
        /// Gets or sets the expression that will be used to sort the results.
        /// This is processed after the <see cref="Select"/> expression.
        /// </summary>
        /// <value>A <see cref="string"/> containing the dynamic LINQ <c>OrderBy()</c> expression.</value>
        public string Sort { get; set; }

        /// <summary>
        /// Gets or sets the number of items to skip before the first item the result set. This
        /// is processed after <see cref="Sort"/> performs sorting.
        /// </summary>
        /// <value>An optional <see cref="int"/> that specifies the number of results to skip.</value>
        public int? Offset { get; set; }

        /// <summary>
        /// Gets or sets the number of items to include in the result set.
        /// This is processed after <see cref="Offset"/> is processed.
        /// </summary>
        /// <value>An optional <see cref="int"/> that specifies the number of results to include.</value>
        public int? Limit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only the number of matching
        /// items should be returned.
        /// </summary>
        /// <value><c>true</c> if this query only wants the count; otherwise, <c>false</c>.</value>
        public bool IsCountOnly { get; set; }
    }
}
