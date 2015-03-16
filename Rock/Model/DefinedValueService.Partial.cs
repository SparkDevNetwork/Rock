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
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.DefinedValue"/> entity objects.
    /// </summary>
    public partial class DefinedValueService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to a specified <see cref="Rock.Model.DefinedType"/> retrieved by the DefinedType's DefinedTypeId.
        /// </summary>
        /// <param name="definedTypeId">A <see cref="System.Int32"/> representing the DefinedTypeId of the <see cref="Rock.Model.DefinedType"/> to retrieve <see cref="Rock.Model.DefinedValue">DefinedValues</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to the specified <see cref="Rock.Model.DefinedType"/>. The <see cref="Rock.Model.DefinedValue">DefinedValues</see> will 
        /// be ordered by the <see cref="DefinedValue">DefinedValue's</see> Order property.</returns>
        public IOrderedQueryable<DefinedValue> GetByDefinedTypeId( int definedTypeId )
        {
            return Queryable()
                .Where( t => t.DefinedTypeId == definedTypeId )
                .OrderBy( t => t.Order )
                .ThenBy( a => a.Value );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to a specified <see cref="Rock.Model.DefinedType"/> retrieved by the DefinedType's Guid identifier.
        /// </summary>
        /// <param name="definedTypeGuid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to retrieve <see cref="Rock.Model.DefinedValue">DefinedValues</see>
        /// for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to the <see cref="Rock.Model.DefinedType"/> specified by the provided Guid. If a match
        /// is not found, an empty collection will be returned.</returns>
        public IOrderedQueryable<DefinedValue> GetByDefinedTypeGuid( Guid definedTypeGuid )
        {
            var definedTypeCache = DefinedTypeCache.Read( definedTypeGuid );
            if ( definedTypeCache != null )
            {
                return GetByDefinedTypeId( definedTypeCache.Id );
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.DefinedValue"/> by it's Guid identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedValue"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.DefinedValue"/> specified by the provided Guid. If a match is not found, a null value will be returned.</returns>
        public DefinedValue GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Returns a DefinedValueId of a  <see cref="Rock.Model.DefinedValue" /> by it's Guid.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedValue"/> to retrieve the DefinedvalueId for.</param>
        /// <returns>A <see cref="System.Int32"/> representing the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> specified by the provided Guid. If a match is not found,
        /// a null value will be returned.</returns>
        public int? GetIdByGuid( Guid guid )
        {
            return Queryable()
                .Where( t => t.Guid == guid )
                .Select( t => t.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the Guid for the DefinedValue that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.DefinedValueCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }
}
