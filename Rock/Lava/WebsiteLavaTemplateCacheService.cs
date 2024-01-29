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
using System.Web;
using Rock.Web.Cache;

namespace Rock.Lava
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that supports caching in a web environnment.
    /// </summary>
    public class WebsiteLavaTemplateCacheService : ILavaTemplateCacheService
    {
        private string _cacheKeyPrefix = string.Empty;

        #region Constructors

        /// <summary>
        /// Create a new instance of the cache.
        /// </summary>
        public WebsiteLavaTemplateCacheService()
        {
            WebsiteLavaTemplateCache.DefaultLifespan = TimeSpan.FromMinutes( 10 );
        }

        #endregion

        /// <summary>
        /// Initialize the cache service.
        /// </summary>
        /// <param name="cacheKeyPrefix"></param>
        public void Initialize( string cacheKeyPrefix )
        {
            _cacheKeyPrefix = cacheKeyPrefix;
        }

        /// <summary>
        /// Returns LavaTemplate object from cache.  If template does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="engine">The key.</param>
        /// <param name="key">The key.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        [Obsolete]
        [RockObsolete( "1.15.1" )]
        public WebsiteLavaTemplateCache Get( ILavaEngine engine, string key, string content )
        {
            WebsiteLavaTemplateCache template;

            var fromCache = true;
            template = WebsiteLavaTemplateCache.GetOrAddExisting( key, () =>
            {
                fromCache = false;

                var parseResult = CompileLavaTemplate( engine, content );

                var cacheEntry = new WebsiteLavaTemplateCache
                {
                    Template = parseResult.Template
                };

                return cacheEntry;
            } );

            if ( fromCache )
            {
                Interlocked.Increment( ref _cacheHits );
            }
            else
            {
                Interlocked.Increment( ref _cacheMisses );
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
            if ( !this.IsCacheEnabled() )
            {
                return false;
            }

            var contains = true;
            var template = WebsiteLavaTemplateCache.GetOrAddExisting( key, () =>
            {
                contains = false;
                return null;
            } );

            return contains;
        }

        private AddOrGetTemplateResult AddOrGetTemplateInternal( ILavaEngine engine, string templateContent, string cacheKey )
        {
            if ( string.IsNullOrWhiteSpace( cacheKey ) )
            {
                cacheKey = GetTemplateKey( templateContent );
            }

            if ( engine == null )
            {
                throw new Exception( "WebsiteLavaTemplateCache template load failed. The cache must be initialized for a specific engine." );
            }

            bool fromCache;
            ILavaTemplate template;
            Exception parseException = null;
            if ( this.IsCacheEnabled() )
            {
                // Retrieve the cached template or add a new template to the cache.
                fromCache = true;
                var templateCache = WebsiteLavaTemplateCache.GetOrAddExisting( cacheKey, () =>
                {
                    // Template not found in cache, so create it.
                    fromCache = false;

                    var parseResult = CompileLavaTemplate( engine, templateContent );
                    parseException = parseResult.Error;

                    var cacheEntry = new WebsiteLavaTemplateCache { Template = parseResult.Template };
                    return cacheEntry;
                } );

                template = templateCache?.Template;
            }
            else
            {
                // Caching is disabled, so return a new template instance.
                fromCache = false;

                var parseResult = CompileLavaTemplate( engine, templateContent );
                parseException = parseResult.Error;

                template = parseResult.Template;
            }

            if ( fromCache )
            {
                Interlocked.Increment( ref _cacheHits );
            }
            else
            {
                Interlocked.Increment( ref _cacheMisses );
            }

            var result = new AddOrGetTemplateResult( template, parseException );
            return result;
        }

        private LavaParseResult CompileLavaTemplate( ILavaEngine engine, string templateContent )
        {
            if ( engine == null )
            {
                throw new Exception( "WebsiteLavaTemplateCache template load failed. The cache must be initialized for a specific engine." );
            }

            var parseResult = engine.ParseTemplate( templateContent );

            // If the source Lava template is invalid, cache the error message instead to prevent subsequent parse attempts.
            if ( parseResult.HasErrors )
            {
                try
                {
                    parseResult.Template = engine.ParseTemplate( parseResult.GetLavaException().GetUserMessage() ).Template;
                }
                catch
                {
                    parseResult.Template = engine.ParseTemplate( "#Lava Template Error#" ).Template;
                }
            }

            return parseResult;
        }

        #region ILavaTemplateCacheService implementation

        private long _cacheHits = 0;
        private long _cacheMisses = 0;

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

        void ILavaTemplateCacheService.ClearCache()
        {
            WebsiteLavaTemplateCache.Clear();

            _cacheHits = 0;
            _cacheMisses = 0;
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

        void ILavaTemplateCacheService.RemoveKey( string key )
        {
            WebsiteLavaTemplateCache.Remove( key );
        }

        ILavaTemplate ILavaTemplateCacheService.GetOrAddTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            var result = AddOrGetTemplateInternal( engine, templateContent, cacheKey );
            return result.Template;
        }

        AddOrGetTemplateResult ILavaTemplateCacheService.AddOrGetTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            return AddOrGetTemplateInternal( engine, templateContent, cacheKey );
        }

        #endregion

        #region Support functions

        private bool IsCacheEnabled()
        {
            // If a web request is active, check if caching is disabled by the CACHE_CONTROL_COOKIE.
            if ( HttpContext.Current?.Request != null )
            {
                var isCachedEnabled = HttpContext.Current.Request.Cookies.Get( RockCache.CACHE_CONTROL_COOKIE );
                if ( isCachedEnabled != null && !isCachedEnabled.Value.AsBoolean() )
                {
                    return false;
                }
            }
            return true;
        }

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

            return $"{_cacheKeyPrefix}:{templateKey}";
        }

        #endregion
    }
}