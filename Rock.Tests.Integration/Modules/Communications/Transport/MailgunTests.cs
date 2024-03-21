using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Communications
{
    [Ignore( "This test requires Mailgun be setup and a valid e-mail address." )]
    public class MailgunTests : DatabaseTestsBase
    {
        [TestMethod]
        public void HttpSendRockMessage()
        {
            var binaryFileService = new BinaryFileService( new RockContext() );

            var rockEmailMessage = new RockEmailMessage
            {
                FromName = "Spark Info",
                FromEmail = "info@sparkdevnetwork.org",
                ReplyToEmail = "info@sparkdevnetwork.org",
                Subject = "Mailgun HTTP Tests",
                Message = "This is a test of the mailgun HTTP API",
                Attachments = new List<BinaryFile>() { binaryFileService.Get( 10 ) }
            };

            rockEmailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( "info@sparkdevnetwork.org", null ) );

            var mailgunHttp = new MailgunHttp();
            mailgunHttp.Send( rockEmailMessage, 0, null, out var errorMessages );

            Assert.That.True( !errorMessages.Any() );
            Assert.That.Equal( System.Net.HttpStatusCode.OK, mailgunHttp.Response.StatusCode );
        }
    }
}
