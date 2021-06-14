using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Rest
{
    [Serializable]
    internal class RockApiError
    {
        public string Message { get; }

        public RockApiError( string message )
        {
            Message = message;
        }
    }
}
