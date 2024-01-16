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
using System.Diagnostics;
using System.ComponentModel.Composition.Primitives;

namespace Rock.Lava.Helpers
{
    internal static class LavaAppendWatchesHelper
    {

        /// <summary>
        /// Appends the media watch info for a single entity.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="attributeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForEntity( IEntity source, string attributeKey, DateTime? startDate, Person currentPerson, RockContext rockContext )
        {
            // Convert entity to dynamic
            dynamic resultDataObject = new RockDynamic( source );
            resultDataObject.EntityTypeId = source.TypeId;
            int? mediaId = null;

            if ( source is MediaElement mediaElement )
            {
                resultDataObject.MediaId = mediaElement.Id;
                mediaId = mediaElement.Id;
                resultDataObject.MediaDefaultFileUrl = mediaElement.DefaultFileUrl;
                resultDataObject.MediaDefaultThumbnailUrl = mediaElement.DefaultThumbnailUrl;
                resultDataObject.MediaGuid = mediaElement.Guid;
            }
            else if ( source is IHasAttributes dataObjectAsIHasAttributes )
            {
                if ( dataObjectAsIHasAttributes.AttributeValues == null )
                {
                    dataObjectAsIHasAttributes.LoadAttributes();
                }

                // Get attribute value
                var mediaAttributeValue = dataObjectAsIHasAttributes.AttributeValues[attributeKey].Value?.AsGuid();

                // Query the database for meta data on the needed media element
                var mediaResult = new MediaElementService( rockContext ).Queryable()
                                        .Where( m => m.Guid == mediaAttributeValue )
                                        .Select( m => new
                                        {
                                            m.Id,
                                            m.Guid,
                                            m.DefaultFileUrl,
                                            m.DefaultThumbnailUrl
                                        } )
                                        .FirstOrDefault();

                if ( mediaResult == null )
                {
                    return source;
                }

                mediaId = mediaResult.Id;
                resultDataObject.MediaGuid = mediaAttributeValue;
                resultDataObject.MediaId = mediaResult.Id;
                resultDataObject.MediaDefaultFileUrl = mediaResult.DefaultFileUrl;
                resultDataObject.MediaDefaultThumbnailUrl = mediaResult.DefaultThumbnailUrl;
            }

            if ( !mediaId.HasValue )
            {
                return resultDataObject;
            }

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( new List<int> { mediaId.Value }, startDate, currentPerson, rockContext );

            AppendWatchDataToDynamic( resultDataObject, mediaWatches );

            return resultDataObject;
        }

        /// <summary>
        /// Appends the media for a collection of entities.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="attributeKey"></param>
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForEntities( ICollection source, string attributeKey, DateTime? startDate, Person currentPerson, RockContext rockContext, int entityTypeId )
        {
            var entityType = EntityTypeCache.Get( entityTypeId );
            List<Guid> mediaGuidList = new List<Guid>();

            // Convert the list into a dynamic
            var resultDataObject = new List<RockDynamic>();

            var stopwatch = Stopwatch.StartNew();
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
                        mediaGuidList.Add( mediaAttributeValue.AsGuid() );
                    }

