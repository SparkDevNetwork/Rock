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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a signal type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheSignalType instead" )]
    public class SignalTypeCache : CachedModel<SignalType>
    {
        #region Constructors

        private SignalTypeCache()
        {
        }

        private SignalTypeCache( CacheSignalType cacheSignalType )
        {
            CopyFromNewCache( cacheSignalType );
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
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is SignalType ) ) return;

            var signalType = (SignalType)model;
            Name = signalType.Name;
            Description = signalType.Description;
            SignalColor = signalType.SignalColor;
            SignalIconCssClass = signalType.SignalIconCssClass;
            Order = signalType.Order;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheSignalType ) ) return;

            var signalType = (CacheSignalType)cacheEntity;
            Name = signalType.Name;
            Description = signalType.Description;
            SignalColor = signalType.SignalColor;
            SignalIconCssClass = signalType.SignalIconCssClass;
            Order = signalType.Order;
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
        /// Returns SignalType object from cache.  If SignalType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( int id, RockContext rockContext = null )
        {
            return new SignalTypeCache( CacheSignalType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new SignalTypeCache( CacheSignalType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds SignalType model to cache, and returns cached object
        /// </summary>
        /// <param name="signalTypeModel">The signal type model.</param>
        /// <returns></returns>
        public static SignalTypeCache Read( SignalType signalTypeModel )
        {
            return new SignalTypeCache( CacheSignalType.Get( signalTypeModel ) );
        }

        /// <summary>
        /// Removes signal type from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheSignalType.Remove( id );
        }

        #endregion
    }
}