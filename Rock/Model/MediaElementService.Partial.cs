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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.MediaElement"/> entities.
    /// </summary>
    public partial class MediaElementService
    {
        /// <summary>
        /// Triggers all the post-save tasks for a <see cref="MediaElement"/>
        /// that need to happen in a background task. This will load the
        /// <see cref="MediaElement"/> from the database to determine which
        /// specific tasks need to be executed.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        internal static void TriggerPostSaveTasks( int mediaElementId )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaElement = new MediaElementService( rockContext ).Queryable()
                    .Include( m => m.MediaFolder )
                    .Include( m => m.MediaFolder.ContentChannel )
                    .AsNoTracking()
                    .Where( m => m.Id == mediaElementId )
                    .SingleOrDefault();

                AddSyncedContentChannelItem( mediaElement, rockContext );

                if ( mediaElement.MediaFolder.WorkflowTypeId.HasValue )
                {
                    mediaElement.LaunchWorkflow( mediaElement.MediaFolder.WorkflowTypeId );
                }
            }
        }

        /// <summary>
        /// Adds the synced content channel item for the media element by
        /// the given identifier.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        public static void AddSyncedContentChannelItem( int mediaElementId )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaElement = new MediaElementService( rockContext ).Queryable()
                    .Include( m => m.MediaFolder )
                    .Include( m => m.MediaFolder.ContentChannel )
                    .AsNoTracking()
                    .Where( m => m.Id == mediaElementId )
                    .SingleOrDefault();

                AddSyncedContentChannelItem( mediaElement, rockContext );
            }
        }

        /// <summary>
        /// Adds the synced content channel item for the media element by
        /// the given identifier.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="rockContext">The <see cref="RockContext"/> to perform the save in.</param>
        /// <remarks>Must be called on a MediaElement that has a valid <see cref="MediaElement.MediaFolder"/> property.</remarks>
        private static void AddSyncedContentChannelItem( MediaElement mediaElement, RockContext rockContext )
        {
            try
            {
                // Freak accident, either we are in a transaction or the
                // media element got deleted before we started executing.
                if ( mediaElement == null || mediaElement.MediaFolder == null )
                {
                    return;
                }

                // No sync, just exit.
                if ( !mediaElement.MediaFolder.IsContentChannelSyncEnabled )
                {
                    return;
                }

                var contentChannelTypeId = mediaElement.MediaFolder.ContentChannel?.ContentChannelTypeId;
                var contentChannelId = mediaElement.MediaFolder.ContentChannelId;
                var attributeId = mediaElement.MediaFolder.ContentChannelAttributeId;
                var status = mediaElement.MediaFolder.ContentChannelItemStatus;

                // Missing required information.
                if ( !contentChannelTypeId.HasValue || !contentChannelId.HasValue || !attributeId.HasValue || !status.HasValue )
                {
                    return;
                }

                var attribute = AttributeCache.Get( attributeId.Value );

                // Shouldn't happen, but make sure we don't hit a NRE later.
                if ( attribute == null )
                {
                    return;
                }

                var contentChannelItemService = new ContentChannelItemService( rockContext );
                var contentChannelItem = new ContentChannelItem();
                contentChannelItemService.Add( contentChannelItem );

                contentChannelItem.ContentChannelId = contentChannelId.Value;
                contentChannelItem.ContentChannelTypeId = contentChannelTypeId.Value;
                contentChannelItem.Title = mediaElement.Name;
                contentChannelItem.Content = mediaElement.Description;
                contentChannelItem.StartDateTime = mediaElement.SourceCreatedDateTime ?? mediaElement.CreatedDateTime ?? RockDateTime.Now;
                contentChannelItem.Status = status.Value;

                // We want both the content channel item and it's attributes
                // to be saved either together or not at all.
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    // 2.5x faster than LoadAttributes/SaveAttributeValues.
                    Rock.Attribute.Helper.SaveAttributeValue( contentChannelItem, attribute, mediaElement.Guid.ToString(), rockContext );
                } );
            }
            catch ( Exception ex )
            {
                var exception = new Exception( "Error while creating synced content channel item for media element.", ex );
                ExceptionLogService.LogException( exception );
            }
        }
    }
}
