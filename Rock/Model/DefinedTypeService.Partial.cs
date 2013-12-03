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
        public IEnumerable<Rock.Model.DefinedType> GetByFieldTypeId( int? fieldTypeId )
        {
            return Repository.Find( t => ( t.FieldTypeId == fieldTypeId || ( fieldTypeId == null && t.FieldTypeId == null ) ) ).OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.DefinedType"/> by GUID identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.DefinedType"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.DefinedType"/> with a matching Guid identifier. If a match is not found, null is returned.</returns>
        public Rock.Model.DefinedType GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
    }
}
