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

using System.Collections.Generic;

namespace Rock.Utility
{
    /// <summary>
    /// Time Interval represents the unit and value of time.
    /// </summary>
    public class TimeInterval
    {
        private readonly Dictionary<TimeIntervalUnit, int> _secondsMultiplier = new Dictionary<TimeIntervalUnit, int>
        {
            { TimeIntervalUnit.Seconds, 1 },
            { TimeIntervalUnit.Minutes, 60 },
            { TimeIntervalUnit.Hours, 60*60 },
            { TimeIntervalUnit.Days, 60*60*24 },
            { TimeIntervalUnit.Months, 60*60*24*30 },
            { TimeIntervalUnit.Years, 60*60*24*365 },
        };

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int? Value { get; set; }

        /// <summary>
        /// Gets or sets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public TimeIntervalUnit Unit { get; set; }

        /// <summary>
        /// Gets the interval in seconds.
        /// </summary>
        /// <returns></returns>
        public int ToSeconds()
        {
            if ( Value == null )
            {
                return 0;
            }

            return Value.Value * _secondsMultiplier[Unit];
        }
    }
}
