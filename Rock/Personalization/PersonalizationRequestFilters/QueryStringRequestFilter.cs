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
using System.Collections.Specialized;
using System.Web;

using Rock.Model;
using Rock.Net;

namespace Rock.Personalization
{
    /// <summary>
    /// Class QueryStringRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class QueryStringRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>The type of the comparison.</value>
        public ComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        /// <value>The comparison value.</value>
        public string ComparisonValue { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        #endregion Configuration

        /// <inheritdoc/>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            return IsMatch( httpRequest?.QueryString );
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            return IsMatch( request?.QueryString );
        }

        /// <summary>
        /// Determines whether the specified query string meets the criteria of this filter.
        /// </summary>
        /// <param name="queryString">The query string parameters.</param>
        /// <returns><c>true</c> if the specified query string is a match; otherwise, <c>false</c>.</returns>
        private bool IsMatch( NameValueCollection queryString )
        {
            if ( queryString == null )
            {
                return false;
            }

            var queryStringValue = queryString[Key] ?? string.Empty;
            var comparisonValue = ComparisonValue ?? string.Empty;

            return queryStringValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
