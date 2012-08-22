//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.CMS
{
	/// <summary>
	/// File POCO Service class
	/// </summary>
    public partial class FileService : Service<File, FileDTO>
    {
        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override File CreateNew()
        {
            return new File();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<FileDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new FileDTO( m ) );
        }
    }
}
