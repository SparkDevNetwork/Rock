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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rock.Obsidian.Blocks;

namespace Rock.Obsidian.Util
{
    /// <summary>
    /// Reflection Helpers
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Gets the namespaces that start with the given root.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="rootNamespace">The root namespace.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetNamespacesThatStartWith( string rootNamespace )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var namespaces = Rock.Reflection.GetNamespacesThatStartWith( assembly, rootNamespace );
            return namespaces;
        }

        /// <summary>
        /// Gets the block categories.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetBlockCategories()
        {
            var type = typeof( ObsidianBlockType );
            var rootNamespace = $"{type.Namespace}.";
            var namespaces = GetNamespacesThatStartWith( rootNamespace );
            return namespaces.Select( ns => ns.Replace( rootNamespace, string.Empty ) ).Distinct();
        }
    }
}
