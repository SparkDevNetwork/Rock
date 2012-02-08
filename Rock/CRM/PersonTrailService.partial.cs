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
        public Guid Current( Guid guid )
        {
            PersonTrail personTrail = GetByPrevGuid( guid );
            while ( personTrail != null )
            {
                guid = personTrail.Guid;
                personTrail = GetByPrevGuid( guid );
            }
            return guid;
        }

        /// <summary>
        /// Get's the current person id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public int Current( int id )
        {
            PersonTrail personTrail = GetByPrevId( id );
            while ( personTrail != null )
            {
                id = personTrail.Id;
                personTrail = GetByPrevId( id );
            }
            return id;
        }

    }
}