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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Model;

namespace Rock.Tests.Integration.Modules.Communications.Transport
{
    /// <summary>
    /// A mock transport component useful for testing communications involving the SMTP protocol.
    /// </summary>
    public class MockSmtpTransport : SMTPComponent
    {
        private ConcurrentQueue<MockSmtpSendResult> _sentEmails = new ConcurrentQueue<MockSmtpSendResult>();

        protected override EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage )
        {
            // Create the message and set the default body text.
            var mailMessage = GetMailMessageFromRockEmailMessage( rockEmailMessage );
            mailMessage.Body = rockEmailMessage.Message;

            // Add the message to the queue of processed items.
            var processedItem = new MockSmtpSendResult
            {
                ProcessedDateTime = RockDateTime.Now,
                RockMessage = rockEmailMessage,
                EmailMessage = mailMessage
            };

            _sentEmails.Enqueue( processedItem );

            return new EmailSendResponse
            {
                Status = CommunicationRecipientStatus.Delivered,
                StatusNote = StatusNote
            };
        }

        public IReadOnlyList<MockSmtpSendResult> ProcessedItems
        {
            get
            {
                return _sentEmails.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// The result of a send message action performed by the MockSmtpTransport.
    /// </summary>
    public class MockSmtpSendResult
    {
        public DateTime ProcessedDateTime { get; set; }
        public RockEmailMessage RockMessage { get; set; }
        public MailMessage EmailMessage { get; set; }
    }
}
