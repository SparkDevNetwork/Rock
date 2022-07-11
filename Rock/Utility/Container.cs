using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Utility
{
    /// <summary>
    /// Provides utilities for working with Containers
    /// </summary>
    class Container
    {
        /// <summary>
        /// Resolves the container name into a Type in a unified way.
        /// </summary>
        /// <param name="containerAssemblyName">Name of the container assembly.</param>
        /// <returns>The container type.</returns>
        public static Type resolveContainer (string containerAssemblyName)
        {
            return Type.GetType( containerAssemblyName );
        }
    }
}
