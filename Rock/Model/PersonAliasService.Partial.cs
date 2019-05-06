// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.PersonAlias"/> entity type objects.
    /// </summary>
    public partial class PersonAliasService
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override PersonAlias Get( int id )
        {
            return Queryable( "Person" ).FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public override PersonAlias Get( Guid guid )
        {
            return Queryable( "Person" ).FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets the primary alias.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetPrimaryAlias( int personId )
        {
            return Queryable( "Person" )
                .FirstOrDefault( a => a.PersonId == personId && a.AliasPersonId == personId );
        }

        /// <summary>
        /// Gets the primary alias identifier.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public virtual int? GetPrimaryAliasId( int personId )
        {
            return this.Queryable().Where( a => a.PersonId == personId && a.AliasPersonId == personId ).Select( a => a.Id ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public virtual int? GetPersonId( int personAliasId )
        {
            var personAlias = Get( personAliasId );
            if ( personAlias != null )
            {
                return personAlias.PersonId;
            }
            return null;
        }

        /// <summary>
        /// Gets the PersonAlias the by AliasPersonId
        /// </summary>
        /// <param name="aliasPersonId">The alias person identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasId( int aliasPersonId )
        {
            var personAlias = Queryable( "Person" ).Where( a => a.AliasPersonId == aliasPersonId ).FirstOrDefault();
            if ( personAlias != null )
            {
                return personAlias;
            }
            else
            {
                // If the personId is valid, there should be a personAlias with the AliasPersonID equal 
                // to that personId.  If there isn't for some reason, create it now.
                var person = new PersonService( (RockContext)this.Context ).Get( aliasPersonId );
                if ( person != null )
                {
                    personAlias = new PersonAlias();
                    personAlias.Guid = Guid.NewGuid();
                    personAlias.AliasPersonId = person.Id;
                    personAlias.AliasPersonGuid = person.Guid;
                    personAlias.PersonId = person.Id;

                    // Use a different context so calling method's changes are not yet saved
                    var rockContext = new RockContext();
                    new PersonAliasService( rockContext ).Add( personAlias );
                    rockContext.SaveChanges();

                    return Get( personAlias.Id );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the by alias unique identifier.
        /// </summary>
        /// <param name="aliasPersonGuid">The alias person unique identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasGuid( Guid aliasPersonGuid )
        {
            var personAlias = Queryable( "Person" ).Where( a => a.AliasPersonGuid == aliasPersonGuid ).FirstOrDefault();
            if ( personAlias != null )
            {
                return personAlias;
            }
            else
            {
                // If the personId is valid, there should be a personAlias with the AliasPersonID equal 
                // to that personId.  If there isn't for some reason, create it now.
                var person = new PersonService( (RockContext)this.Context ).Get( aliasPersonGuid );
                if ( person != null )
                {
                    personAlias = new PersonAlias();
                    personAlias.Guid = Guid.NewGuid();
                    personAlias.AliasPersonId = person.Id;
                    personAlias.AliasPersonGuid = person.Guid;
                    personAlias.PersonId = person.Id;

                    // Use a different context so calling method's changes are not yet saved
                    var rockContext = new RockContext();
                    new PersonAliasService( rockContext ).Add( personAlias );
                    rockContext.SaveChanges();

                    return Get( personAlias.Id );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the by encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasEncryptedKey( string encryptedKey )
        {
            if ( encryptedKey.IsNotNullOrWhiteSpace() )
            {
                string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
                return GetByAliasPublicKey( publicKey );
            }

            return null;
        }

        /// <summary>
        /// Gets the by public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasPublicKey( string publicKey )
        {
            try
            {
                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = Int32.Parse( idParts[0] );
                    Guid guid = new Guid( idParts[1] );

                    PersonAlias personAlias = GetByAliasId( id );
                    if ( personAlias != null && personAlias.AliasPersonGuid.HasValue && personAlias.AliasPersonGuid.Value.CompareTo( guid ) == 0 )
                    {
                        return personAlias;
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
        /// Gets the person.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        /// <returns></returns>
        public Person GetPerson (Guid personAliasGuid)
        {
            return Queryable()
                .Where( a => a.Guid.Equals( personAliasGuid ) )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public Person GetPerson( int personAliasId )
        {
            return Queryable()
                .Where( a => a.Id.Equals( personAliasId ) )
                .Select( a => a.Person )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        public Person GetPersonNoTracking( int personAliasId )
        {
            return Queryable()
                .Where( a => a.Id.Equals( personAliasId ) )
                .Select( a => a.Person )
                .AsNoTracking()
                .FirstOrDefault();
        }
    }
}
