using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Communications
{
    public class MailgunTests
    {
        [TestMethod] [Ignore( "need way of mocking RockContext" )]
        public void HttpSendRockMessage()
        {
            var errorMessages = new List<string>();
            var binaryFileService = new BinaryFileService( new RockContext() );

            var rockEmailMessage = new RockEmailMessage
            {
                FromName = "Spark Info",
                FromEmail = "info@sparkdevnetwork.org",
                ReplyToEmail = "info@sparkdevnetwork.org",
                CCEmails = new List<string>() { "ethantd1@hotmail.com" },
                //BCCEmails = new List<string>() { "ethantd1@gmail.com" },
                Subject = "Mailgun HTTP Tests",
                Message = "This is a test of the mailgun HTTP API",
                //MessageMetaData = new Dictionary<string, string>() { { "", "" }, { "", "" } },
                Attachments = new List<BinaryFile>() { binaryFileService.Get( 10 ) }
            };

            rockEmailMessage.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( "ethan@sparkdevnetwork.org", null ) );

            var mailgunHttp = new MailgunHttp();
            mailgunHttp.Send( rockEmailMessage, 0, null, out errorMessages );

            Assert.That.True( !errorMessages.Any() );
            Assert.That.Equal( System.Net.HttpStatusCode.OK, mailgunHttp.Response.StatusCode );
        }
    }
}
