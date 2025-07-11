﻿// <copyright>
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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.MediaFolder"/> entities.
    /// </summary>
    public partial class MediaFolderService
    {
        /// <summary>  
        /// Adds the missing synced content channel items for all <see cref="MediaElement"/>  
        /// items in the folder.  
        /// PA: Extracted the contents of this method to a new method  
        /// <see cref="M:Rock.Model.MediaFolderService.AddMissingSyncedContentChannelItem(System.Int32,System.Collections.Generic.List{System.Int32})"/>  
        /// to add the functionality to return the number of content channels updated while not affecting the existing functionality  
        /// </summary>  
        /// <param name="mediaFolderId">The media folder identifier.</param>  
        [Obsolete( "Use the AddMissingSyncedContentChannelItem that takes int and List<int>" )]
        [RockObsolete( "17.2" )]
        public static void AddMissingSyncedContentChannelItems( int mediaFolderId )
        {
            AddMissingSyncedContentChannelItem( mediaFolderId );
        }

        /// <summary>
        /// Adds the missing synced content channel items for all <see cref="MediaElement"/>
        /// items in the folder
        /// </summary>
        /// <param name="mediaFolderId"></param>
        /// <returns>Number of content channels that were updated.</returns>
        [Obsolete("Use the AddMissingSyncedContentChannelItem that takes int and List<int>")]
        [RockObsolete( "17.2" )]
        public static int AddMissingSyncedContentChannelItem( int mediaFolderId )
        {
            return AddMissingSyncedContentChannelItem( mediaFolderId, new List<int>() );
        }

        /// <summary>
        /// Adds the missing synced content channel items for all <see cref="MediaElement"/>
        /// items in the folder
        /// </summary>
        /// <param name="mediaFolderId"></param>
        /// <param name="newlyAddedMediaElementIds"></param>
        /// <returns>Number of content channels that were updated.</returns>
        public static int AddMissingSyncedContentChannelItem( int mediaFolderId, List<int> newlyAddedMediaElementIds )
        {
            List<int> mediaElementIds;
            int contentChannelsUpdatedCount;

            using ( var rockContext = new RockContext() )
            {
                var mediaFolder = new MediaFolderService( rockContext ).Get( mediaFolderId );

                // If the folder wasn't found or syncing isn't enabled then exit early.
                if ( mediaFolder == null || !mediaFolder.IsContentChannelSyncEnabled )
                {
                    return 0;
                }

                var contentChannelId = mediaFolder.ContentChannelId;
                var attributeId = mediaFolder.ContentChannelAttributeId;

                // If we don't have required information then exit early.
                if ( !contentChannelId.HasValue || !attributeId.HasValue )
                {
                    return 0;
                }

                // Build a query of all attribute values for the given attribute
                // that are not null or empty.
                var attributeValueQuery = new AttributeValueService( rockContext ).Queryable()
                    .Where( a => a.AttributeId == attributeId.Value && !string.IsNullOrEmpty( a.Value ) );

                // Build a query of all content channel items for the channel.
                var contentChannelItemQuery = new ContentChannelItemService( rockContext ).Queryable()
                    .Where( i => i.ContentChannelId == contentChannelId.Value );

                // Join the two so we end up just content channel items that
                // have a non-null and non-empty attribute value. Finally
                // select just the value so we get a list of MediaElement Guid
                // values that have already been synced.
                var syncedMediaElementValues = contentChannelItemQuery.Join( attributeValueQuery, i => i.Id, v => v.EntityId, ( i, v ) => v.Value );

                // Our final query is to get all MediaElements that do not
                // exist in the previous query. That gives us a final list
                // of items that need to be synced still.
                var mediaElements = new MediaElementService( rockContext ).Queryable()
                                    .Where( e => e.MediaFolderId == mediaFolderId
                                        && !syncedMediaElementValues.Contains( e.Guid.ToString() ) );

                mediaElementIds = mediaElements
                    .Select( e => e.Id )
                    .ToList()
                    .Where( id => !newlyAddedMediaElementIds.Contains( id ) )
                    .ToList();

                contentChannelsUpdatedCount = mediaElements
                    .Select( me => me.MediaFolder.ContentChannelId )
                    .Distinct()
                    .Count();
            }

            // Add the content channel item for each media element.
            foreach ( var mediaElementId in mediaElementIds )
            {
                MediaElementService.AddSyncedContentChannelItem( mediaElementId );
            }
            return contentChannelsUpdatedCount;
        }
    }
}
