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
using System.Linq;

namespace Rock.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    static public class RockQueue
    {
        /// <summary>
        /// Gets the currently executing transaction progress. This should be between 0 and 100
        /// percent or null if the progress cannot be reported.
        /// </summary>
        /// <value>
        /// The currently executing transaction progress.
        /// </value>
        [Obsolete( "Use Rock.Tasks instead of transactions" )]
        [RockObsolete( "1.13" )]
        public static int? CurrentlyExecutingTransactionProgress { get; private set; }

        /// <summary>
        /// The currently executing transaction.
        /// </summary>
        /// <value>
        /// The currently executing transaction.
        /// </value>
        public static ITransaction CurrentlyExecutingTransaction { get; private set; }

        /// <summary>
        /// Gets or sets the transaction queue.
        /// </summary>
        /// <value>
        /// The transaction queue.
        /// </value>
        public static ConcurrentQueue<ITransaction> TransactionQueue { get; set; }

        /// <summary>
        /// Drains this queue.
        /// </summary>
        /// <param name="errorHandler">The error handler.</param>
        public static void Drain( Action<Exception> errorHandler )
        {
            while ( TransactionQueue.TryDequeue( out var transaction ) )
            {
                CurrentlyExecutingTransaction = transaction;

                if ( CurrentlyExecutingTransaction == null )
                {
                    continue;
                }

                try
                {
                    CurrentlyExecutingTransaction.Execute();
                }
                catch ( Exception ex )
                {
                    errorHandler( new Exception( string.Format( "Exception in Global.DrainTransactionQueue(): {0}", transaction.GetType().Name ), ex ) );
                }
            }
        }

        /// <summary>
        /// Determines whether a transaction of a certain type is being run.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        ///   <c>true</c> if [has transaction of type]; otherwise, <c>false</c>.
        /// </returns>
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
        public static bool IsInQueue<T>() where T : ITransaction
        {
            return TransactionQueue.Any( t => t.GetType() == typeof( T ) );
        }

        /// <summary>
        /// Initializes the <see cref="RockQueue" /> class.
        /// </summary>
        static RockQueue()
        {
            TransactionQueue = new ConcurrentQueue<ITransaction>();
        }
    }
}