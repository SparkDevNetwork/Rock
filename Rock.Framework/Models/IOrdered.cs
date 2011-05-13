using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Models
{
    internal interface IOrdered
    {
        int Order { get; }
    }
}
