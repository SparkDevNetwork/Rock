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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Utility.ExtensionMethods
{
    [TestClass]
    public class DateKeyExtensionsTests
    {
        /// <summary>
        /// Should return the correct int.
        /// </summary>
        [TestMethod]
        public void ToDateKey_GivesCorrectInt()
        {
            var tests = new[] {
                new { Date = new DateTime(2020, 10, 25), Expected = 20201025  },
                new { Date = new DateTime(1965, 1, 1), Expected = 19650101  },
            };

            foreach ( var test in tests )
            {
                Assert.That.AreEqual( test.Expected, test.Date.ToDateKey() );
            }

            Assert.That.AreEqual( null, ( ( DateTime? ) null ).ToDateKey() );
        }

        /// <summary>
        /// Should return the correct date part.
        /// </summary>
        [TestMethod]
        public void GetDateKeyYear_GivesCorrectYear()
        {
            var tests = new[] {
                new { DateKey = 0, Expected = 0 },
                new { DateKey = 20201025, Expected = 2020 },
                new { DateKey = 19650101, Expected = 1965  },
            };

            foreach ( var test in tests )
            {
                Assert.That.AreEqual( test.Expected, test.DateKey.GetDateKeyYear() );
            }
        }

        /// <summary>
        /// Should return the correct date part.
        /// </summary>
        [TestMethod]
        public void GetDateKeyMonth_GivesCorrectMonth()
        {
            var tests = new[] {
                new { DateKey = 0, Expected = 0 },
                new { DateKey = 20201025, Expected = 10 },
                new { DateKey = 19650101, Expected = 1  },
            };

            foreach ( var test in tests )
            {
                Assert.That.AreEqual( test.Expected, test.DateKey.GetDateKeyMonth() );
            }
        }

        /// <summary>
        /// Should return the correct date part.
        /// </summary>
        [TestMethod]
        public void GetDateKeyDay_GivesCorrectDay()
        {
            var tests = new[] {
                new { DateKey = 0, Expected = 0 },
                new { DateKey = 20201025, Expected = 25 },
                new { DateKey = 19650102, Expected = 2  },
            };

            foreach ( var test in tests )
            {
                Assert.That.AreEqual( test.Expected, test.DateKey.GetDateKeyDay() );
            }
        }
    }
}
