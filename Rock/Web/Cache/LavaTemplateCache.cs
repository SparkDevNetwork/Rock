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
using System.Runtime.Serialization;
using DotLiquid;

using Rock.Cache;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    [Obsolete( "Use Rock.Cache.LaveTemplateCache instead" )]
    public class LavaTemplateCache
    {
        #region Constructors

        private LavaTemplateCache()
        {
        }

        private LavaTemplateCache( CacheLavaTemplate cacheLavaTemplate )
        {
            Template = cacheLavaTemplate.Template;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        [DataMember]
        public Template Template { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns LavaTemplate object from cache.  If definedValue does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Read( string content )
        {
            return new LavaTemplateCache( CacheLavaTemplate.Get( content ) );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static LavaTemplateCache GetOrAddExisting( string key, Func<LavaTemplateCache> valueFactory )
        {
            return null;
        }

        /// <summary>
        /// Flushes all the LavaTemplateCache items
        /// </summary>
        public static void Flush()
        {
            CacheLavaTemplate.Clear();
        }

        #endregion
    }
}