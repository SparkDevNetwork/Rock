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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Model;
using Rock.Plugin.HotFixes;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Utility
{
    /**
     * 11/1/2022 - DSH
     * 
     * If any tests are added, modified or removed from here you need to
     * update the Obsidian unit tests as well.
     */
    [TestClass]
    public class RockDateTimeHelperTests
    {
        public static DateTime StandardReferenceDateTime => new DateTime( 2022, 11, 2, 9, 10, 24 );

        public static IEnumerable<object[]> TestData
        {
            get
            {
                // Format is slidingRangeValue, currentDateTime, expectedStart, expectedEnd
                return new[]
                {
                    // Current Hour
                    new object[] { "Current||Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 9, 0, 0 ), new DateTime( 2022, 11, 2, 10, 0, 0 ) },
                    // Current Day
                    new object[] { "Current||Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Current Week
                    new object[] { "Current||Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 31, 0, 0, 0 ), new DateTime( 2022, 11, 6, 23, 59, 59, 999 ) },
                    // Current Month
                    new object[] { "Current||Month||", StandardReferenceDateTime, new DateTime( 2022, 11, 1, 0, 0, 0 ), new DateTime( 2022, 11, 30, 23, 59, 59, 999 ) },
                    // Current Year
                    new object[] { "Current||Year||", StandardReferenceDateTime, new DateTime( 2022, 1, 1, 0, 0, 0 ), new DateTime( 2022, 12, 31, 23, 59, 59, 999 ) },

                    // Previous 1 Hours
                    new object[] { "Previous|1|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 8, 0, 0 ), new DateTime( 2022, 11, 2, 9, 0, 0 ) },
                    // Previous 1 Days
                    new object[] { "Previous|1|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 1, 0, 0, 0 ), new DateTime( 2022, 11, 1, 23, 59, 59, 999 ) },
                    // Previous 1 Weeks
                    new object[] { "Previous|1|Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 24, 0, 0, 0 ), new DateTime( 2022, 10, 30, 23, 59, 59, 999 ) },
                    // Previous 1 Months
                    new object[] { "Previous|1|Month||", StandardReferenceDateTime, new DateTime( 2022, 10, 1, 0, 0, 0 ), new DateTime( 2022, 10, 31, 23, 59, 59, 999 ) },
                    // Previous 1 Years
                    new object[] { "Previous|1|Year||", StandardReferenceDateTime, new DateTime( 2021, 1, 1, 0, 0, 0 ), new DateTime( 2021, 12, 31, 23, 59, 59, 999 ) },

                    // Previous 3 Hours
                    new object[] { "Previous|3|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 6, 0, 0 ), new DateTime( 2022, 11, 2, 9, 0, 0 ) },
                    // Previous 3 Days
                    new object[] { "Previous|3|Day||", StandardReferenceDateTime, new DateTime( 2022, 10, 30, 0, 0, 0 ), new DateTime( 2022, 11, 1, 23, 59, 59, 999 ) },
                    // Previous 3 Weeks
                    new object[] { "Previous|3|Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 10, 0, 0, 0 ), new DateTime( 2022, 10, 30, 23, 59, 59, 999 ) },
                    // Previous 3 Months
                    new object[] { "Previous|3|Month||", StandardReferenceDateTime, new DateTime( 2022, 8, 1, 0, 0, 0 ), new DateTime( 2022, 10, 31, 23, 59, 59, 999 ) },
                    // Previous 3 Years
                    new object[] { "Previous|3|Year||", StandardReferenceDateTime, new DateTime( 2019, 1, 1, 0, 0, 0 ), new DateTime( 2021, 12, 31, 23, 59, 59, 999 ) },

                    // Last 1 Hours
                    new object[] { "Last|1|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 9, 0, 0 ), new DateTime( 2022, 11, 2, 10, 0, 0 ) },
                    // Last 1 Days
                    new object[] { "Last|1|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 1 Weeks
                    new object[] { "Last|1|Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 31, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 1 Months
                    new object[] { "Last|1|Month||", StandardReferenceDateTime, new DateTime( 2022, 11, 1, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 1 Years
                    new object[] { "Last|1|Year||", StandardReferenceDateTime, new DateTime( 2022, 1, 1, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },

                    // Last 3 Hours
                    new object[] { "Last|3|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 7, 0, 0 ), new DateTime( 2022, 11, 2, 10, 0, 0 ) },
                    // Last 3 Days
                    new object[] { "Last|3|Day||", StandardReferenceDateTime, new DateTime( 2022, 10, 31, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 3 Weeks
                    new object[] { "Last|3|Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 17, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 3 Months
                    new object[] { "Last|3|Month||", StandardReferenceDateTime, new DateTime( 2022, 9, 1, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Last 3 Years
                    new object[] { "Last|3|Year||", StandardReferenceDateTime, new DateTime( 2020, 1, 1, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },

                    // Next 1 Hours
                    new object[] { "Next|1|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 9, 0, 0 ), new DateTime( 2022, 11, 2, 10, 0, 0 ) },
                    // Next 1 Days
                    new object[] { "Next|1|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 2, 23, 59, 59, 999 ) },
                    // Next 1 Weeks
                    new object[] { "Next|1|Week||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 6, 23, 59, 59, 999 ) },
                    // Next 1 Months
                    new object[] { "Next|1|Month||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 30, 23, 59, 59, 999 ) },
                    // Next 1 Years
                    new object[] { "Next|1|Year||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 12, 31, 23, 59, 59, 999 ) },

                    // Next 3 Hours
                    new object[] { "Next|3|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 9, 0, 0 ), new DateTime( 2022, 11, 2, 12, 0, 0 ) },
                    // Next 3 Days
                    new object[] { "Next|3|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 4, 23, 59, 59, 999 ) },
                    // Next 3 Weeks
                    new object[] { "Next|3|Week||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2022, 11, 20, 23, 59, 59, 999 ) },
                    // Next 3 Months
                    new object[] { "Next|3|Month||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2023, 1, 31, 23, 59, 59, 999 ) },
                    // Next 3 Years
                    new object[] { "Next|3|Year||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 0, 0, 0 ), new DateTime( 2024, 12, 31, 23, 59, 59, 999 ) },

                    // Upcoming 1 Hours
                    new object[] { "Upcoming|1|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 10, 0, 0 ), new DateTime( 2022, 11, 2, 11, 0, 0 ) },
                    // Upcoming 1 Days
                    new object[] { "Upcoming|1|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 3, 0, 0, 0 ), new DateTime( 2022, 11, 3, 23, 59, 59, 999 ) },
                    // Upcoming 1 Weeks
                    new object[] { "Upcoming|1|Week||", StandardReferenceDateTime, new DateTime( 2022, 11, 7, 0, 0, 0 ), new DateTime( 2022, 11, 13, 23, 59, 59, 999 ) },
                    // Upcoming 1 Months
                    new object[] { "Upcoming|1|Month||", StandardReferenceDateTime, new DateTime( 2022, 12, 1, 0, 0, 0 ), new DateTime( 2022, 12, 31, 23, 59, 59, 999 ) },
                    // Upcoming 1 Years
                    new object[] { "Upcoming|1|Year||", StandardReferenceDateTime, new DateTime( 2023, 1, 1, 0, 0, 0 ), new DateTime( 2023, 12, 31, 23, 59, 59, 999 ) },

                    // Upcoming 3 Hours
                    new object[] { "Upcoming|3|Hour||", StandardReferenceDateTime, new DateTime( 2022, 11, 2, 10, 0, 0 ), new DateTime( 2022, 11, 2, 13, 0, 0 ) },
                    // Upcoming 3 Days
                    new object[] { "Upcoming|3|Day||", StandardReferenceDateTime, new DateTime( 2022, 11, 3, 0, 0, 0 ), new DateTime( 2022, 11, 5, 23, 59, 59, 999 ) },
                    // Upcoming 3 Weeks
                    new object[] { "Upcoming|3|Week||", StandardReferenceDateTime, new DateTime( 2022, 11, 7, 0, 0, 0 ), new DateTime( 2022, 11, 27, 23, 59, 59, 999 ) },
                    // Upcoming 3 Months
                    new object[] { "Upcoming|3|Month||", StandardReferenceDateTime, new DateTime( 2022, 12, 1, 0, 0, 0 ), new DateTime( 2023, 2, 28, 23, 59, 59, 999 ) },
                    // Upcoming 3 Years
                    new object[] { "Upcoming|3|Year||", StandardReferenceDateTime, new DateTime( 2023, 1, 1, 0, 0, 0 ), new DateTime( 2025, 12, 31, 23, 59, 59, 999 ) },

                    // Between
                    new object[] { "DateRange|||2022-11-3 12:00:00 AM|2022-11-22 12:00:00 AM", StandardReferenceDateTime, new DateTime( 2022, 11, 3, 0, 0, 0 ), new DateTime( 2022, 11, 22, 23, 59, 59, 999 ) },

                    // Alternative format: Current Week.
                    new object[] { "Current|1|Week||", StandardReferenceDateTime, new DateTime( 2022, 10, 31, 0, 0, 0 ), new DateTime( 2022, 11, 6, 23, 59, 59, 999 ) },
                    // Alternative format: Current Week.
                    new object[] { "1|1|2||", StandardReferenceDateTime, new DateTime( 2022, 10, 31, 0, 0, 0 ), new DateTime( 2022, 11, 6, 23, 59, 59, 999 ) },
                };
            }
        }

        [TestMethod]
        [DynamicData( nameof( TestData ) )]
        public void CalculateDateRangeProducesCorrectResults( string rangeValue, DateTime currentDateTime, DateTime expectedStart, DateTime expectedEnd )
        {
            var range = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( rangeValue, currentDateTime );

            Assert.That.AreEqual( expectedStart, range.Start );
            Assert.That.AreEqual( expectedEnd, range.End );
        }

        [TestMethod]
        [DataRow( 00, 2000 )]
        [DataRow( 28, 2028 )]
        [DataRow( 30, 2030 )]
        [DataRow( 59, 2059 )]
        [DataRow( 60, 1960 )]
        [DataRow( 1752, 1752 )]
        [DataRow( 10000, 10000 )]
        public void ValidatePossibleCreditCardTwoDigitYears( int givenTwoDigitYear, int? expectedYear )
        {
            var actualYear = RockDateTime.ToFourDigitYearForCreditCardExpiration( givenTwoDigitYear );
            Assert.That.AreEqual( expectedYear, actualYear );
        }

        [TestMethod]
        public void EnsureNegativeTwoDigitYearThrowsArgumentOutOfRangeException()
        {
            Assert.That.ThrowsException<ArgumentOutOfRangeException>( () => RockDateTime.ToFourDigitYearForCreditCardExpiration( -10 ) );
        }

        [TestMethod]
        [DataRow( 28, 2028 )]
        [DataRow( 30, 2030 )]
        [DataRow( 00, 2000 )]
        [DataRow( 59, 2059 )]
        [DataRow( 60, 1960 )]
        [DataRow( 1752, null )]
        [DataRow( 10000, null )]
        public void ValidatePossibleSQLCreditCardExpirationDateSavedValues( int givenDigitYear, int? expectedYear )
        {
            var paymentDetail = new FinancialPaymentDetail();
            var actualYear = paymentDetail.ToFourDigitYear( givenDigitYear );
            Assert.That.AreEqual( expectedYear, actualYear );
        }


    }
}
