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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using DotLiquid;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class LavaTemplateCache 
    {
        #region Constructors

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
        [DataMember]
        public Template Template { get; set; }

        #endregion

        #region Static Methods

        private static string CacheKey( string content )
        {
            return string.Format( "Rock:LavaTemplate:{0}", content );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If definedValue does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static LavaTemplateCache Read( string content )
        {
            return GetOrAddExisting( LavaTemplateCache.CacheKey( content ),
                () => Load( content ) );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public static LavaTemplateCache GetOrAddExisting( string key, Func<LavaTemplateCache> valueFactory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            object cacheValue = cache.Get( key );
            if ( cacheValue != null )
            {
                return (LavaTemplateCache)cacheValue;
            }

            LavaTemplateCache value = valueFactory();

            var cacheItemPolicy = new CacheItemPolicy();
            cacheItemPolicy.SlidingExpiration = new TimeSpan( 0, 10, 0 );
            cache.Set( key, value, cacheItemPolicy );

            return value;
        }

        private static LavaTemplateCache Load( string content )
        {
            var lavaTemplate = new LavaTemplateCache();
            lavaTemplate.Template = Template.Parse( content );
            return lavaTemplate;
        }

        /// <summary>
        /// Flushes all the LavaTemplateCache items
        /// </summary>
        public static void Flush()
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            var lavaTemplateCaches = cache.Where( a => a.Key.StartsWith( "Rock:LavaTemplate:" ) );
            foreach ( var lavaTemplateCache in lavaTemplateCaches )
            {
                cache.Remove( lavaTemplateCache.Key );
            }
        }

        #endregion
    }
}