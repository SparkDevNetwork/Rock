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

namespace Rock.Tests.UnitTests.Lava
{
    /// <summary>
    /// Helper class to compare dates for equality within specific tolerances.
    /// </summary>
    public static class DateTimeAssert
    {
        /// <summary>
        /// Asserts that the two dates are equal within a timespan of 500 milliseconds.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate )
        {
            AreEqual( expectedDate, actualDate, TimeSpan.FromMilliseconds( 500 ) );
        }

        /// <summary>
        /// Asserts that the two dates are equal within what ever timespan you deem is sufficient.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <param name="maximumDelta">The maximum delta.</param>
        /// <exception cref="Xunit.Sdk.EqualException">Thrown if the two dates are not equal enough.</exception>
        public static void AreEqual( DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta )
        {
            if ( expectedDate == null && actualDate == null )
            {
                return;
            }
            else if ( expectedDate == null )
            {
                throw new NullReferenceException( "The expected date was null" );
            }
            else if ( actualDate == null )
            {
                throw new NullReferenceException( "The actual date was null" );
            }

            double totalSecondsDifference = Math.Abs( ( ( DateTime ) actualDate - ( DateTime ) expectedDate ).TotalSeconds );

            if ( totalSecondsDifference > maximumDelta.TotalSeconds )
            {
                //throw new Xunit.Sdk.EqualException( string.Format( "{0}", expectedDate ), string.Format( "{0}", actualDate ) );
                throw new Exception( string.Format( "\nExpected Date: {0}\nActual Date: {1}\nExpected Delta: {2}\nActual Delta in seconds: {3}",
                                                expectedDate,
                                                actualDate,
                                                maximumDelta,
                                                totalSecondsDifference ) );
            }
        }
    }
}
