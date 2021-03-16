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
using System.Text.RegularExpressions;
using DotLiquid;
using Rock.Lava;

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
            DefaultLifespan = TimeSpan.FromMinutes( 10 );
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
        [Obsolete( "No longer needed", true )]
        public static LavaTemplateCache GetOrAddExisting( string key, Func<LavaTemplateCache> valueFactory )
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
        [Obsolete( "Use Get instead", true )]
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

            return ItemCache<LavaTemplateCache>.GetOrAddExisting( key, () => Load( content ) );
        }

        private static LavaTemplateCache Load( string content )
        {
            // Strip out Lava comments before parsing the template because they are not recognized by standard Liquid syntax.
            content = LavaHelper.RemoveLavaComments( content );

            var template = Template.Parse( content );

            /* 
             * 2/19/2020 - JPH
             * The DotLiquid library's Template object was not originally designed to be thread safe, but a PR has since
             * been merged into that repository to add this functionality (https://github.com/dotliquid/dotliquid/pull/220).
             * We have cherry-picked the PR's changes into our DotLiquid project, allowing the Template to operate safely
             * in a multithreaded context, which can happen often with our cached Template instances.
             *
             * Reason: Rock Issue #4084, Weird Behavior with Lava Includes
             */
            template.MakeThreadSafe();

            var lavaTemplate = new LavaTemplateCache { Template = template };
            return lavaTemplate;
        }

        #endregion
    }
}