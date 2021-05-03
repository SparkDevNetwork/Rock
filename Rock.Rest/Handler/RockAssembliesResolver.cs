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
using System.Reflection;

namespace Rock.Rest
{
    /// <summary>
    /// The default IAssembliesResolver looks in ALL assemblies in the RockWeb/bin folder when
    /// trying to discover [route]'s, etc. Since we know that they would only
    /// be in Rock Assemblies, we can give it a smaller list.
    /// This improves the startup performance.
    /// </summary>
    public class RockAssembliesResolver : System.Web.Http.Dispatcher.IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>
        /// An &lt;see cref="T:System.Collections.Generic.ICollection`1" /&gt; of assemblies.
        /// </returns>
        public ICollection<Assembly> GetAssemblies()
        {
            return Reflection.GetPluginAssemblies();
        }
    }
}
