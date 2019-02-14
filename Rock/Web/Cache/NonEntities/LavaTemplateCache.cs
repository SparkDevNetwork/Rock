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

using DotLiquid;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class LavaTemplateCache : ItemCache<LavaTemplateCache>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private LavaTemplateCache()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        public Template Template { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "No longer needed" )]
        public static new LavaTemplateCache GetOrAddExisting( string key, Func<LavaTemplateCache> valueFactory )
        {
            return ItemCache<LavaTemplateCache>.GetOrAddExisting( key, null );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Get( string content )
        {
            return Get( content, content );
        }

        /// <summary>
        /// Reads the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use Get instead")]
        public static LavaTemplateCache Read( string content )
        {
            return Get( content );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Get( string key, string content )
        {
            // If cache items need to be serialized, do not cache the template (it's not serializable)
            if ( RockCache.IsCacheSerialized )
            {
                return Load( content );
            }

            return GetOrAddExisting( key, () => Load( content ), new TimeSpan( 0, 10, 0 ) );
        }

        private static LavaTemplateCache Load( string content )
        {
            var lavaTemplate = new LavaTemplateCache { Template = Template.Parse( content ) };
            return lavaTemplate;
        }

        #endregion
    }
}