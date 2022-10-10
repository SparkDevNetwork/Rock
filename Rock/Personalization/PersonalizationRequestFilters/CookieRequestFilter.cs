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
using System.Linq;
using System.Web;

using Rock.Model;
using Rock.Net;

namespace Rock.Personalization
{
    /// <summary>
    /// Class CookieRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class CookieRequestFilter : PersonalizationRequestFilter
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
            // Note that httpRequest.Cookies.Get will create the cookie if it doesn't exist, so make sure to check if it exists first.
            var cookieExists = httpRequest.Cookies.AllKeys.Contains( this.Key );
            string cookieValue;
            if ( cookieExists )
            {
                cookieValue = httpRequest.Cookies.Get( this.Key )?.Value ?? string.Empty;
            }
            else
            {
                cookieValue = string.Empty;
            } 

            var comparisonValue = ComparisonValue ?? string.Empty;

            return cookieValue.CompareTo( comparisonValue, ComparisonType );
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            var cookieValue = request.GetCookieValue( this.Key ) ?? string.Empty;
            var comparisonValue = ComparisonValue ?? string.Empty;

            return cookieValue.CompareTo( comparisonValue, ComparisonType );
        }
    }
}
