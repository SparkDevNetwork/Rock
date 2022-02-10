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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class PersonToken
    {
        #region Methods

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias )
        {
            return CreateNew( personAlias, null, null, null );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person limited to a specific PageId
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias, int pageId )
        {
            return CreateNew( personAlias, null, null, pageId );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string CreateNew( PersonAlias personAlias, DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            if ( personAlias == null )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var person = personAlias.Person;
                if ( person == null )
                {
                    person = new PersonService( rockContext ).Get( personAlias.PersonId );
                }

                if ( person == null )
                {
                    return null;
                }
                // If a token is disallowed by security settings, return an error message.
                if ( !person.IsPersonTokenUsageAllowed() )
                {
                    return "TokenProhibited";
                }

                var token = Rock.Security.Encryption.GenerateUniqueToken();

                PersonToken personToken = new PersonToken();
                personToken.PersonAliasId = personAlias.Id;
                personToken.Token = token;
                if ( expireDateTime != null )
                {
                    personToken.ExpireDateTime = expireDateTime;
                }
                else
                {
                    int? tokenExpireMinutes = GlobalAttributesCache.Get().GetValue( "core.PersonTokenExpireMinutes" ).AsIntegerOrNull();
                    if ( tokenExpireMinutes.HasValue )
                    {
                        personToken.ExpireDateTime = RockDateTime.Now.AddMinutes( tokenExpireMinutes.Value );
                    }
                    else
                    {
                        personToken.ExpireDateTime = null;
                    }
                }

                personToken.TimesUsed = 0;
                personToken.UsageLimit = usageLimit ?? GlobalAttributesCache.Get().GetValue( "core.PersonTokenUsageLimit" ).AsIntegerOrNull();

                personToken.PageId = pageId;

                var personTokenService = new PersonTokenService( rockContext );
                personTokenService.Add( personToken );
                rockContext.SaveChanges( true );

                var encryptedToken = Rock.Security.Encryption.EncryptString( token );

                // do a Replace('%', '!') after we UrlEncode it (to make it more safely embeddable in HTML and cross browser compatible)
                // Note: HttpUtility is NET Core safe despite being in the System.Web
                // namespace. Even still it might be best to replace this with
                // an extension method someday.
                return System.Web.HttpUtility.UrlEncode( encryptedToken ).Replace( '%', '!' );
            }
        }

        #endregion
    }
}
