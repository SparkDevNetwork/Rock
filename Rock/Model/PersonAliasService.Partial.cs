// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
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
        /// Gets the PersonAlias the by alias identifier.
        /// </summary>
        /// <param name="aliasPersonId">The alias person identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasId( int aliasPersonId)
        {
            return Queryable("Person").Where( a => a.AliasPersonId == aliasPersonId ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the by alias unique identifier.
        /// </summary>
        /// <param name="aliasPersonGuid">The alias person unique identifier.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasGuid( Guid aliasPersonGuid )
        {
            return Queryable("Person").Where( a => a.AliasPersonGuid == aliasPersonGuid ).FirstOrDefault();
        }        
        
        /// <summary>
        /// Gets the by encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public virtual PersonAlias GetByAliasEncryptedKey( string encryptedKey )
        {
            string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            return GetByAliasPublicKey( publicKey );
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

                    if ( personAlias != null && personAlias.AliasPersonGuid.CompareTo( guid ) == 0 )
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

    }
}
