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
using Rock.Enums.Controls;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Class SlidingDateRangeBag.
    /// </summary>
    public class SlidingDateRangeBag
    {
        /// <summary>
        /// Gets or sets the range type
        /// </summary>
        /// <value>
        /// The SlidingDateRangePicker.SlidingDateRangeType used to calculate the date range
        /// </value>
        public SlidingDateRangeType RangeType { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        /// <value>
        /// The SlidingDateRangePicker.TimeUnitType used to calculate the date range
        /// </value>
        public TimeUnitType? TimeUnit { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        /// <value>
        /// The amount of a certain time value used to calculate the date range
        /// </value>
        public int? TimeValue { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        /// <value>
        /// The earliest date in the date range
        /// </value>
        public DateTimeOffset? LowerDate { get; set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        /// <value>
        /// The latest date in the date range
        /// </value>
        public DateTimeOffset? UpperDate { get; set; }
    }
}
