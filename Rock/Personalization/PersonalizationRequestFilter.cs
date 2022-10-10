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
using System.Web;

namespace Rock.Personalization
{
    /// <summary>
    /// Filter that determines if a Browser request meets criteria.
    /// </summary>
    public abstract class PersonalizationRequestFilter
    {
        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns><c>true</c> if the specified HTTP request is a match; otherwise, <c>false</c>.</returns>
        public abstract bool IsMatch( HttpRequest httpRequest );

        /// <summary>
        /// Determines whether the specified HTTP request meets the criteria of this filter.
        /// </summary>
        /// <param name="request">The request to be matched against.</param>
        /// <returns><c>true</c> if the specified request is a match; otherwise, <c>false</c>.</returns>
        internal virtual bool IsMatch( Net.RockRequestContext request )
        {
            return false;
        }
    }
}
