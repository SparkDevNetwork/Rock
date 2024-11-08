using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class PersonTokenServiceTests : DatabaseTestsBase
    {
        #region Setup

        public static class PersonGuid
        {
            public const string PersonWithLowAccountProtectionProfileGuid = "45B7F4F9-7164-4112-809A-F5E3E5B065C0";
            public const string PersonWithMediumAccountProtectionProfileGuid = "31F17FCC-60D3-49CB-9351-36A79BAEAC59";
            public const string PersonWithHighAccountProtectionProfileGuid = "CF7CDB0B-BA42-4AEF-A371-8608C5A06654";
            public const string PersonWithExtremeAccountProtectionProfileGuid = "1E747581-391E-4DDC-88F2-2A5148BE9952";
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            CreatePersonWithProtectionProfile( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid(), AccountProtectionProfile.Low );
            CreatePersonWithProtectionProfile( PersonGuid.PersonWithMediumAccountProtectionProfileGuid.AsGuid(), AccountProtectionProfile.Medium );
            CreatePersonWithProtectionProfile( PersonGuid.PersonWithHighAccountProtectionProfileGuid.AsGuid(), AccountProtectionProfile.High );
            CreatePersonWithProtectionProfile( PersonGuid.PersonWithExtremeAccountProtectionProfileGuid.AsGuid(), AccountProtectionProfile.Extreme );
        }

        private static void CreatePersonWithProtectionProfile(Guid guid, AccountProtectionProfile accountProtectionProfile)
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( guid );
            if ( person != null )
            {
                new PersonAliasService( rockContext ).DeleteRange( person.Aliases );
                new PersonSearchKeyService( rockContext ).DeleteRange( person.GetPersonSearchKeys( rockContext ).ToList() );
                personService.Delete( person );
                rockContext.SaveChanges();
            }

            person = new Person()
            {
                RecordTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "I Am A",
                LastName = "Test Person",
                Guid = guid,
                AccountProtectionProfile = accountProtectionProfile
            };

            PersonService.SaveNewPerson( person, rockContext );
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
        }

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            //
        }

        #endregion

        [TestMethod]
        public void PersonWithLowAccountProtectionProfileShouldGetAToken()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithLowAccountProtectionProfile = personService.Get( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid() );

            var token = personWithLowAccountProtectionProfile.GetImpersonationToken();

            Assert.That.IsNotNull( token );
            Assert.That.NotEqual( "TokenProhibited", token );
        }

        [TestMethod]
        public void PersonWithExtremeAccountProtectionProfileShouldNotGetAToken()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithExtremeAccountProtectionProfile = personService.Get( PersonGuid.PersonWithExtremeAccountProtectionProfileGuid.AsGuid() );

            var token = personWithExtremeAccountProtectionProfile.GetImpersonationToken();

            Assert.That.Equal( "TokenProhibited", token );
        }

        [TestMethod]
        public void WithLegacyFallbackValidTokenShouldAllowLogin()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personTokenService = new PersonTokenService( rockContext );
            var personWithLowAccountProtectionProfile = personService.Get( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid() );

            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "core.PersonTokenUseLegacyFallback", "true", false );

            var token = personWithLowAccountProtectionProfile.GetImpersonationToken();

            Assert.That.IsNotNull( token );

            var personFromToken = personTokenService.GetByImpersonationToken( token );

            Assert.That.Equal( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid(), personFromToken.PersonAlias.Person.Guid );
        }

        [TestMethod]
        public void WithLegacyFallbackInvalidTokenShouldNotAllowLogin()
        {
            var rockContext = new RockContext();
            var personTokenService = new PersonTokenService( rockContext );
            var token = "TokenProhibited";

            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "core.PersonTokenUseLegacyFallback", "true", false );

            var personFromToken = personTokenService.GetByImpersonationToken( token );

            Assert.That.IsNull( personFromToken );
        }

        [TestMethod]
        public void WithoutLegacyFallbackValidTokenShouldAllowLogin()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personTokenService = new PersonTokenService( rockContext );
            var personWithLowAccountProtectionProfile = personService.Get( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid() );

            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "core.PersonTokenUseLegacyFallback", "false", false );

            var token = personWithLowAccountProtectionProfile.GetImpersonationToken();

            Assert.That.IsNotNull( token );

            var personFromToken = personTokenService.GetByImpersonationToken( token );

            Assert.That.Equal( PersonGuid.PersonWithLowAccountProtectionProfileGuid.AsGuid(), personFromToken.PersonAlias.Person.Guid );
        }

        [TestMethod]
        public void WithoutLegacyFallbackInvalidTokenShouldNotAllowLogin()
        {
            var rockContext = new RockContext();
            var personTokenService = new PersonTokenService( rockContext );
            var token = "TokenProhibited";

            GlobalAttributesCache globalAttributes = GlobalAttributesCache.Get();
            globalAttributes.SetValue( "core.PersonTokenUseLegacyFallback", "false", false );

            var personFromToken = personTokenService.GetByImpersonationToken( token );

            Assert.That.IsNull( personFromToken );
        }
    }
}
