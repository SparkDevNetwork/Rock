// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
    internal class Container
    {
        /// <summary>
        /// Resolves the container name into a Type in a unified way.
        /// </summary>
        /// <param name="containerAssemblyName">Name of the container assembly.</param>
        /// <returns>The container type.</returns>
        public static Type ResolveContainer (string containerAssemblyName)
        {
            return Type.GetType( containerAssemblyName );
        }
    }
}
