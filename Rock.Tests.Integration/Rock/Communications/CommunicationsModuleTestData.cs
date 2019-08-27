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
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Utility;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Communications
{
    /// <summary>
    /// Create and manage test data for the Rock Communications module.
    /// </summary>
    [TestClass]
    public class CommunicationsModuleTestData
    {
        public static class Constants
        {
            public static string TestCommunicationSingleEmail1Guid = "{BDCDD3ED-22FF-43E8-9860-65D26DBD5B9B}";
            public static string TestCommunicationBulkEmail1Guid = "{D3EA6513-372D-4192-8E8F-DCA2707AA572}";
            public static string TestCommunicationBulkSmsGuid = "{79E18AFD-DB14-485A-80D9-CDAC44CA1098}";

            public static string TestSmsSenderGuid = "{DB8CFE73-4209-4109-8FC5-3C5013AFC290}";
        }

        #region Parameters

        /// <summary>
        /// The string copied to the ForeignKey property of each of the records created as test data.
        /// </summary>
        private const string _TestDataSourceOfChange = "CommunicationsIntegrationTest";

        /// <summary>
        /// The maximum number of people that may be selected as Recipients of any communication.
        /// </summary>
        private const int _MaxRecipientCount = 1000;

        #endregion

        #region Add/Remove Test Data

        //private List<Guid> _EmailCreatorPersonGuidList;
        private Random _Rng = new Random();
        private List<string> _ValidClientOsList = new List<string>() { "Windows 10", "Windows 8", "Windows 7", "Android", "OS X", "iOS", "Linux", "Chrome OS" };
        private List<string> _ValidClientTypeList = new List<string>() { "Mobile", "Tablet", "Crawler", "Outlook", "Desktop", "Browser", "None" };
        private List<string> _ValidClientBrowserList = new List<string>() { "Gmail Image Proxy", "Apple Mail", "IE", "Android Browser", "Chrome", "Outlook", "Windows Live Mail", "Firefox", "Safari", "Thunderbird" };
        private Dictionary<Guid, int> _PersonGuidToAliasIdMap;

        /// <summary>
        /// Adds the required test data to the current database.
        /// </summary>
        [TestMethod]
        [TestCategory( TestCategories.AddData )]
        [TestProperty( "Feature", TestFeatures.DataSetup )]
        public void AddCommunicationModuleTestData()
        {
            RemoveCommunicationModuleTestData();

            AddTestDataForCommunications();
        }

        /// <summary>
        /// Removes the test data from the current database.
        /// </summary>
        [TestMethod]
        [TestCategory( TestCategories.RemoveData )]
        [TestProperty( "Feature", TestFeatures.DataMaintenance )]
        public void RemoveCommunicationModuleTestData()
        {
            var dataContext = new RockContext();

            var communicationService = new CommunicationService( dataContext );

            var communicationsQuery = communicationService.Queryable();

            communicationsQuery = communicationsQuery.Where( x => x.ForeignKey == _TestDataSourceOfChange );

            communicationService.DeleteRange( communicationsQuery );

            var recordsAffected = dataContext.SaveChanges();

            Debug.Print( $"Deleted Communications. (Count={ recordsAffected })" );
        }

        /// <summary>
        /// Adds a predictable set of Communications entries to the test database that can be used for integration testing.
        /// </summary>
        private void AddTestDataForCommunications()
        {
            List<Guid> recipientGuidList;
            List<CommunicationRecipientStatus> availableStatusList;

            var dataContext = new RockContext();

            // Add email for 1 recipient.
            var singleCommunication = this.CreateEmailCommunication( dataContext,
                Constants.TestCommunicationSingleEmail1Guid,
                new DateTime( 2019, 1, 30 ),
                "Welcome to Our Church!",
                "This is a test message....",
                new DateTime( 2019, 1, 30 ),
                TestPeople.AlishaMarblePersonGuid,
                TestPeople.BillMarblePersonGuid );

            recipientGuidList = new List<Guid> { TestPeople.TedDeckerPersonGuid, TestPeople.SarahSimmonsPersonGuid };
            availableStatusList = GetAvailableCommunicationStatusList( 0, 0, 0 );

            this.CreateCommunicationRecipients( dataContext, singleCommunication, recipientGuidList, availableStatusList );
            this.CreateCommunicationInteractions( dataContext, singleCommunication, 1 );

            // Add bulk email to 225 recipients, with associated interactions.
            var bulkCommunication = this.CreateEmailCommunication( dataContext,
                Constants.TestCommunicationBulkEmail1Guid,
                new DateTime( 2019, 1, 30 ),
                "Bulk Email Test 1",
                "This is a test message....",
                new DateTime( 2019, 1, 30 ),
                TestPeople.AlishaMarblePersonGuid,
                TestPeople.BillMarblePersonGuid,
                isBulk: true );

            var personMap = this.GetPersonGuidToAliasIdMap( dataContext );

            recipientGuidList = personMap.Keys.ToList().GetRandomizedList( 225 );
            availableStatusList = GetAvailableCommunicationStatusList( 20, 10, 5 );

            this.CreateCommunicationRecipients( dataContext, bulkCommunication, recipientGuidList, availableStatusList );
            this.CreateCommunicationInteractions( dataContext, bulkCommunication, 0.5M );

            // Add an SMS Sender
            var definedValueService = new DefinedValueService( dataContext );

            var smsSenders = DefinedTypeCache.GetOrThrow( "SMS From Values", SystemGuid.DefinedType.COMMUNICATION_SMS_FROM.AsGuid() );

            var smsSender = new DefinedValue { DefinedTypeId = smsSenders.Id, Value = "SMS_SENDER_1", Guid = Constants.TestSmsSenderGuid.AsGuid(), Order = 1 };

            definedValueService.Add( smsSender );

            dataContext.SaveChanges();

            var smsSenderId = smsSender.Id;

            // Add bulk SMS
            var bulkSms = this.CreateSmsCommunication( dataContext,
                Constants.TestCommunicationBulkSmsGuid,
                new DateTime( 2019, 1, 29 ),
                "This is a test message....",
                new DateTime( 2019, 1, 29 ),
                TestPeople.BillMarblePersonGuid,
                TestPeople.AlishaMarblePersonGuid,
                smsSenderId,
                isBulk: true );

            recipientGuidList = personMap.Keys.ToList().GetRandomizedList( 175 );
            availableStatusList = GetAvailableCommunicationStatusList( 20, 10, 0 );

            this.CreateCommunicationRecipients( dataContext, bulkCommunication, recipientGuidList, availableStatusList );
            this.CreateCommunicationInteractions( dataContext, bulkCommunication, 0M );

        }

        /// <summary>
        /// Create and persist a new Email Communication instance.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="guid"></param>
        /// <param name="communicationDateTime"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="openedDateTime"></param>
        /// <param name="senderPersonAliasGuid"></param>
        /// <param name="reviewerPersonAliasGuid"></param>
        /// <param name="isBulk"></param>
        /// <returns></returns>
        private global::Rock.Model.Communication CreateEmailCommunication( RockContext dataContext, string guid, DateTime? communicationDateTime, string subject, string message, DateTime? openedDateTime, Guid senderPersonAliasGuid, Guid reviewerPersonAliasGuid, bool isBulk = false )
        {
            var personGuidToAliasIdMap = GetPersonGuidToAliasIdMap( dataContext );

            var communicationService = new CommunicationService( dataContext );

            var newEmail = new global::Rock.Model.Communication();

            newEmail.Guid = guid.AsGuid();
            newEmail.CommunicationType = CommunicationType.Email;
            newEmail.Subject = subject;
            newEmail.Status = CommunicationStatus.Approved;
            newEmail.ReviewedDateTime = communicationDateTime;
            newEmail.ReviewerNote = "Read and approved by the Communications Manager.";
            newEmail.IsBulkCommunication = isBulk;
            newEmail.SenderPersonAliasId = personGuidToAliasIdMap[senderPersonAliasGuid];
            newEmail.ReviewerPersonAliasId = personGuidToAliasIdMap[reviewerPersonAliasGuid];
            newEmail.SendDateTime = communicationDateTime;
            newEmail.Message = message;

            newEmail.CreatedDateTime = communicationDateTime;
            newEmail.CreatedByPersonAliasId = personGuidToAliasIdMap[senderPersonAliasGuid];

            newEmail.ForeignKey = _TestDataSourceOfChange;

            communicationService.Add( newEmail );

            dataContext.SaveChanges();

            return newEmail;
        }

            private List<int> _SmsSenderIdList = new List<int>();

        /// <summary>
        /// Create and persist a new SMS Communication instance.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="guid"></param>
        /// <param name="communicationDateTime"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="openedDateTime"></param>
        /// <param name="senderPersonAliasGuid"></param>
        /// <param name="reviewerPersonAliasGuid"></param>
        /// <param name="isBulk"></param>
        /// <returns></returns>
        private global::Rock.Model.Communication CreateSmsCommunication( RockContext dataContext, string guid, DateTime? communicationDateTime, string message, DateTime? openedDateTime, Guid senderPersonAliasGuid, Guid reviewerPersonAliasGuid, int smsSenderId, bool isBulk = false )
        {
            var personGuidToAliasIdMap = GetPersonGuidToAliasIdMap( dataContext );

            var communicationService = new CommunicationService( dataContext );

            var newSms = new global::Rock.Model.Communication();

            newSms.Guid = guid.AsGuid();
            newSms.CommunicationType = CommunicationType.SMS;
            newSms.Status = CommunicationStatus.Approved;
            newSms.ReviewedDateTime = communicationDateTime;
            newSms.ReviewerNote = "Read and approved by the Communications Manager.";
            newSms.IsBulkCommunication = isBulk;
            newSms.SenderPersonAliasId = personGuidToAliasIdMap[senderPersonAliasGuid];
            newSms.ReviewerPersonAliasId = personGuidToAliasIdMap[reviewerPersonAliasGuid];
            newSms.SendDateTime = communicationDateTime;
            newSms.SMSMessage = message;

            newSms.SMSFromDefinedValueId = smsSenderId;

            newSms.CreatedDateTime = communicationDateTime;
            newSms.CreatedByPersonAliasId = personGuidToAliasIdMap[senderPersonAliasGuid];

            newSms.ForeignKey = _TestDataSourceOfChange;

            communicationService.Add( newSms );

            dataContext.SaveChanges();

            return newSms;
        }

        /// <summary>
        /// Create and persist a set of Recipients for the specified Communication.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="communication"></param>
        /// <param name="recipientPersonGuidList"></param>
        /// <param name="possibleRecipientStatusList"></param>
        private void CreateCommunicationRecipients( RockContext dataContext, global::Rock.Model.Communication communication, List<Guid> recipientPersonGuidList, List<CommunicationRecipientStatus> possibleRecipientStatusList )
        {
            int communicationId = communication.Id;

            // Add recipients
            var recipientService = new CommunicationRecipientService( dataContext );
            var interactionService = new InteractionService( dataContext );

            var communicationDateTime = communication.SendDateTime ?? communication.CreatedDateTime ?? DateTime.Now;

            var personMap = this.GetPersonGuidToAliasIdMap( dataContext );

            foreach ( var recipientPersonGuid in recipientPersonGuidList )
            {
                var recipientPersonAliasId = personMap[recipientPersonGuid];

                var newRecipient = new CommunicationRecipient();

                newRecipient.Guid = Guid.NewGuid();
                newRecipient.CommunicationId = communicationId;
                newRecipient.PersonAliasId = recipientPersonAliasId;
                newRecipient.CreatedDateTime = communicationDateTime;
                newRecipient.ForeignKey = _TestDataSourceOfChange;

                var status = possibleRecipientStatusList.GetRandomElement();

                newRecipient.Status = status;

                if ( status == CommunicationRecipientStatus.Opened )
                {
                    // Get a randomized open time for the communication, within 7 days after it was sent.
                    var openedDateTime = GetRandomTimeWithinDayWindow( communicationDateTime, 7 );

                    newRecipient.OpenedDateTime = openedDateTime;

                    newRecipient.OpenedClient = "Gmail";
                }

                recipientService.Add( newRecipient );

                Debug.Print( $"Added Communication Recipient. (CommunicationId={ communicationId }, Status={status})" );

            }

            dataContext.SaveChanges();
        }

        /// <summary>
        /// Create and persist one or more Interactions for the specified Communication.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="communication"></param>
        /// <param name="probabilityOfMultipleInteractions">Specifies the probability that any given recipient has more than one interaction recorded for this Communication</param>
        private void CreateCommunicationInteractions( RockContext dataContext, global::Rock.Model.Communication communication, decimal probabilityOfMultipleInteractions )
        {
            var interactionChannelCommunication = new InteractionChannelService( dataContext ).Get( global::Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );

            var interactionService = new InteractionService( dataContext );
            var recipientService = new CommunicationRecipientService( dataContext );

            // Get or create a new interaction component for this communication.
            var communicationId = communication.Id;

            var componentService = new InteractionComponentService( dataContext );

            var interactionComponent = componentService.GetComponentByEntityId( global::Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid(),
                                    communicationId,
                                    communication.Subject );

            dataContext.SaveChanges();

            // Create an interaction for each communication recipient that has opened the communication.
            var recipients = recipientService.Queryable()
                .AsNoTracking()
                .Where( x => x.CommunicationId == communicationId
                             && x.Status == CommunicationRecipientStatus.Opened );

            foreach ( var recipient in recipients )
            {
                var clientIp = string.Format( "192.168.2.{0}", _Rng.Next( 0, 256 ) );
                var clientOs = _ValidClientOsList.GetRandomElement();
                var clientBrowser = _ValidClientBrowserList.GetRandomElement();
                var clientAgent = $"{clientBrowser} on {clientOs}";
                var clientType = _ValidClientTypeList.GetRandomElement();

                DateTime interactionDateTime;
                //= GetRandomTimeWithinDayWindow( communication.SendDateTime.Value, 7 );
                
                // Add an "Opened" interaction.
                var openedDateTime = recipient.OpenedDateTime ?? communication.SendDateTime.GetValueOrDefault();

                var newInteraction = interactionService.AddInteraction( interactionComponent.Id, recipient.Id, "Opened", "", recipient.PersonAliasId, openedDateTime, clientBrowser, clientOs, clientType, clientAgent, clientIp, null );

                newInteraction.ForeignKey = _TestDataSourceOfChange;

                // Add additional interactions.
                var hasMultipleInteractions = _Rng.Next( 1, 101 ) < probabilityOfMultipleInteractions * 100;

                int additionalOpens = 0;
                int totalClicks = 0;

                if ( hasMultipleInteractions )
                {
                    // Add more Opens...
                    additionalOpens = _Rng.Next( 1, 5 );

                    for ( int i = 0; i < additionalOpens; i++ )
                    {
                        interactionDateTime = GetRandomTimeWithinDayWindow( openedDateTime, 7 );

                        var openInteraction = interactionService.AddInteraction( interactionComponent.Id, recipient.Id, "Opened", "", recipient.PersonAliasId, openedDateTime, clientBrowser, clientOs, clientType, clientAgent, clientIp, null );

                        openInteraction.ForeignKey = _TestDataSourceOfChange;
                    }

                    // Add some Clicks...
                    totalClicks = _Rng.Next( 1, 5 );

                    string clickUrl = null;
                    bool changeUrl = true;
                    
                    for ( int i = 0; i < totalClicks; i++ )
                    {
                        interactionDateTime = GetRandomTimeWithinDayWindow( openedDateTime, 7 );

                        if ( changeUrl )
                        {
                            clickUrl = string.Format("https://communication/link/{0}", i );
                        }

                        var clickInteraction = interactionService.AddInteraction( interactionComponent.Id, recipient.Id, "Click", clickUrl, recipient.PersonAliasId, openedDateTime, clientBrowser, clientOs, clientType, clientAgent, clientIp, null );

                        clickInteraction.ForeignKey = _TestDataSourceOfChange;

                        // Allow a 50% chance that the URL will remain the same for the next click also.
                        changeUrl = _Rng.Next( 1, 101 ) < 50;
                    }

                }

                Debug.Print( $"Added Communication Interaction. (CommunicationId={ communicationId }, Recipient={recipient.PersonAliasId}, Opens={additionalOpens + 1 }, Clicks={ totalClicks })" );
            }

            dataContext.SaveChanges();
        }

        #endregion

        /// <summary>
        /// Returns a random DateTime within a specified time window of a base date.
        /// </summary>
        /// <param name="baseDateTime"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        private DateTime GetRandomTimeWithinDayWindow( DateTime? baseDateTime, int days )
        {
            if ( baseDateTime == null )
            {
                baseDateTime = RockDateTime.Now;
            }

            var minutesToAdd = _Rng.Next( 1, ( Math.Abs( days ) * 1440 ) + 1 );

            if ( days < 0 )
            {
                minutesToAdd *= -1;
            }

            var newDateTime = baseDateTime.Value.AddMinutes( minutesToAdd );

            return newDateTime;
        }

        #region Support Methods

        /// <summary>
        /// Get a weighted list of 100 possible status entries, to allow a random selection with a known percentage chance of success.
        /// </summary>
        /// <param name="percentChanceOfDeliveredNotOpened"></param>
        /// <param name="percentChanceOfFailed"></param>
        /// <param name="percentChanceOfCancelled"></param>
        /// <returns></returns>
        private List<CommunicationRecipientStatus> GetAvailableCommunicationStatusList( int percentChanceOfDeliveredNotOpened, int percentChanceOfFailed, int percentChanceOfCancelled )
        {
            var percentChanceOfOpened = 100 - ( percentChanceOfDeliveredNotOpened + percentChanceOfFailed + percentChanceOfCancelled );

            var statusList = new List<CommunicationRecipientStatus>();

            for ( int i = 0; i < percentChanceOfOpened; i++ )
            {
                statusList.Add( CommunicationRecipientStatus.Opened );
            }

            for ( int i = 0; i < percentChanceOfDeliveredNotOpened; i++ )
            {
                statusList.Add( CommunicationRecipientStatus.Delivered );
            }

            for ( int i = 0; i < percentChanceOfFailed; i++ )
            {
                statusList.Add( CommunicationRecipientStatus.Failed );
            }

            for ( int i = 0; i < percentChanceOfCancelled; i++ )
            {
                statusList.Add( CommunicationRecipientStatus.Cancelled );
            }

            return statusList;
        }

        /// <summary>
        /// Get a lookup table to convert a Person.Guid to a PersonAlias.Id
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        private Dictionary<Guid, int> GetPersonGuidToAliasIdMap( RockContext dataContext )
        {
            if ( _PersonGuidToAliasIdMap == null )
            {
                var aliasService = new PersonAliasService( dataContext );

                var personList = aliasService.Queryable()
                    .AsNoTracking()
                    .Where( x => !x.Person.IsSystem )
                    .Take( _MaxRecipientCount )
                    .GroupBy( x => x.Person.Guid )
                     .ToDictionary( k => k.Key, v => v.First().Id );

                _PersonGuidToAliasIdMap = personList;
            }

            return _PersonGuidToAliasIdMap;
        }

        /// <summary>
        /// Get a known Person who has been assigned a security role of Administrator.
        /// </summary>
        /// <param name="personService"></param>
        /// <returns></returns>
        private Person GetAdminPersonOrThrow( PersonService personService )
        {
            var adminPerson = personService.Queryable().FirstOrDefault( x => x.FirstName == "Alisha" && x.LastName == "Marble" );

            if ( adminPerson == null )
            {
                throw new Exception( "Admin Person not found in test data set." );
            }

            return adminPerson;
        }

        #endregion
    }
}
