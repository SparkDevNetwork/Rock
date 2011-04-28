using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;

namespace Rock.Services
{
    interface IFeed
    {
        string ReturnFeed( int key, int count, string format, out string errorMessage, out string contentType );
    }
}
