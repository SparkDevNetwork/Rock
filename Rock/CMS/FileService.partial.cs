//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.CMS
{
    public partial class FileService
    {
        /// <summary>
        /// Gets the file by GUID.
        /// </summary>
        /// <param name="guidString">The GUID string.</param>
        /// <returns></returns>
		public Rock.CMS.File GetByGuid( string guidString )
        {
			Guid guid = new Guid( guidString );
			return Repository.FirstOrDefault( f => f.Guid == guid );
        }
    }
}