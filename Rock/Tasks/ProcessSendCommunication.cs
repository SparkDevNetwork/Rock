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
using Rock.Utility;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends a communication
    /// </summary>
    public sealed class ProcessSendCommunication : BusStartedTask<ProcessSendCommunication.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var communication = new CommunicationService( rockContext ).Get( message.CommunicationId );
                AsyncHelper.RunSync( () => Model.Communication.SendAsync( communication ) );
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the communication id
            /// </summary>
            /// <value>
            /// The communication id.
            /// </value>
            public int CommunicationId { get; set; }
        }
    }
}