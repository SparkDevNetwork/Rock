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
using System.Web;
using Rock.Common.Tv;
using Rock.Data;
using Rock.Model;
using Rock.Tv.Classes;
using Rock.Web.Cache;

namespace Rock.Tv
{
    /// <summary>
    /// Utility class for various helper methods.
    /// </summary>
    public static class TvHelper
    {
        /// <summary>
        /// Gets the current application site.
        /// </summary>
        /// <param name="validateApiKey">if set to <c>true</c> [validate API key].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static SiteCache GetCurrentApplicationSite( bool validateApiKey = true, RockContext rockContext = null )
        {
            var appId = HttpContext.Current?.Request?.Headers?["X-Rock-App-Id"];

            if ( !appId.AsIntegerOrNull().HasValue )
            {
                return null;
            }

            //
            // Lookup the site from the App Id.
            //
            var site = SiteCache.Get( appId.AsInteger() );
            if ( site == null )
            {
                return null;
            }

            //
            // If we have been requested to validate the Api Key then do so.
            //
            if ( validateApiKey )
            {
                var requestApiKey = System.Web.HttpContext.Current?.Request?.Headers?["X-Rock-Tv-Api-Key"];
                var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AppleTvApplicationSettings>();

                //
                // Ensure we have valid site configuration.
                //
                if ( additionalSettings == null || !additionalSettings.ApiKeyId.HasValue )
                {
                    return null;
                }

                rockContext = rockContext ?? new RockContext();

                // Get user login for the app and verify that it matches the request's key
                var appUserLogin = new UserLoginService( rockContext ).Get( additionalSettings.ApiKeyId.Value );

                if ( appUserLogin != null && appUserLogin.ApiKey == requestApiKey )
                {
                    return site;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return site;
            }
        }


        /// <summary>
        /// Gets the TV person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static TvPerson GetTvPerson( Person person )
        {
            var baseUrl = GlobalAttributesCache.Value( "PublicApplicationRoot" );
            var homePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id;
            var mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
            var alternateIdTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID ).Id;

            var alternateId = person.GetPersonSearchKeys()
                .Where( a => a.SearchTypeValueId == alternateIdTypeId )
                .FirstOrDefault()?.SearchValue;

            return new TvPerson
            {
                FirstName = person.FirstName,
                NickName = person.NickName,
                LastName = person.LastName,
                Gender = ( Rock.Common.Tv.Enum.Gender ) person.Gender,
                BirthDate = person.BirthDate,
                Email = person.Email,
                HomePhone = person.PhoneNumbers.Where( p => p.NumberTypeValueId == homePhoneTypeId ).Select( p => p.NumberFormatted ).FirstOrDefault(),
                MobilePhone = person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobilePhoneTypeId ).Select( p => p.NumberFormatted ).FirstOrDefault(),
                HomeAddress = GetTvAddress( person.GetHomeLocation() ),
                CampusGuid = person.GetCampus()?.Guid,
                PersonAliasId = person.PrimaryAliasId.Value,
                PhotoUrl = ( person.PhotoId.HasValue ? $"{baseUrl}{person.PhotoUrl}" : null ),
                PersonGuid = person.Guid,
                PersonId = person.Id,
                AlternateId = alternateId,
                AuthToken = TvHelper.GetAuthenticationTokenFromPerson( person )
            };
        }

        /// <summary>
        /// Gets the TV address.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static TvAddress GetTvAddress( Location location )
        {
            if ( location == null )
            {
                return null;
            }

            return new TvAddress
            {
                Street1 = location.Street1,
                City = location.City,
                State = location.State,
                PostalCode = location.PostalCode,
                Country = location.Country
            };
        }

        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static string GetAuthenticationTokenFromPerson( Person person )
        {
            var username = person.Users.FirstOrDefault( a => ( a.IsConfirmed ?? true ) && !( a.IsLockedOut ?? false ) )?.UserName;

            // If no userlogin, make a new one. This really should nto be possible as they should have just logged in, but just
            // in case.
            if ( username.IsNotNullOrWhiteSpace() )
            {
                var password = System.Web.Security.Membership.GeneratePassword( 12, 1 );
                username = Rock.Security.Authentication.Database.GenerateUsername( person.NickName, person.LastName );

                var userLogin = UserLoginService.Create(
                                new RockContext(),
                                person,
                                AuthenticationServiceType.Internal,
                                EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                username,
                                password,
                                true );
            }

            return GetAuthenticationTokenFromUsername( username );
        }

        /// <summary>
        /// Gets the authentication token from username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public static string GetAuthenticationTokenFromUsername( string username )
        {
            // Get the auth ticket from the username
            var ticket = new System.Web.Security.FormsAuthenticationTicket( 1,
                username,
                RockDateTime.Now,
                RockDateTime.Now.Add( System.Web.Security.FormsAuthentication.Timeout ),
                true,
                username.StartsWith( "rckipid=" ).ToString() );

            return System.Web.Security.FormsAuthentication.Encrypt( ticket );
        }
    }
}
