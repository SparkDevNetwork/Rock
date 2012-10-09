//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Jobs
{
    /// <summary>
    /// Job failed to load exception
    /// </summary>
    [Serializable]
    public class JobLoadFailedException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="JobLoadFailedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public JobLoadFailedException( string message )
            : base( message )
        {

        }

    }
}