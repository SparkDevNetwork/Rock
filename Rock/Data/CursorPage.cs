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

using System;
using System.Collections.Generic;

namespace Rock.Data
{
    /// <summary>
    /// Represents a single page of results in a cursor-based pagination
    /// sequence.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the page.</typeparam>
    internal class CursorPage<T>
    {
        #region Properties

        /// <summary>
        /// Gets the collection of items contained in the list.
        /// </summary>
        public IList<T> Items { get; }

        /// <summary>
        /// Gets the cursor that can be used to retrieve the next page of
        /// results in a paginated query.
        /// </summary>
        public string NextCursor { get; }

        /// <summary>
        /// Gets a value indicating whether there are additional items
        /// available to retrieve.
        /// </summary>
        public bool HasMore => NextCursor != null;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new, empty, instance of the <see cref="CursorPage{T}"/> class.
        /// </summary>
        internal CursorPage()
        {
            Items = Array.Empty<T>();
        }

        /// <summary>
        /// Initializes a new, instance of the <see cref="CursorPage{T}"/> class.
        /// </summary>
        /// <param name="items">The collection of items contained in the list.</param>
        /// <param name="nextCursor">The cursor that can be used to retrieve the next page of results in a paginated query.</param>
        internal CursorPage( IList<T> items, string nextCursor )
        {
            Items = items;
            NextCursor = nextCursor;
        }

        #endregion
    }
}
