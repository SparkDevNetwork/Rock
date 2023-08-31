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

using AngleSharp.Dom;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Rock;

namespace Rock.Lava.Helpers
{
    internal static class LavaAppendWatchesHelper
    {

        /// <summary>
        /// Appends the media watch info for a single entity.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="attributeKey"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForEntity( IEntity source, string attributeKey, Person currentPerson, RockContext rockContext )
        {
            // Convert entity to dynamic
            dynamic resultDataObject = new RockDynamic( source );
            resultDataObject.EntityTypeId = source.TypeId;
            int? mediaId = null;

            if ( source is MediaElement mediaElement )
            {
                resultDataObject.MediaId = mediaElement.Id;
                mediaId = mediaElement.Id;
            }
            else if ( source is IHasAttributes dataObjectAsIHasAttributes )
            {
                if ( dataObjectAsIHasAttributes.AttributeValues == null )
                {
                    dataObjectAsIHasAttributes.LoadAttributes();
                }

                // Get attribute value
                var mediaAttributeValue = dataObjectAsIHasAttributes.AttributeValues[attributeKey].Value?.AsGuid();

                var mediaEntityType = EntityTypeCache.Get( SystemGuid.EntityType.MEDIA_ELEMENT.AsGuid() );
                mediaId = Reflection.GetEntityIdForEntityType( mediaEntityType.Id, mediaAttributeValue.Value );

                resultDataObject.MediaId = mediaId;
            }

            if ( !mediaId.HasValue )
            {
                return resultDataObject;
            }

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( new List<int> { mediaId.Value }, currentPerson, rockContext );

            AppendWatchDataToDynamic( resultDataObject, mediaWatches );

            return resultDataObject;
        }

        /// <summary>
        /// Appends the media for a collection of entities.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="attributeKey"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForEntities( ICollection source, string attributeKey, Person currentPerson, RockContext rockContext, int entityTypeId )
        {
            var entityType = EntityTypeCache.Get( entityTypeId );
            List<string> mediaGuidList = new List<string>();

            // Convert the list into a dynamic
            var resultDataObject = new List<RockDynamic>();

            foreach ( var item in source )
            {
                // Add to dynamic collection
                dynamic rockDynamicItem = new RockDynamic( item );
                rockDynamicItem.EntityTypeId = entityTypeId;
                resultDataObject.Add( rockDynamicItem );

                // Get the media information
                if ( item is IHasAttributes itemEntity )
                {
                    // In most cases attributes will already have been loaded
                    if ( itemEntity.AttributeValues == null )
                    {
                        itemEntity.LoadAttributes();
                    }

                    var mediaAttributeValue = itemEntity.AttributeValues[attributeKey].Value;

                    if ( mediaAttributeValue.IsNotNullOrWhiteSpace() )
                    {
                        mediaGuidList.Add( mediaAttributeValue );
                    }

                    // Append the media guid to the dynamic item so we can match it later
                    rockDynamicItem.MediaGuid = mediaAttributeValue;
                }
            }

            var mediaEntityType = EntityTypeCache.Get( SystemGuid.EntityType.MEDIA_ELEMENT.AsGuid() );
            var mediaLookup = Reflection.GetEntityIdsForEntityType( mediaEntityType, mediaGuidList );

            // Append media id for matching later
            foreach ( dynamic item in resultDataObject )
            {
                item.MediaId = mediaLookup[item.MediaGuid];
            }

            // Get list of media ids
            var mediaIds = mediaLookup.Select( m => m.Value ).ToList();

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( mediaIds, currentPerson, rockContext );

            // Append the watches into the collection
            foreach ( dynamic media in resultDataObject )
            {
                AppendWatchDataToDynamic( media, mediaWatches );
            }

            return resultDataObject;
        }

        /// <summary>
        /// Appends media watches for a collection of media entities.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForMediaElements( ICollection source, Person currentPerson, RockContext rockContext )
        {
            var mediaIds = new List<int>();

            // Convert the list into a dynamic
            var resultDataObject = new List<RockDynamic>();

            foreach ( var item in source )
            {
                // Add to dynamic collection
                dynamic rockDynamicItem = new RockDynamic( item );
                resultDataObject.Add( rockDynamicItem );

                mediaIds.Add( rockDynamicItem.Id );
            }

            // Get watch data
            var mediaWatches = GetWatchInteractions( mediaIds, currentPerson, rockContext );

            // Append the watch data to the dynamic object
            foreach ( dynamic media in source )
            {
                AppendWatchDataToDynamic( media, mediaWatches );
            }
                
            return resultDataObject;
        }

