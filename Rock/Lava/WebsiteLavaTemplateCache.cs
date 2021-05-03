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
using System.Threading;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that supports caching in a web environnment.
    /// </summary>
    public class WebsiteLavaTemplateCache : ItemCache<WebsiteLavaTemplateCache>, ILavaTemplateCacheService
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new cache item.
        /// </summary>
        public WebsiteLavaTemplateCache()
        {
            DefaultLifespan = TimeSpan.FromMinutes( 10 );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Template object.
        /// </summary>
        public ILavaTemplate Template { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static WebsiteLavaTemplateCache Get( string content )
        {
            return Get( content, content, LavaEngine.CurrentEngine );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <param name="engine">The content.</param>
        /// <returns></returns>
        public static WebsiteLavaTemplateCache Get( string key, string content, ILavaEngine engine )
        {
            WebsiteLavaTemplateCache template;

            // If cache items need to be serialized, do not cache the template because it isn't serializable.
            if ( RockCache.IsCacheSerialized )
            {
                template = Load( content, engine );
            }
            else
            {
                var fromCache = true;

                template = ItemCache<WebsiteLavaTemplateCache>.GetOrAddExisting( key, () =>
                {
                    fromCache = false;
                    return Load( content, engine );
                } );

                if ( fromCache )
                {
                    Interlocked.Increment( ref _cacheHits );
                }
                else
                {
                    Interlocked.Increment( ref _cacheMisses );
                }
            }

            return template;
        }

        /// <summary>
        /// Gets a flag indicating if the cache contains the specified template.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Contains( string key )
        {
            if ( RockCache.IsCacheSerialized )
            {
                return false;
            }

            bool contains = true;

            var template = ItemCache<WebsiteLavaTemplateCache>.GetOrAddExisting( key, () =>
            {
                contains = false;
                return null;
            } );

            return contains;
        }

        private static WebsiteLavaTemplateCache Load( string content, ILavaEngine engine )
        {
            ILavaTemplate template;

            engine.TryParseTemplate( content, out template );
            //LavaEngine.CurrentEngine.TryParseTemplate( content, out template );

            var lavaTemplate = new WebsiteLavaTemplateCache { Template = template };

            return lavaTemplate;
        }

        #endregion

        #region ILavaTemplateCacheService implementation

        private static long _cacheHits = 0;
        private static long _cacheMisses = 0;
        private static ILavaEngine _engine = null;

        long ILavaTemplateCacheService.CacheHits
        {
            get
            {
                return _cacheHits;
            }
        }
        long ILavaTemplateCacheService.CacheMisses
        {
            get
            {
                return _cacheMisses;
            }
        }

        ILavaEngine ILavaTemplateCacheService.LavaEngine
        {
            get
            {
                return _engine;
            }
            set
            {
                _engine = value;
            }
        }

        void ILavaTemplateCacheService.ClearCache()
        {
            WebsiteLavaTemplateCache.Clear();
        }

        bool ILavaTemplateCacheService.ContainsTemplate( string content )
        {
            var key = GetTemplateKey( content );

            return WebsiteLavaTemplateCache.Contains( key );
        }

        void ILavaTemplateCacheService.RemoveTemplate( string content )
        {
            var key = GetTemplateKey( content );

            WebsiteLavaTemplateCache.Remove( key );
        }

        ILavaTemplate ILavaTemplateCacheService.GetOrAddTemplate( string templateContent )
        {
            var key = GetTemplateKey( templateContent );

            var templateCache = Get( key, templateContent, _engine );

            if ( templateCache == null )
            {
                return null;
            }
            else
            {
                return templateCache.Template as ILavaTemplate;
            }
        }

        #endregion

        #region Support functions

        private string GetTemplateKey( string content )
        {
            const int hashLength = 10;
            string templateKey;

            if ( string.IsNullOrEmpty( content ) )
            {
                // Cache the null template specifically, but process other whitespace templates individually
                // to ensure that the format of the final output is preserved.
                templateKey = string.Empty;
            }
            else if ( content.Length <= hashLength )
            {
                // If the content is less than the size of the MD5 hash,
                // simply use the content as the key to save processing time.
                templateKey = content;
            }
            else
            {
                // Calculate a hash of the content using xxHash.
                templateKey = content.XxHash();
            }

            return templateKey;
        }

        #endregion
    }
}