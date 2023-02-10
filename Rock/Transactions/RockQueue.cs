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

using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Queue for <see cref="ITransaction" /> Transactions (not the Bus queue)
    /// </summary>
    /// <remarks>
    /// <para>
    /// There are two queues available on this class. The standard queue and the
    /// fast queue.
    /// </para>
    /// <para>
    /// The standard queue drains whenever <see cref="Drain(Action{Exception})"/>
    /// is called - which usually happens every 60 seconds.
    /// </para>
    /// <para>
    /// The fast queue drains itself automatically every 1 second. Transactions
    /// placed into this queue should complete in less than 1 second if not
    /// even faster. This queue is often used for RealTime notifications so if
    /// a transaction takes 10 seconds to complete then it will hold up future
    /// transactions from being processed until it has finished.
    /// </para>
    /// </remarks>
    static public class RockQueue
    {
        #region Fields

        /// <summary>
        /// The standard transaction queue that drains when requested (which
        /// should happen about once a minute).
        /// </summary>
        private static readonly ConcurrentQueue<ITransaction> _standardTransactionQueue = new ConcurrentQueue<ITransaction>();

        /// <summary>
        /// The fast transaction queue that automatically drains itself
        /// every second.
        /// </summary>
        private static readonly ConcurrentQueue<ITransaction> _fastTransactionQueue = new ConcurrentQueue<ITransaction>();

        /// <summary>
        /// The locking object for <see cref="_fastQueueTokenSource"/>.
        /// </summary>
        private static readonly object _fastQueueThreadLock = new object();

        /// <summary>
        /// The cancellation token source for the currently running fast queue
        /// thread. This is used to shutdown the thread as well as to determine
        /// if the thread is (should be) running.
        /// </summary>
        private static CancellationTokenSource _fastQueueTokenSource;

        #endregion

        #region Methods

        /// <summary>
        /// Starts the fast queue to begin processing transactions in the
        /// queue. Processing will continue until <see cref="ShutdownFastQueue"/>
        /// is called or the application is terminated. If the processing
        /// thread is already running then this method does nothing.
        /// </summary>
        internal static void StartFastQueue()
        {
            lock ( _fastQueueThreadLock )
            {
                // Check if the thread should already be in a running state.
                if ( _fastQueueTokenSource != null )
                {
                    return;
                }

                // Start a new thread and save the object that will be used
                // to shut it down later.
                _fastQueueTokenSource = new CancellationTokenSource();

                var thread = new Thread( ProcessFastQueue )
                {
                    Name = "Rock Fast Queue"
                };

                thread.Start( _fastQueueTokenSource.Token );
            }
        }

        /// <summary>
        /// Shuts down the processing of the fast transaction queue. If the
        /// processing thread is not running then this method does nothing.
        /// </summary>
        internal static void ShutdownFastQueue()
        {
            lock ( _fastQueueThreadLock )
            {
                if ( _fastQueueTokenSource != null )
                {
                    _fastQueueTokenSource.Cancel();
                    _fastQueueTokenSource = null;
                }
            }
        }

        /// <summary>
        /// Gets the transactions that are waiting to be processed in the
        /// standard transaciton queue.
        /// </summary>
        /// <returns>A collection of <see cref="ITransaction"/> objects that are waiting to be processed.</returns>
        public static List<ITransaction> GetQueuedStandardTransactions()
        {
            var transactions = new List<ITransaction>();

#pragma warning disable CS0618 // Type or member is obsolete
            transactions.AddRange( TransactionQueue );
#pragma warning restore CS0618 // Type or member is obsolete

            transactions.AddRange( _standardTransactionQueue );

            return transactions;
        }

        /// <summary>
        /// Adds the <see cref="ITransaction"/> to the standard Rock transaction queue.
        /// </summary>
        /// <param name="transaction">The transaction to be added to the queue.</param>
        public static void Enqueue( ITransaction transaction )
        {
            Enqueue( transaction, false );
        }

        /// <summary>
        /// Adds the <see cref="ITransaction"/> to the Rock transaction queue.
        /// </summary>
        /// <param name="transaction">The transaction to be added to the queue.</param>
        /// <param name="useFastQueue">
        ///     <para>
        ///         <c>true</c> if the fast queue should be used.
        ///     </para>
        ///     <para>
        ///         This queue drains every second so it should only be used with transactions
        ///         that execute extremely fast and need to execute quickly.
        ///     </para>
        /// </param>
        public static void Enqueue( ITransaction transaction, bool useFastQueue )
        {
            if ( transaction is IQueuedTransaction queuedTransaction )
            {
                queuedTransaction.WillEnqueue();
            }

            if ( !useFastQueue )
            {
                _standardTransactionQueue.Enqueue( transaction );
            }
            else
            {
                _fastTransactionQueue.Enqueue( transaction );
            }
        }

        /// <summary>
        /// Drains the standard transaction queue.
        /// </summary>
        /// <param name="errorHandler">The error handler.</param>
        public static void Drain( Action<Exception> errorHandler )
        {
#pragma warning disable CS0618 // Type or member is obsolete
            while ( TransactionQueue.TryDequeue( out var transaction ) )
            {
                CurrentlyExecutingTransaction = transaction;

                if ( CurrentlyExecutingTransaction == null )
                {
                    continue;
                }

                try
                {
                    // This is a bit of a hack just in case somebody puts an
                    // IQueuedTransaction into the legacy queue.
                    if ( CurrentlyExecutingTransaction is IQueuedTransaction queuedTransaction )
                    {
                        queuedTransaction.WillEnqueue();
                    }

                    CurrentlyExecutingTransaction.Execute();
                }
                catch ( Exception ex )
                {
                    errorHandler( new Exception( $"Exception in RockQueue.Drain(): {transaction.GetType().Name}", ex ) );
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete

            while ( _standardTransactionQueue.TryDequeue( out var transaction ) )
            {
                try
                {
                    transaction.Execute();
                }
                catch ( Exception ex )
                {
                    errorHandler( new Exception( $"Exception in RockQueue.Drain(): {transaction.GetType().Name}", ex ) );
                }
            }
        }

        /// <summary>
        /// This is the main entry point for the fast queue processing thread.
        /// </summary>
        /// <param name="parameter">The parameter passed to the thread, which is the <see cref="CancellationToken"/> used to terminate the thread.</param>
        private static void ProcessFastQueue( object parameter )
        {
            var cancellationToken = ( CancellationToken ) parameter;
            int processingIntervalInMilliseconds = 1_000;
            var lastLoggedExceptionTime = DateTime.MinValue;

            // Just keep running until we are asked to stop.
            while ( !cancellationToken.IsCancellationRequested )
            {
                var startTime = RockDateTime.Now;
                DateTime endTime;

                try
                {
                    // If we have any transactions in the queue then fire up
                    // a number of worker tasks to drain the queue in parallel.
                    if ( _fastTransactionQueue.Count > 0 )
                    {
                        int workerCount = 3;
                        var workers = new Task[workerCount];

                        for ( int i = 0; i < workerCount; i++ )
                        {
                            workers[i] = Task.Run( () => FastQueueWorker( cancellationToken ) );
                        }

                        // Wait for all the worker tasks to complete. Since we are
                        // running in a dedicated thread there is no concern about
                        // a deadlock by calling Wait().
                        Task.WhenAll( workers ).Wait();
                    }
                }
                catch ( Exception ex )
                {
                    var timeSinceLastLoggedException = RockDateTime.Now - lastLoggedExceptionTime;

                    // Only log one exception every minute at most.
                    if ( timeSinceLastLoggedException.TotalSeconds >= 60 )
                    {
                        lastLoggedExceptionTime = RockDateTime.Now;

                        try
                        {
                            ExceptionLogService.LogException( ex );
                        }
                        catch
                        {
                            // Intentionally ignored.
                        }
                    }
                }
                finally
                {
                    endTime = RockDateTime.Now;
                }

                var executionTimeInMilliseconds = ( int ) ( endTime - startTime ).TotalMilliseconds;

                // If we have a processing interval of 1,000ms and processing
                // took 200ms then we want only want to sleep for 800ms.
                var waitMilliseconds = processingIntervalInMilliseconds - executionTimeInMilliseconds;

                if ( waitMilliseconds > 0 )
                {
                    Thread.Sleep( waitMilliseconds );
                }
            }
        }

        /// <summary>
        /// Worker task that will run on the standard task pool to process
        /// transactions until the fast queue is empty.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token will be used to signal if we should stop processing.</param>
        private static void FastQueueWorker( CancellationToken cancellationToken )
        {
            while ( !cancellationToken.IsCancellationRequested && _fastTransactionQueue.TryDequeue( out var transaction ) )
            {
                try
                {
                    var startTime = RockDateTime.Now;
                    transaction.Execute();
                    var duration = RockDateTime.Now - startTime;

                    var durationMilliseconds = ( int ) duration.TotalMilliseconds;

                    // If the transaction took a long time to run, log something so it can be
                    // looked at later.
                    if ( durationMilliseconds > 2_500 )
                    {
                        var level = Logging.RockLogLevel.Info;
                        var message = $"Fast execution of transaction {transaction.GetType().Name} took {durationMilliseconds} ms.";

                        if ( durationMilliseconds > 10_000 )
                        {
                            level = Logging.RockLogLevel.Error;
                        }
                        else if ( durationMilliseconds > 5_000 )
                        {
                            level = Logging.RockLogLevel.Warning;
                        }

                        Logging.RockLogger.Log.WriteToLog( level, Logging.RockLogDomains.Core, message );
                    }
                }
                catch ( Exception ex )
                {
                    try
                    {
                        ExceptionLogService.LogException( new Exception( $"Exception in FastQueueWorker(): {transaction.GetType().Name}", ex ) );
                    }
                    catch
                    {
                        // Intentionally ignored, we have to keep working.
                    }
                }
            }
        }

        #endregion

        #region Obsolete

        /// <summary>
        /// Gets the currently executing transaction progress. This should be between 0 and 100
        /// percent or null if the progress cannot be reported.
        /// </summary>
        /// <value>
        /// The currently executing transaction progress.
        /// </value>
        [Obsolete( "This property should not be used and will be removed in a future version of Rock." )]
        [RockObsolete( "1.15" )]
        public static int? CurrentlyExecutingTransactionProgress { get; private set; }

        /// <summary>
        /// The currently executing transaction.
        /// </summary>
        /// <value>
        /// The currently executing transaction.
        /// </value>
        [Obsolete( "This property should not be used and will be removed in a future version of Rock." )]
        [RockObsolete( "1.15" )]
        public static ITransaction CurrentlyExecutingTransaction { get; private set; }

        /// <summary>
        /// Gets or sets the transaction queue.
        /// </summary>
        /// <value>
        /// The transaction queue.
        /// </value>
        [Obsolete( "Use the method GetQueuedStandardTransactions() instead." )]
        [RockObsolete( "1.15" )]
        public static ConcurrentQueue<ITransaction> TransactionQueue { get; set; } = new ConcurrentQueue<ITransaction>();

        /// <summary>
        /// Determines whether a transaction of a certain type is being run.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///   <c>true</c> if [has transaction of type]; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete( "This method should not be used and will be removed in a future version of Rock." )]
        [RockObsolete( "1.15" )]
        public static bool IsExecuting<T>() where T : ITransaction 
        {
            return CurrentlyExecutingTransaction?.GetType() == typeof( T );
        }

        /// <summary>
        /// Determines whether a transaction of a certain type is in the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///   <c>true</c> if [has transaction of type]; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete( "This method should not be used and will be removed in a future version of Rock." )]
        [RockObsolete( "1.15" )]
        public static bool IsInQueue<T>() where T : ITransaction
        {
            return TransactionQueue.Any( t => t.GetType() == typeof( T ) );
        }

        #endregion
    }
}