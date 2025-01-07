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

using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using Rock.Common.Tv;
using Rock.Data;
using Rock.Model;
using Rock.Tv.Classes;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;

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

            // If no userlogin, make a new one. This really should not be possible as they should have just logged in, but just
            // in case.
            if ( username.IsNullOrWhiteSpace() )
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

        #region Roku

        /// <summary>
        /// Gets the Roku page component.
        /// </summary>
        /// <param name="pageGuid"></param>
        /// <param name="sceneGraph"></param>
        /// <param name="rockComponents"></param>
        /// <returns></returns>
        public static ByteArrayContent GetPageAsRokuComponentLibrary( Guid pageGuid, string sceneGraph, string rockComponents )
        {
            var manifestText = $@"title=RockPage
subtitle=SceneGraph Component For Rock Page
major_version=1
minor_version=1
build_version=00001
sg_component_libs_provided=RockPage";

            sceneGraph = WrapSceneGraphInRockComponent( sceneGraph, pageGuid.ToString() );

            using ( MemoryStream memoryStream = new MemoryStream() )
            {
                using ( ZipArchive zip = new ZipArchive( memoryStream, ZipArchiveMode.Create, true ) )
                {
                    var sceneGraphEntry = zip.CreateEntry( $"components/{pageGuid}.xml" );

                    using ( var writer = new StreamWriter( sceneGraphEntry.Open() ) )
                    {
                        writer.Write( sceneGraph );
                    }

                    var manifestEntry = zip.CreateEntry( "manifest" );
                    using ( var writer = new StreamWriter( manifestEntry.Open() ) )
                    {
                        writer.Write( manifestText );
                    }

                    foreach ( var rockComponent in GetRockPageComponents( rockComponents ) )
                    {
                        var rockComponentEntry = zip.CreateEntry( $"components/{rockComponent.Key}.xml" );

                        using ( var writer = new StreamWriter( rockComponentEntry.Open() ) )
                        {
                            writer.Write( rockComponent.Value );
                        }
                    }
                }
                memoryStream.Seek( 0, SeekOrigin.Begin );

                var memoryStreamContent = new ByteArrayContent( memoryStream.ToArray() );
                return memoryStreamContent;
            }
        }

        /// <summary>
        /// Gets each individual Rock component from the Rock components string.
        /// These components are uniquely split by the ###COMPONENT>NAME tag.
        /// </summary>
        /// <param name="rockComponents"></param>
        /// <returns></returns>
        /// <remarks>This method is pretty much useless for anything besides Roku.</remarks>
        private static Dictionary<string, string> GetRockPageComponents( string rockComponents )
        {
            var rockPageComponents = new Dictionary<string, string>();

            // We split these components by the ###COMPONENT>NAME tag.
            // This means that we need to split by '###COMPONENT' and then
            // get a substring of the name from the preceding '>' character.
            // Then we need to trim the string up to the trailing "###" characters.

            foreach ( var rockComponent in rockComponents.Split( new string[] { "###COMPONENT" }, StringSplitOptions.None ) )
            {
                // Split the string by the trailing '###" characters.
                var rockComponentNameAndContent = rockComponent.Split( new string[] { "###" }, StringSplitOptions.None );

                // Invalid component, skip it.
                if ( rockComponentNameAndContent.Length < 2 || !rockComponentNameAndContent[0].StartsWith( ">" ) )
                {
                    continue;
                }

                // Remove the '>' character from the beginning of the string.
                var rockComponentName = rockComponentNameAndContent[0].Substring( 1 ).Trim();
                var rockComponentContent = rockComponentNameAndContent[1].Trim();

                rockComponentContent = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
{rockComponentContent}";

                rockPageComponents.Add( rockComponentName, rockComponentContent );
            }

            return rockPageComponents;
        }

        /// <summary>
        /// Wraps the scene graph in a Rock component tag with information
        /// about the page.
        /// </summary>
        /// <param name="sceneGraph"></param>
        /// <param name="pageGuid"></param>
        /// <returns></returns>
        public static string WrapSceneGraphInRockComponent( string sceneGraph, string pageGuid )
        {
            sceneGraph = sceneGraph.Trim();

            sceneGraph = $@"<component name=""{pageGuid}"" extends=""Group"">
<children>
{sceneGraph}
</children>
</component>
";

            sceneGraph = PrependXmlDeclaration( sceneGraph );

            return sceneGraph;
        }

        /// <summary>
        /// Adds an XML declaration to the beginning of the XML string if it does not already exist.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string PrependXmlDeclaration( string xml )
        {
            var xmlDeclaration = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            var needsXmlDeclaration = !xml.Contains( "<?xml" );

            if ( needsXmlDeclaration )
            {
                xml = $@"{xmlDeclaration}
{xml}";
            }

            return xml;
        }

        #endregion
    }
}