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

namespace Rock.Lava
{
    /// <summary>
    /// A factory that supplies metadata and new instances for shortcodes that are available to the Lava engine.
    /// This provider may also implement caching of shortcode definitions if necessary.
    /// </summary>
    public interface ILavaShortcodeProvider
    {
        /// <summary>
        /// Clear a cached shortcode definition.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Clear a cached shortcode definition.
        /// </summary>
        void RemoveFromCache( string shortcodeName );

        /// <summary>
        /// Get the definition of the named shortcode.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        DynamicShortcodeDefinition GetShortcodeDefinition( string shortcodeName );

        /// <summary>
        /// Get a new instance of the named shortcode.
        /// </summary>
        /// <param name="shortcodeName"></param>
        /// <returns></returns>
        ILavaShortcode GetShortcodeInstance( string shortcodeName, ILavaEngine engine );
    }
}
