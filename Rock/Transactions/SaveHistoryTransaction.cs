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
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    [Obsolete( "Use a System.Task instead." )]
    [RockObsolete( "1.13" )]
    public class SaveHistoryTransaction : ITransaction
    {
        /// <summary>
        /// Keep a list of all the historyRecords that have been queued up with each new SaveHistoryTransaction() then insert them all at once 
        /// </summary>
        private static ConcurrentQueue<History> HistoryRecordsToInsert = new ConcurrentQueue<History>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveHistoryTransaction" /> class.
        /// </summary>
        /// <param name="historyRecordsToInsert">The history records to insert.</param>
        public SaveHistoryTransaction( List<History> historyRecordsToInsert )
        {
            foreach ( var historyRecord in historyRecordsToInsert )
            {
                HistoryRecordsToInsert.Enqueue( historyRecord );
            }
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            var historyRecordsToInsert = new List<History>();
            while ( HistoryRecordsToInsert.TryDequeue( out History historyRecord ) )
            {
                historyRecordsToInsert.Add( historyRecord );
            }

            if ( !historyRecordsToInsert.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.BulkInsert( historyRecordsToInsert );
            }
        }
    }
}
