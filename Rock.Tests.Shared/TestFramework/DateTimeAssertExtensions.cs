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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// Extension methods for the MSTest Assert class.
    /// This class also re-implements the standard Assert methods so that they can be used with the "Assert.This" syntax.
    /// </summary>
    public static partial class AssertExtensions
    {
        /// <summary>
        /// Asserts that the two dates can be considered equivalent because they only vary within a specified tolerance.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        /// <param name="maximumDelta">The maximum delta.</param>
        public static void AreProximate( this Assert assert, DateTime? expectedDate, DateTime? actualDate, TimeSpan maximumDelta )
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
                throw new Exception( string.Format( "\nExpected Date: {0}\nActual Date: {1}\nExpected Delta: {2}\nActual Delta in seconds: {3}",
                                                expectedDate,
                                                actualDate,
                                                maximumDelta,
                                                totalSecondsDifference ) );
            }
        }

        /// <summary>
        /// Asserts that the two DateTime objects can be considered equivalent because they occur on the same date, regardless of the time of day.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        public static void AreEqualDate( this Assert assert, DateTime? expectedDate, DateTime? actualDate, string message = null )
        {
            if ( !AreEqualDate( expectedDate, actualDate, message ) )
            {
                throw new Exception( message ?? string.Format( "\nExpected Date: {0}\nActual Date: {1}\n", expectedDate, actualDate ) );
            }
        }

        /// <summary>
        /// Asserts that the two DateTime objects can be considered equivalent because they occur on the same date, regardless of the time of day.
        /// </summary>
        /// <param name="excludedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        public static void AreNotEqualDate( this Assert assert, DateTime? excludedDate, DateTime? actualDate, string message = null )
        {
            if ( AreEqualDate( excludedDate, actualDate, message ) )
            {
                throw new Exception( message ?? string.Format( "\nExcluded Date matches Actual Date: {0}\n", excludedDate ) );
            }
        }

        /// <summary>
        /// Asserts that the two DateTime objects can be considered equivalent because they occur on the same date, regardless of the time of day.
        /// </summary>
        /// <param name="expectedDate">The expected date.</param>
        /// <param name="actualDate">The actual date.</param>
        private static bool AreEqualDate( DateTime? expectedDate, DateTime? actualDate, string message = null )
        {
            if ( expectedDate == null
                 && actualDate == null )
            {
                return true;
            }
            else if ( expectedDate == null )
            {
                throw new NullReferenceException( message ?? "The expected date was null" );
            }
            else if ( actualDate == null )
            {
                throw new NullReferenceException( message ?? "The actual date was null" );
            }

            return ( expectedDate.Value.Date == actualDate.Value.Date );
        }
    }
}
