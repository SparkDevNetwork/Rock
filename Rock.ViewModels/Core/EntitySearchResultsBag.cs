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

using System.Collections.Generic;

namespace Rock.ViewModels.Core
{
    /// <summary>
    /// Contains the results of an entity search request.
    /// </summary>
    public class EntitySearchResultsBag
    {
        /// <summary>
        /// Gets or sets the number of items in this result.
        /// </summary>
        /// <value>The number of items in this result.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the set of objects that matched the query.
        /// </summary>
        /// <value>The set of objects that matched the query.</value>
        public List<object> Items { get; set; }
    }
}
