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

namespace Rock.Model
{
    /// <summary>
    /// Web Farm Node Service
    /// </summary>
    public partial class WebFarmNodeService
    {
        /// <summary>
        /// Gets the human readable time ago.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static string GetHumanReadablePastTimeDifference( DateTime dateTime )
        {
            var now = RockDateTime.Now;
            var twoMinutesAgo = now.AddMinutes( -2 );
            var twoHoursAgo = now.AddHours( -2 );
            var twoDaysAgo = now.AddDays( -2 );

            if ( dateTime >= twoMinutesAgo )
            {
                var secondsAgo = ( int ) ( now - dateTime ).TotalSeconds;
                return string.Format( "Last Seen {0}s Ago", secondsAgo );
            }
            else if ( dateTime >= twoHoursAgo )
            {
                var minutesAgo = ( int ) ( now - dateTime ).TotalMinutes;
                return string.Format( "Last Seen {0}m Ago", minutesAgo );
            }
            else if ( dateTime >= twoDaysAgo )
            {
                var hoursAgo = ( int ) ( now - dateTime ).TotalHours;
                return string.Format( "Last Seen {0}hr Ago", hoursAgo );
            }

            var daysAgo = ( int ) ( now - dateTime ).TotalDays;
            return string.Format( "Last Seen {0}d Ago", daysAgo );
        }

        #region ViewModels

        /// <summary>
        /// Node View Model
        /// </summary>
        public sealed class NodeViewModel
        {
            /// <summary>
            /// Gets or sets the polling interval seconds.
            /// </summary>
            /// <value>
            /// The polling interval seconds.
            /// </value>
            public decimal PollingIntervalSeconds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is job runner.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is job runner; otherwise, <c>false</c>.
            /// </value>
            public bool IsJobRunner { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is unresponsive.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is unresponsive; otherwise, <c>false</c>.
            /// </value>
            public bool IsUnresponsive { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is leader.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
            /// </value>
            public bool IsLeader { get; set; }

            /// <summary>
            /// Gets or sets the last seen.
            /// </summary>
            /// <value>
            /// The last seen.
            /// </value>
            public DateTime LastSeen { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the node.
            /// </summary>
            /// <value>
            /// The name of the node.
            /// </value>
            public string NodeName { get; set; }

            /// <summary>
            /// Gets or sets the metrics.
            /// </summary>
            /// <value>
            /// The metrics.
            /// </value>
            public IEnumerable<WebFarmNodeMetricService.MetricViewModel> Metrics { get; set; }
        }

        #endregion ViewModels
    }
}
