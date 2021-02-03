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
    /// This class also re-implements the standard Assert methods so that they can be used with the "Assert.That" syntax.
    /// </summary>
    public static partial class AssertExtensions
    {
        /// <summary>
        /// Asserts that the two numbers can be considered equivalent because they only vary within a specified tolerance.
        /// </summary>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="actualValue">The actual value.</param>
        /// <param name="maximumDelta">The maximum delta.</param>
        public static void AreProximate( this Assert assert, double? expectedValue, double? actualValue, int maximumDelta = 0 )
        {
            if ( expectedValue == null && actualValue == null )
            {
                return;
            }
            else if ( expectedValue == null )
            {
                throw new NullReferenceException( "The expected value was null" );
            }
            else if ( actualValue == null )
            {
                throw new NullReferenceException( "The actual value was null" );
            }

            var difference = Math.Abs( ( ( double ) actualValue - ( double ) expectedValue ) );

            if ( difference > maximumDelta )
            {
                throw new Exception( string.Format( "\nExpected Value: {0}\nActual Value: {1}\nExpected Delta: {2}\nActual Delta: {3}",
                                                expectedValue,
                                                actualValue,
                                                maximumDelta,
                                                difference ) );
            }
        }
    }
}
