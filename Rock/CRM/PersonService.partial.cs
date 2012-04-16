//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
                        p.GivenName.StartsWith( firstName.ToLower() ) ) );
        }

        /// <summary>
        /// Gets the by encrypted ID.
        /// </summary>
        /// <param name="encryptedID">The encrypted ID.</param>
        /// <returns></returns>
        public Person GetByEncryptedID( string encryptedID )
        {
            string identifier = Rock.Security.Encryption.DecryptString( encryptedID );

            string[] idParts = identifier.Split( '|' );
            if ( idParts.Length == 2 )
            {
                Guid personGuid = new Guid( idParts[0] );
                int personId = Int32.Parse( idParts[1] );

                Person person = Repository.AsQueryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( person != null )
                    return person;

                // Check to see if the record was merged
                PersonTrailService personTrailService = new PersonTrailService();
                PersonTrail personTrail = personTrailService.Queryable().
                    Where( p => p.Guid == personGuid && p.Id == personId ).FirstOrDefault();

                if ( personTrail != null )
                    return Get( personTrail.Id, true );
            }

            return null;
        }

        /// <summary>
        /// Gets Person by Id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail.</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Rock.CRM.Person Get( int id, bool followTrail )
        {
            if ( followTrail )
                id = new PersonTrailService().Current( id );

            return Get( id );
        }

        /// <summary>
        /// Gets Person by Guid
        /// </summary>
        /// <param name="guid">Guid.</param>
        /// <param name="followTrail">if set to <c>true</c> follow the merge trail</param>
        /// <returns>
        /// Person object.
        /// </returns>
        public Rock.CRM.Person GetByPublicKey( string publicKey, bool followTrail )
        {
            if ( followTrail )
                publicKey = new PersonTrailService().Current( publicKey );

            return GetByPublicKey( publicKey );
        }
    }
}