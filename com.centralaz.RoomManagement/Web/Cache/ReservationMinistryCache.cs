// <copyright>
// Copyright by the Central Christian Church
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
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;
using Rock;
using Rock.Data;
using Rock.Web.Cache;
namespace com.centralaz.RoomManagement.Web.Cache
{
    /// <summary>
    /// Information about a reservation ministry that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class ReservationMinistryCache : CachedModel<ReservationMinistry>
    {
        #region Constructors

        private ReservationMinistryCache()
        {
        }

        private ReservationMinistryCache( ReservationMinistry reservationMinistry )
        {
            CopyFromModel( reservationMinistry );
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Rock.Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is ReservationMinistry )
            {
                var reservationMinistry = (ReservationMinistry)model;
                this.Description = reservationMinistry.Description;
                this.IsActive = reservationMinistry.IsActive;
                this.Name = reservationMinistry.Name;
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
        /// Gets the cache key for the selected reservation ministry id.
        /// </summary>
        /// <param name="id">The reservation ministry id.</param>
        /// <returns></returns>
        public static string CacheKey( int id )
        {
            return string.Format( "com.centralaz.RoomManagement:ReservationMinistry:{0}", id );
        }

        /// <summary>
        /// Returns ReservationMinistry object from cache.  If reservation ministry does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static ReservationMinistryCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( ReservationMinistryCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static ReservationMinistryCache LoadById( int id, RockContext rockContext )
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

        private static ReservationMinistryCache LoadById2( int id, RockContext rockContext )
        {
            var reservationMinistryService = new ReservationMinistryService( rockContext );
            var reservationMinistryModel = reservationMinistryService
                .Queryable().AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( reservationMinistryModel != null )
            {
                return new ReservationMinistryCache( reservationMinistryModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static ReservationMinistryCache Read( Guid guid, RockContext rockContext = null )
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
            var reservationMinistryService = new ReservationMinistryService( rockContext );
            return reservationMinistryService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds ReservationMinistry model to cache, and returns cached object
        /// </summary>
        /// <param name="reservationMinistryModel"></param>
        /// <returns></returns>
        public static ReservationMinistryCache Read( ReservationMinistry reservationMinistryModel )
        {
            return GetOrAddExisting( ReservationMinistryCache.CacheKey( reservationMinistryModel.Id ),
                () => LoadByModel( reservationMinistryModel ) );
        }

        private static ReservationMinistryCache LoadByModel( ReservationMinistry reservationMinistryModel )
        {
            if ( reservationMinistryModel != null )
            {
                return new ReservationMinistryCache( reservationMinistryModel );
            }
            return null;
        }

        /// <summary>
        /// Returns all reservation ministrys
        /// </summary>
        /// <returns></returns>
        public static List<ReservationMinistryCache> All()
        {
            List<ReservationMinistryCache> reservationMinistrys = new List<ReservationMinistryCache>();
            var reservationMinistryIds = GetOrAddExisting( "com.centralaz.RoomManagement:ReservationMinistry:All", () => LoadAll() );
            if ( reservationMinistryIds != null )
            {
                foreach ( int reservationMinistryId in reservationMinistryIds )
                {
                    reservationMinistrys.Add( ReservationMinistryCache.Read( reservationMinistryId ) );
                }
            }
            return reservationMinistrys;
        }

        private static List<int> LoadAll()
        {
            using ( var rockContext = new RockContext() )
            {
                return new ReservationMinistryService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name )
                    .Select( c => c.Id )
                    .ToList();
            }
        }

        /// <summary>
        /// Removes reservation ministry from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( ReservationMinistryCache.CacheKey( id ) );
            FlushCache( "com.centralaz.RoomManagement:ReservationMinistry:All" );
        }

        #endregion
    }
}