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
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a signal type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class SignalTypeCache : CachedModel<SignalType>
    {
        #region Constructors

        private SignalTypeCache()
        {
        }

        private SignalTypeCache( SignalType signalType )
        {
            CopyFromModel( signalType );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType name.
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
        /// Gets or sets the HTML color of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType color.
        /// </value>
        public string SignalColor { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class of the SignalType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType icon class.
        /// </value>
        public string SignalIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is SignalType )
            {
                var signalType = ( SignalType ) model;
                this.Name = signalType.Name;
                this.Description = signalType.Description;
                this.SignalColor = signalType.SignalColor;
                this.SignalIconCssClass = signalType.SignalIconCssClass;
                this.Order = signalType.Order;
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
        /// Gets the cache key for the selected campu id.
        /// </summary>
        /// <param name="id">The campus id.</param>
        /// <returns></returns>
        private static string CacheKey( int id )
        {
            return string.Format( "Rock:SignalType:{0}", id );
        }

        /// <summary>
        /// Returns SignalType object from cache.  If SignalType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( SignalTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static SignalTypeCache LoadById( int id, RockContext rockContext )
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

        private static SignalTypeCache LoadById2( int id, RockContext rockContext )
        {
            var signalTypeService = new SignalTypeService( rockContext );
            var signalTypeModel = signalTypeService
                .Queryable().AsNoTracking()
                .FirstOrDefault( c => c.Id == id );
            if ( signalTypeModel != null )
            {
                return new SignalTypeCache( signalTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( Guid guid, RockContext rockContext = null )
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
            var signalTypeService = new CampusService( rockContext );
            return signalTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds SignalType model to cache, and returns cached object
        /// </summary>
        /// <param name="signalTypeModel">The signal type model.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( SignalType signalTypeModel )
        {
            return GetOrAddExisting( SignalTypeCache.CacheKey( signalTypeModel.Id ),
                () => LoadByModel( signalTypeModel ) );
        }

        private static SignalTypeCache LoadByModel( SignalType signalTypeModel )
        {
            if ( signalTypeModel != null )
            {
                return new SignalTypeCache( signalTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes signal type from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( SignalTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}