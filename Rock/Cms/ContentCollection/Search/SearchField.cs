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

namespace Rock.Cms.ContentCollection.Search
{
    /// <summary>
    /// A single field that will be matched in the query.
    /// </summary>
    internal class SearchField : ISearchItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        /// <value>
        /// The field name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the boost to be applied to the match.
        /// </summary>
        /// <value>
        /// The boost to be applied to the match.
        /// </value>
        public double Boost { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether this item is a phrase search.
        /// A phrase will not be broken up into individual words when searching.
        /// </summary>
        /// <value><c>true</c> if this instance is a phrase; otherwise, <c>false</c>.</value>
        public bool IsPhrase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is wildcard search.
        /// A wildcard search will match any word (or phrase) that begins with
        /// the search value.
        /// </summary>
        /// <value><c>true</c> if this instance is wildcard; otherwise, <c>false</c>.</value>
        public bool IsWildcard { get; set; } = true;

        #endregion

        #region ISearchItem

        /// <inheritdoc/>
        ISearchItem ISearchItem.Clone()
        {
            return new SearchField
            {
                Name = Name,
                Value = Value,
                Boost = Boost,
                IsPhrase = IsPhrase,
                IsWildcard = IsWildcard
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string ToString()
        {
            if ( Boost == 1 )
            {
                return $"{Name}=~'{Value}'";
            }
            else
            {
                return $"{Name}=~'{Value}'^{Boost}";
            }
        }

        #endregion
    }
}
