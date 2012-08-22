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
	/// Service Log POCO Service class
	/// </summary>
    public partial class ServiceLogService : Service<ServiceLog, ServiceLogDTO>
    {
        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override ServiceLog CreateNew()
        {
            return new ServiceLog();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<ServiceLogDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new ServiceLogDTO( m ) );
        }
    }
}
