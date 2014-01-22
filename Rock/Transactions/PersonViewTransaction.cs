// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class PersonViewTransaction : ITransaction
    {

        /// <summary>
        /// Gets or sets the viewer person id.
        /// </summary>
        /// <value>
        /// The viewer person id.
        /// </value>
        public int ViewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the target person id.
        /// </summary>
        /// <value>
        /// The target person id.
        /// </value>
        public int TargetPersonId { get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the source of the view (site id or application name)
        /// </summary>
        /// <value>
        /// Source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the DateTime the person was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateTimeViewed { get; set; }
        
        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            // store the view to the database if the viewer is NOT the target (don't track looking at your own record)
            if ( ViewerPersonId != TargetPersonId )
            {
                using ( new Rock.Data.UnitOfWorkScope() )
                {
                    var pvRecord = new PersonViewed();
                    pvRecord.TargetPersonId = TargetPersonId;
                    pvRecord.ViewerPersonId = ViewerPersonId;
                    pvRecord.ViewDateTime = DateTimeViewed;
                    pvRecord.IpAddress = IPAddress;
                    pvRecord.Source = Source;

                    var pvService = new PersonViewedService();
                    pvService.Add( pvRecord, null );
                    pvService.Save( pvRecord, null );
                }
            }
        }
    }
}