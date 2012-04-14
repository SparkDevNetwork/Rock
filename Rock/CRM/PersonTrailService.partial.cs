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
	public partial class PersonTrailService
	{

        /// <summary>
        /// Get's the current person Guid
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public string Current( string publicKey )
        {
            PersonTrail personTrail = GetByPublicKey ( publicKey );
            while ( personTrail != null )
            {
                publicKey = personTrail.CurrentPublicKey;
                personTrail = GetByPublicKey( publicKey );
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
            PersonTrail personTrail = Get( id );
            while ( personTrail != null )
            {
                id = personTrail.CurrentId;
                personTrail = Get( id );
            }
            return id;
        }

    }
}