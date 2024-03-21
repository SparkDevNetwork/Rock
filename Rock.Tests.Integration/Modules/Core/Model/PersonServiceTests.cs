using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    [TestClass]
    public class PersonServiceTests : DatabaseTestsBase
    {
        #region Setup

        public static class PersonGuid
        {
            public static readonly Guid PersonWithPrimaryAndPreviousEmailsGuid = new Guid( "CAD44822-1683-4E24-BA69-22363B2456E2" );
            public static readonly Guid PersonWithNoEmailsGuid = new Guid( "80383B8D-E474-413F-AC85-EA1C48BA7C05" );
            public static readonly Guid PersonWithPrimaryEmailButDifferentNameGuid = new Guid( "7563E732-A49F-4037-B03B-C798DBD695CB" );
            public const string PersonWithPrimaryEmailLowAccountProtectionProfileGuid = "45B7F4F9-7164-4112-809A-F5E3E5B065C0";
            public const string PersonWithPrimaryEmailMediumAccountProtectionProfileGuid = "31F17FCC-60D3-49CB-9351-36A79BAEAC59";
            public const string PersonWithPrimaryEmailHighAccountProtectionProfileGuid = "CF7CDB0B-BA42-4AEF-A371-8608C5A06654";
            public const string PersonWithPrimaryEmailExtremeAccountProtectionProfileGuid = "1E747581-391E-4DDC-88F2-2A5148BE9952";
        }

        public static class Email
        {
            public static readonly string PreviousEmail1 = "previousEmail1@test.test";
            public static readonly string PreviousEmail2 = "previousEmail2@test.test";
            public static readonly string PreviousEmail3 = "previousEmail3@test.test";
            public static readonly string PrimaryEmail = "PrimaryEmail@test.test";
            public static readonly string BlankEmail = "";

            public const string PrimaryEmailLow = "PrimaryEmailLow@test.test";
            public const string PrimaryEmailMedium = "PrimaryEmailMedium@test.test";
            public const string PrimaryEmailHigh = "PrimaryEmailHigh@test.test";
            public const string PrimaryEmailExtreme = "PrimaryEmailExtreme@test.test";
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            CreatePersonWithPrimaryAndPreviousEmails();
            CreatePersonWithNoEmails();
            CreatePersonWithPrimaryEmailButDifferentName();
            CreatePersonWithPrimaryEmailAndProtectionProfile( PersonGuid.PersonWithPrimaryEmailLowAccountProtectionProfileGuid.AsGuid(), Email.PrimaryEmailLow, AccountProtectionProfile.Low );
            CreatePersonWithPrimaryEmailAndProtectionProfile( PersonGuid.PersonWithPrimaryEmailMediumAccountProtectionProfileGuid.AsGuid(), Email.PrimaryEmailMedium, AccountProtectionProfile.Medium );
            CreatePersonWithPrimaryEmailAndProtectionProfile( PersonGuid.PersonWithPrimaryEmailHighAccountProtectionProfileGuid.AsGuid(), Email.PrimaryEmailHigh, AccountProtectionProfile.High );
            CreatePersonWithPrimaryEmailAndProtectionProfile( PersonGuid.PersonWithPrimaryEmailExtremeAccountProtectionProfileGuid.AsGuid(), Email.PrimaryEmailExtreme, AccountProtectionProfile.Extreme );
        }

        private static void CreatePersonWithPrimaryAndPreviousEmails()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );
            if ( person != null )
            {
                new PersonAliasService( rockContext ).DeleteRange( person.Aliases );
                new PersonSearchKeyService( rockContext ).DeleteRange( person.GetPersonSearchKeys( rockContext ).ToList() );
                personService.Delete( person );
                rockContext.SaveChanges();
            }

            person = new Person()
            {
                RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "I Have A",
                LastName = "CommonLastName",
                Guid = PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid,
                Email = Email.PrimaryEmail
            };

            Group newPersonFamily = PersonService.SaveNewPerson( person, rockContext );

            person = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );

            var primaryAliasId = person.PrimaryAliasId;

            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );

            PersonSearchKey personSearchKeyPreviousEmail1 = new PersonSearchKey()
            {
                PersonAliasId = primaryAliasId,
                SearchTypeValueId = searchTypeValue.Id,
                SearchValue = Email.PreviousEmail1
            };

            personSearchKeyService.Add( personSearchKeyPreviousEmail1 );

            PersonSearchKey personSearchKeyPreviousEmail2 = new PersonSearchKey()
            {
                PersonAliasId = primaryAliasId,
                SearchTypeValueId = searchTypeValue.Id,
                SearchValue = Email.PreviousEmail2
            };

            personSearchKeyService.Add( personSearchKeyPreviousEmail2 );

            PersonSearchKey personSearchKeyPreviousEmail3 = new PersonSearchKey()
            {
                PersonAliasId = primaryAliasId,
                SearchTypeValueId = searchTypeValue.Id,
                SearchValue = Email.PreviousEmail3
            };

            personSearchKeyService.Add( personSearchKeyPreviousEmail3 );
            rockContext.SaveChanges();
        }

        private static void CreatePersonWithPrimaryEmailAndProtectionProfile( Guid guid, string email, AccountProtectionProfile accountProtectionProfile )
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( guid );
            if ( person != null )
            {
                person.AccountProtectionProfile = accountProtectionProfile;
                new PersonAliasService( rockContext ).DeleteRange( person.Aliases );
                new PersonSearchKeyService( rockContext ).DeleteRange( person.GetPersonSearchKeys( rockContext ).ToList() );
                personService.Delete( person );
                rockContext.SaveChanges();
            }

            person = new Person()
            {
                RecordTypeValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "I Have A",
                LastName = "CommonLastName",
                Guid = guid,
                Email = email,
                AccountProtectionProfile = accountProtectionProfile
            };

            PersonService.SaveNewPerson( person, rockContext );
        }

        private static void CreatePersonWithNoEmails()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( PersonGuid.PersonWithNoEmailsGuid );
            if ( person != null )
            {
                new PersonAliasService( rockContext ).DeleteRange( person.Aliases );
                new PersonSearchKeyService( rockContext ).DeleteRange( person.GetPersonSearchKeys( rockContext ).ToList() );
                personService.Delete( person );
                rockContext.SaveChanges();
            }

            person = new Person()
            {
                RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "I Have A",
                LastName = "CommonLastName",
                Guid = PersonGuid.PersonWithNoEmailsGuid,
                Email = Email.BlankEmail
            };

            Group newPersonFamily = PersonService.SaveNewPerson( person, rockContext );


            rockContext.SaveChanges();
        }

        private static void CreatePersonWithPrimaryEmailButDifferentName()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = personService.Get( PersonGuid.PersonWithPrimaryEmailButDifferentNameGuid );
            if ( person != null )
            {
                new PersonAliasService( rockContext ).DeleteRange( person.Aliases );
                new PersonSearchKeyService( rockContext ).DeleteRange( person.GetPersonSearchKeys( rockContext ).ToList() );
                personService.Delete( person );
                rockContext.SaveChanges();
            }

            person = new Person()
            {
                RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                FirstName = "I Have A",
                LastName = "DifferentLastName",
                Guid = PersonGuid.PersonWithPrimaryEmailButDifferentNameGuid,
                Email = Email.PrimaryEmail
            };

            Group newPersonFamily = PersonService.SaveNewPerson( person, rockContext );

            rockContext.SaveChanges();
        }

        #endregion

        #region Person Match tests

        [TestMethod]
        public void PersonWithPrimaryEmailShouldMatch()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithPrimaryAndPreviousEmails = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );
            var emailSearch = Email.PrimaryEmail;

            /* Person should only match if the PrimaryEmail or Previous is exactly the same as Email search.
            */

            var personMatchQuery = new PersonService.PersonMatchQuery( personWithPrimaryAndPreviousEmails.FirstName, personWithPrimaryAndPreviousEmails.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithPrimaryAndPreviousEmails.Gender,
                BirthDate = personWithPrimaryAndPreviousEmails.BirthDate,
                SuffixValueId = personWithPrimaryAndPreviousEmails.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );
            Assert.That.IsNotNull( foundPerson );
        }

        [TestMethod]
        public void PersonWithPrimaryEmailShouldMatchCaseInsensitive()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithPrimaryAndPreviousEmails = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );
            var emailSearch = Email.PrimaryEmail.ToUpperInvariant();

            /* Person should only match if the PrimaryEmail or Previous is exactly the same as Email search.
            */

            var personMatchQuery = new PersonService.PersonMatchQuery( personWithPrimaryAndPreviousEmails.FirstName, personWithPrimaryAndPreviousEmails.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithPrimaryAndPreviousEmails.Gender,
                BirthDate = personWithPrimaryAndPreviousEmails.BirthDate,
                SuffixValueId = personWithPrimaryAndPreviousEmails.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );
            Assert.That.IsNotNull( foundPerson );
        }

        [TestMethod]
        [DataRow( PersonGuid.PersonWithPrimaryEmailLowAccountProtectionProfileGuid, Email.PrimaryEmailLow, AccountProtectionProfile.Low )]
        [DataRow( PersonGuid.PersonWithPrimaryEmailMediumAccountProtectionProfileGuid, Email.PrimaryEmailMedium, AccountProtectionProfile.Medium )]
        [DataRow( PersonGuid.PersonWithPrimaryEmailHighAccountProtectionProfileGuid, Email.PrimaryEmailHigh, AccountProtectionProfile.High )]
        [DataRow( PersonGuid.PersonWithPrimaryEmailExtremeAccountProtectionProfileGuid, Email.PrimaryEmailExtreme, AccountProtectionProfile.Extreme )]
        public void PersonWithPrimaryEmailShouldHandleAccountProtectionProfileCorrectly( string personGuid, string emailSearch, int accountProtectionProfile )
        {
            if ( !Bus.RockMessageBus.IsRockStarted )
            {
                Bus.RockMessageBus.IsRockStarted = true;
                Bus.RockMessageBus.StartAsync().Wait();
            }

            var securitySettingService = new SecuritySettingsService();
            securitySettingService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile>();
            securitySettingService.Save();

            // Give time for cache to update.
            Thread.Sleep( 50 );

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personGuid.AsGuid() );

                var personMatchQuery = new PersonService.PersonMatchQuery( person.FirstName, person.LastName, emailSearch, null )
                {
                    MobilePhone = null,
                    Gender = person.Gender,
                    BirthDate = person.BirthDate,
                    SuffixValueId = person.SuffixValueId
                };

                var foundPerson = personService.FindPerson( personMatchQuery, false );
                Assert.That.IsNotNull( foundPerson );
            }

            securitySettingService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile> { ( AccountProtectionProfile ) accountProtectionProfile };
            securitySettingService.Save();

            // Give time for cache to update.
            Thread.Sleep( 50 );

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var person = personService.Get( personGuid.AsGuid() );

                var personMatchQuery = new PersonService.PersonMatchQuery( person.FirstName, person.LastName, emailSearch, null )
                {
                    MobilePhone = null,
                    Gender = person.Gender,
                    BirthDate = person.BirthDate,
                    SuffixValueId = person.SuffixValueId
                };

                var foundPerson = personService.FindPerson( personMatchQuery, false );
                Assert.That.IsNull( foundPerson );
            }
        }

        [TestMethod]
        public void PersonWithPrimaryEmailShouldNotMatchIfAccountProtectionProfileDisabled()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithPrimaryAndPreviousEmails = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );
            var emailSearch = Email.PrimaryEmail;



            var personMatchQuery = new PersonService.PersonMatchQuery( personWithPrimaryAndPreviousEmails.FirstName, personWithPrimaryAndPreviousEmails.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithPrimaryAndPreviousEmails.Gender,
                BirthDate = personWithPrimaryAndPreviousEmails.BirthDate,
                SuffixValueId = personWithPrimaryAndPreviousEmails.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );
            Assert.That.IsNotNull( foundPerson );
        }

        [TestMethod]
        public void PersonWithPreviousEmailShouldMatch()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithPrimaryAndPreviousEmails = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );
            var emailSearch = Email.PreviousEmail1;

            /* Person should only match if the PrimaryEmail or Previous is exactly the same as Email search.
            */

            var personMatchQuery = new PersonService.PersonMatchQuery( personWithPrimaryAndPreviousEmails.FirstName, personWithPrimaryAndPreviousEmails.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithPrimaryAndPreviousEmails.Gender,
                BirthDate = personWithPrimaryAndPreviousEmails.BirthDate,
                SuffixValueId = personWithPrimaryAndPreviousEmails.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );
            Assert.That.IsNotNull( foundPerson );
        }

        [TestMethod]
        public void PersonWithNoEmailShouldNotMatch()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithNoEmails = personService.Get( PersonGuid.PersonWithNoEmailsGuid );

            /* personWithNoEmails doesn't have an email address, so that should not be a match
            */

            var emailSearch = Email.PrimaryEmail;

            var personMatchQuery = new PersonService.PersonMatchQuery( personWithNoEmails.FirstName, personWithNoEmails.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithNoEmails.Gender,
                BirthDate = personWithNoEmails.BirthDate,
                SuffixValueId = personWithNoEmails.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPersons = personService.FindPersons( personMatchQuery, updatePrimaryEmail );
            bool foundPersonWithNoEmail = foundPersons.Any( a => a.Guid == PersonGuid.PersonWithNoEmailsGuid );

            Assert.That.IsFalse( foundPersonWithNoEmail );
        }

        [TestMethod]
        public void PersonWithPrimaryEmailButDifferentNameShouldNotMatch()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var personWithPrimaryEmailButDifferentName = personService.Get( PersonGuid.PersonWithPrimaryEmailButDifferentNameGuid );
            var personWithPrimaryEmail = personService.Get( PersonGuid.PersonWithPrimaryAndPreviousEmailsGuid );

            var emailSearch = Email.PrimaryEmail;

            var personMatchQuery = new PersonService.PersonMatchQuery( personWithPrimaryEmail.FirstName, personWithPrimaryEmail.LastName, emailSearch, null )
            {
                MobilePhone = null,
                Gender = personWithPrimaryEmail.Gender,
                BirthDate = personWithPrimaryEmail.BirthDate,
                SuffixValueId = personWithPrimaryEmail.SuffixValueId
            };

            bool updatePrimaryEmail = false;
            var foundPersons = personService.FindPersons( personMatchQuery, updatePrimaryEmail );
            bool foundPersonWithPrimaryEmailButDifferentName = foundPersons.Any( a => a.Guid == PersonGuid.PersonWithPrimaryEmailButDifferentNameGuid );
            Assert.That.IsFalse( foundPersonWithPrimaryEmailButDifferentName );
        }

        #endregion Person Match tests

        #region Age Classification

        [TestMethod]
        public void ShouldAssignAppropriateValueToAgeClassification()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var personTurnedAdult = new Person
            {
                FirstName = "Adult",
                LastName = "AgeClassificationTest",
                AgeClassification = AgeClassification.Child,
                IsLockedAsChild = false
            };
            personTurnedAdult.SetBirthDate( RockDateTime.Now.AddYears( -20 ) );

            // Adult locked as child
            var adultLockedAsChild = new Person
            {
                FirstName = "AdultLockedAsChild",
                LastName = "AgeClassificationTest",
                AgeClassification = AgeClassification.Child,
                IsLockedAsChild = true
            };
            adultLockedAsChild.SetBirthDate( RockDateTime.Now.AddYears( -20 ) );

            // Person without birthday
            var personWithoutBirthday = new Person
            {
                FirstName = "PersonWithoutBirthday",
                LastName = "AgeClassificationTest"
            };
            personWithoutBirthday.SetBirthDate( null );

            var personWithUnkownAgeClassificiationLockedAsChild = new Person
            {
                FirstName = "PersonWithoutBirthday",
                LastName = "AgeClassificationTest",
                AgeClassification = AgeClassification.Unknown,
                IsLockedAsChild = true
            };

            var personWithoutFamilyOrBirthday = new Person
            {
                FirstName = "PersonWithoutFamilyOrBirthday",
                LastName = "AgeClassificationTest",
                AgeClassification = AgeClassification.Unknown,
                IsLockedAsChild = true // locking the person as child so that the Age Classification does not change While being inserted into database
            };
            personWithoutFamilyOrBirthday.SetBirthDate( null );

            PersonService.SaveNewPerson( personTurnedAdult, rockContext );
            PersonService.SaveNewPerson( adultLockedAsChild, rockContext );
            PersonService.SaveNewPerson( personWithoutBirthday, rockContext );
            PersonService.SaveNewPerson( personWithUnkownAgeClassificiationLockedAsChild, rockContext );

            Group familyGroup = PersonService.SaveNewPerson( personWithoutFamilyOrBirthday, rockContext );
            GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            groupMemberService.Delete( groupMemberService.Queryable().Where( gm => gm.PersonId == personWithoutFamilyOrBirthday.Id && gm.GroupId == familyGroup.Id ).First() ); // remove from family
            personService.Queryable()
                .Where( p => p.Id == personWithoutFamilyOrBirthday.Id )
                .First().IsLockedAsChild = false; // reset the locking as Child that was done to prevent changes to AgeClassification attribute of the person on being saved to the database.
            rockContext.SaveChanges();

            PersonService.UpdatePersonAgeClassificationAll( rockContext );
            rockContext.SaveChanges();

            var personTurnedAdultFromDatabaseAgeClassification = personService.Queryable()
                .Where( p => p.Id == personTurnedAdult.Id )
                .Select( p => p.AgeClassification )
                .First();
            var adultLockedAsChildFromDatabaseAgeClassification = personService.Queryable()
                .Where( p => p.Id == adultLockedAsChild.Id )
                .Select( p => p.AgeClassification )
                .First();
            var personWithoutBirthdayFromDatabaseAgeClassification = personService.Queryable()
                .Where( p => p.Id == personWithoutBirthday.Id )
                .Select( p => p.AgeClassification )
                .First();
            var personWithUnkownAgeClassificiationLockedAsChildAgeClassification = personService.Queryable()
                .Where( p => p.Id == personWithUnkownAgeClassificiationLockedAsChild.Id )
                .Select( p => p.AgeClassification )
                .First();
            var personWithoutFamilyOrBirthdayAgeClassification = personService.Queryable()
                .Where( p => p.Id == personWithoutFamilyOrBirthday.Id )
                .Select( p => p.AgeClassification )
                .First();


            // Clean up the created accounts
            // Motive: If any of the assertions fail, the test will be terminated midway. If the clean up code happens to be after the assert, it won't get executed if any of the
            // assertions fail. Thus, cleaning up the code before the assertions.

            new PersonAliasService( rockContext ).DeleteRange( personTurnedAdult.Aliases );
            new PersonSearchKeyService( rockContext ).DeleteRange( personTurnedAdult.GetPersonSearchKeys( rockContext ).ToList() );
            personService.Delete( personTurnedAdult );

            new PersonAliasService( rockContext ).DeleteRange( adultLockedAsChild.Aliases );
            new PersonSearchKeyService( rockContext ).DeleteRange( adultLockedAsChild.GetPersonSearchKeys( rockContext ).ToList() );
            personService.Delete( adultLockedAsChild );

            new PersonAliasService( rockContext ).DeleteRange( personWithoutBirthday.Aliases );
            new PersonSearchKeyService( rockContext ).DeleteRange( personWithoutBirthday.GetPersonSearchKeys( rockContext ).ToList() );
            personService.Delete( personWithoutBirthday );

            new PersonAliasService( rockContext ).DeleteRange( personWithUnkownAgeClassificiationLockedAsChild.Aliases );
            new PersonSearchKeyService( rockContext ).DeleteRange( personWithUnkownAgeClassificiationLockedAsChild.GetPersonSearchKeys( rockContext ).ToList() );
            personService.Delete( personWithUnkownAgeClassificiationLockedAsChild );

            new PersonAliasService( rockContext ).DeleteRange( personWithoutFamilyOrBirthday.Aliases );
            new PersonSearchKeyService( rockContext ).DeleteRange( personWithoutFamilyOrBirthday.GetPersonSearchKeys( rockContext ).ToList() );
            personService.Delete( personWithoutFamilyOrBirthday );

            rockContext.SaveChanges();

            // Assertions

            Assert.AreEqual( AgeClassification.Adult, personTurnedAdultFromDatabaseAgeClassification, "Age Classification of adult person should be set to Adult" );
            Assert.AreEqual( AgeClassification.Child, adultLockedAsChildFromDatabaseAgeClassification, "Age Classification of adult person locked as child should not be set to Adult" );
            Assert.AreEqual( AgeClassification.Adult, personWithoutBirthdayFromDatabaseAgeClassification,
                "Age Classification of person without birthday but is not a child in any family should be set to Adult" );
            Assert.AreEqual( AgeClassification.Unknown, personWithUnkownAgeClassificiationLockedAsChildAgeClassification,
                "Age Classification of person without age classification but locked as child should not be altered" );
            Assert.AreEqual( AgeClassification.Unknown, personWithoutFamilyOrBirthdayAgeClassification, "Age Classification of person without age or family should be set to Unknown" );
        }

        #endregion Age Classification
    }
}
