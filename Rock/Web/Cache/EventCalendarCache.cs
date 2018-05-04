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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an event calendar that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheEventCalendar instead" )]
    public class EventCalendarCache : CachedModel<EventCalendar>
    {
        #region Constructors

        private EventCalendarCache()
        {
        }

        private EventCalendarCache( CacheEventCalendar cacheEventCalendar )
        {
            CopyFromNewCache( cacheEventCalendar );
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
        /// Gets or sets the is active.
        /// </summary>
        /// <value>
        /// The is active.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the icons CSS class.
        /// </summary>
        /// <value>
        /// The icons CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the cache key for the selected event calendar id.
        /// </summary>
        /// <param name="id">The event calendar id.</param>
        /// <returns></returns>
        [Obsolete ("No longer used", false)]
        public static string CacheKey( int id )
        {
            return $"Rock:EventCalendar:{id}";
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is EventCalendar ) ) return;

            var eventCalendar = (EventCalendar)model;
            Description = eventCalendar.Description;
            IconCssClass = eventCalendar.IconCssClass;
            IsActive = eventCalendar.IsActive;
            Name = eventCalendar.Name;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheEventCalendar ) ) return;

            var eventCalendar = (CacheEventCalendar)cacheEntity;
            Description = eventCalendar.Description;
            IconCssClass = eventCalendar.IconCssClass;
            IsActive = eventCalendar.IsActive;
            Name = eventCalendar.Name;
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

        #region Static Methods

        /// <summary>
        /// Returns EventCalendar object from cache.  If event calendar does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EventCalendarCache Read( int id, RockContext rockContext = null )
        {
            return new EventCalendarCache( CacheEventCalendar.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EventCalendarCache Read( Guid guid, RockContext rockContext = null )
        {
            return new EventCalendarCache( CacheEventCalendar.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds EventCalendar model to cache, and returns cached object
        /// </summary>
        /// <param name="eventCalendarModel"></param>
        /// <returns></returns>
        public static EventCalendarCache Read( EventCalendar eventCalendarModel )
        {
            return new EventCalendarCache( CacheEventCalendar.Get( eventCalendarModel ) );
        }

        /// <summary>
        /// Returns all event calendars
        /// </summary>
        /// <returns></returns>
        public static List<EventCalendarCache> All()
        {
            var eventCalendars = new List<EventCalendarCache>();

            var cacheEventCalendars = CacheEventCalendar.All();
            if ( cacheEventCalendars == null ) return eventCalendars;

            foreach ( var cacheEventCalendar in cacheEventCalendars )
            {
                eventCalendars.Add( new EventCalendarCache( cacheEventCalendar ) );
            }

            return eventCalendars;
        }

        /// <summary>
        /// Removes event calendar from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheEventCalendar.Remove( id );
        }

        #endregion
    }
}