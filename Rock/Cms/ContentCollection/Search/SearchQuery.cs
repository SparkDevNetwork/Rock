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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rock.Cms.ContentCollection.Search
{
    /// <summary>
    /// A container for one or more query components.
    /// Implements the <see cref="System.Collections.IEnumerable" />
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable" />
    internal class SearchQuery : ISearchItem, IEnumerable
    {
        private readonly List<ISearchItem> _queryItems = new List<ISearchItem>();

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating if all items must match. When set
        /// this query behaves like an AND query, otherwise as an OR query.
        /// </summary>
        /// <value>
        /// A value indicating if all items must match.
        /// </value>
        public bool IsAllMatching { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified search item to the query.
        /// </summary>
        /// <param name="item">The search item to include.</param>
        public void Add( ISearchItem item )
        {
            _queryItems.Add( item );
        }

        /// <summary>
        /// Clones this query instance.
        /// </summary>
        /// <returns>A new instance with the same characteristics of this search.</returns>
        public SearchQuery Clone()
        {
            return ( SearchQuery ) ( ( ISearchItem ) this ).Clone();
        }

        #endregion

        #region ISearchItem

        /// <inheritdoc/>
        ISearchItem ISearchItem.Clone()
        {
            var clonedQuery = new SearchQuery
            {
                IsAllMatching = IsAllMatching
            };

            foreach ( var item in _queryItems )
            {
                clonedQuery._queryItems.Add( item.Clone() );
            }

            return clonedQuery;
        }

        #endregion

        #region IEnumerable

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) _queryItems ).GetEnumerator();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({_queryItems.Select( i => i.ToString() ).JoinStrings( IsAllMatching ? " & " : " | " )})";
        }

        #endregion
    }
}
