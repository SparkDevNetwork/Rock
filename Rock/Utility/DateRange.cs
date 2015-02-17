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
    /// 
    /// </summary>
    [Serializable]
    public class DateRange
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
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
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="dateFormat">The date format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString( string dateFormat)
        {
            if (Start.HasValue && End.HasValue)
            {
                return string.Format( "{0} to {1}", Start.Value.ToString( dateFormat ), End.Value.ToString( dateFormat ) );
            }

            if (Start.HasValue && !End.HasValue)
            {
                return string.Format( "from {0}", Start.Value.ToString(dateFormat) );
            }

            if (!Start.HasValue && End.HasValue)
            {
                return string.Format( "through {0}", End.Value.ToString( dateFormat ) );
            }

            return string.Empty;
        }
    }
}
