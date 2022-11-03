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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Nest;
using Rock.Net;

namespace Rock.Personalization
{
    /// <summary>
    /// Class EnvironmentRequestFilter.
    /// Implements the <see cref="Rock.Personalization.PersonalizationRequestFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.PersonalizationRequestFilter" />
    public class EnvironmentRequestFilter : PersonalizationRequestFilter
    {
        #region Configuration

        /// <summary>
        /// Gets or sets the days of week.
        /// </summary>
        /// <value>The days of week.</value>
        public DayOfWeek[] DaysOfWeek { get; set; } = new DayOfWeek[0];

        /// <summary>
        /// Gets or sets the beginning time of the day.
        /// </summary>
        /// <value>The beginning time of the day..</value>
        public TimeSpan? BeginningTimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the ending time of the day.
        /// </summary>
        /// <value>The ending time of the day..</value>
        public TimeSpan? EndingTimeOfDay { get; set; }

        #endregion Configuration

        /// <inheritdoc/>
        public override bool IsMatch( HttpRequest httpRequest )
        {
            return IsMatch();
        }

        /// <inheritdoc/>
        internal override bool IsMatch( RockRequestContext request )
        {
            return IsMatch();
        }

        /// <summary>
        /// Determines whether the request meets the criteria of this filter.
        /// </summary>
        /// <returns><c>true</c> if the specified request is a match; otherwise, <c>false</c>.</returns>
        private bool IsMatch()
        {
            var requestDateTime = RockDateTime.Now;
            if ( !DaysOfWeek.Any() || !BeginningTimeOfDay.HasValue )
            {
                // If nothing is selected, return true.
                return true;
            }

            var isMatch = true;
            if ( DaysOfWeek.Any() )
            {
                isMatch = DaysOfWeek.Contains( requestDateTime.DayOfWeek );
            }

            if ( isMatch && BeginningTimeOfDay.HasValue )
            {
                var endingTimeOfDay = EndingTimeOfDay ?? requestDateTime.EndOfDay().TimeOfDay;
                isMatch = requestDateTime.TimeOfDay >= BeginningTimeOfDay.Value && requestDateTime.TimeOfDay <= endingTimeOfDay;
            }

            return isMatch;
        }
    }
}