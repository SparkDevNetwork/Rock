using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Jobs
{
    [Serializable]
    public class JobLoadFailedException : System.Exception
    {

        public JobLoadFailedException( string message ) : base( message )
        {
        
        }

    }
}