//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CRM
{
	/// <summary>
	/// Campus POCO Service class
	/// </summary>
    public partial class CampusService : Service<Campus, CampusDTO>
    {
        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override Campus CreateNew()
        {
            return new Campus();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<CampusDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new CampusDTO( m ) );
        }
    }
}
