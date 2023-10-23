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

namespace Rock
{
    /// <summary>
    /// DateTime and TimeStamp Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        private const int _monthMultiple = 100;
        private const int _yearMultiple = 10000;

        #region DateKey Extensions

        /// <summary>
        /// Gets the date value from the datekey. Ex: 20201025 =&gt; Oct 25, 2020
        /// </summary>
        /// <param name="dateKey">The date key.</param>
        /// <returns></returns>
        public static DateTime GetDateKeyDate( this int dateKey )
        {
            var year = dateKey.GetDateKeyYear();
            var month = dateKey.GetDateKeyMonth();
            var day = dateKey.GetDateKeyDay();
            return new DateTime( year, month, day );
        }

        /// <summary>
        /// Gets the year value from the datekey. Ex: 20201025 => 2020
        /// </summary>
        /// <param name="dateKey">The date key.</param>
        /// <returns></returns>
        public static int GetDateKeyYear( this int dateKey )
        {
            return dateKey / _yearMultiple;
        }

        /// <summary>
        /// Gets the month value from the datekey. Ex: 20201025 => 10
        /// </summary>
        /// <param name="dateKey">The date key.</param>
        /// <returns></returns>
        public static int GetDateKeyMonth( this int dateKey )
        {
            return ( dateKey % _yearMultiple ) / _monthMultiple;
        }

        /// <summary>
        /// Gets the day value from the datekey. Ex: 20201025 => 25
        /// </summary>
        /// <param name="dateKey">The date key.</param>
        /// <returns></returns>
        public static int GetDateKeyDay( this int dateKey )
        {
            return dateKey % _monthMultiple;
        }

        /// <summary>
        /// Converts to datekey. Ex: 2020-10-25 becomes 20201025.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static int ToDateKey( this DateTime dateTime )
        {
            var year = dateTime.Year * _yearMultiple;
            var month = dateTime.Month * _monthMultiple;
            var day = dateTime.Day;
            return year + month + day;
        }

        /// <summary>
        /// Converts to datekey. Ex: 2020-10-25 becomes 20201025.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static int? ToDateKey( this DateTime? dateTime )
        {
            if ( dateTime.HasValue )
            {
                return dateTime.Value.ToDateKey();
            }

            return null;
        }

        #endregion DateKey Extensions
    }
}
