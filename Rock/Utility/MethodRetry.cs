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
using System.Threading;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// This class handles the running of a method x number of times with an exponential back off.
    /// </summary>
    public class MethodRetry
    {
        /// <summary>
        /// Gets or sets the delta back off.
        /// </summary>
        /// <value>
        /// The delta back off.
        /// </value>
        private TimeSpan DeltaBackOff { get; set; }

        /// <summary>
        /// Gets or sets the minimum back off.
        /// </summary>
        /// <value>
        /// The minimum back off.
        /// </value>
        private TimeSpan MinimumBackOff { get; set; }

        /// <summary>
        /// Gets or sets the maximum back off.
        /// </summary>
        /// <value>
        /// The maximum back off.
        /// </value>
        private TimeSpan MaximumBackOff { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tries.
        /// </summary>
        /// <value>
        /// The maximum number of tries.
        /// </value>
        private int MaximumNumberOfTries { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodRetry"/> class.
        /// Calling this constructor will give you a maximum of 10 retries with a maximum wait of 3 seconds between retries.
        /// The longest time this should run is 30 seconds.
        /// </summary>
        public MethodRetry() : this( 100, 500, 3000, 10 )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodRetry"/> class.
        /// </summary>
        /// <param name="deltaBackOffMilliseconds">The delta back off milliseconds.</param>
        /// <param name="minimumBackOffMilliseconds">The minimum back off milliseconds.</param>
        /// <param name="maximumBackOffMilliseconds">The maximum back off milliseconds.</param>
        /// <param name="maximumNumberOfTries">The maximum number of tries.</param>
        /// <exception cref="System.ArgumentException">
        /// deltaBackOffMilliseconds
        /// or
        /// minimumBackOffMilliseconds
        /// or
        /// maximumBackOffMilliseconds
        /// or
        /// maximumNumberOfTries
        /// </exception>
        public MethodRetry( int deltaBackOffMilliseconds, int minimumBackOffMilliseconds, int maximumBackOffMilliseconds, int maximumNumberOfTries )
        {
            if ( deltaBackOffMilliseconds <= 0 )
            {
                throw new ArgumentException( $"{nameof( deltaBackOffMilliseconds )} must be greater then zero.", nameof( deltaBackOffMilliseconds ) );
            }

            if ( minimumBackOffMilliseconds <= 0 )
            {
                throw new ArgumentException( $"{nameof( minimumBackOffMilliseconds )} must be greater then zero.", nameof( minimumBackOffMilliseconds ) );
            }

            if ( maximumBackOffMilliseconds <= 0 )
            {
                throw new ArgumentException( $"{nameof( maximumBackOffMilliseconds )} must be greater then zero.", nameof( maximumBackOffMilliseconds ) );
            }

            if ( maximumNumberOfTries <= 0 )
            {
                throw new ArgumentException( $"{nameof( maximumNumberOfTries )} must be greater then zero.", nameof( maximumNumberOfTries ) );
            }

            DeltaBackOff = TimeSpan.FromMilliseconds( deltaBackOffMilliseconds );
            MinimumBackOff = TimeSpan.FromMilliseconds( minimumBackOffMilliseconds );
            MaximumBackOff = TimeSpan.FromMilliseconds( maximumBackOffMilliseconds );
            MaximumNumberOfTries = maximumNumberOfTries;
        }

        /// <summary>
        /// Executes the specified method to retry.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodToRetry">The method to retry.</param>
        /// <param name="isValid">Boolean condition that the return of methodToRetry must meet for it to be considered good.</param>
        /// <returns></returns>
        public T Execute<T>( Func<T> methodToRetry, Func<T, bool> isValid )
        {
            var numberOfAttempts = 0;

            while ( numberOfAttempts < MaximumNumberOfTries )
            {
                var response = methodToRetry();
                if ( isValid( response ) || numberOfAttempts >= MaximumNumberOfTries )
                {
                    return response;
                }

                numberOfAttempts++;
                var waitFor = this.GetNextWaitInterval( numberOfAttempts );
                Thread.Sleep( waitFor );
            }

            return default;
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodToRetry">The method to retry.</param>
        /// <param name="isValid">The is valid.</param>
        /// <returns></returns>
        public async Task<T> ExecuteAsync<T>( Func<Task<T>> methodToRetry, Func<T, bool> isValid )
        {
            var numberOfAttempts = 0;

            while ( numberOfAttempts < MaximumNumberOfTries )
            {
                var response = await methodToRetry().ConfigureAwait( false );
                numberOfAttempts++;

                if ( isValid( response ) || numberOfAttempts >= MaximumNumberOfTries )
                {
                    return response;
                }

                var waitFor = this.GetNextWaitInterval( numberOfAttempts );
                await Task.Delay( waitFor ).ConfigureAwait( false );
            }

            return default;
        }

        private TimeSpan GetNextWaitInterval( int numberOfAttempts )
        {
            var random = new Random();

            var delta = ( int ) ( ( Math.Pow( 2.0, numberOfAttempts ) - 1.0 ) *
                               random.Next(
                                   ( int ) ( DeltaBackOff.TotalMilliseconds * 0.8 ),
                                   ( int ) ( DeltaBackOff.TotalMilliseconds * 1.2 ) ) );

            var interval = ( int ) Math.Min( MinimumBackOff.TotalMilliseconds + delta, MaximumBackOff.TotalMilliseconds );

            return TimeSpan.FromMilliseconds( interval );
        }
    }
}
