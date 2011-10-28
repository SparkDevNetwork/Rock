//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Rock.Models.Crm;
using Rock.Repository.Crm;

namespace Rock.Services.Crm
{
	public partial class PersonService
	{
        public IQueryable<Person> GetByFullName( string fullName )
        {
            string firstName = string.Empty;
            string lastName = string.Empty;

            if (fullName.Contains(' '))
            {
                firstName = fullName.Substring( 0, fullName.LastIndexOf( ' ' ) );
                lastName = fullName.Substring( fullName.LastIndexOf( ' ' ) + 1 );
            }
            else
                lastName = fullName;

            return _repository.AsQueryable().
                    Where( p => p.LastName.ToLower().StartsWith( lastName.ToLower() ) &&
                        ( p.NickName.ToLower().StartsWith( firstName.ToLower() ) ||
                        p.FirstName.StartsWith( firstName.ToLower() ) ) );
        }
	}
}