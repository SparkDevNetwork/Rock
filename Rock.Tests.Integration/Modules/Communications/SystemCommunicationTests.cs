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
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.Communications;
using Rock.Tests.Integration.Modules.Communications.Transport;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Communications
{
    /// <summary>
    /// Tests for System Communications.
    /// </summary>
    [TestCategory( TestFeatures.Communications )]
    [TestClass]
    public class SystemCommunicationTests : DatabaseTestsBase
    {
        [TestMethod]
        public void SendRockCommunication_WithRelativeUrl_ResolvesRelativeUrlToExternalSite()
        {
            var result = SendSystemCommunication( $"Click <a href='/person/1'>here</a> to view your person profile." );

            Assert.That.AreEqual( $"Click <a href='http://www.organization.com/person/1'>here</a> to view your person profile.",
                result.EmailMessage.Body );
        }

        /// <summary>
        // This test documents the current behavior when sending a RockMessage.
        // However, the result does not align with sending a Communication, where the relative Url is resolved to an absolute Url.
        /// </summary>
        [TestMethod]
        public void SendRockMessage_WithUnspecifiedAppRoot_DoesNotResolveRelativeUrl()
        {
            // This result documents the current behavior when sending a RockMessage.
            // However, this does not align with sending a Communication, where the relative Url is resolved to an absolute Url.
            var rockMessage = GetTestRockEmailMessage();
            rockMessage.AppRoot = null;

            var mailMessage = SendTestRockEmailMessage( rockMessage );

            Assert.That.AreEqual( "Click <a href='/person/1'>here</a> to view your person profile.",
                mailMessage.Body );
        }

        [TestMethod]
        public void SendRockMessage_WithSpecifiedAppRoot_ResolvesRelativeUrlToAbsoluteUrl()
        {
            var rockMessage = GetTestRockEmailMessage();
            rockMessage.AppRoot = "http://www.my.church/";

            var mailMessage = SendTestRockEmailMessage( rockMessage );

            Assert.That.AreEqual( "Click <a href='http://www.my.church/person/1'>here</a> to view your person profile.",
                mailMessage.Body );
        }

        private RockEmailMessage GetTestRockEmailMessage()
        {
            var actualEmail = new RockEmailMessage()
            {
                FromEmail = "test@org.com",
                Message = "Click <a href='/person/1'>here</a> to view your person profile.",
            };

            actualEmail.AddRecipient( new RockEmailMessageRecipient( new Person
            {
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "User"
            }, new Dictionary<string, object>() ) );

            return actualEmail;
        }

        private MailMessage SendTestRockEmailMessage( RockEmailMessage actualEmail )
        {
            var medium = CommunicationsDataManager.Instance.GetCommunicationMediumComponent( CommunicationType.Email );
            var mediumAttributes = new Dictionary<string, string>();

            var emailTransport = new MockSmtpTransport();
            emailTransport.Send( actualEmail, medium.EntityType.Id, mediumAttributes, out var errorMessages );
            Assert.IsFalse( errorMessages.Any() );

            var processedMessage = emailTransport.ProcessedItems.LastOrDefault();

            return processedMessage.EmailMessage;
        }

        private MockSmtpSendResult SendSystemCommunication( string messageBody )
        {
            var person = TestDataHelper.GetTestPerson( TestGuids.TestPeople.TedDecker );

            var medium = CommunicationsDataManager.Instance.GetCommunicationMediumComponent( CommunicationType.Email );
            var mediumAttributes = new Dictionary<string, string>();

            var smtpTransport = new MockSmtpTransport();

            var createEmailArgs = new CommunicationService.CreateEmailCommunicationArgs
            {
                Recipients = new List<RockEmailMessageRecipient> { new RockEmailMessageRecipient( person, new Dictionary<string, object>() ) },
                Message = messageBody,
                RecipientStatus = CommunicationRecipientStatus.Pending
            };

            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );
            var communication = communicationService.CreateEmailCommunication( createEmailArgs );

            foreach ( var recipient in communication.Recipients )
            {
                recipient.MediumEntityTypeId = medium.EntityType.Id;
            }

            rockContext.SaveChanges();

            smtpTransport.Send( communication, medium.EntityType.Id, mediumAttributes );

            var processedMessage = smtpTransport.ProcessedItems.LastOrDefault();

            return processedMessage;
        }
    }
}
