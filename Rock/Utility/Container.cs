using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    class Container
    {
        public static Type resolveContainer (string containerAssemblyName)
        {
            return Type.GetType( containerAssemblyName );
        }
    }
}
