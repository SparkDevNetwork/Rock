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

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core.Model
{
    [TestClass]
    public class UserLoginTests : DatabaseTestsBase
    {
        [TestMethod]
        public void PostSave_ShouldUpdatePersonAccountProtectionProfileToMedium()
        {
            var personGuid = Guid.NewGuid();
            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid
            };

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.AreEqual( AccountProtectionProfile.Low, person.AccountProtectionProfile );

                var userLogin = new UserLogin
                {
                    UserName = personGuid.ToString(),
                    Password = "$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C",
                    PersonId = person.Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
                };

                var userLoginService = new UserLoginService( rockContext );
                userLoginService.Add( userLogin );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.AreEqual( AccountProtectionProfile.Medium, person.AccountProtectionProfile );
            }
        }

        [TestMethod]
        [DataRow( AccountProtectionProfile.High )]
        [DataRow( AccountProtectionProfile.Extreme )]
        public void PostSave_ShouldNotUpdatePersonAccountProtectionProfileToMedium( AccountProtectionProfile expectedAccountProtectionProfile )
        {
            var personGuid = Guid.NewGuid();

            var person = new Person
            {
                FirstName = "Test",
                LastName = personGuid.ToString(),
                Email = $"{personGuid}@test.com",
                Guid = personGuid,
                AccountProtectionProfile = ( AccountProtectionProfile ) expectedAccountProtectionProfile
            };

            using ( var rockContext = RockApp.Current.CreateRockContext() )
            {
                var personService = new PersonService( rockContext );
                personService.Add( person );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.AreEqual( ( AccountProtectionProfile ) expectedAccountProtectionProfile, person.AccountProtectionProfile );

                var userLogin = new UserLogin
                {
                    UserName = personGuid.ToString(),
                    Password = "$2a$11$XTLibmiVyu6SArCqLSSi5OQO3tA8cuMWgPVNIfylx5bICaniAfP5C",
                    PersonId = person.Id,
                    EntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
                };

                var userLoginService = new UserLoginService( rockContext );
                userLoginService.Add( userLogin );
                rockContext.SaveChanges();

                person = personService.Get( person.Id );
                Assert.AreEqual( ( AccountProtectionProfile ) expectedAccountProtectionProfile, person.AccountProtectionProfile );
            }
        }

        #region GetByConfirmationCode

        [TestMethod]
        [DataRow( 0, 30, true, DisplayName = "Issued 30 minutes ago" )]
        [DataRow( 1, 55, true, DisplayName = "Issued 1 hour 55 minutes ago" )]
        [DataRow( 0, -5, true, DisplayName = "Issued 5 minutes in the future" )]
        [DataRow( 3, 0, false, DisplayName = "Issued 3 hours ago" )]
        [DataRow( 24, 30, false, DisplayName = "Issued 24 hours 30 minutes ago" )]
        [DataRow( 721, 0, false, DisplayName = "Issued 30 days 1 hour ago" )]
        public void GetByConfirmationCode_ShouldEnforceCodeLifetime( int hoursAgo, int minutesAgo, bool expectValid )
        {
            var userLoginId = CreateTestLogin( true, true );
            var code = BuildConfirmationCode( userLoginId, RockDateTime.Now.AddHours( -hoursAgo ).AddMinutes( -minutesAgo ) );

            var result = GetByConfirmationCode( code );

            if ( expectValid )
            {
                Assert.IsNotNull( result );
                Assert.AreEqual( userLoginId, result.Id );
            }
            else
            {
                Assert.IsNull( result );
            }
        }

        [TestMethod]
        public void GetByConfirmationCode_WithZeroTicks_ShouldReturnNull()
        {
            var userLoginId = CreateTestLogin( true, true );
            var code = BuildConfirmationCode( userLoginId, new DateTime( 0 ) );

            Assert.IsNull( GetByConfirmationCode( code ) );
        }

        [TestMethod]
        public void GetByConfirmationCode_AfterPasswordChange_ShouldRejectOlderCode()
        {
            var userLoginId = CreateTestLogin( true, true );
            var code = BuildConfirmationCode( userLoginId, RockDateTime.Now.AddMinutes( -10 ) );

            Assert.IsNotNull( GetByConfirmationCode( code ) );

            ChangePassword( userLoginId );

            Assert.IsNull( GetByConfirmationCode( code ) );
        }

        [TestMethod]
        public void GetByConfirmationCode_AfterPasswordChange_ShouldAcceptNewCode()
        {
            var userLoginId = CreateTestLogin( true, true );

            ChangePassword( userLoginId );

            var code = BuildConfirmationCode( userLoginId, RockDateTime.Now );

            Assert.IsNotNull( GetByConfirmationCode( code ) );
        }

        [TestMethod]
        public void GetByConfirmationCode_WithNullLastPasswordChange_ShouldAcceptCode()
        {
            var userLoginId = CreateTestLogin( true, false );

            using ( var rockContext = new RockContext() )
            {
                var userLogin = new UserLoginService( rockContext ).Get( userLoginId );
                userLogin.LastPasswordChangedDateTime = null;
                rockContext.SaveChanges();
            }

            var code = BuildConfirmationCode( userLoginId, RockDateTime.Now.AddMinutes( -30 ) );

            Assert.IsNotNull( GetByConfirmationCode( code ) );
        }

        [TestMethod]
        public void GetByConfirmationCode_ForNewUnconfirmedLogin_ShouldAcceptCodeIssuedAtCreation()
        {
            var userLoginId = CreateTestLogin( false, false );
            var code = BuildConfirmationCode( userLoginId, RockDateTime.Now );

            Assert.IsNotNull( GetByConfirmationCode( code ) );
        }

        /// <summary>
        /// Creates a test person with a database login and returns the login identifier.
        /// </summary>
        private static int CreateTestLogin( bool isConfirmed, bool backdatePasswordChange )
        {
            var personGuid = Guid.NewGuid();

            using ( var rockContext = new RockContext() )
            {
                var person = new Person
                {
                    FirstName = "TH021",
                    LastName = "Test",
                    Email = $"{personGuid}@test.com",
                    Guid = personGuid
                };

                new PersonService( rockContext ).Add( person );
                rockContext.SaveChanges();

                var userLogin = UserLoginService.Create( rockContext,
                    person,
                    AuthenticationServiceType.Internal,
                    EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE ).Id,
                    $"th021-test-{personGuid}",
                    "TestPassword1!",
                    isConfirmed,
                    false );

                // Create stamps the password change date as now, so move it back
                // to allow codes issued in the past to be tested.
                if ( backdatePasswordChange )
                {
                    userLogin.LastPasswordChangedDateTime = RockDateTime.Now.AddDays( -60 );
                    rockContext.SaveChanges();
                }

                return userLogin.Id;
            }
        }

        /// <summary>
        /// Builds a confirmation code the same way UserLogin.ConfirmationCode does, but with a chosen issue time.
        /// </summary>
        private static string BuildConfirmationCode( int userLoginId, DateTime issued )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLogin = new UserLoginService( rockContext ).Get( userLoginId );

                Rock.Security.Encryption.TryEncryptString( $"ROCK|{userLogin.EncryptedKey}|{userLogin.UserName}|{issued.Ticks}", out var code );

                return code;
            }
        }

        /// <summary>
        /// Validates the code using a new context so the stored login row is read.
        /// </summary>
        private static UserLogin GetByConfirmationCode( string code )
        {
            using ( var rockContext = new RockContext() )
            {
                return new UserLoginService( rockContext ).GetByConfirmationCode( code );
            }
        }

        /// <summary>
        /// Changes the login's password the same way the Confirm Account block does.
        /// </summary>
        private static void ChangePassword( int userLoginId )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.Get( userLoginId );

                userLoginService.SetPassword( userLogin, "TestPassword2!" );
                rockContext.SaveChanges();
            }
        }

        #endregion
    }
}
