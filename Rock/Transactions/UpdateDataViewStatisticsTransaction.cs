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
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Rock.Transactions
{
    /// <summary>
    /// Updates the DataView statistics of a DataView that has been run.
    /// Use this instead of <see cref="Rock.Tasks.UpdateDataViewStatistics"/> since it updates all the DataViews in a batch.
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class UpdateDataViewStatisticsTransaction : ITransaction
    {
        /// <summary>
        /// Keep a list of all the DataView update requests that have been enqueued
        /// </summary>
        private static readonly ConcurrentQueue<DataViewInfo> DataViewInfoQueue = new ConcurrentQueue<DataViewInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDataViewStatisticsTransaction"/> class.
        /// </summary>
        /// <param name="dataViewInfo">The data view information.</param>
        public UpdateDataViewStatisticsTransaction( DataViewInfo dataViewInfo )
        {
            DataViewInfoQueue.Enqueue( dataViewInfo );
        }

        /// <summary>
        /// Executes method to update the DataView.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute()
        {
            // Dequeue any DataView update requests that have been queued and not processed up to this point.
            var dataViewInfos = new List<DataViewInfo>();

            while ( DataViewInfoQueue.TryDequeue( out DataViewInfo dataViewInfo ) )
            {
                dataViewInfos.Add( dataViewInfo );
            }

            if ( !dataViewInfos.Any() )
            {
                // If all the DataView update requests have been processed, exit.
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var dataViewService = new DataViewService( rockContext );
                var ids = dataViewInfos.Select( d => d.DataViewId ).Distinct().ToList();
                var dataViews = dataViewService.GetByIds( ids ).ToList();

                foreach ( var dataViewInfo in dataViewInfos )
                {
                    var dataView = dataViews.Find( d => d.Id == dataViewInfo.DataViewId );

                    if ( dataView == null )
                    {
                        return;
                    }

                    // We will only update the RunCount if we were given a LastRunDateTime value and the 
                    // RunCount has not already been updated i.e. ShouldIncrementRunCount is set to true.
                    if ( dataViewInfo.LastRunDateTime != null )
                    {
                        dataView.LastRunDateTime = dataViewInfo.LastRunDateTime;
                        if ( dataViewInfo.ShouldIncrementRunCount )
                        {
                            dataView.RunCount = ( dataView.RunCount ?? 0 ) + 1;
                        }
                    }

                    // We will only update the TimeToRunDurationMilliseconds if we were given a TimeToRun value.
                    if ( dataViewInfo.TimeToRunDurationMilliseconds != null )
                    {
                        dataView.TimeToRunDurationMilliseconds = dataViewInfo.TimeToRunDurationMilliseconds;
                    }
                }

                /*
                    8/3/2020 - JH
                    We are calling the SaveChanges( true ) overload that disables pre/post processing hooks
                    because we only want to change the properties explicitly set above. If we don't disable
                    these hooks, the [ModifiedDateTime] value will also be updated every time a DataView is
                    run, which is not what we want here.

                    Reason: GitHub Issue #4321
                    https://github.com/SparkDevNetwork/Rock/issues/4321
                */
                rockContext.SaveChanges( true );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class DataViewInfo
        {
            /// <summary>
            /// Gets or sets the DataView identifier.
            /// </summary>
            /// <value>
            /// The DataView identifier.
            /// </value>
            public int DataViewId { get; set; }

            /// <summary>
            /// Gets or sets the last run date.
            /// </summary>
            /// <value>
            /// The last run date.
            /// </value>
            public DateTime? LastRunDateTime { get; set; }

            /// <summary>
            /// The amount of time in milliseconds that it took to run the <see cref="Rock.Model.DataView"/>
            /// </summary>
            /// <value>
            /// The time to run in ms.
            /// </value>
            public int? TimeToRunDurationMilliseconds { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the run count should be incremented.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [increment run count]; otherwise, <c>false</c>.
            /// </value>
            public bool ShouldIncrementRunCount { get; set; }
        }
    }
}
