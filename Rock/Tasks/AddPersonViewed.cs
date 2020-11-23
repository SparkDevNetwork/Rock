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
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public sealed class AddPersonViewed : BusStartedTask<AddPersonViewed.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            // store the view to the database if the viewer is NOT the target (don't track looking at your own record)
            if ( message.ViewerPersonAliasId != message.TargetPersonAliasId )
            {
                var pvRecord = new PersonViewed();
                pvRecord.TargetPersonAliasId = message.TargetPersonAliasId;
                pvRecord.ViewerPersonAliasId = message.ViewerPersonAliasId;
                pvRecord.ViewDateTime = message.DateTimeViewed;
                pvRecord.IpAddress = message.IPAddress;
                pvRecord.Source = message.Source;

                using ( var rockContext = new RockContext() )
                {
                    var pvService = new PersonViewedService( rockContext );
                    pvService.Add( pvRecord );
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the viewer person id.
            /// </summary>
            /// <value>
            /// The viewer person id.
            /// </value>
            public int ViewerPersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the target person id.
            /// </summary>
            /// <value>
            /// The target person id.
            /// </value>
            public int TargetPersonAliasId { get; set; }

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
        }
    }
}