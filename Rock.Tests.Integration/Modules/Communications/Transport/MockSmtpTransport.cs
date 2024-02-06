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
        protected override EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage )
        {
            return new EmailSendResponse
            {
                Status = CommunicationRecipientStatus.Delivered,
                StatusNote = StatusNote
            };
        }
    }
}
