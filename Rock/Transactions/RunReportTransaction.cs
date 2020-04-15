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

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a report is run.
    /// </summary>
    public class RunReportTransaction : ITransaction
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
        public int? TimeToRunMS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunReportTransaction"/> class.
        /// </summary>
        public RunReportTransaction()
        {
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var reportService = new ReportService( rockContext );
                var report = reportService.Get( ReportId );

                if ( report == null )
                {
                    return;
                }

                if ( LastRunDateTime != null )
                {
                    report.LastRunDateTime = LastRunDateTime;
                    report.RunCount = ( report.RunCount ?? 0 ) + 1;
                }

                // We will only update the RunCount if we were given a TimeToRun value.
                if ( TimeToRunMS != null )
                {
                    report.TimeToRunMS = TimeToRunMS;
                }

                rockContext.SaveChanges();
            }
        }
    }
}