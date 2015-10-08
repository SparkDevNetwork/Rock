// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    /// DateTime Range class which has functions to equate and to display the date range
    /// </summary>
    [Serializable]
    public class DateRange : IEquatable<DateRange>
    {
        /// <summary>
        /// Gets or sets the start (as a DateTime)
        /// NOTE: You normally want to do a '&gt;= DateRange.Start' when using this in a query
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Gets or sets the end date (as a DateTime)
        /// NOTE: You normally want to do a '&lt; DateRange.End' when using this in a query
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime? End { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> class.
        /// </summary>
        public DateRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public DateRange( DateTime? start, DateTime? end )
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString( "f" );
        }

        /// <summary>
        /// returns true of the Start and End date of each daterange are the same
        /// </summary>
        /// <param name="other">The other date range</param>
        /// <returns></returns>
        public bool Equals( DateRange other )
        {
            if ( other == null )
            {
                return false;
            }
            else
            {
                return ( this.Start == other.Start ) && ( this.End == other.End );
            }
        }

        /// <summary>
        /// Returns a date range with each date formatted with the specified format
        /// NOTE: This is the literal date/time range.  If you want to display a human readable date/time range, use ToStringAutomatic()
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString( string dateFormat )
        {
            if ( Start.HasValue && End.HasValue )
            {
                return string.Format( "{0} to {1}", Start.Value.ToString( dateFormat ), End.Value.ToString( dateFormat ) );
            }

            if ( Start.HasValue && !End.HasValue )
            {
                return string.Format( "from {0}", Start.Value.ToString( dateFormat ) );
            }

            if ( !Start.HasValue && End.HasValue )
            {
                return string.Format( "through {0}", End.Value.ToString( dateFormat ) );
            }

            return string.Empty;
        }

        /// <summary>
        /// Displays the date range that makes sense to a human.
        /// For example, if both dates are at midnight, the end date will be displayed as the last *full* day and the time won't be shown
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <param name="dateTimeFormat">The date time format.</param>
        /// <returns></returns>
        public string ToStringAutomatic( string dateFormat = null, string dateTimeFormat = null )
        {
            dateFormat = dateFormat ?? "d";
            dateTimeFormat = dateTimeFormat ?? "g";
            string autoFormat;
            DateTime? humanReadableEnd = End;
            if ( ( Start.HasValue && Start.Value != Start.Value.Date ) || ( humanReadableEnd.HasValue && humanReadableEnd.Value != humanReadableEnd.Value.Date ) )
            {
                // one of the dates is not midnight, so show as a date time
                autoFormat = dateTimeFormat;
            }
            else
            {
                autoFormat = dateFormat;

                if ( humanReadableEnd.HasValue )
                {
                    // when showing just the Date, the human readable end date is the Last Full Day, not the exact Point-In-Time of Midnite that us computers think of
                    humanReadableEnd = humanReadableEnd.Value.AddDays( -1 );
                }
            }

            if ( Start.HasValue && humanReadableEnd.HasValue )
            {
                if ( Start.Value == humanReadableEnd.Value )
                {
                    return string.Format( Start.Value.ToString( autoFormat ) );
                }
                else
                {
                    return string.Format( "{0} to {1}", Start.Value.ToString( autoFormat ), humanReadableEnd.Value.ToString( autoFormat ) );
                }
            }

            if ( Start.HasValue && !humanReadableEnd.HasValue )
            {
                return string.Format( "from {0}", Start.Value.ToString( autoFormat ) );
            }

            if ( !Start.HasValue && humanReadableEnd.HasValue )
            {
                return string.Format( "through {0}", humanReadableEnd.Value.ToString( autoFormat ) );
            }

            return string.Empty;
        }
    }
}
