//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// Defined Type POCO Service class
    /// </summary>
    public partial class DefinedTypeService : Service<DefinedType, DefinedTypeDto>
    {
        /// <summary>
        /// Gets Defined Types by Field Type Id
        /// </summary>
        /// <param name="fieldTypeId">Field Type Id.</param>
        /// <returns>An enumerable list of DefinedType objects.</returns>
        public IEnumerable<Rock.Core.DefinedType> GetByFieldTypeId( int? fieldTypeId )
        {
            return Repository.Find( t => ( t.FieldTypeId == fieldTypeId || ( fieldTypeId == null && t.FieldTypeId == null ) ) ).OrderBy( t => t.Order );
        }
        
        /// <summary>
        /// Gets Defined Type by Guid
        /// </summary>
        /// <param name="guid">Guid.</param>
        /// <returns>DefinedType object.</returns>
        public Rock.Core.DefinedType GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
    }
}
