//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Data
{
	public interface IDto
	{
		int Id { get; set; }
		Guid Guid { get; set; }
		void CopyFromModel( IEntity model );
		void CopyToModel( IEntity model );
	}
}
