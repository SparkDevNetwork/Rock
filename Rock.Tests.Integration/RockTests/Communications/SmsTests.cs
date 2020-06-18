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
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communications.Sms
{
    /// <summary>
    /// Initialize test data for the Steps feature of Rock.
    /// </summary>
    [TestClass]
    public class SmsTests
    {
        private CommunicationsModuleTestHelper _Helper = new CommunicationsModuleTestHelper();

        public static class Constants
        {
            // Constants

            public static string UnnamedPersonMobileNumber = "480 777 1234";
        }

        [TestMethod]
        [TestProperty( "Feature", TestFeatures.Communications )]
        public void IncomingSms_FromUnknownMobileNumber_CreatesNamelessPerson()
        {
            var message = this.GetTestIncomingSmsMessage( CommunicationsModuleTestHelper.Constants.UnknownPerson1MobileNumber );

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            // Verify that we have a new Unnamed Person record associated with the new mobile number.
            var unnamedPerson = personService.GetPersonFromMobilePhoneNumber( message.FromNumber, true );

            dataContext.SaveChanges();

            message.FromPerson = unnamedPerson;

            var unnamedPersonRecordValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS ).Id;

            // Verify that the Record Type is "Unnamed".
            Assert.AreEqual( unnamedPersonRecordValueId, unnamedPerson.RecordTypeValueId, "Person Type for Unnamed Person record is invalid." );

            // Verify that the correct Phone Number has been recorded.
            Assert.IsNotNull( unnamedPerson.PhoneNumbers.FirstOrDefault( x => x.Number == CommunicationsModuleTestHelper.Constants.UnknownPerson1MobileNumber ) );

            //var outcomes = SmsActionService.ProcessIncomingMessage(message);

            // Delete the newly-created unnamed person record.
            var _Helper = new CommunicationsModuleTestHelper();

            dataContext = new RockContext();
            personService = new PersonService( dataContext );

            _Helper.DeleteNamelessPersonRecord( dataContext, CommunicationsModuleTestHelper.Constants.UnknownPerson1MobileNumber );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// An incoming SMS sent from a mobile number that has been previously added as a Nameless Person record, should return the same Nameless Person record.
        /// </summary>
        [TestMethod]
        [TestProperty( "Feature", TestFeatures.Communications )]
        public void IncomingSms_FromKnownNamelessPerson_ReturnsMatchedPerson()
        {
            var message = this.GetTestIncomingSmsMessage( CommunicationsModuleTestHelper.Constants.UnknownPerson1MobileNumber );

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            // Verify that we have a new Unnamed Person record associated with the new mobile number.
            var unnamedPerson1 = personService.GetPersonFromMobilePhoneNumber( message.FromNumber, createNamelessPersonIfNotFound: true );

            // Retrieve the same sender a second time.
            var unnamedPerson2 = personService.GetPersonFromMobilePhoneNumber( message.FromNumber, true );

            // Verify that the same record has been retrieved both times.
            Assert.AreEqual( unnamedPerson1.Id, unnamedPerson2.Id, "Multiple Unnamed Person records created for the same mobile number." );

            // Delete the newly-created unnamed person record.
            _Helper.DeleteNamelessPersonRecord( dataContext, CommunicationsModuleTestHelper.Constants.UnknownPerson1MobileNumber );

            dataContext.SaveChanges();
        }

        /// <summary>
        /// An incoming SMS sent from a mobile number that has been previously added as a Nameless Person record, should return the same Nameless Person record.
        /// </summary>
        [TestMethod]
        [TestProperty( "Feature", TestFeatures.Communications )]
        public void IncomingSms_FromKnownNamedPerson_ReturnsMatchedPerson()
        {
            var message = this.GetTestIncomingSmsMessage( CommunicationsModuleTestHelper.Constants.MobilePhoneTedDecker );

            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            // Verify that we have retrieved Ted Decker from his mobile number.
            var namedPerson1 = personService.GetPersonFromMobilePhoneNumber( message.FromNumber, createNamelessPersonIfNotFound: false );

            // Verify that the correct record has been retrieved.
            Assert.AreEqual( namedPerson1.Guid, CommunicationsModuleTestHelper.Constants.TedDeckerPersonGuid.AsGuid(), "Incorrect Person record retrieved by mobile phone number." );
        }
        [TestMethod]
        [TestProperty( "Feature", TestFeatures.Communications )]
        public void NamelessPersonRecordType_GetDefaultPersonQuery_DoesNotIncludeNameless()
        {
            // Verify at least one NamelessPerson Record Type exists.
            var dataContext = new RockContext();

            var personService = new PersonService( dataContext );

            var personQueryOptions = new PersonService.PersonQueryOptions();

            personQueryOptions.IncludeNameless = true;

            var unfilteredQuery = personService.Queryable( personQueryOptions );


            Assert.IsTrue( this.PersonQueryContainsNamelessPersonRecordType( unfilteredQuery ),
                           "Test data set must contain at least one Person record with a Record Type of \"Nameless Person\"." );

            var defaultQuery = personService.Queryable();

            Assert.IsFalse( this.PersonQueryContainsNamelessPersonRecordType( defaultQuery ),
                            "Base Person Queryable incorrectly includes a Person record with Record Type of \"Nameless Person\"." );
        }

        #region Support functions

        /// <summary>
        /// Verify if the provided Person query contains a Record Type of "Nameless".
        /// </summary>
        /// <param name="personQuery"></param>
        /// <returns></returns>
        private bool PersonQueryContainsNamelessPersonRecordType( IQueryable<Person> personQuery )
        {
            int recordTypeValueIdNameless = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;

            var hasNamelessPerson = personQuery.Any( x => x.RecordTypeValueId == recordTypeValueIdNameless );

            return hasNamelessPerson;
        }

        /// <summary>
        // Create an incoming SMS Message object, from an unknown person to a known SMS Transport Sender/Recipient.
        /// This is the same object that is created for processing when a notification is received by a webhook such as TwilioSms.ashx.
        /// </summary>
        /// <param name="fromNumber"></param>
        /// <returns></returns>
        private SmsMessage GetTestIncomingSmsMessage( string fromNumber )
        {
            var message = new SmsMessage();

            // Get Known SMS Recipient.
            var smsSenderValue = DefinedValueCache.Get( CommunicationsModuleTestHelper.Constants.TestSmsSenderGuid );

            message.ToNumber = smsSenderValue.Description;

            message.FromNumber = fromNumber;

            message.Message = $"Test Message { RockDateTime.Now:dd-MMM-yy hh:mm:ss}";

            return message;
        }

        #endregion
    }
}