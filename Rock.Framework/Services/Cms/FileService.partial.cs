//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Services.Cms
{
    public partial class FileService
    {
		public Rock.Models.Cms.File GetByGuid( string guidString )
        {
			Guid guid = new Guid( guidString );
			return Repository.FirstOrDefault( f => f.Guid == guid );
        }
    }
}