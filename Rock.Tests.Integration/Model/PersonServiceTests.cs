using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Model
{
    [TestClass]
    public class PersonServiceTests
    {
        #region Setup

        public static class PersonGuid
        {
            public static readonly Guid PersonWithPrimaryAndPreviousEmailsGuid = new Guid( "CAD44822-1683-4E24-BA69-22363B2456E2" );
            public static readonly Guid PersonWithNoEmailsGuid = new Guid( "80383B8D-E474-413F-AC85-EA1C48BA7C05" );
            public static readonly Guid PersonWithPrimaryEmailButDifferentNameGuid = new Guid( "7563E732-A49F-4037-B03B-C798DBD695CB" );
        }

        public static class Email
        {
            public static readonly string PreviousEmail1 = "previousEmail1@test.test";
            public static readonly string PreviousEmail2 = "previousEmail2@test.test";
            public static readonly string PreviousEmail3 = "previousEmail3@test.test";
            public static readonly string PrimaryEmail = "PrimaryEmail@test.test";
            public static readonly string BlankEmail = "";
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
    }
}
