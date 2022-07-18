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
using System.Web;

using Rock.Utility;

namespace Rock.Personalization
{
    /// <summary>
    /// Class IPAddressRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class IPAddressRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the type of the match.
        /// </summary>
        /// <value>The type of the match.</value>
        public RangeType MatchType { get; set; }

        /// <summary>
        /// Gets or sets the beginning ip address.
        /// </summary>
        /// <value>The beginning ip address.</value>
        public string BeginningIPAddress { get; set; }
        /// <summary>
        /// Gets or sets the ending ip address.
        /// </summary>
        /// <value>The ending ip address.</value>
        public string EndingIPAddress { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        #endregion Configuration

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is match; otherwise, <c>false</c>.</returns>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            var isInRange = WebRequestHelper.IsIPAddressInRange( new HttpRequestWrapper( httpRequest ), BeginningIPAddress, EndingIPAddress );
            if ( MatchType == RangeType.InRange )
            {
                return isInRange;
            }
            else
            {
                return !isInRange;
            }
        }

        /// <summary>
        /// Enum RangeType
        /// </summary>
        public enum RangeType
        {
            /// <summary>
            /// The in range
            /// </summary>
            InRange = 0,
            /// <summary>
            /// The not in range
            /// </summary>
            NotInRange = 1
        }
    }
}
