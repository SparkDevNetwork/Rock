﻿// <copyright>
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

using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Tracks when a report is run.
    /// </summary>
    public sealed class UpdateReportStatistics : BusStartedTask<UpdateReportStatistics.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var reportService = new ReportService( rockContext );
                var report = reportService.Get( message.ReportId );

                if ( report == null )
                {
                    return;
                }

                if ( message.LastRunDateTime != null )
                {
                    report.LastRunDateTime = message.LastRunDateTime;
                    report.RunCount = ( report.RunCount ?? 0 ) + 1;
                }

                // We will only update the RunCount if we were given a TimeToRun value.
                if ( message.TimeToRunDurationMilliseconds != null )
                {
                    report.TimeToRunDurationMilliseconds = message.TimeToRunDurationMilliseconds;
                }

                /*
                    8/3/2020 - JH
                    We are calling the SaveChanges( true ) overload that disables pre/post processing hooks
                    because we only want to change the properties explicitly set above. If we don't disable
                    these hooks, the [ModifiedDateTime] value will also be updated every time a Report is
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
            /// Gets or sets the report identifier.
            /// </summary>
            /// <value>
            /// The report identifier.
            /// </value>
            public int ReportId { get; set; }

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
        }
    }
}