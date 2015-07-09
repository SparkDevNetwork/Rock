// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an event calendar that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class EventCalendarCache : CachedModel<EventCalendar>
    {
        #region Constructors

        private EventCalendarCache()
        {
        }

        private EventCalendarCache( EventCalendar eventCalendar )
        {
            CopyFromModel( eventCalendar );
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
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is EventCalendar )
            {
                var eventCalendar = (EventCalendar)model;
                this.Description = eventCalendar.Description;
                this.IconCssClass = eventCalendar.IconCssClass;
                this.IsActive = eventCalendar.IsActive;
                this.Name = eventCalendar.Name;
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
        /// Gets the cache key for the selected event calendar id.
        /// </summary>
        /// <param name="id">The event calendar id.</param>
        /// <returns></returns>
        public static string CacheKey( int id )
        {
            return string.Format( "Rock:EventCalendar:{0}", id );
        }

        /// <summary>
        /// Returns EventCalendar object from cache.  If event calendar does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EventCalendarCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( EventCalendarCache.CacheKey( id ), 
                () => LoadById( id, rockContext ) );
        }

        private static EventCalendarCache LoadById( int id, RockContext rockContext )
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

        private static EventCalendarCache LoadById2( int id, RockContext rockContext )
        {
            var eventCalendarService = new EventCalendarService( rockContext );
            var eventCalendarModel = eventCalendarService
                .Queryable().AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( eventCalendarModel != null )
            {
                eventCalendarModel.LoadAttributes( rockContext );
                return new EventCalendarCache( eventCalendarModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EventCalendarCache Read( Guid guid, RockContext rockContext = null )
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
            var eventCalendarService = new EventCalendarService( rockContext );
            return eventCalendarService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ))
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds EventCalendar model to cache, and returns cached object
        /// </summary>
        /// <param name="eventCalendarModel"></param>
        /// <returns></returns>
        public static EventCalendarCache Read( EventCalendar eventCalendarModel )
        {
            return GetOrAddExisting( EventCalendarCache.CacheKey( eventCalendarModel.Id ),
                () => LoadByModel( eventCalendarModel ) );
        }

        private static EventCalendarCache LoadByModel( EventCalendar eventCalendarModel )
        {
            if ( eventCalendarModel != null )
            {
                return new EventCalendarCache( eventCalendarModel );
            }
            return null;
        }

        /// <summary>
        /// Returns all event calendars
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use All() method instead. RockContext parameter is no longer needed." )]
        public static List<EventCalendarCache> All( RockContext rockContext )
        {
            return All();
        }

        /// <summary>
        /// Returns all event calendars
        /// </summary>
        /// <returns></returns>
        public static List<EventCalendarCache> All()
        {
            List<EventCalendarCache> eventCalendars = new List<EventCalendarCache>();
            var eventCalendarIds = GetOrAddExisting( "Rock:EventCalendar:All", () => LoadAll() );
            if ( eventCalendarIds != null )
            {
                foreach ( int eventCalendarId in eventCalendarIds )
                {
                    eventCalendars.Add( EventCalendarCache.Read( eventCalendarId ) );
                }
            }
            return eventCalendars;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new EventCalendarService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes event calendar from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( EventCalendarCache.CacheKey( id ) );
            FlushCache( "Rock:EventCalendar:All" );
        }

        #endregion
    }
}