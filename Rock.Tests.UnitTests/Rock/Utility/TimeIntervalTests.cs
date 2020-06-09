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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.UnitTests.Rock.Utility
{
    [TestClass]
    public class TimeIntervalTests
    {
        [TestMethod]
        public void GetIntervalInSecondsShouldReturnCorrectValues()
        {
            var testData = new Dictionary<TimeInterval, int>
            {
                { new TimeInterval{ Unit = TimeIntervalUnit.Seconds, Value = 345 }, 345 },
                { new TimeInterval{ Unit = TimeIntervalUnit.Minutes, Value = 10 }, 600 },
                { new TimeInterval{ Unit = TimeIntervalUnit.Hours, Value = 1 }, 3600 },
                { new TimeInterval{ Unit = TimeIntervalUnit.Days, Value = 2 }, 172800 },
                { new TimeInterval{ Unit = TimeIntervalUnit.Months, Value = 2 }, 5184000 },
                { new TimeInterval{ Unit = TimeIntervalUnit.Years, Value = 1 }, 31536000 },
            };

            foreach ( var keyValue in testData )
            {
                Assert.That.AreEqual( keyValue.Value, keyValue.Key.ToSeconds() );
            }
        }
    }
}
