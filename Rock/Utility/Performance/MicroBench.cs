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
using System.Reflection;

using Microsoft.Extensions.Logging;

using Rock.Enums.Core;

namespace Rock.Utility.Performance
{
    /// <summary>
    /// <para>
    /// A simple benchmarking suite to measure performance of a piece of code.
    /// This is designed after the concepts of BenchmarkDotNet on a micro scale.
    /// </para>
    /// <para>
    /// Production code should never be released that uses this class. It is
    /// subject to being changed fairly often and may not be compatible between
    /// releases of Rock. It exists for doing benchmark testing in development
    /// only and is included so that plugins can use the same benchmarking code
    /// during their development process.
    /// </para>
    /// </summary>
    public class MicroBench
    {
        #region Properties

        /// <summary>
        /// A delegate to the method that returns the allocated bytes.
        /// </summary>
        private static readonly Func<long> GetAllocatedBytesForCurrentThreadDelegate = CreateGetAllocatedBytesForCurrentThreadDelegate();

        /// <summary>
        /// Gets or sets the logger to use to provide run-time information.
        /// </summary>
        /// <value>The logger to use.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the repitition mode. This configures how long each
        /// benchmark runs for.
        /// </summary>
        /// <value>The repitition mode.</value>
        public BenchmarkRepititionMode RepititionMode { get; set; } = BenchmarkRepititionMode.Normal;

        #endregion

        #region Methods

        /// <summary>
        /// Executes a benchmark for the specified action.
        /// </summary>
        /// <param name="benchmarkAction">The action to be measured.</param>
        /// <returns>An instance of <see cref="BenchmarkStatistics"/> that represents the results after removing any overhead.</returns>
        public BenchmarkResult Benchmark( Action benchmarkAction )
        {
            Logger?.LogDebug( "Determining the number of operations to run." );
            var operationCount = GetOperationCount( benchmarkAction );
            Logger?.LogDebug( $"Will execute {operationCount:N0} operations to run." );

            Logger?.LogDebug( "Determining overhead of measurement code." );
            var overhead = Run( operationCount, () => { }, GetOverheadRunCount() );
            Logger?.LogDebug( $"Overhead calculated as {overhead.ToString( false )}." );

            // Warmup, this should be long enough to kick the CPU temperature
            // up so that we get consistent test results.
            Logger?.LogDebug( "Warming up the measurement code." );
            Run( operationCount, benchmarkAction, GetWarmupRunCount() );

            Logger?.LogDebug( "Beginning real test run." );
            var statistics = Run( operationCount, benchmarkAction, GetRealRunCount() );
            Logger?.LogDebug( $"Raw test results are {statistics.ToString( false )}." );

            statistics = new BenchmarkStatistics(
                statistics.Duration - overhead.Duration,
                statistics.Allocations - overhead.Allocations,
                statistics.Operations );

            Logger?.LogDebug( $"Normalized results are {statistics.ToString( false )}." );

            return new BenchmarkResult( statistics );
        }

        /// <summary>
        /// Runs the specified number of operations.
        /// </summary>
        /// <param name="operationCount">The number of operations to run.</param>
        /// <param name="action">The action to be measured.</param>
        /// <returns>An instance of <see cref="BenchmarkStatistics"/>.</returns>
        private BenchmarkStatistics Run( int operationCount, Action action )
        {
            var startMemory = GetAllocatedBytes();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for ( int i = 0; i < operationCount; i++ )
            {
                action.Invoke();
            }

            stopwatch.Stop();
            var endMemory = GetAllocatedBytes();

            var duration = stopwatch.Elapsed;
            var allocations = startMemory.HasValue && endMemory.HasValue
                ? endMemory.Value - startMemory.Value
                : ( long? ) null;

            return new BenchmarkStatistics( duration, allocations, operationCount );
        }

