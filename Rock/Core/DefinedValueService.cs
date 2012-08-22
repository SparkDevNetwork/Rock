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
	/// Defined Value POCO Service class
	/// </summary>
    public partial class DefinedValueService : Service<DefinedValue, DefinedValueDTO>
    {
		/// <summary>
		/// Gets Defined Values by Defined Type Id
		/// </summary>
		/// <param name="definedTypeId">Defined Type Id.</param>
		/// <returns>An enumerable list of DefinedValue objects.</returns>
	    public IEnumerable<DefinedValue> GetByDefinedTypeId( int definedTypeId )
        {
            return Repository.Find( t => t.DefinedTypeId == definedTypeId ).OrderBy( t => t.Order );
        }
		
		/// <summary>
		/// Gets Defined Value by Guid
		/// </summary>
		/// <param name="guid">Guid.</param>
		/// <returns>DefinedValue object.</returns>
	    public DefinedValue GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override DefinedValue CreateNew()
        {
            return new DefinedValue();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DefinedValueDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new DefinedValueDTO( m ) );
        }
    }
}
