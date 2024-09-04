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

namespace Rock.Utility.Performance
{
    /// <summary>
    /// Contains the statistics from a micro benchmark.
    /// </summary>
    public readonly struct BenchmarkStatistics
    {
        #region Properties

        /// <summary>
        /// Gets the duration for all operations.
        /// </summary>
        /// <value>Duration of all operations.</value>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Gets the number of bytes allocated for all operations.
        /// </summary>
        /// <value>Number of bytes or <c>null</c> if not supported.</value>
        public long? Allocations { get; }

        /// <summary>
        /// Gets the number of operations.
        /// </summary>
        /// <value>The number of operations.</value>
        public int Operations { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="BenchmarkStatistics"/>.
        /// </summary>
        /// <param name="duration">The duration for the operations.</param>
        /// <param name="allocations">The number of allocations for the operations.</param>
        /// <param name="operations">The total number of operations.</param>
        internal BenchmarkStatistics( TimeSpan duration, long? allocations, int operations )
        {
            Duration = duration;
            Allocations = allocations;
            Operations = operations;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the duration for a single operation in milliseconds.
        /// </summary>
        /// <returns>A value that represents the duration in milliseconds.</returns>
        public double GetDurationPerOperation()
        {
            return Duration.TotalMilliseconds / Operations;
        }

        /// <summary>
        /// Gets the number of bytes allocated for a single operation.
        /// </summary>
        /// <returns>A number of bytes or <c>null</c> if not supported.</returns>
        public long? GetAllocationsPerOperation()
        {
            if ( !Allocations.HasValue )
            {
                return null;
            }

            return ( long ) Math.Round( ( double ) Allocations.Value / Operations, MidpointRounding.ToEven );
        }

        /// <summary>
        /// Adds two instances of <see cref="BenchmarkStatistics"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new instance that represents the operation.</returns>
        public static BenchmarkStatistics operator +( BenchmarkStatistics left, BenchmarkStatistics right )
        {
            long? allocations = null;

            if ( left.Allocations.HasValue || right.Allocations.HasValue )
            {
                allocations = ( left.Allocations ?? 0 ) + ( right.Allocations ?? 0 );
            }

            return new BenchmarkStatistics(
                left.Duration + right.Duration,
                allocations,
                left.Operations + right.Operations );
        }

        /// <summary>
        /// Subtracts two instances of <see cref="BenchmarkStatistics"/>.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new instance that represents the operation.</returns>
        public static BenchmarkStatistics operator -( BenchmarkStatistics left, BenchmarkStatistics right )
        {
            long? allocations = null;

            if ( left.Allocations.HasValue || right.Allocations.HasValue )
            {
                allocations = ( left.Allocations ?? 0 ) - ( right.Allocations ?? 0 );
            }

            return new BenchmarkStatistics(
                left.Duration - right.Duration,
                allocations,
                left.Operations - right.Operations );
        }

        /// <summary>
        /// Divides an instance of <see cref="BenchmarkStatistics"/> by a number.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>A new instance that represents the operation.</returns>
        public static BenchmarkStatistics operator /( BenchmarkStatistics left, int right )
        {
            long? allocations = null;

            if ( left.Allocations.HasValue )
            {
                allocations = ( left.Allocations ?? 0 ) / right;
            }

            var duration = left.Duration.Ticks < 10_000
                ? TimeSpan.FromTicks( left.Duration.Ticks / right )
                : TimeSpan.FromMilliseconds( left.Duration.TotalMilliseconds / right );

            return new BenchmarkStatistics(
                duration,
                allocations,
                left.Operations / right );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString( true );
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <param name="verbose">Include the number of operations.</param>
        /// <returns>The fully qualified type name.</returns>
        public string ToString( bool verbose )
        {
            var str = verbose ? $"op={Operations:N0}; " : string.Empty;
            var timePerOp = GetDurationPerOperation();

            if ( timePerOp < 0.001 )
            {
                str = $"{str}time={timePerOp * 1_000_000:N0}ns/op";
            }
            else if ( timePerOp < 1 )
            {
                str = $"{str}time={timePerOp * 1_000:N3}us/op";
            }
            else
            {
                str = $"{str}time={timePerOp:N3}ms/op";
            }

            if ( GetAllocationsPerOperation().HasValue )
            {
                str = $"{str}; alloc={GetAllocationsPerOperation():N0}/op";
            }
            else
            {
                str = $"{str}; alloc=null";
            }

            return str;
        }

        #endregion
    }
}