        /// <summary>
        /// Runs the specified number of operations for a number of groupings.
        /// </summary>
        /// <param name="operationCount">The number of operations to run.</param>
        /// <param name="action">The action to be measured.</param>
        /// <param name="runCount">The number of times to run each run group.</param>
        /// <returns>An instance of <see cref="BenchmarkStatistics"/>.</returns>
        private BenchmarkStatistics Run( int operationCount, Action action, int runCount )
        {
            var statistics = new BenchmarkStatistics();

            for ( int i = 0; i < runCount; i++ )
            {
                var stat = Run( operationCount, action );
                Logger?.LogDebug( $"Benchmark run [{i + 1}/{runCount}]: {stat}" );

                statistics += stat;
            }

            return statistics / runCount;
        }

        /// <summary>
        /// Gets the number of operations needed to get good test results.
        /// </summary>
        /// <param name="benchmarkAction">The action to be measured.</param>
        /// <returns>The number of operations to execute per run.</returns>
        private int GetOperationCount( Action benchmarkAction )
        {
            BenchmarkStatistics statistics;

            // Execute the action once to to an initial warmup so we get the right
            // operation count. This way any cached data is loaded in.
            benchmarkAction();

            for ( var operations = 16; ; operations *= 2 )
            {
                statistics = Run( operations, benchmarkAction );

                if ( statistics.Duration.TotalSeconds >= 0.75 )
                {
                    return operations;
                }
            }
        }

        /// <summary>
        /// Gets the number of bytes allocated on the current thread since it
        /// started.
        /// </summary>
        /// <returns>The number of bytes or <c>null</c> if it can't be determined.</returns>
        private static long? GetAllocatedBytes()
        {
            if ( GetAllocatedBytesForCurrentThreadDelegate == null )
            {
                return null;
            }

            // This must be executed first to get an accurate count.
            GC.Collect();

            return GetAllocatedBytesForCurrentThreadDelegate();
        }

        /// <summary>
        /// Creates a delegate method to the GetAllocatedBytesForCurrentThread()
        /// method, which isn't always public.
        /// </summary>
        /// <returns>A function that returns the number of bytes or null if the delegate couldn't be created.</returns>
        private static Func<long> CreateGetAllocatedBytesForCurrentThreadDelegate()
        {
            try
            {
                var method = typeof( GC )
                    .GetTypeInfo()
                    .GetMethod( "GetAllocatedBytesForCurrentThread", BindingFlags.Public | BindingFlags.Static );

                if ( method == null )
                {
                    return null;
                }

                var del = ( Func<long> ) method.CreateDelegate( typeof( Func<long> ) );

                // Call the delegate once, so that if it throws an exception
                // we can return null.
                del();

                return del;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the overhead run count based on our settings.
        /// </summary>
        /// <returns>The number of runs.</returns>
        private int GetOverheadRunCount()
        {
            if ( RepititionMode == BenchmarkRepititionMode.Fast )
            {
                return 5;
            }
            else if ( RepititionMode == BenchmarkRepititionMode.Extended )
            {
                return 20;
            }
            else
            {
                return 10;
            }
        }

        /// <summary>
        /// Gets the warmup run count based on our settings.
        /// </summary>
        /// <returns>The number of runs.</returns>
        private int GetWarmupRunCount()
        {
            if ( RepititionMode == BenchmarkRepititionMode.Fast )
            {
                return 5;
            }
            else if ( RepititionMode == BenchmarkRepititionMode.Extended )
            {
                return 20;
            }
            else
            {
                return 10;
            }
        }

        /// <summary>
        /// Gets the real run count based on our settings.
        /// </summary>
        /// <returns>The number of runs.</returns>
        private int GetRealRunCount()
        {
            if ( RepititionMode == BenchmarkRepititionMode.Fast )
            {
                return 10;
            }
            else if ( RepititionMode == BenchmarkRepititionMode.Extended )
            {
                return 50;
            }
            else
            {
                return 25;
            }
        }

        #endregion
    }
}
