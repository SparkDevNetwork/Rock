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

namespace Rock.Lava
{
    /// <summary>
    /// A component that is capable of providing a compiled Lava template object from a Lava source document.
    /// The strategy for compiling, caching and retrieving the object is determined by the provider.
    /// </summary>
    public interface ILavaTemplateCacheService
    {
        /// <summary>
        /// The LavaEngine instance that is responsible for compiling a template before it is added to the cache.
        /// </summary>
        ILavaEngine LavaEngine { get; set; }

        /// <summary>
        /// Gets a compiled template from cache or adds it to the cache if it does not already exist.
        /// </summary>
        /// <param name="templateContent">The template source text.</param>
        /// <param name="cacheKey">An optional key that uniquely identifies the template. If not specified, the template source text is used to calculate a key for cache storage and retrieval.</param>
        /// <returns></returns>
        ILavaTemplate GetOrAddTemplate( string templateContent, string cacheKey = null );

        /// <summary>
        /// Remove all templates from the cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Gets a flag indicating if the cache contains the specified template.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool ContainsTemplate( string content );

        /// <summary>
        /// Removes the specified template from the cache.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        void RemoveTemplate( string content );

        /// <summary>
        /// Gets the number of times a request for a template has been satisfied from the cache.
        /// </summary>
        long CacheHits { get; }

        /// <summary>
        /// Gets the number of times a request for a template has required that the template be compiled and cached.
        /// </summary>
        long CacheMisses { get; }
    }
}