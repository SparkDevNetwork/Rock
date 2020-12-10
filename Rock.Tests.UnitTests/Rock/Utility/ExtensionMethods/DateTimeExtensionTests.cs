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
    public class DateTimeExtensionsTests
    {
        /// <summary>
        /// Should return the correct date.
        /// </summary>
        [TestMethod]
        public void StartOfMonth_GivesCorrectDate()
        {
            var tests = new[] {
                new { Date = new DateTime(2020, 10, 25, 5, 6, 50 ), Expected = new DateTime( 2020, 10, 1 ) },
                new { Date = new DateTime(2010, 6, 25, 23, 6, 50 ), Expected = new DateTime( 2010, 6, 1 ) },
            };

            foreach ( var test in tests )
            {
                Assert.That.AreEqual( test.Expected, test.Date.StartOfMonth() );
            }
        }
    }
}
