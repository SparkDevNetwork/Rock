using System;

namespace Rock.Jobs
{
    /// <summary>
    /// Job failed to load exception
    /// </summary>
    [Serializable]
    public class JobLoadFailedException : System.Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="JobLoadFailedException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public JobLoadFailedException( string message ) : base( message )
        {
        
        }

    }
}