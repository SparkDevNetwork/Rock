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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.PersonSearchKey"/> entity types
    /// </summary>
    public partial class PersonSearchKeyService
    {
        private static readonly Object _obj = new object();

        /// <summary>
        /// Generates a random alternate Id search value for use in a
        /// <see cref="Rock.Model.PersonSearchKey" />.  It is comprised of random alpha
        /// numeric characters in the form ccccccc-ccccccc (7 random characters, a dash,
        /// and 7 more random characters). Example "f5f3df2-40b8946".
        /// </summary>
        /// <param name="verifyUnique">if set to <c>true</c> the key will be verified as unique across all existing "Alternate Id" search values.</param>
        /// <returns>
        /// A random key string
        /// </returns>
        public static string GenerateRandomAlternateId( bool verifyUnique = true )
        {
            return GenerateRandomAlternateId( verifyUnique, null );
        }

        /// <summary>
        /// Generates a random alternate Id search value for use in a
        /// <see cref="Rock.Model.PersonSearchKey" />.  It is comprised of random alpha
        /// numeric characters in the form ccccccc-ccccccc (7 random characters, a dash,
        /// and 7 more random characters). Example "f5f3df2-40b8946".
        /// </summary>
        /// <param name="verifyUnique">if set to <c>true</c> the key will be verified as unique across all existing "Alternate Id" search values.</param>
        /// <param name="rockContext">The rock context.  You MUST pass in a RockContext if this is going to be called from inside a WrapTransaction save.</param>
        /// <returns>
        /// A random key string
        /// </returns>
        public static string GenerateRandomAlternateId( bool verifyUnique = true, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            string key = string.Empty;

            if ( verifyUnique )
            {
                lock ( _obj )
                {
                    int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                    var service = new PersonSearchKeyService( rockContext );
                    do
                    {
                        key = GenerateRandomAlternateId();
                    } while ( service.Queryable().AsNoTracking().Any( a => a.SearchTypeValueId == alternateValueId && a.SearchValue == key ) );
                }
            }
            else
            {
                key = GenerateRandomAlternateId();
            }

            return key;
        }

        /// <summary>
        /// Generates a random alternate Id search value for use in a
        /// <see cref="Rock.Model.PersonSearchKey"/>.  It is comprised of random alpha
        /// numeric characters in the form ccccccc-ccccccc (7 random characters, a dash,
        /// and 7 more random characters). Example "f5f3df2-40b8946".
        /// </summary>
        /// <returns>A random key string</returns>
        public static string GenerateRandomAlternateId()
        {
            string randomId = Guid.NewGuid().ToString( "N" );
            return string.Format( "{0}-{1}", randomId.Substring( 0, 7 ), randomId.Substring( 7, 7 ) );
        }

    }
}
