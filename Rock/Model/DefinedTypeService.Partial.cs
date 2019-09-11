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
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.DefinedType"/> entity objects.
    /// </summary>
    public partial class DefinedTypeService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> by FieldTypeId
        /// </summary>
        /// <param name="fieldTypeId">A <see cref="System.Int32"/> representing the FieldTypeId of the <see cref="Rock.Model.FieldType"/> that is used for the 
        /// <see cref="Rock.Model.DefinedValue"/>
        /// </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DefinedType">DefinedTypes</see> that use the specified <see cref="Rock.Model.FieldType"/>.</returns>
        public IOrderedQueryable<Rock.Model.DefinedType> GetByFieldTypeId( int? fieldTypeId )
        {
            return Queryable()
                .Where( t => 
                    ( t.FieldTypeId == fieldTypeId || 
                        ( fieldTypeId == null && t.FieldTypeId == null ) ) )
                .OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.DefinedType"/> by GUID identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.DefinedType"/> with a matching Guid identifier. If a match is not found, null is returned.</returns>
        public Rock.Model.DefinedType GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }


        /// <summary>
        /// Gets the Guid for the DefinedType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = DefinedTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }
}
