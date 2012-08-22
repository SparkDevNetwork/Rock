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
	/// Exception Log POCO Service class
	/// </summary>
    public partial class ExceptionLogService : Service<ExceptionLog, ExceptionLogDTO>
    {
		/// <summary>
		/// Gets Exception Logs by Parent Id
		/// </summary>
		/// <param name="parentId">Parent Id.</param>
		/// <returns>An enumerable list of ExceptionLog objects.</returns>
	    public IEnumerable<ExceptionLog> GetByParentId( int? parentId )
        {
            return Repository.Find( t => ( t.ParentId == parentId || ( parentId == null && t.ParentId == null ) ) );
        }
		
		/// <summary>
		/// Gets Exception Logs by Person Id
		/// </summary>
		/// <param name="personId">Person Id.</param>
		/// <returns>An enumerable list of ExceptionLog objects.</returns>
	    public IEnumerable<ExceptionLog> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
		
		/// <summary>
		/// Gets Exception Logs by Site Id
		/// </summary>
		/// <param name="siteId">Site Id.</param>
		/// <returns>An enumerable list of ExceptionLog objects.</returns>
	    public IEnumerable<ExceptionLog> GetBySiteId( int? siteId )
        {
            return Repository.Find( t => ( t.SiteId == siteId || ( siteId == null && t.SiteId == null ) ) );
        }
    }
}
