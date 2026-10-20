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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Blocks.Types.Mobile.Cms;
using Rock.Common.Mobile.Enums;
using Rock.Common.Mobile.Security.Authentication;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Security
{
    /// <summary>
    /// Tests for the external authentication person matching done by the
    /// mobile Log In block.
    /// </summary>
    [TestClass]
    public class MobileLoginExternalAuthenticationTests : DatabaseTestsBase
    {
        [TestMethod]
        public void GetOrCreatePerson_UnverifiedEmailCreatesNewPerson()
        {
            var testPerson = AddTestPerson();
            var personInfo = CreateUserInfo( testPerson, false );
            var userLoginInfo = CreateUserLoginInfo( personInfo.ForeignKey );

            var userLogin = Login.GetOrCreatePersonFromExternalAuthenticationUserInfo( personInfo, userLoginInfo, new RockContext(), null, null, true );

            Assert.IsNotNull( userLogin );
            Assert.AreNotEqual( testPerson.Id, userLogin.PersonId );
            Assert.AreEqual( 0, GetLoginCount( testPerson.Id ) );
        }

        [TestMethod]
        public void GetOrCreatePerson_VerifiedEmailLinksExistingPerson()
        {
            var testPerson = AddTestPerson();
            var personInfo = CreateUserInfo( testPerson, true );
            var userLoginInfo = CreateUserLoginInfo( personInfo.ForeignKey );

            var userLogin = Login.GetOrCreatePersonFromExternalAuthenticationUserInfo( personInfo, userLoginInfo, new RockContext(), null, null, true );

            Assert.IsNotNull( userLogin );
            Assert.AreEqual( testPerson.Id, userLogin.PersonId );
        }

        [TestMethod]
        public void GetOrCreatePerson_UnverifiedEmailLinksExistingPersonWhenSettingIsOff()
        {
            var testPerson = AddTestPerson();
            var personInfo = CreateUserInfo( testPerson, false );
            var userLoginInfo = CreateUserLoginInfo( personInfo.ForeignKey );

            var userLogin = Login.GetOrCreatePersonFromExternalAuthenticationUserInfo( personInfo, userLoginInfo, new RockContext(), null, null, false );

            Assert.IsNotNull( userLogin );
            Assert.AreEqual( testPerson.Id, userLogin.PersonId );
        }

        /// <summary>
        /// Creates the user information the external provider would return
        /// for the specified person.
        /// </summary>
        /// <param name="person">The person whose names and email are used.</param>
        /// <param name="emailVerified">The email verified value from the provider.</param>
        /// <returns>The user information bag.</returns>
        private static ExternalAuthenticationUserInfoBag CreateUserInfo( Person person, bool emailVerified )
        {
            return new ExternalAuthenticationUserInfoBag
            {
                ForeignKey = System.Guid.NewGuid().ToString( "N" ),
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                EmailVerified = emailVerified,
                Provider = SupportedAuthenticationProvider.Auth0
            };
        }

        /// <summary>
        /// Creates the Auth0 user login information for the foreign key.
        /// </summary>
        /// <param name="foreignKey">The foreign key from the provider.</param>
        /// <returns>The user login information bag.</returns>
        private static Login.ExternalAuthUserLoginBag CreateUserLoginInfo( string foreignKey )
        {
            return new Login.ExternalAuthUserLoginBag
            {
                Username = "AUTH0_" + foreignKey,
                ExternalPass = "auth0",
                // The Auth0 component is not registered in the test database. Any
                // registered authentication component works here because the value
                // is only stored as the UserLogin provider.
                ProviderEntityTypeId = EntityTypeCache.Get( typeof( Rock.Security.ExternalAuthentication.OidcClient ) ).Id
            };
        }

        /// <summary>
        /// Gets the number of logins the person has.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>The number of logins.</returns>
        private static int GetLoginCount( int personId )
        {
            return new UserLoginService( new RockContext() ).Queryable().Count( ul => ul.PersonId == personId );
        }

        /// <summary>
        /// Adds a test person with a unique email and no login.
        /// </summary>
        /// <returns>The new person.</returns>
        private static Person AddTestPerson()
        {
            var person = new Person
            {
                Email = $"{System.Guid.NewGuid().ToString( "N" )}@fakeinbox.com",
                FirstName = "Test",
                LastName = "User"
            };

            var rockContext = new RockContext();
            new PersonService( rockContext ).Add( person );
            rockContext.SaveChanges();

            return person;
        }
    }
}
