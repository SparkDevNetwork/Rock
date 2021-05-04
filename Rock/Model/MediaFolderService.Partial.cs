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
using Rock.Web.Cache;

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
        /// </summary>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        public static void AddMissingSyncedContentChannelItems( int mediaFolderId )
        {
            List<int> mediaElementIds;

            using ( var rockContext = new RockContext() )
            {
                var mediaFolder = new MediaFolderService( rockContext ).Get( mediaFolderId );

                // If the folder wasn't found or syncing isn't enabled then exit early.
                if ( mediaFolder == null || !mediaFolder.IsContentChannelSyncEnabled )
                {
                    return;
                }

                var contentChannelId = mediaFolder.ContentChannelId;
                var attributeId = mediaFolder.ContentChannelAttributeId;

                // If we don't have required information then exit early.
                if ( !contentChannelId.HasValue || !attributeId.HasValue )
                {
                    return;
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
                mediaElementIds = new MediaElementService( rockContext ).Queryable()
                    .Where( e => e.MediaFolderId == mediaFolderId
                        && !syncedMediaElementValues.Contains( e.Guid.ToString() ) )
                    .Select( e => e.Id )
                    .ToList();
            }

            // Add the content channel item for each media element.
            foreach ( var mediaElementId in mediaElementIds )
            {
                MediaElementService.AddSyncedContentChannelItem( mediaElementId );
            }
        }
    }
}
