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
using System.Collections.Generic;
using System.Linq;

using Rock.Model;

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// Contains the results from the GetValidOccurrences() method.
    /// </summary>
    internal class ValidOccurrencesResult
    {
        #region Properties

        /// <summary>
        /// Gets the occurrences that are valid for the individual to join.
        /// </summary>
        /// <value>
        /// The occurrences that are valid for the individual to join.
        /// </value>
        public IEnumerable<InteractiveExperienceOccurrence> Occurrences { get; }

        /// <summary>
        /// Gets a value that indicates if logging in might produce more results.
        /// </summary>
        /// <value>
        /// <c>true</c> if logging in might produce more results; otherwise <c>false</c>.
        /// </value>
        public bool LoginRecommended { get; }

        /// <summary>
        /// Gets a value that indicates if providing geo-location information
        /// might produce more results.
        /// </summary>
        /// <value>
        /// <c>true</c> if providing geo-locaiton information might produce more results; otherwise <c>false</c>.
        /// </value>
        public bool GeoLocationRecommended { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ValidOccurrencesResult"/> class
        /// that represents an empty result set.
        /// </summary>
        public ValidOccurrencesResult()
        {
            Occurrences = Array.Empty<InteractiveExperienceOccurrence>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ValidOccurrencesResult"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="occurrences">The occurrences that were valid for the individual.</param>
        /// <param name="loginRecommended"><c>true</c> if it is recommended to provide a Person object.</param>
        /// <param name="geoLocationRecommended"><c>true</c> if it is recommended to provide geo-location.</param>
        public ValidOccurrencesResult( IEnumerable<InteractiveExperienceOccurrence> occurrences, bool loginRecommended, bool geoLocationRecommended )
        {
            Occurrences = occurrences.ToList();
            LoginRecommended = loginRecommended;
            GeoLocationRecommended = geoLocationRecommended;
        }

        #endregion
    }
}
