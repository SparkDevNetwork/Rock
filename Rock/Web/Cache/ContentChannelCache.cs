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
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Content channel that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class ContentChannelCache : CachedModel<ContentChannel>
    {
        #region Constructors

        private ContentChannelCache()
        {
        }

        private ContentChannelCache( ContentChannel contentChannel )
        {
            CopyFromModel( contentChannel );
        }

        #endregion

        #region Properties

        private object _obj = new object();

        /// <summary>
        /// Gets or sets the content channel type identifier.
        /// </summary>
        /// <value>
        /// The content channel type identifier.
        /// </value>
        public int ContentChannelTypeId { get; set; }

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
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approval].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires approval]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        public bool ItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [child items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        public bool ChildItemsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable RSS].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable RSS]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableRss { get; set; }

        /// <summary>
        /// Gets or sets the channel URL.
        /// </summary>
        /// <value>
        /// The channel URL.
        /// </value>
        public string ChannelUrl { get; set; }

        /// <summary>
        /// Gets or sets the item URL.
        /// </summary>
        /// <value>
        /// The item URL.
        /// </value>
        public string ItemUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes a feed can stay cached before refreshing it from the source.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the type of the control to render when editing content for items of this type.
        /// </summary>
        /// <value>
        /// The type of the item control.
        /// </value>
        public ContentControlType ContentControlType { get; set; }

        /// <summary>
        /// Gets or sets the root image directory to use when the Html control type is used
        /// </summary>
        /// <value>
        /// The image root directory.
        /// </value>
        public string RootImageDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve channel items." );
                supportedActions.AddOrReplace( Rock.Security.Authorization.INTERACT, "The roles and/or users that have access to intertact with the channel item." );
                return supportedActions;
            }
        }

        /// <summary>
        /// Gets the child Content Channels.
        /// </summary>
        /// <value>
        /// The child ContentChannels.
        /// </value>
        public List<ContentChannelCache> ChildContentChannels
        {
            get
            {
                var childContentChannels = new List<ContentChannelCache>();

                lock ( _obj )
                {
                    if ( childContentChannels == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            childContentChannelIds = new ContentChannelService( rockContext )
                                .GetChildContentChannels( this.Id )
                                .Select( g => g.Id )
                                .ToList();
                        }
                    }
                }
                if ( childContentChannelIds != null )
                {
                    foreach ( int id in childContentChannelIds )
                    {
                        var contentChannel = ContentChannelCache.Read( id );
                        if ( contentChannel != null )
                        {
                            childContentChannels.Add( contentChannel );
                        }
                    }
                }

                return childContentChannels;
            }
        }
        private List<int> childContentChannelIds = null;

        /// <summary>
        /// Gets the parent content channels.
        /// </summary>
        /// <value>
        /// The parent content channels.
        /// </value>
        public List<ContentChannelCache> ParentContentChannels
        {
            get
            {
                var parentContentChannels = new List<ContentChannelCache>();

                lock ( _obj )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        parentContentChannelIds = new ContentChannelService( rockContext )
                            .GetParentContentChannels( this.Id )
                            .Select( g => g.Id )
                            .ToList();
                    }
                }

                if ( parentContentChannelIds != null )
                {
                    foreach ( int id in parentContentChannelIds )
                    {
                        var contentChannel = ContentChannelCache.Read( id );
                        if ( contentChannel != null )
                        {
                            parentContentChannels.Add( contentChannel );
                        }
                    }
                }

                return parentContentChannels;
            }

        }
        private List<int> parentContentChannelIds = null;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                using ( var rockContext = new RockContext() )
                {
                    var contentChannelType = new Model.ContentChannelTypeService( rockContext ).Get( ContentChannelTypeId );
                    return contentChannelType != null ? contentChannelType : base.ParentAuthority;
                }
                
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is ContentChannel )
            {
                var contentChannel = ( ContentChannel ) model;
                this.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
                this.Name = contentChannel.Name;
                this.Description = contentChannel.Description;
                this.IconCssClass = contentChannel.IconCssClass;
                this.RequiresApproval = contentChannel.RequiresApproval;
                this.ItemsManuallyOrdered = contentChannel.ItemsManuallyOrdered;
                this.ChildItemsManuallyOrdered = contentChannel.ChildItemsManuallyOrdered;
                this.EnableRss = contentChannel.EnableRss;
                this.ChannelUrl = contentChannel.ChannelUrl;
                this.ItemUrl = contentChannel.ItemUrl;
                this.TimeToLive = contentChannel.TimeToLive;
                this.ContentControlType = contentChannel.ContentControlType;
                this.RootImageDirectory = contentChannel.RootImageDirectory;
                this.IsIndexEnabled = contentChannel.IsIndexEnabled;
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
        /// Gets the cache key for the selected content channel id.
        /// </summary>
        /// <param name="id">The content channel id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:ContentChannel:{0}", id );
        }

        /// <summary>
        /// Returns content channel object from cache.  If content channel does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static ContentChannelCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( ContentChannelCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static ContentChannelCache LoadById( int id, RockContext rockContext )
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

        private static ContentChannelCache LoadById2( int id, RockContext rockContext )
        {
            var contentChannelService = new ContentChannelService( rockContext );
            var contentChannelModel = contentChannelService
                .Queryable().AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( contentChannelModel != null )
            {
                return new ContentChannelCache( contentChannelModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static ContentChannelCache Read( Guid guid, RockContext rockContext = null )
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
            var contentChannelService = new ContentChannelService( rockContext );
            return contentChannelService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds content channel model to cache, and returns cached object
        /// </summary>
        /// <param name="contentChannel"></param>
        /// <returns></returns>
        public static ContentChannelCache Read( ContentChannel contentChannel )
        {
            return GetOrAddExisting( ContentChannelCache.CacheKey( contentChannel.Id ),
                () => LoadByModel( contentChannel ) );
        }

        private static ContentChannelCache LoadByModel( ContentChannel contentChannel )
        {
            if ( contentChannel != null )
            {
                return new ContentChannelCache( contentChannel );
            }
            return null;
        }

        /// <summary>
        /// Returns all content channels
        /// </summary>
        /// <returns></returns>
        public static List<ContentChannelCache> All()
        {
            List<ContentChannelCache> contentChannels = new List<ContentChannelCache>();
            var contentChannelIds = GetOrAddExisting( "Rock:ContentChannel:All", () => LoadAll() );
            if ( contentChannelIds != null )
            {
                foreach ( int contentChannelId in contentChannelIds )
                {
                    var contentChannelCache = ContentChannelCache.Read( contentChannelId );
                    if ( contentChannelCache != null )
                    {
                        contentChannels.Add( contentChannelCache );
                    }
                }
            }
            return contentChannels;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new ContentChannelService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes content channel from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( ContentChannelCache.CacheKey( id ) );
            FlushCache( "Rock:ContentChannel:All" );
        }

        #endregion
    }
}