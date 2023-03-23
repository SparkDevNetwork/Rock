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

namespace Rock.Tests.Integration.TestData
{
    public static class RandomizedDataHelper
    {
        private static Random _rng = new Random();

        /// <summary>
        /// Returns a random DateTime within a specified time window of a base date.
        /// </summary>
        /// <param name="baseDateTime"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DateTime GetRandomTimeWithinDayWindow( DateTime? baseDateTime, int days )
        {
            if ( baseDateTime == null )
            {
                baseDateTime = RockDateTime.Now;
            }

            var minutesToAdd = _rng.Next( 1, ( Math.Abs( days ) * 1440 ) + 1 );

            if ( days < 0 )
            {
                minutesToAdd *= -1;
            }

            var newDateTime = baseDateTime.Value.AddMinutes( minutesToAdd );

            return newDateTime;
        }

        /// <summary>
        /// Get a random integer within the specified inclusive range.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int GetRandomNumber( int minValue, int maxValue )
        {
            // Add 1 to the upper limit so that it is also an inclusive boundary.
            return _rng.Next( minValue, maxValue + 1 );
        }

        /// <summary>
        /// Get a collection of random dates, distributed evenly throughout the specified date range.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="numberOfDates"></param>
        /// <returns></returns>
        public static List<DateTime> GetRandomDatesInPeriod( DateTime startDate, DateTime endDate, int numberOfDates )
        {
            var dates = new List<DateTime>();

            if ( numberOfDates == 0 )
            {
                return dates;
            }

            // Distribute the changes evenly throughout a random period of days.
            var changePeriodInDays = endDate.Subtract( startDate ).TotalDays;

            decimal dayIncrement = decimal.Divide( ( decimal ) changePeriodInDays, numberOfDates );

            decimal dayOffset = 0;

            DateTime dateOfChange;

            for ( int i = 1; i <= numberOfDates; i++ )
            {
                dateOfChange = startDate.AddDays( ( int ) dayOffset );

                dates.Add( dateOfChange );

                dayOffset = dayOffset + dayIncrement;
            }

            return dates;
        }

        public static string GetRandomMacAddress()
        {
            var random = new Random();
            var buffer = new byte[6];

            _rng.NextBytes( buffer );

            var result = string.Concat( buffer.Select( x => string.Format( "{0}:", x.ToString( "X2" ) ) ).ToArray() );

            return result.TrimEnd( ':' );
        }
    }
}
