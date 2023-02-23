using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Model
{
    public partial class UserLoginService
    {
        public static UserLogin GetCurrentUser( bool userIsOnline )
        {
            return null;
        }

        /// <summary>
        /// Updates the last log in, writes to the person's history log, and saves changes to the database
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void UpdateLastLogin( string userName )
        {
        }
    }
}
