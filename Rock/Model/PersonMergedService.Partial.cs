//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Person Merged Service class
    /// </summary>
    public partial class PersonMergedService 
    {
        public PersonMerged GetNew( int previousPersonId )
        {
            return Repository.FirstOrDefault( t => t.PreviousPersonId == previousPersonId );
        }

        public PersonMerged GetNew( Guid previousPersonGuid )
        {
            return Repository.FirstOrDefault( t => t.PreviousPersonGuid == previousPersonGuid );
        }

        public virtual PersonMerged GetNewByPreviousEncryptedKey( string encryptedKey )
        {
            string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            return GetNewByPreviousPublicKey( publicKey );
        }

        public virtual PersonMerged GetNewByPreviousPublicKey( string publicKey )
        {
            try
            {
                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = Int32.Parse( idParts[0] );
                    Guid guid = new Guid( idParts[1] );

                    PersonMerged personMerged = GetNew( id );

                    if ( personMerged != null && personMerged.PreviousPersonGuid.CompareTo( guid ) == 0 )
                    {
                        return personMerged;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Currents the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public string Current( string publicKey )
        {
            PersonMerged personMerged = GetNewByPreviousEncryptedKey( publicKey );
            while ( personMerged != null )
            {
                publicKey = personMerged.NewEncryptedKey;
                personMerged = GetNewByPreviousEncryptedKey( publicKey );
            }
            return publicKey;
        }

        /// <summary>
        /// Currents the specified person id.
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public int Current( int personId )
        {
            PersonMerged personMerged = GetNew( personId );
            while ( personMerged != null )
            {
                personId = personMerged.NewPersonId;
                personMerged = GetNew( personId );
            }
            return personId;
        }

        /// <summary>
        /// Currents the specified person GUID.
        /// </summary>
        /// <param name="personGuid">The person GUID.</param>
        /// <returns></returns>
        public Guid Current( Guid personGuid )
        {
            PersonMerged personMerged = GetNew( personGuid );
            while ( personMerged != null )
            {
                personGuid = personMerged.NewPersonGuid;
                personMerged = GetNew( personGuid );
            }
            return personGuid;
        }
    }
}
