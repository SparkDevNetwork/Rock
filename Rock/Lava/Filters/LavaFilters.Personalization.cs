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
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Lava
{
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Appends Personalization Segment information to entity/entities or a data object created from <see cref="PersistedDataset(ILavaRenderContext, string, string)" />.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="dataObject">An entity, entity collection, or persisted dataset to which the personalization segment information will be added.</param>
        /// <param name="segmentFilter">The purpose key.</param>
        /// <returns>
        /// A copy of the input dataObject where each element includes the following new properties:
        /// * IsInSegment - a flag indicating if the entity is assigned to a segment that matches the current person.
        /// * MatchingSegments - a comma-delimited list of all segments assigned to the entity and matched for the current person.
        /// </returns>
        public static object AppendSegments( ILavaRenderContext context, object dataObject, string segmentFilter = null )
        {
            var items = new List<PersonalizationItemInfo>();
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
            var person = GetCurrentPerson( context );
            var personalizationTypes = segmentFilter.SplitDelimitedValues( "," )
                .Select( x => x.ToStringSafe().Trim().ToLower() )
                .Where( x => x != string.Empty )
                .ToList();

            // Add the Personalization Segments if we have a specified person.
            List<int> personSegmentIdList;
            dynamic resultDataObject;

            personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie( context, System.Web.HttpContext.Current, person );

            var isValidObject = TryGetDynamicOutputObjectFromInput( dataObject, out resultDataObject );
            if ( !isValidObject )
            {
                return dataObject;
            }

            List<int> entityIdList;
            int? dataObjectEntityTypeId;

            TryGetEntityIdList( dataObject, out entityIdList, out dataObjectEntityTypeId );

            // Append the Segment information to the input entity or entities.
            Dictionary<int, string> personalizedEntities;

            if ( person != null )
            {
                var segmentService = new PersonalizationSegmentService( rockContext );
                var segmentQuery = segmentService.Queryable()
                    .Where( s => personSegmentIdList.Contains( s.Id ) );

                if ( segmentFilter.IsNotNullOrWhiteSpace() )
                {
                    segmentQuery = segmentQuery.Where( s => s.Name == segmentFilter );
                }

                // Retrieve the personalization segments, grouped by entity.
                var personalizedEntityItems = rockContext.Set<PersonalizedEntity>()
                    .Where( a => a.PersonalizationType == PersonalizationType.Segment
                        && a.EntityTypeId == dataObjectEntityTypeId
                        && entityIdList.Contains( a.EntityId ) )
                    .Join( segmentQuery,
                        e => e.PersonalizationEntityId,
                        s => s.Id,
                        ( e, s ) => new { EntityId = e.EntityId, SegmentName = s.Name } )
                    .GroupBy( k => k.EntityId )
                    .ToDictionary( k => k.Key, v => v.Select( x => x.SegmentName ).OrderBy( s => s ).ToList() );

                // Format the segments for each entity as a delimited string.
                personalizedEntities = personalizedEntityItems.ToDictionary( k => k.Key, v => v.Value.AsDelimited( ", " ) );
            }
            else
            {
                personalizedEntities = new Dictionary<int, string>();
            }

            // Append the new segment properties to the output.
            var isCollection = IsCollection( dataObject );
            if ( isCollection )
            {
                foreach ( dynamic resultElement in ( IEnumerable ) resultDataObject )
                {
                    var entityId = ( int? ) resultElement.Id;

                    if ( entityId.HasValue )
                    {
                        var isInSegment = personalizedEntities.ContainsKey( entityId.Value );
                        resultElement.IsInSegment = isInSegment;

                        resultElement.MatchingSegments = isInSegment ? personalizedEntities[entityId.Value] : string.Empty;
                    }
                }
            }
            else
            {
                var entityId = ( int? ) resultDataObject.Id;
                if ( entityId.HasValue )
                {
                    var isInSegment = personalizedEntities.ContainsKey( entityId.Value );
                    resultDataObject.IsInSegment = isInSegment;

                    resultDataObject.MatchingSegments = isInSegment ? personalizedEntities[entityId.Value] : string.Empty;
                }
            }

            return resultDataObject;
        }

        private static bool IsCollection( object dataObject )
        {
            var isCollection = false;
            if ( dataObject is IEnumerable collection )
            {
                // Note: Since a single ExpandoObject actually is an IEnumerable (of fields), we'll have to see if this is an IEnumerable of ExpandoObjects
                // to see if we should treat it as a collection.
                var enumerator = collection.GetEnumerator();

                var firstItem = enumerator.MoveNext() ? enumerator.Current : null;
                if ( firstItem != null )
                {
                    isCollection = true;
                }
            }

            return isCollection;
        }

        private static bool TryGetFirstCollectionItem( object dataObject, out dynamic firstItem )
        {
            if ( dataObject is string )
            {
                // String types are recognized as a 'char' collection, but that is not our purpose here.
            }
            else if ( dataObject is IEnumerable collection )
            {
                var enumerator = collection.GetEnumerator();

                firstItem = enumerator.MoveNext() ? enumerator.Current : null;

                return firstItem != null;
            }

            firstItem = null;
            return false;
        }

        private static bool TryGetEntityIdList( dynamic dataObject, out List<int> entityIdList, out int? dataObjectEntityTypeId )
        {
            dynamic firstItem;

            if ( dataObject is IEntity dataObjectAsEntity )
            {
                dataObjectEntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
                entityIdList = new List<int> { dataObjectAsEntity.Id };
                return true;
            }

            TryGetFirstCollectionItem( dataObject, out firstItem );

            var isEntityCollection = firstItem is IEntity;
            if ( isEntityCollection )
            {
                dataObjectEntityTypeId = EntityTypeCache.GetId( firstItem.GetType() );

                var dataObjectAsEntityList = ( ( IEnumerable ) dataObject ).Cast<IEntity>();

                entityIdList = dataObjectAsEntityList.Select( a => a.Id ).ToList();
                return true;
            }

            try
            {
                // if the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset
                var isCollection = IsCollection( dataObject );
                if ( isCollection )
                {
                    IEnumerable<dynamic> dataObjectAsCollection = dataObject as IEnumerable<dynamic>;

                    entityIdList = dataObjectAsCollection
                            .Select( x => ( int? ) x.Id )
                            .Where( e => e.HasValue )
                            .Select( e => e.Value ).ToList();

                    // the dataObjects will each have the same EntityTypeId (assuming they are from a persisted dataset, so we can determine EntityTypeId from the first one
                    dataObjectEntityTypeId = dataObjectAsCollection.Select( a => ( int? ) a.EntityTypeId ).FirstOrDefault();
                }
                else
                {
                    var dataObjectAsEntityProxy = dataObject as dynamic;
                    int? entityId = ( int? ) dataObjectAsEntityProxy.Id;
                    dataObjectEntityTypeId = ( int? ) dataObjectAsEntityProxy.EntityTypeId;
                    entityIdList = new List<int>();
                    if ( entityId.HasValue )
                    {
                        entityIdList.Add( entityId.Value );
                    }
                }
                return true;
            }
            catch
            {
                dataObjectEntityTypeId = null;
                entityIdList = new List<int>();
                return false;
            }
        }

        private static bool TryGetDynamicOutputObjectFromInput( object dataObject, out dynamic resultDataObject )
        {
            resultDataObject = dataObject;

            if ( dataObject == null )
            {
                return false;
            }

            // Determine if the dataObject parameter is a collection or a single object.
            var dataObjectType = dataObject.GetType();

            int? dataObjectEntityTypeId = null;

            if ( dataObject is IEntity entity )
            {
                // The input object is a single Entity.
                resultDataObject = new RockDynamic( dataObject );
                resultDataObject.EntityTypeId = EntityTypeCache.GetId( dataObject.GetType() );
            }
            else if ( dataObject is ExpandoObject xo )
            {
                resultDataObject = ( resultDataObject as ExpandoObject )?.ShallowCopy() ?? resultDataObject;
            }
            else
            {
                dynamic firstItem;
                TryGetFirstCollectionItem( dataObject, out firstItem );

                if ( firstItem == null )
                {
                    return false;
                }

                if ( firstItem is IEntity firstEntity )
                {
                    dataObjectEntityTypeId = EntityTypeCache.GetId( firstEntity.GetType() );

                    var dynamicEntityList = new List<RockDynamic>();

                    var collection = dataObject as IEnumerable;
                    foreach ( var item in collection )
                    {
                        dynamic rockDynamicItem = new RockDynamic( item );
                        rockDynamicItem.EntityTypeId = EntityTypeCache.GetId( item.GetType() );
                        dynamicEntityList.Add( rockDynamicItem );
                    }

                    resultDataObject = dynamicEntityList;
                    return true;
                }
                else if ( firstItem is ExpandoObject )
                {
                    // If the dataObject is neither a single IEntity or a list if IEntity, it is probably from a PersistedDataset.
                    var collection = dataObject as IEnumerable;
                    var expandoCollection = collection.Cast<ExpandoObject>();

                    resultDataObject = expandoCollection.Select( a => a.ShallowCopy() ).ToList();
                    return true;
                }
                else
                {
                    // if we are dealing with a persisted dataset, make a copy of the objects so we don't accidently modify the cached object
                    resultDataObject = ( resultDataObject as IEnumerable<ExpandoObject> ).Select( a => a.ShallowCopy() ).ToList();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the set of personalization items that are relevant to the specified person.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to a Person or a Person object.</param>
        /// <param name="itemTypeList">A comma-delimited list of item types to return.</param>
        /// <returns>The value of the user preference.</returns>
        public static List<PersonalizationItemInfo> PersonalizationItems( ILavaRenderContext context, object input, string itemTypeList )
        {
            var items = new List<PersonalizationItemInfo>();
            var person = LavaHelper.GetPersonFromInputParameter( input, context );

            var personalizationTypes = itemTypeList.SplitDelimitedValues( "," )
                .Select( x => x.ToStringSafe().Trim().ToLower() )
                .Where( x => x != string.Empty )
                .ToList();

            // Add the Personalization Segments if we have a specified person.
            if ( person != null )
            {
                if ( !personalizationTypes.Any() || personalizationTypes.Contains( "segments" ) )
                {
                    var rockContext = LavaHelper.GetRockContextFromLavaContext( context );
                    var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie( context, System.Web.HttpContext.Current, person );

                    var segments = PersonalizationSegmentCache.All()
                        .Where( ps => personSegmentIdList.Contains( ps.Id ) )
                        .Select( ps => new PersonalizationItemInfo
                        {
                            Type = PersonalizationType.Segment,
                            Id = ps.Id,
                            Key = ps.SegmentKey,
                            Name = ps.Name
                        } );
                    items.AddRange( segments );
                }
            }

            // Get the Request Filters that are relevant to the current request.
            if ( !personalizationTypes.Any() || personalizationTypes.Contains( "requestfilters" ) )
            {
                var requestFilterIdList = LavaPersonalizationHelper.GetPersonalizationRequestFilterIdList();

                var filters = RequestFilterCache.All()
                    .Where( rf => requestFilterIdList.Contains( rf.Id ) )
                    .Select( rf => new PersonalizationItemInfo
                    {
                        Type = PersonalizationType.RequestFilter,
                        Id = rf.Id,
                        Key = rf.RequestFilterKey,
                        Name = rf.Name
                    } );
                items.AddRange( filters );
            }

            // Return an ordered list of results.
            items = items.OrderBy( i => i.Type ).ThenBy( i => i.Key ).ToList();
            return items;
        }

        /// <summary>
        /// Temporarily adds one or more personalization segments for the specified person.
        /// </summary>
        /// <remarks>
        /// If executed in the context of a HttpRequest, the result is stored in a session cookie and applies until the cookie expires.
        /// If no HttpRequest is active, the result is stored in the Lava context and applies only for the current render operation.
        /// </remarks>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, a reference to a Person or a Person object.</param>
        /// <param name="segmentKeyList">A comma-delimited list of segment keys to add.</param>
        public static void AddSegment( ILavaRenderContext context, object input, string segmentKeyList )
        {
            var items = new List<PersonalizationItemInfo>();
            var person = LavaHelper.GetPersonFromInputParameter( input, context );

            if ( person == null )
            {
                return;
            }

            var segmentKeys = segmentKeyList.SplitDelimitedValues( "," )
                .Select( x => x.ToStringSafe().Trim().ToLower() )
                .Where( x => x != string.Empty )
                .ToList();

            if ( !segmentKeys.Any() )
            {
                return;
            }

            // Map the segment names to identifiers.
            var newSegmentIdList = PersonalizationSegmentCache.GetActiveSegments()
                .Where( s => s.SegmentKey != null && segmentKeys.Contains( s.SegmentKey.ToLower() ) )
                .Select( s => s.Id )
                .ToList();

            if ( !newSegmentIdList.Any() )
            {
                return;
            }

            // Try to get the current segment list from the Lava context.
            // The scope of the context is only a single template render, so if the segment list exists we can reuse it.
            var httpContext = System.Web.HttpContext.Current;
            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            var key = $"PersonalizationSegmentIdList_{ person.Guid }";
            var personSegmentIdList = LavaPersonalizationHelper.GetPersonalizationSegmentIdListForPersonFromContextCookie( context, httpContext, person );
            var addSegmentIdList = newSegmentIdList.Except( personSegmentIdList ).ToList();

            // Add the new Personalization Segments.
            if ( addSegmentIdList.Any() )
            {
                personSegmentIdList.AddRange( addSegmentIdList );

                var segmentService = new PersonalizationSegmentService( rockContext );
                segmentService.AddSegmentsForPerson( person.Id, addSegmentIdList );

                rockContext.SaveChanges();

                // If this is a HttpRequest, set the personalization cookie in the response.
                if ( httpContext != null )
                {
                    LavaPersonalizationHelper.SetPersonalizationSegmentsForContext( personSegmentIdList, context, httpContext, person );
                }
            }
        }
    }

    /// <summary>
    /// A Lava data object that represents a Personalization Item.
    /// </summary>
    public class PersonalizationItemInfo : RockDynamic
    {
        /// <summary>
        /// The unique identifier of the personalization item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique key by which the personalization item is referenced in Lava.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The type of personalization item.
        /// For example, a Personalization Segment or a Request Filter.
        /// </summary>
        public PersonalizationType Type { get; set; }

        /// <summary>
        /// The friendly name of the personalization item.
        /// </summary>
        public string Name { get; set; }
    }
}
