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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Performance.Core
{
    [TestClass]
    public class SundayDateTest
    {
        [TestMethod]
        public void TestPerformance()
        {
            // warm up
            var warmup = RockDateTime.FirstDayOfWeek;
            int testCount = 1000000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for ( int i = 0; i < testCount; i++ )
            {
                var firstDayOfWeek = RockDateTime.FirstDayOfWeek;
            }

            stopwatch.Stop();
            Debug.WriteLine( $"{stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalMilliseconds / testCount}ms/count  RockDateTime.FirstDayOfWeek" );

            stopwatch.Restart();
            for ( int i = 0; i < testCount; i++ )
            {
                var firstDayOfWeek = RockDateTime.DefaultFirstDayOfWeek;
            }

            stopwatch.Stop();
            Debug.WriteLine( $"{stopwatch.Elapsed.TotalMilliseconds}ms RockDateTime.DefaultFirstDayOfWeek" );
        }
    }
}
