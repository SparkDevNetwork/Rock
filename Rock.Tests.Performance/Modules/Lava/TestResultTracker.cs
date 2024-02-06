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
using System.Collections.Generic;
using System.Linq;

namespace Rock.Tests.Performance.Modules.Lava
{
    public class TestResultTracker
    {
        public List<TestResult> Results = new List<TestResult>();

        public void AddResult( string testKey, int testExecutionNumber, long timeElapsedInTicks )
        {
            Results.Add( new TestResult { TestKey = testKey, ExecutionNumber = testExecutionNumber, TimeElapsedInTicks = timeElapsedInTicks } );
        }

        private List<TestResult> GetFilteredResults(string testKey = null)
        {
            if ( testKey == null )
            {
                return Results;
            }
            else
            {
                return Results.Where( x => x.TestKey == testKey ).ToList();
            }
        }

        public TimeSpan GetTotalTime( string testKey = null )
        {
            var result = GetFilteredResults( testKey ).Sum( x => x.TimeElapsedInTicks );

            return new TimeSpan( result );
        }

        public TimeSpan GetMinTime(string testKey = null)
        {
            var result = GetFilteredResults( testKey ).Min( x => x.TimeElapsedInTicks );

            return new TimeSpan( result );
        }

        public TimeSpan GetMaxTime( string testKey = null )
        {
            var result = GetFilteredResults( testKey ).Max( x => x.TimeElapsedInTicks );

            return new TimeSpan( result );
        }

        public TimeSpan GetMeanTime( string testKey = null )
        {
            var mean = GetTotalTime( testKey ).Ticks / GetFilteredResults( testKey ).Count;

            return new TimeSpan( mean );
        }

        public TimeSpan GetMedianTime( string testKey = null )
        {
            var times = GetFilteredResults( testKey ).Select( x => x.TimeElapsedInTicks );

            return new TimeSpan( GetMedian( times ) );
        }

        private static long GetMedian( IEnumerable<long> numbers )
        {
            if ( numbers == null || !numbers.Any() )
            {
                return 0;
            }

            var sortedNumbers = ( long[] ) numbers.ToArray().Clone();

            Array.Sort( sortedNumbers );

            // Get the median
            var size = sortedNumbers.Length;
            var mid = size / 2;
            var median = ( size % 2 != 0 ) ? sortedNumbers[mid] : ( sortedNumbers[mid] + sortedNumbers[mid - 1] ) / 2;

            return median;
        }
    }

    public class TestResult
    {
        public string TestKey;
        public int ExecutionNumber;
        public long TimeElapsedInTicks = 0;
    }
}