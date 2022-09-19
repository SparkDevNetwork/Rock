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
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// Helper class for processing a number of items across multiple
    /// background tasks.
    /// </summary>
    internal class ParallelProcessor
    {
        #region Properties

        /// <summary>
        /// Gets the maximum number of items that will be processed concurrently.
        /// </summary>
        /// <value>The maximum number of items that will be processed concurrently.</value>
        public int MaxConcurrency { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelProcessor"/> class.
        /// </summary>
        /// <param name="maxConcurrency">The maximum number of items that will be processed concurrently.</param>
        public ParallelProcessor( int maxConcurrency )
        {
            MaxConcurrency = maxConcurrency;
        }

        #endregion

        /// <summary>
        /// Execute as an asynchronous operation for each item in the collection.
        /// </summary>
        /// <remarks>
        /// Any exceptions are logged but will not be propagated back to the caller.
        /// </remarks>
        /// <typeparam name="TItem">The type of item to be processed.</typeparam>
        /// <param name="items">The items that need to be processed.</param>
        /// <param name="action">The action to call for each item to process it.</param>
        /// <returns>A <see cref="Task"/> representing when the operation has completed.</returns>
        public async Task ExecuteAsync<TItem>( IEnumerable<TItem> items, Func<TItem, Task> action )
        {
            using ( var mutex = new System.Threading.SemaphoreSlim( MaxConcurrency ) )
            {
                var tasks = new List<Task>();

                foreach ( var item in items )
                {
                    await mutex.WaitAsync().ConfigureAwait( false );

                    tasks.Add( Task.Run( async () =>
                    {
                        try
                        {
                            await action( item );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );
                        }
                        finally
                        {
                            mutex.Release();
                        }
                    } ) );
                }

                await Task.WhenAll( tasks );
            }
        }

        /// <summary>
        /// Execute as an asynchronous operation for each item in the collection.
        /// </summary>
        /// <remarks>
        /// Any exceptions are logged and will have the default value of <typeparamref name="TItem"/>
        /// inserted into the results. If you need to be able to differentiate
        /// or know the exact exception then include a try/catch in your <paramref name="action"/>.
        /// </remarks>
        /// <typeparam name="TItem">The type of item to be processed.</typeparam>
        /// <typeparam name="TResult">The type of result returned by the processing of each item.</typeparam>
        /// <param name="items">The items that need to be processed.</param>
        /// <param name="action">The action to call for each item to process it.</param>
        /// <returns>A <see cref="Task"/> representing when the operation has completed and containing the results of the actions.</returns>
        public async Task<IEnumerable<TResult>> ExecuteAsync<TItem, TResult>( IEnumerable<TItem> items, Func<TItem, Task<TResult>> action )
        {
            using ( var mutex = new System.Threading.SemaphoreSlim( MaxConcurrency ) )
            {
                var tasks = new List<Task<TResult>>();

                foreach ( var item in items )
                {
                    await mutex.WaitAsync().ConfigureAwait( false );

                    tasks.Add( Task.Run( async () =>
                    {
                        try
                        {
                            return await action( item );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex );

                            return default;
                        }
                        finally
                        {
                            mutex.Release();
                        }
                    } ) );
                }

                return await Task.WhenAll( tasks );
            }
        }
    }
}
