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
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a person is viewed.
    /// </summary>
    public class UserLastActivityTransaction : ITransaction
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
        /// Gets or sets the rock user id in session.
        /// </summary>
        /// <value>
        /// The rock user id in session.
        /// </value>
        [Obsolete( "No longer used" )]
        public int? SessionUserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is on line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is on line]; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnLine { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLastActivityTransaction"/> class.
        /// </summary>
        public UserLastActivityTransaction()
        {
            IsOnLine = true;
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var user = userLoginService.Get( UserId );

                if ( user != null )
                {
                    user.LastActivityDateTime = LastActivityDate;
                    user.IsOnLine = IsOnLine;

                    rockContext.SaveChanges();
                }
            }
        }
    }
}