        /// <summary>
        /// Appends watches to a list of expando objects
        /// </summary>
        /// <param name="source"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForExpandoCollection( List<ExpandoObject> source, Person currentPerson, RockContext rockContext )
        {
            // Get list of media ids
            var mediaIds = new List<int>();

            foreach( dynamic item in source )
            {
                var mediaId = (int?) ConvertDynamicToInt( item.MediaId );

                if ( mediaId.HasValue )
                {
                    // Replace the current media id with the new value in case the original one was a string (just trying to make this more bullet proof)
                    item.MediaId = mediaId.Value;

                    mediaIds.Add( mediaId.Value );
                }
            }

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( mediaIds, currentPerson, rockContext );

            // Append the watches into the collection
            foreach ( dynamic media in source )
            {
                AppendWatchDataToDynamic( media, mediaWatches );
            }

            return source;
        }

        /// <summary>
        /// Appends watch data for the expando. This object needs to have a property of MediaId.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForExpando( dynamic source, Person currentPerson, RockContext rockContext )
        {
            var mediaId = (int?) ConvertDynamicToInt ( source.MediaId );

            if ( mediaId.HasValue )
            {
                // Replace the current media id with the new value in case the original one was a string (just trying to make this more bullet proof)
                source.MediaId = mediaId.Value;

                // Get watches for the items
                var mediaWatches = GetWatchInteractions( new List<int> { mediaId.Value }, currentPerson, rockContext );

                AppendWatchDataToDynamic( source, mediaWatches );
            }

            return source;
        }

        /// <summary>
        /// Appends the watch data to a dynamic media item.
        /// </summary>
        /// <param name="media"></param>
        /// <param name="mediaWatches"></param>
        /// <returns></returns>
        private static void AppendWatchDataToDynamic( dynamic media, List<WatchInteractionSummary> mediaWatches )
        {
            // Get relevant watch
            var watch = mediaWatches.Where( w => w.MediaId == media.MediaId )
                        .OrderByDescending( i => i.InteractionLength )
                        .FirstOrDefault();

            if ( watch == null )
            {
                media.HasWatched = false;
                return;
            }

            media.WatchLength = watch.InteractionLength;
            media.HasWatched = true;

            try
            {
                var watchData = ( dynamic ) watch.InteractionData.FromJsonDynamic();
                media.WatchMap = watchData.WatchMap;
            }
            catch ( Exception ) { }

            return;
        }

        /// <summary>
        /// Gets media watch information for a given set of media ids.
        /// </summary>
        /// <param name="mediaIds"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private static List<WatchInteractionSummary> GetWatchInteractions( List<int> mediaIds, Person currentPerson, RockContext rockContext )
        {
            // Get watches for the items
            var personAliasQuery = new PersonAliasService( rockContext )
                    .Queryable()
                    .Where( a => a.PersonId == currentPerson.Id )
                    .Select( a => a.Id );

            var mediaEventsInteractionChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() ).Id;

            var mediaWatch = new InteractionService( rockContext ).Queryable()
                .Where( i =>
                    personAliasQuery.Contains( i.PersonAliasId.Value )
                    && i.InteractionComponent.InteractionChannelId == mediaEventsInteractionChannelId
                    && mediaIds.Contains( i.InteractionComponent.EntityId.Value )
                )
                .Select( i => new WatchInteractionSummary
                {
                    InteractionLength = i.InteractionLength,
                    MediaId = i.InteractionComponent.EntityId,
                    InteractionData = i.InteractionData
                } )
                .ToList();

            return mediaWatch;
        }

        /// <summary>
        /// Helper method to convert dynamic objects to ints.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int? ConvertDynamicToInt( dynamic item )
        {
            if ( item == null )
            {
                return null;
            }

            if ( item is int || item is long )
            {
                return ( ( long ) item ).ToIntSafe( 0 );
            }

            if (item is string )
            {
                return ( ( string ) item ).AsInteger();
            }

            return null;
        }

        /// <summary>
        /// POCO for getting interaction data back for watches.
        /// </summary>
        private class WatchInteractionSummary
        {
            /// <summary>
            /// Media Id
            /// </summary>
            public int? MediaId { get; set; }

            /// <summary>
            /// Watch Data
            /// </summary>
            public string InteractionData { get; set; }

            /// <summary>
            /// Interaction Length
            /// </summary>
            public double? InteractionLength { get; set; }
        }
    }
}
