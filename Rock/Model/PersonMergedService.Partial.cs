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
    /// Data access/service class for <see cref="Rock.Model.PersonMerged"/> entity type objects.
    /// </summary>
    public partial class PersonMergedService 
    {

        /// <summary>
        /// Gets a <see cref="Rock.Model.PersonMerged"/> entity based on a <see cref="Rock.Model.Person">Person's</see> previous PersonId.
        /// </summary>
        /// <param name="previousPersonId">A <see cref="System.Int32"/> representing a <see cref="Rock.Model.Person">Person's</see> previous PersonId.</param>
        /// <returns>A <see cref="Rock.Model.PersonMerged"/> entity that contains the <see cref="Rock.Model.Person">Person's</see> new identifiers.</returns>
        public PersonMerged GetNew( int previousPersonId )
        {
            return Repository.FirstOrDefault( t => t.PreviousPersonId == previousPersonId );
        }

        /// <summary>
        /// Gets a <see cref="Rock.Model.PersonMerged"/> entity based on a <see cref="Rock.Model.Person">Person's</see> previous Guid.
        /// </summary>
        /// <param name="previousPersonGuid">A <see cref="System.Guid"/> representing the previous Guid identifier of the <see cref="Rock.Model.Person"/>.</param>
        /// <returns>A <see cref="Rock.Model.PersonMrged"/> entity that contains the <see cref="Rock.Model.Person">Person's</see> new identifiers.</returns>
        public PersonMerged GetNew( Guid previousPersonGuid )
        {
            return Repository.FirstOrDefault( t => t.PreviousPersonGuid == previousPersonGuid );
        }

        /// <summary>
        /// Gets a <see cref="Rock.Model.PersonMerged"/> entity by a <see cref="Rock.Model.Person">Person's</see> previous encrypted key value.
        /// </summary>
        /// <param name="encryptedKey">A <see cref="System.String"/> containing a <see cref="Rock.Model.Person">Person's</see> previous encrypted key value.</param>
        /// <returns>A <see cref="Rock.Model.PersonMerged"/> entity that contains the <see cref="Rock.Model.Person">Person's</see> new identifiers.</returns>
        public virtual PersonMerged GetNewByPreviousEncryptedKey( string encryptedKey )
        {
            string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            return GetNewByPreviousPublicKey( publicKey );
        }

        /// <summary>
        /// Gets a <see cref="Rock.Model.PersonMerged"/> entity by a <see cref="Rock.Model.Person">Person's</see> previous public key value.
        /// </summary>
        /// <param name="publicKey">A <see cref="System.String"/> containing a <see cref="Rock.Model.Person">Person's</see> previous public key value.</param>
        /// <returns>A <see cref="Rock.Model.PersonMerged"/> entity that contains a <see cref="Rock.Model.Person">Person's</see> new identifiers.</returns>
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
        /// Gets a <see cref="Rock.Model.Person">Person's</see> current public key by their previous public key value.
        /// </summary>
        /// <param name="publicKey">A <see cref="System.String"/> containing the <see cref="Rock.Model.Person">Person's</see> previous public key.</param>
        /// <returns>A <see cref="System.String"/> representing the <see cref="Rock.Model.Person">Person's</see> current public key value.</returns>
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
        /// Gets a <see cref="Rock.Model.Person">Person's</see> current PersonId by their previous PersonId.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32" /> representing a <see cref="Rock.Model.Person">Person's</see> previous PersonId.</param>
        /// <returns>A <see cref="System.Int32"/> representing a <see cref="Rock.Model.Person">Person's</see> current PersonId.</returns>
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
        /// Gets a <see cref="Rock.Model.Person">Person's</see> current Guid by their previous Guid.
        /// </summary>
        /// <param name="personGuid">A <see cref="System.Int32"/> representing a <see cref="Rock.Model.Person">Person's</see> previous Guid.</param>
        /// <returns>A <see cref="Rock.Model.Guid"/> representing a <see cref="Rock.Model.Person">Person's</see> new Guid.</returns>
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
