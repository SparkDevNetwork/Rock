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
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Tracks when a dataview is run.
    /// </summary>
    public sealed class UpdateDataViewStatistics : BusStartedTask<UpdateDataViewStatistics.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var dataViewService = new DataViewService( rockContext );
                var dataView = dataViewService.Get( message.DataViewId );

                if ( dataView == null )
                {
                    return;
                }

                if ( message.LastRunDateTime != null )
                {
                    dataView.LastRunDateTime = message.LastRunDateTime;
                    if ( message.ShouldIncrementRunCount )
                    {
                        dataView.RunCount = ( dataView.RunCount ?? 0 ) + 1;
                    }
                }

                // We will only update the RunCount if we were given a TimeToRun value.
                if ( message.TimeToRunDurationMilliseconds != null )
                {
                    dataView.TimeToRunDurationMilliseconds = message.TimeToRunDurationMilliseconds;
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
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the dataview identifier.
            /// </summary>
            /// <value>
            /// The dataview identifier.
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
            /// The amount of time in milliseconds that it took to run the <see cref="DataView"/>
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