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

using Rock.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Engine
{
    /// <summary>
    /// An implementation of a provider for Lava Template objects that does not perform any caching.
    /// This is intended for use in performance tests where caching may skew the results.
    /// </summary>
    public class NullTemplateCacheService : ILavaTemplateCacheService
    {
        #region ILavaTemplateCacheService implementation

        private long _cacheMisses = 0;

        public void Initialize( string cacheKeyPrefix )
        {
            return;
        }

        void ILavaTemplateCacheService.AddTemplate( ILavaTemplate template, string cacheKey )
        {
            return;
        }

        long ILavaTemplateCacheService.CacheHits
        {
            get
            {
                return 0;
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
        }

        bool ILavaTemplateCacheService.ContainsKey( string key )
        {
            return false;
        }

        string ILavaTemplateCacheService.GetCacheKeyForTemplate( string content )
        {
            return string.Empty;
        }

        void ILavaTemplateCacheService.RemoveTemplate( string content )
        {
            return;
        }

        void ILavaTemplateCacheService.RemoveKey( string content )
        {
            return;
        }

        ILavaTemplate ILavaTemplateCacheService.GetOrAddTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            throw new NotImplementedException( "Obsolete." );
        }

        AddOrGetTemplateResult ILavaTemplateCacheService.AddOrGetTemplate( ILavaEngine engine, string templateContent, string cacheKey )
        {
            _cacheMisses++;

            var parseResult = engine.ParseTemplate( templateContent );

            ILavaTemplate template;

            if ( parseResult.HasErrors )
            {
                // If the template is invalid, cache the error message to prevent subsequent parse attempts.
                try
                {
                    template = engine.ParseTemplate( parseResult.GetLavaException().GetUserMessage() ).Template;
                }
                catch
                {
                    template = engine.ParseTemplate( "#Lava Template Error#" ).Template;
                }
            }
            else
            {
                template = parseResult.Template;
            }

            var result = new AddOrGetTemplateResult( template, parseResult.Error );
            return result;
        }

        #endregion
    }
}