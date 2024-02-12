using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using IdentityModel;

using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Security.ExternalAuthentication;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Security
{
    [TestClass]
    public class OidcClientTests : DatabaseTestsBase
    {
        [TestMethod]
        public void HandleOidcUserAddUpdate_ExistingPersonShouldBeBoundToNewUser()
        {
            var expectedPerson = AddTestPerson();
            var userId = System.Guid.NewGuid().ToString( "N" );
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.Email, expectedPerson.Email),
                    new Claim(JwtClaimTypes.GivenName, expectedPerson.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, expectedPerson.LastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( expectedPerson.Email, actualUserLogin.Person.Email );
            Assert.That.AreEqual( expectedPerson.FirstName, actualUserLogin.Person.FirstName );
            Assert.That.AreEqual( expectedPerson.LastName, actualUserLogin.Person.LastName );
        }

        [TestMethod]
        public void HandleOidcUserAddUpdate_ExistingUserShouldBindCorrectly()
        {
            var expectedPerson = AddTestPerson();

            var expectedUserName = System.Guid.NewGuid().ToString( "N" );

            var typeId = EntityTypeCache.Get( typeof( OidcClient ) ).Id;
            var rockContext = new RockContext();
            var expectedUserLogin = UserLoginService.Create( rockContext, expectedPerson, AuthenticationServiceType.External, typeId, "OIDC_" + expectedUserName, "oidc", true );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, expectedUserName),
                    new Claim(JwtClaimTypes.Email, expectedPerson.Email),
                    new Claim(JwtClaimTypes.GivenName, expectedPerson.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, expectedPerson.LastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( expectedUserLogin.UserName, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedPerson.Email, actualUserLogin.Person.Email );
            Assert.That.AreEqual( expectedPerson.FirstName, actualUserLogin.Person.FirstName );
            Assert.That.AreEqual( expectedPerson.LastName, actualUserLogin.Person.LastName );
        }

        [TestMethod]
        public void HandleOidcUserAddUpdate_NewUserGetsCreatedWithCorrectData()
        {
            var expectedFirstName = System.Guid.NewGuid().ToString( "N" );
            var expectedLastName = System.Guid.NewGuid().ToString( "N" );
            var expectedMiddleName = System.Guid.NewGuid().ToString( "N" );
            var expectedNickName = System.Guid.NewGuid().ToString( "N" );
            var expectedEmailAddress = System.Guid.NewGuid().ToString( "N" ) + "@fakeinbox.com";
            var userId = System.Guid.NewGuid().ToString( "N" );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.Email, expectedEmailAddress),
                    new Claim(JwtClaimTypes.NickName, expectedNickName),
                    new Claim(JwtClaimTypes.GivenName, expectedFirstName),
                    new Claim(JwtClaimTypes.MiddleName, expectedMiddleName),
                    new Claim(JwtClaimTypes.FamilyName, expectedLastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( "OIDC_" + userId, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedEmailAddress.ToLower(), actualUserLogin.Person.Email.ToLower() );
            Assert.That.AreEqual( expectedFirstName.ToLower(), actualUserLogin.Person.FirstName.ToLower() );
            Assert.That.AreEqual( expectedMiddleName.ToLower(), actualUserLogin.Person.MiddleName.ToLower() );
            Assert.That.AreEqual( expectedLastName.ToLower(), actualUserLogin.Person.LastName.ToLower() );
            Assert.That.AreEqual( expectedNickName.ToLower(), actualUserLogin.Person.NickName.ToLower() );
        }

        [TestMethod]
        [DataRow( "+1 (555) 555-5555", "1", "5555555555", "" )]
        [DataRow( "+44 20 555-5555", "44", "205555555", "" )]
        [DataRow( "+1 555 555-5555", "1", "5555555555", "" )]
        [DataRow( "+1 555 5555555", "1", "5555555555", "" )]
        [DataRow( "1 555 5555555", "1", "5555555555", "" )]
        [DataRow( "5555555555", "1", "5555555555", "" )]
        [DataRow( "+1 (555) 555-5555;ext=1234", "1", "5555555555", "1234" )]
        [DataRow( "+1 555 555-5555;ext=1234", "1", "5555555555", "1234" )]
        [DataRow( "+1 555 5555555;ext=1234", "1", "5555555555", "1234" )]
        [DataRow( "1 555 5555555;ext=1234", "1", "5555555555", "1234" )]
        [DataRow( "5555555555;ext=1234", "1", "5555555555", "1234" )]
        [DataRow( "", "", "", "" )]
        public void HandleOidcUserAddUpdate_NewUserPhoneNumberIsCreatedCorrectly( string phoneNumber, string expectedCountryCode, string expectedPhoneNumber, string expectedExtension )
        {
            var expectedFirstName = System.Guid.NewGuid().ToString( "N" );
            var expectedLastName = System.Guid.NewGuid().ToString( "N" );
            var expectedEmailAddress = System.Guid.NewGuid().ToString( "N" ) + "@fakeinbox.com";
            var userId = System.Guid.NewGuid().ToString( "N" );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.Email, expectedEmailAddress),
                    new Claim(JwtClaimTypes.GivenName, expectedFirstName),
                    new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                    new Claim(JwtClaimTypes.FamilyName, expectedLastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( "OIDC_" + userId, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedEmailAddress.ToLower(), actualUserLogin.Person.Email.ToLower() );
            Assert.That.AreEqual( expectedFirstName.ToLower(), actualUserLogin.Person.FirstName.ToLower() );
            Assert.That.AreEqual( expectedLastName.ToLower(), actualUserLogin.Person.LastName.ToLower() );

            var defaultPhoneNumberType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            var actualPhoneNumber = actualUserLogin.Person.PhoneNumbers.Where( ph => ph.NumberTypeValueId == defaultPhoneNumberType.Id ).FirstOrDefault();

            if ( phoneNumber.IsNullOrWhiteSpace() )
            {
                Assert.That.IsNull( actualPhoneNumber );
                return;
            }

            Assert.That.IsNotNull( actualPhoneNumber );
            Assert.That.AreEqual( expectedCountryCode, actualPhoneNumber.CountryCode );
            Assert.That.AreEqual( expectedPhoneNumber, actualPhoneNumber.Number );
            Assert.That.AreEqual( expectedExtension, actualPhoneNumber.Extension );
        }

        [TestMethod]
        [DataRow( "1125 N 3rd Ave", "Street 2", "Phoenix", "AZ", "85003", "USA" )]
        [DataRow( "1125 N 3rd Ave", "", "Phoenix", "AZ", "85003", "USA" )]
        [DataRow( "", "", "Phoenix", "AZ", "85003", "USA" )]
        [DataRow( "", "", "", "AZ", "85003", "USA" )]
        [DataRow( "", "", "", "", "85003", "USA" )]
        [DataRow( "", "", "", "", "", "USA" )]
        [DataRow( "", "", "", "", "", "" )]
        [DataRow( "1125 N 3rd Ave", "", "", "", "", "" )]
        [DataRow( "", "", "Phoenix", "", "", "" )]
        [DataRow( "", "", "", "AZ", "", "" )]
        [DataRow( "", "", "", "", "85003", "" )]
        public void HandleOidcUserAddUpdate_NewUserAddressIsCreatedCorrectly( string street1, string street2, string city, string state, string postalCode, string country )
        {
            var countryProperty = country.IsNullOrWhiteSpace() ? string.Empty : $"\"country\":\"{ country }\",";
            var postalCodeProperty = postalCode.IsNullOrWhiteSpace() ? string.Empty : $"\"postal_code\":\"{ postalCode }\",";
            var stateProperty = state.IsNullOrWhiteSpace() ? string.Empty : $"\"region\":\"{ state }\",";
            var cityProperty = city.IsNullOrWhiteSpace() ? string.Empty : $"\"locality\":\"{ city }\",";
            var streetCodeProperty = street1.IsNullOrWhiteSpace() ? string.Empty : $@"""street_address"":""{street1 + ( street2.IsNotNullOrWhiteSpace() ? "\r\n" + street2 : "" )}"",";

            var addressJson = $@"{{
                {streetCodeProperty}
                {cityProperty}
                {stateProperty}
                {postalCodeProperty}
                {countryProperty}
            }}";

            var expectedFirstName = System.Guid.NewGuid().ToString( "N" );
            var expectedLastName = System.Guid.NewGuid().ToString( "N" );
            var expectedEmailAddress = System.Guid.NewGuid().ToString( "N" ) + "@fakeinbox.com";
            var userId = System.Guid.NewGuid().ToString( "N" );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.Email, expectedEmailAddress),
                    new Claim(JwtClaimTypes.GivenName, expectedFirstName),
                    new Claim(JwtClaimTypes.Address, addressJson),
                    new Claim(JwtClaimTypes.FamilyName, expectedLastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( "OIDC_" + userId, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedEmailAddress.ToLower(), actualUserLogin.Person.Email.ToLower() );
            Assert.That.AreEqual( expectedFirstName.ToLower(), actualUserLogin.Person.FirstName.ToLower() );
            Assert.That.AreEqual( expectedLastName.ToLower(), actualUserLogin.Person.LastName.ToLower() );

            var homeAddressDv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
            var actualAddress = actualUserLogin.Person.PrimaryFamily.GroupLocations.Where( gl => gl.GroupLocationTypeValueId == homeAddressDv.Id ).FirstOrDefault();

            Assert.That.IsNotNull( actualAddress );
            Assert.That.AreEqual( street1, actualAddress.Location.Street1.ToStringSafe() );
            Assert.That.AreEqual( street2, actualAddress.Location.Street2.ToStringSafe() );
            Assert.That.AreEqual( city, actualAddress.Location.City.ToStringSafe() );
            Assert.That.AreEqual( state, actualAddress.Location.State.ToStringSafe() );
            Assert.That.AreEqual( postalCode, actualAddress.Location.PostalCode.ToStringSafe() );
            Assert.That.AreEqual( country, actualAddress.Location.Country.ToStringSafe() );
        }

        private Person AddTestPerson()
        {
            var person = new Person
            {
                Email = $"{System.Guid.NewGuid().ToString( "N" )}@fakeinbox.com",
                FirstName = "Test",
                LastName = "User"
            };

            var rockContext = new RockContext();
            rockContext.People.Add( person );
            rockContext.SaveChanges();

            return person;
        }

        [TestMethod]
        [DataRow( "male", "Male" )]
        [DataRow( "Male", "Male" )]
        [DataRow( "MALE", "Male" )]
        [DataRow( "m", "Male" )]
        [DataRow( "M", "Male" )]
        [DataRow( "female", "Female" )]
        [DataRow( "Female", "Female" )]
        [DataRow( "FEMALE", "Female" )]
        [DataRow( "f", "Female" )]
        [DataRow( "", "Unknown" )]
        [DataRow( "test", "Unknown" )]
        [DataRow( "false", "Unknown" )]
        public void HandleOidcUserAddUpdate_NewUserGenderIsCreatedCorrectly( string gender, string expectedGender )
        {
            var expectedFirstName = System.Guid.NewGuid().ToString( "N" );
            var expectedLastName = System.Guid.NewGuid().ToString( "N" );
            var expectedEmailAddress = System.Guid.NewGuid().ToString( "N" ) + "@fakeinbox.com";
            var userId = System.Guid.NewGuid().ToString( "N" );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim(JwtClaimTypes.Subject, userId),
                    new Claim(JwtClaimTypes.Email, expectedEmailAddress),
                    new Claim(JwtClaimTypes.GivenName, expectedFirstName),
                    new Claim(JwtClaimTypes.Gender, gender),
                    new Claim(JwtClaimTypes.FamilyName, expectedLastName)
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( "OIDC_" + userId, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedEmailAddress.ToLower(), actualUserLogin.Person.Email.ToLower() );
            Assert.That.AreEqual( expectedFirstName.ToLower(), actualUserLogin.Person.FirstName.ToLower() );
            Assert.That.AreEqual( expectedLastName.ToLower(), actualUserLogin.Person.LastName.ToLower() );
            Assert.That.AreEqual( expectedGender, actualUserLogin.Person.Gender.ToString() );
        }

        [TestMethod]
        [DataRow( "1980", "1980", "", "" )]
        [DataRow( "", "", "", "" )]
        [DataRow( "1980-01-15", "1980", "1", "15" )]
        [DataRow( "0000-01-15", "", "1", "15" )]
        [DataRow( "1980-1-1", "1980", "1", "1" )]
        [DataRow( "1980-1-15", "1980", "1", "15" )]
        [DataRow( "1980-11-1", "1980", "11", "1" )]
        [DataRow( "0000-1-1", "", "1", "1" )]
        [DataRow( "0000-1-10", "", "1", "10" )]
        [DataRow( "0000-11-4", "", "11", "4" )]
        public void HandleOidcUserAddUpdate_NewUserBirthdayIsCreatedCorrectly( string birthday, string expectedYear, string expectedMonth, string expectedDay )
        {
            var expectedFirstName = System.Guid.NewGuid().ToString( "N" );
            var expectedLastName = System.Guid.NewGuid().ToString( "N" );
            var expectedEmailAddress = System.Guid.NewGuid().ToString( "N" ) + "@fakeinbox.com";
            var userId = System.Guid.NewGuid().ToString( "N" );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim( JwtClaimTypes.Subject, userId ),
                    new Claim( JwtClaimTypes.Email, expectedEmailAddress ),
                    new Claim( JwtClaimTypes.GivenName, expectedFirstName ),
                    new Claim( JwtClaimTypes.BirthDate, birthday ),
                    new Claim( JwtClaimTypes.FamilyName, expectedLastName )
                } ),
            };

            var token = ( JwtSecurityToken ) tokenHandler.CreateToken( tokenDescriptor );

            var oidcClient = new OidcClient();
            var userName = oidcClient.HandleOidcUserAddUpdate( token, "test" );

            var userLoginService = new UserLoginService( new RockContext() );
            var actualUserLogin = userLoginService.Queryable().Where( ul => ul.UserName == userName ).FirstOrDefault();

            Assert.That.IsNotNull( actualUserLogin );
            Assert.That.AreEqual( "OIDC_" + userId, actualUserLogin.UserName );
            Assert.That.AreEqual( expectedEmailAddress.ToLower(), actualUserLogin.Person.Email.ToLower() );
            Assert.That.AreEqual( expectedFirstName.ToLower(), actualUserLogin.Person.FirstName.ToLower() );
            Assert.That.AreEqual( expectedLastName.ToLower(), actualUserLogin.Person.LastName.ToLower() );
            Assert.That.AreEqual( expectedYear, actualUserLogin.Person.BirthYear.ToString() );
            Assert.That.AreEqual( expectedMonth, actualUserLogin.Person.BirthMonth.ToString() );
            Assert.That.AreEqual( expectedDay, actualUserLogin.Person.BirthDay.ToString() );
        }
    }
}
