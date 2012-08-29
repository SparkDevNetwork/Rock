//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Crm
{
	/// <summary>
	/// Person Trail POCO Service class
	/// </summary>
    public partial class PersonMergedService : Service<PersonMerged, PersonMergedDto>
    {
		/// <summary>
		/// Gets Person Trails by Current Id
		/// </summary>
		/// <param name="currentId">Current Id.</param>
		/// <returns>An enumerable list of PersonMerged objects.</returns>
	    public IEnumerable<PersonMerged> GetByCurrentId( int currentId )
        {
            return Repository.Find( t => t.CurrentId == currentId );
        }

        /// <summary>
        /// Get's the current person Guid
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public string Current( string publicKey )
        {
            PersonMerged personMerged = GetByEncryptedKey( publicKey );
            while ( personMerged != null )
            {
                publicKey = personMerged.CurrentPublicKey;
                personMerged = GetByEncryptedKey( publicKey );
            }
            return publicKey;
        }

        /// <summary>
        /// Get's the current person id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public int Current( int id )
        {
            PersonMerged personMerged = Get( id );
            while ( personMerged != null )
            {
                id = personMerged.CurrentId;
                personMerged = Get( id );
            }
            return id;
        }
    }
}
