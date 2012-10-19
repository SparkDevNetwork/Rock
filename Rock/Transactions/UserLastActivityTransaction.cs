using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Rock.Cms;

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
        public DateTimeOffset LastActivityDate { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            var userService = new UserService();
            var user = userService.Get( UserId );

            if ( user != null )
            {
                user.LastActivityDate = LastActivityDate;
                userService.Save( user, null );
            }
        }
    }
}