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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Lava shortcode that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class LavaShortcodeCache : CachedModel<LavaShortcode>
    {
        #region Constructors

        private LavaShortcodeCache()
        {
        }

        private LavaShortcodeCache( LavaShortcode shortcode )
        {
            CopyFromModel( shortcode );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the documentation.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the markup.
        /// </summary>
        /// <value>
        /// The markup.
        /// </value>
        public string Markup { get; set; }

        /// <summary>
        /// Gets or sets the type of the tag.
        /// </summary>
        /// <value>
        /// The type of the tag.
        /// </value>
        public TagType TagType { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        public string EnabledLavaCommands { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is LavaShortcode )
            {
                var shortcode = ( LavaShortcode ) model;
                this.IsSystem = shortcode.IsSystem;
                this.Name = shortcode.Name;
                this.Description = shortcode.Description;
                this.IsActive = shortcode.IsActive;
                this.Documentation = shortcode.Documentation;
                this.TagName = shortcode.TagName;
                this.Markup = shortcode.Markup;
                this.TagType = shortcode.TagType;
                this.Parameters = shortcode.Parameters;
                this.EnabledLavaCommands = shortcode.EnabledLavaCommands;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the cache key for the selected lava shortcode id.
        /// </summary>
        /// <param name="id">The lava shortcode id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:LavaShortcode:{0}", id );
        }

        /// <summary>
        /// Returns lava shortcode object from cache.  If lava shortcode does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static LavaShortcodeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( LavaShortcodeCache.CacheKey( id ), 
                () => LoadById( id, rockContext ) );
        }

        private static LavaShortcodeCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static LavaShortcodeCache LoadById2( int id, RockContext rockContext )
        {
            var lavaShortcodeService = new LavaShortcodeService( rockContext );
            var shortcodeModel = lavaShortcodeService
                .Queryable().AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( shortcodeModel != null )
            {
                return new LavaShortcodeCache( shortcodeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static LavaShortcodeCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var lavaShortcodeService = new LavaShortcodeService( rockContext );
            return lavaShortcodeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ))
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified tag name.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static LavaShortcodeCache Read( string tagName, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( tagName,
                () => LoadByTagName( tagName, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByTagName( string tagName, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByTagName2( tagName, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByTagName2( tagName, rockContext2 );
            }
        }

        private static int LoadByTagName2( string tagName, RockContext rockContext )
        {
            var lavaShortcodeService = new LavaShortcodeService( rockContext );
            return lavaShortcodeService
                .Queryable().AsNoTracking()
                .Where( c => c.TagName == tagName )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds lava shortcode model to cache, and returns cached object
        /// </summary>
        /// <param name="shortcodeModel"></param>
        /// <returns></returns>
        public static LavaShortcodeCache Read( LavaShortcode shortcodeModel )
        {
            return GetOrAddExisting( LavaShortcodeCache.CacheKey( shortcodeModel.Id ),
                () => LoadByModel( shortcodeModel ) );
        }

        private static LavaShortcodeCache LoadByModel( LavaShortcode shortcodeModel )
        {
            if ( shortcodeModel != null )
            {
                return new LavaShortcodeCache( shortcodeModel );
            }
            return null;
        }

        /// <summary>
        /// Returns all Lava shortcodes
        /// </summary>
        /// <returns></returns>
        public static List<LavaShortcodeCache> All()
        {
            return All( true );
        }

        /// <summary>
        /// Returns all Lava shortcodes
        /// </summary>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <returns></returns>
        public static List<LavaShortcodeCache> All( bool includeInactive )
        {
            List<LavaShortcodeCache> shortcodes = new List<LavaShortcodeCache>();
            var shortcodeIds = GetOrAddExisting( "Rock:LavaShortcode:All", () => LoadAll() );
            if ( shortcodeIds != null )
            {
                foreach ( int shortcodeId in shortcodeIds )
                {
                    var shortcodeCache = LavaShortcodeCache.Read( shortcodeId );
                    if ( shortcodeCache != null )
                    {
                        if ( includeInactive || shortcodeCache.IsActive )
                        {
                            shortcodes.Add( shortcodeCache );
                        }
                    }
                }
            }
            return shortcodes;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new LavaShortcodeService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes Lava shortcode from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( LavaShortcodeCache.CacheKey( id ) );
            FlushCache( "Rock:LavaShortcode:All" );
        }

        #endregion
    }
}