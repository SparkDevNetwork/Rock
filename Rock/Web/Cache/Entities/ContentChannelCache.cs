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
using System.Linq;
using System.Runtime.Serialization;
using Rock.Cms;
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
    [DataContract]
    public class ContentChannelCache : ModelCache<ContentChannelCache, ContentChannel>
    {
        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the content channel type identifier.
        /// </summary>
        /// <value>
        /// The content channel type identifier.
        /// </value>
        [DataMember]
        public int ContentChannelTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [requires approval].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires approval]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresApproval { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ItemsManuallyOrdered { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether child items are manually ordered or not
        /// </summary>
        /// <value>
        /// <c>true</c> if [child items manually ordered]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ChildItemsManuallyOrdered { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable RSS].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable RSS]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableRss { get; private set; }

        /// <summary>
        /// Gets or sets the channel URL.
        /// </summary>
        /// <value>
        /// The channel URL.
        /// </value>
        [DataMember]
        public string ChannelUrl { get; private set; }

        /// <summary>
        /// Gets or sets the item URL.
        /// </summary>
        /// <value>
        /// The item URL.
        /// </value>
        [DataMember]
        public string ItemUrl { get; private set; }

        /// <summary>
        /// Gets or sets the number of minutes a feed can stay cached before refreshing it from the source.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        [DataMember]
        public int? TimeToLive { get; private set; }

        /// <summary>
        /// Gets or sets the type of the control to render when editing content for items of this type.
        /// </summary>
        /// <value>
        /// The type of the item control.
        /// </value>
        [DataMember]
        public ContentControlType ContentControlType { get; private set; }

        /// <summary>
        /// Gets or sets the root image directory to use when the Html control type is used
        /// </summary>
        /// <value>
        /// The image root directory.
        /// </value>
        [DataMember]
        public string RootImageDirectory { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable personalization].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable personalization]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnablePersonalization { get; private set; }

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        [DataMember]
        public List<int> CategoryIds { get; private set; }

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
                supportedActions.AddOrReplace( Authorization.APPROVE, "The roles and/or users that have access to approve channel items." );
                supportedActions.AddOrReplace( Authorization.INTERACT, "The roles and/or users that have access to interact with the channel item." );
                return supportedActions;
            }
        }

        /// <summary>
        /// Gets or sets the child content channel ids.
        /// </summary>
        /// <value>
        /// The child content channel ids.
        /// </value>
        [DataMember]
        public List<int> ChildContentChannelIds { get; private set; }

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

                if ( ChildContentChannelIds == null )
                {
                    lock ( _obj )
                    {
                        if ( ChildContentChannelIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                ChildContentChannelIds = new ContentChannelService( rockContext )
                                    .GetChildContentChannels( Id )
                                    .Select( g => g.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( ChildContentChannelIds == null ) return childContentChannels;

                foreach ( var id in ChildContentChannelIds )
                {
                    var contentChannel = Get( id );
                    if ( contentChannel != null )
                    {
                        childContentChannels.Add( contentChannel );
                    }
                }

                return childContentChannels;
            }
        }

        /// <summary>
        /// Gets or sets the parent content channel ids.
        /// </summary>
        /// <value>
        /// The parent content channel ids.
        /// </value>
        [DataMember]
        public List<int> ParentContentChannelIds { get; private set; }

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

                if ( ParentContentChannelIds == null )
                {
                    lock ( _obj )
                    {
                        if ( ParentContentChannelIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                ParentContentChannelIds = new ContentChannelService( rockContext )
                                    .GetParentContentChannels( Id )
                                    .Select( g => g.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( ParentContentChannelIds == null ) return parentContentChannels;

                foreach ( var id in ParentContentChannelIds )
                {
                    var contentChannel = Get( id );
                    if ( contentChannel != null )
                    {
                        parentContentChannels.Add( contentChannel );
                    }
                }

                return parentContentChannels;
            }

        }

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
                    var contentChannelType = new ContentChannelTypeService( rockContext ).Get( ContentChannelTypeId );
                    return contentChannelType ?? base.ParentAuthority;
                }

            }
        }

        /// <summary>
        /// Gets the content channel type cache.
        /// </summary>
        /// <value>
        /// The type of the content channel.
        /// </value>
        public ContentChannelTypeCache ContentChannelType => ContentChannelTypeCache.Get( ContentChannelTypeId );

        /// <summary>
        /// Gets the content library configuration json.
        /// </summary>
        /// <value>
        /// The content library configuration json.
        /// </value>
        [DataMember]
        public string ContentLibraryConfigurationJson
        {
            get
            {
                return ContentLibraryConfiguration?.ToJson();
            }

            set
            {
                ContentLibraryConfiguration = value.FromJsonOrNull<ContentLibraryConfiguration>() ?? new ContentLibraryConfiguration();
            }
        }

        /// <summary>
        /// Gets the content library configuration.
        /// </summary>
        /// <value>
        /// The content library configuration.
        /// </value>
        public ContentLibraryConfiguration ContentLibraryConfiguration { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var contentChannel = entity as ContentChannel;
            if ( contentChannel == null ) return;

            ContentChannelTypeId = contentChannel.ContentChannelTypeId;
            Name = contentChannel.Name;
            Description = contentChannel.Description;
            IconCssClass = contentChannel.IconCssClass;
            RequiresApproval = contentChannel.RequiresApproval;
            ItemsManuallyOrdered = contentChannel.ItemsManuallyOrdered;
            ChildItemsManuallyOrdered = contentChannel.ChildItemsManuallyOrdered;
            EnableRss = contentChannel.EnableRss;
            ChannelUrl = contentChannel.ChannelUrl;
            ItemUrl = contentChannel.ItemUrl;
            TimeToLive = contentChannel.TimeToLive;
            ContentControlType = contentChannel.ContentControlType;
            RootImageDirectory = contentChannel.RootImageDirectory;
            IsIndexEnabled = contentChannel.IsIndexEnabled;
            EnablePersonalization = contentChannel.EnablePersonalization;
            CategoryIds = contentChannel.Categories.Select( c => c.Id ).ToList();
            ContentLibraryConfigurationJson = contentChannel.ContentLibraryConfigurationJson;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}