                    // Append the media guid to the dynamic item so we can match it later
                    rockDynamicItem.MediaGuid = mediaAttributeValue.AsGuid();
                }
            }

            // Query the database for meta data on the needed media elements
            var mediaResults = new MediaElementService( rockContext ).Queryable()
                                    .Where( m => mediaGuidList.Contains( m.Guid ) )
                                    .Select( m => new
                                    {
                                        m.Id,
                                        m.Guid,
                                        m.DefaultFileUrl,
                                        m.DefaultThumbnailUrl
                                    } )
                                    .ToList();

            // Append media id for matching later
            foreach ( dynamic item in resultDataObject )
            {
                // Get the matching media result
                var mediaItem = mediaResults.Where( m => m.Guid == item.MediaGuid ).FirstOrDefault();

                if ( mediaItem == null )
                {
                    item.MediaId = null;
                    continue;
                }

                item.MediaId = mediaItem.Id;
                item.MediaDefaultFileUrl = mediaItem.DefaultFileUrl;
                item.MediaDefaultThumbnailUrl = mediaItem.DefaultThumbnailUrl;
            }

            // Get list of media ids
            var mediaIds = mediaResults.Select( m => m.Id ).ToList();

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( mediaIds, startDate, currentPerson, rockContext );

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
        /// <param name="source">The source.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        internal static dynamic AppendMediaForMediaElements( ICollection source, DateTime? startDate, Person currentPerson, RockContext rockContext )
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
            var mediaWatches = GetWatchInteractions( mediaIds, startDate, currentPerson, rockContext );

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
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForExpandoCollection( List<ExpandoObject> source, DateTime? startDate, Person currentPerson, RockContext rockContext )
        {
            // Get list of media ids
            var mediaIds = new List<int>();

            foreach ( dynamic item in source )
            {
                var mediaId = ( int? ) ConvertDynamicToInt( item.MediaId );

                if ( mediaId.HasValue )
                {
                    // Replace the current media id with the new value in case the original one was a string (just trying to make this more bullet proof)
                    item.MediaId = mediaId.Value;

                    mediaIds.Add( mediaId.Value );
                }
                else
                {
                    item.MediaId = null;
                }
            }

            // Get watches for the items
            var mediaWatches = GetWatchInteractions( mediaIds, startDate, currentPerson, rockContext );

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
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForExpando( dynamic source, DateTime? startDate, Person currentPerson, RockContext rockContext )
        {
            var mediaId = ( int? ) ConvertDynamicToInt( source.MediaId );

            if ( mediaId.HasValue )
            {
                // Replace the current media id with the new value in case the original one was a string (just trying to make this more bullet proof)
                source.MediaId = mediaId.Value;

                // Get watches for the items
                var mediaWatches = GetWatchInteractions( new List<int> { mediaId.Value }, startDate, currentPerson, rockContext );

                AppendWatchDataToDynamic( source, mediaWatches );
            }
            else
            {
                source.MediaId = null;
            }

            return source;
        }

        /// <summary>
        /// Appends watch data for a dictionary. The value of each dictionary item needs to have a property of MediaId.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        internal static dynamic AppendMediaForDictionary( IDictionary<string, object> source, DateTime? startDate, Person currentPerson, RockContext rockContext )
        {
            // Get list of media ids
            var mediaIds = new List<int>();

            // Cast dynamic as Dictionary
            IDictionary<string, object> sourceDictionary = source;

            // Harvest Media Ids
            foreach( var media in sourceDictionary )
            {
                if ( media.Value is IDictionary<string, object> mediaObject )
                {
                    if ( !mediaObject.ContainsKey( "MediaId" ) )
                    {
                        continue;
                    }

                    var mediaId = ( int? ) ConvertDynamicToInt( mediaObject["MediaId"] );

                    if ( mediaId.HasValue )
                    {
                        mediaIds.Add( mediaId.Value );
                    }
                }
            }

            // Get watch data
            var mediaWatches = GetWatchInteractions( mediaIds, startDate, currentPerson, rockContext );

            // Append the watch data to the dynamic object
            foreach ( var media in sourceDictionary )
            {
                if ( media.Value is IDictionary<string, object> mediaObject )
                {
                    AppendWatchDataToDictionary( mediaObject, mediaWatches );
                }
            }

            return source;
        }

        /// <summary>
        /// Determines if the dynamic object has a specified key.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool DynamicContainsKey( dynamic source, string key )
        {
            if ( ( ( IDictionary<String, object> ) source ).ContainsKey( key ) )
            {
                return true;
            }

            return false;
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
                        .OrderByDescending( i => i.InteractionDateTime )
                        .FirstOrDefault();

            if ( watch == null )
            {
                media.HasWatched = false;
                return;
            }

            media.WatchLength = watch.InteractionLength;
            media.HasWatched = true;
            media.WatchInteractionGuid = watch.InteractionGuid;
            media.WatchInteractionDateTime = watch.InteractionDateTime;

            try
            {
                var watchData = ( dynamic ) watch.InteractionData.FromJsonDynamic();
                media.WatchMap = watchData.WatchMap;
            }
            catch ( Exception ) { }

            return;
        }

        /// <summary>
        /// Appends the watch data to a dictionary media item.
        /// </summary>
        /// <param name="media"></param>
        /// <param name="mediaWatches"></param>
        private static void AppendWatchDataToDictionary( IDictionary<string, object> media, List<WatchInteractionSummary> mediaWatches )
        {
            // Return if the dictionary item does not have a MediaId
            if ( !media.ContainsKey( "MediaId" ) )
            {
                return;
            }

            // Get relevant watch
            var watch = mediaWatches.Where( w => w.MediaId == media["MediaId"].ToString().AsInteger() )
                    .OrderByDescending( i => i.InteractionDateTime )
                    .FirstOrDefault();

            // If no watch
            if ( watch == null )
            {
                media.AddOrReplace( "HasWatched", false );
                return;
            }

            media.AddOrReplace( "WatchLength", watch.InteractionLength );
            media.AddOrReplace( "HasWatched", true );
            media.AddOrReplace( "WatchInteractionGuid", watch.InteractionGuid );
            media.AddOrReplace( "WatchInteractionDateTime", watch.InteractionDateTime );

            try
            {
                var watchData = ( dynamic ) watch.InteractionData.FromJsonDynamic();
                media.AddOrReplace( "WatchMap", (object) watchData.WatchMap );
            }
            catch ( Exception ) { }

            return;
        }


        /// <summary>
        /// Gets media watch information for a given set of media ids.
        /// </summary>
        /// <param name="mediaIds"></param>
        /// <param name="startDate"></param>
        /// <param name="currentPerson"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private static List<WatchInteractionSummary> GetWatchInteractions( List<int> mediaIds, DateTime? startDate, Person currentPerson, RockContext rockContext )
        {
            if ( currentPerson == null )
            {
                return new List<WatchInteractionSummary>();
            }

            // Get watches for the items
            var mediaEventsInteractionChannelId = InteractionChannelCache.Get( SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() ).Id;

            var mediaWatch = new InteractionService( rockContext ).Queryable()
                .Where( i =>
                    i.PersonAlias.PersonId == currentPerson.Id
                    && i.InteractionComponent.InteractionChannelId == mediaEventsInteractionChannelId
                    && mediaIds.Contains( i.InteractionComponent.EntityId.Value )
                );

            // Add start date if one was provided.
            if ( startDate.HasValue )
            {
                var startDateKey = startDate.ToDateKey();
                mediaWatch = mediaWatch.Where( i => i.InteractionDateKey >= startDateKey );
            }

            return mediaWatch
                .Select( i => new WatchInteractionSummary
                {
                    InteractionLength = i.InteractionLength,
                    MediaId = i.InteractionComponent.EntityId,
                    InteractionData = i.InteractionData,
                    InteractionGuid = i.Guid,
                    InteractionDateTime = i.InteractionDateTime
                } )
                .ToList();
        }

        /// <summary>
        /// Helper method to convert dynamic objects to integers.
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

            if ( item is string )
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
            /// Interaction Guid
            /// </summary>
            public Guid? InteractionGuid { get; set; }

            /// <summary>
            /// Watch Data
            /// </summary>
            public string InteractionData { get; set; }

            /// <summary>
            /// Interaction Length
            /// </summary>
            public double? InteractionLength { get; set; }

            /// <summary>
            /// The date of the interaction
            /// </summary>
            public DateTime InteractionDateTime { get; set; }
        }
    }
}
