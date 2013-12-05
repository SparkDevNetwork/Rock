//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

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
        public IEnumerable<DefinedValue> GetByDefinedTypeId( int definedTypeId )
        {
            return Repository.Find( t => t.DefinedTypeId == definedTypeId ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to a specified <see cref="Rock.Model.DefinedType"/> retrieved by the DefinedType's Guid identifier.
        /// </summary>
        /// <param name="definedTypeGuid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to retrieve <see cref="Rock.Model.DefinedValue">DefinedValues</see>
        /// for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.DefinedValue">DefinedValues</see> that belong to the <see cref="Rock.Model.DefinedType"/> specified by the provided Guid. If a match
        /// is not found, an empty collection will be returned.</returns>
        public IEnumerable<DefinedValue> GetByDefinedTypeGuid( Guid definedTypeGuid )
        {
            var definedType = new DefinedTypeService().GetByGuid( definedTypeGuid );
            return GetByDefinedTypeId( definedType.Id );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.DefinedValue"/> by it's Guid identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedValue"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.DefinedValue"/> specified by the provided Guid. If a match is not found, a null value will be returned.</returns>
        public DefinedValue GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Returns a DefinedValueId of a  <see cref="Rock.Model.DefinedValue" /> by it's Guid.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedValue"/> to retrieve the DefinedvalueId for.</param>
        /// <returns>A <see cref="System.Int32"/> representing the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> specified by the provided Guid. If a match is not found,
        /// a null value will be returned.</returns>
        public int? GetIdByGuid( Guid guid )
        {
            return Repository.AsQueryable()
                .Where( t => t.Guid == guid )
                .Select( t => t.Id )
                .FirstOrDefault();
        }
    }
}
