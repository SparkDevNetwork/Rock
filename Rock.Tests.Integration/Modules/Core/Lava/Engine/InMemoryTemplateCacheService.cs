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
using System.Collections.Concurrent;
using System.Threading;

using Rock.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that caches teplates in memory.
    /// </summary>
    public class InMemoryTemplateCacheService : ILavaTemplateCacheService
    {
        #region ILavaTemplateCacheService implementation

        private long _cacheMisses = 0;
        private long _cacheHits = 0;
        private ConcurrentDictionary<string, ILavaTemplate> _cachedTemplates = new ConcurrentDictionary<string, ILavaTemplate>();

        public void Initialize( string cacheKeyPrefix )
        {
            return;
        }

        void ILavaTemplateCacheService.AddTemplate( ILavaTemplate template, string cacheKey )
        {
            if ( !_cachedTemplates.ContainsKey( cacheKey ) )
            {
                _cachedTemplates[cacheKey] = template;
            }
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
            _cacheMisses = 0;
            _cacheHits = 0;
        }

        bool ILavaTemplateCacheService.ContainsKey( string key )
        {
            return _cachedTemplates.ContainsKey( key );
        }

        string ILavaTemplateCacheService.GetCacheKeyForTemplate( string content )
        {
            return GetTemplateKey( content );
        }

        void ILavaTemplateCacheService.RemoveTemplate( string content )
        {
            var key = GetTemplateKey( content );
            _cachedTemplates.TryRemove( key, out _ );
        }

        AddOrGetTemplateResult ILavaTemplateCacheService.AddOrGetTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            AddOrGetTemplateResult result;

            if ( cacheKey == null )
            {
                cacheKey = GetTemplateKey( templateContent );
            }

            if ( _cachedTemplates.ContainsKey( cacheKey ) )
            {
                Interlocked.Increment( ref _cacheHits );

                result = new AddOrGetTemplateResult( _cachedTemplates[cacheKey], null );
            }
            else
            {
                Interlocked.Increment( ref _cacheMisses );

                var template = engine.ParseTemplate( templateContent ).Template;

                // If this operation fails, ignore it.
                // The template has been added by another thread.
                _cachedTemplates.TryAdd( cacheKey, template );

                result = new AddOrGetTemplateResult( template, null );
            }

            return result;
        }

        ILavaTemplate ILavaTemplateCacheService.GetOrAddTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            throw new NotImplementedException( "Obsolete." );
        }

        public void RemoveKey( string key )
        {
            _cachedTemplates.TryRemove( key, out _ );
        }

        #endregion

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
    }
}