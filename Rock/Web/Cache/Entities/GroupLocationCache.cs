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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a named group location. This is only intended for
    /// use with <see cref="Rock.Model.GroupLocation"/> objects with named
    /// locations.
    /// </summary>
    [Serializable]
    [DataContract]
    public class GroupLocationCache : ModelCache<GroupLocationCache, Rock.Model.GroupLocation>
    {
        #region Fields

        /// <summary>
        /// <c>true</c> if this is for a named location.
        /// </summary>
        private bool _isNamedLocation;

        /// <summary>
        /// Tracks the cached "all item ids" lists per location.
        /// </summary>
        private static readonly AlternateIdListCache<GroupLocationCache, int> _byLocationIdCache = new AlternateIdListCache<GroupLocationCache, int>( "location" );

        #endregion

        #region Properties

        /// <inheritdoc/>
        public override TimeSpan? Lifespan
        {
            // If this isn't for a named location, use a short lifetime of 10 minutes.
            get => _isNamedLocation ? base.Lifespan : new TimeSpan( 0, 10, 0 );
        }

        /// <inheritdoc cref="Rock.Model.GroupLocation.GroupId"/>
        [DataMember]
        public int GroupId { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.LocationId"/>
        [DataMember]
        public int LocationId { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.GroupLocationTypeValueId"/>
        [DataMember]
        public int? GroupLocationTypeValueId { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.IsMailingLocation"/>
        [DataMember]
        public bool IsMailingLocation { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.IsMappedLocation"/>
        [DataMember]
        public bool IsMappedLocation { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.GroupMemberPersonAliasId"/>
        [DataMember]
        public int? GroupMemberPersonAliasId { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.Order"/>
        [DataMember( IsRequired = true )]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Schedule" />
        /// identifiers that are associated with this instance.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Schedule"/> identifiers.
        /// </value>
        public List<int> ScheduleIds { get; private set; }

        /// <inheritdoc cref="Rock.Model.GroupLocation.Location" />
        public NamedLocationCache Location => NamedLocationCache.Get( LocationId );

        /// <inheritdoc cref="Rock.Model.GroupLocation.GroupLocationTypeValue" />
        public DefinedValueCache GroupLocationTypeValue => GroupLocationTypeValueId.HasValue ? DefinedValueCache.Get( GroupLocationTypeValueId.Value ) : null;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="NamedScheduleCache" />
        /// objects that are associated with this instance.
        /// </summary>
        /// <value>
        /// A collection of <see cref="NamedScheduleCache"/> objects.
        /// </value>
        public List<NamedScheduleCache> Schedules => ScheduleIds.Select( NamedScheduleCache.Get ).Where( s => s != null ).ToList();

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets all cache objects for the specified location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A list of <see cref="GroupLocationCache"/> objects.</returns>
        public static List<GroupLocationCache> AllForLocationId( int locationId, RockContext rockContext = null )
        {
            var keys = _byLocationIdCache.GetOrAddKeys( locationId, locId =>
            {
                if ( rockContext != null )
                {
                    return QueryDbForLocationId( locId, rockContext );
                }
                else
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        return QueryDbForLocationId( locId, newRockContext );
                    }
                }
            } );

            return GetMany( keys.AsIntegerList(), rockContext ).ToList();
        }

        /// <summary>
        /// Queries the database for all group location keys for the
        /// given location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of group locations.</returns>
        private static List<string> QueryDbForLocationId( int locationId, RockContext rockContext )
        {
            var service = new GroupLocationService( rockContext );

            return service.Queryable()
                .AsNoTracking()
                .Include( gl => gl.Location )
                .Include( gl => gl.Schedules )
                .Where( gl => gl.LocationId == locationId )
                .Select( i => i.Id )
                .ToList()
                .ConvertAll( i => i.ToString() );
        }

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is GroupLocation groupLocation ) )
            {
                return;
            }

            GroupId = groupLocation.GroupId;
            LocationId = groupLocation.LocationId;
            GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId;
            IsMailingLocation = groupLocation.IsMailingLocation;
            IsMappedLocation = groupLocation.IsMappedLocation;
            GroupMemberPersonAliasId = groupLocation.GroupMemberPersonAliasId;
            Order = groupLocation.Order;
            ScheduleIds = groupLocation.Schedules.Select( s => s.Id ).ToList();

            _isNamedLocation = groupLocation.Location.Name.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Removes or invalidates the CachedItem based on EntityState
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityState">State of the entity. If unknown, use <see cref="EntityState.Detached" /></param>
        public static new void UpdateCachedEntity( int entityId, EntityState entityState )
        {
            throw new NotSupportedException( "Do not call UpdateCachedEntity on GroupLocationCache with an entity identifier." );
        }

        /// <summary>
        /// Removes or invalidates the GroupLocationCache based on <paramref name="entityState"/>.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="entityState">State of the entity. If unknown, use <see cref="EntityState.Detached" /></param>
        public static void UpdateCachedEntity( GroupLocation entity, EntityState entityState )
        {
            if ( entityState == EntityState.Deleted )
            {
                Remove( entity );
            }
            else if ( entityState == EntityState.Added )
            {
                // add this entity to All Ids, but don't fetch it into cache until somebody asks for it
                AddToAllIds( entity );
            }
            else
            {
                FlushItem( entity.Id );
            }
        }

        /// <summary>
        /// This method is not supported on GroupLocationCache, call the method
        /// that takes a <see cref="GroupLocation"/> parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        public static new void Remove( int key )
        {
            throw new NotSupportedException( "Do not call Remove on GroupLocationCache with an entity identifier." );
        }

        /// <summary>
        /// This method is not supported on GroupLocationCache, call the method
        /// that takes a <see cref="GroupLocation"/> parameter.
        /// </summary>
        /// <param name="key">The key.</param>
        public static new void Remove( string key )
        {
            throw new NotSupportedException( "Do not call Remove on GroupLocationCache with a cache key." );
        }

        /// <summary>
        /// Removes the related cache for the entity.
        /// </summary>
        /// <param name="entity">The entity whose cache data should be removed.</param>
        public static void Remove( GroupLocation entity )
        {
            var key = entity.Id.ToString();

            ItemCache<GroupLocationCache>.Remove( key );
            _byLocationIdCache.Remove( key, entity.LocationId );
        }

        /// <summary>
        /// Adds a new entity to the "all ids" lists.
        /// </summary>
        /// <param name="entity">The entity whose cache data should be added.</param>
        public static void AddToAllIds( GroupLocation entity )
        {
            var key = entity.Id.ToString();

            ItemCache<GroupLocationCache>.AddToAllIds( key );
            _byLocationIdCache.Add( key, entity.LocationId );
        }

        /// <summary>
        /// Removes all items of this type from cache.
        /// </summary>
        public static new void Clear()
        {
            ItemCache<GroupLocationCache>.Clear();
            _byLocationIdCache.Clear();
        }

        /// <summary>
        /// Clears the by location cached lookup table for the specified
        /// location. This should be called whenever a GroupLocation
        /// is added, removed, or modified in a way that would change
        /// the list of GroupLocation identifiers associated with the
        /// Location.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        public static void ClearByLocationId( int locationId )
        {
            _byLocationIdCache.Clear( locationId );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Group {GroupId} at {Location.ToStringSafe()}";
        }

        #endregion Public Methods
    }
}
