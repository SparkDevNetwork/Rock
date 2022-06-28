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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Enums.Controls
{

    /// <summary>
    /// Also defined in Rock.Web.UI.Controls.SlidingDateRangePicker, so if changed, please update there as well
    /// </summary>
    [Flags]
    public enum SlidingDateRangeType
    {
        /// <summary>
        /// All
        /// </summary>
        All = -1,

        /// <summary>
        /// The last X days,weeks,months, etc (inclusive of current day,week,month,...) but cuts off so it doesn't include future dates
        /// </summary>
        Last = 0,

        /// <summary>
        /// The current day,week,month,year
        /// </summary>
        Current = 1,

        /// <summary>
        /// The date range
        /// </summary>
        DateRange = 2,

        /// <summary>
        /// The previous X days,weeks,months, etc (excludes current day,week,month,...)
        /// </summary>
        Previous = 4,

        /// <summary>
        /// The next X days,weeks,months, etc (inclusive of current day,week,month,...), but cuts off so it doesn't include past dates
        /// </summary>
        Next = 8,

        /// <summary>
        /// The upcoming X days,weeks,months, etc (excludes current day,week,month,...)
        /// </summary>
        Upcoming = 16
    }

    /// <summary>
    /// Also defined in Rock.Web.UI.Controls.SlidingDateRangePicker, so if changed, please update there as well
    /// </summary>
    public enum TimeUnitType
    {
        /// <summary>
        /// The hour
        /// </summary>
        Hour = 0,

        /// <summary>
        /// The day
        /// </summary>
        Day = 1,

        /// <summary>
        /// The week
        /// </summary>
        Week = 2,

        /// <summary>
        /// The month
        /// </summary>
        Month = 3,

        /// <summary>
        /// The year
        /// </summary>
        Year = 4
    }
}
