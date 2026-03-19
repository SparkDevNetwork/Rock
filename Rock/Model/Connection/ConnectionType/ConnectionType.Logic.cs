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

using Rock.Attribute;
using Rock.Model.Connection.ConnectionType.DTO;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class ConnectionType
    {
        #region Methods
        
        /// <summary>
        /// Gets the additional settings for this Connection Type.
        /// </summary>
        internal ConnectionTypeAdditionalSettings GetConnectionTypeAdditionalSettings()
        {
            return this.GetAdditionalSettingsOrNull<ConnectionTypeAdditionalSettings>();
        }

        /// <summary>
        /// Sets the additional settings for this Connection Type.
        /// </summary>
        internal void SetConnectionTypeAdditionalSettings( ConnectionTypeAdditionalSettings settings )
        {
            this.SetAdditionalSettings( settings );
        }

        /// <summary>
        /// Gets whether <paramref name="targetStatusId"/> is the next sequential, active <see cref="ConnectionStatus"/>
        /// after <paramref name="currentStatusId"/>.
        /// </summary>
        /// <param name="connectionTypeId">The identifier of the <see cref="ConnectionType"/>.</param>
        /// <param name="currentStatusId">The identifier of the current <see cref="ConnectionStatus"/>.</param>
        /// <param name="targetStatusId">The identifier of the target <see cref="ConnectionStatus"/>.</param>
        /// <returns>
        /// Whether <paramref name="targetStatusId"/> is the next sequential, active <see cref="ConnectionStatus"/>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "19.0" )]
        public static bool IsNextSequentialActiveStatus( int connectionTypeId, int currentStatusId, int targetStatusId )
        {
            if ( connectionTypeId <= 0
                || currentStatusId <= 0
                || targetStatusId <= 0 )
            {
                return false;
            }

            var connectionTypeCache = ConnectionTypeCache.Get( connectionTypeId );
            if ( connectionTypeCache == null )
            {
                return false;
            }

            return connectionTypeCache.IsNextSequentialActiveStatus( currentStatusId, targetStatusId );
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return ConnectionTypeCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            ConnectionTypeCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region overrides

        /// <summary>
        /// Gets a list of all attributes defined for the ConnectionTypes specified that
        /// match the entityTypeQualifierColumn and the ConnectionRequest Ids.
        /// </summary>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <param name="entityTypeId">The Entity Type Id for which Attributes to load.</param>
        /// <param name="entityTypeQualifierColumn">The EntityTypeQualifierColumn value to match against.</param>
        /// <returns>A list of attributes defined in the inheritance tree.</returns>
        [Obsolete( "ConnectionRequest now has a ConnectionTypeId to handle inherited attributes." )]
        [RockObsolete( "17.0" )]
        public List<AttributeCache> GetInheritedAttributesForQualifier( Rock.Data.RockContext rockContext, int entityTypeId, string entityTypeQualifierColumn )
        {
            var attributes = new List<AttributeCache>();
            //
            // Walk each group type and generate a list of matching attributes.
            //
            foreach ( var attribute in AttributeCache.GetByEntityType( entityTypeId ) )
            {
                // group type ids exist and qualifier is for a group type id
                if ( string.Compare( attribute.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    int groupTypeIdValue = int.MinValue;
                    if ( int.TryParse( attribute.EntityTypeQualifierValue, out groupTypeIdValue ) && this.Id == groupTypeIdValue )
                    {
                        attributes.Add( attribute );
                    }
                }
            }

            return attributes.OrderBy( a => a.Order ).ToList();
        }

        #endregion

        #endregion
    }
}