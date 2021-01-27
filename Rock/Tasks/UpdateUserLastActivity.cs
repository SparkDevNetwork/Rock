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
    /// Tracks when a person is viewed.
    /// </summary>
    public sealed class UpdateUserLastActivity : BusStartedTask<UpdateUserLastActivity.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var user = userLoginService.Get( message.UserId );

                if ( user != null )
                {
                    user.LastActivityDateTime = message.LastActivityDate;
                    user.IsOnLine = message.IsOnline;

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
            /// Gets or sets the user id.
            /// </summary>
            /// <value>
            /// The user id.
            /// </value>
            public int UserId { get; set; }

            /// <summary>
            /// Gets or sets the last activity date.
            /// </summary>
            /// <value>
            /// The last activity date.
            /// </value>
            public DateTime LastActivityDate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [is on line].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [is on line]; otherwise, <c>false</c>.
            /// </value>
            public bool IsOnline { get; set; } = true;
        }
    }
}