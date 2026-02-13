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

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Represents a paginated result set. This includes the items for the page
    /// as well as additional information about the pagination state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PaginatedResult<T>
    {
        #region Properties

        /// <summary>
        /// The items to be included in the page of results.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// The cursor to be used to retrieve the next page of results. This
        /// will be null if there are no additional pages or if
        /// <see cref="PageNumber"/> should be used instead.
        /// </summary>
        public string NextCursor { get; set; }

        /// <summary>
        /// The page number of the results. This is only valid when using
        /// page-number based pagination and will be null if cursor-based
        /// pagination is being used.
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// The size of the page that was requested.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// The number of items that were actually returned in the page.
        /// </summary>
        public int? ReturnedItemCount { get; set; }

        /// <summary>
        /// Indicates whether there are more items available after the current
        /// page.
        /// </summary>
        public bool? HasMoreItems { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs a new <see cref="PaginatedResult{TResult}"/> that has
        /// the same pagination information as the current instance but with the
        /// specified items.
        /// </summary>
        /// <typeparam name="TResult">The type of item to use in the new result.</typeparam>
        /// <param name="items">The set of items to store in the result.</param>
        /// <returns>A new instance of <see cref="PaginatedResult{T}"/> with <paramref name="items"/>.</returns>
        public PaginatedResult<TResult> WithItems<TResult>( IEnumerable<TResult> items )
        {
            return new PaginatedResult<TResult>
            {
                Items = new List<TResult>( items ),
                NextCursor = NextCursor,
                PageNumber = PageNumber,
                PageSize = PageSize,
                ReturnedItemCount = ReturnedItemCount,
                HasMoreItems = HasMoreItems,
            };
        }

        #endregion
    }
}
