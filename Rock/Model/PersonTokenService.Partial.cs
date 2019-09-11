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
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.PersonToken"/> entity objects.
    /// </summary>
    public partial class PersonTokenService
    {
        /// <summary>
        /// Gets the PersonToken by impersonation token (rckipid)
        /// </summary>
        /// <param name="impersonationToken">The impersonation token.</param>
        /// <returns></returns>
        public PersonToken GetByImpersonationToken( string impersonationToken )
        {
            // the impersonationToken should normally be a UrlEncoded string, but it is possible that the caller already UrlDecoded it, so first try without UrlDecoding it
            var decryptedToken = Rock.Security.Encryption.DecryptString( impersonationToken );

            if ( decryptedToken == null )
            {
                // do a Replace('!', '%') on the token before UrlDecoding because we did a Replace('%', '!') after we UrlEncoded it (to make it embeddable in HTML and cross browser compatible)
                string urlDecodedKey = System.Web.HttpUtility.UrlDecode( impersonationToken.Replace( '!', '%' ) );
                decryptedToken = Rock.Security.Encryption.DecryptString( urlDecodedKey );
            }

            var personToken = this.Queryable().FirstOrDefault( a => a.Token == decryptedToken );
            if ( personToken == null )
            {
                bool tokenUseLegacyFallback = GlobalAttributesCache.Get().GetValue( "core.PersonTokenUseLegacyFallback" ).AsBoolean();
                if ( tokenUseLegacyFallback )
                {
                    var person = new PersonService( this.Context as Rock.Data.RockContext ).GetByLegacyEncryptedKey( impersonationToken, true );
                    if ( person != null )
                    {
                        // if LegacyFallback is enabled, and we found a person, create a fake PersonToken
                        personToken = new PersonToken
                        {
                            PersonAlias = person.PrimaryAlias,
                            ExpireDateTime = null,
                            PageId = null,
                            LastUsedDateTime = RockDateTime.Now,
                            UsageLimit = null
                        };
                    }
                }
            }

            return personToken;
        }
    }
}
