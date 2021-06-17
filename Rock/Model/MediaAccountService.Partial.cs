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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Media;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.MediaAccount"/> entities.
    /// </summary>
    public partial class MediaAccountService
    {
        #region Fields

        /// <summary>
        /// A dictionary that tracks if a <see cref="MediaAccount"/> is currently
        /// running an operation. Only a single operation is allowed per account
        /// so that we don't end up with two imports happening at the same time
        /// since they can be triggered from different places.
        /// </summary>
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _accountOperations = new ConcurrentDictionary<int, SemaphoreSlim>();

        #endregion

        #region Methods

        /// <summary>
        /// Requests that an account synchronize the data in Rock with all of
        /// the media content stored on the provider.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier to be synchronized.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to abort the operation.</param>
        /// <returns>A <see cref="SyncOperationResult"/> object with the result of the operation.</returns>
        public static async Task<SyncOperationResult> SyncMediaInAccountAsync( int mediaAccountId, CancellationToken cancellationToken = default )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaAccount = new MediaAccountService( rockContext ).Get( mediaAccountId );

                if ( mediaAccount == null )
                {
                    return new SyncOperationResult( new[] { "Media Account was not found." } );
                }

                var component = mediaAccount.GetMediaAccountComponent();
                if ( component == null )
                {
                    return new SyncOperationResult( new[] { "Media Account component was not found." } );
                }

                // Set time to just before we start. Better to have a small
                // amount of overlap than miss data.
                var refreshDateTime = RockDateTime.Now;

                var result = await EnqueueOperationAsync( mediaAccountId, () =>
                {
                    return component.SyncMediaAsync( mediaAccount, cancellationToken );
                }, cancellationToken );

                if ( result.IsSuccess )
                {
                    mediaAccount.LastRefreshDateTime = refreshDateTime;
                    rockContext.SaveChanges();
                }

                return result;
            }
        }

        /// <summary>
        /// Requests that an account synchronize all the analytics data in Rock
        /// with the data on the provider.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier to be synchronized.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to abort the operation.</param>
        /// <returns>A <see cref="SyncOperationResult"/> object with the result of the operation.</returns>
        public static async Task<SyncOperationResult> SyncAnalyticsInAccountAsync( int mediaAccountId, CancellationToken cancellationToken = default )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaAccount = new MediaAccountService( rockContext ).GetNoTracking( mediaAccountId );

                if ( mediaAccount == null )
                {
                    return new SyncOperationResult( new[] { "Media Account was not found." } );
                }

                var component = mediaAccount.GetMediaAccountComponent();
                if ( component == null )
                {
                    return new SyncOperationResult( new[] { "Media Account component was not found." } );
                }

                var result = await EnqueueOperationAsync( mediaAccountId, () =>
                {
                    return component.SyncAnalyticsAsync( mediaAccount, cancellationToken );
                }, cancellationToken );

                return result;
            }
        }

        /// <summary>
        /// Requests that an account import into Rock any newly added folders
        /// or media items on the provider. This will only add new media
        /// items, it will not delete or update existing items.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier to be refreshed.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to abort the operation.</param>
        /// <returns>A <see cref="SyncOperationResult"/> object with the result of the operation.</returns>
        public static async Task<SyncOperationResult> RefreshMediaInAccountAsync( int mediaAccountId, CancellationToken cancellationToken = default )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaAccount = new MediaAccountService( rockContext ).Get( mediaAccountId );

                if ( mediaAccount == null )
                {
                    return new SyncOperationResult( new[] { "Media Account was not found." } );
                }

                var component = mediaAccount.GetMediaAccountComponent();
                if ( component == null )
                {
                    return new SyncOperationResult( new[] { "Media Account component was not found." } );
                }

                // Set time to just before we start. Better to have a small
                // amount of overlap than miss data.
                var refreshDateTime = RockDateTime.Now;

                var result = await EnqueueOperationAsync( mediaAccountId, () =>
                {
                    return component.RefreshAccountAsync( mediaAccount, cancellationToken );
                }, cancellationToken );

                if ( result.IsSuccess )
                {
                    mediaAccount.LastRefreshDateTime = refreshDateTime;
                    rockContext.SaveChanges();
                }

                return result;
            }
        }

        /// <summary>
        /// Enqueues an operation for the account. It would be bad if we ran
        /// a refresh operation while a full-sync was still running so this
        /// ensures that each account only has a single operation running at
        /// a time.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier.</param>
        /// <param name="operationFactory">The factory method that will start the operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="SyncOperationResult"/> object with the result of the operation.</returns>
        /// <remarks>There is no guarantee the order in which queued operations run.</remarks>
        private static Task<SyncOperationResult> EnqueueOperationAsync( int mediaAccountId, Func<Task<SyncOperationResult>> operationFactory, CancellationToken cancellationToken )
        {
            var semaphore = _accountOperations.GetOrAdd( mediaAccountId, id => new SemaphoreSlim( 1 ) );

            // Create a new task to perform this operation as a courtesy to
            // the calling methods so they don't have to do it themselves,
            // otherwise we would essentially be a synchronous operation.
            return Task.Run( async () =>
            {
                // Wait until we obtain a lock on the semaphore, which means
                // no other task is executing for this account.
                await semaphore.WaitAsync( cancellationToken );

                try
                {
                    return await operationFactory();
                }
                finally
                {
                    // Since we await the result, this gets executed after the
                    // task has completed.
                    semaphore.Release();
                }
            } );
        }

        #endregion
    }
}
