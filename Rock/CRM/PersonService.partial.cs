//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Configuration;
using System.Linq;

namespace Rock.CRM
{
	public partial class PersonService
	{
        /// <summary>
        /// Gets a list of people with a matching full name
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
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

            return Repository.AsQueryable().
                    Where( p => p.LastName.ToLower().StartsWith( lastName.ToLower() ) &&
                        ( p.NickName.ToLower().StartsWith( firstName.ToLower() ) ||
                        p.FirstName.StartsWith( firstName.ToLower() ) ) );
        }

        public Person GetByEncryptedID( string encryptedID )
        {
            string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
            if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
                encryptionPhrase = "Rock Rocks!";

            string identifier = Rock.Security.Encryption.DecryptString( encryptedID, encryptionPhrase );

            string[] idParts = identifier.Split( '|' );
            if ( idParts.Length == 2 )
            {
                Guid personGuid = new Guid( idParts[0] );
                int personId = Int32.Parse( idParts[1] );

                return Repository.AsQueryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();
            }

            return null;
        }
    }
}