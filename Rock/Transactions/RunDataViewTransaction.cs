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
    /// Tracks when a dataview is run.
    /// </summary>
    public class RunDataViewTransaction : ITransaction
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
        public DateTime LastRunDate { get; set; }

        /// <summary>
        /// The amount of time in milliseconds that it took to run the <see cref="DataView"/>
        /// </summary>
        /// <value>
        /// The time to run in ms.
        /// </value>
        public int? TimeToRunMS { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunDataViewTransaction"/> class.
        /// </summary>
        public RunDataViewTransaction()
        {
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var dataViewService = new DataViewService( rockContext );
                var dataView = dataViewService.Get( DataViewId );

                if ( dataView != null )
                {
                    dataView.LastRunDateTime = LastRunDate;
                    dataView.TimeToRunMS = TimeToRunMS;
                    dataView.RunCount = ( dataView.RunCount ?? 0 ) + 1;

                    rockContext.SaveChanges();
                }
            }
        }
    }
}