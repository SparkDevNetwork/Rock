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
using System.Threading;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that supports caching in a web environnment.
    /// </summary>
    public class WebsiteLavaTemplateCacheService : ILavaTemplateCacheService
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the cache.
        /// </summary>
        public WebsiteLavaTemplateCacheService()
        {
            //
        }

        /// <summary>
        /// Create a new instance of the cache for the specified Lava engine.
        /// </summary>
        public WebsiteLavaTemplateCacheService( ILavaEngine engine )
        {
            //_cache = new WebsiteLavaTemplateCache();
            _engine = engine;

            WebsiteLavaTemplateCache.DefaultLifespan = TimeSpan.FromMinutes( 10 );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public WebsiteLavaTemplateCache Get( string content )
        {
            return Get( content, content );
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public WebsiteLavaTemplateCache Get( string key, string content )
        {
            WebsiteLavaTemplateCache template;

            // If cache items need to be serialized, do not cache the template because it isn't serializable.
            if ( RockCache.IsCacheSerialized )
            {
                template = Load( content );
            }
            else
            {
                var fromCache = true;

                template = WebsiteLavaTemplateCache.GetOrAddExisting( key, () =>
                {
                    fromCache = false;
                    return Load( content );
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
        public bool Contains( string key )
        {
            if ( RockCache.IsCacheSerialized )
            {
                return false;
            }

            bool contains = true;

            var template = WebsiteLavaTemplateCache.GetOrAddExisting( key, () =>
            {
                contains = false;
                return null;
            } );

            return contains;
        }

        private WebsiteLavaTemplateCache Load( string content )
        {
            if ( _engine == null )
            {
                throw new Exception( "WebsiteLavaTemplateCache template load failed. The cache must be initialized for a specific engine." );
            }

            var result = _engine.ParseTemplate( content );

            var cacheEntry = new WebsiteLavaTemplateCache { Template = result.Template };

            return cacheEntry;
        }

        #endregion

        #region ILavaTemplateCacheService implementation

        private long _cacheHits = 0;
        private long _cacheMisses = 0;
        private ILavaEngine _engine = null;

        void ILavaTemplateCacheService.AddTemplate( ILavaTemplate template, string cacheKey )
        {
            if ( string.IsNullOrWhiteSpace( cacheKey ) )
            {
                throw new Exception( "WebsiteLavaTemplateCache template add failed. A cache key must be specified." );
            }

            WebsiteLavaTemplateCache.UpdateCacheItem( cacheKey, new WebsiteLavaTemplateCache() { Template = template } );
        }

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

        bool ILavaTemplateCacheService.ContainsKey( string key )
        {
            return Contains( key );
        }

        string ILavaTemplateCacheService.GetCacheKeyForTemplate( string content )
        {
            var key = GetTemplateKey( content );

            return key;
        }

        void ILavaTemplateCacheService.RemoveTemplate( string content )
        {
            var key = GetTemplateKey( content );

            WebsiteLavaTemplateCache.Remove( key );
        }

        ILavaTemplate ILavaTemplateCacheService.GetOrAddTemplate( string templateContent, string cacheKey )
        {
            if ( string.IsNullOrWhiteSpace( cacheKey ) )
            {
                cacheKey = GetTemplateKey( templateContent );
            }

            var templateCache = Get( cacheKey, templateContent );

            if ( templateCache == null )
            {
                return null;
            }
            else
            {
                return templateCache.Template;
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

            return $"{ _engine.EngineType}:{templateKey}";
        }

        #endregion
    }
}